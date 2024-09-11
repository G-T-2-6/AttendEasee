using AttendEase.Data;
using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;

public class DesignationRepository : IDesignationRepository
{
    private readonly ApplicationDBContext _context;

    public DesignationRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public IEnumerable<string> GetDesignationRolesString()
    {
        return _context.Designations.Where(u => u.Roles != "Admin").Select(d => d.DesignationId + "-" + d.Roles).ToList();
    }
}


