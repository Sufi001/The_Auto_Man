using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class PurchaseController : ApiController
    {
        readonly DataContext db;
        public PurchaseController()
        {
            db = new DataContext();
        }
        [System.Web.Mvc.HttpPost]
        public IHttpActionResult CreatePurchase(DocumentViewModel vm)
            {
            try
            {
                if (!ModelState.IsValid || vm.DocumentDetailList.Count == 0)
                {
                    return BadRequest();
                }
                var docno = SavePurchaseMain(vm.DocumentMain);
                if (docno != "" && docno != Constants.DocumentStatus.UnauthorizedDocument)
                {
                    SavePurchaseDetail(vm.DocumentDetailList, docno, vm.DocumentMain.DocType);
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
        public string SavePurchaseMain(DocumentMainViewModel vm)
        {
            try
            {
                var purchaseMain = new GRN_MAIN();
                bool saveOrUpdateFlag = false;
                if (string.IsNullOrEmpty(vm.DocNo))
                {
                    string prefix = vm.DocType + int.Parse(vm.Location);
                    purchaseMain.GRN_NO = vm.GetPurchaseCode(prefix, 4);
                    saveOrUpdateFlag = true;
                }
                else
                {
                    purchaseMain = db.GRN_MAIN.SingleOrDefault(u => u.GRN_NO == vm.DocNo && u.STATUS == Constants.DocumentStatus.UnauthorizedDocument);
                    if (purchaseMain != null)
                    {
                        //purchaseMain.UPDATED_BY = CommonFunctions.GetUserName();
                        //purchaseMain.UPDATE_DATE = DateTime.Now;
                    }
                    else
                        return Constants.DocumentStatus.UnauthorizedDocument;

                }

                purchaseMain.UPDATED_BY = CommonFunctions.GetUserName();
                purchaseMain.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                purchaseMain.GRN_DATE = vm.DocDate;
                purchaseMain.STATUS = Constants.DocumentStatus.UnauthorizedDocument;
                purchaseMain.TRANSFER_STATUS = "0";
                purchaseMain.DOC_TYPE = vm.DocType;
                purchaseMain.LOC_ID = vm.Location;
                purchaseMain.AMOUNT = Convert.ToDecimal(vm.TotalAmount);
                purchaseMain.DIS_AMT = Convert.ToDecimal(vm.DiscointAmount);
                purchaseMain.SUPL_CODE = vm.SuplCode;
                if (saveOrUpdateFlag)
                {
                    purchaseMain.INSERTED_BY = CommonFunctions.GetUserName();
                    purchaseMain.DOC = DateTime.Now;
                    db.GRN_MAIN.Add(purchaseMain);
                }
                return purchaseMain.GRN_NO;
            }
            catch (System.Exception)
            {

                return "";
            }
        }
        [System.Web.Http.NonAction]
        public bool SavePurchaseDetail(List<DocumentDetailViewModel> vm, string docno, string docType)
        {
            try
            {
                var grnDetailDbList = db.GRN_DETAIL.Where(u => u.GRN_NO == docno).ToList();
                var tdinDbCount = grnDetailDbList.Count;
                if (tdinDbCount > 0)
                {
                    db.GRN_DETAIL.RemoveRange(grnDetailDbList);
                }
                foreach (var item in vm)
                {
                    var product = db.PRODUCTS.Where(x => x.BARCODE == item.Barcode).SingleOrDefault();
                    product.UNIT_COST = item.Cost;
                    product.UNIT_RETAIL = item.Retail;
                    db.SaveChanges();

                    var grnDetail = new GRN_DETAIL();
                    grnDetail.GRN_NO = docno;
                    grnDetail.DISCOUNT = 0;
                    grnDetail.TRANSFER_STATUS = "0";
                    grnDetail.BARCODE = item.Barcode;
                    grnDetail.COST = item.Cost;
                    grnDetail.DIS_AMT = item.Discount;
                    grnDetail.QTY = item.Qty;
                    grnDetail.FREE_QTY = item.FreeQty;
                    grnDetail.DOC_TYPE = docType;
                    //grnDetail.COLOUR = item.Colour;
                    //grnDetail.WAREHOUSE = item.Warehouse;
                    db.GRN_DETAIL.Add(grnDetail);
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
