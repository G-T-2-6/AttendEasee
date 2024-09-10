using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEase.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDBContext _db;
        public ManagerController(ApplicationDBContext db)
        {
            _db = db;
        }

        private User manager { get; set; }

        public Project project { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Project()
        { 
            object userid = TempData["Details"];
            int id = (int)userid;
            manager = (User)_db.Users.FirstOrDefault(u => u.UserId == id);
            project = (Project)_db.Projects.FirstOrDefault(p => p.ProjectId == manager.ProjectId);
            TempData["Details"] = id;
            return View(project);
        }

        public IActionResult Leave()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Leave(Leave leave)
        {
            leave.LeaveStatus = "Pending";
            leave.UserId = 14;
            _db.Leaves.Add(leave);
            _db.SaveChanges();
            return RedirectToAction("Index", "Manager"); 
        }

        [HttpGet]
        public IActionResult ApplyAttendance()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyAttendance([FromBody] Attendance att)
        {
            var existingAttendance = _db.Attendances
                .FirstOrDefault(x => x.UserId == att.UserId && x.Date == att.Date);

            if (existingAttendance != null)
            {
                return Conflict("Attendance for this date has already been applied.");
            }


            att.AttendanceStatus = "Pending";
            att.AccRejDate = null;
            att.AttendanceId = 0;


            _db.Attendances.Add(att);
            _db.SaveChanges();

            return Ok(new { message = "Attendance applied successfully" });
        }

        public IActionResult Employee() 
        {
            object userid = TempData["Details"];
            int id = (int)userid;
            manager = (User)_db.Users.FirstOrDefault(u => u.UserId == id);
            var managerWithSubordinates = _db.Users.Where(u => u.ManagerId == manager.UserId).ToList();
            if (manager == null || managerWithSubordinates == null)
            {
                return RedirectToAction("Errror", "Home");
            }

            TempData["Details"] = id;
            return View(managerWithSubordinates); 
        }

        [HttpGet]
        public IActionResult ViewEmployeeAttendance()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var attendanceRecords = _db.Attendances
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .OrderByDescending(x => x.Date)
                .ToList();

            if (attendanceRecords == null || !attendanceRecords.Any())
            {
                return View(new List<Attendance>());
            }

            return View(attendanceRecords);
        }

        [HttpGet]
        public IActionResult ApproveAttendance(int userId)
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            var attendanceRecords = _db.Attendances
            .Include(a => a.User)  // Include User information in the result
            .Where(a => a.User.ManagerId == userid && a.AttendanceStatus == "Pending")
            .ToList();
            return View(attendanceRecords);
        }

        [HttpPost]
        public IActionResult ApproveRejectAttendance(int userId, bool isApproved, [FromBody] Attendance att)
        {
            var check = _db.Attendances
            .Include(x => x.User)
            .FirstOrDefault(x => x.UserId == att.UserId && x.AttendanceStatus == "Pending");

            if (check == null)
            {
                return NotFound("No pending attendance record found for the specified user and project.");
            }
            check.AttendanceStatus = att.AttendanceStatus;
            check.AccRejDate = DateTime.Now;

            _db.Attendances.Update(check);
            _db.SaveChanges();
            return Ok("done");
        }
    }
}
