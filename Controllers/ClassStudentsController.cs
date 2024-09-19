using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Configurations;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Students;
using SchoolSystem.Models.Students;


namespace SchoolWeb.Controllers
{
    public class ClassStudentsController : Controller
    {
        private readonly IStudentRepository _classStudentRepository;
        private readonly IClassRepository _classRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public ClassStudentsController
            (
                IStudentRepository classStudentRepository,
                IClassRepository classRepository,
                ICourseRepository courseRepository,
                IConfigurationRepository configurationRepository
            )
        {
            _classStudentRepository = classStudentRepository;
            _classRepository = classRepository;
            _courseRepository = courseRepository;
            _configurationRepository = configurationRepository;
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffIndexClassStudents(int Id, string message)
        {
            if (Id == 0)
            {
                return RedirectToAction("StaffIndexClasses", "Classes");
            }

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var clas = await _classRepository.GetByIdAsync(Id);

            if (clas == null)
            {
                ViewBag.ErrorTitle = "No Class Found";
                ViewBag.ErrorMessage = "Class doesn't exist or there was an error";
                return View("Error");
            }

            var course = await _courseRepository.GetByIdAsync(clas.CourseId);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var classStudentsIds = await _classStudentRepository.GetStudentsIdsByClassIdAsync(Id);

            var students = Enumerable.Empty<StudentsViewModel>();

            if (classStudentsIds.Any())
            {
                students = (await _classStudentRepository.GetClassStudentsListAsync(classStudentsIds)).OrderBy(x => x.FirstName);
            }
            else
            {
                ViewBag.Message = "<span class=\"text-danger\">No Students Found</span>";
            }

            var model = new EditClassStudentsViewModel
            {
                ClassId = clas.Id,
                Code = clas.Code,
                Name = clas.Name,
                Course = course.Name,
                Students = students
            };

            return View(model);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditClassStudents(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("StaffIndexClasses", "Classes");
            }

            var clas = await _classRepository.GetByIdAsync(Id);

            if (clas == null)
            {
                ViewBag.ErrorTitle = "No Class Found";
                ViewBag.ErrorMessage = "Class doesn't exist or there was an error";
                return View("Error");
            }

            var course = await _courseRepository.GetByIdAsync(clas.CourseId);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var studentsSelectable = await _classStudentRepository.GetAllStudentsSelectableAsync(clas.Id);

            var model = new EditStudentsSelectableViewModel
            {
                ClassId = clas.Id,
                Code = clas.Code,
                Name = clas.Name,
                Course = course.Name,
                StudentsSelectable = studentsSelectable.ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditClassStudents(EditStudentsSelectableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var clas = await _classRepository.GetByIdAsync(model.ClassId);

                if (clas == null)
                {
                    ViewBag.ErrorTitle = "No Class Found";
                    ViewBag.ErrorMessage = "Class doesn't exist or there was an error";
                    return View("Error");
                }

                if (model.StudentsSelectable == null)
                {
                    ViewBag.ErrorTitle = "No Students Found";
                    ViewBag.ErrorMessage = "There are no students or there was an error";
                    return View("Error");
                }

                var configuration = await _configurationRepository.GetConfigurationsAsync();
                int studentsInClassDb = await _classStudentRepository.GetClassStudentsTotalAsync(model.ClassId);
                int studentsInClassModel = model.StudentsSelectable.Where(x => x.IsSelected).Count();

                string success = string.Empty;

                try
                {
                    foreach (var student in model.StudentsSelectable)
                    {
                        var classStudent = await _classStudentRepository.GetClassStudentAsync(model.ClassId, student.UserId);

                        if (classStudent == null && student.IsSelected)
                        {
                            if (studentsInClassDb >= configuration.MaxStudentsClass)
                            {
                                if (studentsInClassModel > configuration.MaxStudentsClass)
                                {
                                    string warning = $"<span class=\"text-danger\">Maximum students per class reached  ({configuration.MaxStudentsClass})</span>";

                                    return RedirectToAction("StaffIndexClassStudents", "ClassStudents", new { Id = model.ClassId, message = warning });
                                }
                            }

                            await _classStudentRepository.CreateAsync(new Student
                            {
                                ClassId = model.ClassId,
                                UserId = student.UserId
                            });

                            success = "Class students updated successfully";
                            studentsInClassDb++;
                        }

                        if (classStudent != null && !student.IsSelected)
                        {
                            await _classStudentRepository.DeleteAsync(classStudent);

                            success = "Class students updated successfully";
                            studentsInClassDb--;
                        }
                    }
                }
                catch
                {
                    ViewBag.ErrorTitle = "DataBase Error";
                    ViewBag.ErrorMessage = "Error adding students to class";
                    return View("Error");
                }

                return RedirectToAction("StaffIndexClassStudents", "ClassStudents", new { Id = model.ClassId, message = success });
            }

            return View(model);
        }
    }
}