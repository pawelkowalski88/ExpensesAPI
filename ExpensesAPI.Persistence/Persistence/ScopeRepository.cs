using ExpensesAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public class ScopeRepository : IScopeRepository
    {
        private MainDbContext context;
        public ScopeRepository(MainDbContext context)
        {
            this.context = context;
        }

        public Task<List<Scope>> GetScopes()
        {
            return context.Scopes.Include(s => s.Categories).ToListAsync();
        }

        public Task<List<Scope>> GetScopes(User user)
        {
            var result = context.Scopes
                .Where(s => s.Owner == user)
                .Include(s => s.Categories)
                .Include(s => s.ScopeUsers)
                    .ThenInclude(su => su.User)
                .ToListAsync();

            return result;
        }

        public void DeleteScope(Scope s)
        {
            context.Scopes.Remove(s);
        }

        public async Task AddScope(Scope s)
        {
            await context.Scopes.AddAsync(s);
        }

        public async Task UpdateScope(Scope scope, int id)
        {
            var editedScope = await context.Scopes.FirstOrDefaultAsync(s => s.Id == id);
            if (editedScope == null)
                throw new ArgumentOutOfRangeException(message: "Żądany zeszyt nie istnieje", innerException: null);

            editedScope.Name = scope.Name;
            context.Scopes.Update(editedScope);
        }

        public async Task<Scope> GetScope(int id)
        {
            return await context.Scopes
                .Include(s => s.Categories)
                .Include(s => s.Expenses)
                .Include(s => s.ScopeUsers)
                    .ThenInclude(su => su.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public void RemoveUserFromScope(Scope scope, ScopeUser scopeUser)
        {
            scope.ScopeUsers.Remove(scopeUser);
        }
    }
}
