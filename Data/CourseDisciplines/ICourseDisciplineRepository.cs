using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.CourseDisciplines;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.CoursesDisciplines
{
    public interface ICourseDisciplineRepository : IGenericRepository<CourseDiscipline>
    {
        Task<bool> IsCourseDisciplinesEmptyAsync();

        Task<IQueryable<Discipline>> GetDisciplinesByCourseIdAsync(int courseId);

        Task<IQueryable<DisciplinesSelectable>> GetAllDisciplinesSelectableAsync(int courseId);

        Task<CourseDiscipline> GetCourseDisciplineAsync(int courseId, int disciplineId);
    }
}
