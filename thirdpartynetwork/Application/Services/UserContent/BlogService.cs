using AutoMapper;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Blog;
using thirdpartynetwork.Application.Helpers;
using thirdpartynetwork.Core.Interfaces.Services;
using thirdpartynetwork.Core.Interfaces.Repositories;

namespace thirdpartynetwork.Application.Services;

public class BlogService(
    IBlogRepository repository,
    INetworkService networkService,
    IIdentityService identityService,
    INetworkUserService networkUserService,
    IFileService fileService,
    IEntitlementService entitlementService,
    IAccessLevelVerifier accessLevelVerifier,
    IMapper mapper) : UserContentBaseService<Blog>(repository, networkUserService), IBlogService
{
    private readonly INetworkUserService _networkUserService = networkUserService;

    public async Task<ICollection<Blog>> GetByNetworkIdAsync(Guid networkId)
    {
        return await repository.GetByNetworkIdAsync(networkId);
    }

    public async Task<Blog?> GetByIdAsync(Guid networkId, Guid blogId)
    {
        return await repository.GetByIdAsync(blogId);
    }

    public async Task<Blog?> GetBySlugAsync(Guid networkId, string slug)
    {
        return await repository.GetByNetworkIdAndSlugAsync(networkId, slug);
    }

    public async Task<Blog> CreateAsync(Guid networkId, CreateBlogDto createBlog)
    {
        var network = await networkService.GetByIdAsync(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var userProxy = await identityService.GetUserProxyAsync();
        var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id);
        if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");
        
        if (!await entitlementService.CheckUserAddBlogEntitlementAsync(networkUser.UserProxyId, networkId))
            throw new UnauthorizedHttpException("You are exceeding a blog limit.");
        
        var baseSlug = SlugGenerator.GenerateSlug(createBlog.Title);
        var finalSlug = baseSlug;
        var counter = 1;
        
        while (await repository.GetByNetworkIdAndSlugAsync(networkId, finalSlug) != null)
        {
            finalSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        var blog = mapper.Map<Blog>(createBlog);
        blog.Slug = finalSlug;
        blog.Network = network;
        blog.AuthorUserProxyId = networkUser.UserProxyId;

        if (createBlog.CoverImageId != null)
        {
            var coverImage = await fileService.GetByIdAsync(networkId, createBlog.CoverImageId.Value);
            if (coverImage == null) throw new NotFoundHttpException("Cover image not found");

            if (!await accessLevelVerifier.CanNetworkExternallyAdoptAsync(coverImage, ContentKind.File, networkId))
                throw new UnauthorizedHttpException("That file's network does not allow its files to be reused externally.");

            blog.CoverImage = coverImage;
        }

        return await repository.CreateAsync(blog);
    }

    public async Task<Blog> UpdateAsync(Guid networkId, Guid blogId, UpdateBlogDto updateBlog)
    {
        if (!await repository.ExistsAsync(blogId)) throw new NotFoundHttpException();

        var network = await networkService.GetByIdAsync(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        if (updateBlog.CoverImageId.HasValue)
        {
            var coverImage = await fileService.GetByIdAsync(networkId, updateBlog.CoverImageId.Value);
            if (coverImage == null) throw new NotFoundHttpException("Cover image not found");

            if (!await accessLevelVerifier.CanNetworkExternallyAdoptAsync(coverImage, ContentKind.File, networkId))
                throw new UnauthorizedHttpException("That file's network does not allow its files to be reused externally.");
        }

        var blog = await repository.UpdateAsync(blogId, updateBlog);
        return blog;
    }

    public async Task DeleteByIdAsync(Guid networkId, Guid blogId)
    {
        var blog = await GetByIdAsync(networkId, blogId);
        if (blog == null) return;

        await repository.DeleteAsync(blog);
    }
}
