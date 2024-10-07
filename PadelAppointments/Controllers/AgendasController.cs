using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Requests;
using PadelAppointments.Models.Responses;
using PadelAppointments.Services;
using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.User)]
    public class AgendasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserResolver _userResolver;

        public AgendasController(ApplicationDbContext dbContext, UserResolver userResolver)
        {
            _dbContext = dbContext;
            _userResolver = userResolver;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AgendasResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var agendas = await _dbContext.Agendas
                .AsNoTracking()
                .Where(a => a.OrganizationId == _userResolver.OrganizationId)
                .Select(a => new AgendasResponse() 
                { 
                    Id = a.Id, 
                    Name = a.Name,
                    StartsAt = a.StartsAt,
                    EndsAt = a.EndsAt,
                    Interval = a.Interval,
                })
                .ToListAsync();

            return Ok(agendas);
        }

        [HttpGet("{agendaId}/schedules")]
        [ProducesResponseType(typeof(List<ScheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSchedules([FromRoute] int agendaId, [FromQuery][Required] DateOnly date)
        {
            var agenda = await _dbContext.Agendas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == agendaId && a.OrganizationId == _userResolver.OrganizationId);

            if (agenda is null)
            {
                return NotFound("No agenda found");
            }

            var schedules = await _dbContext.Schedules
                .AsNoTracking()
                .Include(s => s.Appointment)
                .ThenInclude(a => a.Checks)
                .ThenInclude(c => c.ItemsConsumed)
                .Where(s => s.Appointment.AgendaId == agendaId && s.Date == date)
                .Select(s => new ScheduleResponse()
                {
                    Id = s.Id,
                    Date = s.Date,
                    Time = s.Time,
                    Appointment = new AppointmentResponse()
                    {
                        Id = s.Appointment.Id,
                        Date = s.Appointment.Date,
                        CustomerName = s.Appointment.CustomerName,
                        CustomerPhoneNumber = s.Appointment.CustomerPhoneNumber,
                        Price = s.Appointment.Price,
                        HasRecurrence = s.Appointment.HasRecurrence,
                        AgendaId = s.Appointment.AgendaId,
                        Check = s.Appointment.Checks
                            .Select(c => new CheckResponse()
                            {
                                Id = c.Id,
                                Date = c.Date,
                                PriceDividedBy = c.PriceDividedBy,
                                PricePaidFor = c.PricePaidFor,
                                ItemsConsumed = c.ItemsConsumed
                                    .Select(i => new ItemConsumedResponse()
                                    {
                                        Id = i.Id,
                                        Quantity = i.Quantity,
                                        Description = i.Description,
                                        Price = i.Price,
                                        Paid = i.Paid,
                                    })
                                    .ToList(),
                            })
                            .FirstOrDefault(c => c.Date == s.Date),
                    },
                })
                .ToListAsync();

            return Ok(schedules);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AgendaRequest request)
        {
            var organization = await _dbContext.Organizations.FindAsync(_userResolver.OrganizationId);
            if (organization is null)
            {
                return BadRequest("Organization not found");
            }

            var agenda = new Agenda()
            {
                Name = request.Name,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                Interval = request.Interval,
                OrganizationId = organization.Id,
            };

            await _dbContext.Agendas.AddAsync(agenda);
            await _dbContext.SaveChangesAsync();

            return Created($"/{agenda.Id}", agenda.Id);
        }

        [HttpPut("{agendaId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int agendaId, [FromBody] UpdateAgendaRequest request)
        {
            var agenda = await _dbContext.Agendas
                .FirstOrDefaultAsync(a => a.Id == agendaId && a.OrganizationId == _userResolver.OrganizationId);

            if (agenda is null)
            {
                return NotFound("No agenda found");
            }

            agenda.Name = request.Name;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{agendaId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int agendaId)
        {
            var agenda = await _dbContext.Agendas
                .Include(x => x.Appointments)
                .FirstOrDefaultAsync(x => x.Id == agendaId && x.OrganizationId == _userResolver.OrganizationId);

            if (agenda is null)
            {
                return NotFound("No agenda found");
            }

            _dbContext.Agendas.Remove(agenda);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
