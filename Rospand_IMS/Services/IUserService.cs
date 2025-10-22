using Rospand_IMS.Models;

namespace Rospand_IMS.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<RoleMaster>> GetAllRolesAsync();
        Task<RoleMaster?> GetRoleByIdAsync(int id);
        Task<RoleMaster> CreateRoleAsync(RoleMaster role);
        Task UpdateRoleAsync(RoleMaster role);
        Task DeleteRoleAsync(int id);
        Task<IEnumerable<PageAccess>> GetPageAccessesByRoleAsync(int roleId);
        Task<PageAccess> CreatePageAccessAsync(PageAccess pageAccess);
        Task UpdatePageAccessAsync(PageAccess pageAccess);
        Task DeletePageAccessAsync(int id);
        bool VerifyPassword(string password, string hashedPassword);
        string HashPassword(string password);
    }
}