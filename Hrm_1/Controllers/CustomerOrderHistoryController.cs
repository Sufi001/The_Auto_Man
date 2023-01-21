using Inventory.Helper;
using Inventory.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class CustomerOrderHistoryController : Controller
    {
        // GET: CustomerOrderHistory
        DataContext db;
        public CustomerOrderHistoryController()
        {
            db = new DataContext();
        }
        public ActionResult Index()
        {
            CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
            obj.CustomerSupplierList = GetCustomers();
            obj.LOCATION = CommonFunctions.GetLocation();
            obj.LocationList = db.LOCATIONs.ToList();
            return View(obj);
        }
        public IEnumerable<CustomerSupplierViewModel> GetCustomers()                                                
        {
            return (db.CUST_DETAIL.Select(u => new CustomerSupplierViewModel
            {
                CUST_CODE = u.CUST_CODE,
                NAME = u.NAME,
                ADDRESS = u.ADDRESS,
                Phone = u.MOBILE,
                EMAIL = u.EMAIL,
                TransNo=u.TRANS_NO,
            }).ToList());
        }

        public ActionResult Edit(string id)
        {
            try
            {
                var list = (from u in db.CUST_DETAIL
                            join tr in db.TRANS_MN on
                            u.TRANS_NO equals tr.TRANS_NO
                            where tr.TRANS_NO==id
                            select new CustomerSupplierViewModel
                            {
                                CUST_CODE = u.CUST_CODE,
                                NAME = u.NAME,
                                ADDRESS = u.ADDRESS,
                                Phone = u.MOBILE,
                                EMAIL = u.EMAIL,
                                RedemPoint = u.REDEMPOINT,
                                TransPoint = u.TRANSPOINT,
                                TransNo = tr.TRANS_NO,
                                TransDate = tr.TRANS_DATE,
                                TransAmount =tr.BY_CASH,
                            }).FirstOrDefault();
                if (list == null)
                    return HttpNotFound();
                var jsonResult = Json(list, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
    }
}