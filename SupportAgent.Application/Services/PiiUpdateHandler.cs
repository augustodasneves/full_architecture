using System;
using System.Threading.Tasks;
using SupportAgent.Application.Interfaces;

namespace SupportAgent.Application.Services
{
    public interface IPiiUpdateHandler
    {
        Task UpdatePiiAsync(Guid userId, string phoneNumber, string address);
    }

    public class PiiUpdateHandler : IPiiUpdateHandler
    {
        private readonly IUserAccountRepository _userRepository;

        public PiiUpdateHandler(IUserAccountRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task UpdatePiiAsync(Guid userId, string phoneNumber, string address)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                // Create user if not exists? Or throw?
                // "User Account API é a fonte da verdade".
                // If user doesn't exist, maybe we create it.
                user = new SupportAgent.Domain.Entities.UserAccount("Unknown", "unknown@example.com"); // Placeholder
                // In reality we might need more info to create.
                // For now, let's assume user exists or we create a stub.
                // Let's throw for now to be safe, or return.
                return; 
            }

            user.UpdatePii(phoneNumber, address);
            await _userRepository.UpdateAsync(user);
        }
    }
}
