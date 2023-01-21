using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class PurchaseReturnController : ApiController
    {
        readonly DataContext db;
        public PurchaseReturnController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult CreatePurchaseReturn(DocumentViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid || vm.DocumentDetailList.Count == 0)
                {
                    return BadRequest();
                }
                var docno = SavePurchaseReturnMain(vm.DocumentMain);
                if (docno != "" && docno != Constants.DocumentStatus.UnauthorizedDocument)
                {
                    SavePurchaseReturnDetail(vm.DocumentDetailList, docno);
                    db.SaveChanges();
                    return Ok(docno);
                }

                return BadRequest("Document is not active.");

            }
            catch (System.Exception ex)
            {

                return BadRequest();
            }
        }
        [NonAction]
        public string SavePurchaseReturnMain(DocumentMainViewModel vm)
        {
            try
            {
                var purchaseReturnMain = new GRFS_MAIN();
                bool saveOrUpdateFlag = false;
                if (string.IsNullOrEmpty(vm.DocNo))
                {
                    string prefix = "R" + int.Parse(vm.Location);
                    purchaseReturnMain.GRF_NO = vm.GetPurchaseReturnCode(prefix, 4);
                    saveOrUpdateFlag = true;
                }
                else
                {
                    purchaseReturnMain = db.GRFS_MAIN.SingleOrDefault(u => u.GRF_NO == vm.DocNo);
                    if (purchaseReturnMain != null)
                    {
                        //purchaseReturnMain.UPDATED_BY = CommonFunctions.GetUserName();
                        //purchaseReturnMain.UPDATE_DATE = DateTime.Now;
                    }
                    else
                        return Constants.DocumentStatus.UnauthorizedDocument;

                }

                purchaseReturnMain.UPDATED_BY = CommonFunctions.GetUserName();
                purchaseReturnMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                purchaseReturnMain.GRF_DATE = vm.DocDate;
                purchaseReturnMain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                purchaseReturnMain.LOCATION = vm.Location;
                purchaseReturnMain.TRANSFER_STATUS = "0";
                purchaseReturnMain.SUPL_CODE = vm.SuplCode;
                if (saveOrUpdateFlag)
                {
                    purchaseReturnMain.DOC = DateTime.Now;
                    purchaseReturnMain.INSERTED_BY = CommonFunctions.GetUserName();
                    db.GRFS_MAIN.Add(purchaseReturnMain);
                }
                return purchaseReturnMain.GRF_NO;
            }
            catch (System.Exception ex)
            {

                return "";
            }
        }
        [NonAction]
        public bool SavePurchaseReturnDetail(List<DocumentDetailViewModel> vm, string docno)
        {
            try
            {
                var grnDetailDbList = db.GRFS_DETAIL.Where(u => u.GRF_NO == docno).ToList();
                var tdinDbCount = grnDetailDbList.Count;
                if (tdinDbCount > 0)
                {
                    db.GRFS_DETAIL.RemoveRange(grnDetailDbList);
                }
                foreach (var item in vm)
                {
                    var grfDetail = new GRFS_DETAIL
                    {
                        GRF_NO = docno,
                        BARCODE = item.Barcode,
                        TRANSFER_STATUS = "0",
                        COST = item.Cost,
                        DISCOUNT = item.Discount,
                        QTY = item.Qty,
                        FREE_QTY = item.FreeQty,
                    };
                    db.GRFS_DETAIL.Add(grfDetail);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

    }
}
