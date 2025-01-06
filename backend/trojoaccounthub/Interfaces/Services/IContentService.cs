namespace trojoaccounthub.Interfaces.Services
{
    public interface IContentService : IBaseService
    {
        ICollection<ContentDto> GetAll();
        ICollection<ContentDto> GetByNetworkId(int networkId);
        ContentDto? GetById(int id);
        ContentDto? Create(int networkId, CreateContentDto content);
        ContentDto? UpdateById(int id, UpdateContentDto content);
        bool DeleteById(int id);
    }
}
