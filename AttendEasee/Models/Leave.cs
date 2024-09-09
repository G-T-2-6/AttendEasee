using System.ComponentModel.DataAnnotations;
namespace AttendEase.Models
{
    public class Leave
    {
        public int LeaveId { get; set; }
        public string LeaveStatus { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? AccRejDate { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}