using Application.DTOs;
using Infrastructure.Repositories;
using Domain.Entities;

namespace Application.Services
{
    public class MarkerService : IMarkerService
    {
        private readonly IMarkerRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public MarkerService(IMarkerRepository repository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<MarkerDTO>> GetAllAsync(Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entities = await _repository.GetAllAsync(ct);
            return entities
                .Where(m => isAdmin || m.IsPublic || m.UserId == currentUserId)
                .Select(MapToDTO);
        }

        public async Task<MarkerDTO?> GetByIdAsync(Guid id, Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            if (entity == null) return null;

            // Публичные метки видит любой, приватные — только создатель или Admin
            if (entity.IsPublic || isAdmin || entity.UserId == currentUserId)
                return MapToDTO(entity);

            return null;
        }

        public async Task<IEnumerable<MarkerDTO>> GetByUserIdAsync(Guid userId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entities = await _repository.GetByUserIdAsync(userId, ct);
            return entities
                .Where(m => isAdmin || m.IsPublic || m.UserId == currentUserId)
                .Select(MapToDTO);
        }

        public async Task<IEnumerable<MarkerDTO>> GetByCategoryIdAsync(Guid categoryId, Guid? currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entities = await _repository.GetByCategoryIdAsync(categoryId, ct);
            return entities
                .Where(m => isAdmin || m.IsPublic || m.UserId == currentUserId)
                .Select(MapToDTO);
        }

        public async Task<IEnumerable<MarkerDTO>> GetPublicAsync(CancellationToken ct = default)
        {
            var entities = await _repository.GetPublicAsync(ct);
            return entities.Select(MapToDTO);
        }

        public async Task<MarkerDTO> CreateAsync(MarkerDTO dto, Guid currentUserId, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, ct)
                ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

            var entity = new MarkerEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CategoryId = dto.CategoryId,
                UserId = currentUserId,
                IsPublic = dto.IsPublic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(entity, ct);

            // Перечитываем с категорией, чтобы заполнить CategoryName в DTO
            var withDetails = await _repository.GetByIdAsync(created.Id, ct);
            return MapToDTO(withDetails ?? created);
        }

        public async Task<MarkerDTO> UpdateAsync(Guid id, MarkerDTO dto, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Marker with id {id} not found");

            // Редактировать может только владелец или Admin
            if (entity.UserId != currentUserId && !isAdmin)
                throw new UnauthorizedAccessException("Доступ запрещён");

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, ct)
                ?? throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found");

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.CategoryId = dto.CategoryId;
            entity.IsPublic = dto.IsPublic;
            entity.UpdatedAt = DateTime.UtcNow;
            // Владелец не меняется при редактировании

            await _repository.UpdateAsync(entity, ct);
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Marker with id {id} not found");

            // Удалять может только владелец или Admin
            if (entity.UserId != currentUserId && !isAdmin)
                throw new UnauthorizedAccessException("Доступ запрещён");

            await _repository.DeleteAsync(entity, ct);
        }

        private static MarkerDTO MapToDTO(MarkerEntity entity)
        {
            return new MarkerDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                CategoryId = entity.CategoryId,
                CategoryName = entity.Category?.Name ?? string.Empty,
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                IsPublic = entity.IsPublic
            };
        }
    }
}
