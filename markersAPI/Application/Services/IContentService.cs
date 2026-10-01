using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public interface IContentService
    {
        Task<ContentDTO?> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<IEnumerable<ContentDTO>> GetByMarkerIdAsync(Guid markerId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<ContentDTO> CreateAsync(ContentDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<ContentDTO> UploadAsync(Guid markerId, IFormFile file, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
        Task<ContentDTO> UpdateAsync(Guid id, ContentDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
        Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken ct = default);
    }
}
