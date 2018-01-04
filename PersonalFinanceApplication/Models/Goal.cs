namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Goal")]
    public partial class Goal
    {
        public int GoalID { get; set; }

        public string GoalName { get; set; }

        [Required]
        public int AccountID { get; set; }

        [Required]
        public int BudgetID { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public decimal BeginningAmount { get; set; }

        [Required]
        public decimal EndAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        //[ForeignKey("AccountID")]
        //public virtual Account Account { get; set; }

        //[ForeignKey("BudgetID")]
        //public virtual Budget Budget { get; set; }
    }
}
