using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Qualifications
{
    public interface IQualificationRepository : IGenericRepository<Qualification>
    {
        Task<Qualification> GetQualificationByNameAsync(string name);

        Task AddQualificationAsync(string name);

        IEnumerable<SelectListItem> GetComboQualifications();
    }
}
