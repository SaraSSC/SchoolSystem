using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Configurations
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly DataContext _context;

        public ConfigurationRepository(DataContext context)
        {
            _context = context;
        }

        // Retrieve the configurations from the database
        public async Task<Configuration> GetConfigurationsAsync()
        {
            // Use FirstOrDefaultAsync to retrieve the first configuration
            // If no configuration exists, it will return null
            return await _context.Configurations.FirstOrDefaultAsync();
        }

        // Save the configurations to the database
        public async Task<bool> SaveConfigurationsAsync(int maxStudents, int maxPercentAbsence)
        {
            bool isSuccess = false;

            // Retrieve the configurations from the database
            var configurations = await GetConfigurationsAsync();

            if (configurations != null)
            {
                // Update the max students and max percentage absence values
                configurations.MaxStudentsClass = maxStudents;
                configurations.MaxPercentageAbsence = maxPercentAbsence;

                // Save the changes to the database
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    // If the save operation was successful, set isSuccess to true
                    isSuccess = true;
                }
            }

            return isSuccess;
        }
    }
}
