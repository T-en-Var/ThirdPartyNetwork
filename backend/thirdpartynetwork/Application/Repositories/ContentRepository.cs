using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class ContentRepository(DataContext context) : BaseRepository(context), IContentRepository
    {
        private readonly DataContext _context = context;

        public Content Create(Content content)
        {
            _context.Contents.Add(content);
            SaveAndUpdate(content);
            return content;
        }

        public void Delete(Content content)
        {
            _context.Remove(content);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Contents.Any(c => c.Id == id);
        }

        public ICollection<Content> GetAll()
        {
            return [.. _context.Contents];
        }

        public Content? GetById(Guid id)
        {
            return _context.Contents.Find(id);
        }

        public ICollection<Content> GetByNetworkId(Guid networkId)
        {
            return [.. _context.Contents.Where(c => c.Network.Id == networkId)];
        }

        public Content Update(Content content)
        {
            var existing = this.GetById(content.Id);
            if (existing == null) throw new Exception("Content not found");

            _context.Entry(existing).CurrentValues.SetValues(content);

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
