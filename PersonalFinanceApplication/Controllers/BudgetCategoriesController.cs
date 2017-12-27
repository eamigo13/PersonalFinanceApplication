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
    public class BudgetCategoriesController : Controller
    {
        private FinanceContext db = new FinanceContext();

        // GET: BudgetCategories
        public ActionResult Index()
        {
            var budgetCategories = db.BudgetCategories.Include(b => b.Budget).Include(b => b.Category);
            return View(budgetCategories.ToList());
        }

        // GET: BudgetCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetCategory budgetCategory = db.BudgetCategories.Find(id);
            if (budgetCategory == null)
            {
                return HttpNotFound();
            }
            return View(budgetCategory);
        }

        // GET: BudgetCategories/Create
        public ActionResult Create()
        {
            ViewBag.BudgetID = new SelectList(db.Budgets, "BudgetID", "BudgetName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: BudgetCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BudgetID,CategoryID,Amount")] BudgetCategory budgetCategory)
        {
            if (ModelState.IsValid)
            {
                db.BudgetCategories.Add(budgetCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BudgetID = new SelectList(db.Budgets, "BudgetID", "BudgetName", budgetCategory.BudgetID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", budgetCategory.CategoryID);
            return View(budgetCategory);
        }

        // GET: BudgetCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetCategory budgetCategory = db.BudgetCategories.Find(id);
            if (budgetCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.BudgetID = new SelectList(db.Budgets, "BudgetID", "BudgetName", budgetCategory.BudgetID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", budgetCategory.CategoryID);
            return View(budgetCategory);
        }

        // POST: BudgetCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BudgetID,CategoryID,Amount")] BudgetCategory budgetCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budgetCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BudgetID = new SelectList(db.Budgets, "BudgetID", "BudgetName", budgetCategory.BudgetID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", budgetCategory.CategoryID);
            return View(budgetCategory);
        }

        // GET: BudgetCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetCategory budgetCategory = db.BudgetCategories.Find(id);
            if (budgetCategory == null)
            {
                return HttpNotFound();
            }
            return View(budgetCategory);
        }

        // POST: BudgetCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BudgetCategory budgetCategory = db.BudgetCategories.Find(id);
            db.BudgetCategories.Remove(budgetCategory);
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
