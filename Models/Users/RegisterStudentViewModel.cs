using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Users
{
    public class RegisterStudentViewModel : User
    {
        public IEnumerable<SelectListItem> Genders { get; set; }


        public IEnumerable<SelectListItem> Qualifications { get; set; }

        /*
         * Custom validation for the password with a Dynamic Data;
         * Defines specific requirements for the password
         */
        [DataType(DataType.Password)]
        [RegularExpression(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[#$@!%&*?._\-]).{6,}",
            ErrorMessage = "Six characters minimum containing lowercase, uppercase, digit and special characters")]
        [Required(ErrorMessage = "{0} is required")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare("Password")]
        [Required(ErrorMessage = "{0} is required")]
        public string Confirm { get; set; }


        [Display(Name = "Picture")]
        [Required(ErrorMessage = "{0} is required")]
        public IFormFile PictureFile { get; set; }

        public bool UseAsProfilePicture { get; set; }
    }
}
