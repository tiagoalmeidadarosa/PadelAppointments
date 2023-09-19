using Microsoft.EntityFrameworkCore;
using PadelAppointments.Entities;
using PadelAppointments.Models.Requests;

namespace PadelAppointments.Endpoints
{
    public static class ChecksGroup
    {
        public static RouteGroupBuilder MapChecksGroup(this RouteGroupBuilder group)
        {
            group.MapPut("/{checkId}", async (ApplicationDbContext db, int checkId, CheckRequest request) =>
            {
                var check = await db.Checks
                    .Include(c => c.ItemsConsumed)
                    .FirstOrDefaultAsync(a => a.Id == checkId);

                if (check == null)
                {
                    return Results.NotFound();
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
                    check.ItemsConsumed = itemsConsumed;
                }

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
