using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence.Resources
{
    public class UserResource
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public List<ScopeUserResource> ScopeUsers { get; set; }
        public bool Selected { get; set; }
    }
}
