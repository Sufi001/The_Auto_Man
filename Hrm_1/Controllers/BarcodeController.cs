using Inventory.ViewModels.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class BarcodeController : Controller
    {
        // GET: Barcode
        readonly DataContext db;
        public BarcodeController()
        {
            db = new DataContext();
        }
        public ActionResult Index()
        {
            var products = db.PRODUCTS.Select(x => new ItemViewModel
            {
                Barcode = x.BARCODE,
                ReferenceCode = x.REFERENCE_CODE,
                Cost = x.UNIT_COST,
                Ctnpcs = x.CTN_PCS,
                Description = x.DESCRIPTION,
                Retail = x.UNIT_RETAIL,
                Uanno = x.UAN_NO,
                UrduName = x.URDU,
            }).ToList().OrderByDescending(x => x.Barcode);
            return View(products);
        }
    }
}