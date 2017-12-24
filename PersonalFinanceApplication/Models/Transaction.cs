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
        public Transaction()
        {

        }

        public Transaction(int BatchID, int AccountID, DateTime Date, string Description, decimal Amount, int StatusID)
        {
            this.BatchID = BatchID;
            this.AccountID = AccountID;
            this.Date = Date;
            this.Description = Description;
            this.Amount = Amount;
            this.StatusID = StatusID;
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

        public int? BatchID { get; set; }

        public int? StatusID { get; set; }

        public bool VendorDetected { get; set; }

        public virtual Batch Batch { get; set; }

        public virtual Account Account { get; set; }

        public virtual Category Category { get; set; }

        public virtual Vendor Vendor { get; set; }

        public virtual Status Status { get; set; }

    }
}
