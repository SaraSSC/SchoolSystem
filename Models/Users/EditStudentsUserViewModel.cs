using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Users
{
    public class EditStudentsUserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicture { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public string Age
        {
            // Calculate age based on BirthDate
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

        public string Qualification { get; set; }

        public string City { get; set; }

        public string Email { get; set; }
    }
}

