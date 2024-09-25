using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.CoursesDisciplines;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Data.Entities;
using SchoolSystem.Helpers.Transformers;
using SchoolSystem.Models.CourseDisciplines;
using SchoolSystem.Models.Courses;


namespace SchoolWeb.Controllers
{
    public class CourseDisciplinesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDisciplineRepository _disciplineRepository;
        private readonly ICourseDisciplineRepository _courseDisciplineRepository;
        private readonly IConverterHelper _converterHelper;

        public CourseDisciplinesController
            (
                ICourseRepository coursesRepository,
                IDisciplineRepository disciplineRepository,
                ICourseDisciplineRepository courseDisciplineRepository,
                IConverterHelper converterHelper
            )
        {
            _courseRepository = coursesRepository;
            _disciplineRepository = disciplineRepository;
            _courseDisciplineRepository = courseDisciplineRepository;
            _converterHelper = converterHelper;
        }


        [Authorize(Roles = "Staff")]
        public IActionResult StaffIndexCourses()
        {
            var models = Enumerable.Empty<CoursesViewModel>();

            var courses = _courseRepository.GetAll();

            if (courses.Any())
            {
                models = (_converterHelper.CoursesToCoursesViewModels(courses)).OrderBy(x => x.Name);
            }
            else
            {
                ViewBag.Message = "<span class=\"text-danger\">No Courses Found</span>";
            }

            return View(models);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> DetailsCourseDisciplines(int Id, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            if (Id == 0)
            {
                return RedirectToAction("StaffIndexCourses", "CourseDisciplines");
            }

            var course = await _courseRepository.GetByIdAsync(Id);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var disciplines = await _courseDisciplineRepository.GetDisciplinesByCourseIdAsync(course.Id);

            if (!disciplines.Any())
            {
                ViewBag.Message = "<span class=\"text-danger\">Course has no disciplines</span>";
            }

            var models = new CourseDisciplinesViewModel
            {
                Id = course.Id,
                Code = course.Code,
                Name = course.Name,
                Area = course.Area,
                Duration = course.Duration,
                Disciplines = disciplines
            };

            return View(models);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditCourseDisciplines(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("StaffIndexCourses", "CourseDisciplines");
            }

            var course = await _courseRepository.GetByIdAsync(Id);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var disciplinesSelectable = await _courseDisciplineRepository.GetAllDisciplinesSelectableAsync(course.Id);

            var model = new CourseDisciplinesSelectableViewModel
            {
                CourseId = course.Id,
                CourseName = $"{course.Code}  |  {course.Name}",
                DisciplinesSelectable = disciplinesSelectable.ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditCourseDisciplines(CourseDisciplinesSelectableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var course = await _courseRepository.GetByIdAsync(model.CourseId);

                if (course == null)
                {
                    ViewBag.ErrorTitle = "No Course Found";
                    ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                    return View("Error");
                }

                if (model.DisciplinesSelectable == null)
                {
                    ViewBag.ErrorTitle = "No Disciplines Found";
                    ViewBag.ErrorMessage = "There are no disciplines or there was an error";
                    return View("Error");
                }

                try
                {
                    foreach (var discipline in model.DisciplinesSelectable)
                    {
                        var courseDiscipline = await _courseDisciplineRepository.GetCourseDisciplineAsync(model.CourseId, discipline.Id);

                        if (courseDiscipline == null && discipline.IsSelected)
                        {
                            await _courseDisciplineRepository.CreateAsync(new CourseDiscipline
                            {
                                CourseId = model.CourseId,
                                DisciplineId = discipline.Id
                            });
                        }

                        if (courseDiscipline != null && !discipline.IsSelected)
                        {
                            await _courseDisciplineRepository.DeleteAsync(courseDiscipline);
                        }
                    }
                }
                catch
                {
                    ViewBag.ErrorTitle = "DataBase Error";
                    ViewBag.ErrorMessage = "Error adding disciplines to course";
                    return View("Error");
                }

                string success = "Course disciplines updated successfully";

                return RedirectToAction("DetailsCourseDisciplines", "CourseDisciplines", new { Id = model.CourseId, message = success });
            }

            return View(model);
        }
    }
}