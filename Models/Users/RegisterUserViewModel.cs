using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Users
{
    public class RegisterUserViewModel : User
    {
        /*
         * enforcing a minimum length requirement of 36 characters for the RoleId property
         */
        [Display(Name = "Role")]
        [RegularExpression("(^.{36,}$)", ErrorMessage = "Select Role")]
        [Required(ErrorMessage = "{0} is required")]
        public string RoleId { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }


        public IEnumerable<SelectListItem> Genders { get; set; }


        public IEnumerable<SelectListItem> Qualifications { get; set; }

        /*
         * Custom validation for the password with a Dynamic Data;
         * Defines specific requirements for the password
         */
        [DataType(DataType.Password)]
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[#$@!%&*?._\-]).{6,}",
            ErrorMessage = "Six characters minimum containing lowercase, uppercase, digit and a least one special character")]
        [Required(ErrorMessage = "{0} is required")]
        public string Password { get; set; }

        /*
         * Compares the password with the Confirm field
         */
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string Confirm { get; set; }

        /*
         * HttpRequest file sent from the client
         */
        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePictureFile { get; set; }
    }
}

