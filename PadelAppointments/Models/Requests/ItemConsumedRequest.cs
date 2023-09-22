using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class ItemConsumedRequest
    {
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Description { get; set; } = default!;

        [Required]
        public double Price { get; set; }

        [Required]
        public bool Paid { get; set; }
    }
}
