using Inventory.Helper;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft;
using System.Web.Mvc;
using Newtonsoft.Json;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [Authorize]
    public class GlChartController : Controller
    {
        DataContext db;
        public GlChartController()
        {
            db = new DataContext();
        }
        //Returning Index View
        [Permission("COA")]
        public ActionResult Index()
        {
            GlChartViewModel VM = new GlChartViewModel();
            //To Populate account types 
            VM.MainAccountList = MainAccountList();
            VM.ControlAccountList = new List<SelectListItem>();
            VM.SubControlAccountList = new List<SelectListItem>();
            VM.SubsidiaryControlAccountList = new List<SelectListItem>();
            return View(VM);
        }
        //To create new record or update existing record 
        [HttpPost]
        public ActionResult Save(GlChartViewModel main)
        {
            try
            {
                if (main == null && !ModelState.IsValid)
                {
                    TempData["fail"] = "Invalid Query";
                    return View("index", main);
                }

                GL_CHART MainAccount = new GL_CHART();
                if (string.IsNullOrEmpty(main.ACCOUNT_CODE))
                {
                    MainAccount.INSERTED_BY = CommonFunctions.GetUserName();
                    MainAccount.DOC = CommonFunctions.GetDateTimeNow();
                    MainAccount.ACCOUNT_TYPE = main.ACCOUNT_TYPE;

                    if (!NullOrEmpty(main.ACCOUNT_TYPE) && NullOrEmpty(main.MAIN_ACCOUNT_CODE) && NullOrEmpty(main.CONTROL_CODE) && NullOrEmpty(main.SUB_CONTROL_CODE))
                    {
                        MainAccount.ACCOUNT_CODE = GetMainAccountCode();
                        MainAccount.MAIN_ACCOUNT = null;
                        MainAccount.LEVEL_NO = "1";
                    }
                    else if (!NullOrEmpty(main.ACCOUNT_TYPE) && !NullOrEmpty(main.MAIN_ACCOUNT_CODE) && NullOrEmpty(main.CONTROL_CODE) && NullOrEmpty(main.SUB_CONTROL_CODE))
                    {
                        MainAccount.ACCOUNT_CODE = GetControlCode(main.MAIN_ACCOUNT_CODE);
                        MainAccount.MAIN_ACCOUNT = main.MAIN_ACCOUNT_CODE;
                        MainAccount.LEVEL_NO = "2";
                    }
                    else if (!NullOrEmpty(main.ACCOUNT_TYPE) && !NullOrEmpty(main.MAIN_ACCOUNT_CODE) && !NullOrEmpty(main.CONTROL_CODE) && NullOrEmpty(main.SUB_CONTROL_CODE))
                    {
                        MainAccount.ACCOUNT_CODE = GetSubControlCode(main.CONTROL_CODE);
                        MainAccount.MAIN_ACCOUNT = main.CONTROL_CODE;
                        MainAccount.LEVEL_NO = "3";
                    }
                    else if (!NullOrEmpty(main.ACCOUNT_TYPE) && !NullOrEmpty(main.MAIN_ACCOUNT_CODE) && !NullOrEmpty(main.CONTROL_CODE) && !NullOrEmpty(main.SUB_CONTROL_CODE))
                    {
                        MainAccount.ACCOUNT_CODE = GetSubsidiaryControlCode(main.SUB_CONTROL_CODE);
                        MainAccount.MAIN_ACCOUNT = main.SUB_CONTROL_CODE;
                        MainAccount.LEVEL_NO = "4";
                    }
                }
                else
                {
                    MainAccount = db.GL_CHART.Where(x => x.ACCOUNT_CODE == main.ACCOUNT_CODE).SingleOrDefault();
                    if (MainAccount == null)
                        return Content("Account Not Found");

                    //MainAccount.UPDATED_BY = CommonFunctions.GetUserName();
                    //MainAccount.UPDATE_DATE = DateTime.Now.Date;
                }

                MainAccount.UPDATED_BY = CommonFunctions.GetUserName();
                MainAccount.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                MainAccount.ACCOUNT_TITLE = main.ACCOUNT_TITLE;
                MainAccount.TITLE_URDU = main.TITLE_URDU;
                MainAccount.PHONE = main.PHONE;
                MainAccount.TRANSFER_STATUS = "0";
                MainAccount.OPEN_BAL = main.OPEN_BAL;
                MainAccount.OPEN_BAL_CR = main.OPEN_BAL_CR;
                MainAccount.ACCOUNT_CATEGORY = main.ACCOUNT_CATEGORY;

                MainAccount.CITY_CODE = null;
                MainAccount.PARTY_TYPE = null;

                if (string.IsNullOrEmpty(main.ACCOUNT_CODE))
                    db.GL_CHART.Add(MainAccount);

                db.SaveChanges();
                TempData["success"] = "Data Save Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["fail"] = "Data Not Save";
                string s = ex.ToString();
                return View("index", main);
            }
        }
        //List of Chart of Account
        public ActionResult List()
        {
            var List = db.GL_CHART.Select(x => new GlChartViewModel { ACCOUNT_CODE = x.ACCOUNT_CODE, ACCOUNT_TITLE = x.ACCOUNT_TITLE, LEVEL_NO = x.LEVEL_NO, ACCOUNT_TYPE = x.ACCOUNT_TYPE, TITLE_URDU = x.TITLE_URDU, MAIN_ACCOUNT = x.GL_CHART2.ACCOUNT_TITLE, OPEN_BAL = x.OPEN_BAL, OPEN_BAL_CR = x.OPEN_BAL_CR }).ToList();
            return View(List);
        }
        //public ActionResult Edit(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return Content("Bad Request");
        //    }
        //    GlChartViewModel VM = new GlChartViewModel();
        //    var account = db.GL_CHART.Where(x => x.ACCOUNT_CODE == id).SingleOrDefault();
        //    if (account != null)
        //    {
        //        VM.ACCOUNT_CODE = account.ACCOUNT_CODE;
        //        VM.ACCOUNT_TITLE = account.ACCOUNT_TITLE;
        //        //VM.MAIN_ACCOUNT = account.MAIN_ACCOUNT;
        //        VM.LEVEL_NO = account.LEVEL_NO; ;
        //        VM.ACCOUNT_CATEGORY = account.ACCOUNT_CATEGORY;
        //        VM.ACCOUNT_TYPE = account.ACCOUNT_TYPE;
        //        VM.OPEN_BAL = account.OPEN_BAL;
        //        VM.OPEN_BAL_CR = account.OPEN_BAL_CR;
        //        VM.CITY_CODE = account.CITY_CODE;
        //        VM.PHONE = account.PHONE;
        //        VM.PARTY_TYPE = account.PARTY_TYPE;
        //        VM.TITLE_URDU = account.TITLE_URDU;

        //        if (account.LEVEL_NO == "1")
        //        {

        //            VM.MainAccountList = MainAccountList();

        //            VM.MAIN_ACCOUNT_CODE = account.ACCOUNT_CODE;

        //            VM.ControlAccountList = new List<SelectListItem>();
        //            VM.SubControlAccountList = new List<SelectListItem>();
        //            VM.SubsidiaryControlAccountList = new List<SelectListItem>();
        //        }
        //        else if (account.LEVEL_NO == "2")
        //        {
        //            VM.MainAccountList = MainAccountList();
        //            VM.ControlAccountList = ControlAccountList();

        //            VM.MAIN_ACCOUNT_CODE = account.MAIN_ACCOUNT;
        //            VM.CONTROL_CODE = account.ACCOUNT_CODE;

        //            VM.SubControlAccountList = new List<SelectListItem>();
        //            VM.SubsidiaryControlAccountList = new List<SelectListItem>();
        //        }
        //        else if (account.LEVEL_NO == "3")
        //        {
        //            VM.MainAccountList = MainAccountList();
        //            VM.ControlAccountList = ControlAccountList();
        //            VM.SubControlAccountList = SubControlAccountList();

        //            VM.MAIN_ACCOUNT_CODE = account.GL_CHART2.GL_CHART2.ACCOUNT_CODE;
        //            VM.CONTROL_CODE = account.MAIN_ACCOUNT;
        //            VM.SUB_CONTROL_CODE = account.ACCOUNT_CODE;

        //            VM.SubsidiaryControlAccountList = new List<SelectListItem>();

        //        }
        //        else if (account.LEVEL_NO == "4")
        //        {
        //            VM.MainAccountList = MainAccountList();
        //            VM.ControlAccountList = ControlAccountList();
        //            VM.SubControlAccountList = SubControlAccountList();
        //            VM.SubsidiaryControlAccountList = SubsidiaryControlAccountList();

        //            VM.MAIN_ACCOUNT_CODE = account.GL_CHART2.GL_CHART2.GL_CHART2.ACCOUNT_CODE;
        //            VM.CONTROL_CODE = account.GL_CHART2.GL_CHART2.ACCOUNT_CODE;
        //            VM.SUB_CONTROL_CODE = account.MAIN_ACCOUNT;
        //            VM.SUBSIDIARY_CONTROL_CODE = account.ACCOUNT_CODE;
        //        }

        //        return View("Index", VM);
        //    }
        //    else
        //    {
        //        return RedirectToAction("List");
        //    }
        //}
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Content("Bad Request");
            }
            GlChartViewModel VM = new GlChartViewModel();
            var account = db.GL_CHART.Where(x => x.ACCOUNT_CODE == id).Select(x=>new {x.ACCOUNT_CODE, x.ACCOUNT_TITLE,x.TITLE_URDU,x.ACCOUNT_CATEGORY,x.ACCOUNT_TYPE,x.OPEN_BAL,x.OPEN_BAL_CR }).SingleOrDefault();
            if (account != null)
            {
                return Json(account, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Content("Bad Request");
            }
        }
        //generate level 1 account code
        public string GetMainAccountCode()
        {
            var code = db.GL_CHART.Where(x => x.LEVEL_NO == "1").Max(x => x.ACCOUNT_CODE);
            if (string.IsNullOrEmpty(code))
                return "1000000000";

            var subCode = code.Substring(0, 1);
            subCode = CommonFunctions.GenerateCode(subCode, 1);
            return CommonFunctions.PadRight(subCode, 10);
        }
        //generate level 2 account code
        public string GetControlCode(string parent)
        {
            var parentCode = parent.Substring(0, 1);
            //var code = db.GL_CHART.Where(x => x.LEVEL_NO == "2").Max(x => x.ACCOUNT_CODE);
            var code = (from da in db.GL_CHART
                        where da.ACCOUNT_CODE.StartsWith(parentCode) && da.LEVEL_NO == "2"
                        select new { da.ACCOUNT_CODE }
                       ).Max(x => x.ACCOUNT_CODE);

            if (string.IsNullOrEmpty(code))
                return parentCode + "010000000";

            var subCode = code.Substring(1, 2);
            subCode = parentCode + CommonFunctions.GenerateCode(subCode, 2);
            return CommonFunctions.PadRight(subCode, 10);
        }
        //generate level 3 account code
        public string GetSubControlCode(string parent)
        {
            var parentCode = parent.Substring(0, 3);
            //var code = db.GL_CHART.Where(x => x.LEVEL_NO == "3").Max(x => x.ACCOUNT_CODE);
            var code = (from da in db.GL_CHART
                        where da.ACCOUNT_CODE.StartsWith(parentCode) && da.LEVEL_NO == "3"
                        select new { da.ACCOUNT_CODE }
                        ).Max(x => x.ACCOUNT_CODE);

            if (string.IsNullOrEmpty(code))
                return parentCode + "0100000";

            var subCode = code.Substring(3, 2);
            subCode = parentCode + CommonFunctions.GenerateCode(subCode, 2);
            return CommonFunctions.PadRight(subCode, 10);
        }
        //generate level 4 account code
        public string GetSubsidiaryControlCode(string parent)
        {
            var parentCode = parent.Substring(0, 5);
            //var code = db.GL_CHART.Where(x => x.LEVEL_NO == "4").Max(x => x.ACCOUNT_CODE);
            var code = (from da in db.GL_CHART
                     where da.ACCOUNT_CODE.StartsWith(parentCode) && da.LEVEL_NO == "4"
                     select new {da.ACCOUNT_CODE}
                ).Max(x=>x.ACCOUNT_CODE);

            if (string.IsNullOrEmpty(code))
                return parentCode + "00001";

            var subCode = code.Substring(5, 5);
            return parentCode + CommonFunctions.GenerateCode(subCode, 5);
        }
        public List<SelectListItem> MainAccountList()
        {
            return db.GL_CHART.Where(x => x.LEVEL_NO == "1").Select(x => new SelectListItem { Text = x.ACCOUNT_TITLE, Value = x.ACCOUNT_CODE }).ToList();
        }
        public List<SelectListItem> ControlAccountList()
        {
            return db.GL_CHART.Where(x => x.LEVEL_NO == "2").Select(x => new SelectListItem { Text = x.ACCOUNT_TITLE, Value = x.ACCOUNT_CODE }).ToList();

        }
        public List<SelectListItem> SubControlAccountList()
        {

            return db.GL_CHART.Where(x => x.LEVEL_NO == "3").Select(x => new SelectListItem { Text = x.ACCOUNT_TITLE, Value = x.ACCOUNT_CODE }).ToList();

        }
        public List<SelectListItem> SubsidiaryControlAccountList()
        {
            return db.GL_CHART.Where(x => x.LEVEL_NO == "4").Select(x => new SelectListItem { Text = x.ACCOUNT_TITLE, Value = x.ACCOUNT_CODE }).ToList();
        }

        //Load chart of account and sub types from front end based on level (1 or 2 or 3 or 4) and Type (asset(A), expense(E), liability(L), revenue(R))
        //public JsonResult GetCOA(string level, string type, string parent)
        //{
        //    List<DdlTemplate> COA = new List<DdlTemplate>();
        //    if (string.IsNullOrEmpty(parent))
        //        COA = db.GL_CHART.Where(x => x.LEVEL_NO == level && x.ACCOUNT_TYPE == type).Select(x => new DdlTemplate { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
        //    else
        //        COA = db.GL_CHART.Where(x => x.LEVEL_NO == level && x.ACCOUNT_TYPE == type && x.MAIN_ACCOUNT == parent).Select(x => new DdlTemplate { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();

        //    if (COA != null)
        //        return Json(COA, JsonRequestBehavior.AllowGet);
        //    else
        //        return Json("", JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetCOA(string level, string parent)
        {
            List<DdlTemplate> COA = new List<DdlTemplate>();
            if (string.IsNullOrEmpty(parent))
                COA = db.GL_CHART.Where(x => x.LEVEL_NO == level).Select(x => new DdlTemplate { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();
            else
                COA = db.GL_CHART.Where(x => x.LEVEL_NO == level && x.MAIN_ACCOUNT == parent).Select(x => new DdlTemplate { Value = x.ACCOUNT_CODE, Text = x.ACCOUNT_TITLE }).ToList();

            if (COA != null)
                return Json(COA, JsonRequestBehavior.AllowGet);
            else
                return Json("", JsonRequestBehavior.AllowGet);
        }

        public bool NullOrEmpty(string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}