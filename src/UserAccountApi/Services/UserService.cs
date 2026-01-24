using Shared.DTOs;
using UserAccountApi.Domain;
using UserAccountApi.Repositories;

namespace UserAccountApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserProfileDto?> GetProfileByPhoneNumberAsync(string phoneNumber)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        if (user == null) return null;

        return MapToProfileDto(user);
    }

    public async Task<(bool Success, string Message, UserProfileDto? Profile)> RegisterUserAsync(CreateUserDto dto)
    {
        // Check for existing phone
        var existingPhone = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
        if (existingPhone != null)
        {
            return (false, "Usuário com este número de telefone já existe.", null);
        }

        // Check for existing CPF
        if (!string.IsNullOrEmpty(dto.Cpf))
        {
            var existingCpf = await _userRepository.GetByCpfAsync(dto.Cpf);
            if (existingCpf != null)
            {
                return (false, "Usuário com este CPF já existe.", null);
            }
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Cpf = dto.Cpf,
            ContactInfo = new ContactInfo
            {
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email
            },
            Address = new Address
            {
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode
            }
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return (true, "Usuário registrado com sucesso.", MapToProfileDto(newUser));
    }

    public async Task<bool> UpdateProfileAsync(UserProfileDto dto)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
        
        if (user == null)
        {
            // Demo logic: create if not exists
            user = new User
            {
                Id = Guid.NewGuid(),
                ContactInfo = new ContactInfo { PhoneNumber = dto.PhoneNumber },
                Address = new Address()
            };
            await _userRepository.AddAsync(user);
        }

        user.Name = dto.Name;
        user.ContactInfo.Email = dto.Email;
        
        // Simple address parsing for demo
        if (!string.IsNullOrEmpty(dto.Address))
        {
            var addressParts = dto.Address.Split(',');
            if (addressParts.Length > 0) user.Address.Street = addressParts[0].Trim();
            if (addressParts.Length > 1) user.Address.City = addressParts[1].Trim();
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        
        return true;
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Cpf = user.Cpf,
            PhoneNumber = user.ContactInfo.PhoneNumber,
            Email = user.ContactInfo.Email,
            Address = $"{user.Address.Street}, {user.Address.City} - {user.Address.State}".Trim(',', ' ', '-')
        };
    }
}
