using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Evaluations;
using SchoolSystem.Helpers.Transformers;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Evaluations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace SchoolSystem.Controllers
{
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IClassRepository _classRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IDisciplineRepository _disciplineRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        public EvaluationsController
            (
                IEvaluationRepository evaluationRepository,
                IClassRepository classRepository,
                ICourseRepository courseRepository,
                IDisciplineRepository disciplineRepository,
                IUserHelper userHelper,
                IConverterHelper converterHelper
            )
        {
            _evaluationRepository = evaluationRepository;
            _classRepository = classRepository;
            _courseRepository = courseRepository;
            _disciplineRepository = disciplineRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffIndexEvaluations()
        {
            var students = await _evaluationRepository.GetStudentEvaluationsIndexAsync();

            if (students.Any())
            {
                foreach (var student in students)
                {
                    student.Courses = await _evaluationRepository.GetComboCoursesByStudentAsync(student.UserId);
                }
            }

            return View(students);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public IActionResult StaffIndexEvaluations(IEnumerable<StudentEvaluationIndexViewModel> model, string userId)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return View(model);
                }

                int courseId = 0;

                foreach (var item in model)
                {
                    if (item.UserId == userId)
                    {
                        courseId = item.CourseId;
                    }
                }

                return RedirectToAction("StaffStudentEvaluations", "Evaluations", new { userId, courseId });
            }

            return View(model);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffStudentEvaluations(string userId, int courseId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("StaffIndexEvaluations", "Evaluations");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorTitle = "No User Found";
                ViewBag.ErrorMessage = "User doesn't exist or there was an error";
                return View("Error");
            }

            if (courseId == 0)
            {
                return RedirectToAction("StaffIndexEvaluations", "Evaluations");
            }

            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var model = new StudentCourseEvaluationsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicturePath = user.ProfilePicturePath,
                CourseName = $"{course.Code}  |  {course.Name}",
                Evaluations = await _evaluationRepository.GetStudentEvaluationsByCourseAsync(user.Id, course.Id)
            };

            return View(model);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationClasses(int Id)
        {
            var model = new EvaluationClassesViewModel
            {
                ClassId = Id,
                Classes = await _classRepository.GetComboClassesAsync()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationClasses(EvaluationClassesViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("RegisterEvaluationDisciplines", "Evaluations", new { Id = model.ClassId });
            }

            model.Classes = await _classRepository.GetComboClassesAsync();
            return View(model);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationDisciplines(int Id, int disciplineId)
        {
            if (Id == 0)
            {
                return RedirectToAction("RegisterEvaluationClasses", "Evaluations");
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

            var model = new EvaluationDisciplinesViewModel
            {
                ClassId = clas.Id,
                ClassName = $"{clas.Code}  |  {clas.Name}",
                CourseId = course.Id,
                CourseName = $"{course.Code}  |  {course.Name}",
                DisciplineId = disciplineId,
                Disciplines = await _disciplineRepository.GetComboDisciplinesInCourseAsync(course.Id)
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationDisciplines(EvaluationDisciplinesViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("RegisterEvaluationStudents", "Evaluations", model);
            }

            model.Disciplines = await _disciplineRepository.GetComboDisciplinesInCourseAsync(model.CourseId);
            return View(model);
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationStudents(EvaluationDisciplinesViewModel modelInput)
        {
            if (!string.IsNullOrEmpty(modelInput.Message))
            {
                ViewBag.Message = modelInput.Message;
            }

            if (ModelState.IsValid)
            {
                var discipline = await _disciplineRepository.GetByIdAsync(modelInput.DisciplineId);

                if (discipline == null)
                {
                    ViewBag.ErrorTitle = "No Discipline Found";
                    ViewBag.ErrorMessage = "Discipline doesn't exist or there was an error";
                    return View("Error");
                }

                var model = new EvaluationStudentsViewModel
                {
                    ClassId = modelInput.ClassId,
                    ClassName = modelInput.ClassName,
                    CourseName = modelInput.CourseName,
                    DisciplineId = modelInput.DisciplineId,
                    DisciplineName = $"{discipline.Code}  |  {discipline.Name}",
                    Students = (await _evaluationRepository.GetClassStudentEvaluationsAsync(modelInput.ClassId, modelInput.DisciplineId)).ToList()
                };

                return View(model);
            }

            return RedirectToAction("RegisterEvaluationClasses", "Evaluations");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterEvaluationStudents(EvaluationStudentsViewModel model)
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

                var discipline = await _disciplineRepository.GetByIdAsync(model.DisciplineId);

                if (discipline == null)
                {
                    ViewBag.ErrorTitle = "No Discipline Found";
                    ViewBag.ErrorMessage = "Discipline doesn't exist or there was an error";
                    return View("Error");
                }

                bool isNewGradesNull = true;

                foreach (var student in model.Students)
                {
                    if (student.NewGrade != null)
                    {
                        isNewGradesNull = false;
                        break;
                    }
                }

                var modelOut = _converterHelper.EvaluationStudentsToDisciplinesViewModel(model);

                if (isNewGradesNull)
                {
                    modelOut.Message = "<span class=\"text-danger\">No evaluations registered</span>";
                    return RedirectToAction("RegisterEvaluationStudents", "Evaluations", modelOut);
                }

                try
                {
                    foreach (var student in model.Students)
                    {
                        if (student.NewGrade != null)
                        {
                            if (student.Grade == null)
                            {
                                await _evaluationRepository.CreateAsync(new Evaluation
                                {
                                    UserId = student.UserId,
                                    ClassId = model.ClassId,
                                    DisciplineId = model.DisciplineId,
                                    Date = model.Date,
                                    Grade = student.NewGrade.Value
                                });
                            }

                            if (student.Grade != null)
                            {
                                var evaluation = await _evaluationRepository.GetEvaluationAsync(student.UserId, clas.Id, discipline.Id);

                                if (evaluation == null)
                                {
                                    ViewBag.ErrorTitle = "No Evaluation Found";
                                    ViewBag.ErrorMessage = "Evaluation doesn't exist or there was an error";
                                    return View("Error");
                                }

                                evaluation.Date = model.Date;
                                evaluation.Grade = student.NewGrade.Value;

                                await _evaluationRepository.UpdateAsync(evaluation);
                            }
                        }
                    }
                }
                catch
                {
                    ViewBag.ErrorTitle = "DataBase Error";
                    ViewBag.ErrorMessage = "Error registering evaluations";
                    return View("Error");
                }

                modelOut.Message = "Evaluations registered successfully";

                return RedirectToAction("RegisterEvaluationStudents", "Evaluations", modelOut);
            }

            return View(model);
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentEvaluationsCourses(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Index", "Home");
            }

            if (userName != this.User.Identity.Name)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userHelper.GetUserByEmailAsync(userName);

            if (user == null)
            {
                ViewBag.ErrorTitle = "No User Found";
                ViewBag.ErrorMessage = "User doesn't exist or there was an error";
                return View("Error");
            }

            var model = new EvaluationCoursesViewModel
            {
                UserId = user.Id,
                Courses = await _evaluationRepository.GetComboCoursesByStudentAsync(user.Id)
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentEvaluationsCourses(EvaluationCoursesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.UserId);

                if (user == null)
                {
                    ViewBag.ErrorTitle = "No User Found";
                    ViewBag.ErrorMessage = "User doesn't exist or there was an error";
                    return View("Error");
                }

                var course = await _courseRepository.GetByIdAsync(model.CourseId);

                if (course == null)
                {
                    ViewBag.ErrorTitle = "No Course Found";
                    ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                    return View("Error");
                }

                return RedirectToAction("StudentEvaluationsCourseDetails", "Evaluations", new { userId = user.Id, courseId = course.Id });
            }

            model.Courses = await _evaluationRepository.GetComboCoursesByStudentAsync(model.UserId);
            return View(model);
        }


        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentEvaluationsCourseDetails(string userId, int courseId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            if (courseId == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                ViewBag.ErrorTitle = "No Course Found";
                ViewBag.ErrorMessage = "Course doesn't exist or there was an error";
                return View("Error");
            }

            var model = new StudentEvaluationsCourseDetailsViewModel
            {
                CourseName = $"{course.Code}  |  {course.Name}  ({course.Duration}Hours)",
                Disciplines = await _evaluationRepository.GetStudentEvaluationDisciplinesByCourseAsync(userId, courseId)
            };

            return View(model);
        }
    }
}
