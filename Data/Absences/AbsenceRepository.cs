using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Configurations;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Absences;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SchoolSystem.Data.Absences
{
    public class AbsenceRepository : GenericRepository<Absence>, IAbsenceRepository
    {
        private readonly DataContext _context;
        private readonly IConfigurationRepository _configurationRepository;

        public AbsenceRepository(DataContext context, IConfigurationRepository configurationRepository)
            : base(context)
        {
            _context = context;
            _configurationRepository = configurationRepository;
        }

        public async Task<bool> IsAbsencesEmptyAsync()
        {
            // Check if the Absences table is empty
            return await _context.Absences.AnyAsync() == false;
        }

        public async Task<IQueryable<StudentAbsences>> GetClassStudentAbsencesAsync(int classId, int disciplineId)
        {
            // Create an empty queryable for students
            var students = Enumerable.Empty<StudentAbsences>().AsQueryable();

            // Get the configuration asynchronously
            var configuration = await _configurationRepository.GetConfigurationsAsync();

            // Execute the query asynchronously
            await Task.Run(() =>
            {
                students =
                (
                    from user in _context.Users
                    join userRole in _context.UserRoles on user.Id equals userRole.UserId
                    join role in _context.Roles on userRole.RoleId equals role.Id
                    join classStudent in _context.Students on user.Id equals classStudent.UserId
                    where role.Name == "Student" && classStudent.ClassId == classId
                    orderby user.FirstName
                    select new
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ProfilePicturePath = user.ProfilePicturePath,
                        HoursAbsence = (
                            from absence in _context.Absences
                            where absence.UserId == user.Id && absence.ClassId == classId && absence.DisciplineId == disciplineId
                            select absence.Duration
                        ).Sum(),
                        HoursDiscipline = (
                            from discipline in _context.Disciplines
                            where discipline.Id == disciplineId
                            select discipline.Duration
                        ).FirstOrDefault()
                    }
                ).Select(x => new StudentAbsences
                {
                    UserId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ProfilePicturePath = x.ProfilePicturePath,
                    HoursAbsence = x.HoursAbsence,
                    PercentageAbsence = CalculatePercentage(x.HoursDiscipline, x.HoursAbsence),
                    Failed = CalculatePercentage(x.HoursDiscipline, x.HoursAbsence) >= configuration.MaxPercentageAbsence
                });
            });

            return students;
        }

        private static int CalculatePercentage(int hoursDiscipline, int hoursAbsence)
        {
            // Check if both hours are zero to avoid division by zero
            if (hoursDiscipline == 0 && hoursAbsence == 0)
            {
                return 0;
            }

            // Calculate the percentage
            double total = Convert.ToDouble(hoursDiscipline);
            double partial = Convert.ToDouble(hoursAbsence);
            double percentage = 100 / (total / partial);

            return Convert.ToInt32(percentage);
        }
    }
}
