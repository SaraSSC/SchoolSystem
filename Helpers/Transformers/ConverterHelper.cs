using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Absences;
using SchoolSystem.Models.Classes;
using SchoolSystem.Models.Courses;
using SchoolSystem.Models.Disciplines;
using SchoolSystem.Models.Evaluations;
using System.Linq;

namespace SchoolSystem.Helpers.Transformers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly ICourseRepository _courseRepository;

        public ConverterHelper(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // Convert a single Course object to a CoursesViewModel object
        public CoursesViewModel CourseToCoursesViewModel(Course course)
        {
            return new CoursesViewModel
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Area = course.Area,
                Duration = course.Duration
            };
        }

        // Convert a collection of Course objects to a collection of CoursesViewModel objects
        public IQueryable<CoursesViewModel> CoursesToCoursesViewModels(IQueryable<Course> courses)
        {
            return courses.Select(x => new CoursesViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Area = x.Area,
                Duration = x.Duration
            });
        }

        // Convert a single Discipline object to a DisciplinesViewModel object
        public DisciplinesViewModel DisciplineToDisciplinesViewModel(Discipline discipline)
        {
            return new DisciplinesViewModel
            {
                Id = discipline.Id,
                Code = discipline.Code,
                Name = discipline.Name,
                Area = discipline.Area,
                Duration = discipline.Duration
            };
        }

        // Convert a collection of Discipline objects to a collection of DisciplinesViewModel objects
        public IQueryable<DisciplinesViewModel> DisciplinesToDisciplinesViewModels(IQueryable<Discipline> disciplines)
        {
            return disciplines.Select(x => new DisciplinesViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Area = x.Area,
                Duration = x.Duration
            });
        }

        // Convert a single Class object to a ClassViewModel object
        public ClassesViewModel ClassToClassesViewModel(Class clas)
        {
            return new ClassesViewModel
            {
                Id = clas.Id,
                Code = clas.Code,
                Name = clas.Name,
                CourseId = clas.CourseId,
                Course = clas.Course,
                StartingDate = clas.StartingDate,
                EndingDate = clas.EndingDate
            };
        }

        // Convert a collection of Class objects to a collection of ClassViewModel objects
        public IQueryable<ClassesViewModel> ClassesToClassesViewModels(IQueryable<Class> classes)
        {
            return classes.Select(x => new ClassesViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CourseId = x.CourseId,
                Course = x.Course,
                StartingDate = x.StartingDate,
                EndingDate = x.EndingDate
            });
        }

        // Convert a single Class object to a RegisterClassViewModel object
        public RegisterClassViewModel ClassToRegisterClassViewModel(Class clas)
        {
            return new RegisterClassViewModel
            {
                Id = clas.Id,
                Code = clas.Code,
                Name = clas.Name,
                CourseId = clas.CourseId,
                Courses = _courseRepository.GetComboCourses(),
                StartingDate = clas.StartingDate,
                EndingDate = clas.EndingDate
            };
        }

        // Convert an AbsenceStudentsViewModel object to an AbsenceDisciplinesViewModel object
        public AbsenceDisciplinesViewModel AbsenceStudentsToDisciplinesViewModel(AbsenceStudentsViewModel model)
        {
            return new AbsenceDisciplinesViewModel
            {
                ClassId = model.ClassId,
                ClassName = model.ClassName,
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                DisciplineId = model.DisciplineId
            };
        }

        // Convert an EvaluationStudentsViewModel object to an EvaluationDisciplinesViewModel object
        public EvaluationDisciplinesViewModel EvaluationStudentsToDisciplinesViewModel(EvaluationStudentsViewModel model)
        {
            return new EvaluationDisciplinesViewModel
            {
                ClassId = model.ClassId,
                ClassName = model.ClassName,
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                DisciplineId = model.DisciplineId
            };
        }
    }
}
