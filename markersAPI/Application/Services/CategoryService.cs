using Application.DTOs;
using Infrastructure.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var entities = await _repository.GetAllAsync(ct);
            return entities.Select(MapToDTO);
        }

        public async Task<CategoryDTO> CreateAsync(CategoryDTO dto, CancellationToken ct = default)
        {
            var entity = new CategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            var created = await _repository.AddAsync(entity, ct);
            return MapToDTO(created);
        }

        public async Task<CategoryDTO> UpdateAsync(Guid id, CategoryDTO dto, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Category with id {id} not found");

            entity.Name = dto.Name;

            await _repository.UpdateAsync(entity, ct);
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Category with id {id} not found");

            try
            {
                await _repository.DeleteAsync(entity, ct);
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Нельзя удалить категорию, к которой привязаны маркеры");
            }
        }

        private static CategoryDTO MapToDTO(CategoryEntity entity)
        {
            return new CategoryDTO
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }
}
