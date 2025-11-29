using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupportAgent.Domain.Entities;
using SupportAgent.Domain.Enums;

namespace SupportAgent.Application.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Chamado>> GetTicketsByUserAsync(Guid userId);
        Task<Chamado?> GetTicketStatusAsync(Guid ticketId);
        Task<Chamado> OpenTicketAsync(Guid userId, string category);
        Task ReopenTicketAsync(Guid ticketId);
        Task UpdatePendingInfoAsync(Guid ticketId, string evidenceLink);
    }
}
