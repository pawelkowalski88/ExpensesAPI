﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Resources
{
    public class CategoryResource
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
