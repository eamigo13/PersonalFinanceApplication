using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    public class FailedRow
    {
        public decimal Amount { get; set; }
        public string invalidAmount { get; set; }
        public DateTime Date { get; set; }
        public string invalidDate { get; set; }
        public string Description { get; set; }
        public int ErrorID { get; set; }
        public string ErrorMessage { get; set; }
    }
}