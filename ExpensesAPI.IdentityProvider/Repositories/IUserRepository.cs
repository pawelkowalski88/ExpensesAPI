using ExpensesAPI.IdentityProvider.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string id);
        //Task<IdentityUser> GetUserWithScopesAsync(string id);
        Task<List<User>> GetUserListAsync(string query, string myId);
        //Task SetSelectedScope(string userId, int scopeId);
        //Task AddUser(string id, string name);
    }
}
