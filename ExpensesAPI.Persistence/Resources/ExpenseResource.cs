﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Resources
{
    public class ExpenseResource : ExpenseResourceBase
    {
        public bool IsDuplicate { get; set; }
    }
}
