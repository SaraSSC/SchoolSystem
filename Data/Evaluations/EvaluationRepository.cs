using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Configurations;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Evaluations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SchoolSystem.Data.Evaluations
{
    public class EvaluationRepository : GenericRepository<Evaluation>, IEvaluationRepository
    {
        private readonly DataContext _context;
        private readonly IConfigurationRepository _configurationRepository;

        public EvaluationRepository(DataContext context, IConfigurationRepository configurationRepository)
            : base(context)
        {
            _context = context;
            _configurationRepository = configurationRepository;
        }

        // Checks if the Evaluations table is empty
        public async Task<bool> IsEvaluationsEmptyAsync()
        {
            return await _context.Evaluations.FirstOrDefaultAsync() == null ? true : false;
        }

        // Retrieves an evaluation based on the user ID, class ID, and discipline ID
        public async Task<Evaluation> GetEvaluationAsync(string userId, int classId, int disciplineId)
        {
            return await _context.Evaluations
                .Where(x => x.UserId == userId && x.ClassId == classId && x.DisciplineId == disciplineId)
                .FirstOrDefaultAsync();
        }

        // Retrieves a list of student evaluations for the index page
        public async Task<IList<StudentEvaluationIndexViewModel>> GetStudentEvaluationsIndexAsync()
        {
            var students = new List<StudentEvaluationIndexViewModel>();

            await Task.Run(() =>
            {
                students =
                 (
                     from user in _context.Users
                     join userRole in _context.UserRoles
                     on user.Id equals userRole.UserId
                     join role in _context.Roles
                     on userRole.RoleId equals role.Id
                     where role.Name == "Student"
                     orderby user.FirstName
                     select new
                     {
                         UserId = user.Id,
                         FirstName = user.FirstName,
                         LastName = user.LastName,
                         ProfilePicturePath = user.ProfilePicturePath
                     }
                 )
                 .Select(x => new StudentEvaluationIndexViewModel
                 {
                     UserId = x.UserId,
                     FirstName = x.FirstName,
                     LastName = x.LastName,
                     ProfilePicturePath = x.ProfilePicturePath
                 }).ToList();
            });

            return students;
        }

        // Retrieves a list of courses for a specific student
        public async Task<IEnumerable<SelectListItem>> GetComboCoursesByStudentAsync(string userId)
        {
            var courses = Enumerable.Empty<SelectListItem>();

            await Task.Run(() =>
            {
                courses =
                (
                    from course in _context.Courses
                    join clas in _context.Classes
                    on course.Id equals clas.CourseId
                    join classStudent in _context.Students
                    on clas.Id equals classStudent.ClassId
                    where classStudent.UserId == userId
                    orderby course.Name
                    select new
                    {
                        CourseId = course.Id,
                        Name = course.Name
                    }
                ).Select(x => new SelectListItem
                {
                    Value = x.CourseId.ToString(),
                    Text = x.Name
                });
            });

            return courses;
        }

        // Retrieves a list of student evaluations for a specific course
        public async Task<IEnumerable<EvaluationViewModel>> GetStudentEvaluationsByCourseAsync(string userId, int courseId)
        {
            var evaluations = Enumerable.Empty<EvaluationViewModel>();

            await Task.Run(() =>
            {
                evaluations =
                (
                    from evaluation in _context.Evaluations
                    join discipline in _context.Disciplines
                    on evaluation.DisciplineId equals discipline.Id
                    join courseDiscipline in _context.CourseDisciplines
                    on discipline.Id equals courseDiscipline.DisciplineId
                    join course in _context.Courses
                    on courseDiscipline.CourseId equals course.Id
                    where evaluation.UserId == userId && course.Id == courseId
                    orderby discipline.Name
                    select new
                    {
                        DisciplineCode = discipline.Code,
                        DisciplineName = discipline.Name,
                        Date = evaluation.Date,
                        Grade = evaluation.Grade
                    }
                ).Select(x => new EvaluationViewModel
                {
                    DisciplineCode = x.DisciplineCode,
                    DisciplineName = x.DisciplineName,
                    Date = x.Date,
                    Grade = x.Grade
                });
            });

            return evaluations;
        }

        // Retrieves a list of student evaluations for a specific class and discipline
        public async Task<IQueryable<StudentEvaluation>> GetClassStudentEvaluationsAsync(int classId, int disciplineId)
        {
            var students = Enumerable.Empty<StudentEvaluation>().AsQueryable();

            await Task.Run(() =>
            {
                students =
                (
                    from user in _context.Users
                    join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId
                    join role in _context.Roles
                    on userRole.RoleId equals role.Id
                    join classStudent in _context.Students
                    on user.Id equals classStudent.UserId
                    where role.Name == "Student" && classStudent.ClassId == classId
                    orderby user.FirstName
                    select new
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ProfilePicturePath = user.ProfilePicturePath,
                        Evaluation = (
                            from evaluation in _context.Evaluations
                            where evaluation.UserId == user.Id && evaluation.ClassId == classId && evaluation.DisciplineId == disciplineId
                            select new
                            {
                                Date = evaluation.Date,
                                Grade = evaluation.Grade
                            }).First()
                    }
                ).Select(x => new StudentEvaluation
                {
                    UserId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ProfilePicturePath = x.ProfilePicturePath,
                    EvaluationDate = x.Evaluation.Date,
                    Grade = x.Evaluation.Grade
                }); ;
            });

            return students;
        }

        // Retrieves a list of student evaluation disciplines for a specific course
        public async Task<IQueryable<StudentEvaluationDisciplines>> GetStudentEvaluationDisciplinesByCourseAsync(string userId, int courseId)
        {
            var evaluations = Enumerable.Empty<StudentEvaluationDisciplines>().AsQueryable();
            var configuration = await _configurationRepository.GetConfigurationsAsync();

            await Task.Run(() =>
            {
                evaluations =
                (
                    from user in _context.Users
                    join absence in _context.Absences
                    on user.Id equals absence.UserId
                    join clas in _context.Classes
                    on absence.ClassId equals clas.Id
                    join course in _context.Courses
                    on clas.CourseId equals course.Id
                    join courseDiscipline in _context.CourseDisciplines
                    on course.Id equals courseDiscipline.CourseId
                    join discipline in _context.Disciplines
                    on courseDiscipline.DisciplineId equals discipline.Id
                    join evaluation in _context.Evaluations
                    on discipline.Id equals evaluation.DisciplineId
                    into results
                    from matchedResults in results.DefaultIfEmpty()
                    where user.Id == userId && course.Id == courseId
                    select new
                    {
                        DiscipCode = discipline.Code,
                        DiscipName = discipline.Name,
                        DiscipDuration = discipline.Duration,
                        HoursAbsence = (
                            from absence in _context.Absences
                            where absence.UserId == user.Id && absence.ClassId == clas.Id && absence.DisciplineId == discipline.Id
                            select absence.Duration
                            ).Sum(),
                        HoursDiscipline = (
                            from discipline in _context.Disciplines
                            where discipline.Id == discipline.Id
                            select discipline.Duration
                            ).FirstOrDefault(),
                        Evaluation = (
                            from evaluation in _context.Evaluations
                            where evaluation.UserId == user.Id && evaluation.ClassId == clas.Id && evaluation.DisciplineId == discipline.Id
                            select new
                            {
                                Date = evaluation.Date,
                                Grade = evaluation.Grade
                            }).First()
                    }
                ).Select(x => new StudentEvaluationDisciplines
                {
                    Code = x.DiscipCode,
                    Name = x.DiscipName,
                    Duration = x.DiscipDuration,
                    HoursAbsence = x.HoursAbsence,
                    PercentageAbsence = CalculatePercentage(x.HoursDiscipline, x.HoursAbsence),
                    Date = x.Evaluation.Date,
                    Grade = x.Evaluation.Grade,
                    FailedAbsence = CalculatePercentage(x.HoursDiscipline, x.HoursAbsence) >= configuration.MaxPercentageAbsence ? true : false,
                    FailedGrade = x.Evaluation.Grade < 10
                }).Distinct();
            });

            return evaluations;
        }

        // Calculates the percentage of absence based on the hours of discipline and absence
        private static int CalculatePercentage(int hoursDiscipline, int hoursAbsence)
        {
            if (hoursDiscipline == 0 && hoursAbsence == 0)
            {
                return 0;
            }

            double total = Convert.ToDouble(hoursDiscipline);
            double partial = Convert.ToDouble(hoursAbsence);
            double percentage = 100 / (total / partial);

            return Convert.ToInt32(percentage);
        }
    }
}
