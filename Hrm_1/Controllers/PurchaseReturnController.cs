using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace Inventory.Controllers
{
    [Authorize]
    public class PurchaseReturnController : Controller
    {
        private readonly DataContext db;
        public PurchaseReturnController()
        {
            db = new DataContext();
        }
        [Permission("Purchase Return")]
        public ActionResult Index()
        {
            return View(GetPurchaseReturnPageViewModel(Constants.Constants.PurchaseReturnDocument));
        }
        public DocumentViewModel GetPurchaseReturnPageViewModel(string doctype)
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();
            obj.DocDate = DateTime.Now;
            obj.DocType = doctype;
            obj.Location = CommonFunctions.GetLocation();
            var Locationlist = db.LOCATIONs.ToList();
            var prefix = obj.DocType + Int32.Parse(obj.Location);
            obj.DocNoDisplay = obj.GetPurchaseReturnCode(prefix, 4);
            var viewmodel = new DocumentViewModel()
            {
                DocumentMain = obj,
                LocationList = Locationlist,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION, Ctnpcs = u.CTN_PCS }).ToList(),
            };
            return viewmodel;
        }
        public ActionResult List()
        {
            List<DocumentListViewModel> obj = new List<DocumentListViewModel>();
            obj = GetPurchaseReturnList();
            return View(obj);
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string docno)
        {
            var grfMain = db.GRFS_MAIN.SingleOrDefault(c => c.GRF_NO == docno);
            if (grfMain == null)
                return HttpNotFound("Page not found. Bad Request");
            //var itemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
            var itemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION, Retail = u.UNIT_RETAIL, Ctnpcs = u.CTN_PCS }).ToList();
            var grfDetail = db.GRFS_DETAIL.Where(c => c.GRF_NO == docno).
                Select(u => new DocumentDetailViewModel
                {
                    Barcode = u.BARCODE,
                    Cost = u.COST,
                    Qty = u.QTY ?? 0,
                    FreeQty = u.FREE_QTY ?? 0,
                    Discount = u.DISCOUNT ?? 0,

                    Amount = ((u.COST * (u.QTY ?? 0) - u.DISCOUNT ?? 0))
                }).ToList();

            foreach (var item in grfDetail)
            {
                var itemName = itemList.SingleOrDefault(u => u.Barcode == item.Barcode);
                item.Description = itemName.Description;
                item.Uanno = itemName.Uanno;
                item.Retail = itemName.Retail.Value;
                item.CtnPcs = itemName.Ctnpcs;
            }
            var purchaseReturnMain = new DocumentMainViewModel();
            purchaseReturnMain.DocDate = grfMain.GRF_DATE;
            purchaseReturnMain.DocNoDisplay = grfMain.GRF_NO;
            purchaseReturnMain.DocNo = grfMain.GRF_NO;
            purchaseReturnMain.Location = grfMain.LOCATION;
            purchaseReturnMain.SuplCode = grfMain.SUPL_CODE;
            purchaseReturnMain.Status = grfMain.STATUS;
            purchaseReturnMain.Print = "True";
            var viewmodel = new DocumentViewModel
            {
                LocationList = db.LOCATIONs.ToList(),
                ItemList = itemList,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier)
                    .Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
                DocumentMain = purchaseReturnMain,
                DocumentDetailList = grfDetail,
            };

            return View("Index", viewmodel);

        }
        public List<DocumentListViewModel> GetPurchaseReturnList()
        {
            DataContext context = new DataContext();
            var list = (from purchaseReturnMain in context.GRFS_MAIN
                        join supplier in context.SUPPLIERs on purchaseReturnMain.SUPL_CODE equals supplier.SUPL_CODE
                        join loc in context.LOCATIONs on purchaseReturnMain.LOCATION equals loc.LOC_ID
                        //join war in context.Warehouses on purchaseReturnMain.warehouse equals war.id
                        let Amount = (decimal?)((from a in context.GRFS_DETAIL
                                                 where purchaseReturnMain.GRF_NO == a.GRF_NO
                                                 select (a.COST * a.QTY) - a.DISCOUNT).Sum()) ?? 0
                        select new
                        {
                            Doc = purchaseReturnMain.DOC,
                            DocNo = purchaseReturnMain.GRF_NO,
                            Location = loc.NAME,
                            SupplierName = supplier.SUPL_NAME,
                            //Warehouse = war.Name,
                            amount = Amount,//(transferDetail.cost * transferDetail.qty),
                            Status = purchaseReturnMain.STATUS
                        }).ToList();
            return list.Select(item => new DocumentListViewModel
            {
                DocNo = item.DocNo,
                Doc = item.Doc,
                Location = item.Location,
                Supplier = item.SupplierName,
                //Warehouse = item.Warehouse,
                Amount = item.amount,
                Status = (item.Status == "0") ? "Unauthorized" : "Authorized"
            }).ToList();
        }

        public string GetCode()
        {

            string prefix = "R" + Int32.Parse(CommonFunctions.GetLocation());
            var code = new DocumentMainViewModel().GetPurchaseReturnCode(prefix, 4);

            return code;
        }

    }
}