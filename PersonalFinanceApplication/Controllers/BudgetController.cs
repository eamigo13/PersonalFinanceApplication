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

            //Add all the goals associated with the budget to the viewbag
            ViewBag.Goals = db.Goals.Where(g => g.BudgetID == id);

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





            //Calculate expected income

            //Create variables for all incomes related with the budget.  Separate into salary incomes and hourly incomes
            var HourlyIncomes = db.Incomes.Where(i => i.BudgetID == id && i.IncomeType.Description == "Hourly").ToList();
            var SalaryIncomes = db.Incomes.Where(i => i.BudgetID == id && i.IncomeType.Description == "Salary").ToList();

            //Today's Date
            DateTime today = DateTime.Today;

            //Week's remaining in budget 
            TimeSpan ts = budget.EndDate - today;
            decimal weeksRemaining = ts.Days / 7;

            //Calculate expected income.  First sum up expected hourly income remaining and then add salaries
            var expectedIncome = HourlyIncomes.Sum(i => (i.HoursPerWeek * i.Wage * weeksRemaining));
            expectedIncome += SalaryIncomes.Sum(i => (i.Wage));

            //Add incomes to Viewbag
            ViewBag.HourlyIncomes = HourlyIncomes;
            ViewBag.SalaryIncomes = SalaryIncomes;

            




            //Add related goals to the viewbag
            ViewBag.Goals = db.Goals.Where(g => g.BudgetID == id).ToArray();

            //Return all BudgetCategory rows associated with the budget.  The following SQL updates the used amount and remaining amount
            var categoryquery = @"  select 
	                                    bc.BudgetID,
	                                    bc.CategoryID,
	                                    bc.Amount,
	                                    isnull(( select (sum(t.Amount)*-1) from [Transaction] t
	                                      where t.CategoryID = bc.CategoryID 
	                                      and t.Date 
		                                    between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                                    and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) as UsedAmount,
	                                    (bc.Amount - isnull(( select (sum(t.Amount) * -1) from [Transaction] t
	                                      where t.CategoryID = bc.CategoryID 
	                                      and t.Date 
		                                    between (select b.BeginDate from Budget b where b.BudgetID = bc.BudgetID) 
		                                    and (select b.EndDate from Budget b where b.BudgetID = bc.BudgetID) ), 0 ) ) as RemainingAmount
	
                                    from BudgetCategory bc
                                    where bc.BudgetID =" + id;

            var budgetcategories = db.Database.SqlQuery<BudgetCategory>(categoryquery).ToList();
            //var totalamountquery = @"";




            //Save the changes to each budgetcategory row to the database
            foreach (var bc in budgetcategories)
            {
                db.Entry(bc).State = EntityState.Modified;
            }

            db.SaveChanges();

            //Add all the used amounts to the viewbag
            var usedAmounts = new decimal[budgetcategories.Count() + 1]; //used amounts for each budget category plus one for 'other' used amount
            var categoryNames = new string[budgetcategories.Count() + 1]; //category name for each budget category plus one for 'other' 
            var nameAmountsDictionary = new Dictionary<string, decimal[]>();
            var categoryTransactionDictionary = new Dictionary<string, SimpleTransaction[]>();

            List<string> excludedCategories = new List<string>
            {
                "Income",
                "Account Transfer"
            };

            //Get all transactions between the begin date and end date of the budget
            var allBudgetTransactions = (from t in new FinanceContext().Transactions
                                         where t.Date >= budget.BeginDate && t.Date <= budget.EndDate && !excludedCategories.Contains(t.Category.CategoryName)
                                         select new SimpleTransaction {
                                             TransactionID = t.TransactionID,
                                             CategoryID = t.CategoryID,
                                             CategoryName = t.Category.CategoryName,
                                             Date = t.Date,
                                             Description = t.Description,
                                             Amount = t.Amount }).ToArray();

            foreach (var item in allBudgetTransactions)
            {
                item.DateString = item.Date.ToShortDateString();
            }

            var totalAmountBudgeted = budgetcategories.Sum(c => c.Amount);
            var totalAmountUsed = allBudgetTransactions.Sum(t => t.Amount) * -1;
            var totalAmountRemaining = totalAmountBudgeted - totalAmountUsed;

            List<int> categoriesInBudget = (from bc in budgetcategories
                                            select bc.CategoryID).ToList();



            var otherTransactions = (from t in new FinanceContext().Transactions
                                     where !categoriesInBudget.Contains(t.CategoryID) && !excludedCategories.Contains(t.Category.CategoryName)
                                     select new SimpleTransaction
                                     {
                                         TransactionID = t.TransactionID,
                                         CategoryID = t.CategoryID,
                                         CategoryName = t.Category.CategoryName,
                                         Date = t.Date,
                                         Description = t.Description,
                                         Amount = t.Amount
                                     }).ToArray();

            var otherAmount = otherTransactions.Sum(t => t.Amount) * -1;

            categoryTransactionDictionary.Add("Other", otherTransactions);
            categoryTransactionDictionary.Add("All", allBudgetTransactions);
            decimal[] allusedremaining = { totalAmountUsed, totalAmountRemaining };
            nameAmountsDictionary.Add("All", allusedremaining );

            usedAmounts[0] = otherAmount;
            categoryNames[0] = "Other";

            for (int i = 1; i < budgetcategories.Count() + 1; i++)
            {
                usedAmounts[i] = (budgetcategories[i - 1].UsedAmount);
                categoryNames[i] = budgetcategories[i - 1].Category.CategoryName;

                decimal[] usedremaining = { budgetcategories[i - 1].UsedAmount, budgetcategories[i - 1].RemainingAmount };
                nameAmountsDictionary.Add(budgetcategories[i - 1].Category.CategoryName, usedremaining);

                var categoryTransations = allBudgetTransactions.Where(t => t.CategoryID == budgetcategories[i - 1].CategoryID).ToArray();
                categoryTransactionDictionary.Add(budgetcategories[i - 1].Category.CategoryName, categoryTransations);

            }

            ViewBag.usedAmounts = usedAmounts;
            ViewBag.categoryNames = categoryNames;
            ViewBag.nameAmountsDictionary = nameAmountsDictionary;
            ViewBag.categoryTransactionDictionary = categoryTransactionDictionary;

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

        [HttpPost]
        public JsonResult AddGoal(Goal goal)
        {
            db.Goals.Add(goal);
            db.SaveChanges();
            return Json(goal.GoalID);
        }

        [HttpPost]
        public JsonResult UpdateGoal(Goal updatedgoal)
        {
            var goal = db.Goals.Find(updatedgoal.GoalID);
            goal.CurrentAmount = updatedgoal.CurrentAmount;
            goal.EndAmount = updatedgoal.EndAmount;
            db.Entry(goal).State = EntityState.Modified;
            db.SaveChanges();
            return Json(goal);
        }

        [HttpPost]
        public JsonResult DeleteGoal(Goal goal)
        {
            var deletedgoal = db.Goals.Find(goal.GoalID);
            db.Goals.Remove(deletedgoal);
            db.SaveChanges();
            return Json(deletedgoal);
        }

        [HttpPost]
        public JsonResult GetBudgetSummary(Budget budget)
        {
            var budgetsummary = new BudgetSummary();

            budgetsummary.ExpectedIncome = db.Budgets.Find(budget.BudgetID).ExpectedIncome;
            budgetsummary.Expenditures = db.BudgetCategories.Where(bc => bc.BudgetID == budget.BudgetID).Sum(bc => bc.Amount);
            budgetsummary.Goals = db.Goals.Where(g => g.BudgetID == budget.BudgetID).Sum(g => g.GoalAmount);
            budgetsummary.Remaining = budgetsummary.ExpectedIncome - budgetsummary.Expenditures - budgetsummary.Goals;

            return Json(budgetsummary);
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
