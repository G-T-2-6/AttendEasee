using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEase.Controllers
{
    //public class LeaveDisplayFormat
    //{
    //    public string EmployeeName {  get; set; }
    //    public string LeaveStatus { get; set; }
    //    public DateTime RequestDate { get; set; }
    //    public DateTime? AccRejDate { get; set; }
    //}
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

        public IActionResult ApplyLeaveManager()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyLeaveManager(Leave obj)
        {
            object userid = TempData["Details"];
            int id = (int)userid;
            manager = (User)_db.Users.FirstOrDefault(u => u.UserId == id);

            if (string.IsNullOrWhiteSpace(Request.Form["StartDate"]) || string.IsNullOrWhiteSpace(Request.Form["EndDate"]))
            {
                TempData["FailureMessage"] = "You need to enter both start date and end date.";
                return RedirectToAction("ViewLeaveManager");
            }

            DateTime startDate = DateTime.Parse(Request.Form["StartDate"]);
            DateTime endDate = DateTime.Parse(Request.Form["EndDate"]);

            if(endDate < startDate)
            {
                TempData["FailureMessage"] = "End Date cannot be before Start Date";
                return RedirectToAction("ViewLeaveManager");   
            }

            int totalDays = (endDate - startDate).Days + 1;

            if(totalDays > manager.AvailableLeavesCount)
            {
                TempData["FailureMessage"] = "Requested Days cannot be more than your leave credit";
                return RedirectToAction("ViewLeaveManager");
            }

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                Leave leave = new Leave();
                leave.LeaveStatus = "Pending";
                leave.UserId = manager.UserId;
                leave.RequestDate = date;
                _db.Leaves.Add(leave);
            }
            _db.SaveChanges();
            TempData["Details"] = id;
            TempData["SuccessMessage"] = "Leave Added Successfully";
            return RedirectToAction("ViewLeaveManager");
        }

        public IActionResult ApproveLeaveManager()
        {
            object userid = TempData["Details"];
            int id = (int)userid;
            manager = (User)_db.Users.FirstOrDefault(u => u.UserId == id);
            int managerId = manager.UserId;

            List<Leave> employeeLeaves = (from user in _db.Users
                                          join leave in _db.Leaves on user.UserId equals leave.UserId
                                          where user.ManagerId == managerId && leave.LeaveStatus=="Pending"
                                          select leave).ToList();

            TempData["Details"] = id;
            return View(employeeLeaves);
        }

        public IActionResult ViewLeaveManager()
        {
            object userid = TempData["Details"];
            int id = (int)userid;
            manager = (User)_db.Users.FirstOrDefault(u => u.UserId == id);
            var managerId = manager.UserId;

            List<Leave> employeeLeaves = _db.Leaves.Where(l => l.UserId == manager.UserId).ToList();

            TempData["Details"] = id;

            return View(employeeLeaves);
        }

        [HttpPost("ApproveLeave/{id}")]
        public IActionResult ApproveLeave(int id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }

            var leaveObj = _db.Leaves.Find(id);
            if (leaveObj == null) return NotFound();
            leaveObj.LeaveStatus = "Approved";
            leaveObj.AccRejDate = DateTime.Now;

            var userObj = _db.Users.FirstOrDefault(u => u.UserId == leaveObj.UserId);
            userObj.AvailableLeavesCount--;

            _db.Leaves.Update(leaveObj);
            _db.Users.Update(userObj);
            _db.SaveChanges();

            return RedirectToAction("ApproveLeaveManager");
        }

        [HttpPost("RejectLeave/{id}")]
        public IActionResult RejectLeave(int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var leaveObj = _db.Leaves.Find(id);
            if (leaveObj == null) return NotFound();
            leaveObj.LeaveStatus = "Rejected";
            leaveObj.AccRejDate = DateTime.Now;

            _db.Leaves.Update(leaveObj);
            _db.SaveChanges();

            return RedirectToAction("ApproveLeaveManager");
        }
    }
}
