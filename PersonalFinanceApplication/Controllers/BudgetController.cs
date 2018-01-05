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

        //Get: Budgets
        public ActionResult Index()
        {
            Budget budget = db.Budgets.Find(0);

            ViewBag.begindate = budget.BeginDate.ToString("yyyy-MM-dd");
            ViewBag.enddate = budget.EndDate.ToString("yyyy-MM-dd");

            ViewBag.budget = budget;

            //Add categories to ViewBag
            ViewBag.Categories = db.Categories.ToList();

            //Add Types to the ViewBag
            ViewBag.BudgetTypes = db.BudgetTypes.ToList();

            //Add Income Types to the ViewBag
            ViewBag.IncomeTypes = db.IncomeTypes.ToList();

            return View();
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

        

        public JsonResult GetGoals(Budget b)
        {
            var goals = db.Goals.Where(g => g.BudgetID == b.BudgetID);
            return Json(JsonConvert.SerializeObject(goals));
        }

        public JsonResult GetIncomes(Budget b)
        {
            var incomes = db.Incomes.Where(i => i.BudgetID == b.BudgetID).ToList();
            var simpleincomes = new IncomeSimple[incomes.Count()];

            for(int i=0; i<incomes.Count(); i++)
            {
                simpleincomes[i] = createSimpleIncome(incomes[i]);
            }
            return Json(JsonConvert.SerializeObject(simpleincomes));
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
        public JsonResult UpdateCategory(BudgetCategory bc)
        {
            var budgetcategory = db.BudgetCategories.Find(bc.BudgetID, bc.CategoryID);
            budgetcategory.Amount = bc.Amount;
            db.Entry(budgetcategory).State = EntityState.Modified;
            db.SaveChanges();
            var returnCategory = createSimpleBudgetCategory(budgetcategory);
            return Json(returnCategory);
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
            return Json(goal);
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
        public JsonResult AddIncome(Income income)
        {
            db.Incomes.Add(income);
            db.SaveChanges();

            var returnincome = createSimpleIncome(db.Incomes.Find(income.IncomeID));
            return Json(returnincome);
        }

        [HttpPost]
        public JsonResult UpdateIncome(Income updatedincome)
        {
            var income = db.Incomes.Find(updatedincome.IncomeID);
            income.Wage = updatedincome.Wage;
            income.HoursPerWeek = updatedincome.HoursPerWeek;
            db.Entry(income).State = EntityState.Modified;
            db.SaveChanges();
            var returnincome = createSimpleIncome(income);
            return Json(returnincome);
        }

        [HttpPost]
        public JsonResult DeleteIncome(Income income)
        {
            var deletedincome = db.Incomes.Find(income.IncomeID);
            db.Incomes.Remove(deletedincome);
            db.SaveChanges();
            var returnincome = createSimpleIncome(deletedincome);
            return Json(returnincome);
        }

        [HttpPost]
        public JsonResult UpdateOtherAmount(Budget b)
        {
            var budget = db.Budgets.Find(0);
            budget.OtherAmount = b.OtherAmount;
            db.Entry(budget).State = EntityState.Modified;
            db.SaveChanges();
            return Json("Other");
        }

        [HttpPost]
        public JsonResult UpdateBudgetDates(Budget b)
        {
            var budget = db.Budgets.Find(0);
            budget.BeginDate = b.BeginDate;
            budget.EndDate = b.EndDate;
            db.Entry(budget).State = EntityState.Modified;
            db.SaveChanges();
            return Json("");
        }

        [HttpPost]
        public JsonResult GetBudgetSummary()       
        {
            Budget budget = db.Budgets.Find(0);

            //Variables needed
            double remainingIncome = 0;
            double remainingExpenses = 0;
            double remainingGoals = 0;
            double budgetsummary = 0;


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
            if (today > budget.EndDate)
            {
                remainingTS = TimeSpan.Zero;
            }
            else if (today > budget.BeginDate)
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

            //Sum up the remaining incomes of all incomes associated with the budget
            var incomes = db.Incomes.ToList();

            foreach(var income in incomes)
            {
                switch(income.IncomeType.Description)
                {
                    case "Salary":
                        remainingIncome += ((double)income.Wage / 52) * weeksRemaining;
                        break;
                    case "Hourly":
                        remainingIncome += ((double)income.Wage * income.HoursPerWeek) * weeksRemaining;
                        break;
                }
            }

            //Sum up the remaining budgeted expenditures
            var budgetcategories = db.BudgetCategories.ToList();
            
            foreach(var category in budgetcategories)
            {
                switch (category.BudgetType.Description)
                {
                    case "Weekly":
                        remainingExpenses += (double)category.Amount * weeksRemaining;
                        break;
                    case "Monthly":
                        remainingExpenses += (double)category.Amount * monthsRemaining;
                        break;
                    case "One-Time":
                        remainingExpenses += (double)category.Amount - calculateUsedAmount(category.CategoryID);
                        break;
                }
            }

            //Add remaining other amount (if more than 0 dollars is remaining
            double remainingOtherExpense = (double)budget.OtherAmount - calculateOtherAmountUsed();
            if(remainingOtherExpense > 0)
            {
                remainingExpenses += remainingOtherExpense;
            }

            //Calculate remaining goal amounts
            var goals = db.Goals;

            remainingGoals = (double)goals.Sum(g => (g.EndAmount - g.CurrentAmount));

            budgetsummary = remainingIncome - remainingGoals - remainingExpenses;

            return Json(Math.Round(budgetsummary, 2));
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

        public IncomeSimple createSimpleIncome(Income i)
        {
            IncomeSimple income = new IncomeSimple();

            income.IncomeID = i.IncomeID;
            income.IncomeName = i.IncomeName;
            income.BudgetID = i.BudgetID;
            income.IncomeTypeID = i.IncomeTypeID;
            income.HoursPerWeek = i.HoursPerWeek;
            income.Wage = i.Wage;
            income.IncomeTypeDescription = db.IncomeTypes.Find(i.IncomeTypeID).Description;

            return income;
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


        //Calculate the used amount of a category
        public double calculateUsedAmount(int categoryid)
        {
            var budget = db.Budgets.Find(0);

            //Get all transactions within the budget date and related to the category
            var transactions = db.Transactions.Where(t => t.CategoryID == categoryid
                                                          && t.Date >= budget.BeginDate
                                                          && t.Date <= budget.EndDate).ToList();

            //calulate the sume of transaction amounts
            double usedamount = (double)transactions.Sum(t => t.Amount) * -1;

            return usedamount;
        }

        public double calculateOtherAmountUsed()
        {
            var budget = db.Budgets.Find(0);

            List<string> excludedCategories = new List<string>
            {
                "Income",
                "Account Transfer"
            };

            var categoriesInBudget = (from c in db.BudgetCategories
                                      select c.CategoryID).ToList();

            double otherAmountUsed = (double)(from t in new FinanceContext().Transactions
                                              where !categoriesInBudget.Contains(t.CategoryID)
                                                 && !excludedCategories.Contains(t.Category.CategoryName)
                                                 && t.Date >= budget.BeginDate
                                                 && t.Date <= budget.EndDate
                                              select t.Amount).Sum() * -1;

            return otherAmountUsed;
        }
    }
}

