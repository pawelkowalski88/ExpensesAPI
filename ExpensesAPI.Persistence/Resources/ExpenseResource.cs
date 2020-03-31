using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence.Resources
{
    public class ExpenseResource : ExpenseResourceBase
    {
        public bool IsDuplicate { get; set; }
    }
}
