using Events.Data;
using Events.Dto;
using Events.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Events.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private int GetUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return int.Parse(userId!);
        }
        [HttpPost("BookTicket")]
        public async Task<ActionResult> BookTicket(TicketDto[] ticketDto)
        {
            List<Ticket> tickets = new();
            foreach(var order in ticketDto)
            {
                var theEvent = await _context.Events.FindAsync(order.EventId);
                if(theEvent == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"there is no Event with this Id:{order.EventId}"
                    });
                }
                if (theEvent.Available < order.NumberOfTickets)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"the requested quantity is not available of product whit id: {order.EventId}"
                    });
                }
                Ticket ticket = new()
                {
                    NumberOfTicket = order.NumberOfTickets,
                    EventId = order.EventId,
                    UserId = GetUserId()
                };
                theEvent.Available -= order.NumberOfTickets;
                _context.Events.Update(theEvent);
                _context.Tickets.Add(ticket);
                tickets.Add(ticket);
            }
            await _context.SaveChangesAsync();
            
            return Ok(new
            {
                success = true,
                data = tickets
            });
        }
        [HttpPut("UpdateTicket")]
        public async Task<ActionResult> UpdateTicket(int id,TicketDto ticketDto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if(ticket == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "there is no Ticket by this Id"
                });
            }
            if(ticket.UserId != GetUserId())
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "you are not the owner of the Ticket"
                });
            }
            var theEvent = await _context.Events.FindAsync(ticketDto.EventId);
            if(ticket.NumberOfTicket < ticketDto.NumberOfTickets)
            {
                int theIncrease = ticketDto.NumberOfTickets - ticket.NumberOfTicket;
                if (theEvent!.Available < theIncrease)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"the requested quantity is not available of product whit id: {ticketDto.EventId}"
                    });
                }
                ticket.NumberOfTicket = ticketDto.NumberOfTickets;
                theEvent.Available -= theIncrease;
                _context.Events.Update(theEvent);
                _context.Tickets.Update(ticket);
            }
            if (ticket.NumberOfTicket > ticketDto.NumberOfTickets)
            {
                int theDecrease = ticket.NumberOfTicket - ticketDto.NumberOfTickets;
                ticket.NumberOfTicket = ticketDto.NumberOfTickets;
                theEvent!.Available += theDecrease;
                _context.Events.Update(theEvent);
                _context.Tickets.Update(ticket);
            }
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                data = ticket
            });

        }
        [HttpGet("GetMyTicket")]
        public async Task<ActionResult> GetMyTicket()
        {
            var tickets = await _context.Tickets.Where(x => x.UserId == GetUserId()).ToListAsync();
            if(tickets == null)
            {
                return Ok(new
                {
                    success = true,
                    data = tickets
                });
            }
            List<Event> theEvents = new();
            foreach(var ticket in tickets)
            {
                var theEvent = await _context.Events.FindAsync(ticket.EventId);
                theEvents.Add(theEvent!);
            }
            return Ok(new
            {
                success = true,
                date = tickets,
                events = theEvents
            });
        }
        [HttpDelete("DeleteTicket")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if(ticket == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "there is no ticket by this Id"
                });
            }
            if(ticket.UserId != GetUserId())
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "you are not the owner of this ticket"
                });
            }
            var theEvent = await _context.Events.FindAsync(ticket.EventId);
            theEvent!.Available += ticket.NumberOfTicket;
            _context.Events.Update(theEvent);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "Deleted"
            });
        }
    }
}
