using System.Collections.Generic;

namespace StudioFlow.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal RentalPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public ICollection<StudioEquipment> StudioEquipment { get; set; } = new List<StudioEquipment>();
        public ICollection<EquipmentBooking> EquipmentBookings { get; set; } = new List<EquipmentBooking>();
    }
}