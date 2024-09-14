using System.ComponentModel.DataAnnotations;


namespace SchoolSystem.Data.Entities
{
        public class CourseDiscipline : IEntity
        {
            public int Id { get; set; }


            [Display(Name = "Course")]
            [Required(ErrorMessage = "{0} is required")]
            public int CourseId { get; set; }

            public Course Course { get; set; }


            [Display(Name = "Discipline")]
            [Required(ErrorMessage = "{0} is required")]
            public int DisciplineId { get; set; }

            public Discipline Discipline { get; set; }
        }
}



