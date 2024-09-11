using AttendEase.Controllers;
using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace AttendEasee.Controllers
{
    public class AdminController : Controller
    {

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {

                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private readonly ApplicationDBContext _dbContext;

        public AdminController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewEmployee()
        {
            var list = _dbContext.Users.Where(user => user.IsAdmin != true).ToList();
            if (list != null)
            {
                return View(list);
            }
            return View();
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            var designations = _dbContext.Designations
            .Select(d => d.DesignationId + "-" + d.Roles)
            .ToList();

            var projectId = _dbContext.Projects
            .Select(p => p.ProjectId)
            .ToList();


            ViewBag.ProjectIds = projectId;

            return View(designations);

        }

        [HttpPost]
        public IActionResult AddEmployee([FromForm] User user, string ProjectCode)
        {
            string password = "@123";
            user.Password = AdminController.HashPassword(password);

            if (!ModelState.IsValid)
            {
                // Capture validation errors
                var validationErrors = ModelState.Values.SelectMany(v => v.Errors)
                                                        .Select(e => e.ErrorMessage)
                                                        .ToList();

                // Store validation messages in TempData for use in the View
                TempData["ValidationErrors"] = validationErrors;
                return RedirectToAction("Add");
            }

            user.Password = "@123";
            if (_dbContext.Users.Any(u => u.Email == user.Email))
            {
                TempData["EmailExistsWarning"] = true;
                return RedirectToAction("Add");
            }
            var project = _dbContext.Projects.FirstOrDefault(p => p.ProjectCode == ProjectCode);
                if (project != null)
                {
                    user.ProjectId = project.ProjectId;
                
            }

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            TempData["AddSuccess"] = true;
            return RedirectToAction("ViewEmployee", "Admin");
        }

        [HttpGet]
        public IActionResult ModifyEmployee()
        {
            var designations = _dbContext.Designations
            .Select(d => d.DesignationId + "-" + d.Roles)
            .ToList();

            var users = _dbContext.Users.Where(u => u.IsAdmin != true).ToList();

            ViewBag.Designations = designations;
            ViewBag.Users = users;

            return View(designations);
        }

        [HttpPut]
        public IActionResult UpdateEmployee([FromForm] User updatedUser)
        {

            var user = _dbContext.Users.SingleOrDefault(u => u.UserId == updatedUser.UserId);

            if (user == null)
            {
                return Json(new { success = false, message = "User does not exist" });
            }

            // Update user details
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.DesignationId = updatedUser.DesignationId;
            user.IsManager = updatedUser.IsManager;

            _dbContext.SaveChanges();

            return Json(new { success = true, message = "User details updated successfully." });
        }


        [HttpPut]
        public IActionResult DeleteEmployee([FromForm] User user)
        {
            var fetched = _dbContext.Users.SingleOrDefault(p => p.UserName == user.UserName);
            if (fetched != null)
            {
                // User exists
                _dbContext.Users.Remove(fetched);
                _dbContext.SaveChanges();
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
            var projects = _dbContext.Projects.Select(p => p.ProjectCode).ToList();

            var userId = _dbContext.Users.Where(u => u.IsAdmin != true).ToList();

            ViewBag.Projects = projects;
            ViewBag.UserIds = userId;

            return View();
        }

        [HttpPost]
        public IActionResult AssignProject(string UserId, string ProjectCode)
        {

            var user = _dbContext.Users.SingleOrDefault(u => u.UserId.ToString() == UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (ProjectCode != "N/A")
            {
                var project = _dbContext.Projects.FirstOrDefault(p => p.ProjectCode == ProjectCode);
                if (project != null)
                {
                    user.ProjectId = project.ProjectId;
                }
            }

            _dbContext.SaveChanges();

            TempData["ProjectAssignSuccess"] = true;
            return RedirectToAction("Index", "Employee");
        }

       




        //----------------------------EmployeeController Done-----------------


        [HttpGet]
        public IActionResult ViewProject()
        {
            var list = _dbContext.Projects.ToList();
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
            _dbContext.Projects.Add(project);
            _dbContext.SaveChanges();
            TempData["AddSuccess"] = true;
            return RedirectToAction("Index", "Project");
        }

        [HttpGet]
        public IActionResult ModifyProject()
        {
            var projectIds = _dbContext.Projects
                            .Select(p => p.ProjectCode)
                            .ToList();
            return View(projectIds);
        }

        [HttpPut]
        public IActionResult UpdateProject([FromForm] Project project)
        {
            var fetched = _dbContext.Projects.SingleOrDefault(p => p.ProjectCode == project.ProjectCode);
            if (fetched != null)
            {
                bool isUpdated = false;
                if (project.Name != null)
                {
                    fetched.Name = project.Name;
                    isUpdated = true;
                }

                if (project.Location != null)
                {
                    fetched.Location = project.Location;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    _dbContext.SaveChanges();
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
            var fetched = _dbContext.Projects.SingleOrDefault(p => p.ProjectCode == project.ProjectCode);
            if (fetched != null)
            {
                bool isUpdated = false;
                if (project.ProjectCode != null)
                {
                    _dbContext.Remove(fetched);
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    _dbContext.SaveChanges();
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

        //------------------------------------------ProjectController Done------------------------------------------


        public IActionResult ManageLeave()
        {
            var leaves = _dbContext.Leaves.Include(l => l.User).Where(x=>x.LeaveStatus=="Pending").ToList();
            return View(leaves);
        }

        [HttpPost]
        public IActionResult UpdateLeave(int id, [FromBody] Leave updatedLeave)
        {
            if (id != updatedLeave.LeaveId)
            {
                return BadRequest("ID mismatch.");
            }

            var leave = _dbContext.Leaves.Find(id);
            if (leave == null)
            {
                return NotFound("Leave request not found.");
            }

            leave.LeaveStatus = updatedLeave.LeaveStatus;
            leave.AccRejDate = DateTime.Now;

            try
            {
                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveExists(id))
                {
                    return NotFound("Leave request not found.");
                }
                throw;
            }

            return Ok("Leave status updated.");
        }

        private bool LeaveExists(int id)
        {
            return _dbContext.Leaves.Any(e => e.LeaveId == id);
        }


        [HttpPost]
        public IActionResult AcceptRejectLeave([FromBody] Leave data)
        {


            if (data == null || string.IsNullOrEmpty(data.LeaveStatus))
            {
                return BadRequest("Status is required.");
            }

            var leaves = _dbContext.Leaves.ToList();
            foreach (var leave in leaves)
            {
                leave.LeaveStatus = data.LeaveStatus;
                leave.AccRejDate = DateTime.Now;
            }
            _dbContext.SaveChanges();

            return Ok("Leaves Updated");
        }

        public IActionResult ManageAttendance()
        {
            var attendanceRecords = _dbContext.Attendances.Include(x => x.User).Where(x => x.AttendanceStatus == "Pending").ToList();
            return View(attendanceRecords);
        }


        [HttpPost]
        public IActionResult ApproveRejectAttendance(int userId, bool isApproved, [FromBody] Attendance att)
        {
            var check = _dbContext.Attendances
            .Include(x => x.User)
            .FirstOrDefault(x => x.UserId == att.UserId && x.AttendanceStatus == "Pending");

            if (check == null)
            {
                return NotFound("No pending attendance record found for the specified user and project.");
            }
            check.AttendanceStatus = att.AttendanceStatus;
            check.AccRejDate = DateTime.Now;

            _dbContext.Attendances.Update(check);
            _dbContext.SaveChanges();
            return Ok("done");
        }

        [HttpGet]
        public IActionResult AttendanceStatus(int userId)
        {
            var attendanceRecords = _dbContext.Attendances
                .Where(x => x.UserId == userId)
                .Include(x => x.User)
                .ToList();

            if (attendanceRecords == null || !attendanceRecords.Any())
            {
                return View(new List<Attendance>());
            }

            return View(attendanceRecords);
        }

    }

    //--------------------------------------LeaveController Done--------------------------------------

    
}
