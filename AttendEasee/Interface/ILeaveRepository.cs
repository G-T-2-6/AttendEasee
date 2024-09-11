using AttendEase.Models;
using System.Collections.Generic;

public interface ILeaveRepository
{
    IEnumerable<Leave> GetLeavesByUserId(int userId);
    List<Leave> GetEmployeeLeaves(int userId);
    void AddLeave(Leave leave);
    Leave GetLeaveById(int userId);
    void UpdateLeave(Leave leave);
    List<Leave> GetPendingLeavesByManagerId(int managerId);
    List<Leave> GetPendingLeaves();
    IEnumerable<Leave> GetAllLeaves();
    bool LeaveExists(int id);
    void Save();
}
