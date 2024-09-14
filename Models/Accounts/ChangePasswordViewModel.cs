using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Accounts
{
    public class ChangePasswordViewModel
    {
        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string OldPassword { get; set; }


        [Display(Name = "New Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string NewPassword { get; set; }


        [Compare("NewPassword")]
        [Required(ErrorMessage = "{0} is required")]
        public string Confirm { get; set; }
    }
}
