using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    public class BudgetCategorySimple
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string BudgetType { get; set; }
        public double Amount { get; set; }
        public double UsedAmount { get; set; }
        public double UsedAmountPerPeriod { get; set; }
        public double RemainingAmount { get; set; }
        public SimpleTransaction[] Transactions { get; set; }
    }
}