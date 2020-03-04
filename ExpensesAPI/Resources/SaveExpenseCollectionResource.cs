using ExpensesAPI.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Resources
{
    public class SaveExpenseCollectionResource
    {
        [Required]
        [CheckChildren]
        public List<ExpenseResourceBase> Expenses { get; set; }
    }
}
