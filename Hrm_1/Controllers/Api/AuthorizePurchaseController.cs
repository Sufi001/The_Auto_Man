using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class AuthorizePurchaseController : ApiController
    {
        readonly DataContext db;
        public AuthorizePurchaseController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        [ApiPermission("Authorize")]
        public IHttpActionResult AuthorizeDocument(DocumentViewModel vm)
        {
            try
            {
                PurchaseController PurchaseApi = new PurchaseController();
                PurchaseApi.CreatePurchase(vm);

                if (string.IsNullOrEmpty(vm.DocumentMain.DocNo))
                {
                    return BadRequest();
                }
                string txt_totalAmount = vm.DocumentDetailList.Select(x => x.Totalamount).FirstOrDefault();
                string totalQty = vm.DocumentDetailList.Select(x => x.TotalQty).FirstOrDefault();
                string Documentdate = Convert.ToString(vm.DocumentMain.DocDate.Date);
                var documentMain = db.GRN_MAIN.Single(u => u.GRN_NO == vm.DocumentMain.DocNo);
                var documentDetail = db.GRN_DETAIL.Where(u => u.GRN_NO == vm.DocumentMain.DocNo).ToList();

                if (!UpdateDocument(vm.DocumentMain.DocNo))
                    return BadRequest();
                if(!SaveProductBalance(documentDetail, vm.DocumentMain.DocNo, vm.DocumentMain.DocType, documentMain.LOC_ID))
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
                var purchaseMain = db.GRN_MAIN.SingleOrDefault(u => u.GRN_NO == docNo);
                if (purchaseMain != null && purchaseMain.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                {
                    purchaseMain.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveProductBalance(List<GRN_DETAIL> vm, string docno, string docType, string location)
        {
            try
            {
                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in vm)
                {
                    var product = db.PRODUCTS.Where(x=>x.BARCODE == item.BARCODE).SingleOrDefault();
                    var prodBalance = new PROD_BALANCE();
                    var prod = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);
                    if (prod != null)
                    {
                        prodBalance = prod;
                    }
                    prodBalance.GRN_QTY = (prodBalance.GRN_QTY ?? 0) + item.QTY;
                    prodBalance.GRN_AMOUNT = (prodBalance.GRN_AMOUNT ?? 0) + (item.QTY * item.COST)-item.DIS_AMT;

                    var LastCost = prodBalance.LAST_COST ?? 0m;
                    var CurrentQty = prodBalance.CURRENT_QTY ?? 0;
                    var CurrentAmount= (CurrentQty) * (LastCost);
                    var newAmount = (item.QTY ?? 0) * (product.UNIT_COST ?? 0);
                    prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.QTY ?? 0);
                    decimal AvgAmount = 0;
                    if (prodBalance.CURRENT_QTY > 0)
                        AvgAmount = (CurrentAmount + newAmount)/(prodBalance.CURRENT_QTY ?? 1);

                    prodBalance.AVG_COST = AvgAmount;
                    prodBalance.LAST_COST = item.COST;
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
                var supplier = db.SUPPLIERs.SingleOrDefault(x=>x.SUPL_CODE == SuplCode);
                if (supplier != null)
                {
                    supplier.TRANSFER_STATUS = "0";
                    supplier.BALANCE = (supplier.BALANCE ?? 0) + Convert.ToDecimal(totalAmount);
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

        //public bool SaveProductBalanceLog(List<GRN_DETAIL> vm, string docno, string docType, string location)
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
        //            prodBalanceLog.SALE_AMOUNT = (item.QTY * item.COST) - item.dis_amt;
        //            prodBalanceLog.Colour = item.Colour;
        //            if (prodBalanceLog.CURRENT_QTY == null)
        //            {
        //                prodBalanceLog.CURRENT_QTY = 0;
        //            }

        //            prodBalanceLog.CURRENT_QTY = (prodBalanceLog.CURRENT_QTY ?? 0) + item.QTY;

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

        //public bool UpdateProdLog(string totalamount, string totalqty, String Documentdate)
        //{
        //    try
        //    {
        //        bool flag = false;

        //        var Date = Convert.ToDateTime(Documentdate);//DateTime.Now.Date;
        //        prod_log obj = new prod_log();

        //        var Prodobj = db.prod_log.Where(x => x.doc_date == Date).FirstOrDefault();
        //        if (Prodobj != null)
        //        {
        //            obj = Prodobj;
        //            if (obj.grn_qty == null)
        //            {
        //                obj.grn_qty = Convert.ToInt32(totalqty);
        //            }
        //            else
        //            {
        //                obj.grn_qty = obj.grn_qty + Convert.ToInt32(totalqty);
        //            }
        //            if (obj.grn_amt == null)
        //            {
        //                obj.grn_amt = Convert.ToInt32(totalamount);
        //            }
        //            else
        //            {
        //                obj.grn_amt = obj.grn_amt + Convert.ToInt32(totalamount);
        //            }
        //            obj.doc_date = Date;
        //            flag = true;
        //        }
        //        else
        //        {
        //            obj.barcode = "000001";
        //            obj.grn_amt = Convert.ToInt32(totalamount);
        //            obj.doc_date = Date;
        //            obj.grn_amt = Convert.ToInt32(totalqty);
        //        }
        //        if (obj.stock_qty == null)
        //        {
        //            obj.stock_qty = 0 + Convert.ToInt32(totalqty);
        //        }
        //        else
        //        {
        //            obj.stock_qty = obj.stock_qty + Convert.ToInt32(totalqty);
        //        }
        //        if (obj.Stock_amt == null)
        //        {
        //            obj.Stock_amt = 0 + Convert.ToInt32(totalamount);
        //        }
        //        else
        //        {
        //            obj.Stock_amt = obj.Stock_amt + Convert.ToInt32(totalamount);
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
