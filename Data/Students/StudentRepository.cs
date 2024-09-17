using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.API;
using SchoolSystem.Models.Students;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Students
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly DataContext _context;

        public StudentRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        // Check if the Students table is empty
        public async Task<bool> IsStudentsEmptyAsync()
        {
            return await _context.Students.FirstOrDefaultAsync() == null;
        }

        // Get the IDs of students in a specific class
        public async Task<IQueryable<string>> GetStudentsIdsByClassIdAsync(int classId)
        {
            var studentsIds = await _context.Students
                .Where(x => x.ClassId == classId)
                .Select(x => x.UserId)
                .ToListAsync();

            return studentsIds.AsQueryable();
        }

        // Get the list of students in a specific class with additional information
        public async Task<IEnumerable<StudentsViewModel>> GetClassStudentsListAsync(IQueryable<string> studentsIds)
        {
            var students = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Student" && studentsIds.Contains(user.Id)
                select new StudentsViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BirthDate = user.BirthDate,
                    City = user.City,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    ProfilePicturePath = user.ProfilePicturePath
                })
                .ToListAsync();

            return students;
        }

        // Get all students with selectable flag indicating if they belong to a specific class
        public async Task<IQueryable<StudentsSelectable>> GetAllStudentsSelectableAsync(int classId)
        {
            var studentsSelectable = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Student"
                select new StudentsSelectable
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BirthDate = user.BirthDate,
                    City = user.City,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    ProfilePicturePath = user.ProfilePicturePath,
                    IsSelected = _context.Students.Any(classStudent => classStudent.ClassId == classId && classStudent.UserId == user.Id)
                })
                .ToListAsync();

            return studentsSelectable.AsQueryable();
        }

        // Get a specific student in a class
        public async Task<Student> GetClassStudentAsync(int classId, string studentId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(x => x.ClassId == classId && x.UserId == studentId);
        }

        // Get the total number of students in a class
        public async Task<int> GetClassStudentsTotalAsync(int classId)
        {
            return await _context.Students
                .CountAsync(x => x.ClassId == classId);
        }

        // Get the list of students in a class identified by class code with additional information
        public async Task<IQueryable<APIViewModel>> GetStudentsByClassCodeAsync(string classCode)
        {
            var students = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                join classStudent in _context.Students on user.Id equals classStudent.UserId
                join clas in _context.Classes on classStudent.ClassId equals clas.Id
                where role.Name == "Student" && clas.Code == classCode
                select new APIViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = _context.Genders.FirstOrDefault(y => y.Id == user.GenderId).Name,
                    Qualification = _context.Qualifications.FirstOrDefault(y => y.Id == user.QualificationId).Name,
                    CcNumber = user.CcNumber,
                    BirthDate = user.BirthDate,
                    Address = user.Address,
                    City = user.City,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email
                })
                .ToListAsync();

            return students.AsQueryable();
        }
    }
}
