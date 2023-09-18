using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class CheckRequest
    {
        [Required]
        public int PriceDividedBy { get; set; }

        [Required]
        public int PricePaidFor { get; set; }

        [Required]
        public IEnumerable<ItemConsumedRequest> ItemsConsumed { get; set; } = Enumerable.Empty<ItemConsumedRequest>();
    }
}
