using SchoolSystem.Data.Entities;
using SchoolSystem.Models.API;
using SchoolSystem.Models.Students;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Students
{
    public interface IStudentRepository: IGenericRepository<Student>
    {
        Task<bool> IsStudentsEmptyAsync();

        Task<IQueryable<string>> GetStudentsIdsByClassIdAsync(int classId);

        Task<IEnumerable<StudentsViewModel>> GetClassStudentsListAsync(IQueryable<string> studentsIds);

        Task<IQueryable<StudentsSelectable>> GetAllStudentsSelectableAsync(int classId);

        Task<Student> GetClassStudentAsync(int classId, string studentId);

        Task<int> GetClassStudentsTotalAsync(int classId);

        Task<IQueryable<APIViewModel>> GetStudentsByClassCodeAsync(string classCode);

        Task<IQueryable<APIViewModel>> GetStudentsByCourseIdsAsync(List<int> courseIds);

    }
}
