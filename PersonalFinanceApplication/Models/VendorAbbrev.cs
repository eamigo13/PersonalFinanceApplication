namespace PersonalFinanceApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("VendorAbbrev")]
    public partial class VendorAbbrev
    {
        [Key]
        public int AbbrevID { get; set; }

        public int? VendorID { get; set; }

        [Required]
        [StringLength(50)]
        public string Abbrev { get; set; }

        public virtual Vendor Vendor { get; set; }
    }
}
