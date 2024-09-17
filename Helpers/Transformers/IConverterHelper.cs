using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Absences;
using SchoolSystem.Models.Classes;
using SchoolSystem.Models.Courses;
using SchoolSystem.Models.Disciplines;
using SchoolSystem.Models.Evaluations;
using System.Linq;

namespace SchoolSystem.Helpers.Transformers
{
    public interface IConverterHelper
    {
        CoursesViewModel CourseToCoursesViewModel(Course course);

        IQueryable<CoursesViewModel> CoursesToCoursesViewModels(IQueryable<Course> courses);

        DisciplinesViewModel DisciplineToDisciplinesViewModel(Discipline discipline);

        IQueryable<DisciplinesViewModel> DisciplinesToDisciplinesViewModels(IQueryable<Discipline> disciplines);

        ClassesViewModel ClassToClassesViewModel(Class clas);

        IQueryable<ClassesViewModel> ClassesToClassesViewModels(IQueryable<Class> classes);

        RegisterClassViewModel ClassToRegisterClassViewModel(Class clas);

        AbsenceDisciplinesViewModel AbsenceStudentsToDisciplinesViewModel(AbsenceStudentsViewModel model);

        EvaluationDisciplinesViewModel EvaluationStudentsToDisciplinesViewModel(EvaluationStudentsViewModel model);
    }
}
