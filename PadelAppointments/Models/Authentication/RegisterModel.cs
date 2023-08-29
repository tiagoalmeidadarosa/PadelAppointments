using System.ComponentModel.DataAnnotations;

namespace PadelAppointments.Models.Authentication;

public class RegisterModel
{
    [Required(ErrorMessage = "User Name is required")]
    public string Username { get; set; } = default!;

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = default!;
}
