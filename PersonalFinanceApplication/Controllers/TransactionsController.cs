using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PersonalFinanceApplication.DAL;
using PersonalFinanceApplication.Models;

namespace PersonalFinanceApplication.Controllers
{
    public class TransactionsController : Controller
    {
        private FinanceContext db = new FinanceContext();

        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = db.Transactions.Include(t => t.Batch);
            return View(transactions.ToList());
        }


        // GET: Transactions/Confirm
        public ActionResult FindVendor(int batchid)
        {
            //retrieve all transactions associated with the batch
            var transactions = db.Transactions.Where(t => t.BatchID == batchid);

            //retrieve a list of vendor description strings and store them in a dictionary
            var vndrstrings = db.VendorAbbrevs.ToList();

            

            //Right now I'm setting each vendor and each category to unknown
            //This will be updated in the future
            foreach(var transaction in transactions) 
            {
                //boolean used to determine if vendor was found in description
                bool VendorFound = false;

                foreach (var vndrstring in vndrstrings)
                {
                    if(transaction.Description.Contains(vndrstring.Abbrev))
                    {
                        transaction.VendorID = vndrstring.VendorID;
                        VendorFound = true;
                        break;
                    }
                }

                //if no vendor was found, assign vendor to 'Unknown'
                if (!VendorFound)
                {
                    transaction.VendorID = 0;
                }

                //Curently setting all categories to unknown.  Will be changed.
                transaction.CategoryID = 0;  
            }

            db.SaveChanges();

            return RedirectToAction("Confirm", new { batchid = batchid });
        }

        // GET: Transactions/Confirm
        public ActionResult Confirm(int? batchid)
        {
            var transactions = new List<Transaction>();

            if (batchid == null)
            {
                //If no parameter is passed in, return all unconfirmed transactions
                transactions = db.Transactions.Where(t => t.StatusID == 0).ToList(); 
            }
            else
            {
                //if a batch id is passed in, only return transactions associated with that batch
                transactions = db.Transactions.Where(t => t.BatchID == batchid).ToList();
            }

            //Pass a list of categories and vendors to the viewbag to be used in a select list
            ViewBag.Vendors = db.Vendors.ToList();
            ViewBag.Categories = db.Categories.ToList();

            return View(transactions);
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            ViewBag.BatchID = new SelectList(db.Batches, "BatchID", "BatchID");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransactionID,Description,Date,Amount,AccountID,VendorID,CategoryID,BatchID")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BatchID = new SelectList(db.Batches, "BatchID", "BatchID", transaction.BatchID);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.BatchID = new SelectList(db.Batches, "BatchID", "BatchID", transaction.BatchID);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransactionID,Description,Date,Amount,AccountID,VendorID,CategoryID,BatchID")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BatchID = new SelectList(db.Batches, "BatchID", "BatchID", transaction.BatchID);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
