using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Students
{
    public class StudentsSelectable : StudentsViewModel
    {
        [Display(Name = "Selected Student")]
        public bool IsSelected { get; set; }
    }
}

