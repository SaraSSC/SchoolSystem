using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Evaluations
{
    public class EvaluationViewModel
    {
        [Display(Name = "Code")]
        public string DisciplineCode { get; set; }

        [Display(Name = "Discipline")]
        public string DisciplineName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Date { get; set; }

        public int Grade { get; set; }
    }
}
