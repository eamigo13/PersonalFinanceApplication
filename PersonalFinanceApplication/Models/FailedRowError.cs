using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("FailedRowErrors")]
    public class FailedRowError
    {
        [Key]
        public int ErrorID { get; set; }

        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
    }
}