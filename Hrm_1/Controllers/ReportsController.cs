using CustomerRec;
using Inventory.ViewModels;
using Inventory.ViewModels.ReportsViewModels;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Inventory.ViewModels.Reservation;
using Inventory.Helper;
using Inventory.Filters;
using Inventory.ViewModels.Order;
using System.Drawing;
using System.Data.SqlClient;
using Inventory.ViewModels.Item;
using Inventory.Models;

namespace Inventory.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        DataContext db;
        public ReportsController()
        {
            db = new DataContext();
        }
        #region Product
        [Permission("Product Report")]
        public ActionResult Product()
        {
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x=>x.PARTY_TYPE == "s").Select(x=> new {Code = x.SUPL_CODE,Name = x.SUPL_NAME }).ToList();
            ViewBag.Brands = db.BRANDs.Select(x => new BrandViewModel { Id = x.ID, Name = x.NAME }).ToList();
            return View();
        }
        //Product List By Department, Group, SubGroup
        public ActionResult ProductList(string Dept_Code, string Group_Code, string SubGroup_Code, string Brand)
        {
            try
            {
                var Products = new List<PRODUCT>();

                if (Dept_Code == "" && Group_Code == "" && SubGroup_Code == "")
                    Products = db.PRODUCTS.ToList();
                else if (Dept_Code != "" && Group_Code == "" && SubGroup_Code == "")
                    Products = db.PRODUCTS.Where(x => x.DEPT_CODE == Dept_Code).ToList();
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code == "")
                    Products = db.PRODUCTS.Where(x => x.DEPT_CODE == Dept_Code && x.GROUP_CODE == Group_Code).ToList();
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code != "")
                    Products = db.PRODUCTS.Where(x => x.DEPT_CODE == Dept_Code && x.GROUP_CODE == Group_Code && x.SRGOUP_CODE == SubGroup_Code).ToList();


                if (!string.IsNullOrEmpty(Brand))
                    Products = Products.Where(x => x.BRAND_ID == Brand).ToList();

                var ItemList = (
                   from product in Products
                   from brand in db.BRANDs.Where(x => x.ID == product.BRAND_ID).DefaultIfEmpty()
                   from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                   from Group in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                   from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                   join pb in db.PROD_BALANCE on product.BARCODE equals pb.BARCODE into pbSum
                   join plb in db.PROD_LOC_REPORT on product.BARCODE equals plb.BARCODE into plbSum

                   select new
                   {
                       BARCODE = product.BARCODE,
                       DESCRIPTION = product.DESCRIPTION,
                       UNIT_SIZE = product.UNIT_SIZE,
                       UNIT_COST = product.UNIT_COST,
                       UNIT_RETAIL = product.UNIT_RETAIL,
                       DEPT_NAME = department != null ? department.DEPT_NAME : "" ,
                       GROUP_NAME = Group != null ? Group.GROUP_NAME : "" ,
                       SGROUP_NAME = subGroup != null ? subGroup.SGROUP_NAME : "",
                       REORDER_LVL = product.REORDER_LVL,
                       EXPIRY_DATE = product.EXPIRY_DATE,
                       location = product.LOCATION,
                       CURRENT_QTY = (pbSum.Sum(x => x.CURRENT_QTY) ?? 0) + (plbSum.Sum(x => x.CURRENT_QTY) ?? 0),
                       Brand = brand != null ? brand.NAME : ""
                   }).ToList();

                //return Json(ItemList, JsonRequestBehavior.AllowGet);

                //Report Path
                string path = @"RPT\Product\ProductList.rdlc";

                //Report Parameters

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ItemList);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult ProductBySupplier(string supplier)
        {
            try
            {
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                if (string.IsNullOrEmpty(supplier))
                {
                    var ItemList = (
                   from suppProduct in db.SUPPLIER_PRODUCTS.Where(x=>x.STATUS == "1")
                   from supp in db.SUPPLIERs.Where(x=>x.SUPL_CODE == suppProduct.SUPL_CODE)
                   from product in db.PRODUCTS.Where(x=>x.BARCODE == suppProduct.BARCODE)
                   from brand in db.BRANDs.Where(x => x.ID == product.BRAND_ID).DefaultIfEmpty()
                   join pb in db.PROD_BALANCE on suppProduct.BARCODE equals pb.BARCODE into pbSum
                   join plb in db.PROD_LOC_REPORT on suppProduct.BARCODE equals plb.BARCODE into plbSum
                   select new
                   {
                       SUPL_NAME = supp.SUPL_NAME,
                       BARCODE = product.BARCODE,
                       DESCRIPTION = product.DESCRIPTION,
                       UNIT_SIZE = product.UNIT_SIZE,
                       UNIT_COST = product.UNIT_COST,
                       UNIT_RETAIL = product.UNIT_RETAIL,
                       REORDER_LVL = product.REORDER_LVL,
                       EXPIRY_DATE = product.EXPIRY_DATE,
                       location = product.LOCATION,
                       CURRENT_QTY = (pbSum.Sum(x=>x.CURRENT_QTY) ?? 0) + (plbSum.Sum(x => x.CURRENT_QTY) ?? 0),
                       Brand = brand != null ? brand.NAME : ""
                   }).ToList();
                    datasource.Add("DataSet1", ItemList);
                }
                else
                {
                    var ItemList = (
                    from suppProduct in db.SUPPLIER_PRODUCTS.Where(x => x.STATUS == "1" && x.SUPL_CODE == supplier)
                    from supp in db.SUPPLIERs.Where(x => x.SUPL_CODE == suppProduct.SUPL_CODE)
                    from product in db.PRODUCTS.Where(x => x.BARCODE == suppProduct.BARCODE)
                    from brand in db.BRANDs.Where(x => x.ID == product.BRAND_ID).DefaultIfEmpty()
                    join pb in db.PROD_BALANCE on suppProduct.BARCODE equals pb.BARCODE into pbSum
                    join plb in db.PROD_LOC_REPORT on suppProduct.BARCODE equals plb.BARCODE into plbSum
                    select new
                    {
                        SUPL_NAME = supp.SUPL_NAME,
                        BARCODE = product.BARCODE,
                        DESCRIPTION = product.DESCRIPTION,
                        UNIT_SIZE = product.UNIT_SIZE,
                        UNIT_COST = product.UNIT_COST,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        REORDER_LVL = product.REORDER_LVL,
                        EXPIRY_DATE = product.EXPIRY_DATE,
                        location = product.LOCATION,
                        CURRENT_QTY = (pbSum.Sum(x => x.CURRENT_QTY) ?? 0) + (plbSum.Sum(x => x.CURRENT_QTY) ?? 0),
                        Brand = brand != null ? brand.NAME : ""
                    }).ToList();
                    datasource.Add("DataSet1", ItemList);
                }
               
                //Report Path
                string path = @"RPT\Product\ProductBySupplier.rdlc";

                //Report Parameters

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                Parameter.Add("UserName", CommonFunctions.GetUserName());


                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        #endregion Product

        #region Purchase
        [Permission("Purchase Report")]
        public ActionResult Purchase()
        {
            //PRSupplier
            //PRDepartment
            //PRAllDocument
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            return View();
        }
        public ActionResult PurchaseReportByDGS(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Purchase\PurchaseReportDGS.rdlc";
                var data = (
                        from grnMain in db.GRN_MAIN.Where(x => x.STATUS == "3" && (x.GRN_DATE >= DateFrom && x.GRN_DATE <= DateTo)).ToList()
                        from grnDetail in db.GRN_DETAIL.Where(x => x.GRN_NO == grnMain.GRN_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == grnDetail.BARCODE).DefaultIfEmpty()
                        from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        join grnD in db.GRN_DETAIL on grnMain.GRN_NO equals grnD.GRN_NO into g
                        select new PurchaseReportDetailViewModel
                        {
                            DepartmentName = department.DEPT_NAME,
                            DepartmentCode = product.DEPT_CODE,
                            GroupCode = product.GROUP_CODE,
                            SGroupCode = product.SRGOUP_CODE,
                            //groupp.GROUP_NAME,
                            //subGroup.SGROUP_NAME,
                            Barcode = product.BARCODE,
                            Description = product.DESCRIPTION,
                            UnitSize = product.CTN_PCS,
                            //product.REORDER_LVL,
                            //product.EXPIRY_DATE,
                            Cost = g.Sum(x => (x.COST * x.QTY)),
                            Amount = grnDetail.COST * grnDetail.QTY,
                            DiscAmt = grnDetail.DIS_AMT,
                            Qty = grnDetail.QTY,
                            FreeQty = grnDetail.FREE_QTY ?? 0,
                            FreeQtyAmt = (grnDetail.FREE_QTY * grnDetail.COST) ?? 0,
                            GstAmt = grnDetail.GST_AMOUNT,
                            NetAmt = (grnDetail.COST * grnDetail.QTY - grnDetail.DIS_AMT)
                            //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.DIS_AMT))
                        }).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("TodayDate", TodayDate);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                var ReportData = new List<PurchaseReportDetailViewModel>();


                if (Dept_Code == "" && Group_Code == "" && SubGroup_Code == "")
                {
                    ReportData = data;
                    Parameter.Add("RoportDepartment", "All Department");
                }
                else if (Dept_Code != "" && Group_Code == "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DepartmentCode == Dept_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                    Parameter.Add("RoportDepartment", "Single Department");
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DepartmentCode == Dept_Code && productsByDept.GroupCode == Group_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                    Parameter.Add("RoportDepartment", "Department and Group");
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code != "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DepartmentCode == Dept_Code && productsByDept.GroupCode == Group_Code && productsByDept.SGroupCode == SubGroup_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                    Parameter.Add("RoportDepartment", "Department, group and SubGroup");
                }

                //var data = PurchaseReportByDGSReportDataSet();

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult PurchaseReportBySupplier(string Supl_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Purchase\PurchaseReportSupplierWise.rdlc";
                var ReportData = new List<PurchaseReportDetailViewModel>();
                var data = (
                       from grnMain in db.GRN_MAIN.Where(x => x.STATUS == "3" && (x.GRN_DATE >= DateFrom && x.GRN_DATE <= DateTo)).ToList()
                       from grnDetail in db.GRN_DETAIL.Where(x => x.GRN_NO == grnMain.GRN_NO).DefaultIfEmpty()
                       from product in db.PRODUCTS.Where(x => x.BARCODE == grnDetail.BARCODE).DefaultIfEmpty()
                       from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == grnMain.SUPL_CODE).DefaultIfEmpty()
                       join grnD in db.GRN_DETAIL on grnMain.GRN_NO equals grnD.GRN_NO into g
                       select new PurchaseReportDetailViewModel
                       {
                           SupplierName = supplier.SUPL_NAME,
                           SupplierCode = grnMain.SUPL_CODE,
                           DepartmentCode = product.DEPT_CODE,
                           GroupCode = product.GROUP_CODE,
                           SGroupCode = product.SRGOUP_CODE,
                           grnDate = grnMain.GRN_DATE,
                           Barcode = product.BARCODE,
                           Description = product.DESCRIPTION,
                           UnitSize = product.CTN_PCS,
                           //product.REORDER_LVL,
                           //product.EXPIRY_DATE,
                           Cost = (grnDetail.COST * (grnDetail.QTY ?? 0)),
                           //Cost = g.Sum(x => (x.COST * x.QTY)),
                           Amount = (grnDetail.COST * (grnDetail.QTY ?? 0)),
                           //Amount = g.Sum(x => (x.COST * x.QTY)),
                           DiscAmt = grnDetail.DIS_AMT,
                           //DiscAmt = g.Sum(x => x.DIS_AMT),
                           Qty = grnDetail.QTY,
                           //Qty = g.Sum(x => x.QTY),
                           FreeQty = grnDetail.FREE_QTY ?? 0,
                           FreeQtyAmt = ((grnDetail.FREE_QTY ?? 0) * grnDetail.COST),
                           //FreeQtyAmt = g.Sum(x => (x.FREE_QTY ?? 0 * x.COST)),
                           GstAmt = grnDetail.GST_AMOUNT,
                           NetAmt = ((grnDetail.COST * (grnDetail.QTY ?? 0)) - (grnDetail.DIS_AMT ?? 0))
                           //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.DIS_AMT))
                       }).ToList();

                if (Supl_Code != "")
                {
                    foreach (var PurchaseBySupplier in data)
                    {
                        if (PurchaseBySupplier.SupplierCode == Supl_Code)
                        {
                            ReportData.Add(PurchaseBySupplier);
                        }
                    }
                }
                else
                    ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("TodayDate", TodayDate);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Supl_Code == "" || Supl_Code == null)
                    Parameter.Add("ReportSupplier", "All Supplier");
                else
                    Parameter.Add("ReportSupplier", "Single Supplier");

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult PurchseReportByDocument(string Doc, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Purchase\PurchaseReportDocument.rdlc";
                var data = (
                        from grnMain in db.GRN_MAIN.Where(x => x.STATUS == "3" && (x.GRN_DATE >= DateFrom && x.GRN_DATE <= DateTo)).ToList()
                        from grnDetail in db.GRN_DETAIL.Where(x => x.GRN_NO == grnMain.GRN_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == grnDetail.BARCODE).DefaultIfEmpty()
                        from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == grnMain.SUPL_CODE).DefaultIfEmpty()
                        join grnD in db.GRN_DETAIL on grnMain.GRN_NO equals grnD.GRN_NO into g
                        select new PurchaseReportDetailViewModel
                        {
                            SupplierName = supplier.SUPL_NAME,
                            DepartmentCode = product.DEPT_CODE,
                            GroupCode = product.GROUP_CODE,
                            SGroupCode = product.SRGOUP_CODE,
                            Grn_Date = grnMain.GRN_DATE.ToString("dd-MM-yyyy"),
                            Grn_No = grnMain.GRN_NO,
                            Barcode = product.BARCODE,
                            Description = product.DESCRIPTION,
                            UnitSize = product.CTN_PCS,
                            //product.REORDER_LVL,
                            //product.EXPIRY_DATE,
                            Cost = g.Sum(x => (x.COST * x.QTY)),
                            Amount = grnDetail.COST * grnDetail.QTY,
                            DiscAmt = grnDetail.DIS_AMT,
                            Qty = grnDetail.QTY,
                            FreeQty = grnDetail.FREE_QTY ?? 0,
                            FreeQtyAmt = (grnDetail.FREE_QTY * grnDetail.COST) ?? 0,
                            GstAmt = grnDetail.GST_AMOUNT,
                            NetAmt = (grnDetail.COST * grnDetail.QTY - grnDetail.DIS_AMT)
                            //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.DIS_AMT))
                        }).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("TodayDate", TodayDate);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Doc == "" || Doc == null)
                    Parameter.Add("ReportSupplier", "All Document");
                else
                    Parameter.Add("ReportSupplier", "Single Document");

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                var ReportData = new List<PurchaseReportDetailViewModel>();
                ReportData = data;

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult GoodReceivedNote(string Doc_Number)
        {
            try
            {
                string path = @"RPT\Purchase\PurchaseReportByDocumentNumber.rdlc";
                var stockOutDetail = (from PurchaseMain in db.GRN_MAIN
                                      join purchaseDetail in db.GRN_DETAIL
                                      on PurchaseMain.GRN_NO equals purchaseDetail.GRN_NO
                                      join supplier in db.SUPPLIERs
                                      on PurchaseMain.SUPL_CODE equals supplier.SUPL_CODE
                                      join product in db.PRODUCTS
                                      on purchaseDetail.BARCODE equals product.BARCODE
                                      where PurchaseMain.GRN_NO == Doc_Number && supplier.PARTY_TYPE == Constants.CustomerSupplier.Supplier
                                      select new
                                      {
                                          ItemCode = purchaseDetail.BARCODE,
                                          ItemName = product.DESCRIPTION,
                                          SupplierName = supplier.SUPL_NAME,
                                          Phone = supplier.MOBILE,
                                          Address = supplier.ADDRESS,
                                          ContactPerson = supplier.CONTACT_NAME,
                                          DocumentNo = PurchaseMain.GRN_NO,
                                          Qty = purchaseDetail.QTY ?? 0,
                                          Rate = purchaseDetail.COST,
                                          UnitRetail = product.UNIT_RETAIL,
                                          Amount = (purchaseDetail.COST * purchaseDetail.QTY),
                                          PartyType = supplier.PARTY_TYPE,
                                          DocumentDate = PurchaseMain.GRN_DATE,
                                          FreeQty = purchaseDetail.FREE_QTY,
                                          UanNo = product.UAN_NO,
                                          CTNSize = product.CTN_PCS ?? 0,
                                          //QtyCTN = Math.Floor(((purchaseDetail.QTY ?? 0.0m ) / product.CTN_PCS ?? 0.0m)).ToString() + "-" + Math.Floor(((((purchaseDetail.QTY ?? 0.0m) / product.CTN_PCS ?? 0.0m) - Math.Floor(((purchaseDetail.QTY ?? 0.0m) / product.CTN_PCS ?? 0.0m)))* purchaseDetail.QTY ?? 0.0m)).ToString(),
                                          QtyCTN = product.CTN_PCS == null || product.CTN_PCS == 0 ? "0-" + Math.Floor((purchaseDetail.QTY ?? 0)).ToString() : Math.Floor(((purchaseDetail.QTY ?? 0) / product.CTN_PCS ?? 0)).ToString() + "-" + Math.Floor(((purchaseDetail.QTY ?? 0) % product.CTN_PCS ?? 0)).ToString(),
                                          Discount = purchaseDetail.DIS_AMT ?? 0
                                      }).ToList();

                DateTime date = stockOutDetail.Select(x => x.DocumentDate).FirstOrDefault();
                string showdate = date.ToString("dd/MM/yyyy");
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("ReportName", "Good Received Note");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Doc_Date", showdate);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", stockOutDetail);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult PrintAllPurchaseDocumentreport(string btn)
        {
            try
            {
                var Date = DateTime.Now.Date;
                string id = "PDF";
                var nextday = Date.AddDays(1);
                LocalReport lr = new LocalReport();
                MyConnection conn = new MyConnection();
                var stockOutDetail = (from salesMain in db.GRN_MAIN
                                      join salesDetail in db.GRN_DETAIL
                                      on salesMain.GRN_NO equals salesDetail.GRN_NO
                                      join supplier in db.SUPPLIERs
                                      on salesMain.SUPL_CODE equals supplier.SUPL_CODE
                                      join product in db.PRODUCTS
                                      on salesDetail.BARCODE equals product.BARCODE
                                      join location in db.LOCATIONs
                                      on salesMain.LOC_ID equals location.LOC_ID
                                      where salesMain.GRN_DATE >= Date && salesMain.GRN_DATE <= nextday && supplier.PARTY_TYPE == Constants.CustomerSupplier.Supplier && salesMain.STATUS == "3"
                                      select new
                                      {
                                          SupplierName = supplier.SUPL_NAME,
                                          SupplierContactNo = supplier.MOBILE,
                                          SupplierAddress = supplier.ADDRESS,
                                          SupplierPhoneNo = supplier.PHONE,
                                          DocumentNo = salesMain.GRN_NO,
                                          ItemCode = salesDetail.BARCODE,
                                          ItemName = product.DESCRIPTION,
                                          Qty = salesDetail.QTY ?? 0,
                                          Rate = salesDetail.COST,
                                          Amount = (salesDetail.COST * salesDetail.QTY) - salesDetail.DIS_AMT ?? 0,
                                          PartyType = supplier.PARTY_TYPE,
                                          Location = location.NAME,
                                          DocumentDate = salesMain.GRN_DATE,
                                          UanNo = product.UAN_NO,
                                          Discount = salesDetail.DIS_AMT ?? 0
                                      }).ToList();
                ReportDataSource rd = new ReportDataSource("DataSet1", stockOutDetail);
                lr.DataSources.Add(rd);
                string path = "";
                ReportParameter[] Par = new ReportParameter[4];
                Par[0] = new ReportParameter("ReportName", "Purchase  Report", false);
                Par[1] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);

                DateTime date = stockOutDetail.Select(x => x.DocumentDate).FirstOrDefault();
                string showdate = date.ToString("dd/MM/yyyy");
                Par[2] = new ReportParameter("UserName", CommonFunctions.GetUserName(), false);
                Par[3] = new ReportParameter("Doc_Date", showdate, false);

                //  path = Path.Combine(Server.MapPath(@" ~\Report\DailyPurchaseReportBySupplier.rdlc"));
                path = Path.Combine(Server.MapPath(@" ~\Report\Purchase Report.rdlc"));
                lr.ReportPath = path;
                lr.SetParameters(Par);
                lr.EnableExternalImages = true;
                string reportType = id;
                lr.Refresh();
                string mimeType;
                string encoding;
                string fileNameExtension;
                string deviceInfo =
                "<DeviceInfo>" +
                "  <OutputFormat>" + id + "</OutputFormat>" +
                //    "  <Orientation>Landscape</Orientation>" +
                //    "  <PageWidth>13in</PageWidth>" +
                "  <MarginTop>0.1in</MarginTop>" +
                "  <MarginRight>0.1in</MarginRight>" +
                "  <MarginLeft>0.1in</MarginLeft>" +
                "  <MarginBottom>0.1in</MarginBottom>" +
                "</DeviceInfo>";
                Warning[] warnings;
                string[] streams;
                byte[] renderedBytes;
                lr.EnableExternalImages = true;
                renderedBytes = lr.Render(
                    reportType,
                    deviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);
                //    lr.Dispose();
                return File(renderedBytes, mimeType);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion Purchase

        #region Stock
        public ActionResult StockAdjustmentReport(string Doc_Number)
        {
            try
            {
                string path = @"RPT\StockAdjustment\StockReportByDocumentNumber.rdlc";
                var stockOutDetail = (from StockMain in db.STOCK_MAIN
                                      join StockDetail in db.STOCK_DETAIL
                                      on StockMain.STOCK_NO equals StockDetail.STOCK_NO
                                      join supplier in db.SUPPLIERs
                                      on StockMain.SUPL_CODE equals supplier.SUPL_CODE
                                      join product in db.PRODUCTS
                                      on StockDetail.BARCODE equals product.BARCODE
                                      where StockMain.STOCK_NO == Doc_Number && supplier.PARTY_TYPE == Constants.CustomerSupplier.Supplier
                                      select new
                                      {
                                          ItemCode = StockDetail.BARCODE,
                                          ItemName = product.DESCRIPTION,
                                          SupplierName = supplier.SUPL_NAME,
                                          Phone = supplier.MOBILE,
                                          Address = supplier.ADDRESS,
                                          ContactPerson = supplier.CONTACT_NAME,
                                          DocumentNo = StockMain.STOCK_NO,
                                          Qty = StockDetail.QTY ?? 0,
                                          Rate = StockDetail.COST,
                                          UnitRetail = product.UNIT_RETAIL,
                                          Amount = (StockDetail.COST * StockDetail.QTY),
                                          PartyType = supplier.PARTY_TYPE,
                                          DocumentDate = StockMain.STOCK_DATE,
                                          FreeQty = StockDetail.FREE_QTY,
                                          UanNo = product.UAN_NO,
                                          CTNSize = product.CTN_PCS ?? 0,
                                          CurrentQty=product.PROD_BALANCE.CURRENT_QTY,
                                          //[FreeQty] = Math.Floor(((purchaseDetail.QTY ?? 0.0m ) / product.CTN_PCS ?? 0.0m)).ToString() + "-" + Math.Floor(((((purchaseDetail.QTY ?? 0.0m) / product.CTN_PCS ?? 0.0m) - Math.Floor(((purchaseDetail.QTY ?? 0.0m) / product.CTN_PCS ?? 0.0m)))* purchaseDetail.QTY ?? 0.0m)).ToString(),
                                          QtyCTN = product.CTN_PCS == null || product.CTN_PCS == 0 ? "0-" + Math.Floor((StockDetail.QTY ?? 0)).ToString() : Math.Floor(((StockDetail.QTY ?? 0) / product.CTN_PCS ?? 0)).ToString() + "-" + Math.Floor(((StockDetail.QTY ?? 0) % product.CTN_PCS ?? 0)).ToString(),
                                          Discount = StockDetail.DIS_AMT ?? 0
                                      }).ToList();

                DateTime date = stockOutDetail.Select(x => x.DocumentDate).FirstOrDefault();
                string showdate = date.ToString("dd/MM/yyyy");
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("ReportName", "Stock Adjustment");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Doc_Date", showdate);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", stockOutDetail);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region PurchaseReturn
        [Permission("Purchase Return Report")]
        public ActionResult PurchaseReturn()
        {
            //PurchaseReturnBySupplier
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            return View();
        }
        public ActionResult PurchaseReturnReportByDGS(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\PurchaseReturn\PurchaseReturnReportByDGS.rdlc";
                var data = (
                        from main in db.GRFS_MAIN.Where(x => x.STATUS == "3" && (x.GRF_DATE >= DateFrom && x.GRF_DATE <= DateTo))
                        from detail in db.GRFS_DETAIL.Where(x => x.GRF_NO == main.GRF_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                        from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        join grnD in db.GRFS_DETAIL on main.GRF_NO equals grnD.GRF_NO into g
                        select new PurchaseReturnReportViewModel
                        {
                            DEPT_CODE = department.DEPT_CODE,
                            GROUP_CODE = groupp.GROUP_CODE,
                            SGROUP_CODE = subGroup.SGROUP_CODE,
                            DEPT_NAME = department.DEPT_NAME,
                            GROUP_NAME = groupp.GROUP_NAME,
                            SGROUP_NAME = subGroup.SGROUP_NAME,
                            BARCODE = product.BARCODE,
                            DESCRIPTION = product.DESCRIPTION,
                            UNIT_SIZE = product.CTN_PCS.ToString(),
                            REORDER_LVL = product.REORDER_LVL,
                            EXPIRY_DATE = product.EXPIRY_DATE,
                            COST = detail.COST,
                            //COST = g.Sum(x => (x.COST * x.QTY)),
                            //Amount = g.Sum(x => (x.COST * x.QTY)),
                            DISC = detail.DISCOUNT,
                            //DISC = g.Sum(x => x.DISCOUNT),
                            QTY = detail.QTY,
                            //QTY = g.Sum(x => x.QTY),
                            //FreeQty = grnDetail.FREE_QTY ?? 0,
                            //FreeQtyAmt = g.Sum(x => (x.FREE_QTY ?? 0 * x.COST)),
                            //GstAmt = grnDetail.GST_AMOUNT,
                            //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.dis_amt))
                        }).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                var ReportData = new List<PurchaseReturnReportViewModel>();
                if (Dept_Code == "" && Group_Code == "" && SubGroup_Code == "")
                {
                    ReportData = data;
                    Parameter.Add("btn", "Purchase Return");
                }
                else if (Dept_Code != "" && Group_Code == "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code)
                            ReportData.Add(productsByDept);
                    }
                    Parameter.Add("btn", "PurchaseReturnByDepartment");
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code)
                            ReportData.Add(productsByDept);
                    }
                    Parameter.Add("btn", "PurchaseReturnByGroup");
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code != "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code && productsByDept.SGROUP_CODE == SubGroup_Code)
                            ReportData.Add(productsByDept);
                    }
                    Parameter.Add("btn", "PurchaseReturnBySubGroup");
                }

                //var data = PurchaseReportByDGSReportDataSet();

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult PurchaseReturnReportBySupplier(string Supl_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\PurchaseReturn\PurchaseReturnBySupplier.rdlc";
                var ReportData = new List<PurchaseReturnReportViewModel>();
                var data = (
                        from main in db.GRFS_MAIN.Where(x => x.STATUS == "3" && (x.GRF_DATE >= DateFrom && x.GRF_DATE <= DateTo)).ToList()
                        from detail in db.GRFS_DETAIL.Where(x => x.GRF_NO == main.GRF_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                        from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == main.SUPL_CODE).DefaultIfEmpty()
                            //from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                            //from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE).DefaultIfEmpty()
                            //from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE).DefaultIfEmpty()
                        join grnD in db.GRFS_DETAIL on main.GRF_NO equals grnD.GRF_NO into g
                        select new PurchaseReturnReportViewModel
                        {
                            //DEPT_CODE = department.DEPT_CODE,
                            //GROUP_CODE = groupp.GROUP_CODE,
                            //SGROUP_CODE = subGroup.SGROUP_CODE,
                            //DEPT_NAME = department.DEPT_NAME,
                            //GROUP_NAME = groupp.GROUP_NAME,
                            //SGROUP_NAME = subGroup.SGROUP_NAME,
                            SUPL_NAME = supplier.SUPL_NAME,
                            SUPL_CODE = supplier.SUPL_CODE,
                            BARCODE = product.BARCODE,
                            DESCRIPTION = product.DESCRIPTION,
                            UNIT_SIZE = product.CTN_PCS.ToString(),
                            REORDER_LVL = product.REORDER_LVL,
                            EXPIRY_DATE = product.EXPIRY_DATE,
                            COST = detail.COST,
                            //COST = g.Sum(x => (x.COST * x.QTY)),
                            //Amount = g.Sum(x => (x.COST * x.QTY)),
                            DISC = detail.DISCOUNT,
                            //DISC = g.Sum(x => x.DISCOUNT),
                            QTY = detail.QTY,
                            //QTY = g.Sum(x => x.QTY),
                            //FreeQty = grnDetail.FREE_QTY ?? 0,
                            //FreeQtyAmt = g.Sum(x => (x.FREE_QTY ?? 0 * x.COST)),
                            //GstAmt = grnDetail.GST_AMOUNT,
                            //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.dis_amt))
                        }).ToList();

                if (Supl_Code != "")
                {
                    foreach (var PurchaseReturnBySupplier in data)
                    {
                        if (PurchaseReturnBySupplier.SUPL_CODE == Supl_Code)
                            ReportData.Add(PurchaseReturnBySupplier);
                    }
                }
                else
                    ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Supl_Code == "" || Supl_Code == null)
                    Parameter.Add("SupplierReport", "All Supplier");
                else
                    Parameter.Add("SupplierReport", "Single Supplier");

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult PurchseReturnReportByDocument(string Doc, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\PurchaseReturn\PurchaseReturnReportByDocument.rdlc";
                var ReportData = new List<PurchaseReturnReportViewModel>();
                var data = (
                        from main in db.GRFS_MAIN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument && (x.GRF_DATE >= DateFrom && x.GRF_DATE <= DateTo)).ToList()
                        from detail in db.GRFS_DETAIL.Where(x => x.GRF_NO == main.GRF_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                        from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == main.SUPL_CODE).DefaultIfEmpty()
                            //from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                            //from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE).DefaultIfEmpty()
                            //from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE).DefaultIfEmpty()
                        join grnD in db.GRFS_DETAIL on main.GRF_NO equals grnD.GRF_NO into g
                        select new PurchaseReturnReportViewModel
                        {
                            DOC_NO = main.GRF_NO,
                            //DEPT_CODE = department.DEPT_CODE,
                            //GROUP_CODE = groupp.GROUP_CODE,
                            //SGROUP_CODE = subGroup.SGROUP_CODE,
                            //DEPT_NAME = department.DEPT_NAME,
                            //GROUP_NAME = groupp.GROUP_NAME,
                            //SGROUP_NAME = subGroup.SGROUP_NAME,
                            SUPL_NAME = supplier.SUPL_NAME,
                            SUPL_CODE = supplier.SUPL_CODE,
                            BARCODE = product.BARCODE,
                            DESCRIPTION = product.DESCRIPTION,
                            UNIT_SIZE = product.CTN_PCS.ToString(),
                            REORDER_LVL = product.REORDER_LVL,
                            EXPIRY_DATE = product.EXPIRY_DATE,
                            COST = detail.COST,
                            //COST = g.Sum(x => (x.COST * x.QTY)),
                            //Amount = g.Sum(x => (x.COST * x.QTY)),
                            DISC = detail.DISCOUNT,
                            //DISC = g.Sum(x => x.DISCOUNT),
                            QTY = detail.QTY,
                            //QTY = g.Sum(x => x.QTY),
                            //FreeQty = grnDetail.FREE_QTY ?? 0,
                            //FreeQtyAmt = g.Sum(x => (x.FREE_QTY ?? 0 * x.COST)),
                            //GstAmt = grnDetail.GST_AMOUNT,
                            //NetAmt = (g.Sum(x => (x.COST * x.QTY)) - g.Sum(x => x.dis_amt))
                        }).ToList();
                ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("SupplierReport", "All Document");
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult PrintPurchaseReturnDocumentreport(string Doc_Number)
        {
            try
            {
                string path = @"RPT\PurchaseReturn\PurchaseReturnReportByDocumentNumber.rdlc";
                var model = (from purchaseReturnMain in db.GRFS_MAIN
                             join purchaseReturnDetail in db.GRFS_DETAIL
                             on purchaseReturnMain.GRF_NO equals purchaseReturnDetail.GRF_NO
                             join supplier in db.SUPPLIERs
                             on purchaseReturnMain.SUPL_CODE equals supplier.SUPL_CODE
                             join product in db.PRODUCTS
                             on purchaseReturnDetail.BARCODE equals product.BARCODE
                             join location in db.LOCATIONs
                             on purchaseReturnMain.LOCATION equals location.LOC_ID
                             select new
                             {
                                 SupplierName = supplier.SUPL_NAME,
                                 DocumentDate = purchaseReturnMain.GRF_DATE,
                                 DocumentNo = purchaseReturnMain.GRF_NO,
                                 ItemCode = purchaseReturnDetail.BARCODE,
                                 ItemName = product.DESCRIPTION,
                                 Qty = purchaseReturnDetail.QTY ?? 0,
                                 Rate = purchaseReturnDetail.COST,
                                 Amount = (purchaseReturnDetail.COST * purchaseReturnDetail.QTY) - purchaseReturnDetail.DISCOUNT ?? 0,
                                 PartyType = supplier.PARTY_TYPE,
                                 Location = location.NAME,
                                 UanNo = product.UAN_NO,
                                 Discount = purchaseReturnDetail.DISCOUNT ?? 0
                             }).ToList();
                var stockOutDetail = model.Where(x => x.DocumentNo == Doc_Number && x.PartyType == "s").ToList();

                DateTime date = stockOutDetail.Select(x => x.DocumentDate).FirstOrDefault();
                string showdate = date.ToString("dd/MM/yyyy");
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("ReportName", "Purchase Return Document");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Doc_Date", showdate);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", stockOutDetail);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion PurchaseReturn

        #region Sales
        [Permission("Sales Report")]
        public ActionResult Sales()
        {
            //SalesByDepartment
            //SalesByGroup
            //SalesBySubGroup
            //SalesBySupplier
            //PriceOverrideReport
            //ItemVoidReport
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            return View();
        }
        public ActionResult SalesByDGS(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Visibility, bool ForWeb = false)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\SaleReportByDGS.rdlc";

                List<SalesReportByDGS> data = new List<SalesReportByDGS>();

                if (ForWeb)
                {
                    data = (
                    from main in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                       && x.CANCEL == "F"
                       && x.TRANS_TYPE == Constants.TransType.Sales
                       && x.SALE_TYPE == Constants.SalesPage.WebStore
                       && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                    join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals detail.TRANS_NO
                    join p in db.PRODUCTS on detail.BARCODE equals p.BARCODE
                    join d in db.DEPARTMENTs on p.DEPT_CODE equals d.DEPT_CODE
                    join gr in db.GROUPS on new {p.DEPT_CODE, p.GROUP_CODE } equals new {gr.DEPT_CODE, gr.GROUP_CODE }
                    join sgr in db.SUB_GROUPS on new {p1 = p.DEPT_CODE, p2 = p.GROUP_CODE, p3 = p.SRGOUP_CODE} equals new { p1 = sgr.DEPT_CODE, p2 = sgr.GROUP_CODE, p3 = sgr.SGROUP_CODE}
                    join Trans in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals Trans.TRANS_NO into g
                    select new SalesReportByDGS
                       {
                           DEPT_NAME = d.DEPT_NAME,
                           DEPT_CODE = d.DEPT_CODE,
                           GROUP_NAME = gr.GROUP_NAME,
                           GROUP_CODE = gr.GROUP_CODE,
                           SGROUP_NAME = sgr.SGROUP_NAME,
                           SGROUP_CODE = sgr.SGROUP_CODE,
                           BARCODE = p.BARCODE,
                           DESCRIPTION = p.DESCRIPTION,
                        //units_sold = transDetail.UNITS_SOLD,
                        //cost_amount = transDetail.UNITS_SOLD * (transDetail.UNIT_COST),
                        //retail_amount = transDetail.UNIT_RETAIL,
                        //disc_amount = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL) * (transDetail.DIS_AMOUNT / 100),
                        //net_amount = transMain.CASH_AMT,
                        //net_margin = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL)

                        units_sold = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                        cost_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                        retail_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                        disc_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),
                        net_amount = main.CASH_AMT,
                        net_margin = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNIT_RETAIL * detail.UNITS_SOLD) - (detail.UNIT_COST * detail.UNITS_SOLD) - ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))) : -((detail.UNIT_RETAIL * detail.UNITS_SOLD) - (detail.UNIT_COST * detail.UNITS_SOLD) - ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),

                        //units_sold = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                        //   cost_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                        //   retail_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                        //   disc_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0) ?? 0,
                        //   net_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : 0) ?? 0,
                        //   net_margin = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : 0) - g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : 0)
                    }).ToList();
                }
                else
                {

                    data = (
                   from main in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                      && x.CANCEL == "F"
                      && x.TRANS_TYPE == Constants.TransType.Sales
                      && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                   join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals detail.TRANS_NO
                   join p in db.PRODUCTS on detail.BARCODE equals p.BARCODE
                   join d in db.DEPARTMENTs on p.DEPT_CODE equals d.DEPT_CODE
                   join gr in db.GROUPS on new { p.DEPT_CODE, p.GROUP_CODE } equals new { gr.DEPT_CODE, gr.GROUP_CODE }
                   join sgr in db.SUB_GROUPS on new { p1 = p.DEPT_CODE, p2 = p.GROUP_CODE, p3 = p.SRGOUP_CODE } equals new { p1 = sgr.DEPT_CODE, p2 = sgr.GROUP_CODE, p3 = sgr.SGROUP_CODE }
                   join Trans in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals Trans.TRANS_NO into g
                   select new SalesReportByDGS
                   {
                       DEPT_NAME = d.DEPT_NAME,
                       DEPT_CODE = d.DEPT_CODE,
                       GROUP_NAME = gr.GROUP_NAME,
                       GROUP_CODE = gr.GROUP_CODE,
                       SGROUP_NAME = sgr.SGROUP_NAME,
                       SGROUP_CODE = sgr.SGROUP_CODE,
                       BARCODE = p.BARCODE,
                       DESCRIPTION = p.DESCRIPTION,

                       units_sold = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                       cost_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                       retail_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                       disc_amount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),
                       net_amount = main.CASH_AMT,
                       net_margin = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNIT_RETAIL * detail.UNITS_SOLD) - (detail.UNIT_COST * detail.UNITS_SOLD) - ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))) : -((detail.UNIT_RETAIL * detail.UNITS_SOLD) - (detail.UNIT_COST * detail.UNITS_SOLD) - ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),


                       //units_sold = transDetail.UNITS_SOLD,
                       //cost_amount = transDetail.UNITS_SOLD * (transDetail.UNIT_COST),
                       //retail_amount = transDetail.UNIT_RETAIL,
                       //disc_amount = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL) * (transDetail.DIS_AMOUNT / 100),
                       //net_amount = transMain.CASH_AMT,
                       //net_margin = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL)
                       // units_sold = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                       //cost_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                       //retail_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                       //disc_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0) ?? 0,
                       //net_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : 0) ?? 0,
                       //net_margin = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : 0) - g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : 0)
                   }).ToList();


                    //data = (
                    //        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    //       && x.CANCEL == "F"



                    //        && x.TRANS_TYPE == Constants.TransType.Sales && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                    //        from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                    //        from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                    //        from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    //        from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    //        from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    //        join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                    //        select new SalesReportByDGS
                    //        {
                    //            DEPT_NAME = department.DEPT_NAME,
                    //            DEPT_CODE = department.DEPT_CODE,
                    //            GROUP_NAME = groupp.GROUP_NAME,
                    //            GROUP_CODE = groupp.GROUP_CODE,
                    //            SGROUP_NAME = subGroup.SGROUP_NAME,
                    //            SGROUP_CODE = subGroup.SGROUP_CODE,
                    //            BARCODE = product.BARCODE,
                    //            DESCRIPTION = product.DESCRIPTION,
                    //        //units_sold = transDetail.UNITS_SOLD,
                    //        //cost_amount = transDetail.UNITS_SOLD * (transDetail.UNIT_COST),
                    //        //retail_amount = transDetail.UNIT_RETAIL,
                    //        //disc_amount = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL) * (transDetail.DIS_AMOUNT / 100),
                    //        //net_amount = transMain.CASH_AMT,
                    //        //net_margin = (transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL),
                    //        units_sold = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                    //            cost_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                    //            retail_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                    //            disc_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0) ?? 0,
                    //            net_amount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : 0) ?? 0,
                    //            net_margin = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : 0) - g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : 0)
                    //        }).ToList();
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                var ReportData = new List<SalesReportByDGS>();

                if (Dept_Code == "" && Group_Code == "" && SubGroup_Code == "")
                    ReportData = data;
                else if (Dept_Code != "" && Group_Code == "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code != "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code && productsByDept.SGROUP_CODE == SubGroup_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult SalesBySupplier(string Supl_Code, string FromDate, string ToDate, string Visibility, bool ForWeb = false)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\SaleReportBySupplier.rdlc";
                var ReportData = new List<SalesBySupplierReportModel>();
                List<SalesBySupplierReportModel> data = new List<SalesBySupplierReportModel>();


                if (ForWeb)
                {
                    data = (
                        from main in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.CANCEL == "F"
                    && x.TRANS_TYPE == Constants.TransType.Sales
                    && x.SALE_TYPE == Constants.SalesPage.WebStore
                    && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals detail.TRANS_NO
                        join product in db.PRODUCTS on detail.BARCODE equals product.BARCODE
                        join sp in db.SUPPLIER_PRODUCTS.Where(x => x.STATUS == "1") on new { detail.BARCODE } equals new { sp.BARCODE }
                        join s in db.SUPPLIERs on sp.SUPL_CODE equals s.SUPL_CODE
                        join trans in db.TRANS_DT.Where(x => x.VOID == "F" && x.EXCH_FLAG == "T") on main.TRANS_NO equals trans.TRANS_NO into g
                        select new SalesBySupplierReportModel
                        {
                            SupplierName = s.SUPL_NAME,
                            SUPL_CODE = s.SUPL_CODE,
                            ItemCode = product.BARCODE,
                            ItemName = product.DESCRIPTION,
                            Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                            Unit_Cost = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                            Rate = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                            Discount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),


                            //Qty = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                            //Unit_Cost = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                            //Rate = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                            //Discount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0)

                        }).OrderBy(x => new { x.SUPL_CODE, x.ItemCode }).ToList();

                    // from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    //&& x.CANCEL == "F"
                    //&& x.TRANS_TYPE == Constants.TransType.Sales
                    //&& x.SALE_TYPE == Constants.SalesPage.WebStore && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                    // from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                    // from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                    // from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                    // from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                    // join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                    // select new SalesBySupplierReportModel
                    // {
                    //     SupplierName = supplier.SUPL_NAME,
                    //     SUPL_CODE = supplier.SUPL_CODE,
                    //     ItemCode = product.BARCODE,
                    //     ItemName = product.DESCRIPTION,
                    //     Qty = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                    //     Unit_Cost = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                    //     Rate = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                    //     Discount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0)
                    // }).ToList();
                }
                else
                {
                    data = (
                         from main in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.CANCEL == "F"
                    && x.TRANS_TYPE == Constants.TransType.Sales
                    && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                         join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG=="T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals detail.TRANS_NO
                         join product in db.PRODUCTS on detail.BARCODE equals product.BARCODE
                         join sp in db.SUPPLIER_PRODUCTS.Where(x => x.STATUS == "1") on new { detail.BARCODE } equals new { sp.BARCODE }
                         join s in db.SUPPLIERs on sp.SUPL_CODE equals s.SUPL_CODE
                         join trans in db.TRANS_DT.Where(x => x.VOID == "F" && x.EXCH_FLAG == "T") on main.TRANS_NO equals trans.TRANS_NO into g
                         select new SalesBySupplierReportModel
                         {
                             SupplierName = s.SUPL_NAME,
                             SUPL_CODE = s.SUPL_CODE,
                             ItemCode = product.BARCODE,
                             ItemName = product.DESCRIPTION,
                             Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                             Unit_Cost = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                             Rate = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                             Discount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),

                         }).OrderBy(x=> new { x.SUPL_CODE,x.ItemCode }).ToList();


                    //from transMain in db.TRANS_MN.Where(x => x.STATUS == "3" && x.TRANS_TYPE == Constants.TransType.Sales && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                    //from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                    //from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                    //from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                    //from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                    //join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                    //select new SalesBySupplierReportModel
                    //{
                    //    SupplierName = supplier.SUPL_NAME,
                    //    SUPL_CODE = supplier.SUPL_CODE,
                    //    ItemCode = product.BARCODE,
                    //    ItemName = product.DESCRIPTION,
                    //    Qty = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                    //    Unit_Cost = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                    //    Rate = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                    //    Discount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0)
                    //}).ToList();
                }


                //var groupedCustomerList = data
                //                        .GroupBy(u => u.ItemCode)
                //                        .Select(grp => grp.ToList())
                //                        .ToList();

                if (Supl_Code != "")
                {
                    foreach (var SalesBySupplier in data)
                    {
                        if (SalesBySupplier.SUPL_CODE == Supl_Code)
                            ReportData.Add(SalesBySupplier);
                    }
                }
                else
                    ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult PriceOverride(string FromDate, string ToDate, string Visibility, string Web, bool ForWeb = false)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\PriceOverrideReport.rdlc";
                var data = (
                         from main in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                            && x.CANCEL == "F"
                            && x.TRANS_TYPE == Constants.TransType.Sales
                            && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                         join detail in db.TRANS_DT.Where(x=>x.PRICE_OVR == "T") on main.TRANS_NO equals detail.TRANS_NO
                         from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                        from user in db.TILL_USERS.Where(x => x.USER_ID == main.USER_ID).DefaultIfEmpty()
                        from branch in db.BRANCHes.Where(x => x.BRANCH_CODE == main.TILL_NO).DefaultIfEmpty()
                         select new
                        {
                            Retail = detail.UNIT_RETAIL,
                            Cost = detail.UNIT_COST,
                            PriceDate = main.TRANS_DATE,
                            Previousretail = detail.BEFORE_PRICE,
                            //Previouscost = detail.PREV_COST,
                            ItemCode = detail.BARCODE,
                            TotalAmount = ((detail.UNIT_RETAIL) * (detail.UNITS_SOLD)),
                            Qty = detail.UNITS_SOLD,
                            DocumentNo = main.TRANS_NO,
                            Description = product.DESCRIPTION,
                            UserName = user.USER_NAME,
                            Branch = branch.BRANCH_NAME
                        }).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult ItemVoid(string FromDate, string ToDate, string Visibility, bool ForWeb = false)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\ItemVoidReport.rdlc";
                var data = new List<ItemVoidReportModel>();

                if (ForWeb)
                {
                    data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                           && x.CANCEL == "F"
                           && x.TRANS_TYPE == Constants.TransType.Sales
                           && x.SALE_TYPE == Constants.SalesPage.WebStore
                           && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        join transDetail in db.TRANS_DT on transMain.TRANS_NO equals transDetail.TRANS_NO
                        where transDetail.VOID == "T"
                        join product in db.PRODUCTS on transDetail.BARCODE equals product.BARCODE
                        from user in db.TILL_USERS.Where(x => x.USER_ID == transMain.VOID_UID).DefaultIfEmpty()
                        from branch in db.BRANCHes.Where(x => x.BRANCH_CODE == transMain.TILL_NO).DefaultIfEmpty()
                        select new ItemVoidReportModel
                        {
                            TRANS_NO = transMain.TRANS_NO,
                            BARCODE = transDetail.BARCODE,
                            UNIT_RETAIL = transDetail.UNIT_RETAIL,
                            SCAN_TIME = transDetail.SCAN_TIME,
                            DESCRIPTION = product.DESCRIPTION,
                            USER_NAME = user.USER_NAME,
                            PRICE = transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL,
                            BRANCH = branch.BRANCH_NAME
                        }).ToList();

                    
                }
                else
                {
                    data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                           && x.TRANS_TYPE == Constants.TransType.Sales
                            && x.CANCEL == "F"
                           && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        join transDetail in db.TRANS_DT on transMain.TRANS_NO equals transDetail.TRANS_NO
                        where transDetail.VOID == "T"
                        join product in db.PRODUCTS on transDetail.BARCODE equals product.BARCODE
                        from user in db.TILL_USERS.Where(x=>x.USER_ID == transMain.VOID_UID).DefaultIfEmpty()
                        from branch in db.BRANCHes.Where(x => x.BRANCH_CODE == transMain.TILL_NO).DefaultIfEmpty()
                        select new ItemVoidReportModel
                        {
                            TRANS_NO = transMain.TRANS_NO,
                            BARCODE = transDetail.BARCODE,
                            UNIT_RETAIL = transDetail.UNIT_RETAIL,
                            SCAN_TIME = transDetail.SCAN_TIME,
                            DESCRIPTION = product.DESCRIPTION,
                            USER_NAME = user.USER_NAME,
                            PRICE = transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL,
                            BRANCH = branch.BRANCH_NAME
                        }).ToList();

                }
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult BillVoid(string BillNo,string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\BillVoid.rdlc";
                var data = new List<ItemVoidReportModel>();

                if (string.IsNullOrEmpty(BillNo))
                {
                    data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                        && x.CANCEL == "T"
                           && x.TRANS_TYPE == Constants.TransType.Sales
                           && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        join transDetail in db.TRANS_DT on transMain.TRANS_NO equals transDetail.TRANS_NO
                        where transDetail.VOID == "T"
                        join product in db.PRODUCTS on transDetail.BARCODE equals product.BARCODE
                        from user in db.TILL_USERS.Where(x => x.USER_ID == transMain.VOID_UID).DefaultIfEmpty()
                        from branch in db.BRANCHes.Where(x => x.BRANCH_CODE == transMain.TILL_NO).DefaultIfEmpty()
                        select new ItemVoidReportModel
                        {
                            TRANS_NO = transMain.TRANS_NO,
                            BARCODE = transDetail.BARCODE,
                            UNIT_RETAIL = transDetail.UNIT_RETAIL,
                            SCAN_TIME = transDetail.SCAN_TIME,
                            DESCRIPTION = product.DESCRIPTION,
                            USER_NAME = user.USER_NAME,
                            PRICE = transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL,
                            QTY = transDetail.UNITS_SOLD,
                            BRANCH = branch.BRANCH_NAME
                        }).ToList();
                }
                else
                {
                    data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                           && x.TRANS_TYPE == Constants.TransType.Sales
                        && x.CANCEL == "T"

                           && x.TRANS_NO == BillNo
                           && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        join transDetail in db.TRANS_DT on transMain.TRANS_NO equals transDetail.TRANS_NO
                        where transDetail.VOID == "T"
                        join product in db.PRODUCTS on transDetail.BARCODE equals product.BARCODE
                        from user in db.TILL_USERS.Where(x => x.USER_ID == transMain.VOID_UID).DefaultIfEmpty()
                        from branch in db.BRANCHes.Where(x => x.BRANCH_CODE == transMain.TILL_NO).DefaultIfEmpty()
                        select new ItemVoidReportModel
                        {
                            TRANS_NO = transMain.TRANS_NO,
                            BARCODE = transDetail.BARCODE,
                            UNIT_RETAIL = transDetail.UNIT_RETAIL,
                            SCAN_TIME = transDetail.SCAN_TIME,
                            DESCRIPTION = product.DESCRIPTION,
                            USER_NAME = user.USER_NAME,
                            PRICE = transDetail.UNITS_SOLD * transDetail.UNIT_RETAIL,
                            QTY = transDetail.UNITS_SOLD,
                            BRANCH = branch.BRANCH_NAME
                        }).ToList();

                }
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult SalesByDocument(string FromDate, string ToDate, string Visibility, bool ForWeb = false)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\SaleReportByDocument.rdlc";

                List<SalesBySupplierReportModel> data = new List<SalesBySupplierReportModel>();
                if (ForWeb)
                {
                    data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                        && x.CANCEL == "F"
                        && x.TRANS_TYPE == Constants.TransType.Sales 
                        && x.SALE_TYPE == Constants.SalesPage.WebStore 
                        && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        from detail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO && (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                        from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == detail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                        //from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                        from supplier in db.BRANCHes.Where(x => x.BRANCH_CODE == transMain.TILL_NO).DefaultIfEmpty()
                        join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                        select new SalesBySupplierReportModel
                        {
                            SupplierName = supplier.BRANCH_NAME,
                            SUPL_CODE = supplier.BRANCH_CODE,
                            //SupplierName = supplier.SUPL_NAME,
                            //SUPL_CODE = supplier.SUPL_CODE,
                            DocumentNo = detail.TRANS_NO,
                            DocumentDate = transMain.TRANS_DATE,
                            ItemCode = product.BARCODE,
                            ItemName = product.DESCRIPTION,

                            Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                            Unit_Cost = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                            Rate = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                            Discount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),

                            //Qty = transDetail.UNITS_SOLD,
                            //Unit_Cost = transDetail.UNIT_COST,
                            Unit_Retail = detail.UNIT_RETAIL,
                            //Rate = transDetail.AMOUNT,
                            //Discount = (((transDetail.DIS_AMOUNT ?? 0) * transDetail.UNIT_RETAIL) / 100) * transDetail.UNITS_SOLD
                        }).ToList();
                }
                else
                {
                    data = (
                            from main in db.TRANS_MN.Where(x => x.STATUS == "3"
                            && x.CANCEL == "F"
                            && x.TRANS_TYPE == Constants.TransType.Sales 
                            && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                            join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")).DefaultIfEmpty()
                            on main.TRANS_NO equals detail.TRANS_NO
                            from product in db.PRODUCTS.Where(x => x.BARCODE == detail.BARCODE).DefaultIfEmpty()
                            from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == detail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                            //from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                            from supplier in db.BRANCHes.Where(x => x.BRANCH_CODE == main.TILL_NO).DefaultIfEmpty()
                            join Trans in db.TRANS_DT on main.TRANS_NO equals Trans.TRANS_NO into g
                            select new SalesBySupplierReportModel
                            {
                                SupplierName = supplier.BRANCH_NAME,
                                SUPL_CODE = supplier.BRANCH_CODE,
                                //SupplierName = supplier.SUPL_NAME,
                                //SUPL_CODE = supplier.SUPL_CODE,
                                DocumentNo = detail.TRANS_NO,
                                DocumentDate = main.TRANS_DATE,
                                ItemCode = product.BARCODE,
                                ItemName = product.DESCRIPTION,

                                Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                                Unit_Cost = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT) : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                                Rate = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                                Discount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),


                                //Qty = transDetail.UNITS_SOLD,
                                //Unit_Cost = transDetail.UNIT_COST,
                                Unit_Retail = detail.UNIT_RETAIL,
                                //Rate = transDetail.AMOUNT,
                                //Discount = (((transDetail.DIS_AMOUNT ?? 0) * transDetail.UNIT_RETAIL) / 100) * transDetail.UNITS_SOLD
                            }).OrderBy(x=>x.ItemCode).ToList();
                }
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult SalesInvoice(string Doc_Number, string Doc_Type, string RptSize = "")
        {
            try
            {
                string path = "";
                if (RptSize == "A4")
                {
                    path = @"RPT\Sales\SaleBillByDocumentNoA4.rdlc";

                }
                else if (RptSize == "A5")
                {
                    path = @"RPT\Sales\SaleBillByDocumentNoA5.rdlc";

                }
                else if (RptSize == "3I")
                {
                    path = @"RPT\Sales\SaleBillByDocumentNo3Inch.rdlc";
                }
                else
                {
                    RptSize = "A4";
                    path = @"RPT\Sales\SaleBillByDocumentNoA4.rdlc";
                    //path = @"RPT\Sales\SaleBillByDocumentNo.rdlc";
                }

                var modelList = (from salesMain in db.TRANS_MN
                                 from salesDetail in db.TRANS_DT.Where(x => x.TRANS_NO == salesMain.TRANS_NO && x.VOID == "F").DefaultIfEmpty()
                                 from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == salesMain.PARTY_CODE).DefaultIfEmpty()
                                 from product in db.PRODUCTS.Where(x => x.BARCODE == salesDetail.BARCODE).DefaultIfEmpty()
                                 from staff in db.STAFF_MEMBER.Where(x => x.SUPL_CODE == salesMain.SALESMAN_CDE).DefaultIfEmpty()
                                 where salesMain.TRANS_NO == Doc_Number && supplier.PARTY_TYPE == Constants.CustomerSupplier.Customer
                                 // join salesDetail in db.TRANS_DT
                                 //on salesMain.TRANS_NO equals salesDetail.TRANS_NO
                                 // join supplier in db.SUPPLIERs
                                 // on salesMain.PARTY_CODE equals supplier.SUPL_CODE
                                 // join product in db.PRODUCTS
                                 // on salesDetail.BARCODE equals product.BARCODE
                                 // join staff in db.STAFF_MEMBER
                                 // on salesMain.SALESMAN_CDE equals staff.SUPL_CODE
                                 // where salesMain.TRANS_NO == Doc_Number && supplier.PARTY_TYPE == Constants.CustomerSupplier.Customer
                                 select new SalesVM
                                 {
                                     SupplierName = (supplier.NAME_URDU == null || supplier.NAME_URDU == "") ? supplier.SUPL_NAME : supplier.NAME_URDU,
                                     SupplierContactNo = supplier.MOBILE,
                                     Phone = supplier.MOBILE,
                                     DocumentDate = salesMain.START_TIME,
                                     Address = supplier.ADDRESS,
                                     SupplierPhoneNo = supplier.PHONE,
                                     SupplierCode = supplier.SUPL_CODE,
                                     DocumentNo = salesMain.TRANS_NO,
                                     ItemCode = salesDetail.BARCODE,
                                     ItemName = (product.URDU == null || product.URDU == "") ? product.DESCRIPTION : product.URDU,
                                     Qty = salesDetail.UNITS_SOLD,
                                     Rate = salesDetail.UNIT_RETAIL,
                                     Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.DIS_AMOUNT,
                                     PartyType = supplier.PARTY_TYPE,
                                     Unit_Cost = salesDetail.UNIT_COST,
                                     BookingDate = salesMain.TRANS_DATE,
                                     UanNo = product.UAN_NO,
                                     CTNSize = product.CTN_PCS,
                                     FreeQty = salesDetail.FREE_QTY,
                                     Advance = salesMain.ADVANCE,
                                     Payment = salesMain.BY_CASH,
                                     Discount = (((salesDetail.DIS_AMOUNT ?? 0) * salesDetail.UNIT_RETAIL) / 100) * salesDetail.UNITS_SOLD,
                                     SalesMan = staff.SUPL_NAME,
                                     QtyCTN = product.CTN_PCS == null || product.CTN_PCS == 0 ? "0-" + Math.Floor(salesDetail.UNITS_SOLD).ToString() : Math.Floor(((salesDetail.UNITS_SOLD) / product.CTN_PCS ?? 0)).ToString() + "-" + Math.Floor((salesDetail.UNITS_SOLD % product.CTN_PCS ?? 0)).ToString(),

                                 }).ToList();//Where(x=>x.PartyType==Constants.Constants.Customer&&x.DocumentDate.Date==Date).ToList();


                if (modelList != null && modelList.Count > 0)
                {
                    foreach (var item in modelList)
                    {
                        item.Amount = (item.Rate * item.Qty) - item.Discount;
                    }
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("ReportName", Doc_Type == "IN" ? "Sales Invoice" : "Sales Return Invoice");
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                var amt = modelList.Sum(x => x.Amount);
                Parameter.Add("AmountInWords", Helper.CommonFunctions.ConvertAmount(Convert.ToDouble(amt)));


                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", modelList);

                if (RptSize == "A4")
                {
                    return CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.3", "0.3", "0.3", "0.3");
                }
                else if (RptSize == "A5")
                {
                    return CreateReport(path, Parameter, datasource, "PDF", "5.83", "8.27", "0.5", "0.3", "0.3", "0.3");
                }
                else if (RptSize == "3I")
                {
                    return CreateReport(path, Parameter, datasource, "PDF", "3", "5", "0.2", "0.2", "0.2", "0.2");
                }
                else
                {
                    return CreateReport(path, Parameter, datasource, "PDF", "5.83", "8.27", "0.5", "0.3", "0.3", "0.3");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult SalesByBranch(string Branch, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Sales\SaleReportByBranch.rdlc";

                var saleList = new List<TRANS_MN>();
                if (!string.IsNullOrEmpty(Branch))
                {
                    saleList = db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.TILL_NO == Branch
                    && x.CANCEL != "T"
                    && x.TRANS_TYPE == Constants.TransType.Sales
                    && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo)).ToList();
                }
                else
                {
                    saleList = db.TRANS_MN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                                                    && x.TRANS_TYPE == Constants.TransType.Sales
                                                     && x.CANCEL != "T"
                                                    && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo)).ToList();
                }

                var ReportData = (
                            from main in saleList
                            
                            join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F")) on main.TRANS_NO equals detail.TRANS_NO
                            //join detail in db.TRANS_DT.Where(x => x.VOID != "T") on main.TRANS_NO equals detail.TRANS_NO
                            join branch in db.BRANCHes on main.TILL_NO equals branch.BRANCH_CODE
                            join product in db.PRODUCTS on detail.BARCODE equals product.BARCODE
                            join sp in db.SUPPLIER_PRODUCTS.Where(x => x.STATUS == "1") on new { detail.BARCODE } equals new { sp.BARCODE }
                            join s in db.SUPPLIERs on sp.SUPL_CODE equals s.SUPL_CODE
                            from user in db.TILL_USERS.Where(x=>x.USER_ID == main.USER_ID).DefaultIfEmpty()
                            //join trans in db.TRANS_DT.Where(x => x.VOID != "T") on main.TRANS_NO equals trans.TRANS_NO into g
                            select new SalesBySupplierReportModel
                            {
                                BranchId = branch == null ? "" : branch.BRANCH_CODE,
                                BranchName = branch == null ? "" : branch.BRANCH_NAME,
                                SupplierName = s.SUPL_NAME,
                                SUPL_CODE = s.SUPL_CODE,
                                ItemCode = product.BARCODE,
                                ItemName = product.DESCRIPTION,
                                Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.UNITS_SOLD : -(detail.UNITS_SOLD),
                                Unit_Cost = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? (detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT : -((detail.UNITS_SOLD * detail.UNIT_COST) + detail.GST_AMOUNT),
                                Rate = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? detail.AMOUNT : (detail.AMOUNT),
                                Discount = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? ((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100)) : -(((detail.UNITS_SOLD * detail.UNIT_RETAIL) * (detail.UNIT_DISC / 100))),
                                SalesMan = user != null ? user.USER_NAME : ""
                                //units_sold = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.UNITS_SOLD : -x.UNITS_SOLD),
                                //cost_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : -(x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT),
                                //retail_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.AMOUNT : -x.AMOUNT),
                                //disc_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : -((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) ?? 0),
                                //net_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : -((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100))) ?? 0,
                                //net_margin = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : -((x.UNIT_RETAIL) * (x.UNITS_SOLD))) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : -((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT))

                                //Qty = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.UNITS_SOLD : 0),
                                //Unit_Cost = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : 0),
                                //Rate = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? x.AMOUNT : 0),
                                //Discount = g.Sum(x => (x.EXCH_FLAG == "T" && x.VOID == "F") ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : 0)
                            }).OrderBy(x=>x.ItemCode).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region SalesReturn
        [Permission("Sales Return Report")]
        public ActionResult SalesReturn()
        {
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            return View();
        }
        public ActionResult SalesReturnByDGS(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\SalesReturn\SaleReturnReportByDGS.rdlc";
                var data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == "3" && x.TRANS_TYPE == Constants.TransType.SalesReturn && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                        from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                        from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE).DefaultIfEmpty()
                        from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE).DefaultIfEmpty()
                        join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                        select new SalesReportByDGS
                        {
                            DEPT_NAME = department.DEPT_NAME,
                            DEPT_CODE = department.DEPT_CODE,
                            GROUP_NAME = groupp.GROUP_NAME,
                            GROUP_CODE = groupp.GROUP_CODE,
                            SGROUP_NAME = subGroup.SGROUP_NAME,
                            SGROUP_CODE = subGroup.SGROUP_CODE,
                            BARCODE = product.BARCODE,
                            DESCRIPTION = product.DESCRIPTION,
                            units_sold = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.UNITS_SOLD : -x.UNITS_SOLD),
                            cost_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : -(x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT),
                            retail_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.AMOUNT : -x.AMOUNT),
                            disc_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : -((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) ?? 0),
                            net_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : -((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100))) ?? 0,
                            net_margin = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : -((x.UNIT_RETAIL) * (x.UNITS_SOLD))) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : -((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT))
                        }).ToList();
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                var ReportData = new List<SalesReportByDGS>();


                if (Dept_Code == "" && Group_Code == "" && SubGroup_Code == "")
                    ReportData = data;
                else if (Dept_Code != "" && Group_Code == "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code == "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }
                else if (Dept_Code != "" && Group_Code != "" && SubGroup_Code != "")
                {
                    foreach (var productsByDept in data)
                    {
                        if (productsByDept.DEPT_CODE == Dept_Code && productsByDept.GROUP_CODE == Group_Code && productsByDept.SGROUP_CODE == SubGroup_Code)
                        {
                            ReportData.Add(productsByDept);
                        }
                    }
                }
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult SalesReturnBySupplier(string Supl_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\SalesReturn\SaleReturnReportBySupplier.rdlc";
                var ReportData = new List<SalesBySupplierReportModel>();
                var data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == "3" && x.TRANS_TYPE == Constants.TransType.SalesReturn && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                        from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                        from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                        join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                        select new SalesBySupplierReportModel
                        {
                            SupplierName = supplier.SUPL_NAME,
                            SUPL_CODE = supplier.SUPL_CODE,
                            ItemCode = product.BARCODE,
                            ItemName = product.DESCRIPTION,
                            Qty = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.UNITS_SOLD : -x.UNITS_SOLD),
                            Unit_Cost = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : -(x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT),
                            Rate = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.AMOUNT : -x.AMOUNT),
                            Discount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : -((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) ?? 0)
                        }).ToList();

                if (Supl_Code != "")
                {
                    foreach (var SalesBySupplier in data)
                    {
                        if (SalesBySupplier.SUPL_CODE == Supl_Code)
                            ReportData.Add(SalesBySupplier);
                    }
                }
                else
                    ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult SalesReturnByDocument(string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\SalesReturn\SaleReturnReportByDocument.rdlc";
                var data = (
                        from transMain in db.TRANS_MN.Where(x => x.STATUS == "3" && x.TRANS_TYPE == Constants.TransType.SalesReturn && (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo))
                        from transDetail in db.TRANS_DT.Where(x => x.TRANS_NO == transMain.TRANS_NO).DefaultIfEmpty()
                        from product in db.PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE).DefaultIfEmpty()
                        from supplierProduct in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                        from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == supplierProduct.SUPL_CODE).DefaultIfEmpty()
                        join Trans in db.TRANS_DT on transMain.TRANS_NO equals Trans.TRANS_NO into g
                        select new SalesBySupplierReportModel
                        {
                            SupplierName = supplier.SUPL_NAME,
                            SUPL_CODE = supplier.SUPL_CODE,
                            ItemCode = product.BARCODE,
                            ItemName = product.DESCRIPTION,
                            Qty = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.UNITS_SOLD : -x.UNITS_SOLD),
                            Unit_Cost = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : -(x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT),
                            Rate = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.AMOUNT : -x.AMOUNT),
                            Discount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : -((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) ?? 0)
                        }).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("UserName", CommonFunctions.GetUserName());

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region StockTransfer(In,Out)
        [Permission("Stock Transfer Report")]
        public ActionResult StockTransfer()
        {
            //TransferStockInBySupplier
            //TransferStockInByDepartment
            //TransferStockOutBySupplier
            //TransferStockOutByDepartment
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            return View();
        }
        public ActionResult StockByDoc(string Doc)
        {
            try
            {
                string path = "";
                var ReportData = new List<PurchaseReturnReportViewModel>();

                var data = (
                    from transferMain in db.TRANSFER_MAIN.Where(x => x.DOC_NO == Doc)
                    from transferDetail in db.TRANSFER_DETAIL.Where(x => x.DOC_NO == transferMain.DOC_NO).DefaultIfEmpty()
                    from product in db.PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE).DefaultIfEmpty()
                    from productSupplier in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                    from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == productSupplier.SUPL_CODE).DefaultIfEmpty()
                    from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    join grnD in db.TRANSFER_DETAIL on transferMain.DOC_NO equals grnD.DOC_NO into g
                    select new PurchaseReturnReportViewModel
                    {
                        DOC_NO = transferMain.DOC_NO,
                        DEPT_NAME = department.DEPT_NAME,
                        SUPL_NAME = supplier.SUPL_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SGROUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        BARCODE = product.BARCODE,
                        DESCRIPTION = product.DESCRIPTION,
                        UNIT_SIZE = product.UNIT_SIZE.ToString(),
                        REORDER_LVL = product.REORDER_LVL,
                        EXPIRY_DATE = product.EXPIRY_DATE,
                        UNIT_COST = transferDetail.COST,
                        //UNIT_COST = g.Sum(x => (x.COST * x.QTY)),
                        TRANSFER_IN_QTY = g.Sum(x => x.QTY),
                        QTY = transferDetail.QTY,
                        //QTY = g.Sum(x => x.QTY),
                        GST_AMOUNT = transferDetail.GST_AMOUNT,
                    }).ToList();

                path = @"RPT\StockTransfer\StockTrasferByDoc.rdlc";
                ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Visibility", "True");
                Parameter.Add("ReportType", "Transfer Stock");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult Stock(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Supl_Code, string StockINorOUT, string ForDeptorSupl, string Visibility)
        {
            try
            {
                string path = "";
                var ReportData = new List<PurchaseReturnReportViewModel>();
                if (StockINorOUT == "IN")
                    StockINorOUT = "I";
                else
                    StockINorOUT = "O";

                var data = (
                    from transferMain in db.TRANSFER_MAIN.Where(x => x.STATUS == "3" && x.DOC_TYPE == StockINorOUT)
                    from transferDetail in db.TRANSFER_DETAIL.Where(x => x.DOC_NO == transferMain.DOC_NO).DefaultIfEmpty()
                    from product in db.PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE).DefaultIfEmpty()
                    from productSupplier in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                    from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == productSupplier.SUPL_CODE).DefaultIfEmpty()
                    from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    join grnD in db.TRANSFER_DETAIL on transferMain.DOC_NO equals grnD.DOC_NO into g
                    select new PurchaseReturnReportViewModel
                    {
                        DEPT_NAME = department.DEPT_NAME,
                        SUPL_NAME = supplier.SUPL_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SGROUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        BARCODE = product.BARCODE,
                        DESCRIPTION = product.DESCRIPTION,
                        UNIT_SIZE = product.UNIT_SIZE.ToString(),
                        REORDER_LVL = product.REORDER_LVL,
                        EXPIRY_DATE = product.EXPIRY_DATE,
                        UNIT_COST = transferDetail.COST,
                        UNIT_RETAIL = transferDetail.RETAIL,
                        //UNIT_COST = g.Sum(x => (x.COST * x.QTY)),
                        //TRANSFER_IN_QTY = g.Sum(x => x.QTY),
                        QTY = transferDetail.QTY,
                        GST_AMOUNT = transferDetail.GST_AMOUNT,
                    }).ToList();

                if (ForDeptorSupl == "DepartmentWise")
                {
                    path = @"RPT\StockTransfer\StockTransferByDepartment.rdlc";
                    //path = Path.Combine(Server.MapPath(@"~\Report\StockTransferOutByDepartment.rdlc"));
                    if (!string.IsNullOrEmpty(Dept_Code))
                    {
                        foreach (var sTransfer in data)
                        {
                            if (sTransfer.DEPT_CODE == Dept_Code)
                            {
                                ReportData.Add(sTransfer);
                            }
                        }
                    }
                    else
                        ReportData = data;
                }
                else
                {
                    path = @"RPT\StockTransfer\StockTrasferBySupplier.rdlc";
                    if (!string.IsNullOrEmpty(Supl_Code))
                    {
                        foreach (var sTransfer in data)
                        {
                            if (sTransfer.SUPL_CODE == Supl_Code)
                            {
                                ReportData.Add(sTransfer);
                            }
                        }
                    }
                    else
                        ReportData = data;
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", "");
                Parameter.Add("DateTo", "");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Visibility", Visibility);
                if (StockINorOUT == "I")
                    Parameter.Add("ReportType", "Transfer Stock In Report");
                else
                    Parameter.Add("ReportType", "Transfer Stock Out Report");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult StockByBranch(string Branch, string DocType, string Visibility)
        {
            try
            {
                string path = "";
                var ReportData = new List<PurchaseReturnReportViewModel>();
                var main = new List<TRANSFER_MAIN>();
                if (DocType == "IN")
                    DocType = Constants.Constants.TransferIn;
                else
                    DocType = Constants.Constants.TransferOut;

                //if (DocType == Constants.Constants.TransferIn)
                //{

                //main = db.TRANSFER_MAIN.Where(x => x.BRANCH_ID_FROM == Branch
                //            && x.DOC_TYPE == Constants.Constants.TransferIn
                //            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                //            ).ToList();

                //}
                //if (DocType == Constants.Constants.TransferOut)
                //{
                //    main = db.TRANSFER_MAIN.Where(x => x.BRANCH_ID_FROM == Branch && x.DOC_TYPE == Constants.Constants.TransferOut).ToList();
                //}

                if (!string.IsNullOrEmpty(Branch))
                {
                    main = db.TRANSFER_MAIN.Where(x => x.BRANCH_ID_FROM == Branch
                            && x.DOC_TYPE == DocType
                            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                            ).ToList();
                }
                else
                {
                    main = db.TRANSFER_MAIN.Where(x => x.DOC_TYPE == DocType
                            && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                            ).ToList();
                }

                var data = (
                    from transferMain in main
                    from transferDetail in db.TRANSFER_DETAIL.Where(x => x.DOC_NO == transferMain.DOC_NO).DefaultIfEmpty()
                    from BranchFrom in db.BRANCHes.Where(x => x.BRANCH_CODE == transferMain.BRANCH_ID_FROM).DefaultIfEmpty()
                    from BranchTo in db.BRANCHes.Where(x => x.BRANCH_CODE == transferMain.BRANCH_ID_TO).DefaultIfEmpty()
                    from product in db.PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE).DefaultIfEmpty()
                    from productSupplier in db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == transferDetail.BARCODE && x.STATUS == "1").DefaultIfEmpty()
                    from supplier in db.SUPPLIERs.Where(x => x.SUPL_CODE == productSupplier.SUPL_CODE).DefaultIfEmpty()
                    from department in db.DEPARTMENTs.Where(x => x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from groupp in db.GROUPS.Where(x => x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    from subGroup in db.SUB_GROUPS.Where(x => x.SGROUP_CODE == product.SRGOUP_CODE && x.GROUP_CODE == product.GROUP_CODE && x.DEPT_CODE == product.DEPT_CODE).DefaultIfEmpty()
                    join grnD in db.TRANSFER_DETAIL on transferMain.DOC_NO equals grnD.DOC_NO into g
                    select new PurchaseReturnReportViewModel
                    {
                        BRANCH_FROM = BranchFrom == null ? "HEAD" : BranchFrom.BRANCH_NAME,
                        BRANCH_TO = BranchTo.BRANCH_NAME,
                        DEPT_NAME = department.DEPT_NAME,
                        SUPL_NAME = supplier.SUPL_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SGROUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        BARCODE = product.BARCODE,
                        DESCRIPTION = product.DESCRIPTION,
                        UNIT_SIZE = product.UNIT_SIZE.ToString(),
                        REORDER_LVL = product.REORDER_LVL,
                        EXPIRY_DATE = product.EXPIRY_DATE,
                        UNIT_COST = transferDetail.COST,
                        UNIT_RETAIL = transferDetail.RETAIL,
                        //UNIT_COST = g.Sum(x => (x.COST * x.QTY)),
                        //TRANSFER_IN_QTY = g.Sum(x => x.QTY),
                        QTY = transferDetail.QTY,
                        GST_AMOUNT = transferDetail.GST_AMOUNT,
                    }).ToList();

                path = @"RPT\StockTransfer\StockTrasferByBranch.rdlc";
                ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", "");
                Parameter.Add("DateTo", "");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Visibility", Visibility);
                if (DocType == "I")
                    Parameter.Add("ReportType", "Transfer Stock In Report");
                else
                    Parameter.Add("ReportType", "Transfer Stock Out Report");

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }




        #endregion

        #region Waste/Gain
        public ActionResult Waste()
        {
            return View();
        }
        public ActionResult WasteByDocumentNumber(string Doc_Number)
        {
            try
            {
                string path = @"RPT\Waste\WasteByDocumentNumber.rdlc";
                var model = (from main in db.WAST_MAIN
                             join detail in db.WAST_DETAIL
                             on main.DOC_NO equals detail.DOC_NO
                             join product in db.PRODUCTS
                             on detail.BARCODE equals product.BARCODE
                             where main.DOC_NO == Doc_Number
                             select new
                             {
                                 DocumentDate = main.DOC_DATE,
                                 DocumentNo = main.DOC_NO,
                                 ItemCode = detail.BARCODE,
                                 ItemName = product.DESCRIPTION,
                                 Qty = detail.QTY,
                                 Rate = detail.COST,
                                 Amount = (detail.COST * detail.QTY),
                                 UanNo = product.UAN_NO,
                             }).ToList();
                DateTime date = model.Select(x => x.DocumentDate).FirstOrDefault();
                string showdate = date.ToString("dd/MM/yyyy");
                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("ReportName", "Stock Adjustment Document");
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("Doc_Date", showdate);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", model);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region Inventory
        [Permission("Inventory Report")]
        public ActionResult Inventory()
        {
            //StockPositionBySupplier
            //StockPositionByDepartment
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Supplierlist = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();
            return View();
        }
        public ActionResult InventoryList(string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Supl_Code, string ForDeptorSupl, string Visibility)
        {
            try
            {
                var data = (
                    from productBalance in db.PROD_BALANCE
                    join product in db.PRODUCTS on productBalance.BARCODE equals product.BARCODE
                    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                    where productSupplier.STATUS == "1"
                    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                    from subGroup in db.SUB_GROUPS
                    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                    join loc in db.PROD_LOC_REPORT on productBalance.BARCODE equals loc.BARCODE into locBalance
                    from B1 in db.PROD_LOC_REPORT.Where(x=>x.BRANCH_ID == "01" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B2 in db.PROD_LOC_REPORT.Where(x=>x.BRANCH_ID == "02" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B3 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "03" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                        //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                    select new StockReportModel
                    {
                        BARCODE = product.BARCODE,
                        ItemName = product.DESCRIPTION,
                        SUPL_NAME = supplier.SUPL_NAME,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        CURRENT_QTY = (productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0)+ (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0),
                        //CURRENT_QTY = productBalance.CURRENT_QTY ?? 0,
                        CURRENT_amount = (product.UNIT_COST ?? 0) * ((productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0) + (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0)),
                        //CURRENT_amount = ((product.UNIT_COST ?? 0) * (productBalance.CURRENT_QTY ?? 0)),
                        OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                        OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                        GRN_QTY = productBalance.GRN_QTY ?? 0,
                        GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                        GRF_QTY = productBalance.GRF_QTY ?? 0,
                        GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                        WAST_QTY = productBalance.WAST_QTY ?? 0,
                        WAST_AMOUNT = productBalance.WAST_AMOUNT ?? 0,
                        GAIN_QTY = productBalance.GAIN_QTY ?? 0,
                        GAIN_AMOUNT = productBalance.GAIN_AMOUNT ?? 0,
                        //SALE_QTY = productBalance.SALE_QTY ?? 0,
                        SALE_QTY = locBalance.Sum(x => x.SALE_QTY) ?? 0,
                        SALE_AMOUNT = locBalance.Sum(x => x.SALE_AMOUNT) ?? 0,
                        //SALE_AMOUNT = productBalance.SALE_AMOUNT ?? 0,
                        TRANSFER_IN_QTY = productBalance.TRANSFER_IN_QTY ?? 0,
                        TRANSFER_IN_AMOUNT = productBalance.TRANSFER_IN_AMOUNT ?? 0,
                        TRANSFER_OUT_QTY = productBalance.TRANSFER_OUT_QTY ?? 0,
                        TRANSFER_OUT_AMOUNT = productBalance.TRANSFER_OUT_AMOUNT ?? 0,
                        DEPT_NAME = department.DEPT_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SRGOUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        B1 = B1.CURRENT_QTY ?? 0,
                        B2 = B2.CURRENT_QTY ?? 0,
                        B3 = B3.CURRENT_QTY ?? 0,
                        BRANCH_TOTAL = (B1.CURRENT_QTY ?? 0) + (B2.CURRENT_QTY ?? 0) + (B3.CURRENT_QTY ?? 0),
                    }).ToList();

                string path = "";
                var ReportData = new List<StockReportModel>();

                if (ForDeptorSupl == "DepartmentWise")
                {
                    path = @"RPT\Inventory\StockPositionByDepartment.rdlc";

                    if (!string.IsNullOrEmpty(Dept_Code))
                    {
                        foreach (var stock in data)
                        {
                            if (stock.DEPT_CODE == Dept_Code)
                                ReportData.Add(stock);
                        }
                    }
                    else
                        ReportData = data;
                }
                else
                {
                    path = @"RPT\Inventory\StockPositionBySupplier.rdlc";
                    if (!string.IsNullOrEmpty(Supl_Code))
                    {
                        foreach (var stock in data)
                        {
                            if (stock.SUPL_CODE == Supl_Code)
                                ReportData.Add(stock);
                        }
                    }
                    else
                        ReportData = data;
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult InventoryByBranch(string Branch, string Visibility)
        {
            try
            {
                List<StockReportModel> data = new List<StockReportModel>();
                if (Branch == "04")
                {
                    data = (
                    from productBalance in db.PROD_BALANCE
                    join product in db.PRODUCTS on productBalance.BARCODE equals product.BARCODE
                    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                    where productSupplier.STATUS == "1"
                    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                    from subGroup in db.SUB_GROUPS
                    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                    join loc in db.PROD_LOC_REPORT on productBalance.BARCODE equals loc.BARCODE into locBalance
                    from B1 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "01" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B2 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "02" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B3 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "03" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                        //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                    select new StockReportModel
                    {
                        BARCODE = product.BARCODE,
                        ItemName = product.DESCRIPTION,
                        SUPL_NAME = supplier.SUPL_NAME,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        CURRENT_QTY = (productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0) + (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0),
                        CURRENT_amount = (product.UNIT_COST ?? 0) * ((productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0) + (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0)),
                        OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                        OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                        GRN_QTY = productBalance.GRN_QTY ?? 0,
                        GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                        GRF_QTY = productBalance.GRF_QTY ?? 0,
                        GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                        WAST_QTY = productBalance.WAST_QTY ?? 0,
                        WAST_AMOUNT = productBalance.WAST_AMOUNT ?? 0,
                        GAIN_QTY = productBalance.GAIN_QTY ?? 0,
                        GAIN_AMOUNT = productBalance.GAIN_AMOUNT ?? 0,
                        //SALE_QTY = productBalance.SALE_QTY ?? 0,
                        SALE_QTY = locBalance.Sum(x => x.SALE_QTY) ?? 0,
                        SALE_AMOUNT = locBalance.Sum(x => x.SALE_AMOUNT) ?? 0,
                        //SALE_AMOUNT = productBalance.SALE_AMOUNT ?? 0,
                        TRANSFER_IN_QTY = productBalance.TRANSFER_IN_QTY ?? 0,
                        TRANSFER_IN_AMOUNT = productBalance.TRANSFER_IN_AMOUNT ?? 0,
                        TRANSFER_OUT_QTY = productBalance.TRANSFER_OUT_QTY ?? 0,
                        TRANSFER_OUT_AMOUNT = productBalance.TRANSFER_OUT_AMOUNT ?? 0,
                        DEPT_NAME = department.DEPT_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SRGOUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        B1 = B1.CURRENT_QTY ?? 0,
                        B2 = B2.CURRENT_QTY ?? 0,
                        B3 = B3.CURRENT_QTY ?? 0,
                        BRANCH_TOTAL = (B1.CURRENT_QTY ?? 0) + (B2.CURRENT_QTY ?? 0) + (B3.CURRENT_QTY ?? 0),
                    }).ToList();
                }
                else
                {
                    data = (
                    from loc in db.PROD_LOC_REPORT
                    join branch in db.BRANCHes.Where(x=>x.BRANCH_CODE != "04") on loc.BRANCH_ID equals branch.BRANCH_CODE
                    join product in db.PRODUCTS on loc.BARCODE equals product.BARCODE
                    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                    where productSupplier.STATUS == "1"
                    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                    from subGroup in db.SUB_GROUPS
                    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                    //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                    select new StockReportModel
                    {
                        BRANCH_ID = loc.BRANCH_ID,
                        BRANCH_NAME = branch.BRANCH_NAME,
                        BARCODE = product.BARCODE,
                        ItemName = product.DESCRIPTION,
                        SUPL_NAME = supplier.SUPL_NAME,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        CURRENT_QTY = loc.CURRENT_QTY ?? 0,
                        CURRENT_amount = ((product.UNIT_COST ?? 0) * (loc.CURRENT_QTY ?? 0)),
                        //OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                        //OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                        //GRN_QTY = productBalance.GRN_QTY ?? 0,
                        //GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                        //GRF_QTY = productBalance.GRF_QTY ?? 0,
                        //GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                        WAST_QTY = loc.WAST_QTY ?? 0,
                        WAST_AMOUNT = loc.WAST_AMOUNT ?? 0,
                        GAIN_QTY = loc.GAIN_QTY ?? 0,
                        GAIN_AMOUNT = loc.GAIN_AMOUNT ?? 0,
                        SALE_QTY = loc.SALE_QTY ?? 0,
                        SALE_AMOUNT = loc.SALE_AMOUNT ?? 0,
                        TRANSFER_IN_QTY = loc.TRANSFER_IN_QTY ?? 0,
                        TRANSFER_IN_AMOUNT = loc.TRANSFER_IN_AMOUNT ?? 0,
                        TRANSFER_OUT_QTY = loc.TRANSFER_OUT_QTY ?? 0,
                        TRANSFER_OUT_AMOUNT = loc.TRANSFER_OUT_AMOUNT ?? 0,
                        DEPT_NAME = department.DEPT_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SRGOUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                    }).ToList();
                }

                string path = @"RPT\Inventory\StockPositionByBranch.rdlc";

                if (Branch == "04")
                    path = @"RPT\Inventory\StockPositionByHeadBranch.rdlc";

                var ReportData = new List<StockReportModel>();
                if (!string.IsNullOrEmpty(Branch))
                {
                    foreach (var stock in data)
                    {
                        if (stock.BRANCH_ID == Branch)
                            ReportData.Add(stock);
                        else if (Branch == "04")
                            ReportData.Add(stock);
                    }

                }
                else
                    ReportData = data;


                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult InventoryByBranchDepartment(string Branch,string Dept, string Visibility)
        {
            try
            {
                List<StockReportModel> data = new List<StockReportModel>();
                if (Branch == "04")
                {
                    data = (
                    from productBalance in db.PROD_BALANCE
                    join product in db.PRODUCTS on productBalance.BARCODE equals product.BARCODE
                    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                    where productSupplier.STATUS == "1"
                    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                    from subGroup in db.SUB_GROUPS
                    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                    join loc in db.PROD_LOC_REPORT on productBalance.BARCODE equals loc.BARCODE into locBalance
                    from B1 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "01" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B2 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "02" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                    from B3 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "03" && x.BARCODE == productBalance.BARCODE).DefaultIfEmpty()
                        //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                    select new StockReportModel
                    {
                        BARCODE = product.BARCODE,
                        ItemName = product.DESCRIPTION,
                        SUPL_NAME = supplier.SUPL_NAME,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        CURRENT_QTY = (productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0) + (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0),
                        CURRENT_amount = (product.UNIT_COST ?? 0) * ((productBalance.GRN_QTY ?? 0) + (productBalance.TRANSFER_IN_QTY ?? 0) + (productBalance.GAIN_QTY ?? 0) - (productBalance.GRF_QTY ?? 0) - (productBalance.TRANSFER_OUT_QTY ?? 0) - (productBalance.WAST_QTY ?? 0)),
                        OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                        OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                        GRN_QTY = productBalance.GRN_QTY ?? 0,
                        GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                        GRF_QTY = productBalance.GRF_QTY ?? 0,
                        GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                        WAST_QTY = productBalance.WAST_QTY ?? 0,
                        WAST_AMOUNT = productBalance.WAST_AMOUNT ?? 0,
                        GAIN_QTY = productBalance.GAIN_QTY ?? 0,
                        GAIN_AMOUNT = productBalance.GAIN_AMOUNT ?? 0,
                        //SALE_QTY = productBalance.SALE_QTY ?? 0,
                        SALE_QTY = locBalance.Sum(x => x.SALE_QTY) ?? 0,
                        SALE_AMOUNT = locBalance.Sum(x => x.SALE_AMOUNT) ?? 0,
                        //SALE_AMOUNT = productBalance.SALE_AMOUNT ?? 0,
                        TRANSFER_IN_QTY = productBalance.TRANSFER_IN_QTY ?? 0,
                        TRANSFER_IN_AMOUNT = productBalance.TRANSFER_IN_AMOUNT ?? 0,
                        TRANSFER_OUT_QTY = productBalance.TRANSFER_OUT_QTY ?? 0,
                        TRANSFER_OUT_AMOUNT = productBalance.TRANSFER_OUT_AMOUNT ?? 0,
                        DEPT_NAME = department.DEPT_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SRGOUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        B1 = B1.CURRENT_QTY ?? 0,
                        B2 = B2.CURRENT_QTY ?? 0,
                        B3 = B3.CURRENT_QTY ?? 0,
                        BRANCH_TOTAL = (B1.CURRENT_QTY ?? 0) + (B2.CURRENT_QTY ?? 0) + (B3.CURRENT_QTY ?? 0),
                    }).ToList();
                }
                else
                {
                    data = (
                    from loc in db.PROD_LOC_REPORT
                    join branch in db.BRANCHes on loc.BRANCH_ID equals branch.BRANCH_CODE
                    join product in db.PRODUCTS on loc.BARCODE equals product.BARCODE
                    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                    where productSupplier.STATUS == "1"
                    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                    from subGroup in db.SUB_GROUPS
                    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                    //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                    select new StockReportModel
                    {
                        BRANCH_ID = loc.BRANCH_ID,
                        BRANCH_NAME = branch.BRANCH_NAME,
                        BARCODE = product.BARCODE,
                        ItemName = product.DESCRIPTION,
                        SUPL_NAME = supplier.SUPL_NAME,
                        UNIT_RETAIL = product.UNIT_RETAIL,
                        CURRENT_QTY = loc.CURRENT_QTY ?? 0,
                        CURRENT_amount = ((product.UNIT_COST ?? 0) * (loc.CURRENT_QTY ?? 0)),
                        //OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                        //OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                        //GRN_QTY = productBalance.GRN_QTY ?? 0,
                        //GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                        //GRF_QTY = productBalance.GRF_QTY ?? 0,
                        //GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                        WAST_QTY = loc.WAST_QTY ?? 0,
                        WAST_AMOUNT = loc.WAST_AMOUNT ?? 0,
                        GAIN_QTY = loc.GAIN_QTY ?? 0,
                        GAIN_AMOUNT = loc.GAIN_AMOUNT ?? 0,
                        SALE_QTY = loc.SALE_QTY ?? 0,
                        SALE_AMOUNT = loc.SALE_AMOUNT ?? 0,
                        TRANSFER_IN_QTY = loc.TRANSFER_IN_QTY ?? 0,
                        TRANSFER_IN_AMOUNT = loc.TRANSFER_IN_AMOUNT ?? 0,
                        TRANSFER_OUT_QTY = loc.TRANSFER_OUT_QTY ?? 0,
                        TRANSFER_OUT_AMOUNT = loc.TRANSFER_OUT_AMOUNT ?? 0,
                        DEPT_NAME = department.DEPT_NAME,
                        GROUP_NAME = groupp.GROUP_NAME,
                        SGROUP_NAME = subGroup.SGROUP_NAME,
                        DEPT_CODE = department.DEPT_CODE,
                        GROUP_CODE = groupp.GROUP_CODE,
                        SRGOUP_CODE = subGroup.SGROUP_CODE,
                        SUPL_CODE = supplier.SUPL_CODE,
                        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                    }).ToList();
                }

                string path = @"RPT\Inventory\StockPositionByBranchDepartment.rdlc";

                if (Branch == "04")
                    path = @"RPT\Inventory\StockPositionByHeadBranchDepartment.rdlc";

                var ReportData = new List<StockReportModel>();
                if (!string.IsNullOrEmpty(Branch))
                {
                    foreach (var stock in data)
                    {
                        if (stock.BRANCH_ID == Branch)
                            ReportData.Add(stock);
                        else if (Branch == "04")
                            ReportData.Add(stock);
                    }

                }
                else
                    ReportData = data;


                if (!string.IsNullOrEmpty(Dept))
                    ReportData = ReportData.Where(x => x.DEPT_CODE == Dept).ToList();

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult InventoryByBarcode(string barcode, string Visibility)
        {
            try
            {
                barcode = db.PRODUCTS.Where(x => x.BARCODE == barcode || x.DESCRIPTION == barcode).Select(x => x.BARCODE).FirstOrDefault();

                //var data = (
                //    from loc in db.PROD_LOC_REPORT
                //    where loc.BARCODE == barcode
                //    join branch in db.BRANCHes on loc.BRANCH_ID equals branch.BRANCH_CODE
                //    join product in db.PRODUCTS on loc.BARCODE equals product.BARCODE
                //    join productSupplier in db.SUPPLIER_PRODUCTS on product.BARCODE equals productSupplier.BARCODE
                //    where productSupplier.STATUS == "1"
                //    join supplier in db.SUPPLIERs on productSupplier.SUPL_CODE equals supplier.SUPL_CODE
                //    join transDetail in db.TRANS_DT on product.BARCODE equals transDetail.BARCODE into sales
                //    join grnDetail in db.GRN_DETAIL on product.BARCODE equals grnDetail.BARCODE into purchase
                //    join department in db.DEPARTMENTs on product.DEPT_CODE equals department.DEPT_CODE
                //    join groupp in db.GROUPS on new { product.GROUP_CODE, product.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                //    from subGroup in db.SUB_GROUPS
                //    where product.GROUP_CODE == subGroup.GROUP_CODE && product.DEPT_CODE == subGroup.DEPT_CODE && product.SRGOUP_CODE == subGroup.SGROUP_CODE
                //    //from subGroup in db.SUB_GROUPS on new {product.GROUP_CODE, product.DEPT_CODE, product.SRGOUP_CODE } equals new {subGroup.GROUP_CODE, subGroup.DEPT_CODE, subGroup.SGROUP_CODE}
                //    select new StockReportModel
                //    {
                //        BRANCH_ID = loc.BRANCH_ID,
                //        BRANCH_NAME = branch.BRANCH_NAME,
                //        BARCODE = product.BARCODE,
                //        ItemName = product.DESCRIPTION,
                //        SUPL_NAME = supplier.SUPL_NAME,
                //        UNIT_RETAIL = product.UNIT_RETAIL,
                //        CURRENT_QTY = loc.CURRENT_QTY ?? 0,
                //        CURRENT_amount = ((product.UNIT_COST ?? 0) * (loc.CURRENT_QTY ?? 0)),
                //        SALE_QTY = loc.SALE_QTY ?? 0,
                //        SALE_AMOUNT = loc.SALE_AMOUNT ?? 0,
                //        TRANSFER_IN_QTY = loc.TRANSFER_IN_QTY ?? 0,
                //        TRANSFER_IN_AMOUNT = loc.TRANSFER_IN_AMOUNT ?? 0,
                //        TRANSFER_OUT_QTY = loc.TRANSFER_OUT_QTY ?? 0,
                //        TRANSFER_OUT_AMOUNT = loc.TRANSFER_OUT_AMOUNT ?? 0,
                //        DEPT_NAME = department.DEPT_NAME,
                //        GROUP_NAME = groupp.GROUP_NAME,
                //        SGROUP_NAME = subGroup.SGROUP_NAME,
                //        DEPT_CODE = department.DEPT_CODE,
                //        GROUP_CODE = groupp.GROUP_CODE,
                //        SRGOUP_CODE = subGroup.SGROUP_CODE,
                //        SUPL_CODE = supplier.SUPL_CODE,
                //        FREE_UNIT_IN_QTY = purchase.Sum(x => x.FREE_QTY ?? 0),
                //        FREE_UNIT_IN_VALUE = purchase.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                //        FREE_UNIT_OUT_QTY = sales.Sum(x => x.FREE_QTY ?? 0),
                //        FREE_UNIT_OUT_VALUE = sales.Sum(x => (x.FREE_QTY ?? 0) * (product.UNIT_RETAIL ?? 0)),
                //    }).ToList();

                //var prod_Bal = (
                //    from pbal in db.PROD_BALANCE
                //    join p in db.PRODUCTS on pbal.BARCODE equals p.BARCODE
                //    join department in db.DEPARTMENTs on p.DEPT_CODE equals department.DEPT_CODE
                //    join groupp in db.GROUPS on new { p.GROUP_CODE, p.DEPT_CODE } equals new { groupp.GROUP_CODE, groupp.DEPT_CODE }
                //    from subGroup in db.SUB_GROUPS
                //    where p.GROUP_CODE == subGroup.GROUP_CODE && p.DEPT_CODE == subGroup.DEPT_CODE && p.SRGOUP_CODE == subGroup.SGROUP_CODE
                //    where pbal.BARCODE == barcode
                //    select new StockReportModel
                //    {
                //        BRANCH_ID = "",
                //        BRANCH_NAME = "",
                //        BARCODE = p.BARCODE,
                //        ItemName = p.DESCRIPTION,
                //        UNIT_RETAIL = p.UNIT_RETAIL,
                //        CURRENT_QTY = pbal.CURRENT_QTY ?? 0,
                //        CURRENT_amount = ((p.UNIT_COST ?? 0) * (pbal.CURRENT_QTY ?? 0)),
                //        //OPEN_QTY = productBalance.OPEN_QTY ?? 0,
                //        //OPEN_COST = ((productBalance.OPEN_COST ?? 0) * (productBalance.OPEN_QTY ?? 0)),
                //        //GRN_QTY = productBalance.GRN_QTY ?? 0,
                //        //GRN_AMOUNT = productBalance.GRN_AMOUNT ?? 0,
                //        //GRF_QTY = productBalance.GRF_QTY ?? 0,
                //        //GRF_AMOUNY = productBalance.GRF_AMOUNY ?? 0,
                //        //WAST_QTY = productBalance.WAST_QTY ?? 0,
                //        //WAST_AMOUNT = productBalance.WAST_AMOUNT ?? 0,
                //        //LOSS_QTY = productBalance.GAIN_QTY ?? 0,
                //        //LOSS_AMOUNT = productBalance.GAIN_AMOUNT ?? 0,
                //        SALE_QTY = pbal.SALE_QTY ?? 0,
                //        SALE_AMOUNT = pbal.SALE_AMOUNT ?? 0,
                //        TRANSFER_IN_QTY = pbal.TRANSFER_IN_QTY ?? 0,
                //        TRANSFER_IN_AMOUNT = pbal.TRANSFER_IN_AMOUNT ?? 0,
                //        TRANSFER_OUT_QTY = pbal.TRANSFER_OUT_QTY ?? 0,
                //        TRANSFER_OUT_AMOUNT = pbal.TRANSFER_OUT_AMOUNT ?? 0,
                //        DEPT_NAME = department.DEPT_NAME,
                //        GROUP_NAME = groupp.GROUP_NAME,
                //        SGROUP_NAME = subGroup.SGROUP_NAME,
                //        DEPT_CODE = department.DEPT_CODE,
                //        GROUP_CODE = groupp.GROUP_CODE,
                //        SRGOUP_CODE = subGroup.SGROUP_CODE,

                //    }).ToList();


                var data = (
                from p in db.PRODUCTS.Where(x => x.BARCODE == barcode).DefaultIfEmpty()

                    //from loc in db.PROD_LOC_REPORT.Where(x => x.BARCODE == barcode)
                    //from B1 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "01" && x.BARCODE == loc.BARCODE).DefaultIfEmpty()
                    //from B2 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "02" && x.BARCODE == loc.BARCODE).DefaultIfEmpty()
                    //from B3 in db.PROD_LOC_REPORT.Where(x => x.BRANCH_ID == "03" && x.BARCODE == loc.BARCODE).DefaultIfEmpty()
                    //join p in db.PRODUCTS.Where(x => x.BARCODE == loc.BARCODE).DefaultIfEmpty()

                join B1 in db.PROD_LOC_REPORT on p.BARCODE equals B1.BARCODE into stock
                join ho in db.PROD_BALANCE on p.BARCODE equals ho.BARCODE into hoStock
                //join B2 in db.PROD_LOC_REPORT on new {barnch = "02",barcode = loc.BARCODE } equals new {barnch = B2.BRANCH_ID,barcode = B2.BARCODE } into B2s 
                //join B3 in db.PROD_LOC_REPORT on new {barnch = "03",barcode = loc.BARCODE } equals new {barnch = B3.BRANCH_ID,barcode = B3.BARCODE } into B3s

                select new StockReportModel
                {
                    BARCODE = p.BARCODE,
                    ItemName = p.DESCRIPTION,
                    B1 = stock.Where(x=>x.BRANCH_ID == "01").Sum(x=>x.CURRENT_QTY ?? 0),
                    B1VALUE = stock.Where(x => x.BRANCH_ID == "01").Sum(x => (x.CURRENT_QTY ?? 0)) * (p.UNIT_RETAIL ?? 0),
                    B2 = stock.Where(x => x.BRANCH_ID == "02").Sum(x => x.CURRENT_QTY ?? 0),
                    B2VALUE = stock.Where(x => x.BRANCH_ID == "02").Sum(x => (x.CURRENT_QTY ?? 0)) * (p.UNIT_RETAIL ?? 0),
                    B3 = stock.Where(x => x.BRANCH_ID == "03").Sum(x => x.CURRENT_QTY ?? 0),
                    B3VALUE = stock.Where(x => x.BRANCH_ID == "03").Sum(x => (x.CURRENT_QTY ?? 0)) * (p.UNIT_RETAIL ?? 0),
                    BRANCH_TOTAL = stock.Where(x => x.BRANCH_ID == "01").Sum(x => x.CURRENT_QTY ?? 0) + stock.Where(x => x.BRANCH_ID == "02").Sum(x => x.CURRENT_QTY ?? 0) + stock.Where(x => x.BRANCH_ID == "03").Sum(x => x.CURRENT_QTY ?? 0),
                    BRANCH_TOTAL_VALUE = stock.Where(x => x.BRANCH_ID == "01").Sum(x => x.CURRENT_QTY ?? 0) + stock.Where(x => x.BRANCH_ID == "02").Sum(x => x.CURRENT_QTY ?? 0) + stock.Where(x => x.BRANCH_ID == "03").Sum(x => x.CURRENT_QTY ?? 0) * (p.UNIT_RETAIL ?? 0),
                    CURRENT_QTY = hoStock.Sum(x=> ((x.GRN_QTY ?? 0) + (x.TRANSFER_IN_QTY ?? 0) + (x.GAIN_QTY ?? 0)) - ((x.WAST_QTY ?? 0) + (x.GRF_QTY ?? 0) + (x.TRANSFER_OUT_QTY ?? 0))),
                    CURRENT_amount = (hoStock.Sum(x=> x.CURRENT_QTY) ?? 0) * (p.UNIT_COST ?? 0),

                    //B1 = B1.CURRENT_QTY ?? 0,
                    //B1VALUE = (B1.CURRENT_QTY ?? 0) * (p.UNIT_RETAIL ?? 0),
                    //B2 = B2.CURRENT_QTY ?? 0,
                    //B2VALUE = (B2.CURRENT_QTY ?? 0) * (p.UNIT_RETAIL ?? 0),
                    //B3 = B3.CURRENT_QTY ?? 0,
                    //B3VALUE = (B3.CURRENT_QTY ?? 0) * (p.UNIT_RETAIL ?? 0),
                    //BRANCH_TOTAL = ((B1.CURRENT_QTY ?? 0) + (B2.CURRENT_QTY ?? 0) + (B3.CURRENT_QTY ?? 0)),
                    //BRANCH_TOTAL_VALUE = ((B1.CURRENT_QTY ?? 0) + (B2.CURRENT_QTY ?? 0) + (B3.CURRENT_QTY ?? 0)) * (p.UNIT_RETAIL ?? 0),
                }).ToList();

                //data = data.Union(prod_Bal).ToList();

                string path = @"RPT\Inventory\StockPositionByBCode.rdlc";
                var ReportData = new List<StockReportModel>();
                ReportData = data;


                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "11.69", "8.27", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult ItemLedger()
        {
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            return View();
        }
        public ActionResult GetItemLedgerP(string Barcode, string FromDate, string ToDate,string Visibility)
        {
            string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
            DateTime DateFrom = Convert.ToDateTime(FromDate);
            DateTime DateTo = Convert.ToDateTime(ToDate);
            DateTo = DateTo.AddDays(1);
            IEnumerable<PRODUCT> pList;

            if (string.IsNullOrEmpty(Barcode))
                pList = db.PRODUCTS.AsEnumerable();
            else
                pList = db.PRODUCTS.Where(x => x.BARCODE == Barcode).AsEnumerable();

            try
            {
                var purchase = (
                    from main in db.GRN_MAIN.Where(x=> (x.GRN_DATE >= DateFrom && x.GRN_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    )
                    join detail in db.GRN_DETAIL on main.GRN_NO equals detail.GRN_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.GRN_NO,
                        DocDate = main.GRN_DATE,
                        Qty = detail.QTY ?? 0,
                        Cost = detail.COST,
                        Discount = detail.DIS_AMT,
                        Retail = product.UNIT_RETAIL,
                        Comments = "Purchase",
                    }).ToList();

                var PurchaseReturn = (
                    from main in db.GRFS_MAIN.Where(x => (x.GRF_DATE >= DateFrom && x.GRF_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    )
                    join detail in db.GRFS_DETAIL on main.GRF_NO equals detail.GRF_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                   select new ItemViewModel
                   {
                       Barcode = product.BARCODE,
                       Description = product.DESCRIPTION,
                       DocNo = detail.GRF_NO,
                       DocDate = main.GRF_DATE,
                       Qty = -detail.QTY ?? 0,
                       Cost = detail.COST,
                       Discount = detail.DISCOUNT,
                       Retail = product.UNIT_RETAIL,
                       Comments = "Purchase Return",
                   }).ToList();

                var transfer = (
                    from main in db.TRANSFER_MAIN.Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    &&( x.BRANCH_ID_FROM == "04" || x.BRANCH_ID_TO == "04")
                    ) 
                    join branchTo in db.BRANCHes on main.BRANCH_ID_TO equals branchTo.BRANCH_CODE
                    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                   select new ItemViewModel
                   {
                       Barcode = product.BARCODE,
                       Description = product.DESCRIPTION,
                       DocNo = detail.DOC_NO,
                       DocDate = main.DOC_DATE,
                       Qty = main.DOC_TYPE == "I" ? detail.QTY : -detail.QTY,
                       Cost = detail.COST,
                       Discount = 0,
                       Retail = product.UNIT_RETAIL,
                       Comments = main.DOC_TYPE == "I" ? "TI " : "TO " + branchTo.BRANCH_NAME,
                   }).ToList();

                var adjustment = (
                    from main in db.WAST_MAIN.Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.BRANCH_ID == "04")
                    join detail in db.WAST_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.DOC_NO,
                        DocDate = main.DOC_DATE,
                        Qty = main.DOC_TYPE == "W" ? -detail.QTY : detail.QTY,
                        Cost = detail.COST,
                        Discount = 0,
                        Retail = product.UNIT_RETAIL,
                        Comments = main.DOC_TYPE == "W" ? "Waste " : "Gain " ,
                    }).ToList();

                var data = purchase.Union(PurchaseReturn).ToList();
                data = data.Union(transfer).ToList();
                data = data.Union(adjustment).ToList();
                
                data = data.OrderBy(x => x.DocDate).ToList();



                decimal qty = 0; 
                foreach (var item in data)
                {
                    qty += item.Qty;
                    item.UnitSize = qty;
                }


                //var result = db.Database.SqlQuery<DbResult>(
                // "select ID, NAME, DB_FIELD from eis_hierarchy");


                string path = "";
                var ReportData = new List<ItemViewModel>();

                    path = @"RPT\Inventory\ItemLedger.rdlc";
                 
                ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public ActionResult GetItemLedgerS(string Barcode, string FromDate, string ToDate, string Branch, string Visibility)
        {
            string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
            DateTime DateFrom = Convert.ToDateTime(FromDate);
            DateTime DateTo = Convert.ToDateTime(ToDate);
            DateTo = DateTo.AddDays(1);
            IEnumerable<PRODUCT> pList;

            if (string.IsNullOrEmpty(Barcode))
                pList = db.PRODUCTS.AsEnumerable();
            else
                pList = db.PRODUCTS.Where(x => x.BARCODE == Barcode).AsEnumerable();

            try
            {
                //var transferFromHead = (
                //    from main in db.TRANSFER_MAIN.Include(x => x.BRANCH).Include(x => x.BRANCH1).Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                //    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                //    && x.BRANCH_ID_FROM == "04"
                //    && x.BRANCH_ID_TO != null
                //    )
                //    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                //    join product in pList on detail.BARCODE equals product.BARCODE
                //    select new ItemViewModel
                //    {
                //        Barcode = product.BARCODE,
                //        Description = product.DESCRIPTION,
                //        DocNo = detail.DOC_NO,
                //        DocDate = main.DOC_DATE,
                //        Qty = detail.QTY,
                //        Cost = detail.COST,
                //        Discount = 0,
                //        Retail = product.UNIT_RETAIL,
                //        Comments = "TFH To " + main.BRANCH1.BRANCH_NAME ,
                //        BranchId = main.BRANCH == null ? main.BRANCH1.BRANCH_CODE : main.BRANCH.BRANCH_CODE
                //    }).ToList();

                //var transferToHead = (
                //    from main in db.TRANSFER_MAIN.Include(x => x.BRANCH).Include(x => x.BRANCH1).Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                //    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                //    && x.BRANCH_ID_FROM != null
                //    && x.BRANCH_ID_TO == "04"
                //    )
                //    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                //    join product in pList on detail.BARCODE equals product.BARCODE
                //    select new ItemViewModel
                //    {
                //        Barcode = product.BARCODE,
                //        Description = product.DESCRIPTION,
                //        DocNo = detail.DOC_NO,
                //        DocDate = main.DOC_DATE,
                //        Qty = -detail.QTY,
                //        Cost = detail.COST,
                //        Discount = 0,
                //        Retail = product.UNIT_RETAIL,
                //        Comments =  main.BRANCH.BRANCH_NAME + "TO HEAD",
                //        BranchId = main.BRANCH == null ? main.BRANCH1.BRANCH_CODE : main.BRANCH.BRANCH_CODE
                //    }).ToList();

                var transferInFrom = (
                    from main in db.TRANSFER_MAIN.Include(x => x.BRANCH).Include(x => x.BRANCH1).Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.DOC_TYPE == Constants.Constants.TransferIn
                     //&& x.BRANCH_ID_FROM != null
                    //&& x.BRANCH_ID_TO != null
                    )
                    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.DOC_NO,
                        DocDate = main.DOC_DATE,
                        Qty = -detail.QTY,
                        Cost = detail.COST,
                        Discount = 0,
                        Retail = product.UNIT_RETAIL,
                        Comments = main.BRANCH.BRANCH_NAME+" To " + main.BRANCH1.BRANCH_NAME ,
                        BranchId = main.BRANCH_ID_FROM
                    }).ToList();

                var transferInTo = (
                    from main in db.TRANSFER_MAIN.Include(x => x.BRANCH).Include(x => x.BRANCH1).Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.DOC_TYPE == Constants.Constants.TransferIn
                    //&& x.BRANCH_ID_FROM != null
                    //&& x.BRANCH_ID_TO != null
                    )
                    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.DOC_NO,
                        DocDate = main.DOC_DATE,
                        Qty = detail.QTY,
                        Cost = detail.COST,
                        Discount = 0,
                        Retail = product.UNIT_RETAIL,
                        Comments = main.BRANCH.BRANCH_NAME + " To " + main.BRANCH1.BRANCH_NAME,
                        BranchId = main.BRANCH_ID_TO
                    }).ToList();


                var transferOut = (
                    from main in db.TRANSFER_MAIN.Include(x => x.BRANCH).Include(x => x.BRANCH1).Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.DOC_TYPE == Constants.Constants.TransferOut

                     //&& x.BRANCH_ID_FROM != null
                    //&& x.BRANCH_ID_TO != null
                    )
                    join detail in db.TRANSFER_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.DOC_NO,
                        DocDate = main.DOC_DATE,
                        Qty = detail.QTY,
                        Cost = detail.COST,
                        Discount = 0,
                        Retail = product.UNIT_RETAIL,
                        Comments = main.BRANCH.BRANCH_NAME + " To " + main.BRANCH1.BRANCH_NAME,
                        BranchId = main.BRANCH_ID_TO
                    }).ToList();

                var Sale = (
                    from main in db.TRANS_MN.Where(x => (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.TRANS_TYPE == Constants.TransType.Sales)
                    join detail in db.TRANS_DT.Where(x => (x.VOID == "F" && x.EXCH_FLAG == "T") || (x.VOID == "F" && x.EXCH_FLAG == "F"))
                    on main.TRANS_NO equals detail.TRANS_NO
                    join branch in db.BRANCHes
                    on main.TILL_NO equals branch.BRANCH_CODE
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.TRANS_NO,
                        DocDate = main.TRANS_DATE,
                        Qty = (detail.EXCH_FLAG == "T" && detail.VOID == "F") ? -detail.UNITS_SOLD : (detail.UNITS_SOLD),
                        Cost = detail.UNIT_COST,
                        Discount = detail.DIS_AMOUNT,
                        Retail = product.UNIT_RETAIL,
                        Comments = "Sale",
                        BranchId = branch.BRANCH_CODE
                    }).ToList();

                var SaleReturn = (
                    from main in db.TRANS_MN.Where(x => (x.TRANS_DATE >= DateFrom && x.TRANS_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.TRANS_TYPE == Constants.TransType.SalesReturn)
                    join detail in db.TRANS_DT
                    on main.TRANS_NO equals detail.TRANS_NO
                    join branch in db.BRANCHes
                    on main.TILL_NO equals branch.BRANCH_CODE
                    join product in pList on detail.BARCODE equals product.BARCODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.TRANS_NO,
                        DocDate = main.TRANS_DATE,
                        Qty = detail.UNITS_SOLD,
                        Cost = detail.UNIT_COST,
                        Discount = detail.DIS_AMOUNT,
                        Retail = product.UNIT_RETAIL,
                        Comments = "Sale Return",
                        BranchId = branch.BRANCH_CODE
                    }).ToList();


                var adjustment = (
                    from main in db.WAST_MAIN.Where(x => (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo)
                    && x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.BRANCH_ID != "04"
                    )
                    join detail in db.WAST_DETAIL on main.DOC_NO equals detail.DOC_NO
                    join product in pList on detail.BARCODE equals product.BARCODE
                    //join branch in db.BRANCHes
                    //on main.BRANCH_ID equals branch.BRANCH_CODE
                    select new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        Description = product.DESCRIPTION,
                        DocNo = detail.DOC_NO,
                        DocDate = main.DOC_DATE,
                        Qty = main.DOC_TYPE == "W" ? -detail.QTY : detail.QTY,
                        Cost = detail.COST,
                        Discount = 0,
                        Retail = product.UNIT_RETAIL,
                        Comments = main.DOC_TYPE == "W" ? "Waste " : "Gain ",
                        BranchId = main.BRANCH_ID
                    }).ToList();

                //var data = transferFromHead.Union(Sale).ToList();
                //data = data.Union(transferToHead).ToList();

                //data = data.Union(transferIn).ToList();
                //data = data.Union(transferOut).ToList();
                var data = transferInFrom.Union(transferInTo).ToList();
                data = data.Union(transferOut).ToList();
                data = data.Union(Sale).ToList();
                data = data.Union(SaleReturn).ToList();
                data = data.Union(adjustment).ToList();
                data = data.OrderBy(x => x.DocDate).ToList();
                

                if (!string.IsNullOrEmpty(Branch))
                {
                    data = data.Where(x => x.BranchId == Branch).ToList();
                }

                decimal qty = 0;
                foreach (var item in data)
                {
                    qty += item.Qty;
                    item.UnitSize = qty;
                }

                string path = "";
                var ReportData = new List<ItemViewModel>();

                path = @"RPT\Inventory\ItemLedger.rdlc";

                ReportData = data;

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region Reservation
        public ActionResult ReservationForm(long resId)
        {
            try
            {
                string path = @"RPT\Reservation\ReservationForm.rdlc";
                List<ReservationFormReportViewModel> DataList = new List<ReservationFormReportViewModel>();
                ReservationViewModel vm = new ReservationViewModel();
                var reservation = db.RESERVATION_MAIN
                    .Include(X => X.CITY1)
                    .Include(X => X.COUNTRY1)
                    .Include(X => X.AMENITY_PRODUCT_DETAIL)
                    .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.PRODUCT))
                    .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.ROOM))
                    .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY)
                    .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY.Select(x => x.PRODUCT))
                    .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY.Select(x => x.ROOM))
                    .Include(x => x.RESERVATION_DETAIL)
                    .Include(x => x.RESERVATION_DETAIL.Select(y => y.ROOM))
                    .Include(x => x.RESERVATION_DETAIL_HISTORY)
                    .Include(x => x.RESERVATION_DETAIL_HISTORY.Select(y => y.ROOM))
                    .Where(x => x.RES_ID == resId).SingleOrDefault();

                List<ReservationMain> mainList = new List<ReservationMain>();
                ReservationMain main = new ReservationMain();
                List<ReservationDetail> detaillist = new List<ReservationDetail>();
                List<ReservationItems> productList = new List<ReservationItems>();
                List<ReservationItems> amenityList = new List<ReservationItems>();
                main.ADDRESS = reservation.ADDRESS;
                main.AMOUNT_PAID = reservation.AMOUNT_PAID;
                main.BALANCE = reservation.BALANCE;
                //main.Cities = db.CITies.ToList();
                main.CITY = reservation.CITY1.CITY_NAME;
                main.COMPANY = reservation.COMPANY;
                //main.Countries = db.COUNTRies.ToList();
                main.COUNTRY = reservation.COUNTRY1.COUNTRY_NAME;
                main.DISCOUNT = reservation.DISCOUNT;
                main.DOB = reservation.DOB;
                main.DOC_DATE = reservation.DOC_DATE;
                main.E_MAIL = reservation.E_MAIL;
                main.FIRST_NAME = reservation.FIRST_NAME;
                main.ID_TYPE = reservation.ID_TYPE == "C" ? "CNIC" : (reservation.ID_TYPE == "D" ? "Driving Licence" : "Other");
                //main.IdTypes = IdTypes();
                main.ID_NO = reservation.ID_NO;
                main.LAST_NAME = reservation.LAST_NAME;
                main.PHONE = reservation.PHONE;
                main.RES_ID = reservation.RES_ID;
                main.STATUS = reservation.STATUS;
                main.SUPL_CODE = reservation.SUPL_CODE;
                main.TOTAL_AMT = reservation.TOTAL_AMT;
                mainList.Add(main);
                foreach (var item in reservation.RESERVATION_DETAIL)
                {
                    ReservationDetail detail = new ReservationDetail();
                    detail.ADULTS = item.ADULTS;
                    detail.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
                    detail.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
                    detail.CHILDREN = item.CHILDREN;
                    detail.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                    detail.GST = item.GST;
                    detail.OTHER_CHARGES = item.OTHER_CHARGES;
                    detail.RATE_PER_DAY = item.RATE_PER_DAY;
                    detail.RES_ID = item.RES_ID;
                    detail.RES_STS = item.RES_STS;
                    detail.REVS_DAYS = item.REVS_DAYS;
                    detail.ROOM_CATEGORY = item.ROOM.CATEGORY;
                    detail.ROOM_CODE = item.ROOM.ROOM_CODE;
                    detail.ROOM_NAME = item.ROOM.ROOM_NAME;
                    detail.ROOM_TYPE = item.ROOM.TYPE;
                    detail.SERVICES_CHARGES = item.SERVICES_CHARGES;
                    detail.TABLES_CODE = item.TABLES_CODE;
                    detail.TOTAL_AMT = item.TOTAL_AMT;
                    detail.STATUS = item.STATUS;
                    detaillist.Add(detail);
                }
                foreach (var item in reservation.RESERVATION_DETAIL_HISTORY)
                {
                    ReservationDetail detail = new ReservationDetail();
                    detail.ADULTS = item.ADULTS;
                    detail.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
                    detail.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
                    detail.CHILDREN = item.CHILDREN;
                    detail.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                    detail.GST = item.GST;
                    detail.OTHER_CHARGES = item.OTHER_CHARGES;
                    detail.RATE_PER_DAY = item.RATE_PER_DAY;
                    detail.RES_ID = item.RES_ID;
                    detail.RES_STS = item.RES_STS;
                    detail.REVS_DAYS = item.REVS_DAYS;
                    detail.ROOM_CATEGORY = item.ROOM.CATEGORY;
                    detail.ROOM_CODE = item.ROOM.ROOM_CODE;
                    detail.ROOM_NAME = item.ROOM.ROOM_NAME;
                    detail.ROOM_TYPE = item.ROOM.TYPE;
                    detail.SERVICES_CHARGES = item.SERVICES_CHARGES;
                    detail.TABLES_CODE = item.TABLES_CODE;
                    detail.TOTAL_AMT = item.TOTAL_AMT;
                    detail.STATUS = item.STATUS;
                    detaillist.Add(detail);
                }
                foreach (var item in reservation.AMENITY_PRODUCT_DETAIL)
                {
                    ReservationItems prod = new ReservationItems();
                    prod.AMOUNT = item.AMOUNT;
                    prod.BARCODE = item.BARCODE;
                    prod.DATETIME = item.DATETIME;
                    prod.DESCRIPTION = item.PRODUCT.DESCRIPTION;
                    prod.QUANTITY = item.QUANTITY;
                    prod.RES_ID = item.RES_ID;
                    prod.ROOM_CODE = item.ROOM_CODE;
                    prod.ROOM_NAME = item.ROOM.ROOM_NAME;
                    prod.UNIT_RETAIL = item.PRODUCT.UNIT_RETAIL;
                    prod.STATUS = item.STATUS;
                    if (item.TYPE == "P")
                        productList.Add(prod);
                    else
                        amenityList.Add(prod);
                }
                foreach (var item in reservation.AMENITY_PRODUCT_DETAIL_HISTORY)
                {
                    ReservationItems prod = new ReservationItems();
                    prod.AMOUNT = item.AMOUNT;
                    prod.BARCODE = item.BARCODE;
                    prod.DATETIME = item.DATETIME;
                    prod.DESCRIPTION = item.PRODUCT.DESCRIPTION;
                    prod.QUANTITY = item.QUANTITY;
                    prod.RES_ID = item.RES_ID;
                    prod.ROOM_CODE = item.ROOM_CODE;
                    prod.ROOM_NAME = item.ROOM.ROOM_NAME;
                    prod.UNIT_RETAIL = item.PRODUCT.UNIT_RETAIL;
                    prod.STATUS = item.STATUS;
                    if (item.TYPE == "P")
                        productList.Add(prod);
                    else
                        amenityList.Add(prod);

                }

                //    var data = (
                //        from resMain in db.RESERVATION_MAIN.Where(x => x.RES_ID == resId)
                //        join city in db.CITies on resMain.CITY equals city.CITY_CODE
                //        join country in db.COUNTRies on resMain.COUNTRY equals country.COUNTRY_CODE
                //        join resDetail in db.RESERVATION_DETAIL on resMain.RES_ID equals resDetail.RES_ID
                //        join room in db.ROOMs on resDetail.ROOM_CODE equals room.ROOM_CODE
                //        join category in db.ROOM_CATEGORY on room.CATEGORY equals category.ROOM_CATEGORY_CODE
                //        join type in db.ROOM_TYPE on room.TYPE equals type.ROOM_TYPE_CODE
                //        join amen in db.AMENITY_PRODUCT_DETAIL on new { resDetail.RES_ID, room.ROOM_CODE } equals new { amen.RES_ID, amen.ROOM_CODE }
                //        join product in db.PRODUCTS on amen.BARCODE equals product.BARCODE
                //        select new ReservationFormReportViewModel
                //        {
                //            RES_ID = resMain.RES_ID,
                //            ADDRESS = resMain.ADDRESS,
                //            AMOUNT_PAID = resMain.AMOUNT_PAID,
                //            BALANCE = resMain.BALANCE,
                //            TOTAL_AMT = resMain.TOTAL_AMT,
                //            SUPL_CODE = resMain.SUPL_CODE,
                //            CITY = city.CITY_NAME,
                //            COMPANY = resMain.COMPANY,
                //            COUNTRY = country.COUNTRY_NAME,
                //            LAST_NAME = resMain.LAST_NAME,
                //            PHONE = resMain.PHONE,
                //            FIRST_NAME = resMain.FIRST_NAME,
                //            ID_NO = resMain.ID_NO,
                //            ID_TYPE = resMain.ID_TYPE == "C" ? "CNIC" : (resMain.ID_TYPE == "D" ? "Driving Licence" : "Other"),
                //            DISCOUNT = resMain.DISCOUNT,
                //            DOB = resMain.DOC_DATE,
                //            DOC_DATE = resMain.DOC_DATE,
                //            E_MAIL = resMain.E_MAIL,

                //            ADULTS = resDetail.ADULTS,
                //            CHECKIN_DATETIME = resDetail.CHECKIN_DATETIME,
                //            CHECKOUT_DATETIME = resDetail.CHECKOUT_DATETIME,
                //            CHILDREN = resDetail.CHILDREN,
                //            STATUS = resDetail.STATUS,
                //            TABLES_CODE = resDetail.TABLES_CODE,
                //            RATE_PER_DAY = resDetail.RATE_PER_DAY,
                //            RES_STS = resDetail.RES_STS,
                //            REVS_DAYS = resDetail.REVS_DAYS,
                //            ROOM_CATEGORY = category.NAME,
                //            ROOM_CODE = room.ROOM_CODE,
                //            ROOM_NAME = room.ROOM_NAME,
                //            ROOM_TYPE = type.NAME,
                //            SERVICES_CHARGES = resDetail.SERVICES_CHARGES,
                //            OTHER_CHARGES = resDetail.OTHER_CHARGES,
                //            GST = resDetail.GST,
                //            DETAIL_STATUS = resDetail.STATUS,
                //            DETAIL_TOTAL = resDetail.TOTAL_AMT,
                //            ESTIMATED_CHECKOUT_DATETIME = resDetail.ESTIMATED_CHECKOUT_DATETIME,

                //            QUANTITY = amen.QUANTITY,
                //            AMOUNT = amen.AMOUNT,
                //            BARCODE = amen.BARCODE,
                //            DATETIME = amen.DATETIME,
                //            DESCRIPTION = product.DESCRIPTION,
                //            ITEM_ROOM_CODE = room.ROOM_CODE,
                //            ITEM_ROOM_NAME = room.ROOM_NAME,
                //            ITEM_STATUS = amen.STATUS,
                //            UNIT_RETAIL = product.UNIT_RETAIL

                //        }).ToList();

                //var dataHistory = (
                //        from resMain in db.RESERVATION_MAIN.Where(x => x.RES_ID == resId)
                //        join city in db.CITies on resMain.CITY equals city.CITY_CODE
                //        join country in db.COUNTRies on resMain.COUNTRY equals country.COUNTRY_CODE
                //        join resDetailHistory in db.RESERVATION_DETAIL_HISTORY on resMain.RES_ID equals resDetailHistory.RES_ID
                //        join room in db.ROOMs on resDetailHistory.ROOM_CODE equals room.ROOM_CODE
                //        join category in db.ROOM_CATEGORY on room.CATEGORY equals category.ROOM_CATEGORY_CODE
                //        join type in db.ROOM_TYPE on room.TYPE equals type.ROOM_TYPE_CODE
                //        join amenHistory in db.AMENITY_PRODUCT_DETAIL_HISTORY on new { resDetailHistory.RES_ID, room.ROOM_CODE } equals new { amenHistory.RES_ID, amenHistory.ROOM_CODE }
                //        join product in db.PRODUCTS on amenHistory.BARCODE equals product.BARCODE
                //        select new ReservationFormReportViewModel
                //        {
                //            RES_ID = resMain.RES_ID,
                //            ADDRESS = resMain.ADDRESS,
                //            AMOUNT_PAID = resMain.AMOUNT_PAID,
                //            BALANCE = resMain.BALANCE,
                //            TOTAL_AMT = resMain.TOTAL_AMT,
                //            SUPL_CODE = resMain.SUPL_CODE,
                //            CITY = city.CITY_NAME,
                //            COMPANY = resMain.COMPANY,
                //            COUNTRY = country.COUNTRY_NAME,
                //            LAST_NAME = resMain.LAST_NAME,
                //            PHONE = resMain.PHONE,
                //            FIRST_NAME = resMain.FIRST_NAME,
                //            ID_NO = resMain.ID_NO,
                //            ID_TYPE = resMain.ID_TYPE == "C" ? "CNIC" : (resMain.ID_TYPE == "D" ? "Driving Licence" : "Other"),
                //            DISCOUNT = resMain.DISCOUNT,
                //            DOB = resMain.DOC_DATE,
                //            DOC_DATE = resMain.DOC_DATE,
                //            E_MAIL = resMain.E_MAIL,

                //            ADULTS = resDetailHistory.ADULTS,
                //            CHECKIN_DATETIME = resDetailHistory.CHECKIN_DATETIME,
                //            CHECKOUT_DATETIME = resDetailHistory.CHECKOUT_DATETIME,
                //            CHILDREN = resDetailHistory.CHILDREN,
                //            STATUS = resDetailHistory.STATUS,
                //            TABLES_CODE = resDetailHistory.TABLES_CODE,
                //            RATE_PER_DAY = resDetailHistory.RATE_PER_DAY,
                //            RES_STS = resDetailHistory.RES_STS,
                //            REVS_DAYS = resDetailHistory.REVS_DAYS,
                //            ROOM_CATEGORY = category.NAME,
                //            ROOM_CODE = room.ROOM_CODE,
                //            ROOM_NAME = room.ROOM_NAME,
                //            ROOM_TYPE = type.NAME,
                //            SERVICES_CHARGES = resDetailHistory.SERVICES_CHARGES,
                //            OTHER_CHARGES = resDetailHistory.OTHER_CHARGES,
                //            GST = resDetailHistory.GST,
                //            DETAIL_STATUS = resDetailHistory.STATUS,
                //            DETAIL_TOTAL = resDetailHistory.TOTAL_AMT,
                //            ESTIMATED_CHECKOUT_DATETIME = resDetailHistory.ESTIMATED_CHECKOUT_DATETIME,

                //            QUANTITY = amenHistory.QUANTITY,
                //            AMOUNT = amenHistory.AMOUNT,
                //            BARCODE = amenHistory.BARCODE,
                //            DATETIME = amenHistory.DATETIME,
                //            DESCRIPTION = product.DESCRIPTION,
                //            ITEM_ROOM_CODE = room.ROOM_CODE,
                //            ITEM_ROOM_NAME = room.ROOM_NAME,
                //            ITEM_STATUS = amenHistory.STATUS,
                //            UNIT_RETAIL = product.UNIT_RETAIL

                //        }).ToList();

                //DataList.AddRange(data);
                //DataList.AddRange(dataHistory);


                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("Visibility", "null");
                //var BgImage = new Uri(Server.MapPath("~/images/Report/" + "gb")).AbsoluteUri;
                //Parameter.Add("BgImage", BgImage);


                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", mainList);
                datasource.Add("DataSet2", detaillist);
                datasource.Add("DataSet3", productList);
                datasource.Add("DataSet4", amenityList);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0", "0", "0", "0");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        public ActionResult Journal(long resId)
        {
            try
            {
                string path = @"RPT\Reservation\GuestJournal.rdlc";
                List<JournalReportViewModel> reportData = new List<JournalReportViewModel>();
                var data = db.JOURNALs.Where(x => x.RES_ID == resId).ToList();
                foreach (var item in data)
                {
                    JournalReportViewModel Journal = new JournalReportViewModel();
                    Journal.DOC_SEQ = item.DOC_SEQ;
                    Journal.DOC_TYPE = item.DOC_TYPE;
                    Journal.DOC_DATE = item.INSERTED_DTE.Value;
                    Journal.NARRATION = item.NARRATION;
                    Journal.DEBIT = item.DEBIT_AMOUNT;
                    Journal.CREDIT = item.CREDIT_AMOUNT;
                    Journal.NARRATION = item.NARRATION;
                    reportData.Add(Journal);
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("ReportType", "Guest Ledger");
                Parameter.Add("GuestName", "ABC");
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                //var BgImage = new Uri(Server.MapPath("~/images/Report/" + "gb")).AbsoluteUri;
                //Parameter.Add("BgImage", BgImage);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", reportData);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        #endregion

        #region Invoice
        public ActionResult Invoice(string Doc_No)
        {
            try
            {
                if (string.IsNullOrEmpty(Doc_No))
                    throw new Exception("Ex");
                string path = @"RPT\Reservation\Invoice.rdlc";
                var journal = db.JOURNALs.Where(x => x.DOC_SEQ == Doc_No).SingleOrDefault();
                List<JournalReportViewModel> reportData = new List<JournalReportViewModel>();
                if (journal != null)
                {
                    reportData.Add(new JournalReportViewModel
                    {
                        AMOUNT = journal.AMOUNT,
                        AMOUNT_IN_WORDS = journal.AMOUNT_IN_WORDS,
                        BANK_NAME = journal.BANK_NAME,
                        CHEQUE_NO = journal.CHQ_NBR,
                        COMPANY = journal.COMPANY,
                        DATED = journal.REFERENCE_DTE,
                        DOC_DATE = journal.INSERTED_DTE.Value,
                        DOC_SEQ = journal.DOC_SEQ,
                        DOC_TYPE = journal.DOC_TYPE,
                        NARRATION = journal.NARRATION,
                        PARTY_CODE = journal.PARTY_CODE,
                        PAYMENT_PURPOSE = journal.PAYMENT_PURPOSE,
                        RECEIVED_FROM = journal.REFERENCE,
                        STATUS = journal.STATUS
                    });
                    Dictionary<string, string> Parameter = new Dictionary<string, string>();
                    Parameter.Add("UserName", CommonFunctions.GetUserName());
                    //Parameter.Add("ReportType", "Guest Ledger");
                    //Parameter.Add("GuestName", "ABC");
                    Parameter.Add("CompanyName", Constants.Constants.Companyname);
                    Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                    Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                    Parameter.Add("CompanyEmail", Constants.Constants.CompanyEmail);
                    var Image = new Uri(Server.MapPath("~/images/Logo/" + "Logo.jpg")).AbsoluteUri;
                    Parameter.Add("Image", Image);

                    Dictionary<string, object> datasource = new Dictionary<string, object>();
                    datasource.Add("DataSet1", reportData);


                    var repotFile = CreateReport(path, Parameter, datasource, "PDF", "5", "5", "0.5", "0.5", "0.5", "0.5");
                    return repotFile;
                }
                else
                    throw new Exception("Ex");
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        #endregion

        #region Payment&Receipt
        public ActionResult RnP(string Doc_No)
        {
            try
            {
                if (string.IsNullOrEmpty(Doc_No))
                    throw new Exception("Ex");
                string path = @"RPT\ReceiptPayment\Payment_Receipt.rdlc";
                var journal = db.JOURNALs.Where(x => x.DOC_SEQ == Doc_No).SingleOrDefault();
                List<JournalReportViewModel> reportData = new List<JournalReportViewModel>();
                if (journal != null)
                {
                    var jour = new JournalReportViewModel();
                    jour.AMOUNT = journal.AMOUNT;
                    jour.AMOUNT_IN_WORDS = journal.AMOUNT_IN_WORDS;
                    jour.BANK_NAME = journal.BANK_NAME;
                    jour.CHEQUE_NO = journal.CHQ_NBR;
                    jour.COMPANY = journal.COMPANY;
                    jour.PARTY_NAME = journal.SUPPLIER.SUPL_NAME;
                    jour.STAFF_NAME = journal.STAFF_MEMBER.SUPL_NAME;
                    jour.DATED = journal.REFERENCE_DTE;
                    jour.DOC_DATE = journal.INSERTED_DTE.Value;
                    jour.DOC_SEQ = journal.DOC_SEQ;
                    jour.DOC_TYPE = journal.V_TYPE;
                    jour.NARRATION = journal.NARRATION;
                    jour.BOOK_NO = journal.BOOK_NO;
                    jour.BILL_NO = journal.BILL_NO;
                    jour.PARTY_CODE = journal.PARTY_CODE;
                    jour.PAYMENT_PURPOSE = journal.PAYMENT_PURPOSE;
                    jour.RECEIVED_FROM = journal.REFERENCE;
                    jour.STATUS = journal.STATUS;

                    var rptName = "";
                    if (journal.V_TYPE == "CR")
                        rptName = "Cash Receipt Voucher";
                    else if (journal.V_TYPE == "CP")
                        rptName = "Cash Payment Voucher";
                    else if (journal.V_TYPE == "BR")
                        rptName = "Bank Receipt Voucher";
                    else
                        rptName = "Bank Payment Voucher";

                    reportData.Add(jour);
                    Dictionary<string, string> Parameter = new Dictionary<string, string>();
                    Parameter.Add("UserName", CommonFunctions.GetUserName());
                    //Parameter.Add("ReportType", "Guest Ledger");
                    //Parameter.Add("GuestName", "ABC");
                    Parameter.Add("CompanyName", Constants.Constants.Companyname);
                    Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                    Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                    Parameter.Add("ReportName", rptName);

                    Dictionary<string, object> datasource = new Dictionary<string, object>();
                    datasource.Add("DataSet1", reportData);

                    var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "5.83", "0.5", "0.5", "2", "2");
                    return repotFile;
                }
                else
                    throw new Exception("Ex");
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        #endregion

        #region Customer
        [Permission("Customer Balance Report")]
        public ActionResult Customer()
        {
            ViewBag.Supplier = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer).ToList();
            return View();
        }
        public ActionResult CustomerBalance(string SUPL_CODE, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Customer\CustomerBalance.rdlc";
                List<SUPPLIER> supplierList = new List<SUPPLIER>();
                List<SupplierBalance> supplierBalanceList = new List<SupplierBalance>();
                if (string.IsNullOrEmpty(SUPL_CODE))
                    supplierList = db.SUPPLIERs.Include(x => x.JOURNALs).Include(x => x.TRANS_MN).Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer).ToList();
                else
                {
                    var supplier = db.SUPPLIERs.Include(x => x.JOURNALs).Include(x => x.TRANS_MN).Where(x => x.SUPL_CODE == SUPL_CODE && x.PARTY_TYPE == Constants.CustomerSupplier.Customer).SingleOrDefault();
                    supplierList.Add(supplier);
                }

                //    var suppl = db.SUPPLIERs.Where(x=>x.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(x=> new SupplierBalance{
                //        SUPL_CODE = x.SUPL_CODE,
                //        SUPL_NAME = x.SUPL_NAME,
                //        INVOICE_NO = null,
                //        DATE = x.DOC,
                //        OPENING_BALANCE = x.OPENING ?? 0,
                //        BALANCE = x.BALANCE ?? 0,
                //        NARRATION = "Opening Balance",
                //        BALANCE_TYPE = "Opening",
                //        PAYMENT = (x.BALANCE ?? 0) + (x.OPENING ?? 0),
                //        RECEIPT = 0,
                //        NET_BALANCE = (x.BALANCE ?? 0) + (x.OPENING ?? 0)
                //}).ToList();
                //    var customerCodeList = suppl.Select(x => x.SUPL_CODE).ToList();

                //    var trans = db.TRANS_MN.Where(x=>x.TRANS_TYPE == Constants.TransType.Sales && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && customerCodeList.Contains(x.PARTY_CODE) ).Select(x=> new SupplierBalance {
                //        SUPL_CODE = x.PARTY_CODE,
                //        SUPL_NAME = x.SUPPLIER.SUPL_NAME,
                //        INVOICE_NO = x.TRANS_NO,
                //        DATE = x.TRANS_DATE,
                //        OPENING_BALANCE = 0,
                //        BALANCE = 0,
                //        NARRATION = "Sales",
                //        BALANCE_TYPE = "Sale Invoice:",
                //        PAYMENT = (x.CASH_AMT ?? 0),
                //        RECEIPT = 0,
                //        NET_BALANCE = 0
                //}).ToList();
                //    var transR = db.TRANS_MN.Where(x => x.TRANS_TYPE == Constants.TransType.SalesReturn && x.STATUS == Constants.DocumentStatus.AuthorizedDocument && customerCodeList.Contains(x.PARTY_CODE)).Select(x => new SupplierBalance
                //    {
                //        SUPL_CODE = x.PARTY_CODE,
                //        SUPL_NAME = x.SUPPLIER.SUPL_NAME,
                //        INVOICE_NO = x.TRANS_NO,
                //        DATE = x.TRANS_DATE,
                //        OPENING_BALANCE = 0,
                //        BALANCE = 0,
                //        NARRATION = "Sales",
                //        BALANCE_TYPE = "Sale Invoice:",
                //        PAYMENT = -(x.CASH_AMT ?? 0),
                //        RECEIPT = 0,
                //        NET_BALANCE = 0
                //    }).ToList();
                //    var jou = db.JOURNALs.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument && customerCodeList.Contains(x.PARTY_CODE)).Select(x => new SupplierBalance {
                //        SUPL_CODE = x.PARTY_CODE,
                //        SUPL_NAME = x.SUPPLIER.SUPL_NAME,
                //        INVOICE_NO = x.DOC_SEQ,
                //        DATE = x.TRANS_DTE,
                //        OPENING_BALANCE = 0,
                //        BALANCE = 0,
                //        NARRATION = "Receipt",
                //        BALANCE_TYPE = "Receipt:",
                //        PAYMENT = 0,
                //        RECEIPT = x.AMOUNT,
                //        NET_BALANCE = 0
                //    }).ToList();
                //    var myUnion = suppl.Union(trans).Union(transR).Union(jou).OrderBy(x=>x.DATE).ToList();

                foreach (var supplier in supplierList)
                {
                    decimal? supplierbalance = null;
                    var sup = new SupplierBalance();
                    sup.DATE = supplier.DOC;
                    sup.INVOICE_NO = null;
                    sup.NARRATION = "Opening Balance";
                    sup.BALANCE_TYPE = "Opening";
                    sup.OPENING_BALANCE = supplier.OPENING;
                    sup.BALANCE = (supplier.BALANCE ?? 0);
                    sup.PAYMENT = /*(supplier.BALANCE ?? 0) +*/ (supplier.OPENING ?? 0);
                    sup.RECEIPT = 0;
                    sup.SUPL_CODE = supplier.SUPL_CODE;
                    sup.SUPL_NAME = supplier.SUPL_NAME;
                    //sup.NET_BALANCE = /*(supplier.BALANCE ?? 0) +*/ (supplier.OPENING ?? 0);
                    //supplierbalance = (supplier.OPENING ?? 0);

                    supplierBalanceList.Add(sup);

                    foreach (var sales in supplier.TRANS_MN)
                    {
                        var sale = new SupplierBalance();
                        sale.DATE = sales.TRANS_DATE;
                        sale.INVOICE_NO = sales.TRANS_NO;

                        if (sales.TRANS_TYPE == Constants.TransType.Sales)
                        {
                            sale.NARRATION = "Sales";
                            sale.BALANCE_TYPE = "Sale Invoice:";
                            sale.PAYMENT = sales.CASH_AMT;
                        }
                        else
                        {
                            sale.NARRATION = "Sales Return";
                            sale.BALANCE_TYPE = "Sale R Invoice:";
                            sale.PAYMENT = -sales.CASH_AMT;
                        }


                        sale.RECEIPT = 0;
                        sale.SUPL_CODE = supplier.SUPL_CODE;
                        sale.SUPL_NAME = supplier.SUPL_NAME;
                        //sale.NET_BALANCE = sales.CASH_AMT + supplierbalance;
                        //supplierbalance = sales.CASH_AMT + supplierbalance;

                        supplierBalanceList.Add(sale);
                    }
                    foreach (var receipt in supplier.JOURNALs)
                    {
                        var rec = new SupplierBalance();
                        rec.DATE = receipt.TRANS_DTE;
                        rec.INVOICE_NO = receipt.DOC_SEQ;
                        rec.NARRATION = "Receipt";
                        rec.BALANCE_TYPE = "Receipt:";
                        rec.PAYMENT = 0;
                        rec.RECEIPT = receipt.AMOUNT;
                        rec.SUPL_CODE = supplier.SUPL_CODE;
                        rec.SUPL_NAME = supplier.SUPL_NAME;
                        //rec.NET_BALANCE = supplierbalance - receipt.AMOUNT;
                        //supplierbalance = supplierbalance - receipt.AMOUNT;

                        supplierBalanceList.Add(rec);
                    }
                }

                supplierBalanceList = supplierBalanceList.OrderBy(x => x.DATE).ToList();
                decimal? netBalance = null;
                Dictionary<string, decimal?> supl_netBalance = new Dictionary<string, decimal?>();
                foreach (var Balance in supplierBalanceList)
                {
                    if (supl_netBalance.ContainsKey(Balance.SUPL_CODE))
                    {
                        netBalance = Balance.NET_BALANCE = (supl_netBalance[Balance.SUPL_CODE].Value + (Balance.PAYMENT ?? 0)) - (Balance.RECEIPT ?? 0);
                        supl_netBalance[Balance.SUPL_CODE] = netBalance;
                    }
                    else
                    {
                        netBalance = Balance.NET_BALANCE = (Balance.PAYMENT ?? 0) - (Balance.RECEIPT ?? 0);
                        supl_netBalance.Add(Balance.SUPL_CODE, netBalance);
                    }

                    var d = supplierBalanceList.Where(x => x.SUPL_CODE == Balance.SUPL_CODE).ToList();
                    d.ForEach(x => x.SUPL_TOTAL_BALANCE = Balance.NET_BALANCE);
                }

                var TotalNetAmount = supl_netBalance.Sum(x => x.Value);

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("Visibility", Visibility);
                Parameter.Add("FromDate", FromDate);
                Parameter.Add("ToDate", ToDate);
                Parameter.Add("TotalNetBalance", TotalNetAmount.ToString());
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", supplierBalanceList);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        #endregion

        #region Supplier
        [Permission("Supplier Balance Report")]
        public ActionResult Supplier()
        {
            ViewBag.Supplier = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            return View();
        }
        public ActionResult SupplierBalance(string SUPL_CODE, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Supplier\SupplierBalance.rdlc";
                List<SUPPLIER> supplierList = new List<SUPPLIER>();
                List<SupplierBalance> supplierBalanceList = new List<SupplierBalance>();
                if (string.IsNullOrEmpty(SUPL_CODE))
                    supplierList = db.SUPPLIERs.Include(x => x.JOURNALs).Include(x => x.GRN_MAIN).Include(x => x.GRFS_MAIN).Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
                else
                {
                    var supplier = db.SUPPLIERs.Include(x => x.JOURNALs).Include(x => x.GRN_MAIN).Include(x => x.GRFS_MAIN).Where(x => x.SUPL_CODE == SUPL_CODE && x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).SingleOrDefault();
                    supplierList.Add(supplier);
                }

                foreach (var supplier in supplierList)
                {
                    decimal? supplierbalance = null;

                    var sup = new SupplierBalance();
                    sup.DATE = supplier.DOC;
                    sup.INVOICE_NO = null;
                    sup.NARRATION = "Opening Balance";
                    sup.BALANCE_TYPE = "Opening";
                    sup.OPENING_BALANCE = supplier.OPENING;
                    sup.BALANCE = /*(supplier.BALANCE ?? 0) + */(supplier.OPENING ?? 0);
                    sup.PAYMENT = /*(supplier.BALANCE ?? 0) + */(supplier.OPENING ?? 0);
                    sup.RECEIPT = 0;
                    sup.SUPL_CODE = supplier.SUPL_CODE;
                    sup.SUPL_NAME = supplier.SUPL_NAME;

                    //sup.NET_BALANCE = /*(supplier.BALANCE ?? 0) +*/ (supplier.OPENING ?? 0);
                    //supplierbalance = (supplier.OPENING ?? 0);

                    supplierBalanceList.Add(sup);

                    foreach (var purchase in supplier.GRN_MAIN)
                    {
                        if (purchase.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            continue;

                        var supBal = new SupplierBalance();
                        supBal.DATE = purchase.GRN_DATE;
                        supBal.INVOICE_NO = purchase.GRN_NO;
                        supBal.NARRATION = "Purchase";
                        supBal.BALANCE_TYPE = "Purchase Invoice:";
                        supBal.PAYMENT = purchase.GRN_DETAIL.Sum(x => (x.QTY * x.COST) - x.DIS_AMT);
                        supBal.RECEIPT = 0;
                        supBal.SUPL_CODE = supplier.SUPL_CODE;
                        supBal.SUPL_NAME = supplier.SUPL_NAME;

                        //supBal.NET_BALANCE = sales.CASH_AMT + supplierbalance;
                        //supplierbalance = sales.CASH_AMT + supplierbalance;

                        supplierBalanceList.Add(supBal);
                    }
                    foreach (var purchaseRetuen in supplier.GRFS_MAIN)
                    {

                        if (purchaseRetuen.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            continue;

                        var supBal = new SupplierBalance();
                        supBal.DATE = purchaseRetuen.GRF_DATE;
                        supBal.INVOICE_NO = purchaseRetuen.GRF_NO;
                        supBal.NARRATION = "Purchase Return";
                        supBal.BALANCE_TYPE = "Purchase Return Invoice:";
                        supBal.PAYMENT = -purchaseRetuen.GRFS_DETAIL.Sum(x => (x.QTY * x.COST) - x.DISCOUNT);
                        supBal.RECEIPT = 0;
                        supBal.SUPL_CODE = supplier.SUPL_CODE;
                        supBal.SUPL_NAME = supplier.SUPL_NAME;

                        //sale.NET_BALANCE = sales.CASH_AMT + supplierbalance;
                        //supplierbalance = sales.CASH_AMT + supplierbalance;

                        supplierBalanceList.Add(supBal);
                    }
                    foreach (var receipt in supplier.JOURNALs)
                    {
                        if (receipt.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            continue;

                        var rec = new SupplierBalance();
                        rec.DATE = receipt.TRANS_DTE;
                        rec.INVOICE_NO = receipt.DOC_SEQ;
                        rec.NARRATION = "Receipt";
                        rec.BALANCE_TYPE = "Receipt:";
                        rec.PAYMENT = 0;
                        rec.RECEIPT = receipt.AMOUNT;
                        rec.SUPL_CODE = supplier.SUPL_CODE;
                        rec.SUPL_NAME = supplier.SUPL_NAME;
                        //rec.NET_BALANCE = supplierbalance - receipt.AMOUNT;
                        //supplierbalance = supplierbalance - receipt.AMOUNT;
                        supplierBalanceList.Add(rec);
                    }
                }

                supplierBalanceList = supplierBalanceList.OrderBy(x => x.DATE).ToList();
                decimal? netBalance = null;
                Dictionary<string, decimal?> supl_netBalance = new Dictionary<string, decimal?>();
                foreach (var Balance in supplierBalanceList)
                {
                    if (supl_netBalance.ContainsKey(Balance.SUPL_CODE))
                    {
                        netBalance = Balance.NET_BALANCE = supl_netBalance[Balance.SUPL_CODE] + (Balance.PAYMENT ?? 0) - (Balance.RECEIPT ?? 0);
                        supl_netBalance[Balance.SUPL_CODE] = netBalance;
                    }
                    else
                    {
                        netBalance = Balance.NET_BALANCE = (Balance.PAYMENT ?? 0) - (Balance.RECEIPT ?? 0);
                        supl_netBalance.Add(Balance.SUPL_CODE, netBalance);
                    }

                    var d = supplierBalanceList.Where(x => x.SUPL_CODE == Balance.SUPL_CODE).ToList();
                    d.ForEach(x => x.SUPL_TOTAL_BALANCE = Balance.NET_BALANCE);
                }

                var TotalNetAmount = supl_netBalance.Sum(x => x.Value);

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("TotalNetBalance", TotalNetAmount.ToString());
                Parameter.Add("Visibility", Visibility);
                Parameter.Add("FromDate", FromDate);
                Parameter.Add("ToDate", ToDate);
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", supplierBalanceList);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        #endregion

        #region Staff

        #endregion

        #region Waste&Gain
        public ActionResult StockAdjustment()
        {
            ViewBag.Departmentlist = db.DEPARTMENTs.ToList();
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            return View();
        }
        public ActionResult WasteGainByDGS(string DocType, string Dept_Code, string Group_Code, string SubGroup_Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Waste\WasteGainReportByDGS.rdlc";

                List<StockAdjustmentReportModel> data = new List<StockAdjustmentReportModel>();

                data = (
               from main in db.WAST_MAIN.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                  && x.DOC_TYPE == DocType && (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo))
               join detail in db.WAST_DETAIL on main.DOC_NO equals detail.DOC_NO
               join p in db.PRODUCTS on detail.BARCODE equals p.BARCODE
               join d in db.DEPARTMENTs on p.DEPT_CODE equals d.DEPT_CODE
                   //join gr in db.GROUPS on new { p.DEPT_CODE, p.GROUP_CODE } equals new { gr.DEPT_CODE, gr.GROUP_CODE }
                   //join sgr in db.SUB_GROUPS on new { p1 = p.DEPT_CODE, p2 = p.GROUP_CODE, p3 = p.SRGOUP_CODE } equals new { p1 = sgr.DEPT_CODE, p2 = sgr.GROUP_CODE, p3 = sgr.SGROUP_CODE }
                   select new StockAdjustmentReportModel
               {
                   DocNo = main.DOC_NO,
                   DocDate = main.DOC_DATE,
                   DeptName = d.DEPT_NAME,
                   DeptCode = d.DEPT_CODE,
                       //GROUP_NAME = gr.GROUP_NAME,
                       //GROUP_CODE = gr.GROUP_CODE,
                       //SGROUP_NAME = sgr.SGROUP_NAME,
                       //SGROUP_CODE = sgr.SGROUP_CODE,
                       Barcode = p.BARCODE,
                   Description = p.DESCRIPTION,
                   Qty = detail.QTY,
                   Type = main.DOC_TYPE,
                   Cost = detail.COST,
                   Amount = detail.QTY * detail.COST,
                   }).ToList();



                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("ReportName", DocType == Constants.Constants.WastDocument ? "Waste Report" : "Gain Report");
                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");


                if (!string.IsNullOrEmpty(Dept_Code))
                    data = data.Where(x => x.DeptCode == Dept_Code).ToList();

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }
        public ActionResult WasteGainByBranch(string DocType, string Branch, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate);
                DateTo = DateTo.AddDays(1);
                string path = @"RPT\Waste\WasteGainReportByBranch.rdlc";

                List<StockAdjustmentReportModel> data = new List<StockAdjustmentReportModel>();

                data = (
               from main in db.WAST_MAIN.Include(x => x.BRANCH).Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument
                    && x.DOC_TYPE == DocType && (x.DOC_DATE >= DateFrom && x.DOC_DATE <= DateTo))
               join detail in db.WAST_DETAIL on main.DOC_NO equals detail.DOC_NO
               join p in db.PRODUCTS on detail.BARCODE equals p.BARCODE
               //join d in db.DEPARTMENTs on p.DEPT_CODE equals d.DEPT_CODE
               //join gr in db.GROUPS on new { p.DEPT_CODE, p.GROUP_CODE } equals new { gr.DEPT_CODE, gr.GROUP_CODE }
               //join sgr in db.SUB_GROUPS on new { p1 = p.DEPT_CODE, p2 = p.GROUP_CODE, p3 = p.SRGOUP_CODE } equals new { p1 = sgr.DEPT_CODE, p2 = sgr.GROUP_CODE, p3 = sgr.SGROUP_CODE }
               select new StockAdjustmentReportModel
               {
                   DocNo = main.DOC_NO,
                   DocDate = main.DOC_DATE,
                   //DeptName = d.DEPT_NAME,
                   //DeptCode = d.DEPT_CODE,
                   //GROUP_NAME = gr.GROUP_NAME,
                   //GROUP_CODE = gr.GROUP_CODE,
                   //SGROUP_NAME = sgr.SGROUP_NAME,
                   //SGROUP_CODE = sgr.SGROUP_CODE,
                   Barcode = p.BARCODE,
                   Description = p.DESCRIPTION,
                   Qty = detail.QTY,
                   Type = main.DOC_TYPE,
                   Cost = detail.COST,
                   Amount = detail.QTY * detail.COST,
                   BranchId = main.BRANCH != null ? main.BRANCH.BRANCH_CODE : "",
                   BranchName = main.BRANCH != null ? main.BRANCH.BRANCH_NAME : "",
               }).ToList();



                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("DateFrom", DateFrom.ToString("dd-MM-yyyy"));
                Parameter.Add("DateTo", DateTo.ToString("dd-MM-yyyy"));
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                Parameter.Add("ReportName", DocType == Constants.Constants.WastDocument ? "Waste Report" : "Gain Report");

                if (Visibility == "true")
                    Parameter.Add("Visibility", "True");
                else
                    Parameter.Add("Visibility", "False");


                if (!string.IsNullOrEmpty(Branch))
                    data = data.Where(x => x.BranchId == Branch).ToList();

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        #endregion

        #region Waste&Gain
        public ActionResult NegitiveStock()
        {
            ViewBag.Branch = db.BRANCHes.Select(x => new BranchViewModel { Id = x.BRANCH_CODE, Name = x.BRANCH_NAME }).ToList();

            return View();
        }
        public ActionResult NegitiveStockByBranch( string Branch)
        {
            try
            {
                string path = @"RPT\Inventory\NegitiveStock.rdlc";

                List<StockAdjustmentReportModel> data = new List<StockAdjustmentReportModel>();

                if (Branch == "04")
                {
                    data = (
               from main in db.PROD_BALANCE.Where(x => 
               ((x.GRN_QTY ?? 0) + (x.TRANSFER_IN_QTY ?? 0) + (x.GAIN_QTY ?? 0) -
               (x.GRF_QTY ?? 0) - (x.TRANSFER_OUT_QTY ?? 0) - (x.WAST_QTY ?? 0)) < 0)
               join p in db.PRODUCTS on main.BARCODE equals p.BARCODE
               select new StockAdjustmentReportModel
               {
                   Barcode = p.BARCODE,
                   Description = p.DESCRIPTION,
                   Qty = main.CURRENT_QTY ?? 0,
                   Cost = p.UNIT_COST ?? 0,
                   Amount = p.UNIT_RETAIL ?? 0,
                   BranchId =  "04",
                   BranchName = "HEAD",
               }).ToList();
                }
                else
                {



                data = (
               from main in db.PROD_LOC_REPORT.Include(x => x.BRANCH).Where(x => x.CURRENT_QTY < 0 )
               join p in db.PRODUCTS on main.BARCODE equals p.BARCODE
               select new StockAdjustmentReportModel
               {
                   Barcode = p.BARCODE,
                   Description = p.DESCRIPTION,
                   Qty = main.CURRENT_QTY ?? 0,
                   Cost = p.UNIT_COST ?? 0,
                   Amount = p.UNIT_RETAIL ?? 0,
                   BranchId = main.BRANCH != null ? main.BRANCH.BRANCH_CODE : "",
                   BranchName = main.BRANCH != null ? main.BRANCH.BRANCH_NAME : "",
               }).ToList();

                }


                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("UserName", CommonFunctions.GetUserName());
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
               

                if (!string.IsNullOrEmpty(Branch))
                    data = data.Where(x => x.BranchId == Branch).ToList();

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", data);

                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return View(ex);
            }
        }

        #endregion


        #region Order
        //[Permission("Inventory Report")]
        public ActionResult ShippindAddress(string OrderNo)
        {
            try
            {
                var sales = db.TRANS_MN.Include(x => x.PARTY_ADDRESS).Include(x => x.SUPPLIER).Where(x => x.TRANS_NO == OrderNo).SingleOrDefault();
                var ReportData = new List<ShippingDetail>();
                ShippingDetail obj = new ShippingDetail();
                if (sales.ADDRESS_ID != null)
                {
                    obj.Address = sales.PARTY_ADDRESS.ADDRESS;
                    obj.Mobile = sales.PARTY_ADDRESS.MOBILE;
                    obj.Name = sales.PARTY_ADDRESS.NAME;

                    ReportData.Add(obj);
                }
                else
                {
                    obj.Address = sales.SUPPLIER.ADDRESS;
                    obj.Mobile = sales.SUPPLIER.MOBILE;
                    obj.Name = sales.SUPPLIER.SUPL_NAME;
                    ReportData.Add(obj);
                }

                string path = "";
                path = @"RPT\Order\ShippingDetail.rdlc";

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportData);
                var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                return repotFile;
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        #endregion

        #region Common
        public JsonResult FillGroup(string id)
        {
            var group = (from a in db.GROUPS where a.DEPT_CODE == id select new { a.GROUP_CODE, a.GROUP_NAME }).ToList();
            int count = group.Count();
            var value = JsonConvert.SerializeObject(@group, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public JsonResult FillSubGroup(string id)
        {
            var subGroup = (from a in db.SUB_GROUPS where a.DEPT_CODE == id.Substring(0, 2) && a.GROUP_CODE == id.Substring(2, 2) select new { a.SGROUP_CODE, a.SGROUP_NAME }).ToList();
            var value = JsonConvert.SerializeObject(subGroup, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public FileResult CreateReport(string reportPath, Dictionary<string, string> ParametersNameValue, Dictionary<string, object> dataSourceNameValue, string rType, string PageWidth, string PageHeight, string MarginTop, string MarginBottom, string MarginLeft, string MarginRight)
        {
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = Server.MapPath("~/" + reportPath);
            localReport.EnableExternalImages = true;

            if (ParametersNameValue != null)
            {
                var index = 0;
                ReportParameter[] Par = new ReportParameter[ParametersNameValue.Count];
                foreach (var parameter in ParametersNameValue)
                {
                    Par[index] = new ReportParameter(parameter.Key, parameter.Value, false);
                    index++;
                }
                localReport.SetParameters(Par);
            }

            if (dataSourceNameValue != null)
            {
                foreach (var datasourceInfo in dataSourceNameValue)
                {
                    ReportDataSource RDS = new ReportDataSource();
                    RDS.Name = datasourceInfo.Key;
                    RDS.Value = datasourceInfo.Value;
                    localReport.DataSources.Add(RDS);
                }
            }


            string reportType = rType;
            string mimeType;
            string encoding;
            string fileNameExtension;
            Warning[] warnings;
            string[] stream;
            byte[] renderByte;

            if (reportType == "Excel")
                fileNameExtension = "xlsx";

            if (reportType == "Word")
                fileNameExtension = "docx";

            if (reportType == "PDF")
                fileNameExtension = "pdf";

            if (reportType == "Image")
                fileNameExtension = "image";
            localReport.Refresh();
            string deviceInfo =

                "<DeviceInfo>" +
                //   "<Orientation>Landscape</Orientation>"
                "  <OutputFormat>" + reportType + "</OutputFormat>" +
                "  <PageWidth>" + PageWidth + "in</PageWidth>" +
                "  <PageHeight>" + PageHeight + "</PageHeight>" +
                "  <MarginTop>" + MarginTop + "in</MarginTop>" +
                "  <MarginLeft>" + MarginLeft + "in</MarginLeft>" +
                "  <MarginRight>" + MarginRight + "in</MarginRight>" +
                "  <MarginBottom>" + MarginBottom + "in</MarginBottom>" +
                "</DeviceInfo>";


            renderByte = localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out stream, out warnings);

            return File(renderByte, mimeType);

        }
        #endregion

        [Permission("Supplier Recovery,Customer Recovery")]
        public ActionResult CSSRecovery(string ViewName)
        {
            ViewBag.Staff = db.STAFF_MEMBER.Select(x => new { x.SUPL_CODE, x.SUPL_NAME }).ToList();
            ViewBag.Customer = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(x => new { x.SUPL_CODE, x.SUPL_NAME }).ToList();
            ViewBag.Supplier = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).Select(x => new { x.SUPL_CODE, x.SUPL_NAME }).ToList();
            return View(ViewName);
        } // CUSTOMER_SUPPLIER_STAFF_BALANCE
        public ActionResult Customer_Supplier_Staff_Recovery(string Query, string Code, string FromDate, string ToDate, string Visibility)
        {
            try
            {
                string TodayDate = Convert.ToString(DateTime.Now).Substring(0, 10);
                DateTime DateFrom = Convert.ToDateTime(FromDate);
                DateTime DateTo = Convert.ToDateTime(ToDate).AddDays(1);
                List<JOURNAL> list = new List<JOURNAL>();
                if (string.IsNullOrEmpty(Query))
                    throw new Exception("Ex");
                if (Query == "CustomerStaffDocumentWise")
                    list = db.JOURNALs.Where(x => x.DOC_TYPE == "R" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                else if (Query == "CustomerWise")
                {
                    if (string.IsNullOrEmpty(Code))
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "R" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                    else
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "R" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.PARTY_CODE == Code && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                }
                else if (Query == "CustomerStaffWise")
                {
                    if (string.IsNullOrEmpty(Code))
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "R" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                    else
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "R" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.STAFF_CODE == Code && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                }
                else if (Query == "SupplierStaffDocumentWise")
                    list = db.JOURNALs.Where(x => x.DOC_TYPE == "P" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                else if (Query == "SupplierWise")
                {
                    if (string.IsNullOrEmpty(Code))
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "P" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                    else
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "P" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.PARTY_CODE == Code && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                }
                else if (Query == "SupplierStaffWise")
                {
                    if (string.IsNullOrEmpty(Code))
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "P" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                    else
                        list = db.JOURNALs.Where(x => x.DOC_TYPE == "P" & x.STATUS == Constants.DocumentStatus.AuthorizedDocument && x.STAFF_CODE == Code && x.TRANS_DTE >= DateFrom && x.TRANS_DTE <= DateTo).ToList();
                }

                string path = @"RPT\Customer_Supplier_Staff_Recovery.rdlc";
                List<JournalReportViewModel> reportData = new List<JournalReportViewModel>();

                if (list != null)
                {
                    foreach (var item in list)
                    {
                        JournalReportViewModel journal = new JournalReportViewModel();

                        journal.DOC_DATE = item.TRANS_DTE.Value;
                        journal.DOC_SEQ = item.DOC_SEQ;
                        journal.PARTY_CODE = item.PARTY_CODE;
                        journal.PARTY_NAME = db.SUPPLIERs.Where(x => x.SUPL_CODE == item.PARTY_CODE).Select(x => x.SUPL_NAME).SingleOrDefault();
                        journal.STAFF_NAME = db.STAFF_MEMBER.Where(x => x.SUPL_CODE == item.STAFF_CODE).Select(x => x.SUPL_NAME).SingleOrDefault();
                        journal.BILL_NO = item.BILL_NO;
                        journal.AMOUNT = item.AMOUNT;
                        //journal.AMOUNT_IN_WORDS = item.AMOUNT_IN_WORDS;
                        //journal.BANK_NAME = item.BANK_NAME;
                        //journal.CHEQUE_NO = item.CHQ_NBR;
                        //journal.COMPANY = item.COMPANY;
                        //journal.DATED = item.REFERENCE_DTE;
                        //journal.DOC_TYPE = item.DOC_TYPE;
                        //journal.NARRATION = item.NARRATION;
                        //journal.PAYMENT_PURPOSE = item.PAYMENT_PURPOSE;
                        //journal.RECEIVED_FROM = item.REFERENCE;
                        //journal.STATUS = item.STATUS;
                        reportData.Add(journal);
                    }
                    Dictionary<string, string> Parameter = new Dictionary<string, string>();
                    Parameter.Add("UserName", CommonFunctions.GetUserName());
                    Parameter.Add("ReportType", "Salesman Recovery");
                    Parameter.Add("CompanyName", Constants.Constants.Companyname);
                    Parameter.Add("Visibility", Visibility);
                    //Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);
                    //Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                    //Parameter.Add("CompanyEmail", Constants.Constants.CompanyEmail);

                    Dictionary<string, object> datasource = new Dictionary<string, object>();
                    datasource.Add("DataSet1", reportData);

                    var repotFile = CreateReport(path, Parameter, datasource, "PDF", "8.27", "11.69", "0.5", "0.3", "0.3", "0.3");
                    return repotFile;
                }
                else
                    throw new Exception("Ex");
            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }
        [HttpPost]
        public void SaveData(IEnumerable<BarcodeReport> RptData)
        {
            try
            {
                Session["RptData"] = RptData;
            }
            catch (Exception ex)
            {
                Content(ex.ToString());
            }
        }
        public ActionResult GenerateBarcode()
        {
            try
            {
                var data = (Session["RptData"]) as List<BarcodeReport>;
                string path = @"RPT\Barcode\PerRow3-h1w1p5.rdlc";
                List<BarcodeReport> ReportList = new List<BarcodeReport>();

                foreach (var item in data)
                {
                    var retail = db.PRODUCTS.Where(x => x.BARCODE == item.Barcode).Select(x => x.UNIT_RETAIL).SingleOrDefault();
                    BarcodeReport RptData = new BarcodeReport();
                    int i = 0;
                    while (i < item.Qty)
                    {
                        Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        RptData.Img = ImageToByte(barcode.Draw(item.Barcode, 50));
                        RptData.Name = item.Name.ToLower();
                        RptData.Barcode = item.Barcode;
                        RptData.Price = retail.ToString();
                        ReportList.Add(RptData);
                        i = i + 3;
                    }
                }

                Dictionary<string, string> Parameter = new Dictionary<string, string>();
                Parameter.Add("CompanyName", Constants.Constants.Companyname);
                //Parameter.Add("UserName", CommonFunctions.GetUserName());
                //Parameter.Add("CompanyPhone", Constants.Constants.CompanyPhone);
                //Parameter.Add("CompanyAddress", Constants.Constants.CompanyAddress);

                Dictionary<string, object> datasource = new Dictionary<string, object>();
                datasource.Add("DataSet1", ReportList);
                Session["RptData"] = null;
                return CreateReport(path, Parameter, datasource, "PDF", "4.5", "1.1", "0.0", "0.0", "0.0", "0.0");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }





















    }
}

//units_sold = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.UNITS_SOLD : -x.UNITS_SOLD),
//cost_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT : -(x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT),
//retail_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? x.AMOUNT : -x.AMOUNT),
//disc_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) : -((x.UNITS_SOLD * x.UNIT_RETAIL) * (x.DIS_AMOUNT / 100)) ?? 0),
//net_amount = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? (x.AMOUNT) : -(x.AMOUNT)) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100)) : -((x.AMOUNT) * (x.TRANS_MN.DISC_RATE / 100))) ?? 0,
//net_margin = g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNIT_RETAIL) * (x.UNITS_SOLD)) : -((x.UNIT_RETAIL) * (x.UNITS_SOLD))) - g.Sum(x => x.TRANS_MN.TYPE == "A" && x.EXCH_FLAG == "T" ? ((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT) : -((x.UNITS_SOLD * x.UNIT_COST) + x.GST_AMOUNT))
