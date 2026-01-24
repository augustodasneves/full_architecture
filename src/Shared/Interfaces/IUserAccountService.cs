namespace Shared.Interfaces;

public interface IUserAccountService
{
    Task<bool> UserExistsAsync(string phoneNumber);
    Task<Shared.DTOs.UserProfileDto?> GetUserProfileAsync(string phoneNumber);
    Task<Shared.DTOs.UserProfileDto?> GetUserProfileByWhatsAppIdAsync(string whatsappId);
}
