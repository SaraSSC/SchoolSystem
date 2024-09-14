using SchoolSystem.Data.Entities;
using System.Collections.Generic;

namespace SchoolSystem.Models.CourseDisciplines
{
    public class CourseDisciplinesViewModel : Course
    {
        public string CodeName => (!string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(Name)) ? $"_>{Code} | {Name}" : string.Empty;

        public IEnumerable<Discipline> Disciplines { get; set; }
    }
}

