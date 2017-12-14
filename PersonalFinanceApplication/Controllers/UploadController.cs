﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualBasic.FileIO;
using PersonalFinanceApplication.Models;
using PersonalFinanceApplication.DAL;
using System.IO;
using System.Data.Entity.Validation;

namespace PersonalFinanceApplication.Controllers
{
    public class UploadController : Controller
    {
        private FinanceContext db = new FinanceContext();

        // GET: Upload
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        // GET: Upload/SelectColumns
        /* This action parses through the first row of the CSV file and generates 
         * a list of columns containing the column number and an example of the column
         * value so the user can associate what columns are associated with the different
         * transaction information
         */
        public ActionResult SelectColumns()
        {
            //Create a new parser to parse through the csv file
            string FilePath = Path.Combine(Server.MapPath("~/Content/csv/import.csv"));
            var parser = new TextFieldParser(FilePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(new string[] { "," });

            //Create a list of columns to be associated with each 'column' in the csv file
            var columns = new List<Column>();

            //Parse the first row of the file to get an example of each piece of data.
            string[] firstrow = parser.ReadFields();

            //Check each record in the first row.  If it contains a value, add it to the list of columns
            for (int i = 0; i < firstrow.Length; i++)
            {
                if (firstrow[i] != "")
                {
                    Column column = new Column(i, firstrow[i]);
                    columns.Add(column);
                }
            }

            /* Pass the list of columns to the view where the user can select
             * which columns are associated with the transaction amount, date
             * and description.
             */
            return View(columns);
        }

        // GET: Upload/ResolveFailedRows
        public ActionResult ResolveFailedRows()
        {
            var failedrows = TempData["failedrows"] as Dictionary<Transaction, string[]>;
            return View(failedrows);
        }


        /* POST: Upload
         * 
         * This action validates that the user is uploading a csv file and then saves it as import.csv.  
         * If the upload is successful, the program continues to the SelectColumns action
         */
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0 && file.FileName.Substring(file.FileName.Length - 4, 4) == ".csv")
                {
                    string FilePath = Path.Combine(Server.MapPath("~/Content/csv/import.csv"));
                    file.SaveAs(FilePath);
                    return RedirectToAction("SelectColumns");
                }
                else
                {
                    return RedirectToAction("Index", new { message = "Please upload a valid .csv file" });
                }
            }
            catch
            {
                return RedirectToAction("Index", new { message = "File upload failed! Please Try Again" } );
            }
        }

        // POST: Upload/SelectColumns
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /* This action associates the columns in the csv file to the transaction information 
         * according to the user's input.  
         */
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult SelectColumns([Bind(Include = "DateColumn")] int DateColumn,
                                          [Bind(Include = "AmountColumn")] int AmountColumn,
                                          [Bind(Include = "DescriptionColumn")] int DescriptionColumn)
        {
            //Create a new parser to parse through the csv file
            string FilePath = Path.Combine(Server.MapPath("~/Content/csv/import.csv"));
            var parser = new TextFieldParser(FilePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(new string[] { "," });

            //This dictionary will contain all the transactions that failed to save to the db and their associated row information
            Dictionary<Transaction, string[]> FailedRows = new Dictionary<Transaction, string[]>();

            //parse through each row, grab the information from appropiate columns, create a new transaction, and add it to the db
            //if the row is invalid, add it to the failed rows list
            int i = 0;
            while (!parser.EndOfData)
            {
                //Parse through the next row in the file
                string[] row = parser.ReadFields();


                DateTime date = Convert.ToDateTime(row[DateColumn]);
                Decimal amount = Decimal.Parse(row[AmountColumn]);
                Transaction transaction = new Transaction(date, amount, row[DescriptionColumn]);

                try
                {
                    db.Transactions.Add(transaction);
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    //Violation of Unique Constraint.  InnerException.Class = 14
                    //EntityValidationErrors.  EntityValidationErros.Count > 0
                    FailedRows.Add(transaction, row);
                    i++;
                }
            }

            //If there are failed rows, redirect to ResolveFailedRows action, else return to UploadConfirmed action
            if(FailedRows.Count > 0)
            {
                TempData["failedrows"] = FailedRows;
                return RedirectToAction("ResolveFailedRows");
            }
            else
            {
                return RedirectToAction("Index");
            }

            //Delete the import.csv file
            //FileInfo file = new FileInfo(FilePath);
            //file.Delete();


        }
    }
}