namespace PadelAppointments.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }

        public int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = default!;
    }
}
