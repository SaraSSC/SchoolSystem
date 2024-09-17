using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Classes
{
    public interface IClassRepository : IGenericRepository<Class>
    {
        Task<bool> IsClassesEmptyAsync();

        Task<bool> IsCodeInUseOnRegisterAsync(string code);

        Task<bool> IsCodeInUseOnEditAsync(int idClass, string code);

        Task<Class> GetByCodeAsync(string code);

        Task<IEnumerable<SelectListItem>> GetComboClassesAsync();

        Task<IQueryable<HomeClassViewModel>> GetHomeClassesAsync();
    }
}
