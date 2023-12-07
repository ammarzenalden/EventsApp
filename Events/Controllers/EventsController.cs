using Events.Data;
using Events.Dto;
using Events.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace Events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }
    private int GetUserId()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        return int.Parse(userId!);
    }
    [HttpGet("GetAllEvents")]
    [AllowAnonymous]
    public async Task<ActionResult> GetEvents([FromQuery] PaginationParams @params) 
    {
        var Events = await _context.Events.OrderBy(p => p.Id).Where(x => x.Available > 0).ToListAsync();
        var pageEvents = Events.Skip((@params.Page - 1) * @params.ItemPerPage).Take(@params.ItemPerPage);
        int countEvents = Events.Count();
        return Ok(new
        {
            success = true,
            Data = pageEvents,
            count = countEvents
        });
    }
    [HttpPost("AddEvent")]
    public async Task<ActionResult> AddEvent(EventDto eventDto)
    {
        int userId = GetUserId();
        Boolean hasNull = false;
        string theMessage = "";
        foreach (PropertyInfo property in eventDto.GetType().GetProperties())
        {
            
            object value = property.GetValue(eventDto)!;
            if (property.Name == "Available")
            {
                _ = int.TryParse(value.ToString(), out int intValue);
                if (intValue < 0)
                {
                    hasNull = true;
                    theMessage = "Available is negative number";
                }
            }
            if (value == null)
            {
                hasNull = true;
                theMessage = $"Property '{property.Name}' is null.";
            }
        }
        if (hasNull)
        {
            return BadRequest(new
            {
                success = true,
                message = theMessage
            });
        };
        Event theEvent = new()
        {
            UserId = userId,
            EventDate = eventDto.EventDate,
            EventName = eventDto.EventName,
            Available = eventDto.Available,
            Description = eventDto.Description,
            Location = eventDto.Location
        };
        _context.Events.Add(theEvent);
        await _context.SaveChangesAsync();
        return Ok(new
        {
            success = true,
            Data = theEvent
        });
    }
    [HttpPut("UpdateEvent")]
    public async Task<ActionResult> UpdateProduct(int id,EventDto eventDto)
    {
        var oldEvent = await _context.Events.FindAsync(id);
        if (oldEvent == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "there is no Event by this id"
            });
        }
        int userId = GetUserId();
        if(userId != oldEvent.UserId)
        {
            return Unauthorized(new
            {
                success = false,
                message = "you are not the owner"
            });
        }
        Boolean hasNull = false;
        string theMessage = "";
        foreach (PropertyInfo property in eventDto.GetType().GetProperties())
        {

            object value = property.GetValue(eventDto)!;
            if (property.Name == "Available")
            {
                _ = int.TryParse(value.ToString(), out int intValue);
                if (intValue < 0)
                {
                    hasNull = true;
                    theMessage = "Available is negative number";
                }
            }
            if (value == null)
            {
                hasNull = true;
                theMessage = $"Property '{property.Name}' is null.";
            }
        }
        if (hasNull)
        {
            return BadRequest(new
            {
                success = true,
                message = theMessage
            });
        };
        Event newEvent = new()
        {
            EventDate = eventDto.EventDate,
            EventName = eventDto.EventName,
            Available = eventDto.Available,
            Description = eventDto.Description,
            Location = eventDto.Location,
            UserId = userId
        };
        
        oldEvent.EventDate = eventDto.EventDate;
        oldEvent.EventName = eventDto.EventName;
        oldEvent.Available = eventDto.Available;
        oldEvent.Description = eventDto.Description;
        oldEvent.Location = eventDto.Location;
        oldEvent.UserId = userId;


        _context.Events.Update(oldEvent);
        await _context.SaveChangesAsync();
        return Ok(new
        {
            success = true,
            data = oldEvent
        });

    }
    [HttpDelete("DeleteEvent")]
    public async Task<ActionResult> DeleteEvent(int id)
    {
        var theEvent = await _context.Events.FindAsync(id);
        if(theEvent == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "there is no Event by this Id"
            });
        }
        if(theEvent.UserId != GetUserId())
        {
            return Unauthorized(new
            {
                success = false,
                message = "you are not the owner"
            });
        }
        _context.Events.Remove(theEvent);
        await _context.SaveChangesAsync();
        return Ok(new
        {
            success = true,
            message = "deleted"
        });
    }


}