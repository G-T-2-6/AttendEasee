using System.ComponentModel.DataAnnotations;
namespace AttendEase.Models
{
    public class Designation
    {
        
        public int DesignationId { get; set; }

        public string Roles { get; set; }   
        public int LeaveCredit { get; set; }

        public ICollection<User> Users { get; set; }
    }
}