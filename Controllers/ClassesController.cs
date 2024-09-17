using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Entities;
using SchoolSystem.Helpers.Transformers;
using SchoolSystem.Models.Classes;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    public class ClassesController : Controller
    {
        private readonly IClassRepository _classRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly ICourseRepository _courseRepository;

        public ClassesController
            (
                IClassRepository classRepository,
                IConverterHelper converterHelper,
                ICourseRepository courseRepository
            )
        {
            _classRepository = classRepository;
            _converterHelper = converterHelper;
            _courseRepository = courseRepository;
        }

        // Action method for StaffIndexClasses
        [Authorize(Roles = "Staff")]
        public IActionResult StaffIndexClasses(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var models = Enumerable.Empty<ClassesViewModel>();

            var classes = _classRepository.GetAll();

            if (classes.Any())
            {
                models = (_converterHelper.ClassesToClassesViewModels(classes)).OrderBy(x => x.Code);
            }
            else
            {
                ViewBag.Message = "<span class=\"text-danger\">No Classes Found</span>";
            }

            return View(models);
        }

        // Action method for RegisterClass
        [Authorize(Roles = "Staff")]
        public IActionResult RegisterClass()
        {
            var model = new RegisterClassViewModel
            {
                Courses = _courseRepository.GetComboCourses()
            };

            return View(model);
        }

        // Action method for RegisterClass (HTTP POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RegisterClass(RegisterClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isCodeUsed = await _classRepository.IsCodeInUseOnRegisterAsync(model.Code);

                if (isCodeUsed)
                {
                    ViewBag.Message = "Code already in use by other class";

                    model.Courses = _courseRepository.GetComboCourses();
                    return View(model);
                }

                if (model.StartingDate.Date > model.EndingDate.Date)
                {
                    ViewBag.Message = "Atencion, ending date must be after starting date";

                    model.Courses = _courseRepository.GetComboCourses();
                    return View(model);
                }

                var clas = new Class
                {
                    Code = model.Code,
                    Name = model.Name,
                    CourseId = model.CourseId,
                    StartingDate = model.StartingDate,
                    EndingDate = model.EndingDate
                };

                try
                {
                    await _classRepository.CreateAsync(clas);

                    string message = "Class added successfully";
                    return RedirectToAction("StaffIndexClasses", "Classes", new { message });
                }
                catch
                {
                    ViewBag.ErrorTitle = "Class Not Added";
                    ViewBag.ErrorMessage = "An error was found";
                    return View("Error");
                }
            }

            model.Courses = _courseRepository.GetComboCourses();
            return View(model);
        }

        // Action method for EditClass
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditClass(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("StaffIndexClasses", "Classes");
            }

            var clas = await _classRepository.GetByIdAsync(Id);

            if (clas == null)
            {
                ViewBag.ErrorTitle = "No Class Found";
                ViewBag.ErrorMessage = "Class doesn't exist or an error has occurred";
                return View("Error");
            }

            var model = _converterHelper.ClassToRegisterClassViewModel(clas);

            return View(model);
        }

        // Action method for EditClass (HTTP POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditClass(RegisterClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isCodeInUse = await _classRepository.IsCodeInUseOnEditAsync(model.Id, model.Code);

                if (isCodeInUse)
                {
                    ViewBag.Message = "<span class=\"text-danger\"> This Code already in use by other class, please choose another</span>";

                    model.Courses = _courseRepository.GetComboCourses();
                    return View(model);
                }

                if (model.StartingDate.Date > model.EndingDate.Date)
                {
                    ViewBag.Message = "<span class=\"text-danger\">Atention ending date must be after starting date</span>";

                    model.Courses = _courseRepository.GetComboCourses();
                    return View(model);
                }

                var clas = await _classRepository.GetByIdAsync(model.Id);

                if (clas != null)
                {
                    clas.Code = model.Code;
                    clas.Name = model.Name;
                    clas.CourseId = model.CourseId;
                    clas.StartingDate = model.StartingDate;
                    clas.EndingDate = model.EndingDate;

                    try
                    {
                        await _classRepository.UpdateAsync(clas);

                        ViewBag.Message = "Class saved successfully";

                        model.Courses = _courseRepository.GetComboCourses();
                        return View(model);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("unique"))
                        {
                            ViewBag.ErrorTitle = $"'{clas.Code}' In Use";
                            ViewBag.ErrorMessage = "Code is already in use";
                        }

                        return View("Error");
                    }
                }
            }

            model.Courses = _courseRepository.GetComboCourses();
            return View(model);
        }

        // Action method for DeleteClass
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> DeleteClass(int Id)
        {
            if (Id == 0)
            {
                ViewBag.ErrorTitle = "Class Not Defined";
                ViewBag.ErrorMessage = "Error trying to delete class";
                return View("Error");
            }

            var clas = await _classRepository.GetByIdAsync(Id);

            if (clas == null)
            {
                ViewBag.ErrorTitle = "Class Not Found";
                ViewBag.ErrorMessage = "Error trying to delete class";
                return View("Error");
            }

            string message = string.Empty;

            try
            {
                await _classRepository.DeleteAsync(clas);
                message = "Class deleted successfully";
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"'{clas.Name}' In Use";
                    ViewBag.ErrorMessage = "This can't be deleted because it's being used by one or more records";
                }

                return View("Error");
            }

            return RedirectToAction("StaffIndexClasses", "Classes", new { message });
        }
    }
}
