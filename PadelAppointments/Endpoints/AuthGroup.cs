using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PadelAppointments.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PadelAppointments.Endpoints
{
    public static class AuthGroup
    {
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
    }
}
