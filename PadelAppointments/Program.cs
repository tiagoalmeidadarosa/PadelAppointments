using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PadelAppointments;
using PadelAppointments.Entities;
using PadelAppointments.Models.Requests;
using PadelAppointments.Models.Responses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("Appointments")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PadelAppointments API",
        Description = "Making appointments to your padel games",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PadelAppointments API V1");
    });
}

app.UseHttpsRedirection();

app.MapGet("/courts", async (ApplicationDbContext db) =>
{
    return await db.Courts
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
        .Where(a => a.CourtId == id && a.Date == date)
        .Select(c => new AppointmentResponse()
        {
            Id = c.Id,
            Date = c.Date,
            Time = c.Time,
            CustomerName = c.CustomerName,
            CustomerPhoneNumber = c.CustomerPhoneNumber,
        })
        .ToListAsync();
})
.Produces<List<AppointmentResponse>>();

app.MapPost("/courts/{id}/appointments", async (ApplicationDbContext db, int id, AppointmentRequest request) =>
{
    var appointment = new Appointment()
    {
        Date = DateOnly.FromDateTime(request.Date),
        Time = TimeOnly.FromDateTime(request.Date),
        CustomerName = request.CustomerName,
        CustomerPhoneNumber = request.CustomerPhoneNumber,
        CourtId = id,
    };

    await db.Appointments.AddAsync(appointment);
    await db.SaveChangesAsync();

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

    appointment.CustomerName = request.CustomerName;
    appointment.CustomerPhoneNumber = request.CustomerPhoneNumber;

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

    db.Appointments.Remove(appointment);
    await db.SaveChangesAsync();

    return Results.Ok();
})
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();