using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class AppointmentRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? CustomerPhoneNumber { get; set; }
    }
}
