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
            //Console.WriteLine(user.Email);
            var result= _dbcontext.Users.ToList();
            var fetched = result.SingleOrDefault(p => p.Email == user.Email && p.Password == user.Password);
            
            if (fetched != null) {

                if (fetched.IsManager == true)
                {
                TempData["Details"] = fetched.UserId;
                    TempData["ismanager"] = true;
                return RedirectToAction("Index", "Manager");
                }
                else if(fetched.IsAdmin==true)
                {
                    TempData["LoginSuccess"] = true;
                    return RedirectToAction("Index", "User");
                }
            }
                return View("Errror");
            
        }

        public IActionResult Errror()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
