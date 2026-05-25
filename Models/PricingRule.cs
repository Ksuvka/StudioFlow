using System;

namespace StudioFlow.Models
{
    public class PricingRule
    {
        public int RuleId { get; set; }

        public int? StudioId { get; set; }
        public Studio? Studio { get; set; }

        public int? DayOfWeek { get; set; }

        public TimeOnly? StartTime { get; set; }   
        public TimeOnly? EndTime { get; set; }   

        public decimal Multiplier { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }
}