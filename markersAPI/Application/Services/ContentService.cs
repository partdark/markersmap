using Application.DTOs;
using Infrastructure.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class ContentService : IContentService
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 МБ
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".webm", ".mp3", ".pdf" };

        private readonly IContentRepository _repository;
        private readonly IMarkerRepository _markerRepository;
        private readonly IConfiguration _configuration;

        public ContentService(IContentRepository repository, IMarkerRepository markerRepository, IConfiguration configuration)
        {
            _repository = repository;
            _markerRepository = markerRepository;
            _configuration = configuration;
        }

        public async Task<ContentDTO?> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity == null) return null;

            if (!await CanReadMarkerContentAsync(entity.MarkerId, currentUserId, isAdmin, ct))
                return null;

            return MapToDTO(entity);
        }

        public async Task<IEnumerable<ContentDTO>> GetByMarkerIdAsync(Guid markerId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            if (!await CanReadMarkerContentAsync(markerId, currentUserId, isAdmin, ct))
                throw new UnauthorizedAccessException("Доступ запрещён");

            var entities = await _repository.GetByMarkerIdAsync(markerId, ct);
            return entities.Select(MapToDTO);
        }

        public async Task<ContentDTO> CreateAsync(ContentDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var marker = await _markerRepository.GetByIdAsync(dto.MarkerId, ct)
                ?? throw new KeyNotFoundException($"Marker with id {dto.MarkerId} not found");

            if (marker.UserId != currentUserId && !isAdmin)
                throw new UnauthorizedAccessException("Доступ запрещён");

            var entity = new ContentEntity
            {
                Id = Guid.NewGuid(),
                Path = dto.Path,
                MarkerId = dto.MarkerId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(entity, ct);
            return MapToDTO(created);
        }

        public async Task<ContentDTO> UploadAsync(Guid markerId, IFormFile file, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var marker = await _markerRepository.GetByIdAsync(markerId, ct)
                ?? throw new KeyNotFoundException($"Marker with id {markerId} not found");

            if (marker.UserId != currentUserId && !isAdmin)
                throw new UnauthorizedAccessException("Доступ запрещён");

            if (file == null || file.Length == 0)
                throw new InvalidDataException("Файл не передан");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidDataException($"Размер файла превышает {MaxFileSizeBytes / 1024 / 1024} МБ");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                throw new InvalidDataException($"Недопустимый тип файла: {ext}");

            // Имя файла генерирует сервер: GUID + расширение (защита от коллизий и path traversal)
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var uploadPath = _configuration["FileStorage:UploadPath"] ?? "./uploads";
            Directory.CreateDirectory(uploadPath);
            var fullPath = Path.Combine(uploadPath, fileName);

            try
            {
                await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                {
                    await file.CopyToAsync(stream, ct);
                }

                var entity = new ContentEntity
                {
                    Id = Guid.NewGuid(),
                    Path = fileName,
                    MarkerId = markerId,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _repository.AddAsync(entity, ct);
                return MapToDTO(created);
            }
            catch
            {
                // Если запись в БД не удалась — файл не оставляем
                try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { /* ignore */ }
                throw;
            }
        }

        public async Task<ContentDTO> UpdateAsync(Guid id, ContentDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Content with id {id} not found");

            var marker = await _markerRepository.GetByIdAsync(entity.MarkerId, ct);
            if (marker == null || (marker.UserId != currentUserId && !isAdmin))
                throw new UnauthorizedAccessException("Доступ запрещён");

            if (dto.MarkerId != entity.MarkerId)
            {
                var newMarker = await _markerRepository.GetByIdAsync(dto.MarkerId, ct)
                    ?? throw new KeyNotFoundException($"Marker with id {dto.MarkerId} not found");
                if (newMarker.UserId != currentUserId && !isAdmin)
                    throw new UnauthorizedAccessException("Доступ запрещён");
            }

            entity.Path = dto.Path;
            entity.MarkerId = dto.MarkerId;

            await _repository.UpdateAsync(entity, ct);
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Content with id {id} not found");

            var marker = await _markerRepository.GetByIdAsync(entity.MarkerId, ct);
            if (marker == null || (marker.UserId != currentUserId && !isAdmin))
                throw new UnauthorizedAccessException("Доступ запрещён");

            await _repository.DeleteAsync(entity, ct);
        }

        private async Task<bool> CanReadMarkerContentAsync(Guid markerId, Guid? currentUserId, bool isAdmin, CancellationToken ct)
        {
            var marker = await _markerRepository.GetByIdAsync(markerId, ct);
            if (marker == null) return false;
            return marker.IsPublic || isAdmin || marker.UserId == currentUserId;
        }

        private ContentDTO MapToDTO(ContentEntity entity)
        {
            var baseUrl = (_configuration["FileStorage:BaseUrl"] ?? "").TrimEnd('/');
            return new ContentDTO
            {
                Id = entity.Id,
                Path = entity.Path,
                Url = string.IsNullOrEmpty(entity.Path) ? null : $"{baseUrl}/{entity.Path.TrimStart('/')}",
                MarkerId = entity.MarkerId,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
