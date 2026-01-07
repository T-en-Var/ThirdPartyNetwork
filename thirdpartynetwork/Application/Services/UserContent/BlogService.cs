using AutoMapper;
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
    IMapper mapper) : UserContentBaseService<Blog>(repository, networkUserService), IBlogService
{
    private readonly INetworkUserService _networkUserService = networkUserService;

    public ICollection<Blog> GetByNetworkId(Guid networkId)
    {
        return repository.GetByNetworkId(networkId);
    }

    public Blog? GetById(Guid networkId, Guid blogId)
    {
        return repository.GetById(blogId);
    }

    public Blog? GetBySlug(Guid networkId, string slug)
    {
        return repository.GetByNetworkIdAndSlug(networkId, slug);
    }

    public Blog Create(Guid networkId, CreateBlogDto createBlog)
    {
        var network = networkService.GetById(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var userProxy = identityService.GetUserProxy();
        var networkUser = _networkUserService.GetByIds(networkId, userProxy.Id);
        if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");
        
        var baseSlug = SlugGenerator.GenerateSlug(createBlog.Title);
        var finalSlug = baseSlug;
        var counter = 1;
        
        while (repository.GetByNetworkIdAndSlug(networkId, finalSlug) != null)
        {
            finalSlug = $"{baseSlug}-{counter}";
            counter++;
        }

        var blog = mapper.Map<Blog>(createBlog);
        blog.Slug = finalSlug;
        blog.Network = network;
        blog.Author = networkUser;

        if (createBlog.CoverImageId != null)
        {
            var coverImage = fileService.GetById(networkId, createBlog.CoverImageId.Value);
            if (coverImage == null) throw new NotFoundHttpException("Cover image not found");
            blog.CoverImage = coverImage;
        }
        
        return repository.Create(blog);
    }

    public Blog Update(Guid networkId, Guid blogId, UpdateBlogDto updateBlog)
    {
        if (!repository.Exists(blogId)) throw new NotFoundHttpException();

        var network = networkService.GetById(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        if (updateBlog.CoverImageId.HasValue)
        {
            var coverImage = fileService.GetById(networkId, updateBlog.CoverImageId.Value);
            if (coverImage == null) throw new NotFoundHttpException("Cover image not found");
        }

        var blog = repository.Update(blogId, updateBlog);
        return blog;
    }

    public void DeleteById(Guid networkId, Guid blogId)
    {
        var blog = GetById(networkId, blogId);
        if (blog == null) return;

        repository.Delete(blog);
    }
}
