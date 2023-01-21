using Inventory.ViewModels.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Inventory.ViewModels.Item;

namespace Inventory.Controllers
{
    public class OrderController : Controller
    {
        DataContext db;
        public OrderController()
        {
            db = new DataContext();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var VM = OrderList();
            return View(VM);
        }

        public OrderProcessingViewModel OrderList()
        {

   

            OrderProcessingViewModel VM = new OrderProcessingViewModel();
            VM.OC = OrderCount();
            VM.list = new List<OrderDocument>();
            var lst = db.TRANS_MN.Include(x => x.TRANS_DT).Include(x => x.PARTY_ADDRESS).Include(x => x.SUPPLIER)
       .Where(x => x.SALE_TYPE == Constants.SalesPage.WebStore && x.STATUS == Constants.DocumentStatus.UnauthorizedDocument).ToList();

            foreach (var sales in lst)
            {
                var OrderDocument = new OrderDocument();
                OrderDocument.PurchasedItems = new List<ItemViewModel>();
                OrderDocument.Action = false;
                OrderDocument.CustomerMessage = "";
                OrderDocument.Date = sales.TRANS_DATE.ToShortDateString();
                OrderDocument.DeliveryDate = sales.DELIVERY_DATE.HasValue ? sales.DELIVERY_DATE.Value.ToShortDateString() : "";
                OrderDocument.OrderNo = sales.TRANS_NO;
                OrderDocument.OrderNote = "";
                OrderDocument.ShipTo = sales.ADDRESS_ID.HasValue ? sales.PARTY_ADDRESS.ADDRESS : sales.SUPPLIER.ADDRESS;
                OrderDocument.Status = GetStatus(sales.STATUS);
                OrderDocument.Total = 0m;
                foreach (var detail in sales.TRANS_DT)
                {
                    var purchase = new ItemViewModel();
                    purchase.Barcode = detail.BARCODE;
                    purchase.Cost = detail.UNIT_COST;
                    purchase.Retail = detail.UNIT_RETAIL;
                    purchase.Description = detail.PRODUCT.DESCRIPTION;
                    purchase.Qty = Convert.ToInt32(detail.UNITS_SOLD);
                    OrderDocument.PurchasedItems.Add(purchase);
                    OrderDocument.Total += (detail.UNITS_SOLD) * (detail.UNIT_RETAIL);
                }
                VM.list.Add(OrderDocument);
            }

        //                public static readonly string UnauthorizedDocument = "0";
        //public static readonly string Pending = "1";
        //public static readonly string Deleted = "2";
        //public static readonly string AuthorizedDocument = "3";
        //public static readonly string Processing = "4";
        //public static readonly string Dispatch = "5";
        //public static readonly string Onhold = "6";
        //public static readonly string Completed = "7";
        //public static readonly string Cancelled = "8";


            //var data = (from sales in db.TRANS_MN.Where(x => x.SALE_TYPE == Constants.SalesPage.WebStore)
            //        from detail in db.TRANS_DT.Where(x => x.TRANS_NO == sales.TRANS_NO)
            //        from ship in db.PARTY_ADDRESS.Where(x => x.ID == sales.ADDRESS_ID)
            //        select new OrderDocument
            //        {
            //                = Action = false,
            //            CustomerMessage = sales.TRANS_NO,
            //            Date = sales.TRANS_NO,
            //            OrderNo = sales.TRANS_NO,
            //            OrderNote = sales.TRANS_NO,
            //            PurchasedItems = new List<ViewModels.Item.ItemViewModel>(),
            //            ShipTo = ship.ADDRESS,
            //            Status = sales.STATUS == "2" ? "Deleted" :
            //              sales.STATUS == "4" ? "Processing" :
            //              sales.STATUS == "5" ? "Dispatch" :
            //              sales.STATUS == "6" ? "Onhold" :
            //              sales.STATUS == "7" ? "Completed" :
            //              sales.STATUS == "8" ? "Cancelled" : "",
            //            Total = sales.TRANS_NO,

            //        }).ToList();



            return VM;
        }
        public OrderCount OrderCount()
        {
            OrderCount oc = new OrderCount();
            oc.AllCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore select s).Count();
            oc.CancelledCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.Cancelled select s).Count();
            oc.CompletedCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.Completed select s).Count();
            oc.DispatchCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.Dispatch select s).Count();
            oc.OnholdCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.UnauthorizedDocument select s).Count();
            oc.ProcessingCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.Processing select s).Count();
            oc.TrashCount = (from s in db.TRANS_MN where s.SALE_TYPE == Constants.SalesPage.WebStore && s.STATUS == Constants.DocumentStatus.Deleted select s).Count();
            return oc;
        }
        public string GetStatus(string str)
        {
            var Status = "";
            if (str == Constants.DocumentStatus.Cancelled)
            {
                Status = "Cancelled";
            }
            else if (str == Constants.DocumentStatus.Completed)
            {
                Status = "Completed";
            }
            else if (str == Constants.DocumentStatus.Deleted)
            {
                Status = "Deleted";
            }
            else if (str == Constants.DocumentStatus.Dispatch)
            {
                Status = "Dispatch";
            }
            else if (str == Constants.DocumentStatus.UnauthorizedDocument)
            {
                Status = "On Hold";
            }
            else if (str == Constants.DocumentStatus.Processing)
            {
                Status = "Processing";
            }
           
            return Status;
        }

    }
}