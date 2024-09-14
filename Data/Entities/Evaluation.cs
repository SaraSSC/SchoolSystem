using System.ComponentModel.DataAnnotations;
using System;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class Evaluation : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "User ID")]
        [Required(ErrorMessage = "{0} is required")]
        public string UserId { get; set; }

        public User User { get; set; }


        [Display(Name = "Class")]
        [Required(ErrorMessage = "{0} is required")]
        public int ClassId { get; set; }

        public Class Class { get; set; }


        [Display(Name = "Discipline")]
        [Required(ErrorMessage = "{0} is required")]
        public int DisciplineId { get; set; }

        public Discipline Discipline { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Date { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Grade { get; set; }
    }
}
