using AttendEase.Data;
using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;
using System.Text;
using System.Text;
using System.Security.Cryptography;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDBContext _context;

    public string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {

            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
    public UserRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public User GetUserById(int userId)
    {
        return _context.Users.FirstOrDefault(u => u.UserId == userId);
    }

    public User GetUserByName(string userName) {
        return _context.Users.FirstOrDefault(u => u.Email == userName);
    }

    public IEnumerable<User> GetManagerSubordinates(int managerId)
    {
        return _context.Users.Where(u => u.ManagerId == managerId).ToList();
    }

    public void UpdateUser(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public IEnumerable<User> GetNonAdminUsers()
    {
        return _context.Users.Where(user => user.IsAdmin != true).ToList();
    }
    public bool IsUserExists(string email)
    {
        return _context.Users.Any(u => u.Email == email);
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void RemoveUser(User user)
    {
        _context.Remove(user);
        _context.SaveChanges();
    }

    public void save()
    {
        _context.SaveChanges();
    }
}
