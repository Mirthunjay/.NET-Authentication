using BasicAuthentication.Models;
using BasicAuthentication.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BasicAuthentication.Common
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository _userRepository; // Dependency injection for the user repository

        // Constructor
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, // Options for configuring authentication schemes
            ILoggerFactory logger, // Factory to create logger objects
            UrlEncoder encoder, // Encoder for URL to ensure safe URLs
            IUserRepository userRepository) // User repository to handle user data
            : base(options, logger, encoder) // Pass options, logger, and encoder to the base class
        {
            _userRepository = userRepository; // Initialize user repository with dependency injection
        }

        // Method to handle the authentication process
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if the Authorization header is present
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                // Fail authentication if header is missing
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            User? user;
            try
            {
                // Parse the Authorization header and validate its format
                if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var authHeader))
                {
                    // Fail authentication if header format is wrong
                    return AuthenticateResult.Fail("Invalid Authorization Header Format");
                }

                // Decode the Base64 encoded credentials from the header
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);

                // Split decoded credentials into username and password
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length != 2)
                {
                    // Ensure credentials are in correct format
                    return AuthenticateResult.Fail("Invalid Authorization Header Content");
                }

                // Extract username
                var username = credentials[0];

                // Extract password
                var password = credentials[1];

                // Validate user against the stored credentials
                user = await _userRepository.ValidateUser(username, password);
            }
            catch (FormatException) // Handle format exceptions for Base64 decoding
            {
                return AuthenticateResult.Fail("Invalid Base64 Encoding in Authorization Header");
            }
            catch (Exception) // Handle general exceptions
            {
                return AuthenticateResult.Fail("Error Processing Authorization Header");
            }

            if (user == null)
            {
                // Check if user is not found or invalid credentials
                return AuthenticateResult.Fail("Invalid Username or Password");
            }

            // Create claims based on the valid user
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            // Create an identity with claims
            var identity = new ClaimsIdentity(claims, Scheme.Name);

            // Create principal from identity
            var principal = new ClaimsPrincipal(identity);

            // Create ticket with principal and scheme
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            // Return success with the authentication ticket
            return AuthenticateResult.Success(ticket);
        }

        // Method to handle the authentication challenge
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Set the WWW-Authenticate header in the response. This instructs the client
            // (usually a web browser) to prompt the user with a login dialog for username and password.
            // "Basic realm=\"BasicAuthenticationDemo\"" describes the authentication method (Basic)
            // and the realm. The realm can be used to describe the protected area or to prompt
            // the user with a more specific identifier about what they are accessing.
            // "charset=\"UTF-8\"" ensures that the characters entered by the user are encoded correctly.
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"BasicAuthenticationDemo\", charset=\"UTF-8\"";

            // Set the HTTP status code to 401 Unauthorized to indicate that the request has failed
            // authentication and needs proper credentials to proceed.
            Response.StatusCode = 401;

            // Send a custom message in the response body. This message can be seen by the client if they
            // access the raw HTTP response. It's a clear indicator as to why the request was rejected.
            await Response.WriteAsync("You need to authenticate to access this resource.");
        }
    }

}
