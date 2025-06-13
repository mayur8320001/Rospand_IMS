using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using Rospand_IMS.Models.LoginM;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rospand_IMS.Services
{
    public interface IAuthService
    {
        Task<User> Register(string username, string password, int roleId);
        Task<string> Login(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> Register(string username, string password, int roleId)
        {
            if (_context.Users.Any(u => u.Username == username))
                return null;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                RoleId = roleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string> Login(string username, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role.Name),
        new Claim("UserId", user.Id.ToString())
    };

            // 🔥 Fix: move query to memory first using ToList()
            var rolePermissions = _context.RolePermissions
                .Include(rp => rp.Page)
                .Where(rp => rp.RoleId == user.RoleId)
                .ToList();

            foreach (var rp in rolePermissions)
            {
                if (rp.CanRead)
                    claims.Add(new Claim("Permission", $"{rp.Page.Controller}:CanRead"));
                if (rp.CanCreate)
                    claims.Add(new Claim("Permission", $"{rp.Page.Controller}:CanCreate"));
                if (rp.CanUpdate)
                    claims.Add(new Claim("Permission", $"{rp.Page.Controller}:CanUpdate"));
                if (rp.CanDelete)
                    claims.Add(new Claim("Permission", $"{rp.Page.Controller}:CanDelete"));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }

}