using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using BCrypt.Net;

namespace Rospand_IMS.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            // Update last login date
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            user.PasswordHash = HashPassword(password);
            user.CreatedDate = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<RoleMaster>> GetAllRolesAsync()
        {
            return await _context.RoleMasters
                .Include(r => r.PageAccesses)
                .ToListAsync();
        }

        public async Task<RoleMaster?> GetRoleByIdAsync(int id)
        {
            return await _context.RoleMasters
                .Include(r => r.PageAccesses)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<RoleMaster> CreateRoleAsync(RoleMaster role)
        {
            _context.RoleMasters.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task UpdateRoleAsync(RoleMaster role)
        {
            _context.RoleMasters.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.RoleMasters.FindAsync(id);
            if (role != null)
            {
                _context.RoleMasters.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PageAccess>> GetPageAccessesByRoleAsync(int roleId)
        {
            return await _context.PageAccesses
                .Where(pa => pa.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<PageAccess> CreatePageAccessAsync(PageAccess pageAccess)
        {
            _context.PageAccesses.Add(pageAccess);
            await _context.SaveChangesAsync();
            return pageAccess;
        }

        public async Task UpdatePageAccessAsync(PageAccess pageAccess)
        {
            _context.PageAccesses.Update(pageAccess);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePageAccessAsync(int id)
        {
            var pageAccess = await _context.PageAccesses.FindAsync(id);
            if (pageAccess != null)
            {
                _context.PageAccesses.Remove(pageAccess);
                await _context.SaveChangesAsync();
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}