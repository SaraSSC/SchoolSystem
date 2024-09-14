using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "First Name")]
        [MaxLength(100)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string FirstName { get; set; }


        [Display(Name = "Last Name")]
        [MaxLength(100)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string LastName { get; set; }


        [Display(Name = "Gender")]
        [Range(1, int.MaxValue, ErrorMessage = "Select Gender")]
        [Required(ErrorMessage = "{0} is required")]
        public int GenderId { get; set; }

        public Gender Gender { get; set; }


        [Display(Name = "Qualification")]
        [Range(1, int.MaxValue, ErrorMessage = "Select Qualification")]
        [Required(ErrorMessage = "{0} is required")]
        public int QualificationId { get; set; }

        public Qualification Qualification { get; set; }


        [Display(Name = "CC Number")]
        [MaxLength(14)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(14, MinimumLength = 8, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string CcNumber { get; set; }


        [Display(Name = "Birth Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime BirthDate { get; set; }


        [MaxLength(200)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Address { get; set; }


        [MaxLength(50)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string City { get; set; }


        [Display(Name = "Phone Number")]
        [MaxLength(14)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(14, MinimumLength = 9, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public override string PhoneNumber { get; set; }


        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "{0} is required")]
        public override string Email { get; set; }


        [MaxLength(50)]
        public string ProfilePicture { get; set; }


        [MaxLength(50)]
        public string Picture { get; set; }


        [Required]
        public bool PasswordChanged { get; set; } = false;


        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";


        [Display(Name = "Profile Picture Full Path")]
        public string ProfilePicturePath => ProfilePicture == null
            ? $"~/images/pictures/00000000.png"
            : $"~/images/pictures/{ProfilePicture}";


        [Display(Name = "Picture Full Path")]
        public string PicturePath => Picture == null
            ? $"~/images/pictures/00000000.png"
            : $"~/images/pictures/{Picture}";
    }
}
