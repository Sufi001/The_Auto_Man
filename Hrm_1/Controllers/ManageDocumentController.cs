using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    [Permission("Unauthorize")]
    public class ManageDocumentController : Controller
    {
        DataContext db;
        public ManageDocumentController()
        {
            db = new DataContext();
        }
        // GET: DocumentManagement
        public ActionResult Index()
        {
            UnauthorizeDocumentViewModel VM = new UnauthorizeDocumentViewModel();
            return View(VM);
        }
        public ActionResult Save(UnauthorizeDocumentViewModel VM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Some Required Fields Are Missing";
                return View("Index", VM);
            }

            DbContextTransaction transaction = db.Database.BeginTransaction();

            try
            {
                var Status = false;

                if (VM.DocType == "Purchase")
                {
                    var doc = "G" + VM.DocNumber;
                    Status = PurchaseUnauthorize(doc);
                }
                else if (VM.DocType == "PurchaseReturn")
                {
                    var doc = "R" + VM.DocNumber;
                    Status = PurchaseReturnUnauthorize(doc);
                }
                else if (VM.DocType == "Sale")
                {
                    var doc = "IN" + VM.DocNumber;
                    Status = SaleUnauthorize(doc);
                }
                else if (VM.DocType == "SaleReturn")
                {
                    var doc = "IR" + VM.DocNumber;
                    Status = SaleReturnUnauthorize(doc);
                }
                else if (VM.DocType == "Receipt" || VM.DocType == "Payment")
                {
                    var prefix = VM.DocType == "Receipt" ? "R" : "P";
                    var doc = prefix + VM.DocNumber;
                    Status = Voucher_Receipt_Payment_Unauthorize(doc);
                }
                else if (VM.DocType == "StockTransferIn" || VM.DocType == "StockTransferOut")
                {
                    var prefix = VM.DocType == "StockTransferIn" ? "I" : "O";
                    var doc = prefix + VM.DocNumber;
                    Status = TransferUnauthorize(doc);
                }
                else if (VM.DocType == "Waste" || VM.DocType == "Gain")
                {
                    var prefix = VM.DocType == "Waste" ? "W" : "G";
                    var doc = prefix + VM.DocNumber;
                    Status = WasteUnauthorize(doc);
                }

                if (Status)
                {
                    transaction.Commit();
                    TempData["Message"] = "ok";
                }
                else
                {
                    TempData["Message"] = "wrong";
                    transaction.Rollback();
                }

                return View("Index", new UnauthorizeDocumentViewModel());
            }
            catch (Exception)
            {
                TempData["Message"] = "wrong";
                transaction.Rollback();
                return View("Index", new UnauthorizeDocumentViewModel());
            }
        }
        public bool PurchaseUnauthorize(string doc)
        {
            var purchase = db.GRN_MAIN.Include(x => x.GRN_DETAIL).Where(x => x.GRN_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (purchase != null)
            {
                purchase.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                purchase.TRANSFER_STATUS = "0";
                purchase.UPDATED_BY = CommonFunctions.GetUserName();
                purchase.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();


                var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == purchase.SUPL_CODE).SingleOrDefault();
                if (supplier != null)
                {
                    supplier.BALANCE = supplier.BALANCE - purchase.AMOUNT;
                    supplier.TRANSFER_STATUS = "0";
                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    db.SaveChanges();
                }

                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in purchase.GRN_DETAIL)
                {
                    item.TRANSFER_STATUS = "0";
                    var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                    prodBalance.GRN_QTY = (prodBalance.GRN_QTY ?? 0) - item.QTY;
                    prodBalance.GRN_AMOUNT = (prodBalance.GRN_AMOUNT ?? 0) - ((item.QTY * item.COST) - item.DIS_AMT);
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY ?? 0);
                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool PurchaseReturnUnauthorize(string doc)
        {
            var purchaseReturn = db.GRFS_MAIN.Include(x => x.GRFS_DETAIL).Where(x => x.GRF_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (purchaseReturn != null)
            {
                purchaseReturn.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                purchaseReturn.TRANSFER_STATUS = "0";
                purchaseReturn.UPDATED_BY = CommonFunctions.GetUserName();
                purchaseReturn.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();

                var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == purchaseReturn.SUPL_CODE).SingleOrDefault();
                if (supplier != null)
                {
                    supplier.BALANCE = supplier.BALANCE + purchaseReturn.GRFS_DETAIL.Sum(x => (x.COST * x.QTY) - (x.DISCOUNT));
                    supplier.TRANSFER_STATUS = "0";
                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    db.SaveChanges();
                }
                db.SaveChanges();

                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in purchaseReturn.GRFS_DETAIL)
                {
                    item.TRANSFER_STATUS = "0";

                    var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                    prodBalance.GRF_QTY = (prodBalance.GRF_QTY ?? 0) - item.QTY;
                    prodBalance.GRF_AMOUNY = (prodBalance.GRF_AMOUNY ?? 0) - ((item.QTY * item.COST) - item.DISCOUNT);
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.QTY ?? 0);

                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool SaleUnauthorize(string doc)
        {
            var sale = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (sale != null)
            {
                sale.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                sale.TRANSFER_STATUS = "0";

                db.SaveChanges();

                var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == sale.PARTY_CODE).SingleOrDefault();
                if (supplier != null)
                {
                    supplier.BALANCE = supplier.BALANCE - sale.CASH_AMT;
                    supplier.TRANSFER_STATUS = "0";
                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    db.SaveChanges();
                }
                db.SaveChanges();

                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in sale.TRANS_DT)
                {
                    item.TRANSFER_STATUS = "0";

                    var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                    prodBalance.SALE_QTY = (prodBalance.SALE_QTY ?? 0) - item.UNITS_SOLD;
                    prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) - (item.AMOUNT);
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.UNITS_SOLD);

                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool SaleReturnUnauthorize(string doc)
        {
            var saleReturn = db.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (saleReturn != null)
            {
                saleReturn.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                saleReturn.TRANSFER_STATUS = "0";

                db.SaveChanges();

                var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == saleReturn.PARTY_CODE).SingleOrDefault();
                if (supplier != null)
                {
                    supplier.BALANCE = supplier.BALANCE + saleReturn.CASH_AMT;
                    supplier.TRANSFER_STATUS = "0";
                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    db.SaveChanges();
                }
                db.SaveChanges();

                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in saleReturn.TRANS_DT)
                {
                    item.TRANSFER_STATUS = "0";

                    var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                    prodBalance.SALE_QTY = (prodBalance.SALE_QTY ?? 0) + item.UNITS_SOLD;
                    prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) + (item.AMOUNT);
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.UNITS_SOLD);

                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool WasteUnauthorize(string doc)
        {
            var wast_Gain = db.WAST_MAIN.Include(x => x.WAST_DETAIL).Where(x => x.DOC_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (wast_Gain != null)
            {
                wast_Gain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                wast_Gain.TRANSFER_STATUS = "0";
                wast_Gain.UPDATED_BY = CommonFunctions.GetUserName();
                wast_Gain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();

                var prodBalanceList = db.PROD_BALANCE.ToList();

                if (wast_Gain.DOC_TYPE == Inventory.Constants.Constants.WastDocument)
                {
                    foreach (var item in wast_Gain.WAST_DETAIL)
                    {
                        item.TRANSFER_STATUS = "0";

                        var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                        prodBalance.WAST_QTY = (prodBalance.WAST_QTY ?? 0) - item.QTY;
                        prodBalance.WAST_AMOUNT = (prodBalance.WAST_AMOUNT ?? 0) - (item.QTY * item.COST);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.QTY);
                        prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    }
                }
                else
                {
                    foreach (var item in wast_Gain.WAST_DETAIL)
                    {
                        item.TRANSFER_STATUS = "0";

                        var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);
                        prodBalance.GAIN_QTY = (prodBalance.GAIN_QTY ?? 0) - item.QTY;
                        prodBalance.GAIN_AMOUNT = (prodBalance.GAIN_AMOUNT ?? 0) - (item.QTY * item.COST);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY);
                        prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    }
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool TransferUnauthorize(string doc)
        {
            var Transfer_In_Out = db.TRANSFER_MAIN.Include(x => x.TRANSFER_DETAIL).Where(x => x.DOC_NO == doc && x.STATUS != Constants.DocumentStatus.UnauthorizedDocument).SingleOrDefault();
            if (Transfer_In_Out != null)
            {
                Transfer_In_Out.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                Transfer_In_Out.TRANSFER_STATUS = "0";
                Transfer_In_Out.UPDATED_BY = CommonFunctions.GetUserName();
                Transfer_In_Out.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();

                var prodBalanceList = db.PROD_BALANCE.ToList();

                if (Transfer_In_Out.DOC_TYPE == Inventory.Constants.Constants.TransferIn)
                {
                    foreach (var item in Transfer_In_Out.TRANSFER_DETAIL)
                    {
                        item.TRANSFER_STATUS = "0";

                        var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                        prodBalance.TRANSFER_IN_QTY = (prodBalance.TRANSFER_IN_QTY ?? 0) - item.QTY;
                        prodBalance.TRANSFER_IN_AMOUNT = (prodBalance.TRANSFER_IN_AMOUNT ?? 0) - (item.QTY * item.COST);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY);

                        prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    }
                }
                else
                {
                    foreach (var item in Transfer_In_Out.TRANSFER_DETAIL)
                    {
                        item.TRANSFER_STATUS = "0";

                        var prodBalance = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);

                        prodBalance.TRANSFER_OUT_QTY = (prodBalance.TRANSFER_OUT_QTY ?? 0) - item.QTY;
                        prodBalance.TRANSFER_OUT_AMOUNT = (prodBalance.TRANSFER_OUT_AMOUNT ?? 0) - (item.QTY * item.COST);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.QTY);

                        prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    }
                }
                db.SaveChanges();
                return true;
            }
            else
                return false;
            }
        public bool Voucher_Receipt_Payment_Unauthorize(string doc)
        {

            var journal = db.JOURNALs.Where(x => x.DOC_SEQ == doc).SingleOrDefault();
            if (journal != null)
            {
                journal.TRANSFER_STATUS = "0";
                journal.STATUS = "0";
                journal.UPDATED_BY = CommonFunctions.GetUserName();
                journal.UPDATED_DTE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();

                var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == journal.PARTY_CODE).Single();
                if (supplier != null)
                {
                    supplier.BALANCE = (supplier.BALANCE ?? 0) + journal.AMOUNT;
                    supplier.TRANSFER_STATUS = "0";
                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    db.SaveChanges();
                }

                return true;
            }
            else
                return false;
        }
    }
}