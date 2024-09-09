
using System.ComponentModel.DataAnnotations;
namespace AttendEase.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceStatus { get; set; }
        public DateTime? AccRejDate { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}