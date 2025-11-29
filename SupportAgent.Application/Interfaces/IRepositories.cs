using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupportAgent.Domain.Entities;

namespace SupportAgent.Application.Interfaces
{
    public interface IChamadoRepository
    {
        Task<Chamado?> GetByIdAsync(Guid id);
        Task<IEnumerable<Chamado>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Chamado chamado);
        Task UpdateAsync(Chamado chamado);
    }

    public interface IUserAccountRepository
    {
        Task<UserAccount?> GetByIdAsync(Guid id);
        Task AddAsync(UserAccount user);
        Task UpdateAsync(UserAccount user);
    }
}
