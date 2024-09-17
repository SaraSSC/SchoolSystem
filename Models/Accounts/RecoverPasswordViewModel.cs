using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Accounts
{
    public class RecoverPasswordViewModel
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
