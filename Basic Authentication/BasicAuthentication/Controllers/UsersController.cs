using BasicAuthentication.Models;
using BasicAuthentication.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasicAuthentication.Controllers
{
    // Applies authorization to ensure all actions within the controller require authentication.
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    //[Authorize]

    // Marks the class as a web API controller with additional features enabled by ASP.NET Core.
    [ApiController]

    // Defines the route template using the controller name.
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // Repository interface for user operations, injected via DI.
        private readonly IUserRepository _userRepository;

        // Constructor accepting IUserRepository via dependency injection.
        public UsersController(IUserRepository userRepository)
        {
            // Assigns the injected repository to a private field.
            _userRepository = userRepository;
        }

        // GET: api/users
        [HttpGet] // Marks this method as responding to HTTP GET requests.
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Retrieves all users from the repository.
            var users = await _userRepository.GetAllUsers();

            // Returns the list of users with an HTTP 200 status code.
            return Ok(users);
        }

        // GET api/users/{id}
        [HttpGet("{id}")] // Marks this method as responding to HTTP GET with a route parameter "id".
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetUserById(id); // Retrieves a user by their ID.
            if (user == null) // Checks if the user is not found.
            {
                return NotFound("User not found."); // Returns a 404 status code if no user is found.
            }
            return Ok(user); // Returns the found user with an HTTP 200 status code.
        }

        // POST api/users
        [HttpPost] // Marks this method as responding to HTTP POST requests.
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            try
            {
                var createdUser = await _userRepository.AddUser(user); // Tries to add a new user to the repository.
                // Returns a 201 status code and the route to access the newly created user.
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Returns a 400 status code if there's an exception, with the exception message.
            }
        }

        // PUT api/users/{id}
        [HttpPut("{id}")] // Marks this method as responding to HTTP PUT requests, with a route parameter "id".
        public async Task<ActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id) // Checks if the URL id matches the user id in the body.
            {
                return BadRequest("ID mismatch in the URL and body."); // Returns a 400 status if they don't match.
            }

            try
            {
                await _userRepository.UpdateUser(user); // Tries to update the user in the repository.
                return NoContent(); // Returns a 204 status code indicating that the operation was successful but no content is returned.
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found.")
                {
                    return NotFound(ex.Message); // Returns a 404 status code if the user is not found.
                }
                return BadRequest(ex.Message); // Returns a 400 status code for other exceptions.
            }
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")] // Marks this method as responding to HTTP DELETE requests, with a route parameter "id".
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await _userRepository.DeleteUser(id); // Tries to delete the user by ID.
                return NoContent(); // Returns a 204 status code indicating the user was successfully deleted.
            }
            catch (Exception ex)
            {
                if (ex.Message == "User not found.")
                {
                    return NotFound(ex.Message); // Returns a 404 status code if the user is not found.
                }
                return BadRequest(ex.Message); // Returns a 400 status code for other exceptions.
            }
        }

    }
}
