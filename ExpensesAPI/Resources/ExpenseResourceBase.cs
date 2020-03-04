using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Resources
{
    public class ExpenseResourceBase
    {
        public int Id { get; set; }
        public string Date { get; set; }
        [Required]
        public string Comment { get; set; }
        public float Value { get; set; }
        public string Details { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
