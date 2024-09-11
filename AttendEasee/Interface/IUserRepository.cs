using AttendEase.Models;

public interface IUserRepository
{
    User GetUserById(int userId);
    string HashPassword(string password);
    User GetUserByName(string userName);
    IEnumerable<User> GetManagerSubordinates(int managerId);
    IEnumerable<User> GetNonAdminUsers();
    void UpdateUser(User user);
    bool IsUserExists(string email);
    void AddUser(User user);
    void RemoveUser(User user);
    void save();
}
