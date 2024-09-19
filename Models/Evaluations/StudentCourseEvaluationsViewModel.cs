using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Evaluations
{
    public class StudentCourseEvaluationsViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicturePath { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        public string CourseName { get; set; }

        public IEnumerable<EvaluationViewModel> Evaluations { get; set; }
    }
}
