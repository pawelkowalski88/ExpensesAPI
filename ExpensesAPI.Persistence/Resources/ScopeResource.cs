using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Resources
{
    public class ScopeResource
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public ICollection<CategoryResource> Categories { get; set; }

        public UserResource Owner { get; set; }

        public ICollection<ScopeUserResource> ScopeUsers { get; set; }
    }
}
