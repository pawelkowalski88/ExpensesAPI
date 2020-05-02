using ExpensesAPI.IdentityProvider.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private ApplicationDbContext context;
        public UserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<User> GetUserAsync(string id)
        {
            return await context.Users
                //.Include(u => u.SelectedScope)
                //.Include(u => u.ScopeUsers)
                //    .ThenInclude(su => su.Scope)
                //        .ThenInclude(s => s.Owner)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUserListAsync(string query, string myId)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var users = await context.Users
                    //.Include(u => u.ScopeUsers)
                    .Where(u => ((u.FirstName + " " + u.LastName).ToLower().Contains(query.ToLower()) || u.EmailLocalPart.ToLower().Contains(query.ToLower())) && u.Id != myId)
                    .ToListAsync();
                return users;
            }
            return new List<User>();
        }

        private bool CompareEmail(User u, string query)
        {
            var atIndex = u.Email.IndexOf('@');
            var result = u.Email.Substring(0, atIndex).ToLower().Contains(query.ToLower());
            return result;
        }

        //public async Task<User> GetUserWithScopesAsync(string id)
        //{
        //    return await context.Users
        //        .Include(u => u.SelectedScope)
        //        .Include(u => u.OwnedScopes)
        //        .FirstOrDefaultAsync(u => u.Id == id);
        //}

        //public async Task AddUser(string id, string name)
        //{
        //    context.Users.Add(new User
        //    {
        //        Id = id,
        //        UserName = name
        //    });

        //    await context.SaveChangesAsync();
        //}
    }
}
