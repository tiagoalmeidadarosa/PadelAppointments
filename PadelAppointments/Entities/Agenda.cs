namespace PadelAppointments.Entities
{
    public class Agenda
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public TimeOnly StartsAt { get; set; }
        public TimeOnly EndsAt { get; set; }
        public int Interval { get; set; }

        public Guid OrganizationId { get; set; }
        public virtual Organization Organization { get; set; } = default!;

        public ICollection<Appointment> Appointments { get; set; } = [];
    }
}
