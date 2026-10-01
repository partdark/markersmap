using Application.DTOs;

namespace Application.Services
{
    public interface IMarkerService
    {
        Task<IEnumerable<MarkerDTO>> GetAllAsync(Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<MarkerDTO?> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<IEnumerable<MarkerDTO>> GetByUserIdAsync(Guid userId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<IEnumerable<MarkerDTO>> GetByCategoryIdAsync(Guid categoryId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<IEnumerable<MarkerDTO>> GetPublicAsync(CancellationToken ct = default);
        Task<MarkerDTO> CreateAsync(MarkerDTO dto, Guid currentUserId, CancellationToken ct = default);
        Task<MarkerDTO> UpdateAsync(Guid id, MarkerDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
        Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
    }
}
