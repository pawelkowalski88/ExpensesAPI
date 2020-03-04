using ExpensesAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string id);
        Task<User> GetUserWithScopesAsync(string id);
        Task<List<User>> GetUserListAsync(string query, string myId);
    }
}
