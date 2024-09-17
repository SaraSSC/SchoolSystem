using System.ComponentModel.DataAnnotations;
using System;

namespace SchoolSystem.Models.Home
{
    public class HomeClassViewModel
    {
        public string Name { get; set; }

        public string Course { get; set; }

        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime EndDate { get; set; }
    }
}
