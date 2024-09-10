    using AttendEase.Data;
using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendEase.Controllers
{
    public class LeaveController : Controller
    {

        private readonly ApplicationDBContext _dbContext;

        public LeaveController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
                var leaves = _dbContext.Leaves.Include(l => l.User).ToList();
                return View(leaves);
        }

        // =========================================== View Leave =================================================

        [HttpGet]
        public IActionResult ViewLeave()
        {
            var users = _dbContext.Leaves.ToList();
            return View(users);
        }

        // GET: Leave/ViewLeave/5
        [HttpPost("ViewLeave/{id}")]
        public IActionResult ViewLeave(int id)
        {
            var leave = _dbContext.Leaves.Include(l => l.User).FirstOrDefault(l => l.LeaveId == id);
            if (leave == null)
            {
                return NotFound("Leave request not found.");
            }
            return View(leave);
        }

        // GET: Leave/ViewLeaves
        [HttpGet("ViewLeaves")]
        public async Task<IActionResult> ViewLeaves()
        {
            var leaves = await _dbContext.Leaves.Include(l => l.User).ToListAsync();
            return View(leaves);
        }

        // =========================================== Add Leave =================================================

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Leave leave)
        {
            leave.LeaveStatus = "Pending";
            leave.UserId = 14;

            Console.WriteLine(leave.LeaveStatus);
            Console.WriteLine(leave.UserId);
            Console.WriteLine(leave.RequestDate);
            
            _dbContext.Leaves.Add(leave);
            _dbContext.SaveChanges();

            // Optionally redirect to another action or view
            return RedirectToAction("Index", "Leave"); // Redirect to a list or detail view
        }

        // ======================================= Accept Reject Leaves =======================================
        
        // POST: Leave/AccRejLeave
        [HttpPost]
        public IActionResult AccRejLeave([FromBody] Leave data)
        {
            Console.WriteLine(data);
            Console.WriteLine(data.LeaveStatus);

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

        // POST: Leave/UpdateLeave/5
        [HttpPost("UpdateLeave/{id}")]
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
    }
}
