using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels.ReportsViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Inventory.Controllers
{
    [Authorize]
    public class DGSController : Controller
    {
        DataContext db;
        public DGSController()
        {
            db = new DataContext();
        }
        // GET: DGS
        [Permission("DGS")]
        public ActionResult Index()
        {
           
            var Departmentlist = db.DEPARTMENTs.OrderBy(X => X.DEPT_CODE).ToList();
            
            ViewBag.Departmentlist = Departmentlist;
            PurchaseReportViewModel obj = new PurchaseReportViewModel();
            var reportMain = new PurchaseReportMainViewModel()
            {
                Groups = db.GROUPS.OrderBy(X=>X.GROUP_CODE).ToList(),
                Sub_Groups = db.SUB_GROUPS.OrderBy(X => X.SGROUP_CODE).ToList(),
               
            };
            obj.ReportMain = reportMain;
            return View(obj);
        }
        public JsonResult FillGroup(string id)
        {
            var group = (from a in db.GROUPS where a.DEPT_CODE == id select new { a.GROUP_CODE, a.GROUP_NAME }).OrderBy(X=>X.GROUP_CODE).ToList();//db.DESIGNATIONs.Where(c => c.DEPT_CODE == id).ToList();
            int count = group.Count();
            //if (desig == null)
            //    return HttpNotFound();

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
            var subGroup = (from a in db.SUB_GROUPS where a.DEPT_CODE == id.Substring(0, 2) && a.GROUP_CODE == id.Substring(2, 2) select new { a.SGROUP_CODE, a.SGROUP_NAME }).OrderBy(X => X.SGROUP_CODE).ToList();//db.DESIGNATIONs.Where(c => c.DEPT_CODE == id).ToList();
            var value = JsonConvert.SerializeObject(subGroup, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        [HttpPost]
        public ActionResult UpdateDepartment(DEPARTMENT dep)
        {
            try
            {
                var v = db.DEPARTMENTs.Where(x => x.DEPT_CODE == dep.DEPT_CODE).FirstOrDefault() ;
                v.DEPT_NAME = dep.DEPT_NAME;
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                v.STATUS = "A";
                v.TRANSFER_STATUS = "0";
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        [HttpPost]
        public ActionResult UpdateGroup (GROUP group)
        {
            try
            {
                var v = db.GROUPS.Where(x => x.GROUP_CODE == group.GROUP_CODE && x.DEPT_CODE==group.DEPT_CODE).FirstOrDefault();
                v.GROUP_NAME = group.GROUP_NAME;   
                v.DEPT_CODE = group.DEPT_CODE;
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.TRANSFER_STATUS = "0";
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                v.STATUS = "A";
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
        public ActionResult UpdateSubGroup (SUB_GROUPS subGroups)
        {
            try
            {
                var v = db.SUB_GROUPS.Where(x=>x.SGROUP_CODE==subGroups.SGROUP_CODE&&x.GROUP_CODE==subGroups.GROUP_CODE&&x.DEPT_CODE==subGroups.DEPT_CODE).FirstOrDefault();
                v.SGROUP_CODE = subGroups.SGROUP_CODE;
                v.SGROUP_NAME = subGroups.SGROUP_NAME;
                v.DEPT_CODE = subGroups.DEPT_CODE;
                v.GROUP_CODE = subGroups.GROUP_CODE;
                v.TRANSFER_STATUS = "0";
                v.STATUS = "A";
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                db.SaveChanges();
                return Content("ok");
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return View();
            }
        }
    }
}