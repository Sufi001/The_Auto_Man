using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class AdjustmentController : ApiController
    {
        readonly DataContext db;
        public AdjustmentController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult CreateAdjustment(DocumentViewModel vm)
            {
            try
            {
                if (!ModelState.IsValid || vm.DocumentDetailList.Count == 0)
                {
                    return BadRequest();
                }
                var docno = SaveAdjustmentMain(vm.DocumentMain);
                if (docno != "" && docno != Constants.DocumentStatus.UnauthorizedDocument)
                {
                    SaveAdjustmentDetail(vm.DocumentDetailList, docno, vm.DocumentMain.DocType);
                    SaveSupplier(vm.DocumentMain.SuplCode, vm.DocumentDetailList, vm.DocumentMain.Userid);
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
        [System.Web.Http.NonAction]
        public string SaveAdjustmentMain(DocumentMainViewModel vm)
        {
            try
            {
                var AdjustmentMain = new STOCK_MAIN();
                bool saveOrUpdateFlag = false;
                if (string.IsNullOrEmpty(vm.DocNo))
                {
                    string prefix = vm.DocType + int.Parse(vm.Location);
                    AdjustmentMain.STOCK_NO = vm.GetAdjustmentCode(prefix, 4);
                    saveOrUpdateFlag = true;
                }
                else
                {
                    AdjustmentMain = db.STOCK_MAIN.SingleOrDefault(u => u.STOCK_NO == vm.DocNo && u.STATUS == Constants.DocumentStatus.UnauthorizedDocument);
                    if (AdjustmentMain != null)
                    {
                        //AdjustmentMain.UPDATED_BY = CommonFunctions.GetUserName();
                        //AdjustmentMain.UPDATE_DATE = DateTime.Now;
                    }
                    else
                        return Constants.DocumentStatus.UnauthorizedDocument;

                }

                AdjustmentMain.UPDATED_BY = CommonFunctions.GetUserName();
                AdjustmentMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                AdjustmentMain.STOCK_DATE = vm.DocDate;
                AdjustmentMain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                AdjustmentMain.TRANSFER_STATUS = "0";
                AdjustmentMain.DOC_TYPE = vm.DocType;
                AdjustmentMain.LOC_ID = vm.Location;
                AdjustmentMain.AMOUNT = Convert.ToDecimal(vm.TotalAmount);
                AdjustmentMain.DIS_AMT = 0;
                AdjustmentMain.SUPL_CODE = vm.SuplCode;
                if (saveOrUpdateFlag)
                {
                    AdjustmentMain.INSERTED_BY = CommonFunctions.GetUserName();
                    AdjustmentMain.DOC = DateTime.Now;
                    db.STOCK_MAIN.Add(AdjustmentMain);
                }
                return AdjustmentMain.STOCK_NO;
            }
            catch (System.Exception)
            {

                return "";
            }
        }
        [System.Web.Http.NonAction]
        public bool SaveAdjustmentDetail(List<DocumentDetailViewModel> vm, string docno, string docType)
        {
            try
            {
                var AdjustmentDetailDbList = db.STOCK_DETAIL.Where(u => u.STOCK_NO == docno).ToList();
                var tdinDbCount = AdjustmentDetailDbList.Count;
                if (tdinDbCount > 0)
                {
                    db.STOCK_DETAIL.RemoveRange(AdjustmentDetailDbList);
                }
                foreach (var item in vm)
                {
                    var product = db.PRODUCTS.Where(x => x.BARCODE == item.Barcode).SingleOrDefault();
                    product.UNIT_COST = item.Cost;
                    product.UNIT_RETAIL = item.Retail;
                    db.SaveChanges();

                    var AdjustmentDetail = new STOCK_DETAIL();
                    AdjustmentDetail.STOCK_NO = docno;
                    AdjustmentDetail.DISCOUNT = 0;
                    AdjustmentDetail.TRANSFER_STATUS = "0";
                    AdjustmentDetail.BARCODE = item.Barcode;
                    AdjustmentDetail.COST = item.Cost;
                    AdjustmentDetail.DIS_AMT = item.Discount;
                    AdjustmentDetail.QTY = item.Qty;
                    AdjustmentDetail.FREE_QTY = item.FreeQty;
                    AdjustmentDetail.DOC_TYPE = docType;
                    //grnDetail.COLOUR = item.Colour;
                    //grnDetail.WAREHOUSE = item.Warehouse;
                    db.STOCK_DETAIL.Add(AdjustmentDetail);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
        [System.Web.Http.NonAction]
        public void SaveSupplier(string supplierCode, List<DocumentDetailViewModel> documentDetailList, string username)
        {
            foreach (var prod in documentDetailList)
            {
                var barCode = prod.Barcode;
            
                db.Database.ExecuteSqlCommand("UPDATE SUPPLIER_PRODUCTS SET [STATUS] = '0', TRANSFER_STATUS = '0' WHERE BARCODE ={0}", barCode);
                db.SaveChanges();

                var supplierExist = db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == barCode && x.SUPL_CODE == supplierCode).SingleOrDefault();
                if (supplierExist != null)
                { 
                    supplierExist.STATUS = "1";
                    supplierExist.UPDATED_BY = username;
                    supplierExist.UPDATE_DATE = DateTime.Now.Date;
                    supplierExist.TRANSFER_STATUS = "0";
                }
                else
                {
                    SUPPLIER_PRODUCTS SupplierProduct = new SUPPLIER_PRODUCTS();
                    SupplierProduct.ACTIVE_DATE = DateTime.Now.Date;
                    SupplierProduct.BARCODE = barCode;
                    SupplierProduct.DOC = DateTime.Now.Date;
                    SupplierProduct.INSERTED_BY = username;
                    SupplierProduct.TRANSFER_STATUS = "0";
                    SupplierProduct.STATUS = "1";
                    SupplierProduct.SUPL_CODE = supplierCode;
                    SupplierProduct.TRANSFER_STATUS = null;
                    db.SUPPLIER_PRODUCTS.Add(SupplierProduct);

                }
                db.SaveChanges();

            }

            //var supplierList = db.SUPPLIER_PRODUCTS.Where(u => u.SUPL_CODE == supplierCode).ToList();
            //if (supplierList == null)
            //{
            //    foreach (var item in documentDetailList)
            //    {
            //        var supplier = supplierList.SingleOrDefault(u => u.BAR_CODE == item.Barcode);
            //        if (supplier == null)
            //        {
            //            SUPPLIER_PRODUCTS obj = new SUPPLIER_PRODUCTS();
            //            obj.BAR_CODE = item.Barcode;
            //            obj.SUPL_CODE = supplierCode;
            //            obj.STATUS = Constants.Constants.ActiveSupplier;
            //            obj.ACTIVE_DATE = DateTime.Now;
            //            obj.DOC = DateTime.Now;
            //            obj.INSERTED_BY = username;
            //            db.SUPPLIER_PRODUCTS.Add(obj);
            //        }

            //    }
            //    supplierList.Select(u => u.STATUS = Constants.Constants.InActiveSupplier);
            //}

        }
    }
}
