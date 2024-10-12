using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Requests;
using PadelAppointments.Models.Responses;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class OrganizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public OrganizationsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<OrganizationsResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var organizations = await _dbContext.Organizations
                .AsNoTracking()
                .Select(o => new OrganizationsResponse() 
                { 
                    Id = o.Id, 
                    Name = o.Name,
                })
                .ToListAsync();

            return Ok(organizations);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrganizationRequest request)
        {
            var organization = new Organization()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
            };

            await _dbContext.Organizations.AddAsync(organization);
            await _dbContext.SaveChangesAsync();

            return Created($"/{organization.Id}", organization.Id);
        }
    }
}
