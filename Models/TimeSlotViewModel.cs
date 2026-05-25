namespace StudioFlow.Models
{
    public class TimeSlotViewModel
    {
        public int SlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
        public bool IsSelected { get; set; }
        public string DisplayTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}