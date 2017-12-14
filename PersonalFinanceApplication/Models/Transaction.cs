namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Transaction")]
    public partial class Transaction
    {
        //Constructor
        public Transaction(DateTime Date, decimal Amount, string Description)
        {
            this.Date = Date;
            this.Amount = Amount;
            this.Description = Description;
        }

        public int TransactionID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public int? AccountID { get; set; }

        public int? VendorID { get; set; }

        public int? CategoryID { get; set; }

        public virtual Account Account { get; set; }

        public virtual Category Category { get; set; }

        public virtual Vendor Vendor { get; set; }
    }
}
