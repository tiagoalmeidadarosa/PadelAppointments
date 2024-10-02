namespace PadelAppointments.Entities
{
    public class Check
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int PriceDividedBy { get; set; }
        public int PricePaidFor { get; set; }

        public int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = default!;

        public ICollection<ItemConsumed> ItemsConsumed { get; set; } = [];
    }
}
