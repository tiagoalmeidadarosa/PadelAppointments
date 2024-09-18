namespace PadelAppointments.Models.Responses
{
    public class CheckResponse
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public int PriceDividedBy { get; set; }
        public int PricePaidFor { get; set; }

        public IEnumerable<ItemConsumedResponse> ItemsConsumed { get; set; } = [];
    }
}
