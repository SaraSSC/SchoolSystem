using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Absences;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.CoursesDisciplines;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Evaluations;
using SchoolSystem.Data.Qualifications;
using SchoolSystem.Data.Reports;
using SchoolSystem.Data.Students;
using SchoolSystem.Helpers.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Data
{

    /// <summary>
    /// This class propouse it's to populate some of the tables.
    /// This way we can have some data to work with when we start the application.
    /// Will be added the admin user, staff user, student user, and some roles.
    /// The qualifications and gender so we can have options to select on the comboboxes.
    /// The configuration of the maximum number of students per class and absence percentage.
    /// At least one report card.
    /// </summary>
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IGenderRepository _genderRepository;
        private readonly IQualificationRepository _qualificationRepository;
        private readonly IReportRepository _reportRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IDisciplineRepository _disciplineRepository;
        private readonly ICourseDisciplineRepository _courseDisciplineRepository;
        private readonly IClassRepository _classRepository;
        private readonly IStudentRepository _classStudentRepository;


        /* Injecting the DataContext, IUserHelper, IGenderRepository, IQualificationRepository,
         *  IReportRepository, ICourseRepository, IDisciplineRepository,
         *  ICourseDisciplineRepository, IClassRepository, IStudentRepository
         */
        public SeedDb(DataContext context,
            IUserHelper userHelper,
            IGenderRepository genderRepository,
            IQualificationRepository qualificationRepository,
            IReportRepository reportRepository,
            ICourseRepository courseRepository,
            IDisciplineRepository disciplineRepository,
            ICourseDisciplineRepository courseDisciplineRepository,
            IClassRepository classRepository,
            IStudentRepository studentRepository)
        {
            _context = context;
            _userHelper = userHelper;
            _genderRepository = genderRepository;
            _qualificationRepository = qualificationRepository;
            _reportRepository = reportRepository;
            _courseRepository = courseRepository;
            _disciplineRepository = disciplineRepository;
            _courseDisciplineRepository = courseDisciplineRepository;
            _classRepository = classRepository;
            _classStudentRepository = studentRepository;

        }

        // Run the function underneth to populate the tables asynchronusly
        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            await AddRolesAsync();
            await AddGendersAsync();
            await AddQualificationsAsync();
            await AddConfigurations();
            await AddUserAdminAsync();
            await AddUserStaffAsync();
            await AddUsersStudentsAsync();
            await AddReportsAsync();
            await AddCoursesAsync();
            await AddDisciplinesAsync();
            

        }

        // Add the roles to the database to then assign them to the users
        private async Task AddRolesAsync()
        {
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Staff");
            await _userHelper.CheckRoleAsync("Student");
        }

        // Add genders to its table
        private async Task AddGendersAsync()
        {
            await _genderRepository.AddGenderAsync("Male");
            await _genderRepository.AddGenderAsync("Female");
            await _genderRepository.AddGenderAsync("Other");
            await _genderRepository.AddGenderAsync("Not Specified");
        }

        // Add qualifications to its table
        private async Task AddQualificationsAsync()
        {
            await _qualificationRepository.AddQualificationAsync("None");
            await _qualificationRepository.AddQualificationAsync("Primary School");
            await _qualificationRepository.AddQualificationAsync("Elementary School");
            await _qualificationRepository.AddQualificationAsync("High School");
            await _qualificationRepository.AddQualificationAsync("Associate's Degree");
            await _qualificationRepository.AddQualificationAsync("Bachelor");
            await _qualificationRepository.AddQualificationAsync("Master");
            await _qualificationRepository.AddQualificationAsync("Doctorate");
        }

        // Add the configuration of the maximum number of students
        // per class and absence percentage
        private async Task AddConfigurations()
        {
            var configurations = await _context.Configurations.FirstOrDefaultAsync();

            if (configurations == null)
            {
                var configuration = new Configuration
                {
                    MaxStudentsClass = 20,
                    MaxPercentageAbsence = 5
                };

                await _context.Configurations.AddAsync(configuration);
                await _context.SaveChangesAsync();
            }
        }

        //Add password for the admin user and others made with this seeding
        private async Task AddUserWithRoleAsync(User user, string role)
        {
            var result = await _userHelper.AddUserAsync(user, "SysAdmin1!");

            if (result != IdentityResult.Success)
            {
                throw new InvalidOperationException($"Seeding could not create {role} user");
            }

            await _userHelper.AddUserToRoleAsync(user, role);

            var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, myToken);
        }

        // Check if the user is in the role, if not add it
        private async Task IsUserInRole(User user, string role)
        {
            var isUserInRole = await _userHelper.IsUserInRoleAsync(user, role);

            if (!isUserInRole)
            {
                await _userHelper.AddUserToRoleAsync(user, role);
            }
        }

        // Add the admin user to the database
        private async Task AddUserAdminAsync()
        {
            var userAdmin = await _userHelper.GetUserByEmailAsync("testemailcoder@gmail.com");

            if (userAdmin == null)
            {
                userAdmin = new User
                {
                    UserName = "testemailcoder@gmail.com",
                    FirstName = "Sara",
                    LastName = "Silva",
                    GenderId = _context.Genders.Where(x => x.Name == "Female").FirstOrDefault().Id,
                    QualificationId = _context.Qualifications.Where(x => x.Name == "Bachelor").FirstOrDefault().Id,
                    CcNumber = "1234567890",
                    BirthDate = DateTime.Today.AddYears(-27),
                    Address = "Rua X, nº6",
                    City = "Porto",
                    PhoneNumber = "+351987654321",
                    Email = "testemailcoder@gmail.com",
                    PasswordChanged = true
                };

                await AddUserWithRoleAsync(userAdmin, "Admin");
            }

            await IsUserInRole(userAdmin, "Admin");
        }

        private async Task AddUserStaffAsync()
        {
            var userStaff = await _userHelper.GetUserByEmailAsync("staff@gmail.com");

            if (userStaff == null)
            {
                userStaff = new User
                {
                    UserName = "staff@gmail.com",
                    FirstName = "Hannah",
                    LastName = "Lancost",
                    GenderId = _context.Genders.Where(x => x.Name == "Female").FirstOrDefault().Id,
                    QualificationId = _context.Qualifications.Where(x => x.Name == "Associate's Degree").FirstOrDefault().Id,
                    CcNumber = "18927365278",
                    BirthDate = DateTime.Today.AddYears(-42),
                    Address = "Rua Y, 65",
                    City = "Lisboa",
                    PhoneNumber = "+351123456789",
                    Email = "staff@gmail.com",
                    PasswordChanged = true
                };

                await AddUserWithRoleAsync(userStaff, "Staff");
            }

            await IsUserInRole(userStaff, "Staff");
        }

        private async Task AddUsersStudentsAsync()
        {
            var userStudent = await _userHelper.GetUserByEmailAsync("student1@gmail.com");

            if (userStudent == null)
            {
                userStudent = new User
                {
                    UserName = "student1@gmail.com",
                    FirstName = "João",
                    LastName = "Faria",
                    GenderId = _context.Genders.Where(x => x.Name == "Male").FirstOrDefault().Id,
                    QualificationId = _context.Qualifications.Where(x => x.Name == "High School").FirstOrDefault().Id,
                    CcNumber = "6538290168",
                    BirthDate = DateTime.Today.AddYears(-20),
                    Address = "Rua A, 1",
                    City = "Lisbon",
                    PhoneNumber = "+351243567281",
                    Email = "student1@gmail.com",
                    PasswordChanged = true
                };

                await AddUserWithRoleAsync(userStudent, "Student");
            }

            await IsUserInRole(userStudent, "Student");
        }

        private async Task AddReportsAsync()
        {
            var userStaff = await _userHelper.GetUserByEmailAsync("staff@gmail.com");
            if (userStaff != null)
            {
                if (await _reportRepository.IsReportsEmptyAsync())
                {
                    Report report1 = new Report
                    {
                        UserId = userStaff.Id,
                        Title = "Add Microsoft SQL Server Discipline",
                        Message = "Please add this discipline as soon as possible. Thank you.",
                        Date = DateTime.Today.AddDays(-6),
                        Status = true,
                        StatusDate = DateTime.Today.AddDays(-1)
                    };
                    Report report2 = new Report
                    {
                        UserId = userStaff.Id,
                        Title = "Add Python Discipline",
                        Message = "Please add this discipline as soon as possible. Thank you.",
                        Date = DateTime.Today.AddDays(-2),
                        Status = false,
                        StatusDate = DateTime.Today.AddDays(-1)
                    };

                    await _reportRepository.CreateAsync(report1);
                    await _reportRepository.CreateAsync(report2);
                }
            }
        }

        private async Task AddCoursesAsync()
        {
            if (await _courseRepository.IsCoursesEmptyAsync())
            {
                Course course1 = new Course
                {
                    Code = "FNTDEV",
                    Name = "Front-End Development",
                    Area = "Web Development",
                    Duration = 1250
                };
                await _courseRepository.CreateAsync(course1);
            }
        }

        private async Task AddDisciplinesAsync()
        {
            if (await _disciplineRepository.IsDisciplinesEmptyAsync())
            {
                Discipline discipline1 = new Discipline
                {
                    Code = "HTMLDEV",
                    Name = "HTML",
                    Area = "Web Development",
                    Duration = 150
                };

                Discipline discipline2 = new Discipline
                {
                    Code = "JSDEV",
                    Name = "Javascript",
                    Area = "Web Development",
                    Duration = 500
                };

                Discipline discipline3 = new Discipline
                {
                    Code = "CSSDEV",
                    Name = "Cascade style shets",
                    Area = "Web Development",
                    Duration = 225
                };

                await _disciplineRepository.CreateAsync(discipline1);
                await _disciplineRepository.CreateAsync(discipline2);
                await _disciplineRepository.CreateAsync(discipline3);
            }
        }
    }
}
