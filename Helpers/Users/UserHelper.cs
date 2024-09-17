using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data;
using SchoolSystem.Models.Accounts;
using SchoolSystem.Models.Students;
using SchoolSystem.Models.Users;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SchoolSystem.Models.API;
using Microsoft.EntityFrameworkCore;

namespace SchoolSystem.Helpers.Users
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;

        public UserHelper
            (
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                RoleManager<IdentityRole> roleManager,
                DataContext context
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync
                (
                    model.Username,
                    model.Password,
                    model.RememberMe,
                    true
                );
        }

        public async Task LogOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<bool> IsCcNumberInUseOnRegisterAsync(string ccNumber)
        {
            var user = await _context.Users.Where(x => x.CcNumber == ccNumber).FirstOrDefaultAsync();

            return user != null ? true : false;
        }

        public async Task<bool> IsCcNumberInUseOnEditAsync(string userId, string ccNumber)
        {
            var user = await _context.Users.Where(x => x.Id != userId && x.CcNumber == ccNumber).FirstOrDefaultAsync();

            return user != null ? true : false;
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
            }
        }

        public async Task<string> GetRoleByIdAsync(string roleId)
        {
            return await _context.Roles
                .Where(x => x.Id == roleId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<string> GetUserRoleAsync(string userId)
        {
            var roleId = _context.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            return await GetRoleByIdAsync(roleId.Result.ToString());
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, true);
        }

        public async Task<bool> IsPasswordChangedAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.PasswordChanged)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<bool> IsEmailConfirmedAsync(string username)
        {
            var user = await _userManager.FindByEmailAsync(username);

            return user.EmailConfirmed ? true : false;
        }

        public async Task<bool> ConfirmPasswordChangedAsync(string userId)
        {
            var user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            var passChanged = false;

            if (user != null)
            {
                user.PasswordChanged = true;
                await _context.SaveChangesAsync();
                passChanged = true;
            }

            return passChanged;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task<string> GetUserProfilePictureAsync(string userId)
        {
            return await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => x.ProfilePicture)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetUserPictureAsync(string userId)
        {
            return await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Picture)
                .FirstOrDefaultAsync();
        }

        public async Task DeletePictureAsync(string pictureName)
        {
            var fullPath = Path.Combine
                (
                    Directory.GetCurrentDirectory(),
                    "wwwroot\\images\\pictures",
                    pictureName
                );

            if (File.Exists(fullPath))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        File.Delete(fullPath);
                    });
                }
                catch
                {
                }
            }
        }

        public IEnumerable<SelectListItem> GetComboRoles()
        {
            var list = _context.Roles
                .Where(x => x.Name != "Student")
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select your role...)",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<EditUsersViewModel>> GetUsersListAsync()
        {
            var users = Enumerable.Empty<EditUsersViewModel>();

            await Task.Run(() =>
            {
                users =
                (
                    from user in _context.Users
                    join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId
                    join role in _context.Roles
                    on userRole.RoleId equals role.Id
                    where role.Name != "Student"
                    orderby user.FirstName
                    select new
                    {
                        Id = user.Id,
                        ProfilePicture = user.ProfilePicturePath,
                        FullName = user.FullName,
                        City = user.City,
                        Email = user.Email,
                        Role = role.Name,
                    }
                ).Select(x => new EditUsersViewModel
                {
                    Id = x.Id,
                    ProfilePicture = x.ProfilePicture,
                    FullName = x.FullName,
                    City = x.City,
                    Email = x.Email,
                    Role = x.Role
                });
            });

            return users;
        }

        public async Task<IEnumerable<Models.Users.EditStudentsUserViewModel>> GetStudentsListAsync()
        {
            var students = Enumerable.Empty<Models.Users.EditStudentsUserViewModel>();

            await Task.Run(() =>
            {
                students =
                (
                    from user in _context.Users
                    join userRole in _context.UserRoles
                    on user.Id equals userRole.UserId
                    join role in _context.Roles
                    on userRole.RoleId equals role.Id
                    where role.Name == "Student"
                    join qualification in _context.Qualifications
                    on user.QualificationId equals qualification.Id
                    orderby user.FirstName
                    select new
                    {
                        Id = user.Id,
                        ProfilePicture = user.ProfilePicturePath,
                        FullName = user.FullName,
                        BirthDate = user.BirthDate,
                        Qualification = qualification.Name,
                        City = user.City,
                        Email = user.Email
                    }
                ).Select(x => new Models.Users.EditStudentsUserViewModel
                {
                    Id = x.Id,
                    ProfilePicture = x.ProfilePicture,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    Qualification = x.Qualification,
                    City = x.City,
                    Email = x.Email
                });
            });

            return students;
        }

        public async Task DeleteUserAsync(User user)
        {
            await _userManager.DeleteAsync(user);
        }

        public async Task<APIViewModel> GetStudentViewModelAsync(string userName)
        {
            var student = new APIViewModel();

            await Task.Run(() =>
            {
                student =
                _context.Users
                .Where(x => x.UserName == userName)
                .Select(x => new APIViewModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Gender = x.Gender.Name,
                    Qualification = x.Qualification.Name,
                    CcNumber = x.CcNumber,
                    BirthDate = x.BirthDate,
                    Address = x.Address,
                    City = x.City,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    ProfilePicturePath = x.ProfilePicturePath,
                    PicturePath = x.PicturePath
                }).FirstOrDefault();
            });

            return student;
        }
    }
}
