using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using SchoolSystem.Models.Home;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data.Courses
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly DataContext _context;

        public CourseRepository(DataContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsCoursesEmptyAsync()
        {
            return await _context.Courses.FirstOrDefaultAsync() == null ? true : false;
        }

        public async Task<bool> IsCodeInUseOnRegisterAsync(string code)
        {
            var course = await _context.Courses.Where(x => x.Code == code).FirstOrDefaultAsync();

            return course != null ? true : false;
        }

        public async Task<bool> IsCodeInUseOnEditAsync(int idCourse, string code)
        {
            var course = await _context.Courses.Where(x => x.Id != idCourse && x.Code == code).FirstOrDefaultAsync();

            return course != null ? true : false;
        }

        public async Task<Course> GetByCodeAsync(string code)
        {
            return await _context.Courses.Where(x => x.Code == code).FirstOrDefaultAsync();
        }

        public IEnumerable<SelectListItem> GetComboCourses()
        {
            var list = _context.Courses.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a course...)",
                Value = "0"
            });

            return list;
        }

        public async Task<IQueryable<HomeCourseViewModel>> GetHomeCoursesAsync()
        {
            var courses = Enumerable.Empty<HomeCourseViewModel>().AsQueryable();

            await Task.Run(() =>
            {
                courses = _context.Courses
                    .OrderBy(x => x.Name)
                    .Select(x => new HomeCourseViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Area = x.Area,
                        Duration = x.Duration
                    });
            });

            return courses;
        }

        public async Task<IQueryable<Course>> GetStudentCourses(string userId)
        {
            var courses = Enumerable.Empty<Course>().AsQueryable();

            await Task.Run(() =>
            {
                courses =
                (
                    from user in _context.Users
                    join classStudent in _context.Students
                    on user.Id equals classStudent.UserId
                    join clas in _context.Classes
                    on classStudent.ClassId equals clas.Id
                    join course in _context.Courses
                    on clas.CourseId equals course.Id
                    where user.Id == userId
                    select new
                    {
                        Id = course.Id,
                        Code = course.Code,
                        Name = course.Name,
                        Area = course.Area,
                        Duration = course.Duration
                    }
                ).Select(x => new Course
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Area = x.Area,
                    Duration = x.Duration
                });
            });

            return courses;
        }
    }
}
