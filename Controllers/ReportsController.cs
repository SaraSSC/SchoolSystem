using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Reports;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Reports;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SchoolSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserHelper _userHelper;

        public ReportsController(IReportRepository reportRepository, IUserHelper userHelper)
        {
            _reportRepository = reportRepository;
            _userHelper = userHelper;
        }

        // Action method for displaying reports to admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndexReports(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user == null)
            {
                ViewBag.ErrorTitle = "User Not Found";
                ViewBag.ErrorMessage = "User doesn't exist or an error was occurred";
                return View("Error");
            }

            if (this.User.Identity.IsAuthenticated)
            {
                TempData["SessionUserProfilePicture"] = user.ProfilePicturePath;
                TempData["SessionUserFirstName"] = user.FirstName;
            }

            var reports = await _reportRepository.GetAllReportsWithUsersAsync();

            if (!reports.Any())
            {
                ViewBag.Message = "<span class=\"text-danger\">No Reports Found</span>";
            }

            return View(reports);
        }

        // Action method for displaying details of a report to admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReportDetails(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("AdminIndexReports", "Reports");
            }

            var report = await _reportRepository.GetReportByIdWithUserAsync(Id);

            if (report == null)
            {
                ViewBag.ErrorTitle = "No Report Found";
                ViewBag.ErrorMessage = "Report doesn't exist or an error was occurred";
                return View("Error");
            }

            return View(report);
        }

        // Action method for updating the status of a report by admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReportDetails(ReportsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var report = await _reportRepository.GetByIdAsync(model.Id);

                if (report == null)
                {
                    ViewBag.ErrorTitle = "Report Not Found";
                    ViewBag.ErrorMessage = "Report doesn't exist or an error was occurred";
                    return View("Error");
                }

                var user = await _userHelper.GetUserByIdAsync(report.UserId);

                if (user == null)
                {
                    ViewBag.ErrorTitle = "User Not Found";
                    ViewBag.ErrorMessage = "User doesn't exist or an error was occurred";
                    return View("Error");
                }

                report.User = user;
                report.Status = true;
                report.StatusDate = DateTime.Today;

                try
                {
                    await _reportRepository.UpdateAsync(report);

                    string message = "Report updated successfully";
                    return RedirectToAction("AdminIndexReports", "Reports", new { message });
                }
                catch
                {
                    ViewBag.ErrorTitle = "Report Not Updated";
                    ViewBag.ErrorMessage = "There was an error";
                    return View("Error");
                }
            }

            return RedirectToAction(nameof(AdminIndexReports));
        }

        // Action method for displaying reports to staff
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffIndexReports(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user == null)
            {
                ViewBag.ErrorTitle = "User Not Found";
                ViewBag.ErrorMessage = "User doesn't exist or an error was occurred";
                return View("Error");
            }

            var reports = await _reportRepository.GetAllReportsByUserAsync(user.Id);

            if (!reports.Any())
            {
                ViewBag.Message = "<span class=\"text-danger\">No Reports Found</span>";
            }

            return View(reports);
        }

        // Action method for displaying details of a report to staff
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffReportDetails(int Id)
        {
            if (Id == 0)
            {
                return RedirectToAction("StaffIndexReports", "Reports");
            }

            var report = await _reportRepository.GetReportByIdWithUserAsync(Id);

            if (report == null)
            {
                ViewBag.ErrorTitle = "No Report Found";
                ViewBag.ErrorMessage = "Report doesn't exist or an error was occurred";
                return View("Error");
            }

            return View(report);
        }

        // Action method for creating a new report by staff
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffNewReport()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user == null)
            {
                ViewBag.ErrorTitle = "User Not Found";
                ViewBag.ErrorMessage = "User doesn't exist or an error was occurred";
                return View("Error");
            }

            var model = new ReportsViewModel
            {
                UserId = user.Id
            };

            return View(model);
        }

        // Action method for submitting a new report by staff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffNewReport(ReportsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user == null)
                {
                    ViewBag.ErrorTitle = "User Not Found";
                    ViewBag.ErrorMessage = "User doesn't exist or an error was occurred";
                    return View("Error");
                }

                var report = new Report
                {
                    UserId = user.Id,
                    User = user,
                    Title = model.Title,
                    Message = model.Message,
                    Date = DateTime.Today,
                    Status = false
                };

                try
                {
                    await _reportRepository.CreateAsync(report);

                    string message = "Report added successfully";
                    return RedirectToAction("StaffIndexReports", "Reports", new { message });
                }
                catch
                {
                    ViewBag.ErrorTitle = "Report Not Added";
                    ViewBag.ErrorMessage = "There was an error";
                    return View("Error");
                }
            }

            return View(model);
        }
    }
}
