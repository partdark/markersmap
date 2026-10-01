namespace Domain.Entities
{
    public class CategoryEntity
    {
        public Guid Id { get; set;} = Guid.NewGuid();

        public  string Name { get; set; } = string.Empty;

      public   ICollection<MarkerEntity> Markers { get; set; } = new List<MarkerEntity>();




    }
}
