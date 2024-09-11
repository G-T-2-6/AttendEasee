using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AttendEase.Controllers
{
    public class HomeController : Controller
    {

        private readonly IUserRepository _userRepository;
        private readonly ApplicationDBContext _dbcontext;

        public HomeController(IUserRepository userRepository, ApplicationDBContext dbcontext)
        {
            _userRepository = userRepository;
            _dbcontext = dbcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index([FromForm] User user)
        {

            string hash= _userRepository.HashPassword(user.Password);

            var result= _dbcontext.Users.ToList();
            var fetched = result.SingleOrDefault(p => p.Email == user.Email && hash==p.Password);
            
            
            if (fetched!=null) {

                HttpContext.Session.SetInt32("UserId", fetched.UserId);
                HttpContext.Session.SetString("UserName", fetched.UserName);

                if (fetched.IsManager == true)
                {
                    TempData["ismanager"] = true;
                    HttpContext.Session.SetString("UserRole", "Manager");  // Store 'Manager' role
                    return RedirectToAction("Index", "Manager");
                }
                else if(fetched.IsAdmin==true)
                {
                    TempData["LoginSuccess"] = true;
                    HttpContext.Session.SetString("UserRole", "Admin");  // Store 'Admin' role
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    HttpContext.Session.SetString("UserRole", "Developer");
                    return RedirectToAction("Index", "Developer");
                }
            }
                return View("Errror");
            
        }

        public IActionResult Forbidden()
        {
            return View();
        }

        public IActionResult Errror()
        {
            return View();
        }

        // Logout function to clear session
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
