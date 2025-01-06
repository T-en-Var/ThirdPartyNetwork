namespace trojoaccounthub.Interfaces.Services
{
    public interface IContentService : IBaseService
    {
        ICollection<ContentDto> GetAll();
        ICollection<ContentDto> GetByNetworkId(Guid networkId);
        ContentDto? GetById(Guid id);
        ContentDto? Create(Guid networkId, CreateContentDto content);
        ContentDto? UpdateById(Guid id, UpdateContentDto content);
        bool DeleteById(Guid id);
    }
}
