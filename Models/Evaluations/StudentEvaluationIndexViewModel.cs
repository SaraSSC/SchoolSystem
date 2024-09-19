using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Evaluations
{
    public class StudentEvaluationIndexViewModel
    {
        [Required]
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicturePath { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        public int CourseId { get; set; }

        [Display(Name = "Course")]
        public IEnumerable<SelectListItem> Courses { get; set; }
    }
}

