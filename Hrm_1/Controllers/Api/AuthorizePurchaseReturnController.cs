using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class AuthorizePurchaseReturnController : ApiController
    {
        readonly DataContext db;
        public AuthorizePurchaseReturnController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        [ApiPermission("Authorize")]
        public IHttpActionResult AuthorizeDocument(DocumentViewModel vm)
        {
            try
            {
                PurchaseReturnController PurchaseReturnAPi = new PurchaseReturnController();
                PurchaseReturnAPi.CreatePurchaseReturn(vm);

                if (string.IsNullOrEmpty(vm.DocumentMain.DocNo))
                {
                    return BadRequest();
                }
                string txt_totalAmount = vm.DocumentDetailList.Select(x => x.Totalamount).FirstOrDefault();
                string totalQty = vm.DocumentDetailList.Select(x => x.TotalQty).FirstOrDefault();
                string Documentdate = Convert.ToString(vm.DocumentMain.DocDate.Date);
                var documentMain = db.GRFS_MAIN.Single(u => u.GRF_NO == vm.DocumentMain.DocNo);
                var documentDetail = db.GRFS_DETAIL.Where(u => u.GRF_NO == vm.DocumentMain.DocNo).ToList();

                if (!UpdateDocument(vm.DocumentMain.DocNo))
                    return BadRequest();
                if(!SaveProductBalance(documentDetail, vm.DocumentMain.DocNo, vm.DocumentMain.DocType, documentMain.LOCATION))
                {
                    return BadRequest();
                }

                SaveSupplierBalance(vm.DocumentMain.SuplCode, txt_totalAmount);
                //UpdateProdLog(txt_totalAmount, totalQty,Documentdate);
                db.SaveChanges();
                return Ok();

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
                var purchaseReturnMain = db.GRFS_MAIN.SingleOrDefault(u => u.GRF_NO == docNo);
                if (purchaseReturnMain != null && purchaseReturnMain.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                {
                    purchaseReturnMain.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                    purchaseReturnMain.UPDATED_BY = CommonFunctions.GetUserName();
                    purchaseReturnMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    //   db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveProductBalance(List<GRFS_DETAIL> vm, string docno, string docType, string location)
        {
            try
            {
                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in vm)
                {
                    var product = db.PRODUCTS.Where(x => x.BARCODE == item.BARCODE).SingleOrDefault();
                    var prodBalance = new PROD_BALANCE();
                    var prod = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE );
                    if (prod != null)
                    {
                        prodBalance = prod;
                    }

                    prodBalance.GRF_QTY = (prodBalance.GRF_QTY ?? 0) + item.QTY;
                    prodBalance.GRF_AMOUNY = ((prodBalance.GRF_AMOUNY ?? 0) + (item.QTY * item.COST))-item.DISCOUNT;
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.QTY ?? 0);

                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                    if (prod == null)
                    {
                        prodBalance.BARCODE = item.BARCODE;
                        db.PROD_BALANCE.Add(prodBalance);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveSupplierBalance(string SuplCode, string totalAmount)
        {
            try
            {
                var supplier = db.SUPPLIERs.SingleOrDefault(x => x.SUPL_CODE == SuplCode);
                if (supplier != null)
                {
                    supplier.TRANSFER_STATUS = "0";
                    supplier.BALANCE -= Convert.ToDecimal(totalAmount);

                    supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //public bool SaveProductBalanceLog(List<GRFS_DETAIL> vm, string docno, string docType, string location)
        //{
        //    try
        //    {
        //        var prodBalanceList = db.PROD_BALANCE.Where(u => u.Location == location).ToList();
        //        var prodBalanceLogList = db.PROD_LOCREPORT.Where(u => u.Location == location).ToList();
        //        var date = DateTime.Now;
        //        foreach (var item in vm)
        //        {
        //            var prodBalanceLog = new PROD_LOCREPORT();
        //            var prod = prodBalanceLogList.SingleOrDefault(
        //                u => u.BARCODE == item.BARCODE && u.Colour == item.Colour
        //                     && u.Location == location && u.Warehouse == item.Warehouse && u.DOC == date);
        //            var prodid = prodBalanceList.Single(u => u.BARCODE == item.BARCODE && u.Colour == item.Colour
        //                && u.Location == location && u.Warehouse == item.Warehouse);
        //            if (prod != null)
        //            {
        //                prodBalanceLog = prod;
        //            }
        //            prodBalanceLog.DOC = DateTime.Now;
        //            prodBalanceLog.SALE_AMOUNT = (item.QTY * item.COST) - item.DISCOUNT;
        //            prodBalanceLog.Colour = item.Colour;
        //            if (prodBalanceLog.CURRENT_QTY == null)
        //            {
        //                prodBalanceLog.CURRENT_QTY = 0;
        //            }
        //            prodBalanceLog.CURRENT_QTY = (prodBalanceLog.CURRENT_QTY ?? 0) - item.QTY;

        //            if (prod == null)
        //            {
        //                prodBalanceLog.BARCODE = item.BARCODE;
        //                prodBalanceLog.Colour = item.Colour;
        //                prodBalanceLog.Warehouse = item.Warehouse;
        //                prodBalanceLog.Location = location;
        //                prodBalanceLog.ProdBalance_Id = prodid.Id;
        //                db.PROD_LOCREPORT.Add(prodBalanceLog);
        //            }

        //        }
        //        db.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return false;
        //    }
        //}


        //public bool UpdateProdLog(string totalamount, string totalqty, string Documentdate)
        //{
        //    try
        //    {
        //        bool flag = false;
        //        //   var Date = DateTime.Now.Date;
        //        var Date = Convert.ToDateTime(Documentdate);
        //        prod_log obj = new prod_log();
        //        var Prodobj = db.prod_log.Where(x => x.doc_date == Date).FirstOrDefault();
        //        if (Prodobj != null)
        //        {
        //            obj = Prodobj;
        //            if (obj.grf_qty == null)
        //            {
        //                obj.grf_qty = Convert.ToInt32(totalqty);
        //            }
        //            else
        //            {
        //                obj.grf_qty = obj.grf_qty + Convert.ToInt32(totalqty);
        //            }
        //            if (obj.grf_amt == null)
        //            {
        //                obj.grf_amt = Convert.ToInt32(totalamount);
        //            }
        //            else
        //            {
        //                obj.grf_amt = obj.grf_amt + Convert.ToInt32(totalamount);
        //            }
        //            obj.doc_date = Date;
        //            flag = true;
        //        }
        //        else
        //        {
        //            obj.barcode = "000001";
        //            obj.grf_amt = Convert.ToInt32(totalamount);
        //            obj.doc_date = Date;
        //            obj.grf_qty = Convert.ToInt32(totalqty);
        //            // db.prod_log
        //        }
        //        if (obj.stock_qty == null)
        //        {
        //            obj.stock_qty = 0 - Convert.ToInt32(totalqty);
        //        }
        //        else
        //        {
        //            obj.stock_qty = obj.stock_qty - Convert.ToInt32(totalqty);
        //        }
        //        if (obj.Stock_amt == null)
        //        {
        //            obj.Stock_amt = 0 - Convert.ToInt32(totalamount);
        //        }
        //        else
        //        {
        //            obj.Stock_amt = obj.Stock_amt - Convert.ToInt32(totalamount);
        //        }
        //        if (flag == false)
        //        {
        //            db.prod_log.Add(obj);
        //        }
        //        return flag;
        //    }
        //    catch (Exception ex)
        //    {
        //        var exception = ex.ToString();
        //        return false;
        //    }
        //}
    }
}
