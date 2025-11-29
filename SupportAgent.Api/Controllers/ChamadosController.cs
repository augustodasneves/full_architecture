using Microsoft.AspNetCore.Mvc;
using SupportAgent.Application.Interfaces;
using SupportAgent.Domain.Entities;
using SupportAgent.Domain.Enums;

namespace SupportAgent.Api.Controllers
{
    [ApiController]
    [Route("v1/chamados")]
    public class ChamadosController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public ChamadosController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("{idUsuario}")]
        public async Task<IActionResult> GetTickets(Guid idUsuario)
        {
            var tickets = await _ticketService.GetTicketsByUserAsync(idUsuario);
            return Ok(tickets);
        }

        [HttpGet("{idChamado}/status")]
        public async Task<IActionResult> GetTicketStatus(Guid idChamado)
        {
            var ticket = await _ticketService.GetTicketStatusAsync(idChamado);
            if (ticket == null) return NotFound();
            return Ok(new { ticket.Status, ticket.ApiDeadline, ticket.IsOverdue });
        }

        [HttpPost]
        public async Task<IActionResult> OpenTicket([FromBody] OpenTicketRequest request)
        {
            var ticket = await _ticketService.OpenTicketAsync(request.UserId, request.Category);
            return CreatedAtAction(nameof(GetTicketStatus), new { idChamado = ticket.Id }, ticket);
        }

        [HttpPut("{idChamado}/reabrir")]
        public async Task<IActionResult> ReopenTicket(Guid idChamado)
        {
            try
            {
                await _ticketService.ReopenTicketAsync(idChamado);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPut("{idChamado}/update-info-pendente")]
        public async Task<IActionResult> UpdatePendingInfo(Guid idChamado, [FromBody] UpdatePendingInfoRequest request)
        {
            try
            {
                await _ticketService.UpdatePendingInfoAsync(idChamado, request.EvidenceLink);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }

    public record OpenTicketRequest(Guid UserId, string Category);
    public record UpdatePendingInfoRequest(string EvidenceLink);
}
