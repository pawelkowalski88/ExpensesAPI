using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public int ScopeId { get; set; }
        public Scope Scope { get; set; }

        public ICollection<Expense> Expenses { get; set; }

        public Category()
        {
            Expenses = new Collection<Expense>();
        }
    }
}
