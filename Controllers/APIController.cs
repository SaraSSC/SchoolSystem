using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Evaluations;
using SchoolSystem.Data.Students;
using SchoolSystem.Helpers.Users;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class APIController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _classStudentRepository;
        private readonly IUserHelper _userHelper;
        private readonly ICourseRepository _courseRepository;
        private readonly IEvaluationRepository _evaluationRepository;

        public APIController
            (
                IClassRepository classRepository,
                IStudentRepository classStudentRepository,
                IUserHelper userHelper,
                ICourseRepository courseRepository,
                IEvaluationRepository evaluationRepository
            )
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _userHelper = userHelper;
            _courseRepository = courseRepository;
            _evaluationRepository = evaluationRepository;
        }

        // Retrieves the students in a class based on the class code
        [HttpGet]
        [Route("api/GetStudentsByClass/{code}")]
        public async Task<IActionResult> GetStudentsByClassCode(string code)
        {
            var clas = await _classRepository.GetByCodeAsync(code);

            if (clas == null)
            {
                return NotFound($"Class with code '{code}' not found");
            }

            var students = await _classStudentRepository.GetStudentsByClassCodeAsync(code);

            if (!students.Any())
            {
                return NotFound($"Class '{code}' doesn't have any students");
            }

            return Ok(students);
        }

        // Retrieves a student based on their username
        [HttpGet]
        [Route("api/GetStudentByUserName/{userName}")]
        public async Task<IActionResult> GetStudentByUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound($"No user found");
            }

            var user = await _userHelper.GetUserByEmailAsync(userName);

            if (user == null)
            {
                return NotFound($"User not found");
            }

            var isStudent = await _userHelper.IsUserInRoleAsync(user, "Student");

            if (!isStudent)
            {
                return NotFound($"Invalid user");
            }

            var student = await _userHelper.GetStudentViewModelAsync(user.UserName);

            if (student == null)
            {
                return NotFound($"Oops");
            }

            return Ok(student);
        }

        // Retrieves the courses of a student based on their username
        [HttpGet]
        [Route("api/GetStudentCourses/{userName}")]
        public async Task<IActionResult> GetStudentCourses(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound($"No user found");
            }

            var user = await _userHelper.GetUserByEmailAsync(userName);

            if (user == null)
            {
                return NotFound($"User not found");
            }

            var isStudent = await _userHelper.IsUserInRoleAsync(user, "Student");

            if (!isStudent)
            {
                return NotFound($"Invalid user");
            }

            var courses = await _courseRepository.GetStudentCourses(user.Id);

            if (courses == null)
            {
                return NotFound($"Oops");
            }

            return Ok(courses);
        }

        // Retrieves the evaluations of a student for a specific course
        [HttpGet]
        [Route("api/GetStudentEvaluations/{userName}/{courseId}")]
        public async Task<IActionResult> GetStudentEvaluations(string userName, int courseId)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound($"No user found");
            }

            var user = await _userHelper.GetUserByEmailAsync(userName);

            if (user == null)
            {
                return NotFound($"User not found");
            }

            var isStudent = await _userHelper.IsUserInRoleAsync(user, "Student");

            if (!isStudent)
            {
                return NotFound($"Invalid user");
            }

            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                return NotFound($"Course not found");
            }

            var evaluation = await _evaluationRepository.GetStudentEvaluationDisciplinesByCourseAsync(user.Id, course.Id);

            if (evaluation == null)
            {
                return NotFound($"Oops");
            }

            return Ok(evaluation);
        }
    }
}
