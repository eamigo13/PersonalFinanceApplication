namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Account")]
    public partial class Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Account()
        {
            Transactions = new HashSet<Transaction>();
            Goals = new HashSet<Goal>();
        }

        public int AccountID { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountName { get; set; }

        public int? AccountTypeID { get; set; }

        public virtual AccountType AccountType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }
    }
}
