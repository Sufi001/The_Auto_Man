using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly DataContext db;
        public PurchaseController()
        {
            db = new DataContext();
        }
        [Permission("Purchase")]
        public ActionResult PurchasePage()
        {
            try
            {
                return View(GetPurchasePageViewModel(Constants.Constants.PurchaseDocument));
            }
            catch (System.Exception)
            {
                return Content("<p>Page is not working.</p>");
            }
        }
        public ActionResult ProjectPage()
        {
            try
            {
                return View(GetPurchasePageViewModel(Constants.Constants.PurchaseDocument));
            }
            catch (System.Exception)
            {
                return Content("<p>Page is not working.</p>");
            }
        }
        public DocumentViewModel GetPurchasePageViewModel(string doctype)
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();
            obj.DocDate = DateTime.Now;
            obj.DocType = doctype;
            obj.Location = CommonFunctions.GetLocation();
            var Locationlist = db.LOCATIONs.ToList();
            var prefix = obj.DocType + Int32.Parse(obj.Location);
            obj.DocNoDisplay = obj.GetPurchaseCode(prefix, 4);
            var viewmodel = new DocumentViewModel
            {
                DocumentMain = obj,
                LocationList = Locationlist,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION, Cost = u.UNIT_COST, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS }).ToList(),
                ColourList = db.COLOURS.ToList()
            };
            return viewmodel;
        }
        public ActionResult List()
        {
            List<DocumentListViewModel> obj = new List<DocumentListViewModel>();
            obj = GetPurchaseList();
            return View(obj);
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string docno)
        {
            var grnMain = db.GRN_MAIN.SingleOrDefault(c => c.GRN_NO == docno);
            if (grnMain == null)
                return HttpNotFound("Page not found. Bad Request");
            var itemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS }).ToList();
            //var itemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
            var grnDetail = db.GRN_DETAIL.Include("P").Where(c => c.GRN_NO == docno).
                Select(u => new DocumentDetailViewModel
                {
                    Barcode = u.BARCODE,
                    Cost = u.COST,
                    Qty = u.QTY ?? 0,
                    FreeQty = u.FREE_QTY ?? 0,
                    Discount = u.DIS_AMT ?? 0,
                    Warehouse = u.WAREHOUSE,
                    Colour = u.COLOUR ?? 0,
                    Amount = ((u.COST * (u.QTY ?? 0) - u.DIS_AMT ?? 0))
                }).ToList();
            var colourList = db.COLOURS.ToList();
            foreach (var item in grnDetail)
            {
                var itemName = itemList.SingleOrDefault(u => u.Barcode == item.Barcode);
                item.Description = itemName.Description;
                item.Uanno = itemName.Uanno;
                item.Retail = itemName.Retail.Value;
                item.CtnPcs = itemName.Ctnpcs;
                var colour = colourList.SingleOrDefault(u => u.id == item.Colour);
                if (colour != null)
                {
                    item.Colour = colour.id;
                    item.ColourName = colour.COLOUR_NAME;
                }
            }
            var purchaseMain = new DocumentMainViewModel();
            purchaseMain.DocDate = grnMain.GRN_DATE;
            purchaseMain.DocNo = grnMain.GRN_NO;
            purchaseMain.DocNoDisplay = grnMain.GRN_NO;
            purchaseMain.DocType = grnMain.DOC_TYPE;
            purchaseMain.Location = grnMain.LOC_ID;
            purchaseMain.Print = "True";

            //purchaseMain.Warehouse = grnMain.warehouse;
            purchaseMain.SuplCode = grnMain.SUPL_CODE;
            purchaseMain.Status = grnMain.STATUS;
            var viewmodel = new DocumentViewModel
            {
                LocationList = db.LOCATIONs.ToList(),
                ItemList = itemList,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier)
                    .Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                DocumentMain = purchaseMain,
                DocumentDetailList = grnDetail,
                ColourList = db.COLOURS.ToList()
            };

            return View("PurchasePage", viewmodel);

        }
        public JsonResult GenerateBarcode()
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();

            var Locationlist = db.LOCATIONs.ToList();
            var locId = Locationlist.Select(x => x.LOC_ID).FirstOrDefault();
            var prefix = Constants.Constants.PurchaseDocument + Int32.Parse(locId);
            var list = obj.DocNoDisplay = obj.GetPurchaseCode(prefix, 4);

            var value = JsonConvert.SerializeObject(list, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult GetSupplierProduct(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var barCodeList = db.SUPPLIER_PRODUCTS.Where(x => x.SUPL_CODE == id && x.STATUS == "1").Select(x => x.BARCODE).ToList();
                if (barCodeList.Count > 0)
                {
                    //from c in db.PRODUCTS
                    //join p in db.PROD_BALANCE on c.BARCODE equals p.BARCODE into ps
                    //from p in ps.DefaultIfEmpty().Select(product => new ItemViewModel {
                    //}).ToList();
                    var productList = db.PRODUCTS.Where(u => barCodeList.Contains(u.BARCODE)).Select(product => new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        Uanno = product.UAN_NO,
                        Ctnpcs = product.CTN_PCS,
                        Cost = product.UNIT_COST ?? 0,
                        Retail = product.UNIT_RETAIL ?? 0,
                        UrduName = product.URDU,
                        CurrentQty=product.PROD_BALANCE.CURRENT_QTY??0,
                        //UnitSize = product.UNIT_SIZE ?? 0
                    }).ToList();

                    var value = JsonConvert.SerializeObject(productList, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
                    return jsonResult;
                }
                else
                {
                    var productList = db.PRODUCTS.Select(product => new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        Uanno = product.UAN_NO,
                        Ctnpcs = product.CTN_PCS,
                        Cost = product.UNIT_COST ?? 0,
                        Retail = product.UNIT_RETAIL ?? 0,
                        UrduName = product.URDU,
                        //UnitSize = product.UNIT_SIZE ?? 0
                    }).ToList();

                    var value = JsonConvert.SerializeObject(productList, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
                    return jsonResult;
                }
                //else
                //    return Json("NoProduct", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("SelectSupplier", JsonRequestBehavior.AllowGet);

        }
        public List<DocumentListViewModel> GetPurchaseList()
        {
            DataContext context = new DataContext();
            var list = (from purchaseMain in context.GRN_MAIN
                        join supplier in context.SUPPLIERs on purchaseMain.SUPL_CODE equals supplier.SUPL_CODE
                        join loc in context.LOCATIONs on purchaseMain.LOC_ID equals loc.LOC_ID
                        //join war in context.Warehouses on purchaseMain.warehouse equals war.id
                        let Amount = (decimal?)((from a in context.GRN_DETAIL
                                                 where purchaseMain.GRN_NO == a.GRN_NO
                                                 select (a.COST * a.QTY) - a.DIS_AMT).Sum()) ?? 0
                        select new
                        {
                            Doc = purchaseMain.DOC,
                            DocNo = purchaseMain.GRN_NO,
                            Location = loc.NAME,
                            SupplierName = supplier.SUPL_NAME,
                            amount = Amount,
                            Status = purchaseMain.STATUS
                        }).ToList();

            return list.Select(item => new DocumentListViewModel
            {
                DocNo = item.DocNo,
                Doc = item.Doc,
                Location = item.Location,
                Supplier = item.SupplierName,
                Amount = item.amount,
                // Amount= Convert.ToInt32(string.Format("{0:0.00}",item.amount)),
                Status = (item.Status == "0") ? "Unauthorized" : "Authorized"
            }).OrderByDescending(x => x.DocNo).ToList();
        }
    }
}