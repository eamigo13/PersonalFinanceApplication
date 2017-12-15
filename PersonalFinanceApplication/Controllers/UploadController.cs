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
using System.Data.Entity;

namespace PersonalFinanceApplication.Controllers
{
    public class UploadController : Controller
    {
        private FinanceContext db = new FinanceContext();

        // GET: Upload
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            ViewBag.Accounts = db.Accounts;
            return View();
        }

        // GET: Upload/SelectColumns
        /* This action parses through the first row of the CSV file and generates 
         * a list of columns containing the column number and an example of the column
         * value so the user can associate what columns are associated with the different
         * transaction information
         */
        public ActionResult SelectColumns(int accountid)
        {
            //Pass the account id to the viewbag
            ViewBag.AccountID = accountid;

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
            var failedrows = TempData["failedrows"] as Dictionary<string[], string>;
            return View(failedrows);
        }


        /* POST: Upload
         * 
         * This action validates that the user is uploading a csv file and then saves it as import.csv.  
         * If the upload is successful, the program continues to the SelectColumns action
         */
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult UploadFile(HttpPostedFileBase file, [Bind( Include = "AccountID" )] int accountid)
        {
            try
            {
                if (file.ContentLength > 0 && file.FileName.Substring(file.FileName.Length - 4, 4) == ".csv")
                {
                    string FilePath = Path.Combine(Server.MapPath("~/Content/csv/import.csv"));
                    file.SaveAs(FilePath);
                    return RedirectToAction("SelectColumns", new { accountid = accountid } );
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


        /* POST: Upload/SelectColumns
         * 
         * This action associates the columns in the csv file to the transaction information 
         * according to the user's input.  It then creates each transaction and saves it to the db.  
         */

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult SelectColumns([Bind(Include = "AccountID")] int AccountID, 
                                          [Bind(Include = "DateColumn")] int DateColumn,
                                          [Bind(Include = "AmountColumn")] int AmountColumn,
                                          [Bind(Include = "DescriptionColumn")] int DescriptionColumn)
        {
            //Create a new batch and save it to the db.
            Batch batch = new Batch(DateTime.Now);
            db.Batches.Add(batch);
            db.SaveChanges();

            //Variables that track how many rows have succeeded and failed
            int SuccessCount = 0;
            int FailCount = 0;

            //Create a new parser to parse through the csv file
            string FilePath = Path.Combine(Server.MapPath("~/Content/csv/import.csv"));
            var parser = new TextFieldParser(FilePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(new string[] { "," });

            //This dictionary will contain all the transactions that failed to save to the db and their associated row information
            Dictionary<string[], string> FailedRows = new Dictionary<string[], string>();

            //parse through each row, grab the information from appropiate columns, create a new transaction, and add it to the db
            //if the row is invalid, add it to the failed rows list
            while (!parser.EndOfData)
            {
                //Parse through the next row in the file
                string[] row = parser.ReadFields();

                //These are the variables we will need to create a new transaction
                DateTime date = new DateTime();
                decimal amount = new decimal();
                string description = "";

                //Error Message variables
                string ErrorMessage = "";
                bool ValidRow = true;

                //Determine if the date field can be converted to a date
                try { date = Convert.ToDateTime(row[DateColumn]); }
                catch { ErrorMessage = "Invalid Date"; ValidRow = false; }

                //Determine if the amount field can be converted to a decimal
                try { amount = Decimal.Parse(row[AmountColumn]); }
                catch { ErrorMessage = "Invalid Amount"; ValidRow = false; }

                //Determine if the description field has a value
                if (row[DescriptionColumn] != "") { description = row[DescriptionColumn]; }
                else { ErrorMessage = "Invalid Description"; ValidRow = false; }

                //Create a transaction from the row and add it to the db if all the fields are valid
                if(ValidRow)
                {
                    Transaction transaction = new Transaction(batch.BatchID, AccountID, date, description, amount);

                    try
                    {
                        //Try to add the transaction to the db.
                        
                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        //If there is an exception, add an error message and mark the row as invalid
                        db.Transactions.Remove(transaction);
                        ErrorMessage = "This row has already been added";
                        ValidRow = false;
                    }
                }

                if(!ValidRow)
                {
                    //If the row is invalid add it to the invalid rows dictionary along with the error message
                    string[] failedrow = { row[DateColumn], row[DescriptionColumn], row[AmountColumn] };
                    FailedRows.Add(failedrow, ErrorMessage );
                    FailCount++;
                }
                else
                {
                    SuccessCount++;
                }
            }

            //Add the number of successful and failed rows to the batch
            Batch updateBatch = db.Batches.Find(batch.BatchID);
            updateBatch.SuccessRows = SuccessCount;
            updateBatch.FailedRows = FailCount;
            db.Entry(updateBatch).State = EntityState.Modified;
            db.SaveChanges();

            if (FailedRows.Count > 0)
            {
                //If there are failed rows, redirect to ResolveFailedRows action
                TempData["failedrows"] = FailedRows;
                return RedirectToAction("ResolveFailedRows");
            }
            else
            {
                //If there are no failed rows, redirect to Confirmed action
                return RedirectToAction("Index");
            }

            //Delete the import.csv file
            //FileInfo file = new FileInfo(FilePath);
            //file.Delete();


        }
    }
}