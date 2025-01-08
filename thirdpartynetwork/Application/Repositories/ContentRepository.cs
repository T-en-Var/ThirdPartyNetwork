using thirdpartynetwork.Data;
using thirdpartynetwork.Interfaces.Repositories;
using thirdpartynetwork.Models;

namespace thirdpartynetwork.Repositories
{
    public class ContentRepository(DataContext _context) : BaseRepository(_context), IContentRepository
    {
        public Content? Create(Content content)
        {
            _context.Contents.Add(content);
            if (!Save()) return null;
            return content;
        }

        public bool Delete(Content content)
        {
            _context.Remove(content);
            return Save();
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

        public Content? Update(Content content)
        {
            Content? existing = this.GetById(content.Id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(content);

            if (!Save()) return null;
            return existing;
        }
    }
}
