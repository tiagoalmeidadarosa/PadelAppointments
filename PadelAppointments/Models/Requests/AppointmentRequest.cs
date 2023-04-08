using PadelAppointments.Enums;
using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class AppointmentRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }

        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? CustomerPhoneNumber { get; set; }

        [Required]
        public double Price { get; set; }

        public RecurrenceType? RecurrenceType { get; set; }
    }
}
