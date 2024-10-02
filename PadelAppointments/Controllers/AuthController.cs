using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PadelAppointments.Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static PadelAppointments.Constants;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(CustomClaimNames.OrganizationId, user.OrganizationId.ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(issuer: _configuration["JWT:ValidIssuer"], audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(1), claims: authClaims, signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var organization = await _dbContext.Organizations.FindAsync(model.OrganizationId);
            if (organization is null)
            {
                return BadRequest("Organization not found");
            }

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return Problem("User already exists!");
            }

            var user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                OrganizationId = organization.Id,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Problem("User creation failed! Please check user details and try again.");
            }

            await AddUserRole(user);

            return Ok("User created successfully!");
        }

        [HttpPost("register/admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return Problem("User already exists!");
            }

            var user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return Problem("User creation failed! Please check user details and try again.");
            }

            await AddAdminRole(user);

            return Ok("User created successfully!");
        }

        private async Task AddAdminRole(ApplicationUser user)
        {
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
        }

        private async Task AddUserRole(ApplicationUser user)
        {
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
        }
    }
}
