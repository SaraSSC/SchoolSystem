using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Disciplines
{
    public interface IDisciplineRepository : IGenericRepository<Discipline>
    {
        Task<bool> IsDisciplinesEmptyAsync();

        Task<bool> IsCodeInUseOnRegisterAsync(string code);

        Task<bool> IsCodeInUseOnEditAsync(int idDiscipline, string code);

        Task<Discipline> GetByCodeAsync(string code);

        Task<IEnumerable<SelectListItem>> GetComboDisciplinesInCourseAsync(int courseId);

        Task<IQueryable<HomeDisciplineViewModel>> GetHomeDisciplinesInCourseAsync(int courseId);
    }
}
