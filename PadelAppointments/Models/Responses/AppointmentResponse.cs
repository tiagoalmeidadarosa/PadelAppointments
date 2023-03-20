using PadelAppointments.Enums;

namespace PadelAppointments.Models.Responses
{
    public class AppointmentResponse
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
    }
}
