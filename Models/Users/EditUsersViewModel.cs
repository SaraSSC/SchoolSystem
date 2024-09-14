using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Users
{
    public class EditUsersViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Picture")]
        public string ProfilePicture { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }

        public string City { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }
}
