using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Qualifications;
using SchoolSystem.Data;
using SchoolSystem.Helpers.Emails;
using SchoolSystem.Helpers.Users;
using SchoolSystem.Models.Accounts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;

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


    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }


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
                    ViewBag.ErrorMessage = "Access your email account and follow the link to activate your account";

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


    public async Task<ActionResult> Logout()
    {
        await _userHelper.LogOutAsync();

        return RedirectToAction("Index", "Home");
    }


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
    public async Task<IActionResult> ActivateAccount(ActivateAccountViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userHelper.GetUserByIdAsync(model.UserId);

            if (user != null)
            {
                string emailMessage = await _userHelper.IsEmailConfirmedAsync(user.Email) ? "Email confirmed" : "Email not confirmed";

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var resetResult = await _userHelper.ResetPasswordAsync(user, myToken, model.Password);

                if (resetResult.Succeeded)
                {
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
    public IActionResult ChangePassword()
    {
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user != null)
            {
                if (model.OldPassword == model.NewPassword)
                {
                    ViewBag.Message = "New password must be different from old";
                    return View(model);
                }

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


    public IActionResult RecoverPassword()
    {
        return View();
    }


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

            Response response = _mailHelper.SendEmail
                (
                    model.Email,
                    "Password Reset",
                    "<h3>SchoolWeb Password Reset</h3>" +
                    $"<p>Dear {user.FullName}, to reset your password click <a href = \"{link}\">here</a>.</p>" +
                    "<p>Thank you.</p>"
                );

            if (response.IsSuccess)
            {
                ViewBag.Message = "<span class=\"text-success\">Access your email account and follow the link to reset your password</span>";
            }

            return View();
        }

        return View(model);
    }


    public IActionResult ResetPassword(string token)
    {
        return View();
    }


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
                    var claims = new[]
                    {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Tokens:Issuer"],
                        _configuration["Tokens:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddDays(15),
                        signingCredentials: credentials);
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


    public IActionResult NotAuthorized()
    {
        return View();
    }
} 