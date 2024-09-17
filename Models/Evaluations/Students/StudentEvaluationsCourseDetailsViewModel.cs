using System.Collections.Generic;

namespace SchoolSystem.Models.Evaluations.Students
{
    public class StudentEvaluationsCourseDetailsViewModel
    {
        public string CourseName { get; set; }

        public IEnumerable<StudentEvaluationDisciplines> Disciplines { get; set; }
    }
}
