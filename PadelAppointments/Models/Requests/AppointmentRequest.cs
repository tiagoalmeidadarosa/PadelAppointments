using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class AppointmentRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public string CustomerName { get; set; } = default!;

        [Required]
        public string CustomerPhoneNumber { get; set; } = default!;

        [Required]
        public double Price { get; set; }

        public bool HasRecurrence { get; set; }

        [Required]
        public int AgendaId { get; set; }

        [Required]
        public IEnumerable<ScheduleRequest> Schedules { get; set; } = [];
    }
}
