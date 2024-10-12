namespace PadelAppointments.Models.Responses
{
    public class ScheduleResponse
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public AppointmentResponse Appointment { get; set; } = new();
    }
}
