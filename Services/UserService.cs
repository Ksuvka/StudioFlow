using System.Data.SqlClient;  
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 
using StudioFlow.Data;
using StudioFlow.Models;
using static StudioFlow.Components.Pages.AdminDashboard;


namespace StudioFlow.Services
{

    public class EditBookingModel
    {
        public int BookingId { get; set; }
        public int StudioId { get; set; }
        public string StudioName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public int SlotId { get; set; }
        public string SlotTime { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OriginalTotalPrice { get; set; }
        public bool IsPaid { get; set; }
        public List<BookingEquipmentItem> SelectedEquipment { get; set; } = new();
    }

    public class BookingEquipmentItem
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal RentalPrice { get; set; }
    }

    public class ManagerBookingViewModel
    {
        public int BookingId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string StudioName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string SlotTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string? Comment { get; set; }
        public List<BookingEquipmentViewModel> EquipmentList { get; set; } = new();
    }

    public class ClientViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
    public class UserService : IUserService
    {


        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task<DateTime?> GetBookingStartTimeAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.TimeSlot)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null) return null;


                var startTimeOfDay = booking.TimeSlot?.StartTime ?? TimeSpan.Zero;

                var startDateTimeUtc = booking.BookingDate.Date.Add(startTimeOfDay);
                var startDateTimeMsk = startDateTimeUtc.AddHours(3);

                Console.WriteLine($"DEBUG: BookingId={bookingId}, StartDateTimeMsk={startDateTimeMsk:yyyy-MM-dd HH:mm:ss}");

                return startDateTimeMsk;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting booking start time: {ex.Message}");
                return null;
            }
        }
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            Console.WriteLine($"AuthenticateAsync: поиск пользователя {email}");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsBlocked);

            if (user == null)
            {
                Console.WriteLine($"Пользователь {email} не найден");
                return null;
            }

            Console.WriteLine($"Пользователь найден, проверяем пароль...");

            bool passwordValid = VerifyPassword(password, user.PasswordHash);
            Console.WriteLine($"Результат проверки пароля: {passwordValid}");

            if (passwordValid)
                return user;

            return null;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                Console.WriteLine($"Проверка пароля: '{password}'");
                Console.WriteLine($"Хеш из БД: {storedHash}");
                bool result = BCrypt.Net.BCrypt.Verify(password, storedHash);
                Console.WriteLine($"Результат BCrypt.Verify: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке пароля: {ex.Message}");
                return false;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserBookingViewModel>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Join(_context.Studios,
                    b => b.StudioId,
                    s => s.StudioId,
                    (b, s) => new UserBookingViewModel
                    {
                        BookingId = b.BookingId,
                        StudioName = s.Name,
                        BookingDate = b.BookingDate,
                        Status = b.Status,
                        TotalPrice = b.TotalPrice,
                        Comment = b.BookingComment
                    })
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings;
        }



        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }


        public async Task<List<StudioViewModel>> GetAllStudiosAsync()
        {
            var studios = await _context.Studios
                .Where(s => s.IsActive == true)
                .Select(s => new StudioViewModel
                {
                    StudioId = s.StudioId,
                    Name = s.Name,
                    Description = s.Description ?? "",
                    Address = s.Address ?? "",
                    Area = s.Area,
                    CeilingHeight = s.CeilingHeight,
                    WallColor = s.WallColor ?? "",
                    HasNaturalLight = s.HasNaturalLight == true,
                    BasePrice = s.BasePrice,
                    ImageUrl = s.ImageUrl ?? "",
                    IsActive = s.IsActive == true
                })
                .ToListAsync();

            return studios;
        }

        public async Task<StudioViewModel?> GetStudioByIdAsync(int studioId)
        {
            var studio = await _context.Studios
                .Where(s => s.StudioId == studioId && (s.IsActive == true))
                .Select(s => new StudioViewModel
                {
                    StudioId = s.StudioId,
                    Name = s.Name,
                    Description = s.Description ?? "",
                    Address = s.Address ?? "",
                    Area = s.Area,
                    CeilingHeight = s.CeilingHeight,
                    WallColor = s.WallColor ?? "",
                    HasNaturalLight = s.HasNaturalLight == true,
                    BasePrice = s.BasePrice,
                    ImageUrl = s.ImageUrl ?? "",
                    IsActive = s.IsActive == true
                })
                .FirstOrDefaultAsync();

            return studio;
        }

        public async Task<List<TimeSlotViewModel>> GetAvailableTimeSlotsAsync(int studioId, DateTime date)
        {
            Console.WriteLine($"GetAvailableTimeSlotsAsync called: studioId={studioId}, date={date:yyyy-MM-dd}");

            var allSlots = await _context.TimeSlots
                .OrderBy(t => t.StartTime)
                .Select(t => new TimeSlotViewModel
                {
                    SlotId = t.SlotId,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    DurationMinutes = t.DurationMinutes,
                    IsAvailable = true,
                    Price = 0
                })
                .ToListAsync();

            Console.WriteLine($"Total slots in DB: {allSlots.Count}");


            var targetDateUtc = date.Date.ToUniversalTime();
            var nextDayUtc = targetDateUtc.AddDays(1);

            var bookedSlots = await _context.Bookings
     .Where(b => b.StudioId == studioId
                 && b.BookingDate >= targetDateUtc
                 && b.BookingDate < nextDayUtc
                 && b.Status != "cancelled"
                 && b.Status != "refunded")
     .Select(b => b.SlotId)
     .ToListAsync();

            Console.WriteLine($"Booked slots count: {bookedSlots.Count}");

            if (bookedSlots.Any())
            {
                Console.WriteLine($"Booked slot IDs: {string.Join(", ", bookedSlots)}");
            }

            foreach (var slot in allSlots)
            {
                slot.IsAvailable = !bookedSlots.Contains(slot.SlotId);
                Console.WriteLine($"Slot {slot.SlotId} ({slot.StartTime:hh\\:mm}-{slot.EndTime:hh\\:mm}) - Available: {slot.IsAvailable}");

                if (slot.IsAvailable)
                {

                    slot.Price = await CalculateSlotPriceAsync(studioId, slot.SlotId, date);
                    Console.WriteLine($"Slot {slot.SlotId} price: {slot.Price}");
                }
                else
                {
                    slot.Price = 0;
                }
            }

            return allSlots;
        }
        private async Task<decimal> CalculateSlotPriceAsync(int studioId, int slotId, DateTime date)
        {
            var studio = await _context.Studios.FindAsync(studioId);
            if (studio == null) return 0;

            var timeSlot = await _context.TimeSlots.FindAsync(slotId);
            if (timeSlot == null) return studio.BasePrice;

            decimal basePrice = studio.BasePrice;
            decimal multiplier = 1.0m;

            int dayOfWeek = (int)date.DayOfWeek;


            var slotStartTime = TimeOnly.FromTimeSpan(timeSlot.StartTime);
            var slotEndTime = TimeOnly.FromTimeSpan(timeSlot.EndTime);

            var pricingRule = await _context.PricingRules
                .Where(p => (p.StudioId == studioId || p.StudioId == null)
                            && (p.DayOfWeek == dayOfWeek || p.DayOfWeek == null)
                            && (p.StartTime == null || p.StartTime <= slotEndTime)
                            && (p.EndTime == null || p.EndTime >= slotStartTime)
                            && (p.IsActive == true))
                .OrderByDescending(p => p.StudioId == studioId ? 1 : 0)
                .ThenByDescending(p => p.StartTime != null ? 1 : 0)
                .FirstOrDefaultAsync();

            if (pricingRule != null)
            {
                multiplier = pricingRule.Multiplier;
                Console.WriteLine($"Pricing rule applied for studio {studioId}, slot {slotId}: multiplier={multiplier}");
            }

            decimal finalPrice = basePrice * multiplier;
            Console.WriteLine($"Slot price: base={basePrice}, multiplier={multiplier}, final={finalPrice}");

            return finalPrice;
        }
        public async Task<decimal> CalculatePriceAsync(int studioId, int slotId, DateTime bookingDate, List<int> equipmentIds)
        {
            var slotPrice = await CalculateSlotPriceAsync(studioId, slotId, bookingDate);

            decimal equipmentPrice = 0;
            if (equipmentIds != null && equipmentIds.Any())
            {
                var equipment = await _context.Equipment
                    .Where(e => equipmentIds.Contains(e.EquipmentId))
                    .ToListAsync();

                equipmentPrice = equipment.Sum(e => e.RentalPrice);
            }

            return slotPrice + equipmentPrice;
        }



        public async Task<decimal> CalculatePriceWithQuantityAsync(int studioId, int slotId, DateTime bookingDate, List<(int EquipmentId, int Quantity)> equipment)
        {
            var slotPrice = await CalculateSlotPriceAsync(studioId, slotId, bookingDate);

            decimal equipmentPrice = 0;
            if (equipment != null && equipment.Any())
            {
                var equipmentIds = equipment.Select(e => e.EquipmentId).ToList();
                var equipmentList = await _context.Equipment
                    .Where(e => equipmentIds.Contains(e.EquipmentId))
                    .ToListAsync();

                foreach (var eq in equipment)
                {
                    var equip = equipmentList.FirstOrDefault(e => e.EquipmentId == eq.EquipmentId);
                    if (equip != null)
                    {
                        equipmentPrice += equip.RentalPrice * eq.Quantity;
                    }
                }
            }

            return slotPrice + equipmentPrice;
        }

        public async Task<bool> CreateBookingAsync(int userId, int studioId, int slotId, DateTime bookingDate, string? comment, List<(int EquipmentId, int Quantity)> equipment)
        {
            try
            {
                Console.WriteLine($"CreateBookingAsync: userId={userId}, studioId={studioId}, slotId={slotId}, date={bookingDate:yyyy-MM-dd}");

                var equipmentIds = equipment.Select(e => e.EquipmentId).ToList();
                var totalPrice = await CalculatePriceAsync(studioId, slotId, bookingDate, equipmentIds);

                var availableEquipment = await GetAvailableEquipmentForTimeAsync(studioId, bookingDate, new List<int> { slotId });

                foreach (var eq in equipment)
                {
                    var eqData = availableEquipment.FirstOrDefault(e => e.EquipmentId == eq.EquipmentId);
                    if (eqData == null || eq.Quantity > eqData.AvailableQuantity)
                    {
                        Console.WriteLine($"Not enough equipment {eq.EquipmentId}");
                        return false;
                    }
                }

                var bookingDateUtc = bookingDate.Date.ToUniversalTime();

                var booking = new Booking
                {
                    UserId = userId,
                    StudioId = studioId,
                    BookingDate = bookingDateUtc,
                    SlotId = slotId,
                    Status = "paid",
                    TotalPrice = totalPrice,
                    BookingComment = comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created booking with ID: {booking.BookingId}");

                foreach (var eq in equipment)
                {
                    var equipmentBooking = new EquipmentBooking
                    {
                        BookingId = booking.BookingId,
                        EquipmentId = eq.EquipmentId,
                        Quantity = eq.Quantity
                    };
                    _context.EquipmentBookings.Add(equipmentBooking);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Booking created successfully with status PAID");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PayForBookingAfterCreationAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;


                if (booking.Status == "paid") return true;

                if (booking.Status != "pending") return false;

                booking.Status = "paid";
                await _context.SaveChangesAsync();

                Console.WriteLine($"Booking {bookingId} paid successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error paying for booking: {ex.Message}");
                return false;
            }
        }


        public async Task<List<EquipmentViewModel>> GetAllEquipmentAsync()
        {
            var equipment = await _context.Equipment
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.Category)
                .ThenBy(e => e.Name)
                .Select(e => new EquipmentViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Description = e.Description ?? "",
                    Category = e.Category ?? "",
                    RentalPrice = e.RentalPrice,
                    Quantity = e.Quantity,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive == true
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<EquipmentViewModel?> GetEquipmentByIdAsync(int equipmentId)
        {
            var equipment = await _context.Equipment
                .Where(e => e.EquipmentId == equipmentId && (e.IsActive == true))
                .Select(e => new EquipmentViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Description = e.Description ?? "",
                    Category = e.Category ?? "",
                    RentalPrice = e.RentalPrice,
                    Quantity = e.Quantity,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive == true
                })
                .FirstOrDefaultAsync();

            return equipment;
        }


        public async Task<List<EquipmentViewModel>> GetAvailableEquipmentForTimeAsync(int studioId, DateTime date, List<int> slotIds)
        {
            Console.WriteLine($"GetAvailableEquipmentForTimeAsync: studioId={studioId}, date={date:yyyy-MM-dd}, slots={string.Join(",", slotIds)}");

            var allEquipment = await _context.Equipment
                .Where(e => e.IsActive == true)
                .Select(e => new EquipmentViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Description = e.Description ?? "",
                    Category = e.Category ?? "",
                    RentalPrice = e.RentalPrice,
                    Quantity = e.Quantity,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive == true,
                    AvailableQuantity = e.Quantity
                })
                .ToListAsync();

            if (!slotIds.Any())
            {
                foreach (var eq in allEquipment)
                {
                    eq.AvailableQuantity = eq.Quantity;
                }
                return allEquipment;
            }

            var targetDateUtc = date.Date.ToUniversalTime();


            var bookingsOnDateTime = await _context.Bookings
      .Where(b => b.BookingDate.Date == targetDateUtc.Date
                  && slotIds.Contains(b.SlotId)
                  && b.Status != "cancelled"
                  && b.Status != "refunded")
      .ToListAsync();

            Console.WriteLine($"Found {bookingsOnDateTime.Count} bookings for these slots (all studios)");

            var bookingIds = bookingsOnDateTime.Select(b => b.BookingId).ToList();

            var bookedEquipment = new Dictionary<int, int>();

            if (bookingIds.Any())
            {
                var equipmentBookings = await _context.EquipmentBookings
                    .Where(eb => bookingIds.Contains(eb.BookingId))
                    .ToListAsync();

                Console.WriteLine($"Found {equipmentBookings.Count} equipment bookings");

                bookedEquipment = equipmentBookings
                    .GroupBy(eb => eb.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Sum(eb => eb.Quantity));
            }

            foreach (var equipment in allEquipment)
            {
                var bookedQty = bookedEquipment.ContainsKey(equipment.EquipmentId)
                    ? bookedEquipment[equipment.EquipmentId]
                    : 0;

                equipment.AvailableQuantity = equipment.Quantity - bookedQty;
                equipment.AvailableQuantity = Math.Max(0, equipment.AvailableQuantity);

                Console.WriteLine($"Equipment {equipment.Name}: Total={equipment.Quantity}, Booked={bookedQty}, Available={equipment.AvailableQuantity}");
            }

            return allEquipment;
        }


        public async Task<List<UserBookingFullViewModel>> GetUserBookingsWithEquipmentAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.EquipmentBookings)
                    .ThenInclude(eb => eb.Equipment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var result = new List<UserBookingFullViewModel>();

            foreach (var b in bookings)
            {
                var timeSlot = await _context.TimeSlots.FindAsync(b.SlotId);
                var studio = await _context.Studios.FindAsync(b.StudioId);

                result.Add(new UserBookingFullViewModel
                {
                    BookingId = b.BookingId,
                    StudioName = studio?.Name ?? "",
                    BookingDate = b.BookingDate,
                    SlotTime = timeSlot != null ? $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}" : "",
                    Status = b.Status,
                    TotalPrice = b.TotalPrice,
                    Comment = b.BookingComment,
                    EquipmentList = b.EquipmentBookings.Select(eb => new BookingEquipmentViewModel
                    {
                        Name = eb.Equipment?.Name ?? "",
                        Quantity = eb.Quantity,
                        RentalPrice = eb.Equipment?.RentalPrice ?? 0
                    }).ToList()
                });
            }

            return result;
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.Status != "pending")
                return false;

            booking.Status = "cancelled";
            await _context.SaveChangesAsync();
            return true;
        }





        public async Task<EditBookingModel?> GetBookingForEditAsync(int bookingId)
        {

            var booking = await _context.Bookings
                .Include(b => b.EquipmentBookings)
                    .ThenInclude(eb => eb.Equipment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && (b.Status == "pending" || b.Status == "paid"));

            if (booking == null) return null;

            var studio = await _context.Studios.FindAsync(booking.StudioId);
            var timeSlot = await _context.TimeSlots.FindAsync(booking.SlotId);

            var selectedEquipment = new List<BookingEquipmentItem>();

            var availableEquipment = await GetAvailableEquipmentForBookingAsync(
                booking.StudioId,
                booking.BookingDate,
                booking.SlotId,
                bookingId);

            foreach (var eb in booking.EquipmentBookings)
            {
                var available = availableEquipment.FirstOrDefault(e => e.EquipmentId == eb.EquipmentId);
                selectedEquipment.Add(new BookingEquipmentItem
                {
                    EquipmentId = eb.EquipmentId,
                    Name = eb.Equipment?.Name ?? "",
                    Quantity = eb.Quantity,
                    MaxQuantity = available?.AvailableQuantity + eb.Quantity ?? eb.Quantity,
                    RentalPrice = eb.Equipment?.RentalPrice ?? 0
                });
            }

            return new EditBookingModel
            {
                BookingId = booking.BookingId,
                StudioId = booking.StudioId,
                StudioName = studio?.Name ?? "",
                BookingDate = booking.BookingDate,
                SlotId = booking.SlotId,
                SlotTime = timeSlot != null ? $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}" : "",
                Comment = booking.BookingComment,
                TotalPrice = booking.TotalPrice,
                SelectedEquipment = selectedEquipment,
                OriginalTotalPrice = booking.TotalPrice,
                IsPaid = booking.Status == "paid"
            };
        }



        public async Task<decimal> GetAdditionalPaymentAmountAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment)
        {
            var booking = await _context.Bookings
                .Include(b => b.EquipmentBookings)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return 0;


            var newTotalPrice = await CalculatePriceWithQuantityAsync(booking.StudioId, booking.SlotId, booking.BookingDate, equipment);

            return newTotalPrice - booking.TotalPrice;
        }

        public async Task<bool> UpdateBookingWithPaymentAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment, decimal additionalPayment)
        {
            var booking = await _context.Bookings
                .Include(b => b.EquipmentBookings)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return false;


            decimal oldTotalPrice = booking.TotalPrice;


            var newTotalPrice = await CalculatePriceWithQuantityAsync(booking.StudioId, booking.SlotId, booking.BookingDate, equipment);


            booking.BookingComment = comment;


            _context.EquipmentBookings.RemoveRange(booking.EquipmentBookings);
            foreach (var eq in equipment.Where(e => e.Quantity > 0))
            {
                _context.EquipmentBookings.Add(new EquipmentBooking
                {
                    BookingId = booking.BookingId,
                    EquipmentId = eq.EquipmentId,
                    Quantity = eq.Quantity
                });
            }

            booking.TotalPrice = newTotalPrice;


            await _context.SaveChangesAsync();

            Console.WriteLine($"Booking {bookingId} updated: old price={oldTotalPrice}, new price={newTotalPrice}, additional payment={additionalPayment}");
            return true;
        }


        public async Task<bool> UpdateBookingAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment)
        {
            var booking = await _context.Bookings
                .Include(b => b.EquipmentBookings)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return false;


            var currentEquipment = booking.EquipmentBookings
                .GroupBy(eb => eb.EquipmentId)
                .ToDictionary(g => g.Key, g => g.Sum(eb => eb.Quantity));

            var availableEquipment = await GetAvailableEquipmentForBookingAsync(
                booking.StudioId,
                booking.BookingDate,
                booking.SlotId,
                bookingId);

            foreach (var eq in equipment)
            {
                int currentQty = currentEquipment.ContainsKey(eq.EquipmentId) ? currentEquipment[eq.EquipmentId] : 0;
                int newQty = eq.Quantity;


                if (newQty > currentQty)
                {
                    int addedQty = newQty - currentQty;
                    var available = availableEquipment.FirstOrDefault(e => e.EquipmentId == eq.EquipmentId);

                    if (available == null || addedQty > available.AvailableQuantity)
                    {
                        Console.WriteLine($"Cannot add {addedQty} of equipment {eq.EquipmentId}. Only {available?.AvailableQuantity ?? 0} available");
                        return false;
                    }
                }
            }



            booking.BookingComment = comment;

            _context.EquipmentBookings.RemoveRange(booking.EquipmentBookings);


            foreach (var eq in equipment.Where(e => e.Quantity > 0))
            {
                _context.EquipmentBookings.Add(new EquipmentBooking
                {
                    BookingId = booking.BookingId,
                    EquipmentId = eq.EquipmentId,
                    Quantity = eq.Quantity
                });
            }


            var totalPrice = await CalculatePriceWithQuantityAsync(booking.StudioId, booking.SlotId, booking.BookingDate, equipment);
            booking.TotalPrice = totalPrice;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<EquipmentViewModel>> GetAvailableEquipmentForBookingAsync(int studioId, DateTime date, int slotId, int excludeBookingId)
        {
            var allEquipment = await _context.Equipment
                .Where(e => e.IsActive == true)
                .Select(e => new EquipmentViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Description = e.Description ?? "",
                    Category = e.Category ?? "",
                    RentalPrice = e.RentalPrice,
                    Quantity = e.Quantity,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive == true,
                    AvailableQuantity = 0
                })
                .ToListAsync();

            var targetDateUtc = date.Date.ToUniversalTime();


            var allBookingsThisSlot = await _context.Bookings
     .Where(b => b.BookingDate.Date == targetDateUtc.Date
                 && b.SlotId == slotId
                 && b.Status != "cancelled"
                 && b.Status != "refunded"
                 && b.BookingId != excludeBookingId)
     .Include(b => b.EquipmentBookings)
     .ToListAsync();


            var totalBookedByOthers = new Dictionary<int, int>();
            foreach (var booking in allBookingsThisSlot)
            {
                foreach (var eb in booking.EquipmentBookings)
                {
                    if (!totalBookedByOthers.ContainsKey(eb.EquipmentId))
                        totalBookedByOthers[eb.EquipmentId] = 0;
                    totalBookedByOthers[eb.EquipmentId] += eb.Quantity;
                }
            }


            var currentBookingEquipment = new Dictionary<int, int>();
            var currentBooking = await _context.Bookings
                .Include(b => b.EquipmentBookings)
                .FirstOrDefaultAsync(b => b.BookingId == excludeBookingId);

            if (currentBooking != null)
            {
                foreach (var eb in currentBooking.EquipmentBookings)
                {
                    currentBookingEquipment[eb.EquipmentId] = eb.Quantity;
                }
            }

            foreach (var equipment in allEquipment)
            {
                int totalQuantity = equipment.Quantity;
                int bookedByOthers = totalBookedByOthers.ContainsKey(equipment.EquipmentId)
                    ? totalBookedByOthers[equipment.EquipmentId]
                    : 0;
                int currentQty = currentBookingEquipment.ContainsKey(equipment.EquipmentId)
                    ? currentBookingEquipment[equipment.EquipmentId]
                    : 0;

                int occupiedByOthers = bookedByOthers;

                int freeTotal = totalQuantity - occupiedByOthers;

                int availableToAdd = freeTotal - currentQty;


                equipment.AvailableQuantity = Math.Max(0, availableToAdd);

                Console.WriteLine($"Всего на складе: {totalQuantity}");
                Console.WriteLine($"Уже в этом бронировании: {currentQty}");
                Console.WriteLine($"Забронировано другими: {bookedByOthers}");
                Console.WriteLine($"Занято всего (другими): {occupiedByOthers}");
                Console.WriteLine($"Свободно всего (включая текущее брони): {freeTotal}");
                Console.WriteLine($"Можно добавить в это бронирование: {equipment.AvailableQuantity}");
                Console.WriteLine($"Максимум в этом бронировании может быть: {currentQty + equipment.AvailableQuantity}");
            }

            return allEquipment;
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, object updateData)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var properties = updateData.GetType().GetProperties();

                string? fullName = null;
                string? phone = null;
                string? email = null;

                foreach (var prop in properties)
                {
                    switch (prop.Name)
                    {
                        case "FullName":
                            fullName = prop.GetValue(updateData)?.ToString();
                            break;
                        case "Phone":
                            phone = prop.GetValue(updateData)?.ToString();
                            break;
                        case "Email":
                            email = prop.GetValue(updateData)?.ToString();
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(fullName))
                {
                    user.FullName = fullName;
                }

                if (phone != null)
                {
                    user.Phone = phone;
                }

                if (!string.IsNullOrEmpty(email) && email != user.Email)
                {
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != userId);
                    if (emailExists) return false;
                    user.Email = email;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
              
                return false;
            }
        }

        public async Task<List<ManagerBookingViewModel>> GetAllBookingsForManagerAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Studio)
                .Include(b => b.TimeSlot)
                .Include(b => b.EquipmentBookings)
                    .ThenInclude(eb => eb.Equipment)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var result = new List<ManagerBookingViewModel>();
            foreach (var b in bookings)
            {
                result.Add(new ManagerBookingViewModel
                {
                    BookingId = b.BookingId,
                    ClientName = b.User?.FullName ?? "Неизвестно",
                    ClientEmail = b.User?.Email ?? "",
                    StudioName = b.Studio?.Name ?? "",
                    BookingDate = b.BookingDate,
                    SlotTime = b.TimeSlot != null ? $"{b.TimeSlot.StartTime:hh\\:mm} - {b.TimeSlot.EndTime:hh\\:mm}" : "",
                    Status = b.Status,
                    TotalPrice = b.TotalPrice,
                    Comment = b.BookingComment,
                    EquipmentList = b.EquipmentBookings.Select(eb => new BookingEquipmentViewModel
                    {
                        Name = eb.Equipment?.Name ?? "",
                        Quantity = eb.Quantity,
                        RentalPrice = eb.Equipment?.RentalPrice ?? 0
                    }).ToList()
                });
            }
            return result;
        }

        public async Task<List<ClientViewModel>> GetAllClientsAsync()
        {
            var clients = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Client")
                .ToListAsync();

            return clients.Select(c => new ClientViewModel
            {
                UserId = c.UserId,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone ?? "",
                CreatedAt = c.CreatedAt,
                RoleName = c.Role.RoleName,
                TotalBookings = _context.Bookings.Count(b => b.UserId == c.UserId)
            }).ToList();
        }

        public async Task<bool> ConfirmBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return false;


            if (booking.Status != "paid") return false;

            booking.Status = "confirmed";
            booking.ConfirmedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.Status != "confirmed") return false;

            booking.Status = "completed";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ManagerBookingViewModel?> GetBookingDetailsForManagerAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Studio)
                .Include(b => b.TimeSlot)
                .Include(b => b.EquipmentBookings)
                    .ThenInclude(eb => eb.Equipment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return null;

            return new ManagerBookingViewModel
            {
                BookingId = booking.BookingId,
                ClientName = booking.User?.FullName ?? "",
                ClientEmail = booking.User?.Email ?? "",
                StudioName = booking.Studio?.Name ?? "",
                BookingDate = booking.BookingDate,
                SlotTime = booking.TimeSlot != null ? $"{booking.TimeSlot.StartTime:hh\\:mm} - {booking.TimeSlot.EndTime:hh\\:mm}" : "",
                Status = booking.Status,
                TotalPrice = booking.TotalPrice,
                Comment = booking.BookingComment,
                EquipmentList = booking.EquipmentBookings.Select(eb => new BookingEquipmentViewModel
                {
                    Name = eb.Equipment?.Name ?? "",
                    Quantity = eb.Quantity,
                    RentalPrice = eb.Equipment?.RentalPrice ?? 0
                }).ToList()
            };
        }


        public async Task<List<PricingRuleViewModel>> GetAllPricingRulesAsync()
        {
            var rules = await _context.PricingRules
                .Include(p => p.Studio)
                .ToListAsync();

            return rules.Select(r => new PricingRuleViewModel
            {
                RuleId = r.RuleId,
                StudioId = r.StudioId,
                StudioName = r.Studio?.Name,

                DayOfWeek = r.DayOfWeek,
                DayOfWeekName = r.DayOfWeek.HasValue
                    ? GetDayOfWeekName(r.DayOfWeek.Value)
                    : null,

                StartTime = r.StartTime?.ToString("HH:mm"),
                EndTime = r.EndTime?.ToString("HH:mm"),

                TimeRange = r.StartTime.HasValue && r.EndTime.HasValue
                    ? $"{r.StartTime.Value:HH\\:mm} - {r.EndTime.Value:HH\\:mm}"
                    : null,

                Multiplier = r.Multiplier,
                IsActive = r.IsActive,
                Description = r.Description
            }).ToList();
        }

        public async Task<List<ClientViewModel>> GetAllClientsWithStatsAsync()
        {
            var clients = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Client")
                .ToListAsync();

            var result = new List<ClientViewModel>();
            foreach (var c in clients)
            {
                var bookings = await _context.Bookings.Where(b => b.UserId == c.UserId).ToListAsync();
                result.Add(new ClientViewModel
                {
                    UserId = c.UserId,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone ?? "",
                    CreatedAt = c.CreatedAt,
                    RoleName = c.Role.RoleName,
                    TotalBookings = bookings.Count,
                    TotalSpent = bookings.Where(b => b.Status == "paid" || b.Status == "completed").Sum(b => b.TotalPrice)
                });
            }
            return result;
        }

        private string GetDayOfWeekName(int dayOfWeek)
        {
            var days = new[] { "Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
            return days[dayOfWeek];
        }


        public async Task<bool> CreateStudioAsync(StudioEditModel studio)
        {
            try
            {
                var newStudio = new Studio
                {
                    Name = studio.Name,
                    Description = studio.Description ?? "",
                    Address = studio.Address ?? "",
                    Area = studio.Area,
                    CeilingHeight = studio.CeilingHeight,
                    WallColor = studio.WallColor,
                    HasNaturalLight = studio.HasNaturalLight,
                    BasePrice = studio.BasePrice,
                    ImageUrl = studio.ImageUrl,
                    IsActive = studio.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Studios.Add(newStudio);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating studio: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStudioAsync(StudioEditModel studio)
        {
            try
            {
                var existingStudio = await _context.Studios.FindAsync(studio.StudioId);
                if (existingStudio == null) return false;

                existingStudio.Name = studio.Name;
                existingStudio.Description = studio.Description ?? "";
                existingStudio.Address = studio.Address ?? "";
                existingStudio.Area = studio.Area;
                existingStudio.CeilingHeight = studio.CeilingHeight;
                existingStudio.WallColor = studio.WallColor;
                existingStudio.HasNaturalLight = studio.HasNaturalLight;
                existingStudio.BasePrice = studio.BasePrice;
                existingStudio.ImageUrl = studio.ImageUrl;
                existingStudio.IsActive = studio.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating studio: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteStudioAsync(int studioId)
        {
            try
            {
                var studio = await _context.Studios.FindAsync(studioId);
                if (studio == null) return false;

                var hasBookings = await _context.Bookings.AnyAsync(b => b.StudioId == studioId);
                if (hasBookings)
                {

                    studio.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                _context.Studios.Remove(studio);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting studio: {ex.Message}");
                return false;
            }
        }
        public async Task<List<StudioViewModel>> GetAllStudiosForAdminAsync()
        {
            var studios = await _context.Studios
                .Select(s => new StudioViewModel
                {
                    StudioId = s.StudioId,
                    Name = s.Name,
                    Description = s.Description ?? "",
                    Address = s.Address ?? "",
                    Area = s.Area,
                    CeilingHeight = s.CeilingHeight,
                    WallColor = s.WallColor ?? "",
                    HasNaturalLight = s.HasNaturalLight == true,
                    BasePrice = s.BasePrice,
                    ImageUrl = s.ImageUrl ?? "",
                    IsActive = s.IsActive == true
                })
                .ToListAsync();

            return studios;
        }

        public async Task<bool> CreateEquipmentAsync(EquipmentEditModel equipment)
        {
            try
            {
                var newEquipment = new Equipment
                {
                    Name = equipment.Name,
                    Description = equipment.Description ?? "",
                    Category = equipment.Category,
                    RentalPrice = equipment.RentalPrice,
                    Quantity = equipment.Quantity,
                    ImageUrl = equipment.ImageUrl,
                    IsActive = equipment.IsActive
                };

                _context.Equipment.Add(newEquipment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating equipment: {ex.Message}");
                return false;
            }


        }
        public async Task<bool> UpdateEquipmentAsync(EquipmentEditModel equipment)
        {
            try
            {
                var existingEquipment = await _context.Equipment.FindAsync(equipment.EquipmentId);
                if (existingEquipment == null) return false;

                existingEquipment.Name = equipment.Name;
                existingEquipment.Description = equipment.Description ?? "";
                existingEquipment.Category = equipment.Category;
                existingEquipment.RentalPrice = equipment.RentalPrice;
                existingEquipment.Quantity = equipment.Quantity;
                existingEquipment.ImageUrl = equipment.ImageUrl;
                existingEquipment.IsActive = equipment.IsActive;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating equipment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEquipmentByIdAsync(int equipmentId)
        {
            try
            {
                var equipment = await _context.Equipment.FindAsync(equipmentId);
                if (equipment == null) return false;


                var hasBookings = await _context.EquipmentBookings.AnyAsync(eb => eb.EquipmentId == equipmentId);
                if (hasBookings)
                {

                    equipment.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting equipment: {ex.Message}");
                return false;
            }
        }

        public async Task<List<EquipmentViewModel>> GetAllEquipmentForAdminAsync()
        {
            var equipment = await _context.Equipment
                .OrderBy(e => e.Category)
                .ThenBy(e => e.Name)
                .Select(e => new EquipmentViewModel
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Description = e.Description ?? "",
                    Category = e.Category ?? "",
                    RentalPrice = e.RentalPrice,
                    Quantity = e.Quantity,
                    ImageUrl = e.ImageUrl,
                    IsActive = e.IsActive == true
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<bool> CreatePricingRuleAsync(PricingRuleEditModel rule)
        {
            try
            {
                var newRule = new PricingRule
                {
                    StudioId = rule.StudioId,
                    DayOfWeek = rule.DayOfWeek,

                    StartTime = !string.IsNullOrEmpty(rule.StartTime)
                        ? TimeOnly.Parse(rule.StartTime)
                        : null,

                    EndTime = !string.IsNullOrEmpty(rule.EndTime)
                        ? TimeOnly.Parse(rule.EndTime)
                        : null,

                    Multiplier = rule.Multiplier,
                    IsActive = rule.IsActive,
                    Description = rule.Description
                };

                _context.PricingRules.Add(newRule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating pricing rule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdatePricingRuleAsync(PricingRuleEditModel rule)
        {
            try
            {
                var existingRule = await _context.PricingRules.FindAsync(rule.RuleId);
                if (existingRule == null) return false;

                existingRule.StudioId = rule.StudioId;
                existingRule.DayOfWeek = rule.DayOfWeek;

                existingRule.StartTime = !string.IsNullOrEmpty(rule.StartTime)
                    ? TimeOnly.Parse(rule.StartTime)
                    : null;

                existingRule.EndTime = !string.IsNullOrEmpty(rule.EndTime)
                    ? TimeOnly.Parse(rule.EndTime)
                    : null;

                existingRule.Multiplier = rule.Multiplier;
                existingRule.IsActive = rule.IsActive;
                existingRule.Description = rule.Description;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating pricing rule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePricingRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _context.PricingRules.FindAsync(ruleId);
                if (rule == null) return false;

                _context.PricingRules.Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting pricing rule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateUserAsync(string fullName, string email, string password, string phone, string roleName)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {

                    role = new Role { RoleName = "Client" };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }

                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    Phone = phone ?? "",
                    RoleId = role.RoleId,
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PayForBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;


                if (booking.Status != "pending") return false;


                booking.Status = "paid";
                await _context.SaveChangesAsync();

                Console.WriteLine($"Booking {bookingId} paid successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error paying for booking: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CancelPaidBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return false;

            if (booking.Status != "paid" && booking.Status != "confirmed") return false;

            booking.Status = "cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Booking {bookingId} cancelled and refunded. Amount: {booking.TotalPrice}");

            return true;
        }
       
        public async Task<bool> CancelConfirmedBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return false;

            if (booking.Status != "confirmed") return false;

            booking.Status = "cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Confirmed booking {bookingId} cancelled");

            return true;
        }
    }
}