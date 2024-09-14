using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Evaluations
{
    public class EvaluationClassesViewModel
    {
        [Display(Name = "Class")]
        [Range(1, int.MaxValue, ErrorMessage = "Select Class")]
        [Required(ErrorMessage = "{0} is required")]
        public int ClassId { get; set; }

        public IEnumerable<SelectListItem> Classes { get; set; }
    }
}
