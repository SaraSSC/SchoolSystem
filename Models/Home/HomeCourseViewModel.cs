using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Home
{
    public class HomeCourseViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Area { get; set; }

        [Display(Name = "Hours")]
        public int Duration { get; set; }
    }
}
