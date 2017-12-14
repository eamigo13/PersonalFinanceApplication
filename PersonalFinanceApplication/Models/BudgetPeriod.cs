namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BudgetPeriod")]
    public partial class BudgetPeriod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BudgetPeriod()
        {
            Budgets = new HashSet<Budget>();
        }

        [Key]
        public int PeriodID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodBegin { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodEnd { get; set; }

        [Required]
        [StringLength(50)]
        public string PeriodName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Budget> Budgets { get; set; }
    }
}
