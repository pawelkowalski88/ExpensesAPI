using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Models
{
    public class ScopeUser
    {
        public int ScopeId { get; set; }
        public Scope Scope { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
