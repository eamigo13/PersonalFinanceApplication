using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonalFinanceApplication.Models
{
    public class Column
    {
        public Column(int colnum, string value)
        {
            this.ColumnNumber = colnum;
            this.ExampleValue = value;
        }

        public int ColumnNumber { get; set; }
        public string ExampleValue { get; set; }
    }
}