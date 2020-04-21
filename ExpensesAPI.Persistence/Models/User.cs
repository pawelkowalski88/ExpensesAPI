using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public Scope SelectedScope { get; set; }
        public int? SelectedScopeId { get; set; }
        public ICollection<ScopeUser> ScopeUsers { get; set; }
        public ICollection<Scope> OwnedScopes { get; set; }

        public User()
        {
            ScopeUsers = new Collection<ScopeUser>();
            OwnedScopes = new Collection<Scope>();
        }
    }
}
