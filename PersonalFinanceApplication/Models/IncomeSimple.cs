using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    public class IncomeSimple
    {
        public int IncomeID { get; set; }
        public int BudgetID { get; set; }
        public int IncomeTypeID { get; set; }
        public int HoursPerWeek { get; set; }
        public decimal Wage { get; set; }
        public string IncomeTypeDescription { get; set; }

    }
}