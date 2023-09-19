using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Requests;

namespace PadelAppointments.Endpoints
{
    public static class AppointmentsGroup
    {
        public static RouteGroupBuilder MapAppointmentsGroup(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (ApplicationDbContext db, AppointmentRequest request) =>
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
                    Schedules = request.Schedules.Select(s => new Schedule()
                    {
                        Date = s.Date,
                        Time = s.Time,
                        CourtId = s.CourtId,
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
                                CourtId = schedule.CourtId,
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

                await db.Appointments.AddAsync(appointment);
                await db.SaveChangesAsync();

                FixAppointmentCycleReferenceError(appointment);

                return Results.Created($"/{appointment.Id}", appointment);
            })
            .Produces(StatusCodes.Status201Created);

            group.MapPut("/{appointmentId}", async (ApplicationDbContext db, int appointmentId, UpdateAppointmentRequest request) =>
            {
                var appointment = await db.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                {
                    return Results.NotFound();
                }

                appointment.CustomerName = request.CustomerName;
                appointment.CustomerPhoneNumber = request.CustomerPhoneNumber;
                appointment.Price = request.Price;

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{appointmentId}/schedules/{scheduleId}", async ([FromServices] ApplicationDbContext db,
                [FromRoute] int appointmentId, [FromRoute] int scheduleId, [FromQuery] bool removeRecurrence) =>
            {
                var appointment = await db.Appointments
                    .Include(a => a.Schedules)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                {
                    return Results.NotFound();
                }

                var schedule = await db.Schedules
                    .FirstOrDefaultAsync(a => a.Id == scheduleId);

                if (schedule == null)
                {
                    return Results.NotFound();
                }

                var schedulesToDelete = appointment.Schedules
                    .Where(s => removeRecurrence
                        ? s.Date >= schedule.Date
                        : s.Date == schedule.Date)
                    .ToList();

                db.Schedules.RemoveRange(schedulesToDelete);
                await db.SaveChangesAsync();

                return Results.Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        private static void FixAppointmentCycleReferenceError(Appointment appointment)
        {
            foreach (var schedule in appointment.Schedules)
            {
                schedule.Appointment = default!;
            }

            foreach (var check in appointment.Checks)
            {
                check.Appointment = default!;
            }
        }
    }
}
