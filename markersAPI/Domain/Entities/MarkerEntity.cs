using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class MarkerEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(-90,90)]
        public double? Latitude { get; set; } = null!;

        [Range(-180, 180)]
        public double? Longitude { get; set; } = null!;

        public Guid CategoryId { get; set; }

        public CategoryEntity Category { get; set; } = null!;

        public Guid UserId { get; set; }

        public UserEntity User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPublic { get; set; } = false;

       public ICollection<ContentEntity> Content { get; set; } = new List<ContentEntity>();
      



    }
}
