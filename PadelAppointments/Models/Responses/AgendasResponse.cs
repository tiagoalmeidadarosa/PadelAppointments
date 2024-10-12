namespace PadelAppointments.Models.Responses
{
    public class AgendasResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public TimeOnly StartsAt { get; set; }
        public TimeOnly EndsAt { get; set; }
        public int Interval { get; set; }
    }
}
