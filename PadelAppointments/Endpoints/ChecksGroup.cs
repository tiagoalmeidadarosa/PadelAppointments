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

                await db.SaveChangesAsync();

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
