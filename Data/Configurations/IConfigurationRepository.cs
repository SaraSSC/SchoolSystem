using SchoolSystem.Data.Entities;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Configurations
{
    public interface IConfigurationRepository
    {
        Task<Configuration> GetConfigurationsAsync();

        Task<bool> SaveConfigurationsAsync(int maxStudents, int maxPercentAbsence);
    }
}

