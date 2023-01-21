using CustomerRec;
using Inventory.Helper;
using Inventory.Models;
using Inventory.Models.FBR;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace Inventory.Controllers.Api
{
    public class SalesController : ApiController
    {
        readonly DataContext db;
        Invoice FBRInvoice;
        public SalesController()
        {
            db = new DataContext();
        }
        string DocType = ""; //IN or IR
        [Authorize]
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult CreateSales(DocumentViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid || vm.DocumentDetailList.Count == 0)
                {
                    return BadRequest();
                }
                var TotalAmount = Convert.ToDecimal(vm.DocumentMain.TotalAmount);
                if (Convert.ToDecimal(vm.DocumentMain.Advance) > TotalAmount)
                {
                    return Ok("NotSaved");
                }
                DocType = vm.DocumentMain.DocType;
                var docno = SaveSalesMain(vm.DocumentMain);
                if (docno != "" && docno != Constants.DocumentStatus.AuthorizedDocument)
                {
                    if (SaveSalesDetail(vm.DocumentDetailList, docno, vm.DocumentMain.DocType))
                    {

                        FBR_DATA fbr = new FBR_DATA();
                        fbr.FBR_NO = "";
                        fbr.INVOICE_NO = docno;
                        fbr.STATUS = "0";
                        if (!db.FBR_DATA.Any(x => x.INVOICE_NO == docno))
                            db.FBR_DATA.Add(fbr);

                        db.SaveChanges();
                        //Printerclass obj = new Printerclass();
                        return Ok(docno);
                    }
                    else
                    {
                        return BadRequest("Document is not active.");
                    }
                }
                return BadRequest("Document is not active.");

            }
            catch (System.Exception ex)
            {
                //var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();
                //var tdinDbCount = salesDetailDbList.Count;
                //if (tdinDbCount > 0)
                //{
                //    // db.TRANS_DT.RemoveRange(salesDetailDbList);}
                //}
                string s = ex.ToString();
                return BadRequest(ex.ToString());
            }
        }
        public bool updatecustomer(string code, string Phoneno)
        {
            bool flag = true;
            if (code != "")
            {
                var suppl = db.SUPPLIERs.SingleOrDefault(x => x.SUPL_CODE == code & x.PARTY_TYPE == "c");
                if (!string.IsNullOrEmpty(Phoneno))
                {
                    suppl.MOBILE = Phoneno;
                    suppl.TRANSFER_STATUS = "0";
                }
            }
            return flag;
        }
        bool saveOrUpdateFlag = false;
        //public string facebookstatus = "";
        public string username = "";
        public string advance = "";
        [NonAction]
        public string SaveSalesMain(DocumentMainViewModel vm)
        {
            try
            {
                saveOrUpdateFlag = false;
                var salesMain = new TRANS_MN();

                if (string.IsNullOrEmpty(vm.DocNo))
                {
                    string prefix = vm.DocType;

                    //string prefix = "IN";
                    salesMain.TRANS_NO = vm.GetSaleCode(prefix, 6);
                    //salesMain.TRANS_NO = vm.GetSaleCode(prefix, 6, (vm.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn);
                    saveOrUpdateFlag = true;
                    salesMain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                    salesMain.SALE_TYPE = vm.RequestPage;
                }
                else
                {
                    if (!saveOrUpdateFlag)
                    {
                        if (Convert.ToInt32(vm.Advance) > 0 && vm.Advance != null)
                        {
                            Updateadvanceoverride(vm, vm.DocNo);
                        }
                    }

                    salesMain = db.TRANS_MN.SingleOrDefault(u => u.TRANS_NO == vm.DocNo && u.STATUS != Constants.DocumentStatus.AuthorizedDocument);
                    if (salesMain != null)
                    {
                        advance = Convert.ToString(salesMain.ADVANCE);
                    }
                    else
                        return Constants.DocumentStatus.AuthorizedDocument;
                }

                salesMain.TRANS_DATE = vm.DocDate;
                salesMain.START_TIME = DateTime.Now;
                // salesMain.cust_id = vm.SuplCode;
                salesMain.TRANS_TYPE = (vm.DocType == Constants.Constants.SalesDocument) ? Constants.TransType.Sales : Constants.TransType.SalesReturn;
                salesMain.LOC_ID = vm.Location;
                salesMain.DN_NO = vm.DnNo;
                //salesMain.Warehouse = vm.Warehouse;
                salesMain.TIME_SLOT = vm.Time;
                salesMain.TRANSFER_STATUS = "0";
                salesMain.TILL_NO = "";
                salesMain.SALESMAN_CDE = vm.staffcode;
                salesMain.CUST_NAME = db.SUPPLIERs.Where(x => x.SUPL_CODE == vm.SuplCode).Select(x => x.SUPL_NAME).SingleOrDefault();
                salesMain.BY_CARD = Convert.ToDecimal(Constants.PaymentType.Card);
                salesMain.PARTY_CODE = vm.SuplCode;
                salesMain.ADVANCE = Convert.ToInt32(vm.Advance);
                salesMain.CASH_AMT = Convert.ToDecimal(vm.TotalAmount);
                salesMain.BY_CASH = salesMain.CASH_AMT.HasValue ? salesMain.CASH_AMT.Value : 0;
                salesMain.DISC_AMNT = Convert.ToDecimal(vm.DiscointAmount);
                salesMain.ADVANCE = Convert.ToInt32(vm.Advance);
                if (vm.Phone != null)
                {
                    salesMain.MOB = vm.Phone.Trim();
                }
                salesMain.CANCEL = "F";
                //salesMain.TRANS_DATE = vm.DocDate+vm.Time;
                salesMain.TYPE = "A";
                salesMain.BY_CASH = Convert.ToDecimal(vm.Payment);
                salesMain.USER_ID = CommonFunctions.GetUserID();

                if (vm.ReturnAmount == null)
                {
                    salesMain.RET_AMT = 0;//Convert.ToInt32(vm.ReturnAmount);
                }
                else
                {
                    salesMain.RET_AMT = Convert.ToDecimal(vm.ReturnAmount);
                }

                updatecustomer(vm.SuplCode, vm.Phone);

                if (saveOrUpdateFlag)
                {
                    db.TRANS_MN.Add(salesMain);
                    if (Convert.ToInt32(vm.Advance) > 0 && vm.Advance != null)
                    {
                        Saveadvanceoverride(vm, salesMain.TRANS_NO);
                    }
                }
                return salesMain.TRANS_NO;
            }
            catch (System.Exception ex)
            {

                return "";
            }
        }
        [NonAction]
        public bool SaveSalesDetail(List<DocumentDetailViewModel> vm, string docno, string docType)
        {
            try
            {
                if (saveOrUpdateFlag == false)
                    priceoverrid(vm, docno);

                var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();

                db.TRANS_DT.RemoveRange(salesDetailDbList);

                foreach (var item in vm)
                {
                    var salesDetail = new TRANS_DT();
                    var product = db.PRODUCTS.Where(x => x.BARCODE == item.Barcode).SingleOrDefault();
                    var ProdGstFlag = product.GST;
                    var ProdGstPercentage = product.GST_PER ?? 0;
                    if (product.GST == "G")
                    {
                        salesDetail.GST_AMOUNT = ((item.Cost * ProdGstPercentage) / (ProdGstPercentage + 100));
                    }
                    else if (product.GST == "R")
                    {
                        salesDetail.GST_AMOUNT = ((item.Cost * ProdGstPercentage) / (ProdGstPercentage + 100));
                    }
                    else
                    {
                        salesDetail.GST_AMOUNT = 0;
                    }
                    salesDetail.TRANS_NO = docno;
                    salesDetail.BARCODE = item.Barcode;
                    salesDetail.UNIT_COST = item.Cost;
                    salesDetail.UNIT_RETAIL = item.Retail;
                    salesDetail.SCAN_TIME = DateTime.Now;
                    salesDetail.UAN_NO = item.Uanno;
                    //salesDetail.dis_amount = item.Discount;
                    salesDetail.TRANSFER_STATUS = "0";
                    salesDetail.AMOUNT = item.Amount;
                    salesDetail.FREE_QTY = item.FreeQty;
                    salesDetail.CTN_QTY = item.CtnQty;
                    salesDetail.UNITS_SOLD = item.Qty;
                    salesDetail.SUPL_CODE = item.staffcode;
                    var totalamount = item.Retail * item.Qty;
                    salesDetail.DIS_AMOUNT = (item.Discount / totalamount) * 100;
                    salesDetail.EXCH_FLAG = "T";
                    salesDetail.VOID = "F";
                    salesDetail.COLOUR = item.Colour;
                    salesDetail.S_TIME = item.DateTimeSlot;
                    salesDetail.WAREHOUSE = item.Warehouse;
                    salesDetail.TRANS_TYPE = docType == Constants.Constants.SalesDocument ? Constants.TransType.Sales : Constants.TransType.SalesReturn;
                    db.TRANS_DT.Add(salesDetail);
                }

                return true;
            }
            catch (System.Exception)
            {
                var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();
                var tdinDbCount = salesDetailDbList.Count;
                if (tdinDbCount > 0)
                {
                    // db.TRANS_DT.RemoveRange(salesDetailDbList);}
                }
                return false;
            }
        }
        [NonAction]
        public bool priceoverrid(List<DocumentDetailViewModel> vm, string docno)
        {
            var transdtlist = db.TRANS_DT.ToList();
            foreach (var item in vm)
            {
                var obj = new PRICE_OVERRIDE_LOG();
                if (transdtlist.Count > 0)
                {
                    var previousobj = transdtlist.Where(x => x.TRANS_NO == docno && x.BARCODE == item.Barcode).FirstOrDefault();
                    if (previousobj != null)
                    {
                        var previousUnitcost = Convert.ToInt32(previousobj.UNIT_COST);
                        var previousRetailcost = Convert.ToInt32(previousobj.UNIT_RETAIL);
                        if (previousUnitcost != item.Cost || previousRetailcost != item.Retail)
                        {
                            obj.PREV_COST = previousobj.UNIT_COST;
                            obj.PREV_RETAIL = previousobj.UNIT_RETAIL;
                            obj.BARCODE = item.Barcode;
                            obj.TRANS_NO = docno;
                            obj.UNIT_COST = item.Cost;
                            obj.UNIT_RETAIL = item.Retail;
                            obj.PRICE_DATE = DateTime.Now;
                            obj.USER_ID = username;
                            obj.Qty = item.Qty;
                            obj.Trans_Type = DocType == Constants.Constants.SalesDocument ? Constants.TransType.Sales : Constants.TransType.SalesReturn;
                            var transdt = db.TRANS_DT.Where(x => x.TRANS_NO == docno && x.BARCODE == item.Barcode).FirstOrDefault();
                            transdt.PRICE_OVR = "T";
                            db.PRICE_OVERRIDE_LOG.Add(obj);
                        }
                    }
                }

            }
            //          }
            return true;
        }
        [NonAction]
        public bool Updateadvanceoverride(DocumentMainViewModel vm, string docno)
        {
            try
            {
                var transdtlist = db.TRANS_MN.Where(x => x.TRANS_NO == docno).FirstOrDefault();
                var obj = new ADVANCE_LOG();
                if (transdtlist != null)
                {
                    var previousUnitcost = Convert.ToInt32(transdtlist.ADVANCE);
                    var advance = Convert.ToInt32(vm.Advance);
                    if (previousUnitcost != advance)
                    {
                        obj.Amount = advance;
                        if (!String.IsNullOrEmpty(username))
                            obj.InsertedBy = username.Trim();

                        obj.InsertedDate = DateTime.Now;
                        obj.Amt_Date = vm.DocDate;
                        obj.Trans_No = docno;
                        obj.Type = "A";
                        obj.Status = "P";
                        db.ADVANCE_LOG.Add(obj);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var data = ex.ToString();
                return false;
            }
        }
        [NonAction]
        public bool Saveadvanceoverride(DocumentMainViewModel vm, string docno)
        {
            try
            {
                var obj = new ADVANCE_LOG();
                var advance = Convert.ToInt32(vm.Advance);
                obj.Amount = advance;
                obj.InsertedBy = username;
                obj.InsertedDate = DateTime.Now;
                obj.Amt_Date = vm.DocDate;
                obj.Trans_No = docno;
                obj.Type = "A";
                obj.Status = "P";
                db.ADVANCE_LOG.Add(obj);
                return true;
            }
            catch (Exception ex)
            {
                var data = ex.ToString();
                return false;
            }
        }

        //string ProductSales = "";
        //[Authorize]
        //[System.Web.Mvc.HttpPost]
        //public IHttpActionResult CreateSales(DocumentViewModel vm)
        //{
        //    //Doctype = PIN for Sales/Index
        //    //Doctype = IN for SalesScreen/Index
        //    //Trans_Type = "4" for Sales/Index Both In Trans_MN and Trans_DT
        //    //Trans_Type = "2" for Sales/Index Both In Trans_MN and Trans_DT
        //    //docType = "2" for Both Sales/Index and SalesScreen/Index

        //    var Documentnumber = vm.DocumentMain.DocNo;
        //    try
        //    {
        //        if (!ModelState.IsValid || vm.DocumentDetailList.Count == 0)
        //        {
        //            return BadRequest();
        //        }
        //        var TotalAmount = Convert.ToInt32(vm.DocumentMain.TotalAmount);
        //        if (Convert.ToInt32(vm.DocumentMain.Advance) > TotalAmount)
        //        {
        //            return Ok("NotSaved");
        //        }
        //        ProductSales = vm.DocumentMain.DocType;
        //        var docno = SaveSalesMain(vm.DocumentMain);
        //        if (docno != "" && docno != Constants.Constants.UnauthorizedDocument)
        //        {
        //            if (SaveSalesDetail(vm.DocumentDetailList, docno, vm.DocumentMain.DocType))
        //            {
        //                db.SaveChanges();
        //                //Printerclass obj = new Printerclass();
        //                return Ok(docno);
        //            }
        //            else
        //            {
        //                return BadRequest("Document is not active.");
        //            }
        //        }
        //        return BadRequest("Document is not active.");

        //    }
        //    catch (System.Exception ex)
        //    {
        //        //var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();
        //        //var tdinDbCount = salesDetailDbList.Count;
        //        //if (tdinDbCount > 0)
        //        //{
        //        //    // db.TRANS_DT.RemoveRange(salesDetailDbList);}
        //        //}
        //        string s = ex.ToString();
        //        return BadRequest(ex.ToString());
        //    }
        //}
        //public bool updatecustomer(string code, string Phoneno)
        //{
        //    bool flag = true;
        //    if (code != "")
        //    {
        //        var suppl = db.SUPPLIERs.SingleOrDefault(x => x.SUPL_CODE == code & x.party_type == "c");
        //        suppl.MOBILE = Phoneno;
        //        //  db.SaveChanges();
        //    }
        //    return flag;
        //}
        //bool saveOrUpdateFlag = false;
        //public string facebookstatus = "";
        //public string username = "";
        //public string advance = "";
        //[NonAction]
        //public string SaveSalesMain(DocumentMainViewModel vm)
        //{
        //    try
        //    {
        //        saveOrUpdateFlag = false;
        //        var salesMain = new TRANS_MN();
        //        if (!String.IsNullOrEmpty(vm.Userid))
        //        {
        //            username = vm.Userid.Trim();
        //        }
        //        if (string.IsNullOrEmpty(vm.DocNo))
        //        {
        //            // string prefix = vm.DocType;
        //            string prefix = "IN";
        //            salesMain.TRANS_NO = vm.GetSaleCode(prefix, 6);
        //            //salesMain.TRANS_NO = vm.GetSaleCode(prefix, 6, (vm.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn);
        //            saveOrUpdateFlag = true;
        //        }
        //        else
        //        {
        //            if (!saveOrUpdateFlag)
        //            {
        //                if (Convert.ToInt32(vm.Advance) > 0 && vm.Advance != null)
        //                {
        //                    Updateadvanceoverride(vm, vm.DocNo);
        //                }
        //            }

        //            salesMain = db.TRANS_MN.SingleOrDefault(u => u.TRANS_NO == vm.DocNo && u.status == Constants.Constants.UnauthorizedDocument);
        //            if (salesMain != null)
        //            {
        //                salesMain.USER_ID = vm.Userid;
        //                salesMain.TRANS_DATE = DateTime.Now;
        //                if (ProductSales != "PIN")
        //                {
        //                    facebookstatus = salesMain.TRANS_TYPE;
        //                }
        //                else
        //                {
        //                    facebookstatus = "";
        //                }
        //                advance = Convert.ToString(salesMain.advance);
        //            }
        //            else
        //                return Constants.Constants.UnauthorizedDocument;
        //        }
        //        //     salesMain.TRANS_DATE = vm.DocDate;
        //        salesMain.START_TIME = DateTime.Now;
        //        // salesMain.cust_id = vm.SuplCode;
        //        salesMain.status = Constants.Constants.UnauthorizedDocument;
        //        if (facebookstatus == "" && ProductSales != "PIN")
        //        {
        //            salesMain.TRANS_TYPE = (vm.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn;
        //        }
        //        else if (facebookstatus == "" && ProductSales == "PIN")
        //        {
        //            salesMain.TRANS_TYPE = "4";
        //        }
        //        else if (facebookstatus == "2" || facebookstatus == "1")
        //        {
        //            salesMain.TRANS_TYPE = "2";
        //        }
        //        else
        //        {
        //            salesMain.TRANS_TYPE = "3";
        //        }

        //        salesMain.LOC_ID = "01"; //vm.Location;
        //        //salesMain.Warehouse = vm.Warehouse;
        //        salesMain.Time_Slot = vm.Time;

        //        //if(vm.Time==)
        //        salesMain.TILL_NO = "";
        //        salesMain.sale_type = vm.RequestPage;
        //        salesMain.BY_CARD = Convert.ToDecimal(Constants.Constants.PaymentType.Card);
        //        salesMain.BY_CASH = Convert.ToDecimal(Constants.Constants.PaymentType.Cash);
        //        salesMain.party_code = vm.SuplCode;
        //        salesMain.advance = Convert.ToInt32(vm.Advance);
        //        if (vm.Phone != null)
        //        {
        //            salesMain.mob = vm.Phone.Trim();
        //        }
        //        salesMain.CANCEL = "";
        //        salesMain.TRANS_DATE = vm.DocDate + vm.Time;
        //        salesMain.TYPE = "A";
        //        salesMain.BY_CASH = Convert.ToDecimal(vm.Payment);
        //        salesMain.USER_ID = vm.Userid ?? "";
        //        if (vm.ReturnAmount == null)
        //        {
        //            salesMain.ret_Amt = 0;//Convert.ToInt32(vm.ReturnAmount);
        //        }
        //        else
        //        {
        //            salesMain.ret_Amt = Convert.ToDecimal(vm.ReturnAmount);
        //        }
        //        updatecustomer(vm.SuplCode, vm.Phone);

        //        if (saveOrUpdateFlag)
        //        {
        //            db.TRANS_MN.Add(salesMain);
        //            if (Convert.ToInt32(vm.Advance) > 0 && vm.Advance != null)
        //            {
        //                Saveadvanceoverride(vm, salesMain.TRANS_NO);
        //            }
        //        }

        //        return salesMain.TRANS_NO;
        //    }
        //    catch (System.Exception ex)
        //    {

        //        return "";
        //    }
        //}
        //[NonAction]
        //public bool SaveSalesDetail(List<DocumentDetailViewModel> vm, string docno, string docType)
        //{
        //    try
        //    {

        //        docType = docType == Constants.Constants.SalesDocument
        //            ? Constants.Constants.TransType.Sales
        //            : Constants.Constants.TransType.SalesReturn;
        //        if (saveOrUpdateFlag == false)
        //        {
        //            priceoverrid(vm, docno);

        //        }
        //        var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();
        //        var tdinDbCount = salesDetailDbList.Count;
        //        if (tdinDbCount > 0)
        //        {
        //            foreach (var m in salesDetailDbList)
        //            {
        //                bool f = true;
        //                var dt = db.TRANS_DT.Where(x => x.TRANS_NO == m.TRANS_NO && x.BARCODE == m.BARCODE).FirstOrDefault();
        //                foreach (var n in vm)
        //                {
        //                    if (m.BARCODE == n.Barcode)
        //                    {
        //                        f = false;
        //                    }
        //                }
        //                if (f == true)
        //                {
        //                    dt.VOID = "T";
        //                }
        //            }
        //            MyConnection conn = new MyConnection();
        //            foreach (var item in vm)
        //            {
        //                var salesDetail = db.TRANS_DT.Where(x => x.TRANS_NO == docno && x.BARCODE == item.Barcode).FirstOrDefault();
        //                bool flag = false;
        //                if (salesDetail != null)
        //                {
        //                    flag = true;
        //                }
        //                else
        //                {
        //                    salesDetail = new TRANS_DT();
        //                    flag = false;
        //                }
        //                salesDetail.TRANS_NO = docno;
        //                salesDetail.BARCODE = item.Barcode;
        //                salesDetail.UNIT_COST = item.Cost;
        //                salesDetail.UNIT_RETAIL = item.Retail;
        //                salesDetail.SCAN_TIME = DateTime.Now;
        //                salesDetail.UAN_NO = item.Uanno;
        //                var totalamount = item.Retail * item.Qty;
        //                salesDetail.dis_amount = (item.Discount / totalamount) * 100;
        //                //salesDetail.dis_amount = item.Discount;
        //                salesDetail.AMOUNT = item.Amount;
        //                salesDetail.UNITS_SOLD = item.Qty;
        //                salesDetail.VOID = "";
        //                salesDetail.EXCH_FLAG = "T";
        //                salesDetail.GST_AMOUNT = 0;
        //                salesDetail.supl_code = item.staffcode;
        //                salesDetail.Colour = item.Colour;
        //                salesDetail.s_time = item.DateTimeSlot;
        //                salesDetail.Warehouse = item.Warehouse;
        //                if (facebookstatus == "" && ProductSales != "PIN")
        //                {
        //                    salesDetail.TRANS_TYPE = (docType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn; ;
        //                }
        //                else if (ProductSales == "PIN")
        //                {
        //                    salesDetail.TRANS_TYPE = "4";
        //                }
        //                else
        //                {
        //                    salesDetail.TRANS_TYPE = "3";
        //                }
        //                if (flag == false)
        //                {
        //                    salesDetail.VOID = "F";
        //                    db.TRANS_DT.Add(salesDetail);
        //                }
        //                else
        //                {
        //                    salesDetail.VOID = "F";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            foreach (var item in vm)
        //            {
        //                var salesDetail = new TRANS_DT();
        //                salesDetail.TRANS_NO = docno;
        //                salesDetail.BARCODE = item.Barcode;
        //                salesDetail.UNIT_COST = item.Cost;
        //                salesDetail.UNIT_RETAIL = item.Retail;
        //                salesDetail.SCAN_TIME = DateTime.Now;
        //                salesDetail.UAN_NO = item.Uanno;
        //                //salesDetail.dis_amount = item.Discount;
        //                salesDetail.AMOUNT = item.Amount;
        //                salesDetail.UNITS_SOLD = item.Qty;
        //                salesDetail.supl_code = item.staffcode;
        //                salesDetail.VOID = "F";
        //                var totalamount = item.Retail * item.Qty;
        //                salesDetail.dis_amount = (item.Discount / totalamount) * 100;
        //                salesDetail.EXCH_FLAG = "T";
        //                salesDetail.GST_AMOUNT = 0;
        //                salesDetail.Colour = item.Colour;
        //                salesDetail.s_time = item.DateTimeSlot;
        //                salesDetail.Warehouse = item.Warehouse;
        //                if (ProductSales != "PIN")
        //                {
        //                    salesDetail.TRANS_TYPE = (docType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn; ;
        //                }
        //                else
        //                {
        //                    salesDetail.TRANS_TYPE = "4";
        //                }
        //                db.TRANS_DT.Add(salesDetail);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (System.Exception)
        //    {
        //        var salesDetailDbList = db.TRANS_DT.Where(u => u.TRANS_NO == docno).ToList();
        //        var tdinDbCount = salesDetailDbList.Count;
        //        if (tdinDbCount > 0)
        //        {
        //            // db.TRANS_DT.RemoveRange(salesDetailDbList);}
        //        }
        //        return false;
        //    }
        //}
    }

}





