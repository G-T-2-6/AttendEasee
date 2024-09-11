using AttendEase.Controllers;
using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using AttendEase.Filters;

namespace AttendEasee.Controllers
{
    [SessionCheck]
    [RoleCheck("Admin")]
    public class AdminController : Controller
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDesignationRepository _designationRepository;

        public AdminController(IAttendanceRepository attendanceRepository,
            ILeaveRepository leaveRepository,
            IUserRepository userRepository,
            IProjectRepository projectRepository,
            IDesignationRepository designationRepository)
        {
            _attendanceRepository = attendanceRepository;
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _designationRepository = designationRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewEmployee()
        {
            var list = _userRepository.GetNonAdminUsers();
            if (list != null)
            {
                return View(list);
            }
            return View();
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            var designations = _designationRepository.GetDesignationRolesString();

            var projectId = _projectRepository.GetProjectsIds();

            ViewBag.ProjectIds = projectId;

            return View(designations);
        }

        [HttpPost]
        public IActionResult AddEmployee([FromForm] User user, string ProjectCode)
        {
            string password = "@123";
            user.Password = _userRepository.HashPassword(password);

            if (_userRepository.IsUserExists(user.Email))
            {
                TempData["EmailExistsWarning"] = true;
                return RedirectToAction("Add");
            }

            var project = _projectRepository.GetProjectByProjectCode(ProjectCode);
            if (project != null)
            {
                user.ProjectId = project.ProjectId;
            }

            _userRepository.AddUser(user);
            TempData["AddEmployeeSuccess"] = true;
            return RedirectToAction("ViewEmployee", "Admin");
        }

        [HttpGet]
        public IActionResult ModifyEmployee()
        {
            var designations = _designationRepository.GetDesignationRolesString();
            var users = _userRepository.GetNonAdminUsers();

            ViewBag.Designations = designations;
            ViewBag.Users = users;

            return View(designations);
        }

        [HttpPut]
        public IActionResult UpdateEmployee([FromForm] User updatedUser)
        {
            var user = _userRepository.GetUserById(updatedUser.UserId);


            if (user == null)
            {
                return Json(new { success = false, message = "User does not exist" });
            }

            // Update user details
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.DesignationId = updatedUser.DesignationId;
            user.IsManager = updatedUser.IsManager;

            _userRepository.save();

            return Json(new { success = true, message = "User details updated successfully." });
        }


        [HttpPut]
        public IActionResult DeleteEmployee([FromForm] User user)
        {
            var fetched = _userRepository.GetUserById(user.UserId);
            Console.WriteLine("======DANDANDAN1==========");

            if (fetched != null)    
            {
                Console.WriteLine("======DANDANDAN==========");
                // User exists
                _userRepository.RemoveUser(fetched);
                return Json(new { success = true });
            }
            else
            {
                // User does not exist
                return Json(new { success = false, message = "User does not exist" });
            }
        }



        [HttpGet]
        public IActionResult AssignProject()
        {
            var projects = _projectRepository.GetAllByProjectCode();
            var userId = _userRepository.GetNonAdminUsers();
            ViewBag.Projects = projects;
            ViewBag.UserIds = userId;
            return View();
        }

        [HttpPost]
        public IActionResult AssignProject(string UserId, string ProjectCode)
        {

            var user = _userRepository.GetUserById(int.Parse(UserId));

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (ProjectCode != "N/A")
            {
                var project = _projectRepository.GetProjectByProjectCode(ProjectCode);
                if (project != null)
                {
                    user.ProjectId = project.ProjectId;
                }
            }

            _userRepository.save();

            TempData["ProjectAssignSuccess"] = true;
            return RedirectToAction("ViewEmployee", "Admin");
        }

       




        //----------------------------EmployeeController Done-----------------


        [HttpGet]
        public IActionResult ViewProject()
        {
            var list = _projectRepository.GetAllProjects();
            return View(list);
        }

        public IActionResult AddProject()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProject([FromForm] Project project)
        {
            project.ProjectId = Convert.ToInt32(project.ProjectCode);
            _projectRepository.AddProject(project);
            TempData["AddProjectSuccess"] = true;
            return RedirectToAction("ViewProject", "Admin");
        }

        [HttpGet]
        public IActionResult ModifyProject()
        {
            var projectIds = _projectRepository.GetAllByProjectCode();
            return View(projectIds);
        }

        [HttpPut]
        public IActionResult UpdateProject([FromForm] Project project)
        {
            var fetched = _projectRepository.GetProjectByProjectCode(project.ProjectCode);

            if (fetched != null)
            {
                if (_projectRepository.UpdateProject(fetched, project.Name, project.Location))
                {
                    return Json(new { success = true });
                }
                else
                {
                    return View("Modify");
                }
            }
            else
            {
                return View("Modify");
            }
        }


        [HttpPut]
        public IActionResult DeleteProject([FromForm] Project project)
        {
            if (_projectRepository.RemoveProjectByProjectCode(project.ProjectCode))
            {
                return Json(new { success = true });
            }
            else
            {
                return View("Modify");
            }
        }

        //------------------------------------------ProjectController Done------------------------------------------


        public IActionResult ManageLeave()
        {
            var leaves = _leaveRepository.GetPendingLeaves();
            return View(leaves);
        }

        [HttpPost]
        public IActionResult UpdateLeave(int id, [FromBody] Leave updatedLeave)
        {
            if (id != updatedLeave.LeaveId)
            {
                return BadRequest("ID mismatch.");
            }

            var leave = _leaveRepository.GetLeaveById(id);
            if (leave == null)
            {
                return NotFound("Leave request not found.");
            }

            leave.LeaveStatus = updatedLeave.LeaveStatus;
            leave.AccRejDate = DateTime.Now;

            try
            {
                _leaveRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_leaveRepository.LeaveExists(id))
                {
                    return NotFound("Leave request not found.");
                }
                throw;
            }

            return Ok("Leave status updated.");
        }

        


        [HttpPost]
        public IActionResult AcceptRejectLeave([FromBody] Leave data)
        {


            if (data == null || string.IsNullOrEmpty(data.LeaveStatus))
            {
                return BadRequest("Status is required.");
            }

            var leaves = _leaveRepository.GetAllLeaves();
            foreach (var leave in leaves)
            {
                leave.LeaveStatus = data.LeaveStatus;
                leave.AccRejDate = DateTime.Now;
            }
            _leaveRepository.Save();

            return Ok("Leaves Updated");
        }

        //--------------------------------------LeaveController Done--------------------------------------

        public IActionResult ManageAttendance()
        {
            var attendanceRecords = _attendanceRepository.GetAllPendingAttendances();
            return View(attendanceRecords);
        }


        [HttpPost]
        public IActionResult ApproveRejectAttendance([FromBody] Attendance att)
        {
            var check = _attendanceRepository.GetPendingAttendance(att.UserId);

            if (check == null)
            {
                return NotFound("No pending attendance record found for the specified user and project.");
            }
            check.AttendanceStatus = att.AttendanceStatus;
            check.AccRejDate = DateTime.Now;

            _attendanceRepository.UpdateAttendance(check);
            return Ok("done");
        }

        [HttpGet]
        public IActionResult AttendanceStatus(int userId)
        {
            var attendanceRecords = _attendanceRepository.GetAttendanceByUserId(userId);

            if (attendanceRecords == null || !attendanceRecords.Any())
            {
                return View(new List<Attendance>());
            }

            return View(attendanceRecords);
        }

    }

    

    
}
