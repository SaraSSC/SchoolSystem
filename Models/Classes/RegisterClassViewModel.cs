using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Data.Entities;
using System.Collections.Generic;

namespace SchoolSystem.Models.Classes
{
    public class RegisterClassViewModel : Class
    {
        public IEnumerable<SelectListItem> Courses { get; set; }
    }
}

