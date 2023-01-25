using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PadelAppointments;
using PadelAppointments.Entities;

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

app.MapGet("/courts", async (ApplicationDbContext db) => await db.Courts.ToListAsync());

app.MapGet("/courts/{id}", async (ApplicationDbContext db, int id) => await db.Courts.FindAsync(id));

app.MapGet("/courts/{id}/appointments", async (ApplicationDbContext db, int id) 
    => await db.Appointments
        .Where(a => a.CourtId == id)
        .ToListAsync());

app.MapPost("/courts/{id}/appointments", async (ApplicationDbContext db, int id, Appointment appointment) =>
{
    appointment.CourtId = id;
    await db.Appointments.AddAsync(appointment);
    await db.SaveChangesAsync();

    return Results.Created($"/courts/{id}/appointments/{appointment.Id}", appointment);
});

app.MapGet("/courts/{id}/appointments/{appointmentId}", async (ApplicationDbContext db, int id, int appointmentId) 
    => await db.Appointments
        .Where(a => a.CourtId == id)
        .FirstOrDefaultAsync(a => a.Id == appointmentId));

app.Run();