using Domain.Entities;

namespace Infrastructure.Repositories
{
    public interface IMarkerRepository
    {
        Task<MarkerEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<MarkerEntity>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<MarkerEntity>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<IEnumerable<MarkerEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
        Task<IEnumerable<MarkerEntity>> GetPublicAsync(CancellationToken ct = default);
        Task<MarkerEntity> AddAsync(MarkerEntity entity, CancellationToken ct = default);
        Task UpdateAsync(MarkerEntity entity, CancellationToken ct = default);
        Task DeleteAsync(MarkerEntity entity, CancellationToken ct = default);
        Task<IEnumerable<MarkerEntity>> GetWithDetailsAsync(CancellationToken ct = default);
    }
}
