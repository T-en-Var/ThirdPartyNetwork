# File Service

**⚠️ SUPERSEDED.** `PLAN.md` §0.1 decided against building this separate `file-service` for
TPN-owned content — MinIO was folded directly into TPN instead (`MinioService : ICDNService`,
same seam `CloudinaryService` used to occupy), keeping one authorization system rather than
running this doc's `ServiceIdentity`/`file_grant` model alongside TPN's own
`ContentAccessGrant`/RBAC system. That plan has since been implemented in full. This
document's good ideas (two MinIO clients for the presign/Host-header gotcha, public/private
buckets, "the API never proxies bytes", soft-delete + sweep) were carried over into TPN's
own file subsystem rather than this separate repo. Kept here for reference / in case this
service is still in use for non-TPN callers (PLAN.md §9) — not because it's still the plan
for TPN's `File` content.

---

Central file storage for all services on the home server. One gatekeeper API owns all file
access; blobs live in MinIO, metadata lives in the shared PostgreSQL instance. Other
services (e.g. **AtHomeServer**) never touch MinIO directly — they talk to the File API
and receive metadata + URLs.

```
                                  ┌──────────────────────────────────────────┐
   Internet                       │              Docker VM                   │
      │                           │                                          │
      ▼                           │  ┌───────────┐      ┌─────────────────┐  │
 cloudflared ──► nginx gateway ───┼─►│ file-api  │─────►│      minio      │  │
                    │             │  └─────┬─────┘      └────────▲────────┘  │
                    │             │        │                     │           │
                    │             │  ┌─────┴─────────┐           │           │
                    │             │  │ AtHomeServer  │           │           │
                    │             │  │ other services│           │           │
                    │             │  └───────────────┘           │           │
                    │             └──────────────────────────────┼───────────┘
                    │                                            │
                    └── cdn.<domain> ── (presigned URLs only) ───┘
                                     ┌────────────┐
                                     │ PostgreSQL │  (separate Proxmox instance)
                                     └────────────┘
```

Two public subdomains route through the existing nginx gateway:

| Subdomain            | Target      | Purpose                                        |
| -------------------- | ----------- | ---------------------------------------------- |
| `files.<domain>`     | `file-api`  | Metadata API (JSON), uploads                   |
| `cdn.<domain>`       | `minio`     | Raw bytes. Public buckets + presigned URLs only |

---

## 1. Repository layout

```
file-service/
├── docker-compose.yml
├── .env                        # never committed; see .env.example
├── .env.example
├── README.md                   # this file
└── api/
    ├── Dockerfile
    ├── Dockerfile.dev
    ├── entrypoint.sh           # run alembic migrations, then uvicorn
    ├── pyproject.toml          # uv-managed
    ├── alembic.ini
    ├── main.py                 # uvicorn entrypoint
    ├── server.py               # create_app(), middleware, exception handlers
    │
    ├── presentation/           # LAYER: HTTP in/out, nothing else
    │   └── endpoints/
    │       ├── __init__.py     # root APIRouter, prefix="/api"
    │       ├── files.py
    │       ├── grants.py
    │       └── health.py
    │
    ├── application/            # LAYER: use cases, DTOs, orchestration
    │   └── files/
    │       ├── dto.py
    │       ├── usecases/
    │       │   ├── upload_file.py
    │       │   ├── get_file.py
    │       │   ├── list_files.py
    │       │   ├── soft_delete_file.py
    │       │   ├── grant_access.py
    │       │   └── revoke_access.py
    │       └── services/
    │           └── url_builder.py
    │
    ├── domain/                 # LAYER: pure business concepts, zero dependencies
    │   ├── file_visibility.py
    │   ├── permissions.py
    │   └── constants/
    │       └── buckets.py
    │
    ├── infrastructure/         # LAYER: everything that talks to the outside world
    │   ├── database/
    │   │   ├── session.py
    │   │   ├── models/
    │   │   │   ├── _base.py
    │   │   │   ├── file.py
    │   │   │   └── file_grant.py
    │   │   ├── repositories/
    │   │   │   ├── _base.py
    │   │   │   ├── file.py
    │   │   │   └── file_grant.py
    │   │   └── migrations/     # alembic
    │   └── storage/
    │       └── minio/
    │           ├── service.py  # StorageService: put/stat/remove/presign
    │           └── dto.py
    │
    └── core/                   # LAYER: cross-cutting plumbing
        ├── configuration.py    # pydantic-settings, fail-fast on bad .env
        ├── common/
        │   ├── result.py       # Result[T, E], succeed(), fail()
        │   └── schema.py       # APIBaseModel, BaseEntity, Page, BaseSearchQuery
        ├── exceptions/
        │   ├── base.py         # CustomException
        │   └── standard.py     # NotFoundException, ForbiddenException, ...
        ├── decorators/
        │   ├── response.py     # @resolve_response(status)
        │   └── service_auth.py # @require_service (replaces user auth decorators)
        └── dependencies/
            └── service_identity.py
```

Same skeleton as the AtHomeServer backend, with two deliberate differences:

1. **`infrastructure/storage/minio/` replaces `infrastructure/trojonetworks/`** — MinIO is
   this project's external system, wrapped in a service the same way the auth network is
   wrapped there.
2. **Callers are services, not users.** `core/decorators/auth_permissions/` becomes
   `core/decorators/service_auth.py` — authentication answers *"which service is calling,
   and optionally on behalf of which end user?"*

---

## 2. Setup

### 2.1 Prerequisites

- The shared external Docker network (create once on the Docker VM):

  ```bash
  docker network create backend-net
  ```

- A database and user on the existing PostgreSQL instance:

  ```sql
  CREATE DATABASE fileservice;
  CREATE USER fileservice WITH PASSWORD '<strong password>';
  GRANT ALL PRIVILEGES ON DATABASE fileservice TO fileservice;
  ```

### 2.2 Environment

`.env.example`:

```env
APP_ENV=development

# Postgres (metadata)
DB_CONNECTION_STRING=postgresql+asyncpg://fileservice:<pw>@<postgres-host>:5432/fileservice

# MinIO
MINIO_ROOT_USER=fileadmin
MINIO_ROOT_PASSWORD=<strong password>
MINIO_INTERNAL_ENDPOINT=minio:9000
MINIO_EXTERNAL_ENDPOINT=cdn.example.com   # must match nginx/cloudflared route
MINIO_EXTERNAL_SECURE=true

# URL signing
PRESIGN_EXPIRY_SECONDS=900                # 15 min default
PRESIGN_EXPIRY_STREAMING_SECONDS=3600     # 1 h for audio/video sessions

# Service tokens: which services may call this API.
# Format: token:service_name, comma separated. Rotate per service.
SERVICE_TOKENS=abc123...:athomeserver,def456...:otherservice
```

### 2.3 docker-compose.yml

```yaml
services:
  file-api:
    build: ./api
    restart: unless-stopped
    env_file: .env
    depends_on:
      - minio
    networks:
      - backend-net

  minio:
    image: minio/minio:latest
    restart: unless-stopped
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    volumes:
      - minio-data:/data
    networks:
      - backend-net
    # No host ports. nginx reaches it over backend-net for cdn.<domain>;
    # temporarily map 9001 if you need the admin console.

volumes:
  minio-data:

networks:
  backend-net:
    external: true
```

### 2.4 nginx gateway (in the gateway repo)

```nginx
# Metadata API
server {
    server_name files.example.com;
    location / {
        proxy_pass http://file-api:8080;
        proxy_set_header Host $host;
        client_max_body_size 512M;        # uploads
    }
}

# Raw bytes: public buckets + presigned URLs. MinIO enforces access itself.
server {
    server_name cdn.example.com;
    location / {
        proxy_pass http://minio:9000;
        proxy_set_header Host $host;      # REQUIRED: presign signature covers Host
        proxy_buffering off;              # stream video/audio, don't spool to disk
    }
}
```

> **The classic presign bug:** a presigned URL is only valid for the exact hostname it was
> signed for. `file-api` therefore uses **two MinIO clients** — one against
> `minio:9000` for internal URLs and S3 operations, one against `cdn.<domain>` purely for
> signing external URLs. If nginx rewrites the `Host` header, MinIO returns
> `SignatureDoesNotMatch`.

### 2.5 First run

```bash
docker compose up -d --build
# entrypoint.sh runs `alembic upgrade head` before starting uvicorn

# One-time bucket setup (or automate in a startup hook):
docker compose exec minio mc alias set local http://localhost:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD
docker compose exec minio mc mb local/public local/private
docker compose exec minio mc anonymous set download local/public   # anonymous GET on public only
```

Swagger lives at `https://files.example.com/api/docs`.

---

## 3. Architecture, layer by layer

Dependency rule (identical to AtHomeServer): **arrows point inward.**

```
presentation ──► application ──► domain
      │               │
      └──────┬────────┘
             ▼
      infrastructure          core (usable from anywhere)
```

- `presentation` may import from `application` and `core`. Never from `infrastructure`.
- `application` orchestrates: it imports `domain`, `core`, and *instantiates* repositories
  and storage services from `infrastructure` (composition via FastAPI `Depends`).
- `domain` imports nothing but the standard library.
- `infrastructure` implements the boring outer world: SQLAlchemy, MinIO SDK.
- `core` is shared plumbing with no business knowledge.

One example file per layer follows — these are the reference implementations to copy when
adding features.

### 3.1 `domain/` — pure business concepts

No FastAPI, no SQLAlchemy, no MinIO. Just the vocabulary of the problem.

```python
# domain/file_visibility.py
from enum import StrEnum


class FileVisibility(StrEnum):
    PUBLIC = "public"      # anonymous GET via cdn, stable URL, cacheable forever
    UNLISTED = "unlisted"  # no auth check; the unguessable presigned URL is the secret
    PRIVATE = "private"    # owner service, or an explicit row in file_grant


class GrantType(StrEnum):
    SERVICE = "service"          # grant_value = "athomeserver"
    USER = "user"                # grant_value = "<user uuid>"
    SERVICE_ROLE = "service_role"  # grant_value = "athomeserver:admin"
```

```python
# domain/constants/buckets.py
PUBLIC_BUCKET = "public"
PRIVATE_BUCKET = "private"
```

> **Why an enum + grants table, not one giant enum:** visibility answers *"what class of
> protection?"*; the `file_grant` table answers *"who exactly?"*. "Shared with role admin
> in AtHomeServer" is a row, not a new enum member — the enum never grows again.

### 3.2 `core/` — plumbing

Copied nearly verbatim from AtHomeServer: `Result[T, E]` with `succeed()`/`fail()`,
`CustomException` hierarchy, `@resolve_response`, `APIBaseModel`/`BaseEntity`/`Page`.
The one genuinely new piece is service authentication:

```python
# core/dependencies/service_identity.py
from dataclasses import dataclass
import uuid

from fastapi import Header, HTTPException

from core.configuration import config


@dataclass(frozen=True)
class ServiceIdentity:
    """Who is calling: always a service, optionally acting for an end user."""
    service_name: str
    on_behalf_of_user: uuid.UUID | None


class UseServiceAuth:
    async def __call__(
        self,
        x_service_token: str = Header(...),
        x_on_behalf_of: uuid.UUID | None = Header(default=None),
    ) -> ServiceIdentity:
        service_name = config.service_tokens.get(x_service_token)
        if service_name is None:
            raise HTTPException(status_code=401, detail="Unknown service token")
        return ServiceIdentity(service_name=service_name, on_behalf_of_user=x_on_behalf_of)
```

This plays the role AtHomeServer's `IdentityService` plays: use cases receive a
`ServiceIdentity` and make authorization decisions with it. The trust model is
deliberately simple — the file API trusts a *service* to truthfully assert which end user
it acts for (`X-On-Behalf-Of`), because services authenticate their own users themselves.
The file API's job is inter-service authorization, not re-authenticating end users.

```python
# core/configuration.py (excerpt)
from typing import ClassVar
from pydantic_settings import BaseSettings, SettingsConfigDict


class EnvConfig(BaseSettings):
    APP_ENV: str
    DB_CONNECTION_STRING: str

    MINIO_ROOT_USER: str
    MINIO_ROOT_PASSWORD: str
    MINIO_INTERNAL_ENDPOINT: str
    MINIO_EXTERNAL_ENDPOINT: str
    MINIO_EXTERNAL_SECURE: bool = True

    PRESIGN_EXPIRY_SECONDS: int = 900
    PRESIGN_EXPIRY_STREAMING_SECONDS: int = 3600

    SERVICE_TOKENS: str  # "token:name,token:name"

    model_config: ClassVar[SettingsConfigDict] = SettingsConfigDict(env_file=".env", extra="ignore")
```

Fail-fast validation on startup, same as AtHomeServer's `get_env_config`.

### 3.3 `infrastructure/database/` — models & repositories

```python
# infrastructure/database/models/file.py
import uuid
from datetime import datetime
from typing import TYPE_CHECKING

from sqlalchemy import BigInteger, DateTime, Index
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import Mapped, mapped_column, relationship

from infrastructure.database.models._base import Base

if TYPE_CHECKING:
    from infrastructure.database.models.file_grant import FileGrant


class File(Base):
    __tablename__: str = "file"

    bucket: Mapped[str] = mapped_column()
    object_key: Mapped[str] = mapped_column(unique=True)
    original_filename: Mapped[str] = mapped_column()
    mime_type: Mapped[str] = mapped_column()
    size_bytes: Mapped[int] = mapped_column(BigInteger)

    visibility: Mapped[str] = mapped_column(index=True)   # FileVisibility value
    owner_service: Mapped[str] = mapped_column(index=True)
    owner_user_id: Mapped[uuid.UUID | None] = mapped_column(nullable=True, index=True)

    deleted_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True), nullable=True)
    extra: Mapped[dict | None] = mapped_column(JSONB, nullable=True)  # per-type metadata

    grants: Mapped[list["FileGrant"]] = relationship(
        back_populates="file", lazy="selectin", cascade="all, delete-orphan"
    )


Index("ix_file_owner_active", File.owner_service, File.deleted_at)
```

The repository extends the generic `BaseRepository` (same `_base.py` as AtHomeServer, with
`get`, `get_page`, `get_by_id`, `create`, `update`, `delete` + query `modifier` callables)
and adds only file-specific queries:

```python
# infrastructure/database/repositories/file.py
import uuid

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from infrastructure.database.models.file import File
from infrastructure.database.repositories._base import BaseRepository


class FileRepository(BaseRepository[File]):
    def __init__(self, session: AsyncSession):
        super().__init__(File, session)

    async def get_active_by_id(self, file_id: uuid.UUID) -> File | None:
        query = select(File).where(File.id == file_id, File.deleted_at.is_(None))
        result = await self.session.execute(query)
        return result.scalar_one_or_none()
```

### 3.4 `infrastructure/storage/minio/` — the storage service

The MinIO SDK is wrapped exactly like AtHomeServer wraps its external auth network: one
service class, DTOs at the boundary, no SDK types leaking into `application`.

```python
# infrastructure/storage/minio/service.py
from datetime import timedelta
from typing import BinaryIO

from minio import Minio

from core.configuration import config


class StorageService:
    """Sole owner of MinIO access. Application code never imports `minio` directly."""

    def __init__(self) -> None:
        # Internal client: actual S3 operations + internal-network presigns.
        self._internal = Minio(
            config.env.MINIO_INTERNAL_ENDPOINT,
            access_key=config.env.MINIO_ROOT_USER,
            secret_key=config.env.MINIO_ROOT_PASSWORD,
            secure=False,  # TLS terminates at cloudflared/nginx
        )
        # External client: ONLY for signing URLs the browser will use.
        # Signature covers the hostname, hence two clients.
        self._external = Minio(
            config.env.MINIO_EXTERNAL_ENDPOINT,
            access_key=config.env.MINIO_ROOT_USER,
            secret_key=config.env.MINIO_ROOT_PASSWORD,
            secure=config.env.MINIO_EXTERNAL_SECURE,
        )

    def put(self, bucket: str, key: str, stream: BinaryIO, content_type: str) -> int:
        result = self._internal.put_object(
            bucket, key, stream,
            length=-1, part_size=10 * 1024 * 1024,  # stream unknown-size uploads
            content_type=content_type,
        )
        return self._internal.stat_object(bucket, result.object_name).size

    def remove(self, bucket: str, key: str) -> None:
        self._internal.remove_object(bucket, key)

    def presign_internal(self, bucket: str, key: str, expiry: timedelta) -> str:
        return self._internal.presigned_get_object(bucket, key, expires=expiry)

    def presign_external(self, bucket: str, key: str, expiry: timedelta) -> str:
        return self._external.presigned_get_object(bucket, key, expires=expiry)

    def public_urls(self, bucket: str, key: str) -> tuple[str, str]:
        scheme = "https" if config.env.MINIO_EXTERNAL_SECURE else "http"
        return (
            f"http://{config.env.MINIO_INTERNAL_ENDPOINT}/{bucket}/{key}",
            f"{scheme}://{config.env.MINIO_EXTERNAL_ENDPOINT}/{bucket}/{key}",
        )
```

> The `minio` SDK is synchronous. Presigning is pure local HMAC computation (no network
> call) so it's free. `put`/`remove` do real I/O — acceptable at homelab load; if it ever
> matters, wrap them in `anyio.to_thread.run_sync` inside this class and nothing above
> this layer changes. That isolation is the point of the wrapper.

### 3.5 `application/files/` — DTOs and use cases

DTOs follow the `APIBaseModel`/`BaseEntity` conventions (camelCase aliases,
`from_attributes`):

```python
# application/files/dto.py
import uuid
from datetime import datetime

from core.common.schema import APIBaseModel, BaseEntity, BaseSearchQuery
from domain.file_visibility import FileVisibility, GrantType


class FileUrlsDto(APIBaseModel):
    internal_url: str
    external_url: str
    expires_at: datetime | None      # None => stable public URL


class FileDto(BaseEntity):
    original_filename: str
    mime_type: str
    size_bytes: int
    visibility: FileVisibility
    owner_service: str
    owner_user_id: uuid.UUID | None
    urls: FileUrlsDto | None = None  # populated by GetFileUseCase


class FetchFileFilters(BaseSearchQuery):
    mime_type_prefix: str | None = None   # e.g. "image/"
    owner_user_id: uuid.UUID | None = None


class CreateGrantDto(APIBaseModel):
    grant_type: GrantType
    grant_value: str
    permission: str = "read"
    expires_at: datetime | None = None
```

One class per use case, dependencies injected in `__init__`, business logic in
`__call__`, always returning `Result[...]` — never raising for expected failures:

```python
# application/files/usecases/get_file.py
import uuid
from datetime import datetime, timedelta, timezone
from typing import Annotated

from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from application.files.dto import FileDto, FileUrlsDto
from core.common.result import Result, fail, succeed
from core.configuration import config
from core.dependencies.service_identity import ServiceIdentity, UseServiceAuth
from core.exceptions.standard import NotFoundException
from domain.file_visibility import FileVisibility, GrantType
from infrastructure.database.models.file import File
from infrastructure.database.repositories.file import FileRepository
from infrastructure.database.repositories.file_grant import FileGrantRepository
from infrastructure.database.session import get_db_session
from infrastructure.storage.minio.service import StorageService


class GetFileUseCase:
    def __init__(
        self,
        identity: Annotated[ServiceIdentity, Depends(UseServiceAuth())],
        session: Annotated[AsyncSession, Depends(get_db_session)],
        storage: Annotated[StorageService, Depends()],
    ) -> None:
        self.identity = identity
        self.files = FileRepository(session=session)
        self.grants = FileGrantRepository(session=session)
        self.storage = storage

    async def __call__(self, file_id: uuid.UUID) -> Result[FileDto]:
        file = await self.files.get_active_by_id(file_id)
        if file is None:
            return fail(NotFoundException("File not found."))

        if not await self._may_read(file):
            # 404, not 403: don't confirm the file exists to callers without access.
            return fail(NotFoundException("File not found."))

        dto = FileDto.model_validate(file)
        dto.urls = self._build_urls(file)
        return succeed(dto)

    async def _may_read(self, file: File) -> bool:
        if file.visibility in (FileVisibility.PUBLIC, FileVisibility.UNLISTED):
            return True
        if file.owner_service == self.identity.service_name:
            return True
        return await self.grants.has_read_grant(
            file_id=file.id,
            service_name=self.identity.service_name,
            user_id=self.identity.on_behalf_of_user,
        )

    def _build_urls(self, file: File) -> FileUrlsDto:
        if file.visibility == FileVisibility.PUBLIC:
            internal, external = self.storage.public_urls(file.bucket, file.object_key)
            return FileUrlsDto(internal_url=internal, external_url=external, expires_at=None)

        seconds = (
            config.env.PRESIGN_EXPIRY_STREAMING_SECONDS
            if file.mime_type.startswith(("video/", "audio/"))
            else config.env.PRESIGN_EXPIRY_SECONDS
        )
        expiry = timedelta(seconds=seconds)
        return FileUrlsDto(
            internal_url=self.storage.presign_internal(file.bucket, file.object_key, expiry),
            external_url=self.storage.presign_external(file.bucket, file.object_key, expiry),
            expires_at=datetime.now(timezone.utc) + expiry,
        )
```

And the write side, for symmetry:

```python
# application/files/usecases/upload_file.py
import uuid
from typing import Annotated

from fastapi import Depends, UploadFile
from sqlalchemy.ext.asyncio import AsyncSession

from application.files.dto import FileDto
from core.common.result import Result, fail, succeed
from core.dependencies.service_identity import ServiceIdentity, UseServiceAuth
from core.exceptions.standard import ValidationException
from domain.constants.buckets import PRIVATE_BUCKET, PUBLIC_BUCKET
from domain.file_visibility import FileVisibility
from infrastructure.database.repositories.file import FileRepository
from infrastructure.database.session import get_db_session
from infrastructure.storage.minio.service import StorageService


class UploadFileUseCase:
    def __init__(
        self,
        identity: Annotated[ServiceIdentity, Depends(UseServiceAuth())],
        session: Annotated[AsyncSession, Depends(get_db_session)],
        storage: Annotated[StorageService, Depends()],
    ) -> None:
        self.identity = identity
        self.files = FileRepository(session=session)
        self.storage = storage

    async def __call__(
        self,
        upload: UploadFile,
        visibility: FileVisibility,
        owner_user_id: uuid.UUID | None,
    ) -> Result[FileDto]:
        if not upload.filename or not upload.content_type:
            return fail(ValidationException("Filename and content type are required."))

        file_id = uuid.uuid4()
        bucket = PUBLIC_BUCKET if visibility == FileVisibility.PUBLIC else PRIVATE_BUCKET
        # Key layout: service/uuid/filename — collision-proof, self-documenting in MinIO.
        key = f"{self.identity.service_name}/{file_id}/{upload.filename}"

        size = self.storage.put(bucket, key, upload.file, upload.content_type)

        file = await self.files.create(
            id=file_id,
            bucket=bucket,
            object_key=key,
            original_filename=upload.filename,
            mime_type=upload.content_type,
            size_bytes=size,
            visibility=visibility,
            owner_service=self.identity.service_name,
            owner_user_id=owner_user_id,
        )
        return succeed(FileDto.model_validate(file))
```

### 3.6 `presentation/endpoints/` — thin routers

Endpoints declare routes, inject use cases, and let the decorators translate `Result` into
HTTP. No logic here, ever.

```python
# presentation/endpoints/files.py
import uuid
from typing import Annotated

from fastapi import APIRouter, Depends, UploadFile

from application.files.dto import FetchFileFilters, FileDto
from application.files.usecases.get_file import GetFileUseCase
from application.files.usecases.list_files import ListFilesUseCase
from application.files.usecases.soft_delete_file import SoftDeleteFileUseCase
from application.files.usecases.upload_file import UploadFileUseCase
from core.common.schema import Page
from core.decorators.response import resolve_response
from domain.file_visibility import FileVisibility

router = APIRouter(prefix="/files")


@router.get("", response_model=Page[FileDto])
@resolve_response(200)
async def get_list(
    filters: Annotated[FetchFileFilters, Depends()],
    use_case: Annotated[ListFilesUseCase, Depends()],
):
    return await use_case(filters)


@router.get("/{file_id}", response_model=FileDto)
@resolve_response(200)
async def get_single(file_id: uuid.UUID, use_case: Annotated[GetFileUseCase, Depends()]):
    return await use_case(file_id)


@router.post("", response_model=FileDto, status_code=201)
@resolve_response(201)
async def upload(
    file: UploadFile,
    use_case: Annotated[UploadFileUseCase, Depends()],
    visibility: FileVisibility = FileVisibility.PRIVATE,
    owner_user_id: uuid.UUID | None = None,
):
    return await use_case(file, visibility, owner_user_id)


@router.delete("/{file_id}", status_code=204)
@resolve_response(204)
async def soft_delete(file_id: uuid.UUID, use_case: Annotated[SoftDeleteFileUseCase, Depends()]):
    return await use_case(file_id)
```

```python
# presentation/endpoints/__init__.py
from fastapi import APIRouter

from .files import router as files_router
from .grants import router as grants_router
from .health import router as health_router

router = APIRouter(prefix="/api")

router.include_router(files_router, tags=["Files"])
router.include_router(grants_router, tags=["Grants"])
router.include_router(health_router, tags=["Health"])

__all__ = ["router"]
```

Service auth is enforced *inside use cases* via `Depends(UseServiceAuth())` rather than as
a router-level dependency, so the health endpoint stays open and every use case explicitly
declares that it needs an identity — mirroring how AtHomeServer's use cases each pull in
`IdentityService`.

---

## 4. Flows

### 4.1 AtHomeServer uploads a profile picture

```
Browser                AtHomeServer               file-api                 MinIO      Postgres
   │  POST /me/avatar      │                          │                      │            │
   ├──────────────────────►│                          │                      │            │
   │                       │ POST /api/files          │                      │            │
   │                       │  X-Service-Token: ...    │                      │            │
   │                       │  X-On-Behalf-Of: <user>  │                      │            │
   │                       │  visibility=private      │                      │            │
   │                       ├─────────────────────────►│  put_object          │            │
   │                       │                          ├─────────────────────►│            │
   │                       │                          │  INSERT file row     │            │
   │                       │                          ├───────────────────────────────────►
   │                       │   201 {id, ...}          │                      │            │
   │                       │◄─────────────────────────┤                      │            │
   │                       │ stores file id on the    │                      │            │
   │                       │ user row in ITS OWN db   │                      │            │
   │   200 OK              │                          │                      │            │
   │◄──────────────────────┤                          │                      │            │
```

Key point: **AtHomeServer stores only the file `id`** (a UUID) in its own database, e.g.
`user.avatar_file_id`. It never stores URLs — URLs are ephemeral and always re-fetched.

### 4.2 A user views a private profile picture

```
Browser                AtHomeServer                    file-api              MinIO (cdn.<domain>)
   │  GET /me              │                               │                      │
   ├──────────────────────►│                               │                      │
   │                       │ GET /api/files/{avatar_id}    │                      │
   │                       │  X-Service-Token, X-On-Behalf-Of                     │
   │                       ├──────────────────────────────►│                      │
   │                       │                               │ visibility=private   │
   │                       │                               │ → check owner/grants │
   │                       │                               │ → presign 15 min URL │
   │                       │  200 { urls.externalUrl }     │                      │
   │                       │◄──────────────────────────────┤                      │
   │  200 { avatarUrl }    │                               │                      │
   │◄──────────────────────┤                               │                      │
   │  <img src=externalUrl>  GET https://cdn.../private/...?X-Amz-Signature=...  │
   ├─────────────────────────────────────────────────────────────────────────────►
   │                              bytes stream straight from MinIO through nginx │
   │◄─────────────────────────────────────────────────────────────────────────────
```

`file-api` decides **whether**; MinIO delivers **what**. The API never proxies bytes.

- If another service (say a "moderation" service) needs to read that same file, the owner
  service calls `POST /api/files/{id}/grants` with
  `{"grantType": "service", "grantValue": "moderation", "permission": "read"}` —
  authorization becomes a row, not a deployment.

### 4.3 Public file (e.g. a shared static asset)

Uploaded with `visibility=public` → lands in the `public` bucket, which has anonymous
`GET` enabled. `urls.externalUrl` is stable
(`https://cdn.example.com/public/athomeserver/<uuid>/logo.png`), `expiresAt` is `null`,
and services may hard-code/cache it (e.g. render it directly in HTML). No signing, no
`file-api` involvement on reads at all.

### 4.4 Streaming video / music

Identical to 4.2 with two differences:

1. `file-api` notices `mime_type` starts with `video/`/`audio/` and signs with the longer
   `PRESIGN_EXPIRY_STREAMING_SECONDS` so playback doesn't die mid-session.
2. The player issues `Range: bytes=...` requests against the presigned URL for seeking —
   MinIO handles ranges natively, nginx has `proxy_buffering off`, and no code in this
   repo touches the byte stream.

If the frontend's player needs a refresh (long movie, expired URL), it simply re-requests
the metadata endpoint — signing is cheap and stateless.

### 4.5 Soft delete & cleanup

- `DELETE /api/files/{id}` sets `deleted_at` (only the owner service may). Every read path
  filters on `deleted_at IS NULL`, so the file vanishes instantly from the API.
- Already-issued presigned URLs keep working until they expire (≤ 1 h) — acceptable, by
  design. If instant revocation ever matters, that specific file class should be proxied
  through the API instead of presigned.
- A periodic cleanup task (cron container or later the worker) hard-deletes: fetch rows
  where `deleted_at < now() - interval '30 days'`, `storage.remove()` the object, delete
  the row.

---

## 5. Consuming the file API from another repo (client side)

Inside AtHomeServer, the file service is just another external system → it gets a wrapper
in *that* repo's `infrastructure/`, exactly like `trojonetworks/`:

```
AtHomeServer/backend/infrastructure/fileservice/
├── dtos.py          # mirrors FileDto / FileUrlsDto shapes
└── service.py       # FileServiceClient
```

```python
# AtHomeServer: infrastructure/fileservice/service.py (sketch)
import uuid
import httpx

from core.configuration import config
from infrastructure.fileservice.dtos import FileDto


class FileServiceClient:
    def __init__(self) -> None:
        self._client = httpx.AsyncClient(
            base_url="http://file-api:8080/api",       # over backend-net
            headers={"X-Service-Token": config.env.FILE_SERVICE_TOKEN},
        )

    async def upload(self, filename: str, content: bytes, content_type: str,
                     user_id: uuid.UUID, visibility: str = "private") -> FileDto:
        resp = await self._client.post(
            "/files",
            params={"visibility": visibility, "owner_user_id": str(user_id)},
            headers={"X-On-Behalf-Of": str(user_id)},
            files={"file": (filename, content, content_type)},
        )
        resp.raise_for_status()
        return FileDto.model_validate(resp.json())

    async def get(self, file_id: uuid.UUID, user_id: uuid.UUID | None = None) -> FileDto:
        headers = {"X-On-Behalf-Of": str(user_id)} if user_id else {}
        resp = await self._client.get(f"/files/{file_id}", headers=headers)
        resp.raise_for_status()
        return FileDto.model_validate(resp.json())
```

AtHomeServer's use cases then depend on `FileServiceClient` the same way they depend on
their repositories. Its own endpoints only ever expose `externalUrl` to browsers,
`internalUrl` stays inside the Docker network.

---

## 6. Roadmap: the worker (edits, transcodes, thumbnails)

Not built yet, but the architecture already has its seat reserved:

- **Third compose service** (`worker`), same codebase, different entrypoint, fat image
  (`ffmpeg`, `libvips`) while `file-api` stays slim.
- New table `file_job` (`file_id`, `job_type`, `status`, `params JSONB`,
  `result_file_id`) + `application/jobs/` use cases:
  `POST /api/files/{id}/jobs` → insert `pending` row → return job id.
- Worker polls Postgres for `pending` jobs (upgrade to Redis if volume demands), runs
  ffmpeg/vips, uploads the result **as a new file through the same StorageService**, links
  it via `result_file_id`, marks the job `done`.
- Edits never mutate originals — an edit produces a new file. Cheap, reversible, and it
  makes caching/presigning trivially correct.

---

## 7. House rules

1. **Dependency direction is law.** `presentation` never imports `infrastructure`;
   `domain` imports nothing.
2. **Expected failures are `fail(...)`, never `raise`.** Exceptions are for bugs;
   `Result` is the contract between application and presentation.
3. **One use case, one file, one `__call__`.** If a use case grows a second public
   method, it's two use cases.
4. **No `minio` imports outside `infrastructure/storage/`.** Same for `sqlalchemy`
   outside `infrastructure/database/`.
5. **Never store URLs in any database — store file ids.** URLs expire; ids don't.
6. **Unauthorized reads return 404, not 403.** Existence is information.
7. **Every schema change is an alembic migration.** `alembic revision --autogenerate`,
   review the diff, commit it.
