using System.ComponentModel.DataAnnotations.Schema;

namespace PadelAppointments.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        [ForeignKey("Court")]
        public int CourtId { get; set; }

        [ForeignKey("Recurrence")]
        public int? RecurrenceId { get; set; }

        public virtual Court? Court { get; set; }
        public virtual Recurrence? Recurrence { get; set; }
    }
}
