using SchoolSystem.Data.Entities;
using System;

namespace SchoolSystem.Models.Reports
{
    public class ReportsViewModel : Report
    {
        public string Age => $"{(DateTime.Today - Date).Days} Days";
    }
}
