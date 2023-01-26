using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class UpdateAppointmentRequest
    {
        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? CustomerPhoneNumber { get; set; }
    }
}
