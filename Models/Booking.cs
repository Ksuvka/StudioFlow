using System;
using System.Collections.Generic;

namespace StudioFlow.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int StudioId { get; set; }
        public DateTime BookingDate { get; set; }
        public int SlotId { get; set; }
        public string Status { get; set; } = "pending";
        public decimal TotalPrice { get; set; }
        public string? BookingComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public User User { get; set; } = null!;
        public Studio Studio { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
        public ICollection<EquipmentBooking> EquipmentBookings { get; set; } = new List<EquipmentBooking>();
    }
}