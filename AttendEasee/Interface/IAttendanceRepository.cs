using AttendEase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public interface IAttendanceRepository
{
    IEnumerable<Attendance> GetAttendanceByUserId(int userId);
    Attendance GetAttendanceByDate(int userId, DateTime date);
    void AddAttendance(Attendance attendance);

    Attendance GetAttendanceByUserIdAndDate(int userId, DateTime date);

    IEnumerable<Attendance> GetPendingAttendanceByManager(int userId);
    IEnumerable<Attendance> GetAllPendingAttendances();
    Attendance GetPendingAttendance(int userId);

    void UpdateAttendance(Attendance attendance);

    void Save();
}
