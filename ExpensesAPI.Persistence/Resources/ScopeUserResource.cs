using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence.Resources
{
    public class ScopeUserResource
    {
        public int ScopeId { get; set; }
        public ScopeResource Scope { get; set; }
        public string UserId { get; set; }
        public UserResource User { get; set; }
    }
}
