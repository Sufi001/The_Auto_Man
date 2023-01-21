using CustomerRec;
using Inventory.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Inventory.Constants;
using System.Web.Security;
using Inventory.Helper;
using Inventory.ViewModel;
using Inventory.ViewModels.Item;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [Authorize]
    public class RestaurantController : Controller
    {
        private readonly DataContext db;
        public RestaurantController()
        {
            db = new DataContext();
        }
        [Permission("Sales,Sales Return,Booking,Booking Return,Order Booking")]
        public ActionResult Index(string docType, string requestPage)
        {

            try
            {
                //request page may be Index or Booking
                requestPage = requestPage == SalesPage.Index ? SalesPage.Index : (requestPage == SalesPage.Booking ? SalesPage.Booking : SalesPage.OrderBooking);
                docType = docType == Constants.Constants.SalesDocument ? Constants.Constants.SalesDocument : Constants.Constants.SalesReturnDocument; // Checking For for DocType => Sales or Sales Return
                var ViewModel = GetSalesPageViewModel(docType, requestPage); //Model For Index(Sales and Sales Return) and Booking(Sales and Sales Return)
                return View(requestPage, ViewModel);

            }
            catch (Exception ex)
            {
                string message = string.Format("<b>Message:</b> {0}<br /><br />", ex.Message);
                message += string.Format("<b>StackTrace:</b> {0}<br /><br />", ex.StackTrace.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>Source:</b> {0}<br /><br />", ex.Source.Replace(Environment.NewLine, string.Empty));
                message += string.Format("<b>TargetSite:</b> {0}", ex.TargetSite.ToString().Replace(Environment.NewLine, string.Empty));
                return Content(message);
            }
        }
        public DocumentViewModel GetSalesPageViewModel(string docType, string pageRequest = "")
        {
            //Prepare ViewModel for Both View Index(Sales and Sales Return from index page) and Booking(Sales and Sales Return from Booking page)
            DocumentMainViewModel obj = new DocumentMainViewModel();
            obj.DocDate = DateTime.Now;
            obj.DocType = docType;
            obj.RequestPage = pageRequest;
            obj.Location = CommonFunctions.GetLocation();

            if (pageRequest == SalesPage.OrderBooking)
                obj.DocNoDisplay = obj.GetDNCode();
            else
                obj.DocNoDisplay = obj.GetSaleCode(obj.DocType, 6);

            var viewmodel = new DocumentViewModel
            {
                DocumentMain = obj,
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME, SupplierUrduName = u.NAME_URDU, categorycode = u.DISC_CATEGORY_ID }).OrderBy(x => x.SupplierName).ToList(),
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION, UrduName = u.URDU }).ToList(),
                //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
                //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "S").OrderBy(x=>x.DESCRIPTION).Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
                //ColourList = db.COLOURS.ToList(),
                Stafflist = db.STAFF_MEMBER.ToList(),
                //Onlinebooking = obj.getOnlineBooking(),
                DisountCategorylist = db.CUSTOMER_DISCOUNT_CATEGORY.ToList(),
                LocationList = db.LOCATIONs.ToList()
            };
            return viewmodel;
        }
        public JsonResult GenerateBarcode(string DocType)
        {
            DocumentMainViewModel obj = new DocumentMainViewModel();
            string list = ""; //=  obj.GetSaleCode("IN",6,"1");
            if (DocType == "IN")
            {
                list = obj.GetSaleCode("IN", 6);
            }
            else
            {
                list = obj.GetSaleCode("IR", 6);
            }
            var value = JsonConvert.SerializeObject(list, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult List(string docType, string requestPage)
        {
            //Prepare List View for Both Pages List(sales List and Sales Return List) and BookingList(Booking List and Booking Return List)
            List<DocumentListViewModel> obj = new List<DocumentListViewModel>();
            requestPage = requestPage == Constants.SalesPage.Index ? Constants.SalesPage.Index : Constants.SalesPage.Booking;

            var TransType = docType == Constants.Constants.SalesDocument ? Constants.TransType.Sales : Constants.TransType.SalesReturn; //Checking For for DocType => Sales return "2" or Sales Return return "4" 

            var ViewModel = GetSalesList(TransType, requestPage);

            ViewBag.requestPage = requestPage;

            ViewBag.doctype = docType;

            return View(ViewModel);
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string docno, string requestPage)
        {
            //Will find object for both views Index and Booking based on requestPage Parameter
            var salesMaininDb = db.TRANS_MN.SingleOrDefault(c => c.TRANS_NO == docno);
            if (salesMaininDb == null)
                return HttpNotFound("Page not found. Bad Request");

            var salesDetail = db.TRANS_DT.Include(x => x.PRODUCT).Where(c => c.TRANS_NO == docno).Select(u => new DocumentDetailViewModel
            {
                Description = u.PRODUCT.DESCRIPTION,
                Uanno = u.PRODUCT.UAN_NO,
                CtnPcs = u.PRODUCT.CTN_PCS,
                Barcode = u.BARCODE,
                CtnQty = u.CTN_QTY,
                Retail = u.UNIT_RETAIL,
                ProductUnitRetail = u.PRODUCT.UNIT_RETAIL,
                ProductPackRetail = u.PRODUCT.PACK_RATE,
                ProductPackQty = u.PRODUCT.PACK_QTY,
                DateTimeSlot = u.S_TIME,
                staffcode = u.SUPL_CODE,
                Qty = u.UNITS_SOLD,
                FreeQty = u.FREE_QTY,
                Discount = (((u.DIS_AMOUNT ?? 0) * u.UNIT_RETAIL) / 100) * u.UNITS_SOLD,
                Amount = ((u.UNIT_RETAIL * (u.UNITS_SOLD) - (((u.DIS_AMOUNT ?? 0) * u.UNIT_RETAIL) / 100) * u.UNITS_SOLD))
            }).ToList();

            var docMain = new DocumentMainViewModel();
            docMain.DocDate = salesMaininDb.TRANS_DATE;
            docMain.DocNo = salesMaininDb.TRANS_NO;

            docMain.RequestPage = requestPage;

            if (requestPage == "WebStore")
                requestPage = "Index";


            docMain.Print = "True";
            docMain.DocNoDisplay = salesMaininDb.TRANS_NO;
            docMain.Payment = salesMaininDb.BY_CASH.ToString();
            docMain.Advance = salesMaininDb.ADVANCE.ToString();
            docMain.Phone = salesMaininDb.MOB;
            //docMain.staffcode = db.STAFF_MEMBER.Where(x => x.SUPL_CODE == docMain.staffmemberid).Select(u => u.SUPL_NAME).SingleOrDefault();
            docMain.staffcode = salesMaininDb.SALESMAN_CDE;
            docMain.DocType = (salesMaininDb.TRANS_TYPE == Constants.TransType.Sales) ? Constants.Constants.SalesDocument : Constants.Constants.SalesReturnDocument;
            docMain.Location = CommonFunctions.GetLocation();
            // docMain.Warehouse = salesMaininDb.Warehouse;
            docMain.SuplCode = salesMaininDb.PARTY_CODE;
            docMain.Status = salesMaininDb.STATUS;
            docMain.ReturnAmount = Convert.ToString(salesMaininDb.RET_AMT);
            var viewmodel = new DocumentViewModel
            {
                ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION, UrduName = u.URDU, Ctnpcs = u.CTN_PCS }).ToList(),
                SupplierList = db.SUPPLIERs.Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME, SupplierUrduName = u.NAME_URDU }).ToList(),
                DocumentMain = docMain,
                Stafflist = db.STAFF_MEMBER.ToList(),
                DocumentDetailList = salesDetail,
                LocationList = db.LOCATIONs.ToList(),
            };
            return View(requestPage, viewmodel);
        }
        public List<DocumentListViewModel> GetSalesList(string transType, string salesPage)
        {
            if (salesPage == "Index")
            {
                DataContext context = new DataContext();
                var list = (from salesMain in context.TRANS_MN
                            from supplier in context.SUPPLIERs.Where(x => x.SUPL_CODE == salesMain.PARTY_CODE).DefaultIfEmpty()
                            from location in context.LOCATIONs.Where(x => x.LOC_ID == salesMain.LOC_ID).DefaultIfEmpty()
                            where salesMain.TRANS_TYPE == transType && (salesMain.SALE_TYPE == "Index" || (salesMain.SALE_TYPE == "WebStore" && salesMain.STATUS == Constants.DocumentStatus.Dispatch)) //&& salesDetail.TRANS_TYPE == transType
                            select new
                            {
                                Doc = salesMain.TRANS_DATE,
                                loc = location.NAME,
                                DocNo = salesMain.TRANS_NO,
                                supplierName = supplier.SUPL_NAME,
                                //Warehouse = war.Name,
                                //amount = Amount,//(transferDetail.cost * transferDetail.qty),
                                amount = salesMain.CASH_AMT,//(transferDetail.cost * transferDetail.qty),
                                Status = salesMain.STATUS
                            }).ToList().OrderByDescending(x => x.DocNo);

                return list.Select(item => new DocumentListViewModel
                {
                    DocNo = item.DocNo,
                    Doc = item.Doc,
                    Supplier = item.supplierName,
                    Location = item.loc,
                    Amount = item.amount ?? 0,
                    //Location = 
                    Status = (item.Status == Constants.DocumentStatus.UnauthorizedDocument) ? "Unauthorized" :
                    (item.Status == Constants.DocumentStatus.AuthorizedDocument) ? "Authorized" :
                    (item.Status == Constants.DocumentStatus.Cancelled) ? "Cancelled" :
                    (item.Status == Constants.DocumentStatus.Completed) ? "Completed" :
                    (item.Status == Constants.DocumentStatus.Deleted) ? "Deleted" :
                    (item.Status == Constants.DocumentStatus.Dispatch) ? "Dispatch" :
                    (item.Status == Constants.DocumentStatus.Pending) ? "Pending" :
                    (item.Status == Constants.DocumentStatus.Processing) ? "Processing" :
                    "Unknown"
                }).ToList();
            }
            else
            {
                DataContext context = new DataContext();
                var list = (from salesMain in context.TRANS_MN
                            from supplier in context.SUPPLIERs.Where(x => x.SUPL_CODE == salesMain.PARTY_CODE).DefaultIfEmpty()
                            from location in context.LOCATIONs.Where(x => x.LOC_ID == salesMain.LOC_ID).DefaultIfEmpty()
                            where salesMain.TRANS_TYPE == transType && salesMain.SALE_TYPE == salesPage //&& salesDetail.TRANS_TYPE == transType
                            select new
                            {
                                Doc = salesMain.START_TIME,
                                loc = location.NAME,
                                DocNo = salesMain.TRANS_NO,
                                supplierName = supplier.SUPL_NAME,
                                //Warehouse = war.Name,
                                //amount = Amount,//(transferDetail.cost * transferDetail.qty),
                                amount = salesMain.CASH_AMT,//(transferDetail.cost * transferDetail.qty),
                                Status = salesMain.STATUS
                            }).ToList().OrderByDescending(x => x.DocNo);

                return list.Select(item => new DocumentListViewModel
                {
                    DocNo = item.DocNo,
                    Doc = item.Doc,
                    Supplier = item.supplierName,
                    Location = item.loc,
                    Amount = item.amount ?? 0,
                    //Location = 
                    Status = (item.Status == "0") ? "Unauthorized" : "Authorized"
                }).ToList();
            }




        }
        public JsonResult GetEvents(string docType)
        {
            var Trans_Type = "";
            if (docType == Constants.Constants.SalesDocument)
                Trans_Type = Constants.TransType.Sales;
            else
                Trans_Type = Constants.TransType.SalesReturn;

            //var model = (from salesMain in db.TRANS_MN
            //                 //  join salesDetail in db.TRANS_DT
            //                 //  on salesMain.TRANS_NO equals salesDetail.TRANS_NO
            //             join supplier in db.SUPPLIERs
            //             on salesMain.party_code equals supplier.SUPL_CODE
            //             //  join product in db.PRODUCTS
            //             //   on salesDetail.BARCODE equals product.BARCODE
            //             //   join location in db.Login_Location
            //             // on salesMain.LOC_ID equals location.Log_Loc_Id
            //             join discountcategory in db.CUSTOMER_DISCOUNT_CATEGORY.DefaultIfEmpty()
            //          on supplier.DISC_CATEGORY_ID equals discountcategory.CATEGORY_ID
            //             //   into dis  from discountcategory in dis.DefaultIfEmpty()
            //             where salesMain.TRANS_TYPE != "4"
            //             select new
            //             {
            //                 SupplierName = supplier.SUPL_NAME,
            //                 SupplierId = supplier.SUPL_CODE,
            //                 //  SupplierContactNo = supplier.MOBILE,
            //                 DocumentDate = salesMain.TRANS_DATE,
            //                 //   SupplierAddress = supplier.ADDRESS,
            //                 SupplierPhoneNo = salesMain.mob,
            //                 DocumentNo = salesMain.TRANS_NO,
            //                 Status = salesMain.status,
            //                 Doctype = salesMain.TRANS_TYPE,
            //                 Time = salesMain.Time_Slot,
            //                 ReturnAmount = salesMain.ret_Amt ?? 0,
            //                 //    ItemCode = salesDetail.BARCODE,
            //                 //    ItemName = product.DESCRIPTION,
            //                 //   Qty = salesDetail.UNITS_SOLD,
            //                 //  Rate = salesDetail.UNIT_RETAIL,
            //                 // Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
            //                 PartyType = supplier.party_type,
            //                 // Location = location.Log_Loc_Name,
            //                 //  LocationId=location.Log_Loc_Id
            //                 //    Username = username,
            //                 //   UanNo = product.UAN_NO,
            //                 //   Discount = salesDetail.dis_amount ?? 0,
            //                 Advance = salesMain.advance,
            //                 Discountcategorydiscription = discountcategory.DESCRIPTION != null ? discountcategory.DESCRIPTION : "",
            //                 DiscountCategoryId = discountcategory.CATEGORY_ID != 0 ? discountcategory.CATEGORY_ID : 0,
            //                 CustomerDiscount = discountcategory.DISCOUNT != 0 ? discountcategory.DISCOUNT : 0,
            //                 Cancel = salesMain.CANCEL,
            //                 Payment = salesMain.BY_CASH

            //             }).ToList();
            //var list = model.GroupBy(x => x.DocumentNo).Select(x => x.FirstOrDefault()).ToList();

            var list = (
                            from salesDetail in db.TRANS_DT
                            join salesMain in db.TRANS_MN
                            on salesDetail.TRANS_NO equals salesMain.TRANS_NO
                            join supplier in db.SUPPLIERs
                            on salesMain.PARTY_CODE equals supplier.SUPL_CODE
                            //join discountcategory in db.CUSTOMER_DISCOUNT_CATEGORY.DefaultIfEmpty()
                            //on supplier.DISC_CATEGORY_ID equals discountcategory.CATEGORY_ID
                            where salesMain.TRANS_TYPE == Trans_Type && salesMain.SALE_TYPE == "Booking"
                            select new
                            {
                                SupplierName = supplier.SUPL_NAME,
                                SupplierId = supplier.SUPL_CODE,
                                //DocumentDate = salesMain.TRANS_DATE,
                                DocumentDate = salesDetail.S_TIME,
                                SupplierPhoneNo = salesMain.MOB,
                                DocumentNo = salesMain.TRANS_NO,
                                Status = salesMain.STATUS,
                                Doctype = salesMain.TRANS_TYPE,
                                Time = salesMain.TIME_SLOT,
                                ReturnAmount = salesMain.RET_AMT ?? 0,
                                PartyType = supplier.PARTY_TYPE,
                                Advance = salesMain.ADVANCE,
                                //Discountcategorydiscription = discountcategory.DESCRIPTION != null ? discountcategory.DESCRIPTION : "",
                                //DiscountCategoryId = discountcategory.CATEGORY_ID != 0 ? discountcategory.CATEGORY_ID : 0,
                                //CustomerDiscount = discountcategory.DISCOUNT != 0 ? discountcategory.DISCOUNT : 0,
                                Cancel = salesMain.CANCEL,
                                Payment = salesMain.BY_CASH

                            }).ToList();
            list = list.OrderBy(x => x.DocumentDate.Value.Hour).ToList();


            string value = String.Empty;
            value = JsonConvert.SerializeObject(list, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        //     public ActionResult PrintSalesReport(string id)
        //     {
        //         //CommonFunctions obj1 = new CommonFunctions();
        //         //if (obj1.Authenticate() == false)
        //         //{
        //         //    return RedirectToAction("Login", "Account");
        //         //}
        //         string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //         HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //         FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //         string UserName = ticket.Name;
        //         var username = UserName;//Session["username"].ToString();
        //         string companyAddress;
        //         string companyPhone;
        //         string LocationId = null;
        //         if (Request.Cookies["Location"] != null)
        //             LocationId = Request.Cookies["Location"].Value;


        //         //if (Session["location"].ToString() == "01")

        //         if (LocationId == "01")
        //         {
        //             companyAddress = Configuration.KarachiAddress;
        //             companyPhone = Configuration.KarachiPhone;
        //         }
        //         else
        //         {
        //             companyAddress = Configuration.LahoreAddress;
        //             companyPhone = Configuration.LahorePhone;
        //         }

        //         var model = (from salesMain in db.TRANS_MN
        //                      join salesDetail in db.TRANS_DT
        //                      on salesMain.TRANS_NO equals salesDetail.TRANS_NO
        //                      join supplier in db.SUPPLIERs
        //                      on salesMain.party_code equals supplier.SUPL_CODE
        //                      join product in db.PRODUCTS
        //                      on salesDetail.BARCODE equals product.BARCODE
        //                      join location in db.LOCATIONs
        //                      on salesMain.LOC_ID equals location.LOC_ID
        //                      select new
        //                      {
        //                          SupplierName = supplier.SUPL_NAME,
        //                          SupplierContactNo = supplier.MOBILE,
        //                          DocumentDate=salesMain.TRANS_DATE,
        //                          SupplierAddress = supplier.ADDRESS,
        //                          SupplierPhoneNo = supplier.PHONE,
        //                          DocumentNo = salesMain.TRANS_NO,
        //                          ItemCode = salesDetail.BARCODE,
        //                          ItemName = product.DESCRIPTION,
        //                          Qty = salesDetail.UNITS_SOLD,
        //                          Rate = salesDetail.UNIT_RETAIL,
        //                          Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.DIS_AMOUNT ?? 0,
        //                          PartyType = supplier.PARTY_TYPE,
        //                          Location = location.NAME,
        //                          Username = username,
        //                          Discount = salesDetail.DIS_AMOUNT ?? 0
        //                      }).ToList();

        //         var modelList = model.Where(u => u.DocumentNo == id && u.PartyType == Constants.CustomerSupplier.Customer).ToList();

        //         var viewmodel = new SalesInvoiceReportViewModel();
        //         var company = new CompanyViewModel();
        //         company.CompanyName = Configuration.CompanyName;
        //         company.CompanyAddress = companyAddress;
        //         company.CompanyContactNo = companyPhone;
        //         viewmodel.Company = company;
        //         var reportMain = new ReportMainViewModel();
        //         var reportDetailList = new List<ReportDetailViewModel>();
        //         foreach (var item in modelList)
        //         {
        //             reportMain.DocumentNo = item.DocumentNo;
        //             reportMain.SupplierName = item.SupplierName;
        //             reportMain.SupplierAddress = item.SupplierAddress;
        //             reportMain.SupplierContactNo = item.SupplierContactNo;
        //             reportMain.SupplierPhoneNo = item.SupplierPhoneNo;
        //             reportMain.Username = item.Username;
        //             reportMain.Location = item.Location;
        //             var reportDetail = new ReportDetailViewModel();
        //             reportDetail.ItemCode = item.ItemCode;
        //             reportDetail.ItemName = item.ItemName;
        //             reportDetail.Qty = item.Qty;
        //             reportDetail.Rate = item.Rate;
        //             reportDetail.Amount = item.Amount;
        //             reportDetail.ItemCode = item.ItemCode;
        //             reportDetail.PartyType = item.PartyType;
        //            // reportDetail.Warehouse = item.Warehouse;
        //             reportDetail.Discount = item.Discount;
        //             reportDetailList.Add(reportDetail);
        //         }
        //         reportMain.PartType = "Customer Name : ";
        //         reportMain.DocumentName = "Invoice";
        //         viewmodel.ReportMain = reportMain;
        //         viewmodel.ReportDetailList = reportDetailList;
        //         return View("SalesReport", viewmodel);
        //     }
        //     [HttpGet]
        //     public ActionResult PrintSaleDocumentreport(string Doc_Number,string Doc_Type,string btn)
        //     {
        //         try
        //         {

        //                var Date1 = DateTime.Now.Date;//.ToString("yyyy-MM-dd");
        //             string id = "PDF";
        //             var nextday1 = Date1.AddDays(1);
        //            var Date=Date1.ToString("yyyy-MM-dd");
        //             var nextday = nextday1.ToString("yyyy-MM-dd");

        //             LocalReport lr = new LocalReport();
        //             MyConnection conn = new MyConnection();
        //             //            var modelList  = conn.Select(@"SELECT isnull(DEPARTMENT.DEPT_NAME,'NAN' )  DEPT_NAME,
        //             //     isnull(GROUPS.GROUP_NAME,'NAN')      GROUP_NAME,
        //             //     isnull(SUB_GROUPS.SGROUP_NAME,'NAN') SGROUP_NAME,
        //             //     PRODUCTS.BARCODE,
        //             //     PRODUCTS.DESCRIPTION,
        //             //(select count(price.TRANS_NO) from PRICE_overide_log
        //             // join PRICE_overide_log  price on TRANS_MN.TRANS_NO=price.TRANS_NO
        //             //  where convert(date, price.PRICE_DATE, 112) between '"+Date+"' and '"+nextday+"') as priceoverridecount, (select isnull(Sum(price.UNIT_RETAIL),0) from PRICE_overide_log join PRICE_overide_log  price on TRANS_MN.TRANS_NO=price.TRANS_NO where convert(date, price.PRICE_DATE, 112) between '"+Date+"' and '"+nextday+"') as priceoverridecount,(select count(TRANS_DT.VOId) from TRANS_MN join TRANS_DT on TRANS_MN.TRANS_NO=TRANS_DT.TRANS_NO  where convert(date, TRANS_MN.TRANS_DATE, 112) between '"+Date+"' and '"+nextday+"' and TRANS_DT.VOID='T') 	as ItemsVoidCount,(select Round(Sum(isnull(TRANS_DT.UNIT_RETAIL,0)*isnull(TRANS_DT.UNITS_SOLD,0)-TRANS_DT.dis_amount),0) from TRANS_MN join TRANS_DT on TRANS_MN.TRANS_NO=TRANS_DT.TRANS_NO where convert(date, TRANS_MN.TRANS_DATE, 112) between '"+Date+"' and '"+nextday+"' and TRANS_DT.VOID='T') as VoidAmount,sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then units_sold Else -units_sold end) end) units_sold,round(sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then (units_sold*TRANS_DT.UNIT_COST)+ TRANS_DT.GST_AMOUNT ELSE -(units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT end) end),2) cost_amount,round(sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then (amount ) Else  -abs(amount ) end) end),0)  retail_amount,isnull(round(sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then units_sold*TRANS_DT.UNIT_RETAIL*(dis_amount/100) ELSE -abs(units_sold*TRANS_DT.UNIT_RETAIL)*(dis_amount/100) end) end),2),0) disc_amount,round(sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then (amount )Else -abs(amount ) end) end),2)- isnull(sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T'Then (amount)*(trans_mn.disc_rate/100)ELSE -abs (amount*(trans_mn.disc_rate/100)) end) end),0) net_amount, round( sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then (TRANS_DT.unit_retail * TRANS_DT.units_sold ) Else -abs(TRANS_DT.unit_retail * TRANS_DT.units_sold) end) end)- sum(CASE TRANS_MN.TYPE When 'A' Then (CASE TRANS_DT.EXCH_FLAG When 'T' Then (units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT ELSE -(units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT end)end),2) net_margin FROM PRODUCTS,TRANS_MN,TRANS_DT,DEPARTMENT,GROUPS,PRICE_overide_log,SUB_GROUPS WHERE TRANS_MN.TRANS_NO = TRANS_DT.TRANS_NO AND  TRANS_DT.BARCODE = PRODUCTS.BARCODE and ( GROUPS.DEPT_CODE = DEPARTMENT.DEPT_CODE ) and  ( SUB_GROUPS.DEPT_CODE = GROUPS.DEPT_CODE ) and  ( SUB_GROUPS.GROUP_CODE = GROUPS.GROUP_CODE ) and  ( PRODUCTS.SRGOUP_CODE = SUB_GROUPS.SGROUP_CODE ) and ( PRODUCTS.GROUP_CODE = SUB_GROUPS.GROUP_CODE ) and  ( PRODUCTS.DEPT_CODE = SUB_GROUPS.DEPT_CODE )    AND 	 TRANS_MN.TRANS_TYPE in ('4') 	  AND ((convert(date, TRANS_MN.TRANS_DATE, 112) between '"+Date+"' and '"+nextday+"' ) ) GROUP BY  DEPARTMENT.DEPT_NAME,GROUPS.GROUP_NAME,SUB_GROUPS.SGROUP_NAME,PRODUCTS.BARCODE,PRODUCTS.DESCRIPTION,TRANS_MN.TRANS_NO").Tables[0];
        //             var modelList = conn.Select(@"SELECT isnull(DEPARTMENT.DEPT_NAME,'NAN' )  DEPT_NAME,
        //      isnull(GROUPS.GROUP_NAME,'NAN')      GROUP_NAME,
        //      isnull(SUB_GROUPS.SGROUP_NAME,'NAN') SGROUP_NAME,
        //      PRODUCTS.BARCODE,
        //      PRODUCTS.DESCRIPTION,
        //sum(CASE TRANS_MN.TYPE When 'A' Then 
        //  	 (CASE TRANS_DT.EXCH_FLAG When 'T' Then units_sold Else -units_sold end) end) units_sold,
        //   round(sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then 
        //(units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT ELSE -(units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT end) end),2) cost_amount,
        //   round(sum(CASE TRANS_MN.TYPE When 'A' Then 
        //  (CASE TRANS_DT.EXCH_FLAG When 'T' Then (TRANS_DT.UNIT_RETAIL*UNITS_SOLD) Else -abs(TRANS_DT.UNIT_RETAIL*UNITS_SOLD ) end) end),0)  retail_amount,
        //      isnull(round(sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then units_sold*TRANS_DT.UNIT_RETAIL*(dis_amount/100) 
        // ELSE -abs(units_sold*TRANS_DT.UNIT_RETAIL)*(dis_amount/100) end) end),2),0) disc_amount,
        //      round(sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then (amount ) Else -abs(amount ) end) end),2)- 
        //isnull(sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then (amount)*(trans_mn.disc_rate/100) 
        //ELSE -abs (amount*(trans_mn.disc_rate/100)) end) end),0) net_amount,
        //   round( sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then (TRANS_DT.unit_retail * TRANS_DT.units_sold ) Else 
        //-abs(TRANS_DT.unit_retail * TRANS_DT.units_sold) end) end)- sum(CASE TRANS_MN.TYPE When 'A' Then 
        //(CASE TRANS_DT.EXCH_FLAG When 'T' Then 
        //   	 (units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT ELSE -(units_sold*TRANS_DT.UNIT_COST)+TRANS_DT.GST_AMOUNT end) end),2)net_margin,
        // 		  (select count(price.TRANS_NO) from PRICE_overide_log
        //   join PRICE_overide_log  price on TRANS_MN.TRANS_NO=price.TRANS_NO
        //    where convert(date, price.PRICE_DATE, 112) between '" + Date+"' and '"+nextday+"') as priceoverridecount, (select isnull(Sum(price.UNIT_RETAIL),0) from PRICE_overide_log join PRICE_overide_log  price on TRANS_MN.TRANS_NO=price.TRANS_NO where convert(date, price.PRICE_DATE, 112) between '"+Date+"' and '"+nextday+"') as priceoverridecount,(select count(TRANS_DT.VOId) from TRANS_MN join TRANS_DT on TRANS_MN.TRANS_NO=TRANS_DT.TRANS_NO  where convert(date, TRANS_MN.TRANS_DATE, 112) between '"+Date+"' and '"+nextday+"' and TRANS_DT.VOID='T') 	as ItemsVoidCount,(select Round(Sum(isnull(TRANS_DT.UNIT_RETAIL,0)*isnull(TRANS_DT.UNITS_SOLD,0)-TRANS_DT.dis_amount),0) from TRANS_MN join TRANS_DT on TRANS_MN.TRANS_NO=TRANS_DT.TRANS_NO where convert(date, TRANS_MN.TRANS_DATE, 112) between '"+Date+"' and '"+nextday+ "' and TRANS_DT.VOID='T') as VoidAmount FROM PRODUCTS,TRANS_MN,TRANS_DT,DEPARTMENT,GROUPS,SUB_GROUPS  WHERE TRANS_MN.TRANS_NO = TRANS_DT.TRANS_NO AND  TRANS_DT.BARCODE = PRODUCTS.BARCODE and ( GROUPS.DEPT_CODE = DEPARTMENT.DEPT_CODE ) and  ( SUB_GROUPS.DEPT_CODE = GROUPS.DEPT_CODE ) and ( SUB_GROUPS.GROUP_CODE = GROUPS.GROUP_CODE ) and  ( PRODUCTS.SRGOUP_CODE = SUB_GROUPS.SGROUP_CODE ) and  ( PRODUCTS.GROUP_CODE = SUB_GROUPS.GROUP_CODE ) and  ( PRODUCTS.DEPT_CODE = SUB_GROUPS.DEPT_CODE ) AND( TRANS_MN.TRANS_TYPE='4'or TRANS_MN.TRANS_TYPE='3' or TRANS_MN.TRANS_TYPE='2') AND ((convert(date, TRANS_MN.TRANS_DATE, 112) between '" + Date+"' and ' "+nextday+"' ) )GROUP BY  DEPARTMENT.DEPT_NAME,GROUPS.GROUP_NAME,SUB_GROUPS.SGROUP_NAME,PRODUCTS.BARCODE,PRODUCTS.DESCRIPTION,TRANS_MN.TRANS_NO").Tables[0];
        //             //var modelList = (from salesMain in db.TRANS_MN
        //             //             join salesDetail in db.TRANS_DT
        //             //             on salesMain.TRANS_NO equals salesDetail.TRANS_NO
        //             //             join supplier in db.SUPPLIERs
        //             //             on salesMain.party_code equals supplier.SUPL_CODE
        //             //             join product in db.PRODUCTS
        //             //             on salesDetail.BARCODE equals product.BARCODE
        //             //             //  join location in db.Login_Location
        //             //             //  on salesMain.LOC_ID equals location.Log_Loc_Id
        //             //            where salesMain.START_TIME>=Date && salesMain.START_TIME<=nextday &&supplier.party_type== Constants.Constants.Customer&&salesMain.status=="4"
        //             //             select new
        //             //             {
        //             //                 SupplierName = supplier.SUPL_NAME,
        //             //                 SupplierContactNo = supplier.MOBILE,
        //             //                 DocumentDate = salesMain.START_TIME,
        //             //                 SupplierAddress = supplier.ADDRESS,
        //             //                 SupplierPhoneNo = supplier.PHONE,
        //             //                 SupplierCode=supplier.SUPL_CODE,
        //             //                 DocumentNo = salesMain.TRANS_NO,
        //             //                 ItemCode = salesDetail.BARCODE,
        //             //                 ItemName = product.DESCRIPTION,
        //             //                 Qty = salesDetail.UNITS_SOLD,
        //             //                 Rate = salesDetail.UNIT_RETAIL,
        //             //                 Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
        //             //                 PartyType = supplier.party_type,
        //             //                 //    Location = location.Log_Loc_Name,
        //             //                 //    Username = username,
        //             //                 Unit_Cost=salesDetail.UNIT_COST,
        //             //                 BookingDate = salesMain.TRANS_DATE,
        //             //                 UanNo = product.UAN_NO,
        //             //                 Discount = salesDetail.dis_amount ?? 0
        //             //             }).ToList();//Where(x=>x.PartyType==Constants.Constants.Customer&&x.DocumentDate.Date==Date).ToList();

        //             ReportDataSource rd = new ReportDataSource("DataSet1", modelList);

        //             string path = "";
        //             ReportParameter[] Par = new ReportParameter[4];
        //             string UserName;
        //             UserName = (string)System.Web.HttpContext.Current.Session["username"];
        //             if (UserName == null)
        //             {
        //                 string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //                 HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //                 FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //                 UserName = ticket.Name;
        //             }
        //             if (btn!=null&&btn == "SalesReport")
        //             {
        //                 Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //               //    path = Path.Combine(Server.MapPath(@"~\Report\Sale Report By Suplier.rdlc"));
        //                 path = Path.Combine(Server.MapPath(@"~\Report\Daily Sale Report By Item Wise.rdlc"));
        //                 lr.DataSources.Add(rd);
        //             }
        //             //  DateTime date = modelList.Select(x => x.DocumentDate).FirstOrDefault();
        //             //   string showdate = date.ToString("dd/MM/yyyy");
        //             string showdate = DateTime.Now.ToString("dd/MM/yyyy");
        //             Par[1] = new ReportParameter("UserName", UserName, false);
        //             Par[2] = new ReportParameter("DateFrom", showdate, false);
        //             Par[3] = new ReportParameter("DateTo", showdate, false);
        //             lr.ReportPath = path;
        //             lr.SetParameters(Par);
        //             lr.EnableExternalImages = true;
        //             string reportType = id;
        //             lr.Refresh();
        //             string mimeType;
        //             string encoding;
        //             string fileNameExtension;
        //             string deviceInfo =
        //             "<DeviceInfo>" +
        //             "  <OutputFormat>" + id + "</OutputFormat>" +
        //            // "  <Orientation>Landscape</Orientation>" +
        //             "  <PageWidth>9in</PageWidth>" +
        //             "  <MarginTop>0.1in</MarginTop>" +
        //             "  <MarginRight>0.1in</MarginRight>" +
        //         //    "  <MarginLeft>0.1in</MarginLeft>" +
        //             "  <MarginBottom>0.1in</MarginBottom>" +
        //             "</DeviceInfo>";
        //             Warning[] warnings;
        //             string[] streams;
        //             byte[] renderedBytes;
        //             lr.EnableExternalImages = true;
        //             renderedBytes = lr.Render(
        //                 reportType,
        //                 deviceInfo,
        //                 out mimeType,
        //                 out encoding,
        //                 out fileNameExtension,
        //                 out streams,
        //                 out warnings);
        //             //    lr.Dispose();
        //             return File(renderedBytes, mimeType);
        //         }
        //         catch (Exception ex)
        //         {
        //             return Content(ex.ToString());
        //         }
        //     }
        [HttpPost]
        public JsonResult gettable(string Doc_No)
        {
            //var model = (from salesDetail in db.TRANS_DT

            //             join product in db.PRODUCTS
            //              on salesDetail.BARCODE equals product.BARCODE
            //             join staffmember in db.StaffMembers on
            //             salesDetail.supl_code equals staffmember.SUPL_CODE
            //             where (salesDetail.VOID == "F" )

            //             select new
            //             {
            //                 DocumentNo = salesDetail.TRANS_NO,
            //                 ItemCode = salesDetail.BARCODE,
            //                 ItemName = product.DESCRIPTION,
            //                 Qty = salesDetail.UNITS_SOLD,
            //                 Retail = salesDetail.UNIT_RETAIL,
            //                 Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
            //                 Cost = salesDetail.UNIT_COST,
            //                 UanNo = product.UAN_NO,
            //                 Discount = salesDetail.dis_amount ?? 0,
            //                 warehouse = "01",
            //                 warhousename = "select",
            //                 colour = "01",
            //                 colourname = "select",
            //                 SalesBycode = salesDetail.supl_code,
            //                 SalesByName = staffmember.SUPL_NAME,

            //             }).ToList();
            MyConnection conn = new MyConnection();
            var data = conn.Select(@"select TRANS_DT.TRANS_NO as DocumentNo,TRANS_DT.BARCODE as ItemCode,PRODUCTS.DESCRIPTION as ItemName,
                            TRANS_DT.UNITS_SOLD  as Qty,TRANS_DT.UNIT_RETAIL as Retail,                                       TRANS_DT.s_time as TimeSlot,
                            --((isnull(TRANS_DT.UNITS_SOLD,0)*isnull(TRANS_DT.UNIT_RETAIL,0))-isnull(TRANS_DT.dis_amount,0)) as Amount,
                            isnull(TRANS_DT.AMOUNT,0) as Amount,
                            PRODUCTS.UNIT_COST as Cost,
                            isnull(TRANS_DT.dis_amount,0) as Discount,'01' as warehouse,'Select' as warhousename,
                            '01' as colour,'Select' as colourname,TRANS_DT.supl_code as SalesBycode,StaffMembers.SUPL_NAME as SalesByName  from TRANS_DT
                            join PRODUCTS on TRANS_DT.BARCODE=PRODUCTS.BARCODE
                            left join StaffMembers on TRANS_DT.supl_code=StaffMembers.SUPL_CODE
                            where TRANS_DT.VOID='F'
                            and TRANS_DT.TRANS_NO='" + Doc_No + "'").Tables[0];
            // var list = model.Where(x => x.DocumentNo == Doc_No).ToList();

            string value = String.Empty;
            value = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveNewCustomer(string Name, string Phone)
        {
            if (Phone != null)
            {
                var list = db.SUPPLIERs.Where(x => x.PARTY_TYPE == "c" && x.MOBILE == Phone).Select(x => x.MOBILE).ToList();
                if (list.Count > 0)
                {

                    //var ListFull = getCustomerList(false);
                    //string valueFull = String.Empty;
                    //valueFull = JsonConvert.SerializeObject(ListFull, Formatting.Indented,
                    //    new JsonSerializerSettings
                    //    {
                    //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    //    });
                    //ViewBag.status = "1";
                    //return Json(valueFull, JsonRequestBehavior.AllowGet);

                    return null;
                }
            }
            CustomerSupplierViewModel Code = new CustomerSupplierViewModel();
            SUPPLIER Sup = new SUPPLIER();
            //Sup.SUPL_CODE = Code.GetSuplCode(6, "c");
            Sup.SUPL_NAME = Name;
            Sup.MOBILE = Phone;
            Sup.ADDRESS = " ";
            Sup.DOC = DateTime.Now;
            Sup.DISC_CATEGORY_ID = 1;
            Sup.PARTY_TYPE = Constants.CustomerSupplier.Customer;
            db.SUPPLIERs.Add(Sup);
            db.SaveChanges();

            //  var newevents = new List<CalenderViewModel>();
            // var List = getCustomerList(true);
            var List = db.SUPPLIERs.Where(x => x.PARTY_TYPE == "c").ToList();
            string value = String.Empty;
            value = JsonConvert.SerializeObject(List, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(value, JsonRequestBehavior.AllowGet);

            //return View();
        }
        [HttpPost]
        public ActionResult GetPhoneNumberanddiscountcategory(string SUPL_CODE)
        {
            ////var List = db.SUPPLIERs.Where(x => x.party_type == "c" && x.SUPL_CODE == SUPL_CODE).Select(x => new { x.MOBILE,x.DISC_CATEGORY_ID }).FirstOrDefault();
            //var List = (from s in db.SUPPLIERs
            //            join d in db.CUSTOMER_DISCOUNT_CATEGORY
            //            on s.DISC_CATEGORY_ID equals d.CATEGORY_ID
            //            where s.SUPL_CODE == SUPL_CODE && s.party_type == "c"
            //            select new
            //            {
            //                MOBILE = s.MOBILE,
            //                DiscountCategoryId = s.DISC_CATEGORY_ID,
            //                DiscountCategoryName = d.DESCRIPTION,
            //                Discount = d.DISCOUNT
            //            }).FirstOrDefault();

            var List = db.SUPPLIERs.Where(x => x.SUPL_CODE == SUPL_CODE).Select(x => x.MOBILE).SingleOrDefault();
            string value = String.Empty;
            value = JsonConvert.SerializeObject(List, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        public List<CustomerSupplierViewModel> getCustomerList(bool flag)
        {
            var customers = db.SUPPLIERs.Where(x => x.PARTY_TYPE == "c").ToList();
            List<CustomerSupplierViewModel> List = new List<CustomerSupplierViewModel>();

            foreach (var v in customers)
            {
                CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
                obj.SUPL_NAME = v.SUPL_NAME;
                obj.SUPL_CODE = v.SUPL_CODE;
                if (flag)
                {
                    obj.STATUS = "1";
                }
                List.Add(obj);
            }
            return List;
        }
        public ActionResult bookingStatus(string DocNo)
        {
            bool flag = false;
            var List = db.TRANS_MN.FirstOrDefault(x => x.STATUS == "0" && x.TRANS_NO == DocNo);
            if (List != null)
            {
                List.CANCEL = "3";
                flag = true;
            }
            db.SaveChanges();
            if (flag)
            {
                string value = String.Empty;
                value = JsonConvert.SerializeObject(List, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound("Page not found. Bad Request");
            }
        }
        [HttpPost]
        public ActionResult UpdateOnlineBookingtomainbooking(string DocNo)
        {
            try
            {
                MyConnection conn = new MyConnection();
                var model = conn.Select(@"select trnsmn.trans_No,trnsmn.Time_Slot,trnsdt.UAN_NO,trnsmn.TRANS_TYPE,trnsmn.TRANS_DATE,trnsmn.reference_person as Customer,
                                    trnsmn.mob as mobileno,trnsdt.BARCODE, trnsdt.UNIT_COST, trnsdt.UNIT_RETAIL, trnsdt.UNITS_SOLD, trnsdt.supl_code as StaffCode
                                    from TRANS_OnMn trnsmn
                                    left join TRANS_OnDt trnsdt on trnsmn.TRANS_NO = trnsdt.TRANS_NO
                                    where trnsmn.TRANS_NO='" + DocNo + "'").Tables[0];
                string DocumentNo = "";
                string FaceBookBookingno = "";
                var userlist = db.USERS.ToList();
                string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
                HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                string UserName = ticket.Name;
                var userid = userlist.Where(x => x.USER_NAME == UserName).Select(x => x.USER_ID).FirstOrDefault();
                if (model.Rows.Count > 0)
                {
                    var transmn = new TRANS_MN();
                    DocumentMainViewModel vm = new DocumentMainViewModel();
                    string prefix = "IN";
                    DocumentNo = vm.GetSaleCode(prefix, 6);
                    //DocumentNo = vm.GetSaleCode(prefix, 6, (vm.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn);
                    transmn.TRANS_NO = DocumentNo;
                    //  transmn.TRANS_NO = model.Rows[0]["trans_No"].ToString();
                    transmn.TRANS_DATE = Convert.ToDateTime(model.Rows[0]["TRANS_DATE"]);
                    transmn.TRANS_TYPE = "3"; //model.Rows[0]["TRANS_TYPE"].ToString();
                    transmn.START_TIME = DateTime.Now;
                    transmn.PARTY_CODE = getcustomercode(model.Rows[0]["Customer"].ToString(), model.Rows[0]["mobileno"].ToString());
                    transmn.TYPE = "A";
                    transmn.LOC_ID = "01";

                    // item.INSERTED_BY = username;
                    //   string us = Request.Cookies["UserSettings"].Value;
                    transmn.USER_ID = userid;//userlist.Where(x=>x.);
                    transmn.MOB = model.Rows[0]["mobileno"].ToString();
                    transmn.STATUS = "0";
                    transmn.TILL_NO = "1";
                    transmn.CANCEL = "0";
                    transmn.BY_CARD = Convert.ToDecimal(Constants.PaymentType.Card);
                    transmn.BY_CASH = Convert.ToDecimal(Constants.PaymentType.Cash);
                    transmn.TIME_SLOT = Convert.ToDateTime(model.Rows[0]["Time_Slot"].ToString()).TimeOfDay;
                    db.TRANS_MN.Add(transmn);
                    for (int i = 0; i < model.Rows.Count; i++)
                    {
                        var salesDetail = new TRANS_DT();
                        FaceBookBookingno = model.Rows[i]["trans_No"].ToString();
                        salesDetail.TRANS_NO = DocumentNo;//model.Rows[i]["trans_No"].ToString();
                        salesDetail.BARCODE = model.Rows[i]["BARCODE"].ToString();
                        salesDetail.UNIT_COST = Convert.ToDecimal(model.Rows[i]["UNIT_COST"].ToString());
                        salesDetail.UNIT_RETAIL = Convert.ToDecimal(model.Rows[i]["UNIT_RETAIL"].ToString());
                        salesDetail.SCAN_TIME = DateTime.Now;
                        salesDetail.UNITS_SOLD = Convert.ToDecimal(model.Rows[i]["UNITS_SOLD"].ToString());
                        salesDetail.UAN_NO = model.Rows[i]["UAN_NO"].ToString();
                        salesDetail.DIS_AMOUNT = 0;
                        salesDetail.AMOUNT = salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD;
                        salesDetail.SUPL_CODE = model.Rows[i]["StaffCode"].ToString();
                        salesDetail.VOID = "F";
                        salesDetail.EXCH_FLAG = "T";
                        salesDetail.GST_AMOUNT = 0;
                        salesDetail.COLOUR = 1;//item.Colour;
                        salesDetail.WAREHOUSE = "01";
                        salesDetail.TRANS_TYPE = "3";//(docType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn; ;
                        db.TRANS_DT.Add(salesDetail);
                    }
                }
                string value = String.Empty;
                var list = db.SUPPLIERs.Where(x => x.PARTY_TYPE == "c").ToList();
                value = JsonConvert.SerializeObject(list, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var exception = ex.ToString();
                string value = String.Empty;
                value = JsonConvert.SerializeObject("False", Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                return Json(value, JsonRequestBehavior.AllowGet);
            }
        }
        public string getcustomercode(string names, string mobile)
        {
            try
            {
                string customercode = "";
                var customerc = db.SUPPLIERs.Where(x => x.SUPL_NAME == names && x.MOBILE == mobile && x.PARTY_TYPE == Constants.CustomerSupplier.Customer).Select(x => x.SUPL_CODE).FirstOrDefault();
                if (customerc != null)
                {
                    customercode = customerc;
                }
                else
                {
                    CustomerSupplierViewModel Code = new CustomerSupplierViewModel();
                    SUPPLIER Sup = new SUPPLIER();
                    //Sup.SUPL_CODE = Code.GetSuplCode(6, "c");
                    customercode = Sup.SUPL_CODE;
                    Sup.SUPL_NAME = names;
                    Sup.MOBILE = mobile;
                    Sup.ADDRESS = " ";
                    Sup.DOC = DateTime.Now;
                    Sup.DISC_CATEGORY_ID = 1;
                    Sup.PARTY_TYPE = Constants.CustomerSupplier.Customer;
                    db.SUPPLIERs.Add(Sup);

                }
                return customercode;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        //public JsonResult GetOnlineBooking ()
        //{
        //    DocumentMainViewModel obj = new DocumentMainViewModel();
        //    var list = obj.getOnlineBooking();//model.Where(x => x.DocumentNo == Doc_No).ToList();
        //    string value = String.Empty;
        //    value = JsonConvert.SerializeObject(list, Formatting.Indented,
        //        new JsonSerializerSettings
        //        {
        //            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //        });
        //    return Json(value, JsonRequestBehavior.AllowGet);
        //}

        //public DocumentViewModel GetSalesPageViewModel(string docType)
        //{
        //    DocumentMainViewModel obj = new DocumentMainViewModel();
        //    obj.DocDate = DateTime.Now;
        //    obj.DocType = docType;
        //    string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //    HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //    string UserName = (ticket.Name).Trim();
        //    obj.Userid = db.USERS.Where(x => x.USER_NAME == UserName).Select(x => x.USER_ID).FirstOrDefault();//UserName;//Session["username"].ToString();
        //    obj.DocNoDisplay = obj.GetSaleCode(obj.DocType, 6);
        //    //obj.DocNoDisplay = obj.GetSaleCode(obj.DocType, 6, (obj.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn);
        //    var Supplierlist = db.SUPPLIERs.Where(u => u.party_type == Constants.Constants.Customer).Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList();
        //    //obj.DocType = "PIN";
        //    if (Supplierlist.Count > 0)
        //    {
        //        obj.SuplCode = Supplierlist.Select(x => x.SupplierCode).FirstOrDefault();
        //    }
        //    obj.Location = db.USERS.Where(x => x.USER_NAME == UserName).Select(x => x.LOC_ID).FirstOrDefault();
        //    var viewmodel = new DocumentViewModel
        //    {
        //        DocumentMain = obj,
        //        LogLocationList = db.Login_Location.ToList(),
        //        WarehouseList = db.Warehouses.ToList(),
        //        SupplierList = Supplierlist,
        //        ItemList = db.PRODUCTS.Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
        //        //ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList(),
        //        ColourList = db.Colours.ToList()
        //    };
        //    return viewmodel;
        //}


        //
        //public ActionResult SalesReturn()
        //{
        //    try
        //    {
        //        return View("Index", GetSalesPageViewModel(Constants.Constants.SalesReturnDocument));
        //    }
        //    catch (System.Exception)
        //    {
        //        return Content("<p>Page is not working.</p>");
        //    }
        //}

        //
        //public ActionResult PrintSalesReport(string id)
        //{

        //    string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //    HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //    string UserName = ticket.Name;
        //    var username = UserName;//Session["username"].ToString();
        //    string companyAddress;
        //    string companyPhone;
        //    string LocationId = null;
        //    if (Request.Cookies["Location"] != null)
        //        LocationId = Request.Cookies["Location"].Value;

        //    if (LocationId == "01")
        //    {
        //        companyAddress = Configuration.KarachiAddress;
        //        companyPhone = Configuration.KarachiPhone;
        //    }
        //    else
        //    {
        //        companyAddress = Configuration.LahoreAddress;
        //        companyPhone = Configuration.LahorePhone;
        //    }

        //    var model = (from salesMain in db.TRANS_MN
        //                 join salesDetail in db.TRANS_DT
        //                 on salesMain.TRANS_NO equals salesDetail.TRANS_NO
        //                 join supplier in db.SUPPLIERs
        //                 on salesMain.party_code equals supplier.SUPL_CODE
        //                 join product in db.PRODUCTS
        //                 on salesDetail.BARCODE equals product.BARCODE
        //                 join location in db.LOCATIONs
        //                 on salesMain.LOC_ID equals location.LOC_ID
        //                 join warehouse in db.Warehouses
        //                 on salesDetail.Warehouse equals warehouse.id
        //                 select new
        //                 {
        //                     SupplierName = supplier.SUPL_NAME,
        //                     SupplierContactNo = supplier.MOBILE,
        //                     SupplierAddress = supplier.ADDRESS,
        //                     SupplierPhoneNo = supplier.PHONE,
        //                     DocumentNo = salesMain.TRANS_NO,
        //                     ItemCode = salesDetail.BARCODE,
        //                     ItemName = product.DESCRIPTION,
        //                     Qty = salesDetail.UNITS_SOLD,
        //                     Rate = salesDetail.UNIT_RETAIL,
        //                     Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
        //                     PartyType = supplier.party_type,
        //                     Location = location.LOCATION1,
        //                     Warehouse = warehouse.Name,
        //                     Username = username,
        //                     Discount = salesDetail.dis_amount ?? 0

        //                 }).ToList();

        //    var modelList = model.Where(u => u.DocumentNo == id && u.PartyType == Constants.Constants.Customer).ToList();

        //    var viewmodel = new SalesInvoiceReportViewModel();
        //    var company = new CompanyViewModel();
        //    company.CompanyName = Configuration.CompanyName;
        //    company.CompanyAddress = companyAddress;
        //    company.CompanyContactNo = companyPhone;
        //    viewmodel.Company = company;
        //    var reportMain = new ReportMainViewModel();
        //    var reportDetailList = new List<ReportDetailViewModel>();
        //    foreach (var item in modelList)
        //    {
        //        reportMain.DocumentNo = item.DocumentNo;
        //        reportMain.SupplierName = item.SupplierName;
        //        reportMain.SupplierAddress = item.SupplierAddress;
        //        reportMain.SupplierContactNo = item.SupplierContactNo;
        //        reportMain.SupplierPhoneNo = item.SupplierPhoneNo;
        //        reportMain.Username = item.Username;
        //        reportMain.Location = item.Location;

        //        var reportDetail = new ReportDetailViewModel();
        //        reportDetail.ItemCode = item.ItemCode;
        //        reportDetail.ItemName = item.ItemName;
        //        reportDetail.Qty = item.Qty;
        //        reportDetail.Rate = item.Rate;
        //        reportDetail.Amount = item.Amount;
        //        reportDetail.ItemCode = item.ItemCode;
        //        reportDetail.PartyType = item.PartyType;
        //        reportDetail.Warehouse = item.Warehouse;
        //        reportDetail.Discount = item.Discount;

        //        reportDetailList.Add(reportDetail);

        //    }
        //    reportMain.PartType = "Customer Name : ";
        //    reportMain.DocumentName = "Invoice";

        //    viewmodel.ReportMain = reportMain;
        //    viewmodel.ReportDetailList = reportDetailList;


        //    return View("SalesReport", viewmodel);
        //}
        //
        //public ActionResult List()
        //{
        //    DocumentListViewModel obj = new DocumentListViewModel();
        //    return View(obj.GetSalesList(Constants.Constants.TransType.Sales));
        //}
        //
        //public ActionResult Edit(string docno)
        //{
        //    var salesMaininDb = db.TRANS_MN.SingleOrDefault(c => c.TRANS_NO == docno);
        //    if (salesMaininDb == null)
        //        return HttpNotFound("Page not found. Bad Request");
        //    var itemList = db.PRODUCTS.Select(u => new ItemViewModel { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
        //    //var itemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "P").Select(u => new ItemViewMode { Uanno = u.UAN_NO, Barcode = u.BARCODE, Description = u.DESCRIPTION }).ToList();
        //    var salesDetail = db.TRANS_DT.Where(c => c.TRANS_NO == docno).
        //        Select(u => new DocumentDetailViewModel
        //        {
        //            Barcode = u.BARCODE,
        //            Retail = u.UNIT_RETAIL,
        //            Qty = u.UNITS_SOLD,
        //            Discount = (((u.dis_amount ?? 0) * u.UNIT_RETAIL) / 100) * u.UNITS_SOLD,
        //            Warehouse = u.Warehouse,
        //            Colour = u.Colour ?? 0,
        //            Amount = ((u.UNIT_RETAIL * (u.UNITS_SOLD) - ((((u.dis_amount ?? 0) * u.UNIT_RETAIL) / 100) * u.UNITS_SOLD)))
        //        }).ToList();
        //    var warehouseList = db.Warehouses.Where(u => u.loc_id == salesMaininDb.LOC_ID).ToList();
        //    var colourList = db.Colours.ToList();
        //    foreach (var item in salesDetail)
        //    {
        //        var itemName = itemList.SingleOrDefault(u => u.Barcode == item.Barcode);
        //        item.Description = itemName.Description;
        //        item.Uanno = itemName.Uanno;
        //    }

        //    var docMain = new DocumentMainViewModel();
        //    docMain.DocDate = salesMaininDb.TRANS_DATE;
        //    docMain.DocNo = salesMaininDb.TRANS_NO;
        //    docMain.DocNoDisplay = salesMaininDb.TRANS_NO;
        //    docMain.Userid = salesMaininDb.USER_ID.Trim();
        //    docMain.DocType = "PIN";
        //    docMain.Location = salesMaininDb.LOC_ID;
        //    docMain.SuplCode = salesMaininDb.party_code;
        //    docMain.Status = salesMaininDb.status;
        //    var viewmodel = new DocumentViewModel
        //    {
        //        LogLocationList = db.Login_Location.ToList(),
        //        ItemList = itemList,
        //        SupplierList = db.SUPPLIERs.Where(u => u.party_type == Constants.Constants.Customer)
        //            .Select(u => new SupplierViewModel { SupplierCode = u.SUPL_CODE, SupplierName = u.SUPL_NAME }).ToList(),
        //        DocumentMain = docMain,
        //        DocumentDetailList = salesDetail,

        //    };

        //    return View("Index", viewmodel);

        //}
        //[HttpGet]
        //public ActionResult PrintBillByDocumentNumber(string Doc_Number)
        //{
        //    try
        //    {
        //        var Date = DateTime.Now.Date;
        //        string id = "PDF";
        //        var nextday = Date.AddDays(1);
        //        LocalReport lr = new LocalReport();
        //        MyConnection conn = new MyConnection();
        //        var modelList = (from salesMain in db.TRANS_MN
        //                         join salesDetail in db.TRANS_DT
        //                         on salesMain.TRANS_NO equals salesDetail.TRANS_NO
        //                         join supplier in db.SUPPLIERs
        //                         on salesMain.party_code equals supplier.SUPL_CODE
        //                         join product in db.PRODUCTS
        //                         on salesDetail.BARCODE equals product.BARCODE
        //                         //  join location in db.Login_Location
        //                         //  on salesMain.LOC_ID equals location.Log_Loc_Id
        //                         where salesMain.TRANS_NO == Doc_Number && supplier.party_type == Constants.Constants.Customer //&& salesMain.status == "3"
        //                         select new
        //                         {
        //                             SupplierName = supplier.SUPL_NAME,
        //                             SupplierContactNo = supplier.MOBILE,
        //                             DocumentDate = salesMain.START_TIME,
        //                             SupplierAddress = supplier.ADDRESS,
        //                             SupplierPhoneNo = supplier.PHONE,
        //                             SupplierCode = supplier.SUPL_CODE,
        //                             DocumentNo = salesMain.TRANS_NO,
        //                             ItemCode = salesDetail.BARCODE,
        //                             ItemName = product.DESCRIPTION,
        //                             Qty = salesDetail.UNITS_SOLD,
        //                             Rate = salesDetail.UNIT_RETAIL,
        //                             Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
        //                             PartyType = supplier.party_type,
        //                             //    Location = location.Log_Loc_Name,
        //                             //    Username = username,
        //                             Unit_Cost = salesDetail.UNIT_COST,
        //                             BookingDate = salesMain.TRANS_DATE,
        //                             UanNo = product.UAN_NO,
        //                             Discount = salesDetail.dis_amount ?? 0
        //                         }).ToList();//Where(x=>x.PartyType==Constants.Constants.Customer&&x.DocumentDate.Date==Date).ToList();

        //        ReportDataSource rd = new ReportDataSource("DataSet1", modelList);

        //        string path = "";
        //        ReportParameter[] Par = new ReportParameter[4];
        //        //  if (btn != null && btn == "SalesReport")
        //        // {
        //        Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        path = Path.Combine(Server.MapPath(@"~\Report\Sale Bill By Document No.rdlc"));
        //        lr.DataSources.Add(rd);
        //        string UserName;
        //        UserName = (string)System.Web.HttpContext.Current.Session["username"];
        //        if (UserName == null)
        //        {
        //            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //            UserName = ticket.Name;
        //        }
        //        //  }
        //        //else
        //        //{
        //        //    // Par[0] = new ReportParameter("ReportName", "Sales Return Document", false);
        //        //    Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        //    path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));
        //        //   modelList= modelList.Where(x => x.DocumentNo == Doc_Number).ToList();
        //        //    rd = new ReportDataSource("DataSet1", modelList);
        //        //}
        //        // path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));D:\AFZAAL AHMAD\working apps\WorkingApps\Inventory for Saloon\Inventory\Hrm_1\Report\SalesReportByDate .rdlc
        //        Par[1] = new ReportParameter("UserName", UserName, false);
        //        Par[2] = new ReportParameter("CompanyAddress", Constants.Constants.CompanyAddress, false);
        //        Par[3] = new ReportParameter("CompanyPhone", Constants.Constants.CompanyPhone, false);
        //        lr.ReportPath = path;
        //        lr.SetParameters(Par);
        //        //  lr.EnableExternalImages = true;
        //        string reportType = id;
        //        lr.Refresh();
        //        string mimeType;
        //        string encoding;
        //        string fileNameExtension;
        //        string deviceInfo =
        //        "<DeviceInfo>" +
        //        "  <OutputFormat>" + id + "</OutputFormat>" +
        //        // "  <Orientation>Landscape</Orientation>" +
        //        "  <PageWidth>3in</PageWidth>" +
        //        "  <MarginTop>0.1in</MarginTop>" +
        //        "  <MarginRight>0.1in</MarginRight>" +
        //        "  <MarginLeft>0.1in</MarginLeft>" +
        //        "  <MarginBottom>0.1in</MarginBottom>" +
        //        "</DeviceInfo>";
        //        Warning[] warnings;
        //        string[] streams;
        //        byte[] renderedBytes;
        //        lr.EnableExternalImages = true;
        //        renderedBytes = lr.Render(
        //            reportType,
        //            deviceInfo,
        //            out mimeType,
        //            out encoding,
        //            out fileNameExtension,
        //            out streams,
        //            out warnings);
        //        //    lr.Dispose();
        //        return File(renderedBytes, mimeType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.ToString());
        //    }
        //}
        //[HttpGet]
        //public ActionResult PrintSaleDocumentreportByDocumentNumber(string Doc_Number, string Doc_Type, string btn)
        //{
        //    try
        //    {
        //        var Date = DateTime.Now.Date;
        //        string id = "PDF";
        //        var nextday = Date.AddDays(1);
        //        LocalReport lr = new LocalReport();
        //        MyConnection conn = new MyConnection();
        //        var modelList = (from salesMain in db.TRANS_MN
        //                         join salesDetail in db.TRANS_DT
        //                         on salesMain.TRANS_NO equals salesDetail.TRANS_NO
        //                         join supplier in db.SUPPLIERs
        //                         on salesMain.party_code equals supplier.SUPL_CODE
        //                         join product in db.PRODUCTS
        //                         on salesDetail.BARCODE equals product.BARCODE
        //                         //  join location in db.Login_Location
        //                         //  on salesMain.LOC_ID equals location.Log_Loc_Id
        //                         where salesMain.TRANS_NO == Doc_Number && supplier.party_type == Constants.Constants.Customer //&& salesMain.status == "3"
        //                         select new
        //                         {
        //                             SupplierName = supplier.SUPL_NAME,
        //                             SupplierContactNo = supplier.MOBILE,
        //                             DocumentDate = salesMain.START_TIME,
        //                             SupplierAddress = supplier.ADDRESS,
        //                             SupplierPhoneNo = supplier.PHONE,
        //                             SupplierCode = supplier.SUPL_CODE,
        //                             DocumentNo = salesMain.TRANS_NO,
        //                             ItemCode = salesDetail.BARCODE,
        //                             ItemName = product.DESCRIPTION,
        //                             Qty = salesDetail.UNITS_SOLD,
        //                             Rate = salesDetail.UNIT_RETAIL,
        //                             Amount = (salesDetail.UNIT_RETAIL * salesDetail.UNITS_SOLD) - salesDetail.dis_amount ?? 0,
        //                             PartyType = supplier.party_type,
        //                             //    Location = location.Log_Loc_Name,
        //                             //    Username = username,
        //                             Unit_Cost = salesDetail.UNIT_COST,
        //                             BookingDate = salesMain.TRANS_DATE,
        //                             UanNo = product.UAN_NO,
        //                             // Advance=salesMain.advance,
        //                             Discount = (((salesDetail.dis_amount ?? 0) * salesDetail.UNIT_RETAIL) / 100) * salesDetail.UNITS_SOLD
        //                         }).ToList();//Where(x=>x.PartyType==Constants.Constants.Customer&&x.DocumentDate.Date==Date).ToList();

        //        ReportDataSource rd = new ReportDataSource("DataSet1", modelList);

        //        string path = "";
        //        ReportParameter[] Par = new ReportParameter[4];
        //        //  if (btn != null && btn == "SalesReport")
        //        // {
        //        Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        //path = Path.Combine(Server.MapPath(@"~\Report\Sale Report By Document No .rdlc"));
        //        path = Path.Combine(Server.MapPath(@"~\Report\Sale Bill.rdlc"));
        //        lr.DataSources.Add(rd);
        //        string UserName;
        //        UserName = (string)System.Web.HttpContext.Current.Session["username"];
        //        if (UserName == null)
        //        {
        //            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //            UserName = ticket.Name;
        //        }
        //        //  }
        //        //else
        //        //{
        //        //    // Par[0] = new ReportParameter("ReportName", "Sales Return Document", false);
        //        //    Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        //    path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));
        //        //   modelList= modelList.Where(x => x.DocumentNo == Doc_Number).ToList();
        //        //    rd = new ReportDataSource("DataSet1", modelList);
        //        //}
        //        // path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));D:\AFZAAL AHMAD\working apps\WorkingApps\Inventory for Saloon\Inventory\Hrm_1\Report\SalesReportByDate .rdlc
        //        Par[1] = new ReportParameter("UserName", UserName, false);
        //        Par[2] = new ReportParameter("CompanyAddress", Constants.Constants.CompanyAddress, false);
        //        Par[3] = new ReportParameter("CompanyPhone", Constants.Constants.CompanyPhone, false);
        //        lr.ReportPath = path;
        //        lr.SetParameters(Par);
        //        //  lr.EnableExternalImages = true;
        //        string reportType = id;
        //        lr.Refresh();
        //        // lr.PrintToPrinter();
        //        string mimeType;
        //        string encoding;
        //        string fileNameExtension;
        //        string deviceInfo =
        //        "<DeviceInfo>" +
        //        "  <OutputFormat>" + id + "</OutputFormat>" +
        //          // "  <Orientation>Landscape</Orientation>" +
        //          // "  <PageWidth>10in</PageWidth>" +
        //          "  <PageWidth>3.1in</PageWidth>" +
        //        "  <MarginTop>0.1in</MarginTop>" +
        //        "  <MarginRight>0.1in</MarginRight>" +
        //        "  <MarginLeft>0.1in</MarginLeft>" +
        //        "  <MarginBottom>0.1in</MarginBottom>" +
        //        "</DeviceInfo>";
        //        Warning[] warnings;
        //        string[] streams;
        //        byte[] renderedBytes;
        //        lr.EnableExternalImages = true;

        //        renderedBytes = lr.Render(
        //            reportType,
        //            deviceInfo,
        //            out mimeType,
        //            out encoding,
        //            out fileNameExtension,
        //            out streams,
        //            out warnings);
        //        //var doc = new Document();
        //        //var reader = new PdfReader(renderedBytes);

        //        //using (FileStream fs = new FileStream(Server.MapPath("~/Report/Sale Report By Document No.pdf"), FileMode.Create))
        //        //{
        //        //    PdfStamper stamper = new PdfStamper(reader, fs);
        //        //    string Printer = "HP LaserJet 1022";
        //        //    if (Printer == null || Printer == "")
        //        //    {
        //        //        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
        //        //        stamper.Close();
        //        //    }
        //        //    else
        //        //    {
        //        //        stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
        //        //        stamper.Close();
        //        //    }
        //        //}
        //        //reader.Close();

        //        //FileStream fss = new FileStream(Server.MapPath("~/Report/Sale Report By Document No.pdf"), FileMode.Open);
        //        //byte[] bytes = new byte[fss.Length];
        //        //fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
        //        //fss.Close();
        //        //System.IO.File.Delete(Server.MapPath("~/Report/Sale Report By Document No.pdf"));
        //        //////    lr.Dispose();
        //        //PrintDocument printDoc = new PrintDocument();
        //        //     printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
        //        return File(renderedBytes, mimeType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.ToString());
        //    }
        //}

        //public ActionResult SrList()
        //{
        //    DocumentListViewModel obj = new DocumentListViewModel();
        //    return View("List", obj.GetSalesList(Constants.Constants.TransType.SalesReturn));
        //}
        //
        //[HttpGet]
        //public ActionResult StockReport ()
        //{
        //    try
        //    {
        //        var Date = DateTime.Now.Date;
        //        string id = "PDF";
        //        var nextday = Date.AddDays(1);
        //        LocalReport lr = new LocalReport();
        //            MyConnection conn = new MyConnection();
        //            var modelList = conn.Select(@"select pb.BARCODE,p.UNIT_RETAIL,p.DESCRIPTION as ItemName,pb.GRN_AMOUNT,pb.GRN_QTY,pb.GRF_AMOUNY,pb.GRF_QTY,
        //                                        pb.SALE_QTY,pb.SALE_AMOUNT,pb.TRANSFER_IN_AMOUNT,pb.TRANSFER_IN_QTY,
        //                                        pb.TRANSFER_OUT_AMOUNT,pb.TRANSFER_OUT_QTY  from PROD_BALANCE pb 
        //                                        left join  PRODUCTS p on pb.BARCODE =p.BARCODE ").Tables[0];
        //        //Where(x=>x.PartyType==Constants.Constants.Customer&&x.DocumentDate.Date==Date).ToList();

        //        ReportDataSource rd = new ReportDataSource("DataSet1", modelList);

        //        string path = "";
        //        ReportParameter[] Par = new ReportParameter[4];
        //        //  if (btn != null && btn == "SalesReport")
        //        // {
        //        Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        path = Path.Combine(Server.MapPath(@"~\Report\Stock Position.rdlc"));
        //        lr.DataSources.Add(rd);
        //        string UserName;
        //        UserName = (string)System.Web.HttpContext.Current.Session["username"];
        //        if (UserName == null)
        //        {
        //            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
        //            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
        //            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
        //            UserName = ticket.Name;
        //        }
        //        //  }
        //        //else
        //        //{
        //        //    // Par[0] = new ReportParameter("ReportName", "Sales Return Document", false);
        //        //    Par[0] = new ReportParameter("CompanyName", Constants.Constants.Companyname, false);
        //        //    path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));
        //        //   modelList= modelList.Where(x => x.DocumentNo == Doc_Number).ToList();
        //        //    rd = new ReportDataSource("DataSet1", modelList);
        //        //}
        //        // path = Path.Combine(Server.MapPath(@"~\Report\SaleReportByDocumentNo.rdlc"));D:\AFZAAL AHMAD\working apps\WorkingApps\Inventory for Saloon\Inventory\Hrm_1\Report\SalesReportByDate .rdlc
        //        Par[1] = new ReportParameter("UserName", UserName, false);
        //        Par[2] = new ReportParameter("CompanyAddress", Constants.Constants.CompanyAddress, false);
        //        Par[3] = new ReportParameter("CompanyPhone", Constants.Constants.CompanyPhone, false);
        //        lr.ReportPath = path;
        //        lr.SetParameters(Par);
        //        //  lr.EnableExternalImages = true;
        //        string reportType = id;
        //        lr.Refresh();
        //        string mimeType;
        //        string encoding;
        //        string fileNameExtension;
        //        string deviceInfo =
        //        "<DeviceInfo>" +
        //        "  <OutputFormat>" + id + "</OutputFormat>" +
        //         "  <Orientation>Landscape</Orientation>" +
        //        "  <PageWidth>12in</PageWidth>" +
        //        "  <MarginTop>0.1in</MarginTop>" +
        //        "  <MarginRight>0.1in</MarginRight>" +
        //        "  <MarginLeft>0.1in</MarginLeft>" +
        //        "  <MarginBottom>0.1in</MarginBottom>" +
        //        "</DeviceInfo>";
        //        Warning[] warnings;
        //        string[] streams;
        //        byte[] renderedBytes;
        //        lr.EnableExternalImages = true;

        //        renderedBytes = lr.Render(
        //            reportType,
        //            deviceInfo,
        //            out mimeType,
        //            out encoding,
        //            out fileNameExtension,
        //            out streams,
        //            out warnings);
        //        //    lr.Dispose();


        //        return File(renderedBytes, mimeType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.ToString());
        //    }
        //}
        public ActionResult SaveDn(DocumentViewModel vm)
        {
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var DnMain = new DN_MAIN();
                    DnMain.DN_NO = vm.DocumentMain.GetDNCode();
                    DnMain.DN_DATE = DateTime.Now;
                    DnMain.START_TIME = DateTime.Now;
                    DnMain.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                    DnMain.LOC_ID = vm.DocumentMain.Location;
                    DnMain.TIME_SLOT = vm.DocumentMain.Time;
                    DnMain.CUST_NAME = vm.DocumentMain.CustomerName;
                    DnMain.TRANSFER_STATUS = "0";
                    DnMain.TILL_NO = "01";
                    DnMain.DELIVERY_PLACE = "01";
                    DnMain.DELIVERY_DATE = DateTime.Now;
                    DnMain.SALESMAN_CDE = vm.DocumentMain.staffcode;
                    DnMain.SALE_TYPE = vm.DocumentMain.RequestPage;
                    DnMain.BY_CARD = Convert.ToDecimal(Constants.PaymentType.Card);
                    DnMain.BY_CASH = Convert.ToDecimal(Constants.PaymentType.Cash);
                    DnMain.party_code = vm.DocumentMain.SuplCode;
                    DnMain.ADVANCE = Convert.ToInt32(vm.DocumentMain.Advance);
                    DnMain.CASH_AMT = Convert.ToDecimal(vm.DocumentMain.TotalAmount);
                    DnMain.DISC_AMNT = Convert.ToDecimal(vm.DocumentMain.DiscointAmount);
                    DnMain.ADVANCE = Convert.ToInt32(vm.DocumentMain.Advance);
                    if (vm.DocumentMain.Phone != null)
                    {
                        DnMain.MOB = vm.DocumentMain.Phone.Trim();
                    }
                    DnMain.CANCEL = "";
                    DnMain.TYPE = "A";
                    DnMain.BY_CASH = Convert.ToDecimal(vm.DocumentMain.Payment);
                    DnMain.USER_ID = vm.DocumentMain.Userid ?? "";

                    if (vm.DocumentMain.ReturnAmount == null)
                    {
                        DnMain.RET_AMT = 0;//Convert.ToInt32(vm.ReturnAmount);
                    }
                    else
                    {
                        DnMain.RET_AMT = Convert.ToDecimal(vm.DocumentMain.ReturnAmount);
                    }
                    db.DN_MAIN.Add(DnMain);
                    db.SaveChanges();
                    foreach (var item in vm.DocumentDetailList)
                    {
                        var DnDetail = new DN_DETAIL();
                        DnDetail.AMOUNT = item.Amount;
                        DnDetail.BARCODE = item.Barcode;
                        DnDetail.DN_NO = DnMain.DN_NO;
                        DnDetail.EXCH_FLAG = "T";
                        DnDetail.FREE_QTY = item.FreeQty;
                        DnDetail.GST_AMOUNT = 0;
                        DnDetail.TRANSFER_STATUS = "0";
                        DnDetail.UNITS_SOLD = item.Qty;
                        DnDetail.UNIT_COST = item.Cost;
                        DnDetail.UNIT_RETAIL = item.Retail;
                        DnDetail.VOID = "";
                        DnDetail.WAREHOUSE = "";
                        DnDetail.UAN_NO = item.Uanno;
                        var totalamount = item.Retail * item.Qty;
                        DnDetail.DIS_AMOUNT = (item.Discount / totalamount) * 100;
                        DnDetail.S_TIME = item.DateTimeSlot;
                        DnDetail.SUPL_CODE = item.staffcode;
                        db.DN_DETAIL.Add(DnDetail);
                    }
                    db.SaveChanges();
                    var saveSales = new Api.SalesController();

                    vm.DocumentMain.DnNo = DnMain.DN_NO;
                    vm.DocumentMain.RequestPage = Constants.SalesPage.Index;

                    var docNo = saveSales.CreateSales(vm);
                    transaction.Commit();

                    var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", new { docType = "IN", requestPage = "OrderBooking" });
                    return Json(new { Url = redirectUrl });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string s = ex.ToString();
                    return Content("Error");
                }
            }

        }
        public ActionResult DnList()
        {
            var VM = db.DN_MAIN.Select(x => new DocumentListViewModel { DocNo = x.DN_NO, Doc = x.DN_DATE, Supplier = x.CUST_NAME, Amount = x.CASH_AMT ?? 0 }).ToList();
            return View(VM);
        }
        [Permission("Edit Document")]
        public ActionResult DnEdit(string docno)
        {
            if (!string.IsNullOrEmpty(docno))
            {
                var TransNo = db.TRANS_MN.Where(x => x.DN_NO == docno).Select(x => x.TRANS_NO).SingleOrDefault();
                if (TransNo != null)
                    return RedirectToAction("Edit", new { docno = TransNo, requestPage = SalesPage.Index });
                else
                    return RedirectToAction("DnList");
            }
            else
            {
                return View("DnList");
            }
        }

        public ActionResult Process(string bCode)
        {
            if (!string.IsNullOrEmpty(bCode))
            {
                var sales = db.TRANS_MN.Where(x => x.TRANS_NO == bCode).SingleOrDefault();
                if (sales != null)
                {
                    sales.STATUS = DocumentStatus.Processing;
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);

                }
                return Json("NotFound", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Complete(string bCode)
        {
            if (!string.IsNullOrEmpty(bCode))
            {
                var sales = db.TRANS_MN.Where(x => x.TRANS_NO == bCode).SingleOrDefault();
                if (sales != null)
                {
                    sales.STATUS = DocumentStatus.Completed;
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);

                }
                return Json("NotFound", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Dispatch(string bCode)
        {
            if (!string.IsNullOrEmpty(bCode))
            {
                var sales = db.TRANS_MN.Where(x => x.TRANS_NO == bCode).SingleOrDefault();
                if (sales != null)
                {
                    sales.STATUS = DocumentStatus.Dispatch;
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);

                }
                return Json("NotFound", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
        }






    }
}
