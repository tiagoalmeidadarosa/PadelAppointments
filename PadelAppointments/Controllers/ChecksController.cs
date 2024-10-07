using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Authentication;
using PadelAppointments.Models.Requests;
using PadelAppointments.Services;

namespace PadelAppointments.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.User)]
    public class ChecksController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserResolver _userResolver;

        public ChecksController(ApplicationDbContext dbContext, UserResolver userResolver)
        {
            _dbContext = dbContext;
            _userResolver = userResolver;
        }

        [HttpPut("{checkId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int checkId, [FromBody] CheckRequest request)
        {
            var check = await _dbContext.Checks
                .Include(x => x.ItemsConsumed)
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Agenda)
                .FirstOrDefaultAsync(x => x.Id == checkId && x.Appointment.Agenda.OrganizationId == _userResolver.OrganizationId);

            if (check == null)
            {
                return NotFound("No check found");
            }

            check.PriceDividedBy = request.PriceDividedBy;
            check.PricePaidFor = request.PricePaidFor;

            var itemsConsumed = request.ItemsConsumed
                .Select(itemConsumed => new ItemConsumed()
                {
                    Quantity = itemConsumed.Quantity,
                    Description = itemConsumed.Description,
                    Price = itemConsumed.Price,
                    Paid = itemConsumed.Paid,
                })
                .ToList();

            var itemsConsumedAreEqual = Enumerable.SequenceEqual(check.ItemsConsumed, itemsConsumed, new ItemConsumed());
            if (!itemsConsumedAreEqual)
            {
                //remove deleted rows
                check.ItemsConsumed = check.ItemsConsumed.Where(i => request.ItemsConsumed.Any(r => r.Id == i.Id)).ToList();

                foreach (var item in request.ItemsConsumed)
                {
                    var itemConsumedFromCheck = check.ItemsConsumed.FirstOrDefault(i => i.Id == item.Id);
                    if (itemConsumedFromCheck is null)
                    {
                        // add
                        check.ItemsConsumed.Add(new ItemConsumed()
                        {
                            Quantity = item.Quantity,
                            Description = item.Description,
                            Price = item.Price,
                            Paid = item.Paid,
                        });
                    }
                    else
                    {
                        // update
                        itemConsumedFromCheck.Quantity = item.Quantity;
                        itemConsumedFromCheck.Description = item.Description;
                        itemConsumedFromCheck.Price = item.Price;
                        itemConsumedFromCheck.Paid = item.Paid;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
