using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Services
{
    public class FileService(
        IFileRepository repository,
        INetworkService networkService,
        IIdentityService identityService,
        INetworkUserService networkUserService,
        ICDNService cdnService,
        IFeatureFlagValidator featureFlagValidator,
        IMapper mapper) : UserContentBaseService<File>(repository, networkUserService), IFileService
    {
        private readonly INetworkUserService _networkUserService = networkUserService;

        public async Task<ICollection<File>> GetByNetworkIdAsync(Guid networkId)
        {
            var file = await repository.GetByNetworkIdAsync(networkId);
            return file;
        }

        public async Task<File?> GetByIdAsync(Guid networkId, Guid fileId)
        {
            var file = await repository.GetByIdAsync(networkId, fileId);
            return file;
        }

        public async Task<File> CreateAsync(Guid networkId, CreateFileDto createFile)
        {
            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var userProxy = await identityService.GetUserProxyAsync();
            var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            var file = mapper.Map<File>(createFile);
            file.Network = network;
            file.AuthorUserProxyId = networkUser.UserProxyId;

            file = await repository.CreateAsync(networkId, file);
            return file;
        }

        public async Task<File> UploadFileAsync(Guid networkId, IFormFile file)
        {
            var network = await networkService.GetByIdAsync(networkId) ?? throw new NotFoundHttpException("Network not found");

            var userProxy = await identityService.GetUserProxyAsync();
            var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id) ?? throw new NotFoundHttpException("NetworkUser not found");

            if (!await featureFlagValidator.CheckUserAddNetworkFileFeatureFlagAsync(networkUser.UserProxyId, networkId, file))
                throw new UnauthorizedHttpException("You are exceeding a file upload limit.");

            var objectId = Guid.NewGuid();
            var uploadResult = await cdnService.UploadAsync(file, objectId, network.Id.ToString());

            var newFile = new File
            {
                Bucket = uploadResult.Bucket,
                ObjectKey = uploadResult.ObjectKey,
                Format = uploadResult.Format,
                SizeBytes = uploadResult.Bytes,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Duration = uploadResult.Duration,
                MediaType = uploadResult.ResourceType,
                Network = network,
                AuthorUserProxyId = networkUser.UserProxyId,
                Name = file.FileName,
                PublishedAt = DateTime.UtcNow,
            };

            return await repository.CreateAsync(networkId, newFile);
        }

        public async Task<File> UpdateAsync(Guid networkId, Guid fileId, UpdateFileDto updateFile)
        {
            if (!await repository.ExistsAsync(fileId)) throw new NotFoundHttpException();

            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            return await repository.UpdateAsync(networkId, fileId, updateFile);
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid fileId)
        {
            var file = await GetByIdAsync(networkId, fileId);
            if (file == null) return;

            await cdnService.DeleteAsync(file.Bucket, file.ObjectKey);

            await repository.DeleteAsync(file);
        }
    }
}