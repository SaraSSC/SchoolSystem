using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Classes
{
    public class ClassRepository : GenericRepository<Class>, IClassRepository
    {
        private readonly DataContext _context;

        public ClassRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        // Check if the Classes table is empty
        public async Task<bool> IsClassesEmptyAsync()
        {
            return !await _context.Classes.AnyAsync();
        }

        // Check if a code is already in use during registration
        public async Task<bool> IsCodeInUseOnRegisterAsync(string code)
        {
            return await _context.Classes.AnyAsync(x => x.Code == code);
        }

        // Check if a code is already in use during editing
        public async Task<bool> IsCodeInUseOnEditAsync(int idClass, string code)
        {
            return await _context.Classes.AnyAsync(x => x.Id != idClass && x.Code == code);
        }

        // Get a class by its code
        public async Task<Class> GetByCodeAsync(string code)
        {
            return await _context.Classes.FirstOrDefaultAsync(x => x.Code == code);
        }

        // Get a list of classes for a combo box
        public async Task<IEnumerable<SelectListItem>> GetComboClassesAsync()
        {
            var list = await _context.Classes
                .Select(x => new SelectListItem
                {
                    Text = $"{x.Code}  |  {x.Name}",
                    Value = x.Id.ToString()
                })
                .ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a class...)",
                Value = "0"
            });

            return list;
        }

        // Get a list of classes for the home page
        public async Task<IQueryable<HomeClassViewModel>> GetHomeClassesAsync()
        {
            var classes = await _context.Classes
                .Include(x => x.Course)
                .Where(x => x.CourseId == x.Course.Id)
                .OrderBy(x => x.Name)
                .Select(x => new HomeClassViewModel
                {
                    Name = x.Name,
                    Course = x.Course.Name,
                    StartDate = x.StartingDate,
                    EndDate = x.EndingDate
                })
                .ToListAsync();

            return classes.AsQueryable();
        }
    }
}
