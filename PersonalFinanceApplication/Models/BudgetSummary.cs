using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    public class BudgetSummary
    {
        public decimal ExpectedIncome { get; set; }
        public decimal Expenditures { get; set; }
        public decimal Goals { get; set; }
        public decimal Remaining { get; set; }
    }
}