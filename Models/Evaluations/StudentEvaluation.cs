using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Evaluations
{
    public class StudentEvaluation
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicturePath { get; set; }

        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? EvaluationDate { get; set; }

        public int? Grade { get; set; }

        [Display(Name = "New Grade")]
        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int? NewGrade { get; set; }
    }
}
