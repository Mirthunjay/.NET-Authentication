using BasicAuthentication.Models;

namespace BasicAuthentication.Service.Interface
{
    public interface IUserRepository
    {
      Task<User?> ValidateUser(string username, string password); // Method to validate a user's credentials.
      Task<List<User>> GetAllUsers(); // Method to retrieve all users.
      Task<User?> GetUserById(int id); // Method to fetch a single user by ID.
      Task<User> AddUser(User user); // Method to add a new user.
      Task<User> UpdateUser(User user); // Method to update an existing user's details.
      Task DeleteUser(int id); // Method to delete a user by ID.
    }
}
