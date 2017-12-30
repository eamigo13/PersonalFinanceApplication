namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Budget")]
    public partial class Budget
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Budget()
        {
            BudgetCategories = new HashSet<BudgetCategory>();
            Goals = new HashSet<Goal>();
        }

        public int BudgetID { get; set; }

        [Required]
        [StringLength(50)]
        public string BudgetName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public DateTime BeginDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal ExpectedIncome { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }
    }
}
