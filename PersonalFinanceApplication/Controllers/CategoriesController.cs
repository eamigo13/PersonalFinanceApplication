using PersonalFinanceApplication.DAL;
using PersonalFinanceApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PersonalFinanceApplication.Controllers
{
    public class CategoriesController : Controller
    {
        private FinanceContext db = new FinanceContext();

        /* POST -- /Vendors/Add
         * This method adds a new Vendor to the db.
         */
        [HttpPost]
        public JsonResult Add(Category category)
        {
            db.Categories.Add(category);
            db.SaveChanges();
            return Json(category);
        }
    }
}