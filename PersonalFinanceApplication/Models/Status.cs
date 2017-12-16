using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    [Table("Status")]
    public class Status
    {
        public int StatusID { get; set; }

        public string StatusDesc { get; set; }
    }
}