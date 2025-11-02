using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourWinFormsApp
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(string baseUrl = "http://localhost:5135")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        // Register new user
        public async Task<ApiResponse> RegisterAsync(string username, string password)
        {
            try
            {
                var data = new { username, password };
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = responseContent
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Login user
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var data = new { username, password };
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LoginResult>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return new LoginResponse
                    {
                        Success = true,
                        Username = result.Username,
                        Token = result.Token,
                        Message = result.Message
                    };
                }
                else
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Generate password
        public async Task<string> GeneratePasswordAsync(
            int length = 12,
            bool includeUppercase = true,
            bool includeLowercase = true,
            bool includeNumbers = true,
            bool includeSpecialChars = true)
        {
            try
            {
                var data = new
                {
                    length,
                    includeUppercase,
                    includeLowercase,
                    includeNumbers,
                    includeSpecialChars
                };

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/generate-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<PasswordResult>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result.Password;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate password: {ex.Message}");
            }
        }

        // Change password
        public async Task<ApiResponse> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            try
            {
                var data = new { username, oldPassword, newPassword };
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/change-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new ApiResponse
                {
                    Success = response.IsSuccessStatusCode,
                    Message = responseContent
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }

    // Response models
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class LoginResponse : ApiResponse
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }

    // Internal models for deserialization
    internal class LoginResult
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }

    internal class PasswordResult
    {
        public string Password { get; set; }
    }
}