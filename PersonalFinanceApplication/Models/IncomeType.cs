using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("IncomeType")]
    public class IncomeType
    {
        public int IncomeTypeID { get; set; }
        public string Description { get; set; }
    }
}