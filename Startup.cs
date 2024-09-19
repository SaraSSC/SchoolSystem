using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SchoolSystem.Data;
using SchoolSystem.Data.Absences;
using SchoolSystem.Data.Classes;
using SchoolSystem.Data.Configurations;
using SchoolSystem.Data.Courses;
using SchoolSystem.Data.CoursesDisciplines;
using SchoolSystem.Data.Disciplines;
using SchoolSystem.Data.Entities;
using SchoolSystem.Data.Evaluations;
using SchoolSystem.Data.Qualifications;
using SchoolSystem.Data.Reports;
using SchoolSystem.Data.Students;
using SchoolSystem.Helpers.Emails;
using SchoolSystem.Helpers.Transformers;
using SchoolSystem.Helpers.Users;
using SchoolWeb.Data.Students;
using SchoolWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSystem
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            /*
             *  Dependency injection for the User roles
             *  Require unique email
             *  Lockout is allowed for new users
             *  Lockout time is set to 1 minute
             */
            services.AddIdentity<User, IdentityRole>(cfg =>
            {
                cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                cfg.User.RequireUniqueEmail = true;
                cfg.Lockout.AllowedForNewUsers = true;
                cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<DataContext>();

            services.AddControllersWithViews();

            services.AddAuthentication().AddCookie().AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = this.Configuration["Tokens:Issuer"],
                    ValidAudience = this.Configuration["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Tokens:Key"]))
                };
            });

            /*
            * Dependency injection configuration for the database context
            */
            services.AddDbContext<DataContext>(cfg =>
            {
                cfg.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddTransient<SeedDb>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<IEmailHelper,EmailHelper>();
            services.AddScoped<IConverterHelper, ConverterHelper>();

            services.AddScoped<IGenderRepository, GenderRepository>();
            services.AddScoped<IQualificationRepository, QualificationRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IDisciplineRepository, DisciplineRepository>();
            services.AddScoped<ICourseDisciplineRepository, CourseDisciplineRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IAbsenceRepository, AbsenceRepository>();
            services.AddScoped<IEvaluationRepository, EvaluationRepository>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Accounts/NotAuthorized";
                options.AccessDeniedPath = "/Accounts/NotAuthorized";
            });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Errors/Error");
				app.UseHsts();
			}

			app.UseStatusCodePagesWithReExecute("/error/{0}");
		
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

          

            /*
            * Set the default code culture to pt-PT (calender, currency, etc)
            */
            CultureInfo cultureInfo = new CultureInfo("pt-PT");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        }
    }
}
