using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Users
{
    public class EditProfileViewModel
    {
        [Required]
        public string UserId { get; set; }


        [Display(Name = "First Name")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string FirstName { get; set; }


        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string LastName { get; set; }


        [Display(Name = "Gender")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a Gender")]
        [Required(ErrorMessage = "{0} is required")]
        public int GenderId { get; set; }

        public IEnumerable<SelectListItem> Genders { get; set; }


        [Display(Name = "Qualification")]
        [Range(1, int.MaxValue, ErrorMessage = "Select your Qualification")]
        [Required(ErrorMessage = "{0} is required")]
        public int QualificationId { get; set; }

        public IEnumerable<SelectListItem> Qualifications { get; set; }


        [Display(Name = "CC Number")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(14, MinimumLength = 8, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string CcNumber { get; set; }


        [Display(Name = "Birth Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Address { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string City { get; set; }


        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(14, MinimumLength = 9, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string PhoneNumber { get; set; }


        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePictureFile { get; set; }


        [Display(Name = "Student Picture")]
        public IFormFile PictureFile { get; set; }


        public string ProfilePicturePath { get; set; }

        /*
         * if true, the profile picture will be removed
         */
        public bool RemoveProfilePicture { get; set; }


        public string PicturePath { get; set; }


        public string Email { get; set; }


        public string Role { get; set; }
    }
}
