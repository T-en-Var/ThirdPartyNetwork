using AutoMapper;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories
{
    public class PageBlockRepository(DataContext context, IMapper mapper) : BaseRepository(context), IPageBlockRepository
    {
        private readonly DataContext _context = context;

        public bool Exists(Guid id)
        {
            return _context.PageBlocks.Any(c => c.Id == id);
        }

        public ICollection<PageBlock> GetAll()
        {
            return [.. _context.PageBlocks];
        }

        public ICollection<PageBlock> GetByCustomPageId(Guid customPageId)
        {
            return [.. _context.PageBlocks.Where(p => p.CustomPageId == customPageId)];
        }

        public ICollection<PageBlock> GetByParentPageId(Guid parentPageId)
        {
            return [.. _context.PageBlocks.Where(p => p.ParentPageId == parentPageId)];
        }

        public ICollection<PageBlock> GetByParentPageBlockId(Guid parentPageId)
        {
            return [.. _context.PageBlocks.Where(p => p.ParentPageId == parentPageId)];
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