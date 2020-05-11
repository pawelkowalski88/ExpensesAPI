using ExpensesAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public interface IUserRepository<TUser> where TUser : User
    {
        Task<User> GetUserAsync(string id);
        Task<User> GetUserWithScopesAsync(string id);
        Task<List<User>> GetUserListAsync(string query, string myId);
        Task<List<User>> GetUserDetails(List<string> ids);
        Task SetSelectedScope(string userId, int scopeId);
        Task AddUser(string id, string name);
    }
}
