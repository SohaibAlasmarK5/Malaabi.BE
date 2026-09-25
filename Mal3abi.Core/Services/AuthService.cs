using Mal3abi.Core.Entites;
using Mal3abi.Core.Interfaces.Services;
using Mal3abi.Core.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Mal3abi.Core.Constants.Values;

namespace Mal3abi.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseResource> RegisterAsync(RegisterRequestResource request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
                throw new Exception("An account with this email already exists.");

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone,
                FullName = request.Name,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
                throw new ValidationException(message);
            }

            // Public registration always creates a regular User. Admin (court
            // owner) and SuperAdmin accounts are never self-registered — the
            // three roles themselves are seeded once at startup (RoleSeeder),
            // not created here.
            await _userManager.AddToRoleAsync(user, Roles.User);

            var token = await GenerateTokenAsync(user, Roles.User);
            return new AuthResponseResource { Token = token, User = ToUserResource(user, Roles.User) };
        }

        public async Task<AuthResponseResource> LoginAsync(LoginRequestResource request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Invalid email or password.");

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.User;
            var token = await GenerateTokenAsync(user, role);
            return new AuthResponseResource { Token = token, User = ToUserResource(user, role) };
        }

        public async Task<UserResource?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return null;

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.User;
            return ToUserResource(user, role);
        }

        private Task<string> GenerateTokenAsync(User user, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "43200"); // 30 days default

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        private static UserResource ToUserResource(User user, string role) => new UserResource
        {
            Id = user.Id.ToString(),
            Name = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            Role = role,
        };
    }
}