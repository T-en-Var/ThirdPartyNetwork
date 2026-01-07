using AutoMapper;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Repositories
{
    public class FileRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<File>(context, verifier), IFileRepository
    {
        private readonly DataContext _context = context;
        protected override string ReadPermissionName => Constants.Permissions.ReadFile;

        protected override IQueryable<File> GetQueryable()
        {
            return _context.Files.AsQueryable();
        }

        public File Create(File file)
        {
            _context.Files.Add(file);
            SaveAndUpdate(file);
            return file;
        }

        public void Delete(File file)
        {
            if (file.IsSystemProtected) throw new UnauthorizedHttpException("File is system protected");
            
            _context.Remove(file);
            _context.SaveChanges();
        }

        public bool Exists(Guid id)
        {
            return _context.Files.Any(c => c.Id == id);
        }

        public File? GetById(Guid id)
        {
            return _context.Files.Find(id);
        }

        public ICollection<File> GetByNetworkId(Guid networkId)
        {
            return GetAccessibleQueryable(networkId).Where(c => c.NetworkId == networkId).ToList();
        }

        public File Update(Guid fileId, UpdateFileDto updateFile)
        {
            var existing = GetById(fileId);
            if (existing == null) throw new Exception("File not found");
            
            if (existing.IsSystemProtected) throw new UnauthorizedHttpException("File is system protected");

            mapper.Map(updateFile, existing); 

            SaveAndUpdate(existing);
            return existing;
        }
    }
}
