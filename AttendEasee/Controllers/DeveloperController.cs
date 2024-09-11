    using AttendEase.Data;
using AttendEase.Filters;
using AttendEasee.Migrations;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEasee.Controllers
{
    [SessionCheck]
    [RoleCheck("Developer")]
    public class DeveloperController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IAttendanceRepository _attendanceRepository;

        public DeveloperController(IUserRepository userRepository, 
            ILeaveRepository leaveRepository, 
            IAttendanceRepository attendanceRepository)
        {
            _userRepository = userRepository;
            _leaveRepository = leaveRepository;
            _attendanceRepository = attendanceRepository;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var logged = _userRepository.GetUserById(userId ?? 0);
            return View(logged);
        }

        public IActionResult ApplyLeaveDeveloper()
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            int id = (int)userid;
            var developer = _userRepository.GetUserById(id);
            return View(developer);
        }

        [HttpPost]
        public IActionResult ApplyLeaveDeveloper(Leave obj)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return NotFound();

            var developer = _userRepository.GetUserById((int)userId);

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
            int appliedLeaves = _leaveRepository.GetEmployeeLeaves(userId ?? 0).Count();

            if (totalDays + appliedLeaves > developer.AvailableLeavesCount)
            {
                TempData["FailureMessage"] = "Requested Days cannot be more than your leave credit";
                return RedirectToAction("ViewLeaveDeveloper");
            }

            bool added = false;

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var find = _leaveRepository.GetLeaveByDateAndUserId(date, developer.UserId);
                if(find.Count>0)
                {
                    continue;
                }
                Leave leave = new Leave();
                leave.LeaveStatus = "Pending";
                leave.UserId = developer.UserId;
                leave.RequestDate = date;
                _leaveRepository.AddLeave(leave);
                added = true;
            }
            if (added == false)
            {
                TempData["FailureMessage"] = "Leave Requests already exist for all the entered dates";
            }
            _leaveRepository.Save();
            TempData["SuccessMessage"] = "Leave Added Successfully";
            return RedirectToAction("ViewLeaveDeveloper");
        }


        [HttpGet]
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
                return RedirectToAction("ChangePassword", "Developer");
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

                return RedirectToAction("ChangePassword", "Developer");

            }

            string newPassword = Request.Form["NewPassword"].ToString();
            string newPasswordHash = _userRepository.HashPassword(newPassword);
            if (userObj.Password == newPasswordHash)

            {
                TempData["FailureMessage"] = "Old and new passwords cannot be same";
                return RedirectToAction("ChangePassword", "Developer");
            }
            userObj.Password = newPasswordHash;
            _userRepository.UpdateUser(userObj);
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index", "Developer");

        }




        public IActionResult ViewLeaveDeveloper()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var leaves = _leaveRepository.GetLeavesByUserId(userId ?? 0);
            return View(leaves);
        }

        public IActionResult ApplyAttendance()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApplyAttendance([FromBody] Attendance att)
        {
            var existingAttendance = _attendanceRepository.GetAttendanceByDate(att.UserId, att.Date);

            if (existingAttendance != null)
            {
                return Conflict("Attendance for this date has already been applied.");
            }

            att.AttendanceStatus = "Pending";
            att.AccRejDate = null;
            _attendanceRepository.AddAttendance(att);
            _attendanceRepository.Save();

            return RedirectToAction("ViewAttendanceDeveloper");
        }

        public IActionResult ViewAttendanceDeveloper()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var attendanceRecords = _attendanceRepository.GetAttendanceByUserId(userId ?? 0);
            return View(attendanceRecords);
        }
    }
}
