using CustomerRec;
using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class AuthorizeController : ApiController
    {
        readonly DataContext db;
        public AuthorizeController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        [ApiPermission("Authorize")]
        public IHttpActionResult AuthorizeDocument(DocumentViewModel vm)
        {
            try
            {
                SalesController SalesApi = new SalesController();
                SalesApi.CreateSales(vm);
                if (string.IsNullOrEmpty(vm.DocumentMain.DocNo))
                {
                    return BadRequest();
                }
                string txt_totalAmount = vm.DocumentDetailList.Select(x => x.Totalamount).FirstOrDefault();
                string totalQty = vm.DocumentDetailList.Select(x => x.TotalQty).FirstOrDefault();
                string Documentdate = Convert.ToString(vm.DocumentMain.DocDate.Date);
                var documentMain = db.TRANS_MN.Single(u => u.TRANS_NO == vm.DocumentMain.DocNo);
                //var documentDetail = db.TRANS_DT.Where(u => u.TRANS_NO == vm.DocumentMain.DocNo&&u.VOID=="F").ToList();
                var documentDetail = db.TRANS_DT.Where(u => u.TRANS_NO == vm.DocumentMain.DocNo).ToList();
                if (!UpdateDocument(vm.DocumentMain.DocNo))
                    return BadRequest();
                if(!SaveProductBalance(documentDetail, vm.DocumentMain.DocNo, vm.DocumentMain.DocType, documentMain.LOC_ID))
                {
                    return BadRequest();
                }

                SaveCustomerBalance(vm.DocumentMain.SuplCode, txt_totalAmount, vm.DocumentMain.DocType);
                //UpdateProdLog(txt_totalAmount,totalQty,Documentdate);
                db.SaveChanges();
                //MyConnection conn = new MyConnection();
                //conn.Select("update advance_log set status = 'C' where trans_no = '"+vm.DocumentMain.DocNo+"'");
             
                return Ok();
            }
            catch (System.Exception ex)
            {
                string s = ex.ToString();
                return BadRequest(ex.ToString());
            }
        }
        public bool SaveCustomerBalance(string SuplCode, string totalAmount,string doctype)
        {
            try
            {
                var supplier = db.SUPPLIERs.SingleOrDefault(x => x.SUPL_CODE == SuplCode);
                if (supplier != null)
                {
                    supplier.TRANSFER_STATUS = "0";
                    if (doctype == Inventory.Constants.Constants.SalesDocument)
                    {
                        supplier.BALANCE = (supplier.BALANCE ?? 0)+ Convert.ToDecimal(totalAmount);
                        return true;
                    }
                    else
                    {
                        supplier.BALANCE = (supplier.BALANCE ?? 0) - Convert.ToDecimal(totalAmount);
                        return true;
                    }
                    
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool UpdateDocument(string docNo)
        {
            try
            {
                var salesMain = db.TRANS_MN.SingleOrDefault(u => u.TRANS_NO == docNo);
                if (salesMain != null && salesMain.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                {
                    salesMain.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveProductBalance(List<TRANS_DT> vm, string docno, string docType, string location)
        {
            try
            {
                var prodBalanceList = db.PROD_BALANCE.ToList();
                foreach (var item in vm)
                {
                    var prodBalance = new PROD_BALANCE();
                    var prod = prodBalanceList.SingleOrDefault(u => u.BARCODE == item.BARCODE);
                    if (prod != null)
                    {
                        prodBalance = prod;
                    }
                    //var discount = item.UNIT_RETAIL * ((item.DIS_AMOUNT * item.UNITS_SOLD)/100) ?? 0;
                    if (docType == Inventory.Constants.Constants.SalesDocument)
                    {
                        prodBalance.SALE_QTY = (prodBalance.SALE_QTY ?? 0) + item.UNITS_SOLD;
                        //prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) + (item.UNITS_SOLD * item.UNIT_RETAIL) - discount;
                        prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) + (item.AMOUNT);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) - (item.UNITS_SOLD);
                    }
                    else
                    {
                        prodBalance.SALE_QTY = (prodBalance.SALE_QTY ?? 0) - item.UNITS_SOLD;
                        //prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) -((item.UNITS_SOLD * item.UNIT_RETAIL) - discount);
                        prodBalance.SALE_AMOUNT = (prodBalance.SALE_AMOUNT ?? 0) - (item.AMOUNT);
                        prodBalance.CURRENT_QTY = (prodBalance.CURRENT_QTY ?? 0) + (item.UNITS_SOLD);
                    }

                    prodBalance.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    if (prod == null)
                    {
                        prodBalance.BARCODE = item.BARCODE;
                        //prodBalance.Colour = item.Colour;
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

        //public bool SaveProductBalanceLog(List<TRANS_DT> vm, string docno, string docType, string location)
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
        //            prodBalanceLog.SALE_AMOUNT = (item.UNITS_SOLD * item.UNIT_RETAIL) - item.dis_amount;
        //            prodBalanceLog.Colour = item.Colour;
        //            if (prodBalanceLog.CURRENT_QTY == null)
        //            {
        //                prodBalanceLog.CURRENT_QTY = 0;
        //            }
        //            if (docType == Constants.Constants.SalesDocument)
        //            {
        //                prodBalanceLog.CURRENT_QTY = (prodBalanceLog.CURRENT_QTY ?? 0) - item.UNITS_SOLD;
        //            }
        //            else
        //            {
        //                prodBalanceLog.CURRENT_QTY = (prodBalanceLog.CURRENT_QTY ?? 0) + item.UNITS_SOLD;
        //            }
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
        //        // var Date = DateTime.Now.Date;
        //        var Date = Convert.ToDateTime(Documentdate);
        //        prod_log obj = new prod_log();
        //        var Prodobj = db.prod_log.Where(x => x.doc_date == Date).FirstOrDefault();
        //        if (Prodobj != null)
        //        {
        //            obj = Prodobj;
        //            if (obj.sale_qty == null)
        //            {
        //                obj.sale_qty = Convert.ToInt32(totalqty);
        //            }
        //            else
        //            {
        //                obj.sale_qty = obj.sale_qty + Convert.ToInt32(totalqty);
        //            }
        //            if (obj.sale_amt == null)
        //            {
        //                obj.sale_amt = Convert.ToInt32(totalamount);
        //            }
        //            else
        //            {
        //                obj.sale_amt = obj.sale_amt + Convert.ToInt32(totalamount);
        //            }
        //            obj.doc_date = Date;
        //            flag = true;
        //        }
        //        else
        //        {
        //            obj.barcode = "000001";
        //            obj.sale_amt = Convert.ToInt32(totalamount);
        //            obj.doc_date = Date;
        //            obj.sale_qty = Convert.ToInt32(totalqty);
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
