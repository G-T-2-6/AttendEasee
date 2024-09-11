using System.ComponentModel.DataAnnotations;

namespace AttendEase.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? AvailableLeavesCount { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? IsManager { get; set; }

        public int? DesignationId { get; set; }
        public Designation Designation { get; set; }

        public int? ProjectId { get; set; }
        public Project Project { get; set; }

        public int? ManagerId { get; set; }
        public User Manager { get; set; }
        public ICollection<User> Subordinates { get; set; }

        public ICollection<Leave> Leaves { get; set; }
        public ICollection<Attendance> Attendances { get; set; }

    }
}
