using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("BudgetType")]
    public class BudgetType
    {
        public BudgetType()
        {
            BudgetCategories = new HashSet<BudgetCategory>();
        }
        public int BudgetTypeID { get; set; }
        public string Description { get; set; }

        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }
    }
}