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

        public IActionResult Attendance()
        {
            return View();
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
    }
}
