using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
namespace Inventory.Controllers.Api
{
    public class TransferController : ApiController
    {
        readonly DataContext db;
        public TransferController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult CreateTransfer(TransferPageViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid || vm.TransferDetailList.Count == 0)
                    return BadRequest();

                var docno = SaveTransferMain(vm.TransferMain);
                if (docno != "" && docno != Constants.DocumentStatus.UnauthorizedDocument)
                {
                    var td = new TransferDetailViewModel();
                    SaveTransferDetail(vm.TransferDetailList, docno);
                    db.SaveChanges();
                    return Ok(docno);
                }
                else
                    return BadRequest("Document is not active.");
            }
            catch (System.Exception ex)
            {
                return BadRequest();
            }
        }
        [NonAction]
        public bool SaveTransferDetail(List<TransferDetailViewModel> vm, string docno)
        {
            try
            {
                var tdinDb = db.TRANSFER_DETAIL.Where(u => u.DOC_NO == docno).ToList();
                var tdinDbCount = tdinDb.Count;
                if (tdinDbCount > 0)
                {
                    db.TRANSFER_DETAIL.RemoveRange(tdinDb);
                }
                foreach (var item in vm)
                {

                    if (!string.IsNullOrEmpty(item.DocNo))
                    {
                        var detail = db.GRN_DETAIL.Where(x => x.GRN_NO == item.DocNo && x.BARCODE == item.Barcode).SingleOrDefault();

                        var qty = (detail.QTY ?? 0) - (detail.TRANSFER_QTY ?? 0);

                        if (item.Qty <= qty)
                        {
                            detail.TRANSFER_QTY = (detail.TRANSFER_QTY ?? 0) + item.Qty;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                    }


                    var transferDetail = new TRANSFER_DETAIL();
                    transferDetail.DOC_NO = docno;
                    transferDetail.BARCODE = item.Barcode;
                    transferDetail.COST = item.Cost;
                    transferDetail.QTY = item.Qty;
                    transferDetail.RETAIL = item.Retail;
                    transferDetail.UNIT_SIZE = item.Usize;
                    transferDetail.UAN_NO = item.Uanno;
                    transferDetail.TRANSFER_STATUS = "0";
                    db.TRANSFER_DETAIL.Add(transferDetail);
                }
                return true;
            }
            catch (System.Exception ex)
            {

                return false;
            }
        }
        [NonAction]
        public string SaveTransferMain(TransferMainViewModel vm)
        {
            try
            {
                var transfermain = new TRANSFER_MAIN();
                bool AddNew = false;
                if (string.IsNullOrEmpty(vm.doc_no))
                {
                    string prefix = vm.doc_type;//+ vm.warehouse;
                    transfermain.DOC_NO = vm.GetTransferCode(prefix, 7);
                    AddNew = true;
                }
                else
                {
                    transfermain = db.TRANSFER_MAIN.SingleOrDefault(u => u.DOC_NO == vm.doc_no && u.STATUS == Constants.DocumentStatus.UnauthorizedDocument);
                    if (transfermain != null)
                    {
                        //transfermain.UPDATED_BY = vm.userid;
                        //transfermain.UPDATE_DATE = DateTime.Now;
                    }
                    else
                        return Constants.DocumentStatus.UnauthorizedDocument;

                }

                transfermain.UPDATED_BY = vm.userid;
                transfermain.UPDATE_DATE = DateTime.Now;
                transfermain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                transfermain.DOC = DateTime.Now;
                transfermain.DOC_TYPE = vm.doc_type;
                transfermain.LOCATION = vm.location;
                transfermain.TRANSFER_STATUS = "0";
                transfermain.WAREHOUSE = vm.warehouse;

                transfermain.BRANCH_ID_FROM = (vm.doc_type == Constants.Constants.TransferOut && string.IsNullOrEmpty(vm.BranchIdFrom)) ? "04" : vm.BranchIdFrom;
                transfermain.BRANCH_ID_TO = vm.BranchIdTo;

                if (AddNew)
                {
                    transfermain.INSERTED_BY = vm.userid;
                    transfermain.DOC_DATE = vm.doc_date;
                    db.TRANSFER_MAIN.Add(transfermain);
                }
                return transfermain.DOC_NO;
            }
            catch (System.Exception)
            {

                return "";
            }
        }
    }
}
