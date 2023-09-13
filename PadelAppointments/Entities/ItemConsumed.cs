using System.ComponentModel.DataAnnotations.Schema;

namespace PadelAppointments.Entities
{
    public class ItemConsumed : IEqualityComparer<ItemConsumed>
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = default!;
        public double Price { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        public virtual Appointment? Appointment { get; set; }

        public bool Equals(ItemConsumed? x, ItemConsumed? y)
        {
            return x?.Quantity == y?.Quantity && x?.Description == y?.Description && x?.Price == y?.Price;
        }

        public int GetHashCode(ItemConsumed obj)
        {
            return base.GetHashCode();
        }
    }
}
