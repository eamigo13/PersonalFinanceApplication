using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    /* This class is only used to pass transactions from the budget controller to the view as a JSON object
     * Using the full Transaction doesn't work because JSON can't serialize the related 'Account' and 'Category'
     */
    
    public class SimpleTransaction
    {
        public int TransactionID { get; set; }
        public int? CategoryID { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}