using System.Collections.Generic;

namespace SchoolSystem.Models.Evaluations.Students
{
    public class StudentEvaluationCourseDetailsViewModel
    {
        public string CourseName { get; set; }

        public IEnumerable<StudentEvaluationDisciplines> Disciplines { get; set; }
    }
}
