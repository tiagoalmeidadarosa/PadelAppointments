using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class AgendaRequest
    {
        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public TimeOnly StartsAt { get; set; }

        [Required]
        public TimeOnly EndsAt { get; set; }

        [Required]
        public TimeOnly Interval { get; set; }
    }
}
