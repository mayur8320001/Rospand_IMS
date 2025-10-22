using Rospand_IMS.Models;

namespace Rospand_IMS.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<Menu>> GetAllMenusAsync();
        Task<Menu?> GetMenuByIdAsync(int id);
        Task<Menu> CreateMenuAsync(Menu menu);
        Task UpdateMenuAsync(Menu menu);
        Task DeleteMenuAsync(int id);
        Task SeedDefaultMenusAsync();
    }
}