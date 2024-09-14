using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data.Entities;
using System.Linq;
using SchoolSystem.Models.API;
namespace SchoolSystem.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Absence> Absences { get; set; }

        public DbSet<Class> Classes { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Configuration> Configurations { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<CourseDiscipline> CourseDisciplines { get; set; }

        public DbSet<Discipline> Disciplines { get; set; }

        public DbSet<Evaluation> Evaluations { get; set; }

        public DbSet<Gender> Genders { get; set; }

        public DbSet<Qualification> Qualifications { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * Configure the primary key for the entities
             */
            modelBuilder.Entity<Class>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Discipline>()
                .HasIndex(x => x.Code)
                .IsUnique();

            /*
             * Enable cascade delete for the entities
             */

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(x => x.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SchoolSystem.Models.API.APIViewModel> APIViewModel { get; set; }
    }
}


