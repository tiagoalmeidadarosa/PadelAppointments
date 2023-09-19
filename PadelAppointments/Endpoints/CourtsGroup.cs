using Microsoft.EntityFrameworkCore;
using PadelAppointments.Models.Responses;

namespace PadelAppointments.Endpoints
{
    public static class CourtsGroup
    {
        public static RouteGroupBuilder MapCourtsGroup(this RouteGroupBuilder group)
        {
            group.MapGet("/", async (ApplicationDbContext db) =>
            {
                var courts = await db.Courts
                    .AsNoTracking()
                    .Select(c => new CourtsResponse()
                    {
                        Id = c.Id,
                        Name = c.Name,
                    })
                    .ToListAsync();

                return courts;
            })
            .Produces<List<CourtsResponse>>();

            group.MapGet("/{courtId}/schedules", async (ApplicationDbContext db, int courtId, DateOnly date) =>
            {
                var schedules = await db.Schedules
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

                return schedules;
            })
            .Produces<List<ScheduleResponse>>();

            return group;
        }
    }
}
