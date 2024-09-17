using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Reports;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Reports
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        private readonly DataContext _context;

        public ReportRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        // Check if the Reports table is empty
        public async Task<bool> IsReportsEmptyAsync()
        {
            // Use AnyAsync() instead of FirstOrDefaultAsync() to check if any record exists
            return !await _context.Reports.AnyAsync();
        }

        // Get all reports with associated users
        public async Task<IQueryable<ReportsViewModel>> GetAllReportsWithUsersAsync()
        {
            // Use AsQueryable() instead of Enumerable.Empty<ReportsViewModel>().AsQueryable()
            var reports = _context.Reports
                .Include(x => x.User)
                .OrderBy(x => x.Date)
                .Select(x => new ReportsViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    User = x.User,
                    Title = x.Title,
                    Message = x.Message,
                    Date = x.Date,
                    Status = x.Status,
                    StatusDate = x.StatusDate
                });

            // No need to wrap the query in Task.Run() as it's already asynchronous
            return await Task.FromResult(reports);
        }

        // Get a report by its ID with the associated user
        public async Task<ReportsViewModel> GetReportByIdWithUserAsync(int Id)
        {
            var report = await _context.Reports
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == Id);

            // No need for the null check, as FirstOrDefaultAsync() returns null if no record is found
            if (report == null)
            {
                return null;
            }

            return new ReportsViewModel
            {
                Id = report.Id,
                UserId = report.UserId,
                User = report.User,
                Title = report.Title,
                Message = report.Message,
                Date = report.Date,
                Status = report.Status,
                StatusDate = report.StatusDate
            };
        }

        // Get all reports for a specific user
        public async Task<IQueryable<ReportsViewModel>> GetAllReportsByUserAsync(string userId)
        {
            var reports = _context.Reports
                .Include(x => x.User)
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Date)
                .Select(x => new ReportsViewModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    User = x.User,
                    Title = x.Title,
                    Message = x.Message,
                    Date = x.Date,
                    Status = x.Status,
                    StatusDate = x.StatusDate
                });

            // No need to wrap the query in Task.Run() as it's already asynchronous
            return await Task.FromResult(reports);
        }
    }
}
