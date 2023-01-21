using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace Inventory.Controllers
{
    [Authorize]
    public class TransferController : Controller
    {
        private readonly DataContext db;
        public TransferController()
        {
            db = new DataContext();
        }
        [Permission("Transfer In")]
        public ActionResult TransferPage()
        {
            try
            {
                return View(GetTransferPageViewModel(Constants.Constants.TransferIn));
            }
            catch (System.Exception ex)
            {
                return Content("<p>Page is not working.</p>");
            }
        }
        [Permission("Transfer Out")]
        public ActionResult TransferPageOut()
        {
            try
            {
                return View("TransferPage", GetTransferPageViewModel(Constants.Constants.TransferOut));
            }
            catch (System.Exception ex)
            {
                return Content("<p>Page not found.</p>");
            }
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string docno)
        {
            var transfermain = db.TRANSFER_MAIN.SingleOrDefault(c => c.DOC_NO == docno);
            if (transfermain == null)
                return HttpNotFound("Page not found. Bad Request");
            var trnasferDetail = db.TRANSFER_DETAIL.Where(c => c.DOC_NO == docno);
            var itemlist = db.PRODUCTS.Select(u => new { barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
            //var itemlist = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new { barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
            List<ItemViewModel> itemList = new List<ItemViewModel>();
            foreach (var item in itemlist)
            {
                ItemViewModel temp = new ItemViewModel
                {
                    Barcode = item.barcode,
                    Description = item.Description
                };
                itemList.Add(temp);
            }
            List<TransferDetailViewModel> tdList = new List<TransferDetailViewModel>();
            foreach (var item in trnasferDetail)
            {

                var itemName = itemList.SingleOrDefault(u => u.Barcode == item.BARCODE);
                decimal? InHandQty;
                if (transfermain.DOC_TYPE == Constants.Constants.TransferIn)
                    InHandQty = db.PROD_LOC_REPORT.Where(x => x.BARCODE == item.BARCODE && x.BRANCH_ID == transfermain.BRANCH_ID_FROM).Select(x => x.CURRENT_QTY).SingleOrDefault();
                else
                {
                    //InHandQty = item.QTY;

                    var pb = db.PROD_BALANCE.Where(x => x.BARCODE == item.BARCODE).SingleOrDefault();
                    if (pb != null)
                        InHandQty = item.QTY + (pb.GRN_QTY ?? 0) + (pb.TRANSFER_IN_QTY ?? 0) + (pb.GAIN_QTY ?? 0) - (pb.WAST_QTY ?? 0) - (pb.GRF_QTY ?? 0) - (pb.TRANSFER_OUT_QTY ?? 0);
                    else
                        InHandQty = item.QTY ;
                }



                if (itemName == null)
                {
                    itemName = new ItemViewModel();
                    itemName.Description = "";
                    itemName.Barcode = "0";
                }
                TransferDetailViewModel temp = new TransferDetailViewModel
                {
                    InHandQty = InHandQty,
                    Barcode = item.BARCODE,
                    Cost = item.COST,
                    Qty = item.QTY,
                    Usize = item.UNIT_SIZE ?? 0,
                    Retail = item.RETAIL ?? 0,
                    Uanno = item.UAN_NO,
                    Description = itemName.Description
                };
                tdList.Add(temp);
            }
            var transferMain = new TransferMainViewModel();
            transferMain.doc_date = transfermain.DOC;
            transferMain.DocNoDisplay = transfermain.DOC_NO;
            transferMain.doc_no = transfermain.DOC_NO;
            transferMain.BranchIdFrom = transfermain.BRANCH_ID_FROM;
            transferMain.BranchIdTo = transfermain.BRANCH_ID_TO;
            transferMain.userid = CommonFunctions.GetUserName();
            //transferMain.userid = transfermain.inserted_by;
            transferMain.doc_type = transfermain.DOC_TYPE;
            transferMain.warehouse = transfermain.WAREHOUSE;
            transferMain.Status = transfermain.STATUS;
            //transferMain.LocationTypeList = transferMain.getLocationTypeList();
            //transferMain.LocationType = Regex.Replace(transfermain.location, @"\s+", String.Empty);;
            transferMain.LocationTypeList = db.LOCATIONs.Select(x => new LocationType { Location_Id = x.LOC_ID, Location_Name = x.NAME }).ToList();
            transferMain.LocationType = transfermain.LOCATION;
            transferMain.warehouse = transfermain.WAREHOUSE;

            var viewmodel = new TransferPageViewModel
            {
                //  LocationList = locationlist,
                Branches = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList(),
                WarehouseList = db.WAREHOUSEs.ToList(),
                ItemList = itemList,
                TransferDetailList = tdList,
                TransferMain = transferMain
            };

            return View("TransferPage", viewmodel);

        }
        public ActionResult List()
        {
            TransferListViewModel obj = new TransferListViewModel();
            return View(obj.GetStockTransferList(Constants.Constants.TransferIn));
        }
        public ActionResult TransferOutList()
        {
            TransferListViewModel obj = new TransferListViewModel();
            return View("List", obj.GetStockTransferList(Constants.Constants.TransferOut));
        }
        public TransferPageViewModel GetTransferPageViewModel(string doctype)
        {
            TransferMainViewModel obj = new TransferMainViewModel();
            //var vm = new TransferPageViewModel();
            //string prefix = vm.doc_type + "01" + "01";
            //transfermain.doc_no = vm.GetTransferCode(prefix, 4);
            obj.doc_date = DateTime.Now;
            obj.doc_type = doctype;
            obj.DocNoDisplay = obj.GetTransferCode(doctype, 7);
            obj.LocationType = CommonFunctions.GetLocation();
            //obj.doc_no_display =
            obj.userid = CommonFunctions.GetUserName();
            //obj.LocationTypeList = obj.getLocationTypeList();
            obj.LocationTypeList = db.LOCATIONs.Select(x=> new LocationType { Location_Id = x.LOC_ID, Location_Name = x.NAME}).ToList();
            //if (doctype== Constants.Constants.TransferIn)
            //{
            //    obj.LocationType = "1";
            //}
            //else
            //{
            //    obj.LocationType = "2";
            //}
            var viewmodel = new TransferPageViewModel
            {
                TransferMain = obj,
                Branches = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList(),
                //LocationList = db.LOCATIONs.ToList(),
                WarehouseList = db.WAREHOUSEs.ToList(),
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList()
                //ItemList = db.PRODUCTS.Where(x=>x.FLASH_PACK=="P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList()

            };
            return viewmodel;
        }

        public ActionResult MaxTransferQty(string type,string barcode, string uan,string branchId)
        {
            var qty = "";

            if (string.IsNullOrEmpty(barcode))
                barcode = db.PRODUCT_UAN_LIST.Where(x => x.UAN_NO == uan).Select(x => x.BARCODE).SingleOrDefault();

            if (type == Constants.Constants.TransferIn)
            {
                qty = db.PROD_LOC_REPORT.Where(x => x.BARCODE == barcode && x.BRANCH_ID == branchId).Select(x => x.CURRENT_QTY).SingleOrDefault().ToString();
            }
            else if (type == Constants.Constants.TransferOut)
                qty = db.PROD_BALANCE.Where(x => x.BARCODE == barcode).Select(x => (x.GRN_QTY ?? 0) + (x.TRANSFER_IN_QTY ?? 0) + (x.GAIN_QTY ?? 0) - (x.WAST_QTY ?? 0) - (x.GRF_QTY ?? 0) - (x.TRANSFER_OUT_QTY ?? 0)).SingleOrDefault().ToString();

           
            return Json(qty,JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetGrn(string id)
        {
            var grn = db.GRN_MAIN.Include(x => x.GRN_DETAIL).Where(x => x.GRN_NO == id 
            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
            ).SingleOrDefault();





            if (grn != null)
            {
               List<DocumentDetailViewModel> vm = new List<DocumentDetailViewModel>();


                foreach (var detail in grn.GRN_DETAIL)
                {
                    DocumentDetailViewModel obj = new DocumentDetailViewModel();
                    var product = db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).SingleOrDefault();

                    var pb = db.PROD_BALANCE.Where(x => x.BARCODE == detail.BARCODE).SingleOrDefault();

                    var InHand = (detail.QTY ?? 0) - (detail.TRANSFER_QTY ?? 0);

                    obj.DocNo = detail.GRN_NO;
                    obj.Barcode = detail.BARCODE;
                    obj.Cost = detail.COST;
                    obj.Discount = detail.DIS_AMT ?? 0;
                    obj.Qty = detail.QTY.HasValue ? detail.QTY.Value : 0;
                    obj.Amount = ((detail.QTY * detail.COST) - detail.DIS_AMT) ?? 0;
                    obj.Description = product.DESCRIPTION;
                    obj.Retail = product.UNIT_RETAIL.HasValue ? product.UNIT_RETAIL.Value : 0;
                    obj.QtyinHand = (pb.GRN_QTY ?? 0) + (pb.TRANSFER_IN_QTY ?? 0) - ( pb.GRF_QTY ?? 0 ) - (pb.TRANSFER_OUT_QTY ?? 0);
                    //obj.QtyinHand = pb.CURRENT_QTY <= InHand ? pb.CURRENT_QTY : InHand;
                    //obj.QtyinHand = db.PROD_BALANCE.Where(x => x.BARCODE == detail.BARCODE).Select(x => x.CURRENT_QTY).SingleOrDefault();

                    vm.Add(obj);
                }

                return Json(vm, JsonRequestBehavior.AllowGet);
            }




            return Json("" , JsonRequestBehavior.AllowGet);
        }



    }
}