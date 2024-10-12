using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class ScheduleRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }
    }
}
