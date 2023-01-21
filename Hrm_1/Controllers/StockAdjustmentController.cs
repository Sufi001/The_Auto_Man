using Inventory.Helper;
using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using Inventory.Filters;
using Newtonsoft.Json;

namespace Inventory.Controllers
{
    public class StockAdjustmentController : Controller
    {
        private readonly DataContext db;
        public StockAdjustmentController()
        {
            db = new DataContext();
        }
        [Permission("Waste & Gain")]
        public ActionResult Index()
        {
            ViewBag.Branches = db.BRANCHes.Select(x=> new {Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();
            return View(GetViewModel());
        }
        public DocumentViewModel GetViewModel()
        {
            DocumentViewModel VM = new DocumentViewModel();
            VM.LocationList = db.LOCATIONs.ToList();
            VM.ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION, UrduName = u.URDU, Cost = u.UNIT_COST, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS }).ToList();

            VM.DocumentMain = new DocumentMainViewModel();
            VM.DocumentMain.DocDate = DateTime.Now;
            VM.DocumentMain.Location = CommonFunctions.GetLocation();

            VM.DocumentDetailList = new List<DocumentDetailViewModel>();

            return VM;
        }
        public ActionResult List()
        {
            var data = (from main in db.WAST_MAIN
                        join detail in db.WAST_DETAIL on main.DOC_NO equals detail.DOC_NO into d
                        select new DocumentListViewModel
                        {
                            DocNo = main.DOC_NO,
                            Doc = main.DOC_DATE,
                            Amount = d.Sum(x=> x.COST * x.QTY),
                            Status = main.STATUS == Constants.DocumentStatus.AuthorizedDocument ? "Authorized" : "Unauthorized"
                        }
                ).ToList();
            return View(data);
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string docno)
        {
            ViewBag.Branches = db.BRANCHes.Select(x => new { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            if (string.IsNullOrEmpty(docno))
            {
                return Content("Invalid");
            }
            var Wast = db.WAST_MAIN.Include(x=> x.WAST_DETAIL).Include(x=>x.WAST_DETAIL.Select(y=>y.PRODUCT)).Where(x => x.DOC_NO == docno).SingleOrDefault();
            if (Wast != null)
            {

                DocumentViewModel DVM = GetViewModel();
                DVM.DocumentMain.DocNo = Wast.DOC_NO;
                DVM.DocumentMain.Status = Wast.STATUS;
                DVM.DocumentMain.DocDate = Wast.DOC_DATE;
                DVM.DocumentMain.DocType = Wast.DOC_TYPE;
                DVM.DocumentMain.BranchId = Wast.BRANCH_ID;
                DVM.DocumentMain.Location = CommonFunctions.GetLocation();

                foreach (var detail in Wast.WAST_DETAIL)
                {
                    DocumentDetailViewModel DVMdetail = new DocumentDetailViewModel();
                    DVMdetail.Barcode = detail.BARCODE;
                    DVMdetail.Uanno = detail.PRODUCT.UAN_NO;
                    DVMdetail.Description = detail.PRODUCT.DESCRIPTION;
                    DVMdetail.Retail = detail.PRODUCT.UNIT_RETAIL ?? 0;
                    DVMdetail.Cost = detail.COST;
                    //DVMdetail.GST_AMOUNT = detail;
                    DVMdetail.Qty = detail.QTY;

                    DVM.DocumentDetailList.Add(DVMdetail);
                }

                return View("Index", DVM);
            }

            return View("Index");
        }
        public ActionResult Save(DocumentViewModel DVM)
        {
            ViewBag.Branches = db.BRANCHes.Select(x => new { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            if (DVM == null)
            {
                return Content("Invalid");
            }
            try
            {
                WAST_MAIN WastMain = new WAST_MAIN();

                if (string.IsNullOrEmpty(DVM.DocumentMain.DocNo))
                {
                    WastMain.DOC_NO = GetWastCode(DVM.DocumentMain.DocType);
                    WastMain.DOC_TYPE = DVM.DocumentMain.DocType;
                    WastMain.INSERTED_BY = CommonFunctions.GetUserName();
                    WastMain.DOC = CommonFunctions.GetDateTimeNow();
                }
                else
                {
                    WastMain = db.WAST_MAIN.Where(x => x.DOC_NO == DVM.DocumentMain.DocNo && x.STATUS != Constants.DocumentStatus.AuthorizedDocument).SingleOrDefault();
                    

                    var detailList = WastMain.WAST_DETAIL.Where(x => x.DOC_NO == DVM.DocumentMain.DocNo).ToList();
                    if (detailList != null && detailList.Count > 0)
                        db.WAST_DETAIL.RemoveRange(detailList);
                }

                WastMain.UPDATED_BY = CommonFunctions.GetUserName();
                WastMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                WastMain.DOC_DATE = DVM.DocumentMain.DocDate;
                WastMain.STATUS = DVM.DocumentMain.Status;
                WastMain.LOCATION = CommonFunctions.GetLocation();
                WastMain.BRANCH_ID = DVM.DocumentMain.BranchId;
                WastMain.TRANSFER_STATUS = "0";


                foreach (var detail in DVM.DocumentDetailList)
                {
                    WAST_DETAIL WastDetail = new WAST_DETAIL();
                    WastDetail.BARCODE = detail.Barcode;
                    WastDetail.COST = detail.Cost;
                    WastDetail.DOC_NO = WastMain.DOC_NO;
                    //WastDetail.GST_AMOUNT = detail;
                    WastDetail.QTY = detail.Qty;
                    WastDetail.TRANSFER_STATUS = "0";
                    WastMain.WAST_DETAIL.Add(WastDetail);
                }

                if (string.IsNullOrEmpty(DVM.DocumentMain.DocNo))
                    db.WAST_MAIN.Add(WastMain);

                db.SaveChanges();
                return Content(WastMain.DOC_NO);
            }
            catch (Exception ex)
            {
                return Content("Invalid");
            }
        }
        [Permission("Authorize")]
        public ActionResult Authorize(DocumentViewModel DVM)
        {
            if (DVM == null)
            {
                return Content("Invalid");
            }
            try
            {
                foreach (var item in DVM.DocumentDetailList)
                {
                    if (DVM.DocumentMain.BranchId == "04")
                    {

                        var prodBalance = new PROD_BALANCE();
                        var prod = db.PROD_BALANCE.SingleOrDefault(u => u.BARCODE == item.Barcode);
                        if (prod != null)
                        {
                            prodBalance = prod;
                        }

                        if (DVM.DocumentMain.DocType == Inventory.Constants.Constants.WastDocument)
                        {
                            prodBalance.WAST_QTY = (prodBalance.WAST_QTY ?? 0) + item.Qty;
                            prodBalance.WAST_AMOUNT = (prodBalance.WAST_AMOUNT ?? 0) + (item.Qty * item.Cost);
                            prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.Qty);
                        }
                        else
                        {
                            prodBalance.GAIN_QTY = (prodBalance.GAIN_QTY ?? 0) + item.Qty;
                            prodBalance.GAIN_AMOUNT = (prodBalance.GAIN_AMOUNT ?? 0) + (item.Qty * item.Cost);
                            prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.Qty);
                        }

                        prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                        if (prod == null)
                        {
                            prodBalance.BARCODE = item.Barcode;
                            db.PROD_BALANCE.Add(prodBalance);
                        }
                    }
                    else
                    {
                        var ProLocReport = new PROD_LOC_REPORT();
                        var locBalance = db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == DVM.DocumentMain.BranchId && x.BARCODE == item.Barcode).SingleOrDefault();

                        if (locBalance != null)
                            ProLocReport = locBalance;


                        if (DVM.DocumentMain.DocType == Inventory.Constants.Constants.WastDocument)
                        {
                            ProLocReport.WAST_QTY = (ProLocReport.WAST_QTY ?? 0) + item.Qty;
                            ProLocReport.WAST_AMOUNT = (ProLocReport.WAST_AMOUNT ?? 0) + (item.Qty * item.Cost);
                            ProLocReport.CURRENT_QTY = (ProLocReport.CURRENT_QTY ?? 0) - (item.Qty);
                        }
                        else
                        {
                            ProLocReport.GAIN_QTY = (ProLocReport.GAIN_QTY ?? 0) + item.Qty;
                            ProLocReport.GAIN_AMOUNT = (ProLocReport.GAIN_AMOUNT ?? 0) + (item.Qty * item.Cost);
                            ProLocReport.CURRENT_QTY = (ProLocReport.CURRENT_QTY ?? 0) + (item.Qty);
                        }

                        ProLocReport.UPDATED_DATE = CommonFunctions.GetDateTimeNow();

                        if (locBalance == null)
                        {
                            ProLocReport.BRANCH_ID = DVM.DocumentMain.BranchId;
                            ProLocReport.BARCODE = item.Barcode;
                            db.PROD_LOC_REPORT.Add(ProLocReport);
                        }


                    }

                }
                DVM.DocumentMain.Status = Constants.DocumentStatus.AuthorizedDocument;
                var content = Save(DVM);

                return Content(content.ToString());
            }
            catch (Exception ex)
            {
                return Content("Invalid");
            }
        }
        public string GetWastCode(string prefix)
        {
            string code = db.WAST_MAIN.Where(u => u.DOC_NO.StartsWith(prefix)).Max(u => u.DOC_NO);
            if (string.IsNullOrEmpty(code))
                code = "0000001";
            else
                code = CommonFunctions.GenerateCode(code.Substring(1), 7);

            return prefix + code;
        }
        public ActionResult Adjustment()
        {
            try
            {
                return View(GetAdjustmentPageViewModel(Constants.Constants.AdjustmentDocument));
            }
            catch (System.Exception)
            {
                return Content("<p>Page is not working.</p>");
            }
        }
        public DocumentViewModel GetAdjustmentPageViewModel(string doctype)
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();
            obj.DocDate = DateTime.Now;
            obj.DocType = doctype;
            obj.Location = CommonFunctions.GetLocation();
            var Locationlist = db.LOCATIONs.ToList();
            var prefix = obj.DocType + Int32.Parse(obj.Location);
            obj.DocNoDisplay = obj.GetAdjustmentCode(prefix, 4);
            var viewmodel = new DocumentViewModel
            {
                DocumentMain = obj,
                LocationList = Locationlist,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION, Cost = u.UNIT_COST, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS,CurrentQty=u.PROD_BALANCE.CURRENT_QTY }).ToList(),
                ColourList = db.COLOURS.ToList()
            };
            return viewmodel;
        }
        public ActionResult StockList()
        {
            List<DocumentListViewModel> obj = new List<DocumentListViewModel>();
            obj = GetAdjustList();
            return View(obj);
        }
        public List<DocumentListViewModel> GetAdjustList()
        {
            DataContext context = new DataContext();
            var list = (from StockMain in context.STOCK_MAIN
                        join supplier in context.SUPPLIERs on StockMain.SUPL_CODE equals supplier.SUPL_CODE
                        join loc in context.LOCATIONs on StockMain.LOC_ID equals loc.LOC_ID
                        //join war in context.Warehouses on purchaseMain.warehouse equals war.id
                        let Amount = (decimal?)((from a in context.STOCK_DETAIL
                                                 where StockMain.STOCK_NO == a.STOCK_NO
                                                 select (a.COST * a.QTY) - a.DIS_AMT).Sum()) ?? 0
                        select new
                        {
                            Doc = StockMain.DOC,
                            DocNo = StockMain.STOCK_NO,
                            Location = loc.NAME,
                            SupplierName = supplier.SUPL_NAME,
                            amount = Amount,
                            Status = StockMain.STATUS
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
        public ActionResult EditStock(string docno)
        {
            var stockMain = db.STOCK_MAIN.SingleOrDefault(c => c.STOCK_NO == docno);
            if (stockMain == null)
                return HttpNotFound("Page not found. Bad Request");
            var itemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS, CurrentQty = u.PROD_BALANCE.CURRENT_QTY }).ToList();
            //var itemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
            var stockDetail = db.STOCK_DETAIL.Where(c => c.STOCK_NO == docno).
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
            foreach (var item in stockDetail)
            {
                var itemName = itemList.SingleOrDefault(u => u.Barcode == item.Barcode);
                item.Description = itemName.Description;
                item.Uanno = itemName.Uanno;
                item.Retail = itemName.Retail.Value;
                item.CtnPcs = itemName.Ctnpcs;
                item.QtyinHand = itemName.CurrentQty ?? 0;
                var colour = colourList.SingleOrDefault(u => u.id == item.Colour);
                if (colour != null)
                {
                    item.Colour = colour.id;
                    item.ColourName = colour.COLOUR_NAME;
                }
            }
            var Main = new DocumentMainViewModel();
            Main.DocDate = stockMain.STOCK_DATE;
            Main.DocNo = stockMain.STOCK_NO;
            Main.DocNoDisplay = stockMain.STOCK_NO;
            Main.DocType = stockMain.DOC_TYPE;
            Main.Location = stockMain.LOC_ID;
            Main.Print = "True";

            //purchaseMain.Warehouse = grnMain.warehouse;
            Main.SuplCode = stockMain.SUPL_CODE;
            Main.Status = stockMain.STATUS;
            var viewmodel = new DocumentViewModel
            {
                LocationList = db.LOCATIONs.ToList(),
                ItemList = itemList,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier)
                    .Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                DocumentMain = Main,
                DocumentDetailList = stockDetail,
                ColourList = db.COLOURS.ToList()
            };

            return View("Adjustment", viewmodel);

        }
        public JsonResult GenerateBarcode()
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();

            var Locationlist = db.LOCATIONs.ToList();
            var locId = Locationlist.Select(x => x.LOC_ID).FirstOrDefault();
            var prefix = Constants.Constants.AdjustmentDocument + Int32.Parse(locId);
            var list = obj.DocNoDisplay = obj.GetAdjustmentCode(prefix, 4);

            var value = JsonConvert.SerializeObject(list, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}