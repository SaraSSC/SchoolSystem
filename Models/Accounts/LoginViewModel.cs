using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Accounts
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress]
        public string Username { get; set; }


        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} is required")]
        public string Password { get; set; }

        /*
         * The RememberMe property is used to store the user's choice to remember the login.
         */
        public bool RememberMe { get; set; }
    }
}
