using Domain.Entities;

namespace Infrastructure.Repositories
{
    public interface ICategoryRepository
    {
        Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default);
        Task<CategoryEntity?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<CategoryEntity> AddAsync(CategoryEntity entity, CancellationToken ct = default);
        Task UpdateAsync(CategoryEntity entity, CancellationToken ct = default);
        Task DeleteAsync(CategoryEntity entity, CancellationToken ct = default);
        Task<IEnumerable<CategoryEntity>> GetWithMarkersAsync(CancellationToken ct = default);
    }
}
