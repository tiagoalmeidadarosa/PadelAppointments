namespace PadelAppointments.Models.Responses
{
    public class ItemConsumedResponse
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; } = default!;
        public double Price { get; set; }
        public bool Paid { get; set; }
    }
}
