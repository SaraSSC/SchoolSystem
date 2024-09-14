using System.ComponentModel.DataAnnotations;
using System;
using SchoolSystem.Data.Entities;
using System.Security.Principal;

namespace SchoolSystem.Data.Entities
{
    public class Class : IEntity
    {
        public int Id { get; set; }


        [MaxLength(20)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Code { get; set; }


        [MaxLength(100)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Name { get; set; }


        [Display(Name = "Course")]
        [Required(ErrorMessage = "{0} is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a Course")]
        public int CourseId { get; set; }

        public Course Course { get; set; }


        [Display(Name = "Starting Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime StartingDate { get; set; }


        [Display(Name = "Ending Date")]
        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime EndingDate { get; set; }
    }
}

