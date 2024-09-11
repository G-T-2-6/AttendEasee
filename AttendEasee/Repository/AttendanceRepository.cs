using AttendEase.Data;
using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly ApplicationDBContext _context;

    public AttendanceRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public IEnumerable<Attendance> GetAttendanceByUserId(int userId)
    {
        return _context.Attendances
            .Where(a => a.UserId == userId)
            .Include(x => x.User)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    public Attendance GetAttendanceByDate(int userId, DateTime date)
    {
        return _context.Attendances.FirstOrDefault(a => a.UserId == userId && a.Date == date);
    }

    public void AddAttendance(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
    }

    public Attendance GetAttendanceByUserIdAndDate(int userId, DateTime date)
    {
        return _context.Attendances.FirstOrDefault(a => a.UserId == userId && a.Date == date);
    }

    public IEnumerable<Attendance> GetPendingAttendanceByManager(int userId)
    {
        return (_context.Attendances
            .Include(a => a.User)  // Include User information in the result
            .Where(a => a.User.ManagerId == userId && a.AttendanceStatus == "Pending")
            .ToList());
    }

    public Attendance GetPendingAttendance(int userId)
    {
        return (_context.Attendances
            .Include(x => x.User)
            .FirstOrDefault(x => x.UserId == userId && x.AttendanceStatus == "Pending"));
    }

    public IEnumerable<Attendance> GetAllPendingAttendances()
    {
        return (_context.Attendances.Include(x => x.User).Where(x => x.AttendanceStatus == "Pending").ToList());
    }

    public void UpdateAttendance(Attendance attendance)
    {
        _context.Attendances.Update(attendance);
        _context.SaveChanges();
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
