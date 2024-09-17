using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Qualifications;
using SchoolSystem.Data;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Users;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using SchoolSystem.Helpers.Emails;

namespace SchoolSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IGenderRepository _genderRepository;
        private readonly IQualificationRepository _qualificationRepository;
        private readonly IEmailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        public UsersController
            (
                IUserHelper userHelper,
                IGenderRepository genderRepository,
                IQualificationRepository qualificationRepository,
                IEmailHelper mailHelper,
                IConfiguration configuration
            )
        {
            _userHelper = userHelper;
            _genderRepository = genderRepository;
            _qualificationRepository = qualificationRepository;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        // This action method is used to display the registration form for a user.
        // It is accessible only to users with the "Admin" role.
        public IActionResult RegisterUser(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.Message = message;
            }

            var model = new RegisterUserViewModel
            {
                Roles = _userHelper.GetComboRoles(),
                Genders = _genderRepository.GetComboGenders(),
                Qualifications = _qualificationRepository.GetComboQualifications()
            };

            return View(model);
        }

        // This action method is used to handle the form submission when registering a user.
        // It is accessible only to users with the "Admin" role.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    var isCcNumberInUse = await _userHelper.IsCcNumberInUseOnRegisterAsync(model.CcNumber);

                    if (isCcNumberInUse)
                    {
                        ViewBag.Message = "<span class=\"text-danger\">CC Number already in use</span>";

                        model.Roles = _userHelper.GetComboRoles();
                        model.Genders = _genderRepository.GetComboGenders();
                        model.Qualifications = _qualificationRepository.GetComboQualifications();
                        return View(model);
                    }

                    // Picture
                    string pictureName = string.Empty;

                    if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                    {
                        pictureName = Guid.NewGuid() + Path.GetExtension(model.ProfilePictureFile.FileName);

                        await SaveUploadedPicture(model.ProfilePictureFile, pictureName);
                    }

                    // User
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        GenderId = model.GenderId,
                        QualificationId = model.QualificationId,
                        CcNumber = model.CcNumber,
                        BirthDate = model.BirthDate,
                        Address = model.Address,
                        City = model.City,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        UserName = model.Email,
                        ProfilePicture = pictureName != string.Empty ? pictureName : null,
                        PasswordChanged = false
                    };

                    var resultAdd = await _userHelper.AddUserAsync(user, model.Password);

                    if (resultAdd != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to save user");
                        return View(model);
                    }

                    // Role
                    string role = await _userHelper.GetRoleByIdAsync(model.RoleId);
                    await AddUserToRoleAsync(user, role);

                    string message = "<span class=\"text-success\">Save successful</span>";

                    // Email
                    Response response = await SendRegistrationEmailAsync(user, model.Email, model.FullName);

                    if (response.IsSuccess)
                    {
                        message += "<br /><span class=\"text-success\">Email sent</span>";
                        return RedirectToAction("RegisterUser", "Users", new { message });
                    }

                    message += "<br />Failed to send email";
                    return RedirectToAction("RegisterUser", "Users", new { message });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email already registered");

                    model.Roles = _userHelper.GetComboRoles();
                    model.Genders = _genderRepository.GetComboGenders();
                    model.Qualifications = _qualificationRepository.GetComboQualifications();
                }
            }

            return View(model);
        }

        // Helper method to save the uploaded picture to the server
        private async Task SaveUploadedPicture(IFormFile picture, string pictureName)
        {
            var path = Path.Combine
                (
                    Directory.GetCurrentDirectory(),
                    "wwwroot\\images\\pictures",
                    pictureName
                );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await picture.CopyToAsync(stream);
            }
        }

        // Helper method to add a user to a role
        private async Task AddUserToRoleAsync(User user, string role)
        {
            await _userHelper.AddUserToRoleAsync(user, role);

            var isUserInRole = await _userHelper.IsUserInRoleAsync(user, role);

            if (!isUserInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, role);
            }
        }

        // Helper method to send a registration email to the user
        private async Task<Response> SendRegistrationEmailAsync(User user, string email, string fullName)
        {
            string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

            string tokenLink = Url.Action
                (
                    "ActivateAccount",
                    "Accounts",
                    new { userid = user.Id, token = myToken },
                    protocol: HttpContext.Request.Scheme
                );

            Response response = _mailHelper.SendEmail
                (
                    email,
                    "Activate Account",
                    "<h3>Activate your SchoolSytem Account</h3>" +
                    $"<p>Dear {fullName}, you are now registered at SchoolSystem.</p>" +
                    $"<p>Remind you that this access key shuldn't be shared with others</p>" +
                    $"<p>Please click <a href=\"{tokenLink}\">here</a> to activate your account.</p>" +
                    "<p>Kind regards.</p>"
                );

            return response;
        }
    }
}
