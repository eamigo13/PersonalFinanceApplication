using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using PersonalFinanceApplication.Models;

namespace PersonalFinanceApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Upload");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            //var parser = new TextFieldParser(@"c:\Users\eamig\source\repos\PersonalFinanceApplication\PersonalFinanceApplication\Content\csv\bk_download.csv");
            //parser.TextFieldType = FieldType.Delimited;
            //parser.SetDelimiters(new string[] { "," });

            //ViewBag.table = new List<string[]>();

            //var columns = new List<Column>();

            //string[] firstrow = parser.ReadFields();

            //for(int i=0; i<firstrow.Length; i++)
            //{
            //    Column column = new Column();
            //    column.ColumnNumber = i;
            //    column.ExampleValue = firstrow[i];
            //    columns.Add(column);
            //}

            //while (!parser.EndOfData)
            //{
            //    string[] row = parser.ReadFields();
            //    /* do something */
            //    ViewBag.table.Add(row);
            //}

            

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}