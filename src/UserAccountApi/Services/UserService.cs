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
        _logger.LogInformation("Searching profile for Phone: {PhoneNumber}", phoneNumber);
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        if (user == null) 
        {
            _logger.LogWarning("User not found in database for Phone: {PhoneNumber}", phoneNumber);
            return null;
        }

        _logger.LogInformation("User found: {Name} for Phone: {PhoneNumber}", user.Name, phoneNumber);
        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto?> GetProfileByWhatsAppIdAsync(string whatsappId)
    {
        _logger.LogInformation("Searching profile for WhatsApp ID: {WhatsAppId}", whatsappId);
        var user = await _userRepository.GetByWhatsAppIdAsync(whatsappId);
        if (user == null)
        {
            _logger.LogWarning("User not found in database for WhatsApp ID: {WhatsAppId}", whatsappId);
            return null;
        }

        _logger.LogInformation("User found: {Name} for WhatsApp ID: {WhatsAppId}", user.Name, whatsappId);
        return MapToProfileDto(user);
    }

    public async Task<(bool Success, string Message, UserProfileDto? Profile)> RegisterUserAsync(CreateUserDto dto)
    {
        // Check for existing WhatsApp ID if provided
        if (!string.IsNullOrEmpty(dto.WhatsAppId))
        {
            var existingWa = await _userRepository.GetByWhatsAppIdAsync(dto.WhatsAppId);
            if (existingWa != null)
            {
                return (false, "Usuário com este ID do WhatsApp já existe.", null);
            }
        }

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
                Email = dto.Email,
                WhatsAppId = dto.WhatsAppId
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
        User? user = null;
        bool isNewUser = false;

        if (!string.IsNullOrEmpty(dto.WhatsAppId))
        {
            user = await _userRepository.GetByWhatsAppIdAsync(dto.WhatsAppId);
        }

        if (user == null && !string.IsNullOrEmpty(dto.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
        }
        
        if (user == null)
        {
            _logger.LogInformation("Updating profile for unknown user. Creating new User record for {Jid}", dto.WhatsAppId);
            isNewUser = true;
            user = new User
            {
                Id = Guid.NewGuid(),
                ContactInfo = new ContactInfo 
                { 
                    PhoneNumber = dto.PhoneNumber,
                    WhatsAppId = dto.WhatsAppId 
                },
                Address = new Address()
            };
        }

        user.Name = dto.Name;
        user.ContactInfo.Email = dto.Email;
        user.ContactInfo.WhatsAppId = string.IsNullOrEmpty(dto.WhatsAppId) ? user.ContactInfo.WhatsAppId : dto.WhatsAppId;
        user.ContactInfo.PhoneNumber = string.IsNullOrEmpty(dto.PhoneNumber) ? user.ContactInfo.PhoneNumber : dto.PhoneNumber;
        
        if (!string.IsNullOrEmpty(dto.Address))
        {
            var addressParts = dto.Address.Split(',');
            if (addressParts.Length > 0) user.Address.Street = addressParts[0].Trim();
            if (addressParts.Length > 1) user.Address.City = addressParts[1].Trim();
        }

        if (isNewUser)
        {
            await _userRepository.AddAsync(user);
        }
        else
        {
            // For existing tracked entities, we don't strictly need to call Update()
            // but we can call it to be explicit if the implementation allows.
            // However, the current UserRepository implementation uses _context.Update() 
            // which can be problematic if the entity is already tracked or if it's new.
            // Since it's already tracked (fetched via Repository), EF will detect changes automatically.
        }

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
            WhatsAppId = user.ContactInfo.WhatsAppId,
            Address = $"{user.Address.Street}, {user.Address.City} - {user.Address.State}".Trim(',', ' ', '-')
        };
    }
}
