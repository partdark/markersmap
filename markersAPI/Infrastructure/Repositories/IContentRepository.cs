using Domain.Entities;

namespace Infrastructure.Repositories
{
    public interface IContentRepository
    {
        Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<ContentEntity>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<ContentEntity>> GetByMarkerIdAsync(Guid markerId, CancellationToken ct = default);
        Task<ContentEntity> AddAsync(ContentEntity entity, CancellationToken ct = default);
        Task UpdateAsync(ContentEntity entity, CancellationToken ct = default);
        Task DeleteAsync(ContentEntity entity, CancellationToken ct = default);
    }
}
