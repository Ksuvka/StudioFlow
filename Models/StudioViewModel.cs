namespace StudioFlow.Models
{
    public class StudioViewModel
    {
        public int StudioId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public decimal? Area { get; set; }
        public decimal? CeilingHeight { get; set; }
        public string WallColor { get; set; } = "";
        public bool HasNaturalLight { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = "";
        public bool IsActive { get; set; }
    }
}