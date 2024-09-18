namespace PadelAppointments.Entities
{
    public class Court
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = [];
        public ICollection<Schedule> Schedules { get; set; } = [];
    }
}
