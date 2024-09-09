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
            .Select(d => d.DesignationId + "-" + d.Roles)  // Assuming 'Title' represents the role name
            .ToList();

            var projectId = _dbContext.Projects
            .Select(p => p.ProjectId)  // Assuming 'ProjectId' is the property for project IDs
            .ToList();


            ViewBag.ProjectIds = projectId;

            return View(designations);

        }

        [HttpPost]
        public IActionResult Add([FromForm] User user)
        {
            user.Password = "@123";
            if (_dbContext.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("email already exists");
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
                bool isUpdated = false;
                if (user.UserName != null)
                {
                    _dbContext.Users.Remove(fetched);
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
    }
}
