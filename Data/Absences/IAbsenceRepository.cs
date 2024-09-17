using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Absences;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Absences
{
    public interface IAbsenceRepository : IGenericRepository<Absence>
    {
        Task<bool> IsAbsencesEmptyAsync();

        Task<IQueryable<StudentAbsences>> GetClassStudentAbsencesAsync(int classId, int disciplineId);
    }
}
