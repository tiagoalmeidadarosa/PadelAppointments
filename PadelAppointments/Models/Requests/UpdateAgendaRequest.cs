using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Requests
{
    public class UpdateAgendaRequest
    {
        [Required]
        public string Name { get; set; } = default!;
    }
}
