using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolSystem.Data.Qualifications;
using SchoolSystem.Data;
using SchoolSystem.Helpers.Emails;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Accounts;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;

namespace SchoolSystem.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IGenderRepository _genderRepository;
        private readonly IQualificationRepository _qualificationRepository;
        private readonly IEmailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        public AccountsController
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


        // Handles the GET request for the Login page
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        // Handles the POST request for the Login page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    var emailConfirmed = await _userHelper.IsEmailConfirmedAsync(model.Username);

                    if (!emailConfirmed)
                    {
                        ViewBag.ErrorTitle = "Email Not Confirmed";
                        ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account, otherwise you won't be able to access";

                        await _userHelper.LogOutAsync();
                        return View("Error");
                    }

                    var passwordChanged = await _userHelper.IsPasswordChangedAsync(model.Username);

                    if (!passwordChanged)
                    {
                        ViewBag.ErrorTitle = "Password Not Changed";
                        ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account";

                        await _userHelper.LogOutAsync();
                        return View("Error");
                    }

                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First());
                    }

                    if (await _userHelper.IsUserInRoleAsync(await _userHelper.GetUserByEmailAsync(model.Username), "Admin"))
                    {
                        return RedirectToAction("AdminIndexReports", "Reports");
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.ToString());
                }
            }

            return View(model);
        }


        // Handles the Logout action
        public async Task<ActionResult> Logout()
        {
            await _userHelper.LogOutAsync();

            return RedirectToAction("Index", "Home");
        }


        // Handles the GET request for activating the account
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorTitle = "User ID or Token Missing";
                ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account";
                return View("Error");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorTitle = "User Not Found";
                ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account";
                return View("Error");
            }

            bool isEmailConfirmed = await _userHelper.IsEmailConfirmedAsync(user.Email);

            if (!isEmailConfirmed)
            {
                var result = await _userHelper.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    ViewBag.ErrorTitle = "Failed Email Confirmation";
                    ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account";
                    return View("Error");
                }
            }

            ViewBag.MessageEmail = "Email confirmed";
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        // Handles the POST request for activating the account
        public async Task<IActionResult> ActivateAccount(ActivateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.UserId);

                if (user != null)
                {
                    // Check if the user's email is confirmed
                    string emailMessage = await _userHelper.IsEmailConfirmedAsync(user.Email) ? "Email confirmed" : "Email not confirmed";

                    // Generate a password reset token for the user
                    var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                    // Reset the user's password using the generated token and the new password from the model
                    var resetResult = await _userHelper.ResetPasswordAsync(user, myToken, model.Password);

                    if (resetResult.Succeeded)
                    {
                        // Confirm that the password has been changed
                        var passChanged = await _userHelper.ConfirmPasswordChangedAsync(model.UserId);

                        if (passChanged)
                        {
                            ViewBag.Message = "<span class=\"text-success\">Password changed successfully</span>";
                            ViewBag.MessageEmail = emailMessage;
                            return View();
                        }
                        else
                        {
                            ViewBag.Message = "Error while trying to confirm password change";
                            ViewBag.MessageEmail = emailMessage;
                            return View(model);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Error while trying to change password";
                        ViewBag.MessageEmail = emailMessage;
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found");
                }
            }

            return View(model);
        }


        [Authorize]
        // Handles the GET request for changing the password
        public IActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        // Handles the POST request for changing the password
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user != null)
                {
                    // Check if the new password is different from the old password
                    if (model.OldPassword == model.NewPassword)
                    {
                        ViewBag.Message = "New password must be different from old";
                        return View(model);
                    }

                    // Change the user's password using the old password and the new password from the model
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        string message = "<br />Password changed successfully";

                        return RedirectToAction("EditOwnProfile", "Users", new { message });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found");
                }
            }

            return View(model);
        }


        // Handles the GET request for the RecoverPassword page
        public IActionResult RecoverPassword()
        {
            return View();
        }


        // Handles the POST request for the RecoverPassword page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email not registered");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = Url.Action
                    (
                        "ResetPassword",
                        "Accounts",
                        new { token = myToken },
                        protocol: HttpContext.Request.Scheme
                    );

                // Send password reset email to the user
                Response response = _mailHelper.SendEmail
                    (
                        model.Email,
                        "Password Reset",
                        "<h3>SchoolSystem Password Reset</h3>" +
                        $"<p>Dear {user.FullName}, to reset your password click <a href = \"{link}\">here</a>.</p>" +
                        "<p>Kind regards.</p>"
                    );

                if (response.IsSuccess)
                {
                    ViewBag.Message = "<span class=\"text-success\">Access your email account and follow the link to reset your password</span>";
                }

                return View();
            }

            return View(model);
        }


        // Handles the GET request for the ResetPassword page
        public IActionResult ResetPassword(string token)
        {
            return View();
        }


        // Handles the POST request for the ResetPassword page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(model.UserName);

            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    ViewBag.Message = "<span class=\"text-success\">Password reset successful</span>";
                    return View();
                }

                ViewBag.Message = "Error while trying to reset password";
                return View(model);
            }

            ViewBag.Message = "User not found";
            return View(model);
        }


        [HttpPost]
        // Creates a token for authentication based on the provided login credentials
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(
                        user,
                        model.Password);

                    if (result.Succeeded)
                    {
                        // Define the claims for the token
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        // Generate the key and credentials for signing the token
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        // Create the JWT token with the specified claims, expiration, and signing credentials
                        var token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);

                        // Prepare the response with the token and its expiration date
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }


        // Handles the NotAuthorized action
        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
