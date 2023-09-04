using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PadelAppointments.Entities;
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

            app.MapGroup("appointments")
                .MapAppointmentsGroup()
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
            group.MapGet("/", async (ApplicationDbContext db) =>
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

            group.MapGet("/{courtId}/schedules", async (ApplicationDbContext db, int courtId, DateOnly date) =>
            {
                return await db.Schedules
                    .AsNoTracking()
                    .Include(s => s.Appointment)
                    .Where(s => s.CourtId == courtId && s.Date == date)
                    .Select(s => new ScheduleResponse()
                    {
                        Id = s.Id,
                        Date = s.Date,
                        Time = s.Time,
                        CourtId = s.CourtId,
                        Appointment = new AppointmentResponse()
                        {
                            Id = s.Appointment!.Id,
                            Date = s.Appointment.Date,
                            CustomerName = s.Appointment.CustomerName,
                            CustomerPhoneNumber = s.Appointment.CustomerPhoneNumber,
                            Price = s.Appointment.Price,
                            HasRecurrence = s.Appointment.HasRecurrence,
                        },
                    })
                    .ToListAsync();
            })
            .Produces<List<ScheduleResponse>>();

            return group;
        }

        public static RouteGroupBuilder MapAppointmentsGroup(this RouteGroupBuilder group)
        {
            group.MapPost("/", async (ApplicationDbContext db, AppointmentRequest request) =>
            {
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

                await db.Appointments.AddAsync(appointment);
                await db.SaveChangesAsync();

                // Fix cycle reference error
                foreach (var schedule in appointment.Schedules)
                {
                    schedule.Appointment = null;
                }

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
                    .Include(s => s.Schedules)
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

                var schedulesToDelete = appointment.Schedules!
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
    }
}
