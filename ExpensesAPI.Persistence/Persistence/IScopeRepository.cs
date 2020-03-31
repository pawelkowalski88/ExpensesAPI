using ExpensesAPI.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public interface IScopeRepository
    {
        Task<List<Scope>> GetScopes();
        Task<List<Scope>> GetScopes(User user);
        Task<Scope> GetScope(int id);
        Task AddScope(Scope s);
        void DeleteScope(Scope s);
        Task UpdateScope(Scope scope, int id);
        void RemoveUserFromScope(Scope scope, ScopeUser scopeUser);
    }
}
