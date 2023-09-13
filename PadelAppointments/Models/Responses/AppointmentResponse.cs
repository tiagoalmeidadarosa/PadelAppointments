namespace PadelAppointments.Models.Responses
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public double Price { get; set; }
        public bool HasRecurrence { get; set; }
        public IEnumerable<ItemConsumedResponse> ItemsConsumed { get; set; } = Enumerable.Empty<ItemConsumedResponse>();
    }
}
