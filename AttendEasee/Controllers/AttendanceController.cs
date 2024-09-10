using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEase.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDBContext _dbcontext;

        public AttendanceController(ApplicationDBContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ApplyAttendance()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyAttendance([FromBody] Attendance att)
        {
            
            var existingAttendance = _dbcontext.Attendances
                .FirstOrDefault(x => x.UserId == att.UserId && x.Date == att.Date);

            if (existingAttendance != null)
            {
                return Conflict("Attendance for this date has already been applied.");
            }

            
            att.AttendanceStatus = "Pending";
            att.AccRejDate = null; 
            att.AttendanceId = 0;  

            
            _dbcontext.Attendances.Add(att);
            _dbcontext.SaveChanges();

            return Ok(new { message = "Attendance applied successfully" });
        }
        [HttpGet]
        public IActionResult ViewEmployeeAttendance(int userId)
        {
            var attendanceRecords = _dbcontext.Attendances
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .ToList();

            if (attendanceRecords == null || !attendanceRecords.Any())
            {
                return View(new List<Attendance>());
            }

            return View(attendanceRecords);
        }



        public IActionResult ViewAllAttendance()
        {
            var attendanceRecords = _dbcontext.Attendances.Include(x => x.User).Where(x => x.AttendanceStatus == "Pending").ToList();
            return View(attendanceRecords);
        }


        [HttpPost]
        public IActionResult ApproveRejectAttendance(int userId, bool isApproved, [FromBody] Attendance att)
        {
           var check = _dbcontext.Attendances
           .Include(x => x.User)
           .FirstOrDefault(x => x.UserId == att.UserId && x.AttendanceStatus == "Pending");

            if (check == null)
            {
                return NotFound("No pending attendance record found for the specified user and project.");
            }
            check.AttendanceStatus = att.AttendanceStatus;
            check.AccRejDate = DateTime.Now;

            _dbcontext.Attendances.Update(check);
            _dbcontext.SaveChanges();
            return Ok("done");
        }



    }
}