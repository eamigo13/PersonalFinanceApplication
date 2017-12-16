using System;
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

        // GET: Upload/ResolveErrors
        public ActionResult ResolveErrors(int batchid)
        {
            ViewBag.batchid = batchid;
            var failedrows = TempData["failedrows"] as List<FailedRow>;
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

            //This array will contain all the transactions that failed to save to the db and their associated row information
            List<FailedRow> FailedRows = new List<FailedRow>();

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

                //Error variables.  
                bool ValidRow = true;
                FailedRow FailedRow = new FailedRow();

                //Determine if the date field can be converted to a date
                try
                {
                    date = Convert.ToDateTime(row[DateColumn]);
                    FailedRow.Date = date;
                }
                catch
                {
                    FailedRow.invalidDate = row[DateColumn];
                    FailedRow.ErrorID = 1;
                    FailedRow.ErrorMessage += "Invalid Date.  Please enter a valid date.\n";
                    ValidRow = false;
                }

                //Determine if the amount field can be converted to a decimal
                try
                {
                    amount = Decimal.Parse(row[AmountColumn]);
                    FailedRow.Amount = amount;
                }
                catch
                {
                    FailedRow.invalidDate = row[AmountColumn];
                    FailedRow.ErrorID = 2;
                    FailedRow.ErrorMessage += "Invalid Amount.  Please enter a valid amount.\n";
                    ValidRow = false;
                }

                //Determine if the description field has a value
                if (row[DescriptionColumn] != "")
                {
                    description = row[DescriptionColumn];
                    FailedRow.Description = description;
                }
                else
                {
                    FailedRow.ErrorID = 3;
                    FailedRow.ErrorMessage += "No Description.  Please enter a description.\n";
                    ValidRow = false;
                }

                //Create a transaction from the row and add it to the db if all the fields are valid
                if(ValidRow)
                {
                    Transaction transaction = new Transaction(batch.BatchID, AccountID, date, description, amount, 0);

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
                        FailedRow.ErrorID = 4;
                        FailedRow.ErrorMessage += "Duplicate transaction cannot be uploaded.\n";
                        ValidRow = false;
                    }
                }

                if(!ValidRow)
                {
                    //If the row is invalid add it to the invalid rows dictionary along with the error message
                    string[] failedrow = { row[DateColumn], row[DescriptionColumn], row[AmountColumn] };
                    FailedRows.Add(FailedRow);
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
                //If there are failed rows, redirect to ResolveErrors action
                TempData["failedrows"] = FailedRows;
                return RedirectToAction("ResolveErrors", new { batchid = batch.BatchID } );
            }
            else
            {
                //If there are no failed rows, redirect to Confirmed action
                return RedirectToAction("FindVendor", "Transaction", new { batchid = batch.BatchID } );
            }

            //Delete the import.csv file
            //FileInfo file = new FileInfo(FilePath);
            //file.Delete();
        }

        [HttpPost]
        public ActionResult ResolveErrors()
        {
            return RedirectToAction("Index");
        }
    }
}