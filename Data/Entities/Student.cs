using System.ComponentModel.DataAnnotations;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class Student : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "Class")]
        [Required(ErrorMessage = "{0} is required")]
        public int ClassId { get; set; }

        public Class Class { get; set; }


        [Display(Name = "User ID")]
        [Required(ErrorMessage = "{0} is required")]
        public string UserId { get; set; }

        public User User { get; set; }


        //Comes from the Class entity
        [Display(Name = "Course")]
        public int CourseId { get; set; }

       
    }
}

