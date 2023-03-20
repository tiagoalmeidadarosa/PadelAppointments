﻿using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Enums;
using PadelAppointments.Models.Requests;
using PadelAppointments.Models.Responses;

namespace PadelAppointments
{
    public static class Endpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            const int numberOfDaysInAWeek = 7;
            const int numberOfWeeksInAMonth = 4;

            app.MapGet("/courts", async (ApplicationDbContext db) =>
            {
                return await db.Courts
                    .AsNoTracking()
                    .Select(c => new CourtsResponse()
                    {
                        Id = c.Id,
                        Name = c.Name,
                    })
                    .ToListAsync();
            })
            .Produces<List<CourtsResponse>>();

            app.MapGet("/courts/{id}/appointments", async (ApplicationDbContext db, int id, DateOnly date) =>
            {
                return await db.Appointments
                    .AsNoTracking()
                    .Include(a => a.Recurrence)
                    .Where(a => a.CourtId == id && a.Date == date)
                    .Select(a => new AppointmentResponse()
                    {
                        Id = a.Id,
                        Date = a.Date,
                        Time = a.Time,
                        CustomerName = a.CustomerName,
                        CustomerPhoneNumber = a.CustomerPhoneNumber,
                        RecurrenceType = a.Recurrence == null ? null : a.Recurrence.Type,
                    })
                    .ToListAsync();
            })
            .Produces<List<AppointmentResponse>>();

            app.MapPost("/courts/{id}/appointments", async (ApplicationDbContext db, int id, AppointmentRequest request) =>
            {
                Appointment appointment;
                if (request.RecurrenceType is null)
                {
                    appointment = await CreateAppointment(db, id, request);
                }
                else
                {
                    var recurrence = new Recurrence()
                    {
                        Type = (RecurrenceType)request.RecurrenceType,
                    };

                    appointment = await CreateAppointment(db, id, request);
                    recurrence.Appointments.Add(appointment);

                    switch (request.RecurrenceType)
                    {
                        case RecurrenceType.NextWeek:
                            request.Date = request.Date.AddDays(numberOfDaysInAWeek);
                            recurrence.Appointments.Add(await CreateAppointment(db, id, request));

                            break;
                        case RecurrenceType.NextMonth:
                            for (int i = 1; i <= numberOfWeeksInAMonth - 1; i++) //minus 1 because the first record has already been created
                            {
                                request.Date = request.Date.AddDays(numberOfDaysInAWeek);
                                recurrence.Appointments.Add(await CreateAppointment(db, id, request));
                            }

                            break;
                        default:
                            break;
                    }

                    await db.Recurrences.AddAsync(recurrence);
                }

                await db.SaveChangesAsync();

                //Fix cycle reference error
                appointment.Recurrence = null;

                return Results.Created($"/courts/{id}/appointments/{appointment.Id}", appointment);
            })
            .Produces(StatusCodes.Status201Created);

            app.MapPut("/courts/{id}/appointments/{appointmentId}", async (ApplicationDbContext db, int id,
                int appointmentId, UpdateAppointmentRequest request) =>
            {
                var appointment = await db.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CourtId == id);

                if (appointment == null)
                {
                    return Results.NotFound();
                }

                if (appointment.RecurrenceId is null)
                {
                    appointment.CustomerName = request.CustomerName;
                    appointment.CustomerPhoneNumber = request.CustomerPhoneNumber;
                }
                else
                {
                    var recurrence = await db.Recurrences
                        .Include(r => r.Appointments)
                        .FirstOrDefaultAsync(r => r.Id == appointment.RecurrenceId);

                    if (recurrence is not null)
                    {
                        foreach (var a in recurrence.Appointments)
                        {
                            a.CustomerName = request.CustomerName;
                            a.CustomerPhoneNumber = request.CustomerPhoneNumber;
                        }
                    }
                }

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            app.MapDelete("/courts/{id}/appointments/{appointmentId}", async (ApplicationDbContext db, int id, int appointmentId) =>
            {
                var appointment = await db.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CourtId == id);

                if (appointment == null)
                {
                    return Results.NotFound();
                }

                if (appointment.RecurrenceId is null)
                {
                    db.Appointments.Remove(appointment);
                }
                else
                {
                    var recurrence = await db.Recurrences
                        .Include(r => r.Appointments)
                        .FirstOrDefaultAsync(r => r.Id == appointment.RecurrenceId);

                    if (recurrence is not null)
                    {
                        foreach (var a in recurrence.Appointments)
                        {
                            db.Appointments.Remove(a);
                        }

                        db.Recurrences.Remove(recurrence);
                    }
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }

        private static async Task<Appointment> CreateAppointment(ApplicationDbContext db, int courtId, AppointmentRequest request)
        {
            var appointment = new Appointment()
            {
                Date = request.Date,
                Time = request.Time,
                CustomerName = request.CustomerName,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                CourtId = courtId,
            };

            await db.Appointments.AddAsync(appointment);

            return appointment;
        }
    }
}
