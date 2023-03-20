using PadelAppointments.Enums;

namespace PadelAppointments.Entities
{
    public class Recurrence
    {
        public int Id { get; set; }
        public RecurrenceType Type { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
