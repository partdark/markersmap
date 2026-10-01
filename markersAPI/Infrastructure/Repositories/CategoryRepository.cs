using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MarkersDBContext _context;

        public CategoryRepository(MarkersDBContext context)
        {
            _context = context;
        }

        public async Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Category.FindAsync(new object[] { id }, ct);
        }

        public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Category.ToListAsync(ct);
        }

        public async Task<CategoryEntity?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _context.Category.FirstOrDefaultAsync(c => c.Name == name, ct);
        }

        public async Task<CategoryEntity> AddAsync(CategoryEntity entity, CancellationToken ct = default)
        {
            _context.Category.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(CategoryEntity entity, CancellationToken ct = default)
        {
            _context.Category.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(CategoryEntity entity, CancellationToken ct = default)
        {
            _context.Category.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<CategoryEntity>> GetWithMarkersAsync(CancellationToken ct = default)
        {
            return await _context.Category
                .Include(c => c.Markers)
                .ToListAsync(ct);
        }
    }
}
