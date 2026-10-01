namespace Application.DTOs
{
    public class ContentDTO
    {
        public Guid Id { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
        public Guid MarkerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
