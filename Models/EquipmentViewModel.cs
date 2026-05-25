
namespace StudioFlow.Models
{
    public class EquipmentViewModel
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal RentalPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SelectedQuantity { get; set; }
        public int AvailableQuantity { get; set; } 

       
    }
}