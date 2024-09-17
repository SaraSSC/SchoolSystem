using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Reports;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Reports
{
    public interface IReportRepository : IGenericRepository<Report>
    {
        Task<bool> IsReportsEmptyAsync();

        Task<IQueryable<ReportsViewModel>> GetAllReportsWithUsersAsync();

        Task<ReportsViewModel> GetReportByIdWithUserAsync(int Id);

        Task<IQueryable<ReportsViewModel>> GetAllReportsByUserAsync(string userId);
    }
}
