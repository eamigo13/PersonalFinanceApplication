using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("Income")]
    public class Income
    {
        public int IncomeID { get; set; }
        public int BudgetID { get; set; }
        public int IncomeTypeID { get; set; }
        public int HoursPerWeek { get; set; }
        public decimal Wage { get; set; }
        public string IncomeName { get; set; }

        [ForeignKey("BudgetID")]
        public virtual Budget Budget { get; set; }

        [ForeignKey("IncomeTypeID")]
        public virtual IncomeType IncomeType { get; set; }

    }
}