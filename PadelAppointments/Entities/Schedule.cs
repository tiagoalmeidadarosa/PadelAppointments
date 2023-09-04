using System.ComponentModel.DataAnnotations.Schema;

namespace PadelAppointments.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [ForeignKey("Court")]
        public int CourtId { get; set; }

        public virtual Appointment? Appointment { get; set; }
        public virtual Court? Court { get; set; }
    }
}
