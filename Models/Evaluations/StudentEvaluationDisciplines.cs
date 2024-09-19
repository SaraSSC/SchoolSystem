using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Evaluations
{
    public class StudentEvaluationDisciplines
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int Duration { get; set; }

        [Display(Name = "Absence")]
        public int HoursAbsence { get; set; }

        [Display(Name = "%")]
        public int PercentageAbsence { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? Date { get; set; }

        public int? Grade { get; set; }

        public bool FailedAbsence { get; set; }

        public bool FailedGrade { get; set; }

        /*
         *This property is used to change the background color of the row in the view to red
         */
        public string BackgroundColor => FailedAbsence || FailedGrade ? "background-color: #FFF2F2" : "";


    }
}
