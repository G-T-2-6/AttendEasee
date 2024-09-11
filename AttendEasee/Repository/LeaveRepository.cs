using AttendEase.Data;
using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class LeaveRepository : ILeaveRepository
{
    private readonly ApplicationDBContext _context;

    public LeaveRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public IEnumerable<Leave> GetLeavesByUserId(int userId)
    {
        return _context.Leaves.Where(l => l.UserId == userId).ToList();
    }

    public void AddLeave(Leave leave)
    {
        _context.Leaves.Add(leave);
    }

    public List<Leave> GetEmployeeLeaves(int userId)
    {
        return _context.Leaves.Where(l => l.UserId == userId).ToList();
    }

    public Leave GetLeaveById(int userId)
    {
        return _context.Leaves.Find(userId);
    }

    public void UpdateLeave(Leave leave)
    {
        _context.Leaves.Update(leave);
        _context.SaveChanges();
    }
    
    public List<Leave> GetPendingLeavesByManagerId(int managerId)
    {
        return (from user in _context.Users 
                join leave in _context.Leaves on user.UserId equals leave.UserId
                where user.ManagerId == managerId && leave.LeaveStatus == "Pending"
                select leave).ToList();
    }

    public List<Leave> GetPendingLeaves()
    {
        return _context.Leaves.Include(l => l.User).Where(x => x.LeaveStatus == "Pending").OrderByDescending(d => d.RequestDate).ToList();
    }

    public List<Leave> GetLeaveByDateAndUserId(DateTime date, int userId)
    {
        return _context.Leaves.Where(l => l.RequestDate == date && l.UserId == userId).ToList();
    }

    public bool LeaveExists(int id)
    {
        return _context.Leaves.Any(e => e.LeaveId == id);
    }

    public IEnumerable<Leave> GetAllLeaves()
    {
        return _context.Leaves.ToList();
    }
    public void Save()
    {
        _context.SaveChanges();
    }
}
