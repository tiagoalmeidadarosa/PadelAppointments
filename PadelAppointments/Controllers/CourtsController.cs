using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Responses;
using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.User)]
    public class CourtsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public CourtsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CourtsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var courts = await _dbContext.Courts
                .AsNoTracking()
                .Select(c => new CourtsResponse() { Id = c.Id, Name = c.Name })
                .ToListAsync();

            return Ok(courts);
        }

        [HttpGet("{courtId}/schedules")]
        [ProducesResponseType(typeof(List<ScheduleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSchedules([FromRoute] int courtId, [FromQuery][Required] DateOnly date)
        {
            var court = await _dbContext.Courts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courtId);

            if (court is null)
            {
                return NotFound("No court found");
            }

            var schedules = await _dbContext.Schedules
                .AsNoTracking()
                .Include(s => s.Appointment)
                .ThenInclude(a => a.Checks)
                .ThenInclude(c => c.ItemsConsumed)
                .Where(s => s.CourtId == courtId && s.Date == date)
                .Select(s => new ScheduleResponse()
                {
                    Id = s.Id,
                    Date = s.Date,
                    Time = s.Time,
                    CourtId = s.CourtId,
                    Appointment = new AppointmentResponse()
                    {
                        Id = s.Appointment.Id,
                        Date = s.Appointment.Date,
                        CustomerName = s.Appointment.CustomerName,
                        CustomerPhoneNumber = s.Appointment.CustomerPhoneNumber,
                        Price = s.Appointment.Price,
                        HasRecurrence = s.Appointment.HasRecurrence,
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
    }
}
