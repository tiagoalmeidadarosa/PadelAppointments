using System.ComponentModel.DataAnnotations.Schema;

namespace PadelAppointments.Entities
{
    public class ItemConsumed : IEqualityComparer<ItemConsumed>
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = default!;
        public double Price { get; set; }
        public bool Paid { get; set; }

        [ForeignKey("Check")]
        public int CheckId { get; set; }
        public virtual Check Check { get; set; } = default!;

        public bool Equals(ItemConsumed? x, ItemConsumed? y)
        {
            return x?.Quantity == y?.Quantity && 
                x?.Description == y?.Description && 
                x?.Price == y?.Price &&
                x?.Paid == y?.Paid;
        }

        public int GetHashCode(ItemConsumed obj)
        {
            return base.GetHashCode();
        }
    }
}
