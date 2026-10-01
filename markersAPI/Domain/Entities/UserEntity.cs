using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class UserEntity : IdentityUser<Guid>
    {
        public ICollection<MarkerEntity> Markers { get; set; } = new List<MarkerEntity>();
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
