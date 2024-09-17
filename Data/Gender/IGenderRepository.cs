using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolSystem.Data
{
    public interface IGenderRepository : IGenericRepository<Gender>
    {
        Task<Gender> GetGenderByNameAsync(string name);

        Task AddGenderAsync(string name);

        IEnumerable<SelectListItem> GetComboGenders();
    }
}
