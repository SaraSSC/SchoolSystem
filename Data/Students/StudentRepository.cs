using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Students;
using SchoolSystem.Models.API;
using SchoolSystem.Models.Students;

namespace SchoolWeb.Data.Students
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly DataContext _context;

        public StudentRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsStudentsEmptyAsync()
        {
            return await _context.Students.FirstOrDefaultAsync() == null ? true : false;
        }

        public async Task<IQueryable<string>> GetStudentsIdsByClassIdAsync(int classId)
        {
            var studentsIds = Enumerable.Empty<string>().AsQueryable();

            await Task.Run(() =>
            {
                studentsIds = _context.Students
                .Where(x => x.ClassId == classId)
                .Select(x => x.UserId);
            });

            return studentsIds;
        }

        public async Task<IEnumerable<StudentsViewModel>> GetClassStudentsListAsync(IQueryable<string> studentsIds)
        {
            var students = Enumerable.Empty<StudentsViewModel>();

            await Task.Run(() =>
            {
                students = (
                from user in _context.Users
                join userRole in _context.UserRoles
                on user.Id equals userRole.UserId
                join role in _context.Roles
                on userRole.RoleId equals role.Id
                where role.Name == "Student" && studentsIds.Contains(user.Id)
                select new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    BirthDate = user.BirthDate,
                    City = user.City,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    ProfilePicturePath = user.ProfilePicturePath
                }
                ).Select(x => new StudentsViewModel
                {
                    UserId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    BirthDate = x.BirthDate,
                    City = x.City,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    ProfilePicturePath = x.ProfilePicturePath
                });
            });

            return students;
        }

        public async Task<IQueryable<StudentsSelectable>> GetAllStudentsSelectableAsync(int classId)
        {
            var studentsSelectable = Enumerable.Empty<StudentsSelectable>().AsQueryable();

            await Task.Run(() =>
            {
                studentsSelectable =
                    (
                        from user in _context.Users
                        join userRole in _context.UserRoles
                        on user.Id equals userRole.UserId
                        join role in _context.Roles
                        on userRole.RoleId equals role.Id
                        where role.Name == "Student"
                        select user
                    )
                    .Select(x => new StudentsSelectable
                    {
                        UserId = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        BirthDate = x.BirthDate,
                        City = x.City,
                        PhoneNumber = x.PhoneNumber,
                        Email = x.Email,
                        ProfilePicturePath = x.ProfilePicturePath,
                        IsSelected =
                        (
                            from classStudents in _context.Students
                            where classStudents.ClassId == classId
                            select classStudents.UserId
                        )
                        .Contains(x.Id) ? true : false
                    });
            });

            return studentsSelectable;
        }

        public async Task<Student> GetClassStudentAsync(int classId, string studentId)
        {
            return await _context.Students
                .Where(x => x.ClassId == classId && x.UserId == studentId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetClassStudentsTotalAsync(int classId)
        {
            int studentsInClass = 0;

            await Task.Run(() =>
            {
                studentsInClass = _context.Students
                .Where(x => x.ClassId == classId)
                .Select(x => x.UserId).Count();
            });
            return studentsInClass;
        }

        public async Task<IQueryable<APIViewModel>> GetStudentsByClassCodeAsync(string classCode)
        {
            var students = Enumerable.Empty<APIViewModel>().AsQueryable();

            await Task.Run(() =>
            {
                students =
                    (
                        from user in _context.Users
                        join userRole in _context.UserRoles
                        on user.Id equals userRole.UserId
                        join role in _context.Roles
                        on userRole.RoleId equals role.Id
                        join classStudent in _context.Students
                        on user.Id equals classStudent.UserId
                        join clas in _context.Classes
                        on classStudent.ClassId equals clas.Id
                        where role.Name == "Student" && clas.Code == classCode
                select user
                    )
                    .Select(x => new APIViewModel
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Gender = _context.Genders.Where(y => y.Id == x.GenderId).FirstOrDefault().Name,
                        Qualification = _context.Qualifications.Where(y => y.Id == x.QualificationId).FirstOrDefault().Name,
                        CcNumber = x.CcNumber,
                        BirthDate = x.BirthDate,
                        Address = x.Address,
                        City = x.City,
                        PhoneNumber = x.PhoneNumber,
                        Email = x.Email
                    });
            });

            return students;
        }
    }
}