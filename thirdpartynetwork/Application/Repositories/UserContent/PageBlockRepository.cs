using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Application.Repositories.UserContent;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class PageBlockRepository(
        DataContext context,
        IMapper mapper,
        IAccessLevelVerifier verifier
    ) : UserContentBaseRepository<PageBlock>(context, verifier), IPageBlockRepository
    {
        private readonly DataContext _context = context;

        protected override IQueryable<PageBlock> GetQueryable()
        {
            return _context.PageBlocks.AsQueryable();
        }

        public bool Exists(Guid id)
        {
            return _context.PageBlocks.Any(c => c.Id == id);
        }

        public ICollection<PageBlock> GetByCustomPageId(Guid networkId, Guid customPageId)
        {
            return GetAccessibleQueryable(networkId, Constants.Permissions.ReadPageBlock).Where(p => p.CustomPageId == customPageId).ToList();
        }

        public PageBlock? GetById(Guid id)
        {
            return _context.PageBlocks.Find(id);
        }

        public PageBlock Create(PageBlock pageBlock)
        {
            _context.PageBlocks.Add(pageBlock);
            SaveAndUpdate(pageBlock);
            return pageBlock;
        }

        public PageBlock Update(Guid pageBlockId, UpdatePageBlockDto updatePageBlock)
        {
            var existing = GetById(pageBlockId);
            if (existing == null) throw new Exception("PageBlock not found");

            mapper.Map(updatePageBlock, existing); 

            SaveAndUpdate(existing);
            return existing;
        }

        public void Delete(PageBlock pageBlock)
        {
            _context.Remove(pageBlock);
            _context.SaveChanges();
        }
    }
}