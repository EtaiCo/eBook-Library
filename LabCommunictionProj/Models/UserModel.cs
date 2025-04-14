using System.ComponentModel.DataAnnotations;
namespace LabCommunictionProj.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(10, MinimumLength =2,ErrorMessage = "First name must be at least 2 chars and at most 10 chars" )]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Last name must be at least 2 chars and at most 10 chars")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Mail is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Mail {  get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "  Phone number must be 10 numbers")]
        public string PhoneNumber {  get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Invalid Password. Must be at least 8 characters long, include an uppercase letter, a lowercase letter, a number, and a special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Id is required")]
        [RegularExpression("^[0-9]{9}$", ErrorMessage = "Id number must be 9 numbers")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Is Admin is required")]
        [RegularExpression("^(yes|no)$", ErrorMessage = "Option must be either 'yes' or 'no'")]
        public string IsAdmin { get; set; }

    }
}
