using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEase.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDBContext _dbContext;

        public EmployeeController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var list = _dbContext.Users.Where(user => user.IsAdmin != true).ToList();
            if (list != null)
            {
                return View(list);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Add()
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
        public IActionResult Add([FromForm] User user, string ProjectCode)
        {
            user.Password = "@123";
            if (_dbContext.Users.Any(u => u.Email == user.Email))
            {
                TempData["EmailExistsWarning"] = true;
                return RedirectToAction("Add");
            }

            if (ProjectCode != "N/A")
            {
                var project = _dbContext.Projects.FirstOrDefault(p => p.ProjectCode == ProjectCode);
                if (project != null)
                {
                    user.ProjectId = project.ProjectId;
                }
            }

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            TempData["AddSuccess"] = true;
            return RedirectToAction("Index", "Employee");
        }

        [HttpGet]
        public IActionResult Modify()
        {
            var projectIds = _dbContext.Designations
                            .Select(p => p.DesignationId)
                            .ToList();
            return View(projectIds);
        }



        [HttpPut]
        public IActionResult Delete([FromForm] User user)
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

        //[HttpPost]
        //public IActionResult AssignProject(string projectCode, int userId, string role)
        //{
        //    // Fetch the project by ProjectId
        //    var project = _dbContext.Projects.SingleOrDefault(p => p.ProjectCode == projectCode);
        //    if (project == null)
        //    {
        //        return NotFound("Project not found");
        //    }

        //    // Fetch the user by UserId (either Manager or Employee)
        //    var user = _dbContext.Users.SingleOrDefault(u => u.UserId == userId);
        //    if (user == null)
        //    {
        //        return NotFound("User not found");
        //    }

        //    user.ProjectId = project.ProjectId;


        //    _dbContext.SaveChanges();

        //    return Ok("User assigned to project successfully");
        //}
    }
}
