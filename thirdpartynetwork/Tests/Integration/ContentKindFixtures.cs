using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Tests.Integration;

/// <summary>
/// One entry per ContentKind, describing everything the 3 gating-group test classes need to
/// treat all five kinds uniformly: how to seed an instance directly via DataContext (bypassing
/// HTTP — see BUG-CREATE-001 in RbacOnlyEndpointTests, which makes the real POST endpoints
/// unusable for File/Blog/Configuration/CustomPage), route shape, and which of the "read"
/// endpoints exist for this kind.
/// </summary>
public static class ContentKindCatalog
{
    public static readonly ContentKindDescriptor[] All =
    [
        new ContentKindDescriptor(
            ContentKind.File, "files", "fileId", HasList: true, AnonRedirectSegment: "cdn",
            SingleGetUsesManagePermission: true /* PLAN.md-adjacent, but NOT documented there — see BUG-002 */),
        new ContentKindDescriptor(ContentKind.Blog, "blogs", "blogId", HasList: true, AnonRedirectSegment: "view"),
        new ContentKindDescriptor(ContentKind.Configuration, "configurations", "configurationId", HasList: true, AnonRedirectSegment: "view"),
        new ContentKindDescriptor(ContentKind.CustomPage, "customPages", "customPageId", HasList: true, AnonRedirectSegment: "view"),
        new ContentKindDescriptor(ContentKind.PageBlock, "customPages/{parentCustomPageId}/pageBlocks", "pageBlockId", HasList: false, AnonRedirectSegment: "view"),
    ];
}

public record ContentKindDescriptor(
    ContentKind Kind,
    string RouteSegmentTemplate,
    string RouteKey,
    bool HasList,
    string AnonRedirectSegment,
    bool SingleGetUsesManagePermission = false)
{
    /// <summary>Base collection route under a network, e.g. "networks/{networkId}/files". For
    /// PageBlock this needs parentCustomPageId filled in first (see WithParent).</summary>
    public string CollectionRoute(Guid networkId, Guid? parentCustomPageId = null)
    {
        var segment = RouteSegmentTemplate.Replace("{parentCustomPageId}", parentCustomPageId?.ToString());
        return $"networks/{networkId}/{segment}";
    }

    public string ItemRoute(Guid networkId, Guid contentId, Guid? parentCustomPageId = null) =>
        $"{CollectionRoute(networkId, parentCustomPageId)}/{contentId}";
}

/// <summary>A seeded content item plus the identifiers needed to route to it.</summary>
public record SeededContent(ContentKind Kind, Guid NetworkId, Guid ContentId, Guid? ParentCustomPageId, Guid? AuthorUserProxyId);

public static class ContentKindFixtures
{
    /// <summary>
    /// Seeds one instance of the given kind directly via EF (not via the — currently broken
    /// for 4/5 kinds, see BUG-CREATE-001 — HTTP create endpoints). For PageBlock, also seeds a
    /// parent CustomPage carrying the SAME state, since PageBlock's own AccessLevelVerifier
    /// check delegates entirely to its parent CustomPage (AccessLevelVerifier.cs:
    /// CheckPageBlockAccessAsync -> CheckCustomPageAccessAsync) — the PageBlock's own
    /// PublishedAt/AuthorUserProxyId/grants are irrelevant to its own visibility.
    /// </summary>
    public static async Task<SeededContent> CreateAsync(
        DataContext ctx,
        ContentKind kind,
        Guid networkId,
        Guid? authorUserProxyId = null,
        DateTime? publishedAt = null,
        OwnershipMode ownershipMode = OwnershipMode.AuthorOwned,
        DateTime? deletedAt = null)
    {
        switch (kind)
        {
            case ContentKind.File:
            {
                var file = new File
                {
                    Id = Guid.NewGuid(),
                    Bucket = "private",
                    ObjectKey = $"test/{Guid.NewGuid()}",
                    Format = "png",
                    MediaType = "image",
                    Name = "test-file",
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.Files.Add(file);
                await ctx.SaveChangesAsync();
                return new SeededContent(kind, networkId, file.Id, null, authorUserProxyId);
            }
            case ContentKind.Blog:
            {
                var blog = new Blog
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Blog",
                    Slug = $"test-blog-{Guid.NewGuid():N}",
                    Summary = "summary",
                    Body = JsonDocument.Parse("{\"blocks\":[]}"),
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.Blogs.Add(blog);
                await ctx.SaveChangesAsync();
                return new SeededContent(kind, networkId, blog.Id, null, authorUserProxyId);
            }
            case ContentKind.Configuration:
            {
                var configuration = new Configuration
                {
                    Id = Guid.NewGuid(),
                    Key = $"test.key.{Guid.NewGuid():N}",
                    Value = JsonDocument.Parse("{\"enabled\":true}"),
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.Configurations.Add(configuration);
                await ctx.SaveChangesAsync();
                return new SeededContent(kind, networkId, configuration.Id, null, authorUserProxyId);
            }
            case ContentKind.CustomPage:
            {
                var page = new CustomPage
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Page",
                    Slug = $"test-page-{Guid.NewGuid():N}",
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.CustomPages.Add(page);
                await ctx.SaveChangesAsync();
                return new SeededContent(kind, networkId, page.Id, null, authorUserProxyId);
            }
            case ContentKind.PageBlock:
            {
                // Parent CustomPage carries the requested state — see method doc.
                var parent = new CustomPage
                {
                    Id = Guid.NewGuid(),
                    Name = "Parent Page",
                    Slug = $"parent-page-{Guid.NewGuid():N}",
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.CustomPages.Add(parent);

                var block = new PageBlock
                {
                    Id = Guid.NewGuid(),
                    Text = "block text",
                    Position = 0,
                    Data = JsonDocument.Parse("{}"),
                    CustomPageId = parent.Id,
                    NetworkId = networkId,
                    AuthorUserProxyId = authorUserProxyId,
                    PublishedAt = publishedAt,
                    OwnershipMode = ownershipMode,
                    DeletedAt = deletedAt,
                };
                ctx.PageBlocks.Add(block);
                await ctx.SaveChangesAsync();
                return new SeededContent(kind, networkId, block.Id, parent.Id, authorUserProxyId);
            }
            default:
                throw new NotSupportedException(kind.ToString());
        }
    }

    /// <summary>Minimal valid HTTP request body for POST-create, per kind (used only by the
    /// RBAC-only group's Create tests, which exercise the real endpoint directly).</summary>
    public static HttpContent BuildCreateRequestBody(ContentKind kind) => kind switch
    {
        ContentKind.File => BuildFileUploadContent(),
        ContentKind.Blog => JsonContent(new
        {
            title = "New Blog", summary = "s", body = JsonDocument.Parse("{}").RootElement,
        }),
        ContentKind.Configuration => JsonContent(new
        {
            key = $"new.key.{Guid.NewGuid():N}", value = JsonDocument.Parse("{}"),
        }),
        ContentKind.CustomPage => JsonContent(new
        {
            name = "New Page", slug = $"new-page-{Guid.NewGuid():N}",
        }),
        ContentKind.PageBlock => JsonContent(new
        {
            text = "new block", position = 0, data = JsonDocument.Parse("{}").RootElement,
        }),
        _ => throw new NotSupportedException(kind.ToString()),
    };

    /// <summary>Minimal valid HTTP request body for PUT-update, per kind.</summary>
    public static HttpContent BuildUpdateRequestBody(ContentKind kind) => kind switch
    {
        ContentKind.File => JsonContent(new { name = "renamed" }),
        ContentKind.Blog => JsonContent(new
        {
            title = "Updated Blog", summary = "s2", body = JsonDocument.Parse("{}").RootElement,
        }),
        ContentKind.Configuration => JsonContent(new
        {
            key = $"updated.key.{Guid.NewGuid():N}", value = JsonDocument.Parse("{}"),
        }),
        ContentKind.CustomPage => JsonContent(new
        {
            name = "Updated Page", slug = $"updated-page-{Guid.NewGuid():N}",
        }),
        ContentKind.PageBlock => JsonContent(new
        {
            text = "updated block", position = 1, data = JsonDocument.Parse("{}").RootElement,
        }),
        _ => throw new NotSupportedException(kind.ToString()),
    };

    private static HttpContent BuildFileUploadContent()
    {
        var form = new MultipartFormDataContent();
        var bytes = "fake-file-bytes"u8.ToArray();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "test.png");
        return form;
    }

    private static HttpContent JsonContent(object body)
    {
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
