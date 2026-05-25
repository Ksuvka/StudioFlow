namespace StudioFlow.Models
{
    public class StudioEquipment
    {
        public int StudioId { get; set; }
        public int EquipmentId { get; set; }
        public int Quantity { get; set; }

        public Studio Studio { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
    }
}