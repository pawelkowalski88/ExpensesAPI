using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence.Models
{
    public class Query : ISortingQuery
    {
        public string SortBy { get; set; }
        public string SortBySecondary { get; set; }
        public bool SortAscending { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String Categories { get; set; }
    }
}
