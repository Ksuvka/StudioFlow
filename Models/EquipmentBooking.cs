namespace StudioFlow.Models
{
    public class EquipmentBooking
    {
        public int BookingId { get; set; }
        public int EquipmentId { get; set; }
        public int Quantity { get; set; }

        public Booking Booking { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
    }
}