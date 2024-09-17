using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Qualifications
{
    public class QualificationRepository : GenericRepository<Qualification>, IQualificationRepository
    {
        private readonly DataContext _context;

        public QualificationRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        // Retrieve a qualification by name asynchronously
        public async Task<Qualification> GetQualificationByNameAsync(string name)
        {
            // Use FirstOrDefaultAsync to retrieve the first qualification with the given name
            return await _context.Qualifications.FirstOrDefaultAsync(x => x.Name == name);
        }

        // Add a qualification asynchronously
        public async Task AddQualificationAsync(string name)
        {
            // Check if the qualification already exists
            var qualification = await this.GetQualificationByNameAsync(name);

            if (qualification != null)
            {
                // If the qualification already exists, return without adding it again
                return;
            }

            // Create a new qualification with the given name
            var newQualification = new Qualification { Name = name };

            // Add the new qualification to the context
            await _context.Qualifications.AddAsync(newQualification);

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        // Get a list of qualifications for a combo box
        public IEnumerable<SelectListItem> GetComboQualifications()
        {
            // Select the qualifications from the context and map them to SelectListItem objects
            var list = _context.Qualifications.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            // Insert a default SelectListItem at the beginning of the list
            list.Insert(0, new SelectListItem
            {
                Text = "(Select your qualification...)",
                Value = "0"
            });

            return list;
        }
    }
}
