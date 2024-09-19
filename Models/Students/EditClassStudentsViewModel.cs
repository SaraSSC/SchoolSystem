using System.Collections.Generic;

namespace SchoolSystem.Models.Students
{
    public class EditClassStudentsViewModel
    {
        public int ClassId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Course { get; set; }

        public string ClassName => $"_>{Code}  |  {Name}  -  {Course}";

        public IEnumerable<StudentsViewModel> Students { get; set; }
    }
}

