using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence.Validation
{
    public class ListValidationResult : ValidationResult
    {
        public ListValidationResult() : base("")
        {

        }

        public List<ValidationResult> ListResult { get; set; }
    }
}
