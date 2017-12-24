using PersonalFinanceApplication.DAL;
using PersonalFinanceApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PersonalFinanceApplication.Controllers
{
    public class VendorsController : Controller
    {
        private FinanceContext db = new FinanceContext();

        /* POST -- /Vendors/Add
         * This method adds a new Vendor to the db.
         */
        [HttpPost]
        public JsonResult Add(Vendor vendor)
        {
            db.Vendors.Add(vendor);
            db.SaveChanges();
            return Json(vendor);
        }

        /* POST -- /Vendors/AddAbbrev
         * This method adds a new Vendor to the db.
         */
        [HttpPost]
        public JsonResult AddAbbrev(int VendorID, string Abbrev)
        {
            VendorAbbrev abbrev = new VendorAbbrev();

            abbrev.VendorID = VendorID;
            abbrev.Abbrev = Abbrev;
            db.VendorAbbrevs.Add(abbrev);
            db.SaveChanges();
            return Json(abbrev);
        }
    }
}