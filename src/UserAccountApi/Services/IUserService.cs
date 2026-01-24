using Shared.DTOs;

namespace UserAccountApi.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileByPhoneNumberAsync(string phoneNumber);
    Task<UserProfileDto?> GetProfileByWhatsAppIdAsync(string whatsappId);
    Task<(bool Success, string Message, UserProfileDto? Profile)> RegisterUserAsync(CreateUserDto dto);
    Task<bool> UpdateProfileAsync(UserProfileDto dto);
}
