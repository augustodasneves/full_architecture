using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportAgent.Application.Interfaces;
using SupportAgent.Domain.Entities;

namespace SupportAgent.Infrastructure.Persistence.Repositories
{
    public class ChamadoRepository : IChamadoRepository
    {
        private readonly AppDbContext _context;

        public ChamadoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Chamado?> GetByIdAsync(Guid id)
        {
            return await _context.Chamados.FindAsync(id);
        }

        public async Task<IEnumerable<Chamado>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Chamados
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(Chamado chamado)
        {
            await _context.Chamados.AddAsync(chamado);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Chamado chamado)
        {
            _context.Chamados.Update(chamado);
            await _context.SaveChangesAsync();
        }
    }

    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly AppDbContext _context;

        public UserAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserAccount?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddAsync(UserAccount user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserAccount user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
