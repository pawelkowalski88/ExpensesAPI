using System.Collections.Generic;

namespace ExpensesAPI.Domain.Models
{
    public class IdentityServerUser : User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
    }
}
