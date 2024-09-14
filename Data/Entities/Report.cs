using System.ComponentModel.DataAnnotations;
using System;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class Report : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "User ID")]
        [Required(ErrorMessage = "{0} is required")]
        public string UserId { get; set; }

        public User User { get; set; }


        [MaxLength(100)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Title { get; set; }


        [MaxLength(8000)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(8000, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Message { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Date { get; set; }


        public bool? Status { get; set; }


        [Display(Name = "Solved Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? StatusDate { get; set; }
    }
}
