using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Absences
{
    public class AbsenceStudentsViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public string ClassName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Required]
        public int DisciplineId { get; set; }

        public string DisciplineName { get; set; }

        [Required]
        public int DisciplineDuration { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public DateTime Date { get; set; }

        public IList<StudentAbsences> Students { get; set; }
    }
}

