using PadelAppointments.Entities;
using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class UpdateAppointmentRequest
    {
        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? CustomerPhoneNumber { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public IEnumerable<ItemConsumedRequest> ItemsConsumed { get; set; } = Enumerable.Empty<ItemConsumedRequest>();
    }
}
