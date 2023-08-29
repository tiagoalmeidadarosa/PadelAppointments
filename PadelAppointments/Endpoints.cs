using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PadelAppointments.Entities;
using PadelAppointments.Enums;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Requests;
using PadelAppointments.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PadelAppointments
{
    public static class Endpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapGroup("auth")
                .MapAuthGroup();

            app.MapGroup("courts")
                .MapCourtsGroup()
                .RequireAuthorization();
        }

        public static RouteGroupBuilder MapAuthGroup(this RouteGroupBuilder group)
        {
            group.MapPost("/login", async ([FromServices] UserManager<ApplicationUser> userManager, [FromServices] IConfiguration configuration, [FromBody] LoginModel model) =>
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));

                    var token = new JwtSecurityToken(
                        issuer: configuration["JWT:ValidIssuer"],
                        audience: configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    return Results.Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }

                return Results.Unauthorized();
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/register", async ([FromServices] UserManager<ApplicationUser> userManager, [FromServices] IConfiguration configuration, [FromBody] RegisterModel model) =>
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                {
                    return Results.Problem("User already exists!");
                }

                var user = new ApplicationUser()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return Results.Problem("User creation failed! Please check user details and try again.");
                }

                return Results.Ok("User created successfully!");
            })
            .RequireAuthorization(UserRoles.Admin)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

            //group.MapPost("/register/admin", async ([FromServices] UserManager<ApplicationUser> userManager, [FromServices] RoleManager<IdentityRole> roleManager,
            //    [FromServices] IConfiguration configuration, [FromBody] RegisterModel model) =>
            //{
            //    var userExists = await userManager.FindByNameAsync(model.Username);
            //    if (userExists != null)
            //    {
            //        return Results.Problem("User already exists!");
            //    }

            //    ApplicationUser user = new ApplicationUser()
            //    {
            //        Email = model.Email,
            //        SecurityStamp = Guid.NewGuid().ToString(),
            //        UserName = model.Username
            //    };
            //    var result = await userManager.CreateAsync(user, model.Password);
            //    if (!result.Succeeded)
            //    {
            //        return Results.Problem("User creation failed! Please check user details and try again.");
            //    }

            //    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            //    }
            //    if (!await roleManager.RoleExistsAsync(UserRoles.User))
            //    {
            //        await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            //    }

            //    if (await roleManager.RoleExistsAsync(UserRoles.Admin))
            //    {
            //        await userManager.AddToRoleAsync(user, UserRoles.Admin);
            //    }

            //    return Results.Ok("User created successfully!");
            //})
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status500InternalServerError);

            return group;
        }

        public static RouteGroupBuilder MapCourtsGroup(this RouteGroupBuilder group)
        {
            const int NUMBER_OF_DAYS_IN_A_WEEK = 7;

            group.MapGet("/", async (ApplicationDbContext db) =>
            {
                return await GetCourts(db);
            })
            .Produces<List<CourtsResponse>>();

            group.MapGet("/appointments", async (ApplicationDbContext db, DateOnly date) =>
            {
                var courtsAppointments = new List<CourtAppointmentsResponse>();

                var courts = await GetCourts(db);
                foreach (var court in courts)
                {
                    var appointments = await GetAppointments(db, court.Id, date);
                    courtsAppointments.Add(new()
                    {
                        Id = court.Id,
                        Appointments = appointments,
                    });
                }

                return courtsAppointments;
            })
            .Produces<List<CourtAppointmentsResponse>>();

            group.MapGet("/{id}/appointments", async (ApplicationDbContext db, int id, DateOnly date) =>
            {
                return await GetAppointments(db, id, date);
            })
            .Produces<List<AppointmentResponse>>();

            group.MapPost("/{id}/appointments", async (ApplicationDbContext db, int id, AppointmentRequest request) =>
            {
                Appointment appointment = await CreateAppointment(db, id, request);

                if (request.HasRecurrence)
                {
                    var recurrence = new Recurrence()
                    {
                        Type = RecurrenceType.Yearly,
                        Appointments = new List<Appointment>() { appointment },
                    };

                    while (true)
                    {
                        request.Date = request.Date.AddDays(NUMBER_OF_DAYS_IN_A_WEEK);

                        if (request.Date.Year != DateTime.Now.Year)
                        {
                            break;
                        }

                        recurrence.Appointments.Add(await CreateAppointment(db, id, request));
                    }

                    await db.Recurrences.AddAsync(recurrence);
                }

                await db.SaveChangesAsync();

                //Fix cycle reference error
                appointment.Recurrence = null;

                return Results.Created($"/courts/{id}/appointments/{appointment.Id}", appointment);
            })
            .Produces(StatusCodes.Status201Created);

            group.MapPut("/{id}/appointments/{appointmentId}", async (ApplicationDbContext db, int id, int appointmentId, UpdateAppointmentRequest request) =>
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
                    appointment.Price = request.Price;
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
                            a.Price = request.Price;
                        }
                    }
                }

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{id}/appointments/{appointmentId}", async ([FromServices] ApplicationDbContext db, [FromRoute] int id, 
                [FromRoute] int appointmentId, [FromQuery] bool removeRecurrence) =>
            {
                var appointment = await db.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.CourtId == id);

                if (appointment == null)
                {
                    return Results.NotFound();
                }

                if (appointment.RecurrenceId is null || !removeRecurrence)
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

            return group;
        }

        private static async Task<List<CourtsResponse>> GetCourts(ApplicationDbContext db)
        {
            return await db.Courts
                .AsNoTracking()
                .Select(c => new CourtsResponse()
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync();
        }

        private static async Task<List<AppointmentResponse>> GetAppointments(ApplicationDbContext db, int id, DateOnly date)
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
                    Price = a.Price,
                    HasRecurrence = a.Recurrence != null,
                })
                .ToListAsync();
        }

        private static async Task<Appointment> CreateAppointment(ApplicationDbContext db, int courtId, AppointmentRequest request)
        {
            var appointment = new Appointment()
            {
                Date = request.Date,
                Time = request.Time,
                CustomerName = request.CustomerName,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                Price = request.Price,
                CourtId = courtId,
            };

            await db.Appointments.AddAsync(appointment);

            return appointment;
        }
    }
}
