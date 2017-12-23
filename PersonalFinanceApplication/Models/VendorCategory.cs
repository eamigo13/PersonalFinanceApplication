using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("VendorCategory")]
    public class VendorCategory
    {
        public VendorCategory()
        {

        }

        public VendorCategory(int VendorID, int CategoryID, int TransactionCount)
        {
            this.VendorID = VendorID;
            this.CategoryID = CategoryID;
            this.TransactionCount = TransactionCount;
        }

        [Key]
        [Column(Order = 1)]
        public int VendorID { get; set; }

        [Key]
        [Column(Order = 2)]
        public int CategoryID { get; set; }

        public int TransactionCount { get; set; }

        [ForeignKey("VendorID")]
        public virtual Vendor Vendor { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

    }
}