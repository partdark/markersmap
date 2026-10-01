using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ContentRepository : IContentRepository
    {
        private readonly MarkersDBContext _context;

        public ContentRepository(MarkersDBContext context)
        {
            _context = context;
        }

        public async Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Content.FindAsync(new object[] { id }, ct);
        }

        public async Task<IEnumerable<ContentEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Content.ToListAsync(ct);
        }

        public async Task<IEnumerable<ContentEntity>> GetByMarkerIdAsync(Guid markerId, CancellationToken ct = default)
        {
            return await _context.Content
                .Where(c => c.MarkerId == markerId)
                .ToListAsync(ct);
        }

        public async Task<ContentEntity> AddAsync(ContentEntity entity, CancellationToken ct = default)
        {
            _context.Content.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task UpdateAsync(ContentEntity entity, CancellationToken ct = default)
        {
            _context.Content.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(ContentEntity entity, CancellationToken ct = default)
        {
            _context.Content.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
