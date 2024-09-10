using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEasee.Controllers
{
    public class DeveloperController : Controller
    {
        private readonly ApplicationDBContext _db;
        public DeveloperController(ApplicationDBContext db) {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public User developer { get; set; }

        public IActionResult ApplyLeaveDeveloper()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyLeaveDeveloper(Leave obj)
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            int id = (int)userid;
            developer = (User)_db.Users.FirstOrDefault(u => u.UserId == id);

            if (string.IsNullOrWhiteSpace(Request.Form["StartDate"]) || string.IsNullOrWhiteSpace(Request.Form["EndDate"]))
            {
                TempData["FailureMessage"] = "You need to enter both start date and end date.";
                return RedirectToAction("ViewLeaveDeveloper");
            }

            DateTime startDate = DateTime.Parse(Request.Form["StartDate"]);
            DateTime endDate = DateTime.Parse(Request.Form["EndDate"]);

            if (endDate < startDate)
            {
                TempData["FailureMessage"] = "End Date cannot be before Start Date";
                return RedirectToAction("ViewLeaveDeveloper");
            }

            int totalDays = (endDate - startDate).Days + 1;

            if (totalDays > developer.AvailableLeavesCount)
            {
                TempData["FailureMessage"] = "Requested Days cannot be more than your leave credit";
                return RedirectToAction("ViewLeaveDeveloper");
            }

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                Leave leave = new Leave();
                leave.LeaveStatus = "Pending";
                leave.UserId = developer.UserId;
                leave.RequestDate = date;
                _db.Leaves.Add(leave);
            }
            _db.SaveChanges();
            TempData["SuccessMessage"] = "Leave Added Successfully";
            return RedirectToAction("ViewLeaveDeveloper");
        }
        public IActionResult ViewLeaveDeveloper()
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            int id = (int)userid;
            developer = (User)_db.Users.FirstOrDefault(u => u.UserId == id);
            List<Leave> leaves = _db.Leaves.Where(l => l.UserId == developer.UserId).ToList();
            return View(leaves);
        }

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

            return RedirectToAction("ViewAttendanceDeveloper","Developer");
        }

        public IActionResult ViewAttendanceDeveloper()
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
    }
}
