using System.ComponentModel.DataAnnotations;
using SchoolSystem.Data.Entities;

namespace SchoolSystem.Data.Entities
{
    public class Discipline : IEntity
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


        [MaxLength(50)]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} must be {2} characters minimum and {1} characters maximum")]
        public string Area { get; set; }


        [Required(ErrorMessage = "{0} is required")]
        [Range(1, 1000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Duration { get; set; }
    }
}
