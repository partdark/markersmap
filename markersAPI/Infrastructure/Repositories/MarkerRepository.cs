using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MarkerRepository : IMarkerRepository
    {
        private readonly MarkersDBContext _context;

        public MarkerRepository(MarkersDBContext context)
        {
            _context = context;
        }

        public async Task<MarkerEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<IEnumerable<MarkerEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<MarkerEntity>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .Where(m => m.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<MarkerEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .Where(m => m.CategoryId == categoryId)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<MarkerEntity>> GetPublicAsync(CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .Where(m => m.IsPublic)
                .ToListAsync(ct);
        }

        public async Task<MarkerEntity> AddAsync(MarkerEntity entity, CancellationToken ct = default)
        {
            _context.Marker.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(MarkerEntity entity, CancellationToken ct = default)
        {
            _context.Marker.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(MarkerEntity entity, CancellationToken ct = default)
        {
            _context.Marker.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<MarkerEntity>> GetWithDetailsAsync(CancellationToken ct = default)
        {
            return await _context.Marker
                .Include(m => m.Category)
                .Include(m => m.User)
                .Include(m => m.Content)
                .ToListAsync(ct);
        }
    }
}
