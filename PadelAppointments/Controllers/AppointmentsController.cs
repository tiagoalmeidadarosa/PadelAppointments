using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Requests;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.User)]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AppointmentsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] AppointmentRequest request)
        {
            const int DEFAULT_PRICE_DIVIDED_BY = 4;
            const int DEFAULT_PRICE_PAID_FOR = 0;
            const int NUMBER_OF_DAYS_IN_A_WEEK = 7;

            var appointment = new Appointment()
            {
                Date = request.Date,
                CustomerName = request.CustomerName,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                Price = request.Price,
                HasRecurrence = request.HasRecurrence,
                AgendaId = request.AgendaId,
                Schedules = request.Schedules.Select(s => new Schedule()
                {
                    Date = s.Date,
                    Time = s.Time,
                }).ToList(),
            };

            if (request.HasRecurrence)
            {
                while (true)
                {
                    request.Date = request.Date.AddDays(NUMBER_OF_DAYS_IN_A_WEEK);

                    if (request.Date.Year != DateTime.Now.Year)
                    {
                        break;
                    }

                    foreach (var schedule in request.Schedules)
                    {
                        appointment.Schedules.Add(new()
                        {
                            Date = request.Date,
                            Time = schedule.Time,
                        });
                    }
                }
            }

            appointment.Checks = appointment.Schedules.GroupBy(s => s.Date).Select(g => new Check()
            {
                Date = g.Key,
                PriceDividedBy = DEFAULT_PRICE_DIVIDED_BY,
                PricePaidFor = DEFAULT_PRICE_PAID_FOR,
            }).ToList();

            await _dbContext.Appointments.AddAsync(appointment);
            await _dbContext.SaveChangesAsync();

            return Created($"/{appointment.Id}", appointment.Id);
        }

        [HttpPut("{appointmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int appointmentId, [FromBody] UpdateAppointmentRequest request)
        {
            var appointment = await _dbContext.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound("No appointment found");
            }

            appointment.CustomerName = request.CustomerName;
            appointment.CustomerPhoneNumber = request.CustomerPhoneNumber;
            appointment.Price = request.Price;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{appointmentId}/schedules/{scheduleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int appointmentId, [FromRoute] int scheduleId, [FromQuery] bool removeRecurrence)
        {
            var appointment = await _dbContext.Appointments
                .Include(a => a.Schedules)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return NotFound("No appointment found");
            }

            var schedule = await _dbContext.Schedules
                .FirstOrDefaultAsync(a => a.Id == scheduleId);

            if (schedule == null)
            {
                return NotFound("No schedule found");
            }

            var schedulesToDelete = appointment.Schedules
                .Where(s => removeRecurrence ? s.Date >= schedule.Date : s.Date == schedule.Date)
                .ToList();

            _dbContext.Schedules.RemoveRange(schedulesToDelete);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
