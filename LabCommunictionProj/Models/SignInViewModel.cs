using System.ComponentModel.DataAnnotations;

namespace LabCommunictionProj.Models
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "Mail is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
