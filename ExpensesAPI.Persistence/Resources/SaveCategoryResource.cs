using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Resources
{
    public class SaveCategoryResource : CategoryResource
    {
        public int ScopeId { get; set; }
    }
}
