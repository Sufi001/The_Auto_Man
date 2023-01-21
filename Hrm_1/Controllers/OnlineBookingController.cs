using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    public class OnlineBookingController : Controller
    {
        private readonly DataContext db;
        public OnlineBookingController()
        {
            db = new DataContext();
        }
        // GET: OnlineBooking
        public ActionResult OnlineBooking ()
        {
            try
            {
                //if (obj1.Authenticate() == false)
                //{
                //    return RedirectToAction("Login", "Account");
                //}
                return View(GetSalesPageViewModel());
            }
            catch (System.Exception)
            {
                return Content("<p>Page is not working.</p>");
            }
        }
        public OnlineBookingViewModel GetSalesPageViewModel()
        {
            OnlineBookingViewModel obj = new OnlineBookingViewModel();
            obj.DocDate = DateTime.Now;
            obj.ItemList = db.PRODUCTS.Where(x => x.FLASH_PACK == "S").OrderBy(x=>x.DESCRIPTION).Select(u => new ItemViewModel { Barcode = u.BARCODE, Description = u.DESCRIPTION, Cost = u.UNIT_COST }).ToList();
            obj.Providerlists = db.STAFF_MEMBER.Select(u => new OnlineBookingViewModel {providercode=u.SUPL_CODE,providerName=u.SUPL_NAME }).ToList();
            return obj;
        }
        //[HttpPost]
        //public ActionResult OnlineBookingSave(OnlineBookingViewModel vm)
        // {
        //    try
        //    {
        //        if (!ModelState.IsValid || vm.DetailBooking.Count == 0)
        //        {
        //            return Content("Null");
        //        }
        //        var docno = SaveSalesMain(vm);
        //        if (docno != "" && docno != Constants.Constants.UnauthorizedDocument)
        //        {
        //         if(SaveSalesDetail(vm.DetailBooking, docno))
        //            {
        //                db.SaveChanges();
        //                return Content(docno);
        //            }
        //            else
        //            {
        //                return Content("Null");
        //            }
                  
        //        }
        //        return Content("Null");
        //    }
        //    catch (System.Exception ex)
        //    {
        //        string s = ex.ToString();
        //     //   return Content(ex.ToString());
        //        return Content("Null");
        //    }
        //   // return View();
        //}
        //public string SaveSalesMain(OnlineBookingViewModel vm)
        //{
        //    try
        //    {
        //        var salesMain = new TRANS_OnMn();
        //        bool saveOrUpdateFlag = false;
        //        if (string.IsNullOrEmpty(vm.DocNo))
        //        {
        //            string prefix = "OB";
        //            salesMain.TRANS_NO = vm.GetSaleCode(prefix, 6);
        //            saveOrUpdateFlag = true;
        //        }
        //        else
        //        {
        //            salesMain = db.TRANS_OnMn.SingleOrDefault(u => u.TRANS_NO == vm.DocNo && u.status == Constants.Constants.UnauthorizedDocument);
        //            if (salesMain != null)
        //            {
        //                salesMain.USER_ID = "01";//vm.Userid;
        //                salesMain.TRANS_DATE = DateTime.Now;
        //            }
        //            else
        //                return Constants.Constants.UnauthorizedDocument;
        //        }
        //        //     salesMain.TRANS_DATE = vm.DocDate;
        //        salesMain.START_TIME = DateTime.Now;
        //        // salesMain.cust_id = vm.SuplCode;
        //        salesMain.status = Constants.Constants.UnauthorizedDocument;
        //        salesMain.TRANS_TYPE = "2";//(vm.DocType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn;
        //        salesMain.LOC_ID = "01"; //vm.Location;
        //        //salesMain.Warehouse = vm.Warehouse;
        //        salesMain.Time_Slot = vm.Time;
        //        salesMain.reference_person = vm.FirstName;
        //        //if(vm.Time==)
        //        salesMain.TILL_NO = "";
        //        salesMain.BY_CARD = Convert.ToDecimal(Constants.Constants.PaymentType.Card);
        //        salesMain.BY_CASH = Convert.ToDecimal(Constants.Constants.PaymentType.Cash);
        //        salesMain.party_code = "dummmy";//vm.SuplCode;

        //        salesMain.mob = vm.Phone;
        //        salesMain.CANCEL = "";
        //        salesMain.TRANS_DATE = vm.DocDate + vm.Time;
        //        salesMain.TYPE = "";
        //        salesMain.USER_ID = "01" ?? "";
        //        //updatecustomer(vm.SuplCode, vm.Phone);
        //        if (saveOrUpdateFlag)
        //        {
        //            db.TRANS_OnMn.Add(salesMain);
        //        }
        //        return salesMain.TRANS_NO;
        //    }
        //    catch (System.Exception)
        //    {
        //        return "Null";
        //    }
        //}
        //public bool SaveSalesDetail(List<OnlineBookingViewModel> vm, string docno)
        //{
        //    try
        //    {
               
        //        var salesDetailDbList = db.TRANS_OnDt.Where(u => u.TRANS_NO == docno).ToList();
        //        var tdinDbCount = salesDetailDbList.Count;
        //        if (tdinDbCount > 0)
        //        {
        //            db.TRANS_OnDt.RemoveRange(salesDetailDbList);
        //        }
        //        foreach (var item in vm)
        //        {
        //            var salesDetail = new TRANS_OnDt();
        //            salesDetail.TRANS_NO = docno;
        //            salesDetail.BARCODE = item.ItemCode;
        //            salesDetail.UNIT_COST = 200;//Convert.ToInt32(item.ItemPrice);
        //            salesDetail.UNIT_RETAIL = Convert.ToInt32(item.ItemPrice);//item.Retail;
        //            salesDetail.SCAN_TIME = DateTime.Now;
        //            salesDetail.UAN_NO = item.ItemCode; //item.Uanno;
        //            salesDetail.dis_amount = 0;//item.Discount;
        //            salesDetail.AMOUNT = Convert.ToInt32(item.TotalPrice);
        //            salesDetail.UNITS_SOLD = 1;//item.Qty;
        //            salesDetail.VOID = "";
        //            salesDetail.supl_code = item.providercode;

        //            salesDetail.EXCH_FLAG = "";
        //            salesDetail.GST_AMOUNT = 0;
        //            salesDetail.Colour = 1;//item.Colour;
        //            salesDetail.GST_AMOUNT = 1;
        //            salesDetail.VOID = "1";//item.Warehouse;
        //            salesDetail.TRANS_TYPE = "2"; //(docType == Constants.Constants.SalesDocument) ? Constants.Constants.TransType.Sales : Constants.Constants.TransType.SalesReturn; ;
        //            db.TRANS_OnDt.Add(salesDetail);
        //        }
        //        return true;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return false;
        //    }
        //}
    }
}