using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.CourseDisciplines;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.CoursesDisciplines
{
    public class CourseDisciplineRepository : GenericRepository<CourseDiscipline>, ICourseDisciplineRepository
    {
        private readonly DataContext _context;

        public CourseDisciplineRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        // Check if the CourseDisciplines table is empty
        public async Task<bool> IsCourseDisciplinesEmptyAsync()
        {
            return await _context.CourseDisciplines.AnyAsync() == false;
        }

        // Get disciplines associated with a specific course
        public async Task<IQueryable<Discipline>> GetDisciplinesByCourseIdAsync(int courseId)
        {
            // Use AsQueryable() to create an empty queryable
            var disciplines = Enumerable.Empty<Discipline>().AsQueryable();

            // Execute the query asynchronously
            await Task.Run(() =>
            {
                disciplines = _context.CourseDisciplines
                    .Include(x => x.Discipline)
                    .Where(x => x.CourseId == courseId)
                    .Select(x => new Discipline
                    {
                        Id = x.Discipline.Id,
                        Code = x.Discipline.Code,
                        Name = x.Discipline.Name,
                        Area = x.Discipline.Area,
                        Duration = x.Discipline.Duration
                    });
            });

            return disciplines;
        }

        // Get all disciplines with a flag indicating if they are selected for a specific course
        public async Task<IQueryable<DisciplinesSelectable>> GetAllDisciplinesSelectableAsync(int courseId)
        {
            // Use AsQueryable() to create an empty queryable
            var disciplinesSelectable = Enumerable.Empty<DisciplinesSelectable>().AsQueryable();

            // Execute the query asynchronously
            await Task.Run(() =>
            {
                disciplinesSelectable = _context.Disciplines
                    .Select(x => new DisciplinesSelectable
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        Area = x.Area,
                        Duration = x.Duration,
                        IsSelected = _context.CourseDisciplines
                            .Any(courseDisciplines => courseDisciplines.CourseId == courseId && courseDisciplines.DisciplineId == x.Id)
                    });
            });

            return disciplinesSelectable;
        }

        // Get a specific course discipline
        public async Task<CourseDiscipline> GetCourseDisciplineAsync(int courseId, int disciplineId)
        {
            return await _context.CourseDisciplines
                .FirstOrDefaultAsync(x => x.CourseId == courseId && x.DisciplineId == disciplineId);
        }
    }
}
