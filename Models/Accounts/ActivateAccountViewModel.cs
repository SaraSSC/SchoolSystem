using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Accounts
{
    /*
     *  This is be used to activate the account of a user
     *  with a token sent to the user's email.
     */
    public class ActivateAccountViewModel
    {
        public string UserId { get; set; }


        [DataType(DataType.Password)]
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[#$@!%&*?._\-]).{6,}",
            ErrorMessage = "Six characters minimum containing lowercase, uppercase, digit and special characters")]
        [Required(ErrorMessage = "{0} is required")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare("Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string Confirm { get; set; }
    }
}
