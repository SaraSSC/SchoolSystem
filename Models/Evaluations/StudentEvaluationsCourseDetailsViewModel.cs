using System.Collections.Generic;

namespace SchoolSystem.Models.Evaluations
{
    public class StudentEvaluationsCourseDetailsViewModel
    {
        public string CourseName { get; set; }

        public IEnumerable<StudentEvaluationDisciplines> Disciplines { get; set; }
    }
}
