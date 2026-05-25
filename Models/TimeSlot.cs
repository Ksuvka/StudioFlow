using System;
using System.Collections.Generic;

namespace StudioFlow.Models
{
    public class TimeSlot
    {
        public int SlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}