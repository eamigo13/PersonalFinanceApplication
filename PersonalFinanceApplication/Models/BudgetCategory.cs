﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("BudgetCategory")]
    public class BudgetCategory
    {
        [Key]
        [Column(Order = 1)]
        public int BudgetID { get; set; }

        [Key]
        [Column(Order = 2)]
        public int CategoryID { get; set; }

        public int BudgetTypeID { get; set; }

        public decimal Amount { get; set; }

        [ForeignKey("BudgetID")]
        public virtual Budget Budget { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        [ForeignKey("BudgetTypeID")]
        public virtual BudgetType BudgetType { get; set; }
    }
}