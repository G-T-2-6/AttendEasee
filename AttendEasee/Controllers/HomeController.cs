using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AttendEase.Controllers
{
    public class HomeController : Controller
    {
       

        private readonly ApplicationDBContext _dbcontext;

        

        public HomeController(ApplicationDBContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        public IActionResult Index()
        {
            return View();
        }

        

        [HttpPost]
        public IActionResult Index([FromForm] User user)
        { 
            var result= _dbcontext.Users.ToList();
            var fetched = result.SingleOrDefault(p => p.Email == user.Email && p.Password == user.Password);
            
            if (fetched != null) {

                HttpContext.Session.SetInt32("UserId", fetched.UserId);
                HttpContext.Session.SetString("UserName", fetched.UserName);

                if (fetched.IsManager == true)
                {
                    TempData["ismanager"] = true;
                    TempData["Details"] = fetched.UserId;
                    return RedirectToAction("Index", "Manager");
                }
                else if(fetched.IsAdmin==true)
                {
                    TempData["LoginSuccess"] = true;
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
                return View("Errror");
            
        }

        public IActionResult Errror()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
