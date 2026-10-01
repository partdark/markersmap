using Application.DTOs;

namespace Application.Services
{
    public interface ICategoryService
    {
        Task<CategoryDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<CategoryDTO>> GetAllAsync(CancellationToken ct = default);
        Task<CategoryDTO> CreateAsync(CategoryDTO dto, CancellationToken ct = default);
        Task<CategoryDTO> UpdateAsync(Guid id, CategoryDTO dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
