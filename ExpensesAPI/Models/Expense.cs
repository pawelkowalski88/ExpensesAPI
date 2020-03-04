using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Models
{
    public class Expense
    {
        private DateTime _date;

        public int Id { get; set; }

        [Required]
        public Category Category { get; set; }
        public int CategoryId { get; set; }

        public int ScopeId { get; set; }

        public Scope Scope { get; set; }

        [Required]
        public DateTime Date
        {
            get => _date.Date;
            set => _date = value;
        }

        [Required]
        [StringLength(1024)]
        public string Comment { get; set; }

        [Required]
        public float Value { get; set; }

        [StringLength(1024)]
        public string Details { get; set; }
    }
}
