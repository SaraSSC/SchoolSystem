using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseDisciplines
{
    public class DisciplinesSelectable
    {
        [Required]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        public int Duration { get; set; }

        [Display(Name = "Selected")]
        public bool IsSelected { get; set; }
    }
}

