using Microsoft.EntityFrameworkCore;
using StudioFlow.Models;

namespace StudioFlow.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<StudioEquipment> StudioEquipment { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EquipmentBooking> EquipmentBookings { get; set; }
        public DbSet<PricingRule> PricingRules { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Role>().ToTable("roles");
            modelBuilder.Entity<Studio>().ToTable("studios");
            modelBuilder.Entity<Equipment>().ToTable("equipment");
            modelBuilder.Entity<StudioEquipment>().ToTable("studioequipment");
            modelBuilder.Entity<TimeSlot>().ToTable("timeslots");
            modelBuilder.Entity<Booking>().ToTable("bookings");
            modelBuilder.Entity<EquipmentBooking>().ToTable("equipmentbookings");
            modelBuilder.Entity<PricingRule>().ToTable("pricingrules");

            // Настройка User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasColumnName("passwordhash").IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).HasColumnName("fullname").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.RoleId).HasColumnName("roleid");
                entity.Property(e => e.IsBlocked).HasColumnName("isblocked").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastLoginAt).HasColumnName("lastloginat");
               

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId)
                    .HasConstraintName("users_roleid_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("roleid");
                entity.Property(e => e.RoleName).HasColumnName("rolename").IsRequired().HasMaxLength(50);

                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // Настройка Studio
            modelBuilder.Entity<Studio>(entity =>
            {
                entity.HasKey(e => e.StudioId);
                entity.Property(e => e.StudioId).HasColumnName("studioid");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
                entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
                entity.Property(e => e.Area).HasColumnName("area").HasColumnType("decimal(10,2)");
                entity.Property(e => e.CeilingHeight).HasColumnName("ceilingheight").HasColumnType("decimal(10,2)");
                entity.Property(e => e.WallColor).HasColumnName("wallcolor").HasMaxLength(100);
                entity.Property(e => e.HasNaturalLight).HasColumnName("hasnaturallight").HasDefaultValue(false);
                entity.Property(e => e.BasePrice).HasColumnName("baseprice").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.ImageUrl).HasColumnName("imageurl").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Настройка Equipment
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.EquipmentId);
                entity.Property(e => e.EquipmentId).HasColumnName("equipmentid");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(100);
                entity.Property(e => e.RentalPrice).HasColumnName("rentalprice").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(1);
                entity.Property(e => e.ImageUrl).HasColumnName("imageurl").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
            });

            // Настройка StudioEquipment
            modelBuilder.Entity<StudioEquipment>(entity =>
            {
                entity.HasKey(e => new { e.StudioId, e.EquipmentId });
                entity.Property(e => e.StudioId).HasColumnName("studioid");
                entity.Property(e => e.EquipmentId).HasColumnName("equipmentid");
                entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(1);

                entity.HasOne(e => e.Studio)
                    .WithMany(s => s.StudioEquipment)
                    .HasForeignKey(e => e.StudioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipment)
                    .WithMany(eq => eq.StudioEquipment)
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка TimeSlot
            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.HasKey(e => e.SlotId);
                entity.Property(e => e.SlotId).HasColumnName("slotid");
                entity.Property(e => e.StartTime).HasColumnName("starttime").IsRequired();
                entity.Property(e => e.EndTime).HasColumnName("endtime").IsRequired();
                entity.Property(e => e.DurationMinutes).HasColumnName("durationminutes").IsRequired();
            });

            // Настройка Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingId).HasColumnName("bookingid");
                entity.Property(e => e.UserId).HasColumnName("userid");
                entity.Property(e => e.StudioId).HasColumnName("studioid");
                entity.Property(e => e.BookingDate).HasColumnName("bookingdate");
                entity.Property(e => e.SlotId).HasColumnName("slotid");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(e => e.TotalPrice).HasColumnName("totalprice").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.BookingComment).HasColumnName("bookingcomment").HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ConfirmedAt).HasColumnName("confirmedat");
                entity.Property(e => e.CancelledAt).HasColumnName("cancelledat");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("bookings_userid_fkey")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Studio)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(e => e.StudioId)
                    .HasConstraintName("bookings_studioid_fkey")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TimeSlot)
                    .WithMany(ts => ts.Bookings)
                    .HasForeignKey(e => e.SlotId)
                    .HasConstraintName("bookings_slotid_fkey")
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.StudioId, e.BookingDate, e.SlotId, e.Status })
                    .HasDatabaseName("bookings_studioid_bookingdate_slotid_status_key")
                    .IsUnique();
            });

            // Настройка EquipmentBooking
            modelBuilder.Entity<EquipmentBooking>(entity =>
            {
                entity.HasKey(e => new { e.BookingId, e.EquipmentId });
                entity.Property(e => e.BookingId).HasColumnName("bookingid");
                entity.Property(e => e.EquipmentId).HasColumnName("equipmentid");
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.EquipmentBookings)
                    .HasForeignKey(e => e.BookingId)
                    .HasConstraintName("equipmentbookings_bookingid_fkey")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipment)
                    .WithMany(eq => eq.EquipmentBookings)
                    .HasForeignKey(e => e.EquipmentId)
                    .HasConstraintName("equipmentbookings_equipmentid_fkey")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка PricingRule
            modelBuilder.Entity<PricingRule>(entity =>
            {
                entity.HasKey(e => e.RuleId);
                entity.Property(e => e.RuleId).HasColumnName("ruleid");
                entity.Property(e => e.StudioId).HasColumnName("studioid");
                entity.Property(e => e.DayOfWeek).HasColumnName("dayofweek");
                entity.Property(e => e.StartTime).HasColumnName("starttime");
                entity.Property(e => e.EndTime).HasColumnName("endtime");
                entity.Property(e => e.Multiplier).HasColumnName("multiplier").HasColumnType("decimal(3,2)").HasDefaultValue(1.00);
                entity.Property(e => e.IsActive).HasColumnName("isactive").HasDefaultValue(true);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);

                entity.HasOne(e => e.Studio)
                    .WithMany(s => s.PricingRules)
                    .HasForeignKey(e => e.StudioId)
                    .HasConstraintName("pricingrules_studioid_fkey")
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}