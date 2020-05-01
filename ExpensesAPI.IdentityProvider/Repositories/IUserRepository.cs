using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider.Repositories
{
    public interface IUserRepository
    {
        Task<IdentityUser> GetUserAsync(string id);
        //Task<IdentityUser> GetUserWithScopesAsync(string id);
        Task<List<IdentityUser>> GetUserListAsync(string query, string myId);
        //Task SetSelectedScope(string userId, int scopeId);
        //Task AddUser(string id, string name);
    }
}
