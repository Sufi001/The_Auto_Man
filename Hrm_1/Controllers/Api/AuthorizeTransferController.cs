using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class AuthorizeTransferController : ApiController
    {
        readonly DataContext db;
        public AuthorizeTransferController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        [ApiPermission("Authorize")]
        public IHttpActionResult AuthorizeDocument(TransferPageViewModel vm)
        {
            try
            {
                if (string.IsNullOrEmpty(vm.TransferMain.doc_no))
                {
                    return BadRequest();
                }

                TransferController TransferApi = new TransferController();
                TransferApi.CreateTransfer(vm);

                string txt_totalAmount = vm.TransferDetailList.Select(x => x.Totalamount).FirstOrDefault();
                string totalQty = vm.TransferDetailList.Select(x => x.TotalQty).FirstOrDefault();
                string Documentdate = Convert.ToString(vm.TransferMain.doc_date.Date);
                var Main = db.TRANSFER_MAIN.Single(u => u.DOC_NO == vm.TransferMain.doc_no);
                var Details = db.TRANSFER_DETAIL.Where(u => u.DOC_NO == vm.TransferMain.doc_no).ToList();

                if (!UpdateDocument(vm.TransferMain.doc_no))
                    return BadRequest();

                var result = ProductBalance(Details, vm.TransferMain.BranchIdFrom, vm.TransferMain.BranchIdTo, vm.TransferMain.doc_type);

                if (!result)
                {
                    return BadRequest("Invalid Operation");
                }
                //if (Main.DOC_TYPE == Constants.Constants.TransferOut)
                //{
                //    var success = ProductBalance(Details, vm.TransferMain.doc_type);
                //    if (!success)
                //        return BadRequest();

                //    ProductLocationBalance(Details, "", vm.TransferMain.BranchIdTo);
                //}

                //if (Main.DOC_TYPE == Constants.Constants.TransferIn)
                //{
                //    var success = ProductLocationBalance(Details, vm.TransferMain.BranchIdFrom, vm.TransferMain.BranchIdTo);
                //    if (!success)
                //        return BadRequest();
                //}


                db.SaveChanges();
                return Ok();

                return BadRequest();
            }
            catch (System.Exception ex)
            {
                string s = ex.ToString();
                return BadRequest(ex.ToString());
            }
        }
        public bool UpdateDocument(string docNo)
        {
            try
            {
                var purchaseMain = db.TRANSFER_MAIN.SingleOrDefault(u => u.DOC_NO == docNo);
                if (purchaseMain != null && purchaseMain.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                {
                    purchaseMain.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                    purchaseMain.UPDATED_BY = CommonFunctions.GetUserName();
                    purchaseMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    // db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //public bool ProductBalance(List<TRANSFER_DETAIL> vm, string docType)
        //{
        //    try
        //    {
        //        var prodBalanceList = db.PROD_BALANCE.ToList();
        //        foreach (var item in vm)
        //        {
        //            var prodBalance = new PROD_BALANCE();
        //            var prod = prodBalanceList.FirstOrDefault(u => u.BARCODE == item.BARCODE);
        //            if (prod != null)
        //            {
        //                prodBalance = prod;
        //            }
        //            if (docType == Constants.Constants.TransferIn)
        //            {
        //                prodBalance.TRANSFER_IN_QTY = (prodBalance.TRANSFER_IN_QTY ?? 0) + item.QTY;
        //                prodBalance.TRANSFER_IN_AMOUNT = (prodBalance.TRANSFER_IN_AMOUNT ?? 0) + (item.QTY * item.COST);
        //                prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.QTY);
        //            }
        //            else
        //            {
        //                prodBalance.TRANSFER_OUT_QTY = (prodBalance.TRANSFER_OUT_QTY ?? 0) + item.QTY;
        //                prodBalance.TRANSFER_OUT_AMOUNT = (prodBalance.TRANSFER_OUT_AMOUNT ?? 0) + (item.QTY * item.COST);
        //                prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY);
        //            }

        //            if (prod == null)
        //            {
        //                prodBalance.BARCODE = item.BARCODE;
        //                db.PROD_BALANCE.Add(prodBalance);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        public bool ProductBalance(List<TRANSFER_DETAIL> vm, string from, string to, string docType)
        {
            try
            {

                if (from == to)
                    return false;

                var prodLocBalance = db.PROD_LOC_REPORT.ToList();
                foreach (var item in vm)
                {
                    if (docType == Constants.Constants.TransferIn)
                    {

                        var BalanceFrom = new PROD_LOC_REPORT();
                        var transferFrom = prodLocBalance.SingleOrDefault(u => u.BARCODE == item.BARCODE && u.BRANCH_ID == from);

                        if (transferFrom != null)
                            BalanceFrom = transferFrom;

                        if (item.QTY > BalanceFrom.CURRENT_QTY)
                            return false;

                        BalanceFrom.TRANSFER_OUT_QTY = (BalanceFrom.TRANSFER_OUT_QTY ?? 0) + item.QTY;
                        BalanceFrom.TRANSFER_OUT_AMOUNT = (BalanceFrom.TRANSFER_OUT_AMOUNT ?? 0) + (item.QTY * item.RETAIL);
                        BalanceFrom.CURRENT_QTY = (BalanceFrom.CURRENT_QTY ?? 0) - (item.QTY);
                        BalanceFrom.UPDATED_DATE = CommonFunctions.GetDateTimeNow();

                        if (transferFrom == null)
                        {
                            BalanceFrom.BARCODE = item.BARCODE;
                            BalanceFrom.BRANCH_ID = from;
                            db.PROD_LOC_REPORT.Add(BalanceFrom);
                        }
                    }
                    else
                    {
                        var prodBalance = new PROD_BALANCE();
                        var prod = db.PROD_BALANCE.SingleOrDefault(u => u.BARCODE == item.BARCODE);
                        if (prod != null)
                            prodBalance = prod;
                       
                            prodBalance.TRANSFER_OUT_QTY = (prodBalance.TRANSFER_OUT_QTY ?? 0) + item.QTY;
                            prodBalance.TRANSFER_OUT_AMOUNT = (prodBalance.TRANSFER_OUT_AMOUNT ?? 0) + (item.QTY * item.COST);
                            prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY);
                            prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                        if (prod == null)
                        {
                            prodBalance.BARCODE = item.BARCODE;
                            prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                            db.PROD_BALANCE.Add(prodBalance);
                        }

                    }


                    if (to == "04")
                    {
                        var BalanceTo = new PROD_BALANCE();
                        var transferTo = db.PROD_BALANCE.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                        if (transferTo != null)
                            BalanceTo = transferTo;

                        BalanceTo.TRANSFER_IN_QTY = (BalanceTo.TRANSFER_IN_QTY ?? 0) + item.QTY;
                        BalanceTo.TRANSFER_IN_AMOUNT = (BalanceTo.TRANSFER_IN_AMOUNT ?? 0) + (item.QTY * item.RETAIL);
                        BalanceTo.CURRENT_QTY = (BalanceTo.CURRENT_QTY ?? 0) + (item.QTY);
                        BalanceTo.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                        if (transferTo == null)
                        {
                            BalanceTo.BARCODE = item.BARCODE;
                            db.PROD_BALANCE.Add(BalanceTo);
                        }
                    }
                    else
                    {

                        var BalanceTo = new PROD_LOC_REPORT();
                        var transferTo = prodLocBalance.SingleOrDefault(u => u.BARCODE == item.BARCODE && u.BRANCH_ID == to);

                        if (transferTo != null)
                            BalanceTo = transferTo;

                        BalanceTo.TRANSFER_IN_QTY = (BalanceTo.TRANSFER_IN_QTY ?? 0) + item.QTY;
                        BalanceTo.TRANSFER_IN_AMOUNT = (BalanceTo.TRANSFER_IN_AMOUNT ?? 0) + (item.QTY * item.RETAIL);
                        BalanceTo.CURRENT_QTY = (BalanceTo.CURRENT_QTY ?? 0) + (item.QTY);
                        BalanceTo.UPDATED_DATE = CommonFunctions.GetDateTimeNow();

                        if (transferTo == null)
                        {
                            BalanceTo.BARCODE = item.BARCODE;
                            BalanceTo.BRANCH_ID = to;
                            db.PROD_LOC_REPORT.Add(BalanceTo);
                        }

                    }
                    







                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
