using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Pass_Api_Maker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static Dictionary<string, User> users = new Dictionary<string, User>();

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (users.ContainsKey(request.Username))
            {
                return BadRequest(new { message = "User already exists" });
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            users[request.Username] = user;

            return Ok(new { message = "User registered successfully", username = user.Username });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!users.ContainsKey(request.Username))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var user = users[request.Username];
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = GenerateToken(request.Username);

            return Ok(new
            {
                message = "Login successful",
                username = user.Username,
                token = token
            });
        }

        [HttpPost("generate-password")]
        public IActionResult GeneratePassword([FromBody] PasswordGenerationRequest request)
        {
            var password = GenerateSecurePassword(
                request.Length,
                request.IncludeUppercase,
                request.IncludeLowercase,
                request.IncludeNumbers,
                request.IncludeSpecialChars
            );

            return Ok(new { password = password });
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!users.ContainsKey(request.Username))
            {
                return NotFound(new { message = "User not found" });
            }

            var user = users[request.Username];
            if (!VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return Unauthorized(new { message = "Current password is incorrect" });
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            return Ok(new { message = "Password changed successfully" });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private string GenerateToken(string username)
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private string GenerateSecurePassword(int length, bool upper, bool lower, bool numbers, bool special)
        {
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string numberChars = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var charSet = "";
            if (upper) charSet += upperChars;
            if (lower) charSet += lowerChars;
            if (numbers) charSet += numberChars;
            if (special) charSet += specialChars;

            if (string.IsNullOrEmpty(charSet))
                charSet = lowerChars;

            var password = new char[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[length * 4];
                rng.GetBytes(bytes);

                for (int i = 0; i < length; i++)
                {
                    var randomInt = BitConverter.ToUInt32(bytes, i * 4);
                    password[i] = charSet[(int)(randomInt % charSet.Length)];
                }
            }

            return new string(password);
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PasswordGenerationRequest
    {
        public int Length { get; set; } = 12;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeNumbers { get; set; } = true;
        public bool IncludeSpecialChars { get; set; } = true;
    }

    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}