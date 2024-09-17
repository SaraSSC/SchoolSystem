using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data
{
    public class GenderRepository : GenericRepository<Gender>, IGenderRepository
    {
        private readonly DataContext _context;

        public GenderRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        // Add a gender asynchronously
        public async Task AddGenderAsync(string name)
        {
            // Check if the gender already exists
            var gender = await this.GetGenderByNameAsync(name);

            if (gender != null)
            {
                return; // Gender already exists, no need to add
            }

            // Add the new gender to the context and save changes
            await _context.Genders.AddAsync(new Gender { Name = name });
            await _context.SaveChangesAsync();
        }

        // Get the list of genders for a combo box
        public IEnumerable<SelectListItem> GetComboGenders()
        {
            // Select the name and id of each gender from the context
            var list = _context.Genders.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            // Insert a default option at the beginning of the list
            list.Insert(0, new SelectListItem
            {
                Text = "(Select your gender...)",
                Value = "0"
            });

            return list;
        }

        // Get a gender by name asynchronously
        public async Task<Gender> GetGenderByNameAsync(string name)
        {
            // Find the first gender with the specified name in the context
            return await _context.Genders.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
