using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseDisciplines
{
    public class CourseDisciplinesSelectableViewModel
    {
        [Required]
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        [Required]
        public IList<DisciplinesSelectable> DisciplinesSelectable { get; set; }
    }
}

