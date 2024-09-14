using System.ComponentModel.DataAnnotations;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class Qualification : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "Qualification")]
        [MaxLength(100)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Name { get; set; }
    }
}
