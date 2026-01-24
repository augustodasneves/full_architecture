using Microsoft.EntityFrameworkCore;
using UserAccountApi.Data;
using UserAccountApi.Domain;

namespace UserAccountApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ContactInfo.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetByWhatsAppIdAsync(string whatsappId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ContactInfo.WhatsAppId == whatsappId);
    }

    public async Task<User?> GetByCpfAsync(string cpf)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Cpf == cpf);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
