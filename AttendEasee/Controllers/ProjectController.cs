using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace AttendEase.Controllers
{
    public class ProjectController : Controller
    {

        private readonly ApplicationDBContext _dbContext;

        public ProjectController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var list= _dbContext.Projects.ToList();
            return View(list);
        }




        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add([FromForm] Project project)
        {
            project.ProjectId = Convert.ToInt32(project.ProjectCode);
            _dbContext.Projects.Add(project);
            _dbContext.SaveChanges();
            TempData["AddSuccess"] = true;
            return RedirectToAction("Index","Project");
        }

        [HttpGet]
        public IActionResult Modify()
        {
            var projectIds = _dbContext.Projects
                            .Select(p => p.ProjectCode)
                            .ToList();
            return View(projectIds);
        }

        [HttpPut]
        public IActionResult Update([FromForm] Project project)
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
        public IActionResult Delete([FromForm] Project project)
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






    }
}
