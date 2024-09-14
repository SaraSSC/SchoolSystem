using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Students
{
    public class StudentsViewModel
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime BirthDate { get; set; }

        public string City { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicturePath { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        public string Age
        {
            get
            {
                int age = DateTime.Today.Year - BirthDate.Year;

                if (BirthDate > DateTime.Today.AddYears(-age))
                {
                    age--;
                }

                return $"{age} Years";
            }
        }
    }
}

