using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    /*This model is used to pass information from the /Transactions/Confirm view to the controller.
     * The only elements being confirmed are the CategoryID and VendorID so the whole transaction model
     * isn't necessary
     */
    public class ConfirmedTransaction
    {
        public int TransactionID { get; set; }
        public int CategoryID { get; set; }
        public int VendorID { get; set; }
        public bool VendorDetected { get; set; }
        public string Description { get; set; }
    }
}