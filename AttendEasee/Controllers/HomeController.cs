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

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a hex string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


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

            string hash=HomeController.HashPassword(user.Password);

            var result= _dbcontext.Users.ToList();
            var fetched = result.SingleOrDefault(p => p.Email == user.Email && hash==p.Password);
            
            
            if (fetched!=null) {

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
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Developer");
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
