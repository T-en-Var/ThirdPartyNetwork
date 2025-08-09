using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Repositories
{
    public class FileRepository(DataContext context, IMapper mapper) : BaseRepository(context), IFileRepository
    {
        private readonly DataContext _context = context;

        public File Create(File file)
        {
            _context.Files.Add(file);
            SaveAndUpdate(file);
            return file;
        }

        public void Delete(File file)
        {
            _context.Remove(file);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Files.Any(c => c.Id == id);
        }

        public ICollection<File> GetAll()
        {
            return [.. _context.Files];
        }

        public File? GetById(Guid id)
        {
            return _context.Files.Find(id);
        }

        public ICollection<File> GetByNetworkId(Guid networkId)
        {
            return [.. _context.Files.Where(c => c.Network.Id == networkId).Include(f => f.Author)];
        }

        public File Update(Guid fileId, UpdateFileDto updateFile)
        {
            var existing = GetById(fileId);
            if (existing == null) throw new Exception("File not found");

            mapper.Map(updateFile, existing); 

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
