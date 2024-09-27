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
using SchoolWeb.Helpers;

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


            [Authorize(Roles = "Admin")]
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


            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize(Roles = "Admin")]
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


            [Authorize(Roles = "Staff")]
            public IActionResult RegisterStudent(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                var model = new RegisterStudentViewModel
                {
                    Genders = _genderRepository.GetComboGenders(),
                    Qualifications = _qualificationRepository.GetComboQualifications()
                };

                return View(model);
            }


            [HttpPost]
            [Authorize(Roles = "Staff")]
            public async Task<IActionResult> RegisterStudent(RegisterStudentViewModel model)
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

                            model.Genders = _genderRepository.GetComboGenders();
                            model.Qualifications = _qualificationRepository.GetComboQualifications();
                            return View(model);
                        }

                        // Picture
                        string profilePictureName = string.Empty;
                        string pictureName = string.Empty;

                        if (model.PictureFile != null && model.PictureFile.Length > 0)
                        {
                            pictureName = Guid.NewGuid() + Path.GetExtension(model.PictureFile.FileName);

                            await SaveUploadedPicture(model.PictureFile, pictureName);

                            if (model.UseAsProfilePicture)
                            {
                                profilePictureName = Guid.NewGuid() + Path.GetExtension(model.PictureFile.FileName);

                                await SaveUploadedPicture(model.PictureFile, profilePictureName);
                            }
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
                            ProfilePicture = model.UseAsProfilePicture ? profilePictureName : null,
                            Picture = pictureName,
                            PasswordChanged = false
                        };

                        var resultAdd = await _userHelper.AddUserAsync(user, model.Password);

                        if (resultAdd != IdentityResult.Success)
                        {
                            ModelState.AddModelError(string.Empty, "Failed to save student");
                            return View(model);
                        }

                        // Role
                        await AddUserToRoleAsync(user, "Student");

                        string message = "<span class=\"text-success\">Save successful</span>";

                        // Email
                        Response response = await SendRegistrationEmailAsync(user, model.Email, model.FullName);

                        if (response.IsSuccess)
                        {
                            message += "<br /><span class=\"text-success\">Email sent</span>";
                            return RedirectToAction("RegisterStudent", "Users", new { message });
                        }

                        message += "<br />Failed to send email";
                        return RedirectToAction("RegisterStudent", "Users", new { message });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email already registered");

                        model.Genders = _genderRepository.GetComboGenders();
                        model.Qualifications = _qualificationRepository.GetComboQualifications();
                    }
                }

                return View(model);
            }


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
                        "<h3>Activate your SchoolSystem Account</h3>" +
                        $"<p>Dear {fullName}, you are now registered at SchoolWeb.</p>" +
                        $"<p>Please click <a href=\"{tokenLink}\">here</a> to activate your account.</p>" +
                        "<p>Thank you.</p>"
                    );

                return response;
            }


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


            private async Task AddUserToRoleAsync(User user, string role)
            {
                await _userHelper.AddUserToRoleAsync(user, role);

                var isUserInRole = await _userHelper.IsUserInRoleAsync(user, role);

                if (!isUserInRole)
                {
                    await _userHelper.AddUserToRoleAsync(user, role);
                }
            }


            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> EditUsers(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                var users = await _userHelper.GetUsersListAsync();

                return View(users);
            }


            [Authorize(Roles = "Staff")]
            public async Task<IActionResult> EditStudents(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                var students = await _userHelper.GetStudentsListAsync();

                return View(students);
            }


            [Authorize]
            public async Task<IActionResult> EditOwnProfile(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                var model = new EditProfileViewModel
                {
                    Genders = _genderRepository.GetComboGenders(),
                    Qualifications = _qualificationRepository.GetComboQualifications(),
                    Role = await _userHelper.GetUserRoleAsync(user.Id)
                };

                if (user != null)
                {
                    model.UserId = user.Id;
                    model.FirstName = user.FirstName;
                    model.LastName = user.LastName;
                    model.GenderId = user.GenderId;
                    model.QualificationId = user.QualificationId;
                    model.CcNumber = user.CcNumber;
                    model.BirthDate = user.BirthDate;
                    model.Address = user.Address;
                    model.City = user.City;
                    model.PhoneNumber = user.PhoneNumber;
                    model.ProfilePicturePath = user.ProfilePicturePath;
                    model.PicturePath = user.PicturePath;
                    model.Email = user.Email;

                    TempData["SessionUserProfilePicture"] = user.ProfilePicturePath;
                    TempData["SessionUserFirstName"] = user.FirstName;
                }

                return View(model);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize]
            public async Task<IActionResult> EditOwnProfile(EditProfileViewModel model)
            {
                string message = string.Empty;

                if (ModelState.IsValid)
                {
                    message = await EditProfile(model);
                }

                return RedirectToAction("EditOwnProfile", "Users", new { message });
            }


            [Authorize(Roles = "Admin, Staff")]
            public async Task<IActionResult> EditUserProfile(string message, string Id)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.Message = message;
                }

                var user = await _userHelper.GetUserByIdAsync(Id);

                if (user == null)
                {
                    ViewBag.ErrorTitle = "No User Found";
                    ViewBag.ErrorMessage = "User doesn't exist or there was an error";
                    return View("Error");
                }

                var model = new EditProfileViewModel
                {
                    Genders = _genderRepository.GetComboGenders(),
                    Qualifications = _qualificationRepository.GetComboQualifications(),
                    Role = await _userHelper.GetUserRoleAsync(user.Id)
                };

                if (user != null)
                {
                    model.UserId = user.Id;
                    model.FirstName = user.FirstName;
                    model.LastName = user.LastName;
                    model.GenderId = user.GenderId;
                    model.QualificationId = user.QualificationId;
                    model.CcNumber = user.CcNumber;
                    model.BirthDate = user.BirthDate;
                    model.Address = user.Address;
                    model.City = user.City;
                    model.PhoneNumber = user.PhoneNumber;
                    model.ProfilePicturePath = user.ProfilePicturePath;
                    model.PicturePath = user.PicturePath;
                    model.Email = user.Email;

                    if (user.UserName == this.User.Identity.Name)
                    {
                        TempData["SessionUserProfilePicture"] = user.ProfilePicturePath;
                        TempData["SessionUserFirstName"] = user.FirstName;
                    }
                }

                return View(model);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize(Roles = "Admin, Staff")]
            public async Task<IActionResult> EditUserProfile(EditProfileViewModel model)
            {
                string message = string.Empty;

                if (ModelState.IsValid)
                {
                    message = await EditProfile(model);
                }

                return RedirectToAction("EditUserProfile", "Users", new { message });
            }


            private async Task<string> EditProfile(EditProfileViewModel model)
            {
                string message = string.Empty;

                var user = await _userHelper.GetUserByIdAsync(model.UserId);

                if (user != null)
                {
                    string profilePictureName = string.Empty;
                    string pictureName = string.Empty;
                    string oldProfilePictureName = string.Empty;
                    string oldPictureName = string.Empty;

                    if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                    {
                        profilePictureName = Guid.NewGuid() + Path.GetExtension(model.ProfilePictureFile.FileName);

                        await SaveUploadedPicture(model.ProfilePictureFile, profilePictureName);

                        oldProfilePictureName = user.ProfilePicture;
                    }

                    if (model.PictureFile != null && model.PictureFile.Length > 0)
                    {
                        pictureName = Guid.NewGuid() + Path.GetExtension(model.PictureFile.FileName);

                        await SaveUploadedPicture(model.PictureFile, pictureName);

                        oldPictureName = user.Picture;
                    }

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.GenderId = model.GenderId;
                    user.QualificationId = model.QualificationId;
                    user.BirthDate = model.BirthDate;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.PhoneNumber = model.PhoneNumber;

                    var isCcNumberInUse = await _userHelper.IsCcNumberInUseOnEditAsync(model.UserId, model.CcNumber);

                    if (!isCcNumberInUse)
                    {
                        user.CcNumber = model.CcNumber;
                    }
                    else
                    {
                        message += "<br /><span class=\"text-danger\">CC Number already in use</span>";
                    }

                    if (!string.IsNullOrEmpty(profilePictureName))
                    {
                        user.ProfilePicture = profilePictureName;
                    }

                    if (!string.IsNullOrEmpty(pictureName))
                    {
                        user.Picture = pictureName;
                    }

                    var response = await _userHelper.UpdateUserAsync(user);

                    if (response.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(oldProfilePictureName))
                        {
                            await _userHelper.DeletePictureAsync(oldProfilePictureName);
                        }

                        if (!string.IsNullOrEmpty(oldPictureName))
                        {
                            await _userHelper.DeletePictureAsync(oldPictureName);
                        }

                        message += "<br />Profile saved";

                        if (!string.IsNullOrEmpty(profilePictureName))
                        {
                            message += "<br />Profile picture updated";
                        }

                        if (!string.IsNullOrEmpty(pictureName))
                        {
                            message += "<br />Student picture updated";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description);
                    }

                    if (model.RemoveProfilePicture)
                    {
                        oldProfilePictureName = await _userHelper.GetUserProfilePictureAsync(user.Id);

                        user.ProfilePicture = null;

                        if (!string.IsNullOrEmpty(oldProfilePictureName))
                        {
                            var newResponse = await _userHelper.UpdateUserAsync(user);

                            if (newResponse.Succeeded)
                            {
                                await _userHelper.DeletePictureAsync(oldProfilePictureName);

                                message += "<br />Profile picture updated";
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description);
                            }
                        }
                    }
                }

                return message;
            }


            [Authorize(Roles = "Admin, Staff")]
            public async Task<IActionResult> DeleteProfile(string Id)
            {
                if (string.IsNullOrEmpty(Id))
                {
                    ViewBag.ErrorTitle = "User Not Defined";
                    ViewBag.ErrorMessage = "Error trying to delete user";
                    return View("Error");
                }

                var user = await _userHelper.GetUserByIdAsync(Id);

                if (user == null)
                {
                    ViewBag.ErrorTitle = "User Not Found";
                    ViewBag.ErrorMessage = "Error trying to delete user";
                    return View("Error");
                }

                if (user.UserName == this.User.Identity.Name)
                {
                    ViewBag.ErrorTitle = "Current User";
                    ViewBag.ErrorMessage = "You cannot delete yourself";
                    return View("Error");
                }

                string message = string.Empty;
                try
                {
                    await _userHelper.DeleteUserAsync(user);
                    message = "Profile deleted successfully";

                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        await _userHelper.DeletePictureAsync(user.ProfilePicture);
                    }

                    if (!string.IsNullOrEmpty(user.Picture))
                    {
                        await _userHelper.DeletePictureAsync(user.Picture);
                    }

                    if (this.User.IsInRole("Admin"))
                    {
                        return RedirectToAction($"EditUsers", "Users", new { message });
                    }

                    if (this.User.IsInRole("Staff"))
                    {
                        return RedirectToAction($"EditStudents", "Users", new { message });
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                    {
                        ViewBag.ErrorTitle = $"'{user.FullName}' In Use";
                        ViewBag.ErrorMessage = "Cannot be deleted because it is in use by one or more records";
                    }

                    return View("Error");
                }

                return RedirectToAction("Index", "Home");
            }
        }
    }

