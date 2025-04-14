using System.ComponentModel.DataAnnotations;

namespace LabCommunictionProj.Models.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Invalid Password. Must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.")]
        public string NewPassword { get; set; }
    }
}
