namespace Domain.Entities
{
    public class ContentEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Path { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid MarkerId { get; set; }
        public MarkerEntity Marker { get; set; } = null!;



    }
}
