using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Configurations;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models;
using SchoolSystem.Models.Configurations;
using SchoolSystem.Models.Home;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IClassRepository _classRepository;
        private readonly IDisciplineRepository _disciplineRepository;

        public HomeController
            (
                IUserHelper userHelper,
                IConfigurationRepository configurationRepository,
                ICourseRepository courseRepository,
                IClassRepository classRepository,
                IDisciplineRepository disciplineRepository

            )
        {
            _userHelper = userHelper;
            _configurationRepository = configurationRepository;
            _courseRepository = courseRepository;
            _classRepository = classRepository;
            _disciplineRepository = disciplineRepository;
        }

        // Action method for the home page
        public async Task<IActionResult> Index()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user != null)
                {
                    TempData["SessionUserProfilePicture"] = user.ProfilePicturePath;
                    TempData["SessionUserFirstName"] = user.FirstName;
                }
            }

            var model = new HomeCoursesClassesViewModel
            {
                Courses = await _courseRepository.GetHomeCoursesAsync(),
                Classes = await _classRepository.GetHomeClassesAsync()
            };

            return View(model);
        }

        // Action method for displaying disciplines of a course
        public async Task<IActionResult> HomeCourseDisciplines(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new HomeCourseDisciplinesViewModel
            {
                Disciplines = await _disciplineRepository.GetHomeDisciplinesInCourseAsync(Id)
            };

            return View(model);
        }

        // Action method for the privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Action method for displaying configurations
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Configurations(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var configurations = await _configurationRepository.GetConfigurationsAsync();

            if (configurations == null)
            {
                ViewBag.ErrorTitle = "Configurations Not Found";
                ViewBag.ErrorMessage = "An error as found";
                return View("Error");
            }

            var model = new ConfigurationViewModel
            {
                MaxStudentsClass = configurations.MaxStudentsClass,
                MaxPercentageAbsence = configurations.MaxPercentageAbsence
            };

            return View(model);
        }

        // Action method for saving configurations
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Configurations(ConfigurationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isSuccess = await _configurationRepository.SaveConfigurationsAsync(model.MaxStudentsClass, model.MaxPercentageAbsence);

                if (isSuccess)
                {
                    string message = "Configuration saved successfully";
                    return RedirectToAction("Configurations", "Home", new { message });
                }
            }

            return View(model);
        }
    }
}
