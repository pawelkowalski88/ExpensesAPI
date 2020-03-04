using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Models
{
    public class Scope
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Expense> Expenses { get; set; }

        public ICollection<Category> Categories { get; set; }

        public User Owner { get; set; }

        public string OwnerId { get; set; }

        public ICollection<ScopeUser> ScopeUsers { get; set; }

        public Scope()
        {
            Expenses = new Collection<Expense>();
            Categories = new Collection<Category>();
            ScopeUsers = new Collection<ScopeUser>();
        }
    }
}
