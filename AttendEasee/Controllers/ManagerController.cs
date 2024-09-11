using AttendEase.Filters;
using AttendEase.Data;
using AttendEase.Models;
using AttendEasee.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AttendEase.Controllers
{
    //public class LeaveDisplayFormat
    //{
    //    public string EmployeeName {  get; set; }
    //    public string LeaveStatus { get; set; }
    //    public DateTime RequestDate { get; set; }
    //    public DateTime? AccRejDate { get; set; }
    //}
    [SessionCheck]
    [RoleCheck("Manager")]
    public class ManagerController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;

        public ManagerController(IAttendanceRepository attendanceRepository, 
            ILeaveRepository leaveRepository, 
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _attendanceRepository = attendanceRepository;
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _webHostEnvironment = webHostEnvironment;
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
            var userid = HttpContext.Session.GetInt32("UserId");
            manager = _userRepository.GetUserById(userid ?? 0);
            project = _projectRepository.GetProjectByProjectId((int)manager.ProjectId);
            return View(project);
        }

        public IActionResult Leave()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Leave(Leave leave)
        //{
        //    leave.LeaveStatus = "Pending";
        //    leave.UserId = (int)HttpContext.Session.GetInt32("UserId");
        //    _db.Leaves.Add(leave);
        //    _db.SaveChanges();
        //    return RedirectToAction("Index", "Manager"); 
        //}

        [HttpGet]
        public IActionResult ApplyAttendance()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyAttendance([FromBody] Attendance att)
        {
            var existingAttendance = _attendanceRepository.GetAttendanceByUserIdAndDate(att.UserId, att.Date);

            if (existingAttendance != null)
            {
                return Conflict("Attendance for this date has already been applied.");
            }


            att.AttendanceStatus = "Pending";
            att.AccRejDate = null;
            att.AttendanceId = 0;


            _attendanceRepository.AddAttendance(att);
            _attendanceRepository.Save();

            return Ok(new { message = "Attendance applied successfully" });
        }

        public IActionResult Employee() 
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            manager = _userRepository.GetUserById(userid ?? 0);

            var managerWithSubordinates = _userRepository.GetManagerSubordinates(manager.UserId);
            if (manager == null || managerWithSubordinates == null)
            {
                return RedirectToAction("Errror", "Home");
            }

            return View(managerWithSubordinates); 
        }



        public IActionResult ChangePassword()

        {
            return View();
        }

        [HttpPost]

        public IActionResult ChangePassword(int? id)

        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (string.IsNullOrWhiteSpace(Request.Form["OldPassword"]) || string.IsNullOrWhiteSpace(Request.Form["NewPassword"]))
            {
                TempData["FailureMessage"] = "You need to enter both old and new passwords";
                return RedirectToAction("ChangePassword", "Manager");
            }

            string oldPassword = Request.Form["OldPassword"].ToString();
            string oldPasswordHash = _userRepository.HashPassword(oldPassword);
            var userObj = _userRepository.GetUserById(userId ?? 0);

            if (userObj == null)

            {
                return NotFound();
            }

            if (userObj.Password != oldPasswordHash)

            {

                TempData["FailureMessage"] = "Old passwords do not match";

                return RedirectToAction("ChangePassword", "Manager");

            }

            string newPassword = Request.Form["NewPassword"].ToString();
            string newPasswordHash = _userRepository.HashPassword(newPassword);
            if (userObj.Password == newPasswordHash)

            {
                TempData["FailureMessage"] = "Old and new passwords cannot be same";
                return RedirectToAction("ChangePassword", "Manager");
            }
            userObj.Password = newPasswordHash;
            _userRepository.UpdateUser(userObj);
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index", "Manager");

        }



        public IActionResult ApplyLeaveManager()
        {
            return View();
        }

        [HttpPost]    
        public async Task<IActionResult> ApplyLeaveManager(Leave leave, IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["FailureMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Index", "Home");
            }

            manager = _userRepository.GetUserById(userId ?? 0);

            if (manager == null)
            {
                TempData["FailureMessage"] = "Manager not found. Please try again.";
                return RedirectToAction("ViewLeaveManager");
            }

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

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                leave.FilePath = "/uploads/" + uniqueFileName;
            }

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                
                
                Leave newLeave = new Leave()
                {
                    LeaveStatus = "Pending",
                    UserId = manager.UserId,
                    RequestDate = date,
                    Description = leave.Description,
                    Address = leave.Address,
                    FilePath = leave.FilePath
                };
                _leaveRepository.AddLeave(newLeave);
            }
            _leaveRepository.Save();
            TempData["SuccessMessage"] = "Leave Added Successfully";
            return RedirectToAction("ViewLeaveManager");
        }

        public IActionResult ApproveLeaveManager()
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            manager = _userRepository.GetUserById(userid ?? 0);
            int managerId = manager.UserId;

            List<Leave> employeeLeaves = _leaveRepository.GetPendingLeavesByManagerId(managerId);

            return View(employeeLeaves);
        }

        public IActionResult ViewLeaveManager()
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            manager = _userRepository.GetUserById(userid ?? 0);
            var managerId = manager.UserId;
            List<Leave> employeeLeaves = _leaveRepository.GetEmployeeLeaves(managerId);
            return View(employeeLeaves);
        }

        [HttpPost("ApproveLeave/{id}")]
        public IActionResult ApproveLeave(int id)
        {
            if(id == 0 || id == null)
            {
                return NotFound();
            }

            var leaveObj = _leaveRepository.GetLeaveById(id);
            if (leaveObj == null) return NotFound();
            leaveObj.LeaveStatus = "Approved";
            leaveObj.AccRejDate = DateTime.Now;

            var userObj = _userRepository.GetUserById(leaveObj.UserId);
            userObj.AvailableLeavesCount--;

            _leaveRepository.UpdateLeave(leaveObj);
            _userRepository.UpdateUser(userObj);

            return RedirectToAction("ApproveLeaveManager");
        }

        [HttpPost("RejectLeave/{id}")]
        public IActionResult RejectLeave(int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var leaveObj = _leaveRepository.GetLeaveById(id);
            if (leaveObj == null) return NotFound();
            leaveObj.LeaveStatus = "Rejected";
            leaveObj.AccRejDate = DateTime.Now;

            _leaveRepository.UpdateLeave(leaveObj);

            return RedirectToAction("ApproveLeaveManager");
        }

        [HttpGet]
        public IActionResult ViewEmployeeAttendance()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var attendanceRecords = _attendanceRepository.GetAttendanceByUserId(userId ?? 0);

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
            var attendanceRecords = _attendanceRepository.GetPendingAttendanceByManager(userId);
            return View(attendanceRecords);
        }

        [HttpPost]
        public IActionResult ApproveRejectAttendance(int userId, bool isApproved, [FromBody] Attendance att)
        {
            var check = _attendanceRepository.GetPendingAttendance(userId);

            if (check == null)
            {
                return NotFound("No pending attendance record found for the specified user and project.");
            }
            check.AttendanceStatus = att.AttendanceStatus;
            check.AccRejDate = DateTime.Now;

            _attendanceRepository.UpdateAttendance(check);
            return Ok("done");
        }
    }
}
