using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class AppointmentRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? CustomerPhoneNumber { get; set; }

        [Required]
        public double Price { get; set; }

        public bool HasRecurrence { get; set; }

        [Required]
        public IEnumerable<ScheduleRequest> Schedules { get; set; } = Enumerable.Empty<ScheduleRequest>();

        [Required]
        public IEnumerable<ItemConsumedRequest> ItemsConsumed { get; set; } = Enumerable.Empty<ItemConsumedRequest>();
    }
}
