//РАБОЧЕЕ

using StudioFlow.Models;
using static StudioFlow.Components.Pages.AdminDashboard;
using static StudioFlow.Components.Pages.Profile;
using static StudioFlow.Services.UserService;

namespace StudioFlow.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UserExistsAsync(string email);
        Task UpdateLastLoginAsync(int userId);
        Task<List<UserBookingViewModel>> GetUserBookingsAsync(int userId);


        Task<bool> CancelConfirmedBookingAsync(int bookingId);
        Task<List<StudioViewModel>> GetAllStudiosAsync();
        Task<StudioViewModel?> GetStudioByIdAsync(int studioId);
        Task<List<TimeSlotViewModel>> GetAvailableTimeSlotsAsync(int studioId, DateTime date);
        Task<decimal> CalculatePriceAsync(int studioId, int slotId, DateTime bookingDate, List<int> equipmentIds);
        Task<bool> CreateBookingAsync(int userId, int studioId, int slotId, DateTime bookingDate, string? comment, List<(int EquipmentId, int Quantity)> equipment);

        Task<List<EquipmentViewModel>> GetAllEquipmentAsync();
        Task<EquipmentViewModel?> GetEquipmentByIdAsync(int equipmentId);

        Task<List<EquipmentViewModel>> GetAvailableEquipmentForTimeAsync(int studioId, DateTime date, List<int> slotIds);

        Task<List<UserBookingFullViewModel>> GetUserBookingsWithEquipmentAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId);

        Task<EditBookingModel?> GetBookingForEditAsync(int bookingId);
        Task<bool> UpdateBookingAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment);
        Task<List<EquipmentViewModel>> GetAvailableEquipmentForBookingAsync(int studioId, DateTime date, int slotId, int excludeBookingId);


        Task<bool> UpdateUserProfileAsync(int userId, object updateData);

        Task<decimal> CalculatePriceWithQuantityAsync(int studioId, int slotId, DateTime bookingDate, List<(int EquipmentId, int Quantity)> equipment);


        Task<List<ManagerBookingViewModel>> GetAllBookingsForManagerAsync();
        Task<List<ClientViewModel>> GetAllClientsAsync();
        Task<bool> ConfirmBookingAsync(int bookingId);
        Task<ManagerBookingViewModel?> GetBookingDetailsForManagerAsync(int bookingId);
        Task<List<PricingRuleViewModel>> GetAllPricingRulesAsync();
        Task<List<ClientViewModel>> GetAllClientsWithStatsAsync();

        Task<bool> CreateStudioAsync(StudioEditModel studio);
        Task<bool> UpdateStudioAsync(StudioEditModel studio);
        Task<bool> DeleteStudioAsync(int studioId);
        Task<List<StudioViewModel>> GetAllStudiosForAdminAsync();

        Task<bool> CreateEquipmentAsync(EquipmentEditModel equipment);
        Task<bool> UpdateEquipmentAsync(EquipmentEditModel equipment);
        Task<bool> DeleteEquipmentByIdAsync(int equipmentId);
        Task<List<EquipmentViewModel>> GetAllEquipmentForAdminAsync();

        Task<bool> CreatePricingRuleAsync(PricingRuleEditModel rule);
        Task<bool> UpdatePricingRuleAsync(PricingRuleEditModel rule);
        Task<bool> DeletePricingRuleAsync(int ruleId);

        Task<bool> CreateUserAsync(string fullName, string email, string password, string phone, string roleName);

        Task<bool> PayForBookingAsync(int bookingId);

        Task<bool> CompleteBookingAsync(int bookingId);

        Task<decimal> GetAdditionalPaymentAmountAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment);
        Task<bool> UpdateBookingWithPaymentAsync(int bookingId, string? comment, List<(int EquipmentId, int Quantity)> equipment, decimal additionalPayment);

        Task<bool> CancelPaidBookingAsync(int bookingId);

        Task<bool> PayForBookingAfterCreationAsync(int bookingId);

        Task<DateTime?> GetBookingStartTimeAsync(int bookingId);
    }
    public class UserBookingViewModel
    {
        public int BookingId { get; set; }
        public string StudioName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string? Comment { get; set; }
    }

    public class UserBookingFullViewModel
    {
        public int BookingId { get; set; }
        public string StudioName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string SlotTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string? Comment { get; set; }
        public List<BookingEquipmentViewModel> EquipmentList { get; set; } = new();
    }

    public class BookingEquipmentViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal RentalPrice { get; set; }
    }
}