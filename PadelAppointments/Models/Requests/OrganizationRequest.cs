using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class OrganizationRequest
    {
        [Required]
        public string Name { get; set; } = default!;
    }
}
