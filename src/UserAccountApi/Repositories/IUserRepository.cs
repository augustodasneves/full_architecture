using UserAccountApi.Domain;

namespace UserAccountApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByWhatsAppIdAsync(string whatsappId);
    Task<User?> GetByCpfAsync(string cpf);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task SaveChangesAsync();
}
