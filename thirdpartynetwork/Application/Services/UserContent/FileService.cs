using AutoMapper;
using CloudinaryDotNet.Actions;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Services
{
    public class FileService(
        IFileRepository repository,
        INetworkService networkService,
        IIdentityService identityService,
        INetworkUserService networkUserService,
        ICDNService cdnService,
        IMapper mapper) : UserContentBaseService<File>(repository, networkUserService), IFileService
    {
        private readonly INetworkUserService _networkUserService = networkUserService;

        public ICollection<File> GetByNetworkId(Guid networkId)
        {
            var file = repository.GetByNetworkId(networkId);
            return file;
        }

        public File? GetById(Guid networkId, Guid fileId)
        {
            var file = repository.GetById(fileId);
            return file;
        }

        public File Create(Guid networkId, CreateFileDto createFile)
        {
            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var userProxy = identityService.GetUserProxy();
            var networkUser = _networkUserService.GetByIds(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            var file = mapper.Map<File>(createFile);
            file.Network = network;
            file.Author = networkUser;

            file = repository.Create(file);
            return file;
        }

        public async Task<File> UploadFileAsync(Guid networkId, IFormFile file)
        {
            var network = networkService.GetById(networkId) ?? throw new NotFoundHttpException("Network not found");

            var userProxy = identityService.GetUserProxy();
            var networkUser = _networkUserService.GetByIds(networkId, userProxy.Id) ?? throw new NotFoundHttpException("NetworkUser not found");

            var publicId = Guid.NewGuid();

            // Upload via your ICloudinaryService (which wraps Cloudinary SDK)
            var uploadResult = await cdnService.UploadAsync(file, publicId, network.Id.ToString());

            // Map to your File entity
            var newFile = new File
            {
                PublicId = uploadResult.Id,
                Url = uploadResult.SecureUrl,
                Format = uploadResult.Format,
                SizeBytes = uploadResult.Bytes,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Duration = uploadResult.Duration,
                MediaType = uploadResult.ResourceType,
                Network = network,
                Author = networkUser,
                Name = file.FileName,
                AccessLevel = AccessLevel.Private
            };

            return repository.Create(newFile);
        }

        public File Update(Guid networkId, Guid fileId, UpdateFileDto updateFile)
        {
            if (!repository.Exists(fileId)) throw new NotFoundHttpException();

            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var file = repository.Update(fileId, updateFile);
            return file;
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid fileId)
        {
            var file = GetById(networkId, fileId);
            if (file == null) return;

            await cdnService.DeleteAsync($"{networkId}/{file.PublicId}", file.MediaType);

            repository.Delete(file);
        }
    }
}