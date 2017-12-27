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
    public class BudgetController : Controller
    {
        private FinanceContext db = new FinanceContext();

        // GET: Budgets
        public ActionResult Index()
        {
            return View(db.Budgets.ToList());
        }

        //Get: Budgets/View
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }

            //Add the budget to the viewbag
            ViewBag.budget = budget;

            //Return all BudgetCategory rows associated with the budget
            var query = @"  select 
	                            bc.BudgetID,
	                            bc.CategoryID,
	                            bc.Amount,
	                            isnull(( select sum(t.Amount) from [Transaction] t
	                              where t.CategoryID = bc.CategoryID 
	                              and t.Date 
		                            between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                            and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) as UsedAmount,
	                            (bc.Amount - isnull(( select sum(t.Amount) from [Transaction] t
	                              where t.CategoryID = bc.CategoryID 
	                              and t.Date 
		                            between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                            and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) ) as RemainingAmount
	
                            from BudgetCategory bc
                            where bc.BudgetID =" + id;

            var budgetcategories = db.Database.SqlQuery<BudgetCategory>(query).ToList();

            foreach(var bc in budgetcategories)
            {
                db.Entry(bc).State = EntityState.Modified;
            }

            db.SaveChanges();

            //var budgetcategories = db.BudgetCategories.Where(b => b.BudgetID == id).ToList();



            //Add categories to ViewBag
            ViewBag.Categories = db.Categories.ToList();

            

            //Add a select list of the categories

            return View(budgetcategories);
        }

        //Get: Budgets/View
        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }

            //Add the budget to the viewbag
            ViewBag.budget = budget;

            //Return all BudgetCategory rows associated with the budget
            var query = @"  select 
	                            bc.BudgetID,
	                            bc.CategoryID,
	                            bc.Amount,
	                            isnull(( select sum(t.Amount) from [Transaction] t
	                              where t.CategoryID = bc.CategoryID 
	                              and t.Date 
		                            between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                            and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) as UsedAmount,
	                            (bc.Amount - isnull(( select sum(t.Amount) from [Transaction] t
	                              where t.CategoryID = bc.CategoryID 
	                              and t.Date 
		                            between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                            and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) ) as RemainingAmount
	
                            from BudgetCategory bc
                            where bc.BudgetID =" + id;

            var budgetcategories = db.Database.SqlQuery<BudgetCategory>(query).ToList();

            foreach (var bc in budgetcategories)
            {
                db.Entry(bc).State = EntityState.Modified;
            }

            db.SaveChanges();

            //var budgetcategories = db.BudgetCategories.Where(b => b.BudgetID == id).ToList();



            //Add categories to ViewBag
            ViewBag.Categories = db.Categories.ToList();



            //Add a select list of the categories

            return View(budgetcategories);
        }

        [HttpPost]
        public JsonResult AddCategory(BudgetCategory budgetcategory)
        {
            db.BudgetCategories.Add(budgetcategory);
            db.SaveChanges();
            var categoryName = db.Categories.Find(budgetcategory.CategoryID).CategoryName;
            return Json(categoryName);
        }

        [HttpPost]
        public JsonResult UpdateCategory(BudgetCategory budgetcategory)
        {
            db.Entry(budgetcategory).State = EntityState.Modified;
            db.SaveChanges();
            return Json(budgetcategory);
        }

        [HttpPost]
        public JsonResult DeleteCategory(BudgetCategory budgetcategory)
        {
            var deletedbc = db.BudgetCategories.Find(budgetcategory.BudgetID, budgetcategory.CategoryID);
            db.BudgetCategories.Remove(deletedbc);
            db.SaveChanges();
            return Json(budgetcategory);
        }

        // GET: Budgets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        // GET: Budgets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BudgetID,BudgetName,Description,BeginDate,EndDate")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Budgets.Add(budget);
                db.SaveChanges();
                return RedirectToAction("View", new { id = budget.BudgetID } );
            }

            return View(budget);
        }

        //// GET: Budgets/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Budget budget = db.Budgets.Find(id);
        //    if (budget == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(budget);
        //}

        //// POST: Budgets/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "BudgetID,BudgetName,Description,BeginDate,EndDate")] Budget budget)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(budget).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(budget);
        //}

        // GET: Budgets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget budget = db.Budgets.Find(id);
            db.Budgets.Remove(budget);
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
