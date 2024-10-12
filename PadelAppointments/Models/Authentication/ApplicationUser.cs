using Microsoft.AspNetCore.Identity;

namespace PadelAppointments.Models.Authentication;

public class ApplicationUser : IdentityUser
{
    public Guid OrganizationId { get; set; }
}
