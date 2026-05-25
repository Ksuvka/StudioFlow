using System;
using System.Collections.Generic;

namespace StudioFlow.Models
{
    public class Studio
    {
        public int StudioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? Area { get; set; }
        public decimal? CeilingHeight { get; set; }
        public string? WallColor { get; set; }
        public bool HasNaturalLight { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<StudioEquipment> StudioEquipment { get; set; } = new List<StudioEquipment>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
    }
}