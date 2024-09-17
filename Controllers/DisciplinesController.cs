using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Data.Entities;
using SchoolSystem.Helpers.Transformers;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Disciplines;
using System.Linq;
using System.Threading.Tasks;
using SchoolSystem.Helpers;


namespace SchoolSystem.Controllers
{
    public class DisciplinesController : Controller
    {
        private readonly IDisciplineRepository _disciplineRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        public DisciplinesController
            (
                IDisciplineRepository disciplineRepository,
                IUserHelper userHelper,
                IConverterHelper converterHelper
            )
        {
            _disciplineRepository = disciplineRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }

        // Action method for displaying the list of disciplines for the admin
        [Authorize(Roles = "Admin")]
        public IActionResult AdminIndexDisciplines(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var models = Enumerable.Empty<DisciplinesViewModel>();

            var disciplines = _disciplineRepository.GetAll();

            if (disciplines.Any())
            {
                models = (_converterHelper.DisciplinesToDisciplinesViewModels(disciplines)).OrderBy(x => x.Name);
            }
            else
            {
                ViewBag.Message = "<span class=\"text-danger\">No Disciplines Found</span>";
            }

            return View(models);
        }

        // Action method for displaying the registration form for a new discipline
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterDiscipline()
        {
            return View();
        }

        // Action method for handling the registration of a new discipline
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterDiscipline(DisciplinesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isCodeUsed = await _disciplineRepository.IsCodeInUseOnRegisterAsync(model.Code);

                if (isCodeUsed)
                {
                    ViewBag.Message = "<span class=\"text-danger\">Code already in use by other discipline</span>";
                    return View(model);
                }

                var discipline = new Discipline
                {
                    Code = model.Code,
                    Name = model.Name,
                    Area = model.Area,
                    Duration = model.Duration
                };

                try
                {
                    await _disciplineRepository.CreateAsync(discipline);

                    string message = "Discipline added successfully";
                    return RedirectToAction("AdminIndexDisciplines", "Disciplines", new { message });
                }
                catch
                {
                    ViewBag.ErrorTitle = "Discipline Not Added";
                    ViewBag.ErrorMessage = "An error was found";
                    return View("Error");
                }
            }

            return View(model);
        }

        // Action method for displaying the edit form for a discipline
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDiscipline(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("AdminIndexDisciplines", "Disciplines");
            }

            var discipline = await _disciplineRepository.GetByIdAsync(Id);

            if (discipline == null)
            {
                ViewBag.ErrorTitle = "No Discipline Found";
                ViewBag.ErrorMessage = "Discipline doesn't exist or an error occurred";
                return View("Error");
            }

            var model = _converterHelper.DisciplineToDisciplinesViewModel(discipline);

            return View(model);
        }

        // Action method for handling the edit of a discipline
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDiscipline(DisciplinesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isCodeInUse = await _disciplineRepository.IsCodeInUseOnEditAsync(model.Id, model.Code);

                if (isCodeInUse)
                {
                    ViewBag.Message = "<span class=\"text-danger\">Code already in use by other discipline, please choose another</span>";
                    return View(model);
                }

                var discipline = await _disciplineRepository.GetByIdAsync(model.Id);

                if (discipline != null)
                {
                    discipline.Code = model.Code;
                    discipline.Name = model.Name;
                    discipline.Area = model.Area;
                    discipline.Duration = model.Duration;

                    try
                    {
                        await _disciplineRepository.UpdateAsync(discipline);

                        ViewBag.Message = "Discipline saved successfully";
                        return View(model);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message.Contains("unique"))
                        {
                            ViewBag.ErrorTitle = $"'{discipline.Code}' In Use";
                            ViewBag.ErrorMessage = "Code is already in use";
                        }

                        return View("Error");
                    }
                }
            }

            return View(model);
        }

        // Action method for deleting a discipline
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDiscipline(int Id)
        {
            if (Id == 0)
            {
                ViewBag.ErrorTitle = "Discipline Not Defined";
                ViewBag.ErrorMessage = "Error trying to delete discipline";
                return View("Error");
            }

            var discipline = await _disciplineRepository.GetByIdAsync(Id);

            if (discipline == null)
            {
                ViewBag.ErrorTitle = "Discipline Not Found";
                ViewBag.ErrorMessage = "Error trying to delete discipline";
                return View("Error");
            }

            string message = string.Empty;

            try
            {
                await _disciplineRepository.DeleteAsync(discipline);
                message = "Discipline deleted successfully";
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"'{discipline.Name}' In Use";
                    ViewBag.ErrorMessage = "This can't be deleted because it's being used by one or more records";
                }

                return View("Error");
            }

            return RedirectToAction("AdminIndexDisciplines", "Disciplines", new { message });
        }
    }
}
