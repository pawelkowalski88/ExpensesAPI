using ExpensesAPI.Domain.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Resources
{
    public class SaveExpenseCollectionResource
    {
        [Required]
        //[CheckChildren] - fix the attribute
        public List<ExpenseResourceBase> Expenses { get; set; }
    }
}
