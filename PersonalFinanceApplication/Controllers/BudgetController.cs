using Newtonsoft.Json;
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

        //Get: Budgets/Expenditures
        public JsonResult Expenditures(Budget b)
        {
            //Find the budget in the database
            int id = b.BudgetID;
            Budget budget = db.Budgets.Find(id);

            /* 
             * Determine how much time is left and how much time has past in the budget in 
             * months, weeks, and days.  These valuse will be used in determining expected 
             * income (for hourly jobs) and the remaining budget for weekly and monthly budget 
             * categories.
             */

            //Today
            DateTime today = DateTime.Today;

            //Determine how many months, weeks, and days are past
            TimeSpan pastTS = new TimeSpan();
            if (today > budget.BeginDate)
            {
                //If the the budget is already started, calculate how much time is past in the budget
                pastTS = today - budget.BeginDate;
            }
            else
            {
                pastTS = TimeSpan.Zero;
            }

            double daysPast = pastTS.Days;
            double weeksPast = daysPast / 7;
            double monthsPast = daysPast / 30.42; //Avg days in a month is 30.42 in a non-leap year.  


            //Determine how many months, weeks, and days are left
            TimeSpan remainingTS = new TimeSpan();
            if (today > budget.BeginDate)
            {
                //If the the budget is already started, calculate how much time is left in the budget
                remainingTS = budget.EndDate - today;
            }
            else
            {
                remainingTS = budget.EndDate - budget.BeginDate;
            }

            double daysRemaining = remainingTS.Days;
            double weeksRemaining = daysRemaining / 7;
            double monthsRemaining = daysRemaining / 30.42; //Avg days in a month is 30.42 in a non-leap year.  


            /*  
             * This section creates all the objects that are needed for the Expenditures tab. 
             */

            //Create a list of budgetcategories related to the budget
            BudgetCategory[] bc = db.BudgetCategories.Where(c => c.BudgetID == id).ToArray();

            //We don't want to incluce "Income" transactions and "Account Transfer" transactions in our calculations of expenditures
            List<string> excludedCategories = new List<string>
            {
                "Income",
                "Account Transfer"
            };

            //Create a list of category ids in the budget to be used in different linq queries
            List<int> categoriesInBudget = (from c in bc
                                            select c.CategoryID).ToList();

            /* Create an array of BudgetCategorySimples to be passed to the client as a json object.
             * The array will have a length of the budgetcategory list + 1 (for the other category which
             * includes all transactions with a category not budgeted for and the 'Total' category).
             */

            BudgetCategorySimple[] budgetCategories = new BudgetCategorySimple[bc.Length + 2];

            //Add a blank budgetcategory to the first index in the array.  This will be written over before the end of the program
            budgetCategories[0] = new BudgetCategorySimple();

            /* Create the 'other' category and and it to the array budgetCategories.  As part of 
             * creation add an array of 'other' transactions to the 'other' category and make the 
             * budget type a One-Time budget.
             */

            BudgetCategorySimple other = new BudgetCategorySimple();

            other.CategoryName = "Other";
            other.CategoryID = -2;

            other.BudgetType = "One-Time";

            other.Amount = (double)budget.OtherAmount;

            other.Transactions = (from t in new FinanceContext().Transactions
                                  where !categoriesInBudget.Contains(t.CategoryID)
                                     && !excludedCategories.Contains(t.Category.CategoryName)
                                     && t.Date >= budget.BeginDate
                                     && t.Date <= budget.EndDate
                                  select new SimpleTransaction
                                  {
                                      TransactionID = t.TransactionID,
                                      CategoryID = t.CategoryID,
                                      CategoryName = t.Category.CategoryName,
                                      Date = t.Date,
                                      Description = t.Description,
                                      Amount = t.Amount
                                  }).ToArray();

            //Convert the date in all the transactions to shortdates strings
            foreach (var item in other.Transactions)
            {
                item.DateString = item.Date.ToShortDateString();
            }

            other.UsedAmount = (double)other.Transactions.Sum(t => t.Amount) * -1;
            other.UsedAmountThisPeriod = other.UsedAmount;

            other.UsedAmountPerPeriod = other.UsedAmount;

            other.RemainingAmount = other.Amount - other.UsedAmount;
            other.RemainingAmountThisPeriod = other.RemainingAmount;

            budgetCategories[1] = other;

            

            //Add all the other information from all the other BudgetCatgories into the arrary of simple budget categories
            for (int i = 0; i < bc.Length; i++)
            {
                //Add the new category to the array
                budgetCategories[i + 2] = createSimpleBudgetCategory(bc[i]);
            }

            /* Create the 'Total' category and and it to the array budgetCategories.  As part of 
             * creation add an array of 'other' transactions to the 'other' category and make the 
             * budget type a One-Time budget.
             */

            BudgetCategorySimple total = new BudgetCategorySimple();

            total.CategoryName = "Total";
            total.CategoryID = -1;

            total.BudgetType = "One-Time";

            total.Amount = (double)budget.OtherAmount;

            total.Transactions = (from t in new FinanceContext().Transactions
                                  where !excludedCategories.Contains(t.Category.CategoryName)
                                     && t.Date >= budget.BeginDate
                                     && t.Date <= budget.EndDate
                                  select new SimpleTransaction
                                  {
                                      TransactionID = t.TransactionID,
                                      CategoryID = t.CategoryID,
                                      CategoryName = t.Category.CategoryName,
                                      Date = t.Date,
                                      Description = t.Description,
                                      Amount = t.Amount
                                  }).ToArray();

            //Convert the date in all the transactions to shortdates strings
            foreach (var item in total.Transactions)
            {
                if (categoriesInBudget.Contains(item.CategoryID))
                {
                    item.Tag = item.CategoryName;
                }
                else
                {
                    item.Tag = "Other";
                }
                item.DateString = item.Date.ToShortDateString();
            }

            total.UsedAmount = budgetCategories.Sum(c => c.UsedAmount);
            total.UsedAmountThisPeriod = total.UsedAmount;

            total.UsedAmountPerPeriod = total.UsedAmount;

            total.RemainingAmount = budgetCategories.Sum(c => c.RemainingAmount);
            total.RemainingAmountThisPeriod = total.RemainingAmount;

            budgetCategories[0] = total;

            //Return the array of simple budget categories as a json object
            return Json(JsonConvert.SerializeObject(budgetCategories));
        }

        //Get: Budgets/View
        public ActionResult View(int? id)
        {
            ViewBag.budget = db.Budgets.Find(id);

            //Create variables for all incomes related with the budget.  Separate into salary incomes and hourly incomes
            var HourlyIncomes = db.Incomes.Where(i => i.BudgetID == id && i.IncomeType.Description == "Hourly").ToList();
            var SalaryIncomes = db.Incomes.Where(i => i.BudgetID == id && i.IncomeType.Description == "Salary").ToList();

            //Calculate expected income.  First sum up expected hourly income remaining and then add salaries
            var expectedIncome = HourlyIncomes.Sum(i => (i.HoursPerWeek * i.Wage * 4));//(decimal)weeksRemaining));
            expectedIncome += SalaryIncomes.Sum(i => (i.Wage));

            //Add incomes to Viewbag
            ViewBag.HourlyIncomes = HourlyIncomes;
            ViewBag.SalaryIncomes = SalaryIncomes;

            //Add related goals to the viewbag
            ViewBag.Goals = db.Goals.Where(g => g.BudgetID == id).ToArray();

            //Add categories to ViewBag
            ViewBag.Categories = db.Categories.ToList();

            //Add Types to the ViewBag
            ViewBag.BudgetTypes = db.BudgetTypes.ToList();

            return View();
        }

        [HttpPost]
        public JsonResult AddCategory(BudgetCategory budgetcategory)
        {
            db.BudgetCategories.Add(budgetcategory);
            db.SaveChanges();
            var categoryName = db.Categories.Find(budgetcategory.CategoryID).CategoryName;

            var budget = db.Budgets.Find(budgetcategory.BudgetID);

            var returnCategory = createSimpleBudgetCategory(budgetcategory);
            
            return Json(returnCategory);
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

            budgetsummary.ExpectedIncome = db.Budgets.Find(budget.BudgetID).OtherAmount;
            budgetsummary.Expenditures = db.BudgetCategories.Where(bc => bc.BudgetID == budget.BudgetID).Sum(bc => bc.Amount);
            budgetsummary.Goals = db.Goals.Where(g => g.BudgetID == budget.BudgetID).Sum(g => g.GoalAmount);
            budgetsummary.Remaining = budgetsummary.ExpectedIncome - budgetsummary.Expenditures - budgetsummary.Goals;

            return Json(budgetsummary);
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

        public BudgetCategorySimple createSimpleBudgetCategory(BudgetCategory bc)
        {
            //Get the budget from the database
            var budget = db.Budgets.Find(bc.BudgetID);

            /* 
             * Determine how much time is left and how much time has past in the budget in 
             * months, weeks, and days.  These valuse will be used in determining expected 
             * income (for hourly jobs) and the remaining budget for weekly and monthly budget 
             * categories.
             */

            //Today
            DateTime today = DateTime.Today;

            //Determine how many months, weeks, and days are past
            TimeSpan pastTS = new TimeSpan();
            if (today > budget.BeginDate)
            {
                //If the the budget is already started, calculate how much time is past in the budget
                pastTS = today - budget.BeginDate;
            }
            else
            {
                pastTS = TimeSpan.Zero;
            }

            double daysPast = pastTS.Days;
            double weeksPast = daysPast / 7;
            double monthsPast = daysPast / 30.42; //Avg days in a month is 30.42 in a non-leap year.  


            //Determine how many months, weeks, and days are left
            TimeSpan remainingTS = new TimeSpan();
            if (today > budget.BeginDate)
            {
                //If the the budget is already started, calculate how much time is left in the budget
                remainingTS = budget.EndDate - today;
            }
            else
            {
                remainingTS = budget.EndDate - budget.BeginDate;
            }

            double daysRemaining = remainingTS.Days;
            double weeksRemaining = daysRemaining / 7;
            double monthsRemaining = daysRemaining / 30.42; //Avg days in a month is 30.42 in a non-leap year.  


            //Create the new simple budget category
            BudgetCategorySimple newCategory = new BudgetCategorySimple();

            //Add the category name
            newCategory.CategoryID = bc.CategoryID;
            newCategory.CategoryName = bc.Category.CategoryName;

            //Add the budget type
            var bt = db.BudgetTypes.Find(bc.BudgetTypeID);
            newCategory.BudgetType = bt.Description;

            //Add the amount
            newCategory.Amount = (double)bc.Amount;

            //Add all the transactions with the same category and within the budget dates
            int CategoryID = bc.CategoryID;
            newCategory.Transactions = (from t in new FinanceContext().Transactions
                                        where t.CategoryID == CategoryID
                                           && t.Date >= budget.BeginDate
                                           && t.Date <= budget.EndDate
                                        select new SimpleTransaction
                                        {
                                            TransactionID = t.TransactionID,
                                            CategoryID = t.CategoryID,
                                            CategoryName = t.Category.CategoryName,
                                            Date = t.Date,
                                            Description = t.Description,
                                            Amount = t.Amount,
                                            Tag = t.Category.CategoryName
                                        }).ToArray();

            //Convert the date in all the transactions to shortdates strings
            foreach (var item in newCategory.Transactions)
            {
                item.DateString = item.Date.ToShortDateString();
            }

            //Add the used amount
            newCategory.UsedAmount = (double)newCategory.Transactions.Sum(t => t.Amount) * -1;

            //Calculate the used amount this period
            switch (newCategory.BudgetType)
            {
                case "Weekly":
                    newCategory.UsedAmountThisPeriod = (double)newCategory.Transactions.Where(t => t.Date >= today.AddDays(-7)).Sum(t => t.Amount) * -1;
                    break;
                case "Monthly":
                    newCategory.UsedAmountThisPeriod = (double)newCategory.Transactions.Where(t => t.Date.Month == today.Month).Sum(t => t.Amount) * -1;
                    break;
                case "One-Time":
                    newCategory.UsedAmountThisPeriod = newCategory.UsedAmount;
                    break;
            }

            //Calculate the used amount per period
            switch (newCategory.BudgetType)
            {
                case "Weekly":
                    newCategory.UsedAmountPerPeriod = newCategory.UsedAmount / weeksPast;
                    break;
                case "Monthly":
                    newCategory.UsedAmountPerPeriod = newCategory.UsedAmount / monthsPast;
                    break;
                case "One-Time":
                    newCategory.UsedAmountPerPeriod = newCategory.UsedAmount;
                    break;
            }

            //Calculate the remaining amount
            switch (newCategory.BudgetType)
            {
                case "Weekly":
                    newCategory.RemainingAmount = newCategory.Amount * weeksRemaining;
                    break;
                case "Monthly":
                    newCategory.RemainingAmount = newCategory.Amount * monthsRemaining;
                    break;
                case "One-Time":
                    newCategory.RemainingAmount = newCategory.Amount - newCategory.UsedAmount;
                    break;
            }

            //Calculate remaining amount this period
            newCategory.RemainingAmountThisPeriod = newCategory.Amount - newCategory.UsedAmountThisPeriod;

            return newCategory;
        }
    }
}

