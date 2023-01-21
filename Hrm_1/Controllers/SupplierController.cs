using Inventory.Filters;
using Inventory.Helper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Security;
using System.Collections.Generic;
using Inventory.ViewModel;

namespace Inventory.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {

        public ActionResult project()
        {
            CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
            obj.CustomerSupplierList = GetSuppliers();
            obj.LOCATION = CommonFunctions.GetLocation();
            obj.LocationList = db.LOCATIONs.ToList();
            obj.CityList = db.CITies.ToList();
            obj.AreaList = db.AREAs.ToList();
            return View(obj);

        }
        //Used
        readonly DataContext db;
        public SupplierController()
        {
            db = new DataContext();
        }
        [Permission("Supplier")]
        public ActionResult Suppliers()
        {
            CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
            obj.CustomerSupplierList = GetSuppliers();
            obj.LOCATION = CommonFunctions.GetLocation();
            obj.LocationList = db.LOCATIONs.ToList();
            obj.CityList = db.CITies.ToList();
            obj.AreaList = db.AREAs.ToList();
            return View(obj);
        }
        [HttpPost]
        public ActionResult Save(CustomerSupplierViewModel VM)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                VM.CustomerSupplierList = GetSuppliers();
                VM.LOCATION = CommonFunctions.GetLocation();
                VM.LocationList = db.LOCATIONs.ToList();
                VM.CityList = db.CITies.ToList();
                VM.AreaList = db.AREAs.Where(x=>x.AREA_CODE == VM.AREA).ToList();

                if (!ModelState.IsValid)
                {
                    TempData["message"] = "fail";
                    return View("Suppliers", VM);
                }

                var supplier = new SUPPLIER();
                GL_CHART Account = new GL_CHART();
                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                {
                    supplier.INSERTED_BY = CommonFunctions.GetUserName();
                    supplier.DOC = CommonFunctions.GetDateTimeNow();
                    supplier.SUPL_CODE = GetSupplierCode();
                    supplier.PARTY_TYPE = Constants.CustomerSupplier.Supplier;

                    GlChartController gl = new GlChartController();
                    Account.INSERTED_BY = CommonFunctions.GetUserName();
                    Account.DOC = CommonFunctions.GetDateTimeNow();
                    Account.ACCOUNT_TYPE = "L";
                    Account.ACCOUNT_CODE = gl.GetSubsidiaryControlCode("1020400000");
                    Account.MAIN_ACCOUNT = "1020400000";
                    Account.LEVEL_NO = "4";
                    Account.ACCOUNT_TITLE = VM.NAME;
                    Account.PARTY_TYPE = Constants.CustomerSupplier.Supplier;
                    Account.TRANSFER_STATUS = "0";
                    Account.ACCOUNT_CATEGORY = null;
                    db.GL_CHART.Add(Account);
                    db.SaveChanges();

                    supplier.ACCOUNT_CODE = Account.ACCOUNT_CODE;
                }
                else
                {
                    supplier = db.SUPPLIERs.SingleOrDefault(c => c.SUPL_CODE == VM.SUPL_CODE && c.PARTY_TYPE == Constants.CustomerSupplier.Supplier);

                    //supplier.UPDATED_BY = CommonFunctions.GetUserName();
                    //supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                    //Title Update On Supplier Update
                    Account = db.GL_CHART.Where(x => x.ACCOUNT_CODE == supplier.ACCOUNT_CODE).SingleOrDefault();
                    if (Account != null)
                    {
                        Account.UPDATED_BY = CommonFunctions.GetUserName();
                        Account.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                        Account.ACCOUNT_TITLE = VM.NAME;
                        Account.TRANSFER_STATUS = "0";
                        db.SaveChanges();

                    }
                    //Incase Supplier Already Exist But Does Not Have Any Account  
                    else
                    {

                        Account = new GL_CHART();
                        GlChartController gl = new GlChartController();
                        Account.INSERTED_BY = CommonFunctions.GetUserName();
                        Account.DOC = CommonFunctions.GetDateTimeNow();
                        Account.UPDATED_BY = CommonFunctions.GetUserName();
                        Account.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                        Account.ACCOUNT_TYPE = "L";
                        Account.ACCOUNT_CODE = gl.GetSubsidiaryControlCode("1020400000");
                        Account.MAIN_ACCOUNT = "1020400000";
                        Account.LEVEL_NO = "4";
                        Account.ACCOUNT_TITLE = VM.NAME;
                        Account.PARTY_TYPE = Constants.CustomerSupplier.Supplier;
                        Account.TRANSFER_STATUS = "0";
                        Account.ACCOUNT_CATEGORY = null;
                        db.GL_CHART.Add(Account);
                        db.SaveChanges();

                        supplier.ACCOUNT_CODE = Account.ACCOUNT_CODE;
                    }
                }

                supplier.UPDATED_BY = CommonFunctions.GetUserName();
                supplier.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                supplier.SUPL_NAME = VM.NAME;
                supplier.PAY_DAYS = VM.PayDays;
                supplier.PAY_TERMS = VM.PayTerms;
                supplier.ADDRESS = VM.ADDRESS;
                supplier.PHONE = VM.Phone;
                supplier.MOBILE = VM.Mobile;
                supplier.EMAIL = VM.EMAIL;
                supplier.CNIC = VM.CNIC;
                supplier.STN_NO = VM.STRN;
                supplier.LOCATION = VM.LOCATION;
                supplier.AREA_CODE = VM.AREA;
                supplier.CITY_CODE = VM.CITY;
                supplier.NAME_URDU = VM.UrduName;
                supplier.OPENING = VM.OpeningBalance ?? 0;
                supplier.CONTACT_PERSON = VM.Contact_Person;
                supplier.PHONE2 = VM.Phone2;

                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                    db.SUPPLIERs.Add(supplier);
                
                db.SaveChanges();

            transaction.Commit();
                TempData["message"] = "success";
                return RedirectToAction("Suppliers");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                transaction.Rollback();
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                TempData["message"] = "fail";
                return View("Suppliers", VM);
            }
        }
        [HttpGet]
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            try
            {
                var Supplier = db.SUPPLIERs.Include(x => x.AREA).SingleOrDefault(c => c.SUPL_CODE == id && c.PARTY_TYPE == "s");
                if (Supplier == null)
                    return HttpNotFound();
                var SupplierVM = new CustomerSupplierViewModel();
                SupplierVM.SUPL_CODE = Supplier.SUPL_CODE;
                SupplierVM.NAME = Supplier.SUPL_NAME;
                SupplierVM.ADDRESS = Supplier.ADDRESS;
                SupplierVM.Phone = Supplier.PHONE;
                SupplierVM.Phone2 = Supplier.PHONE2;
                SupplierVM.Mobile = Supplier.MOBILE;
                SupplierVM.UrduName = Supplier.NAME_URDU;
                SupplierVM.EMAIL = Supplier.EMAIL;
                SupplierVM.CITY = Supplier.CITY_CODE;
                SupplierVM.AREA = Supplier.AREA_CODE;
                SupplierVM.STRN = Supplier.STN_NO;
                SupplierVM.CNIC = Supplier.CNIC;
                SupplierVM.account_code = Supplier.ACCOUNT_CODE;
                SupplierVM.LOCATION = Supplier.LOCATION;
                SupplierVM.Balance = Supplier.BALANCE;
                SupplierVM.OpeningBalance = Supplier.OPENING;
                SupplierVM.PayTerms = Supplier.PAY_TERMS;
                SupplierVM.PayDays = Supplier.PAY_DAYS;
                SupplierVM.Contact_Person = Supplier.CONTACT_PERSON;
                if (!string.IsNullOrEmpty(Supplier.AREA_CODE))
                    SupplierVM.AREA_NAME = Supplier.AREA.AREA_NAME;

                var value = JsonConvert.SerializeObject(SupplierVM, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception ex)
            {
                return Content("Something went wrong");
            }
        }
        public string GetSupplierCode()
        {
            string s = db.SUPPLIERs.Max(u => u.SUPL_CODE);

            if (string.IsNullOrEmpty(s))
            {
                return "000001";
            }
            string code = CommonFunctions.GenerateCode(s, 6);
            return code;
        }
        public IEnumerable<CustomerSupplierViewModel> GetSuppliers()
        {
            return db.SUPPLIERs.Include(x => x.CITY).Include(x => x.AREA).Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Supplier)
                .Select(u => new CustomerSupplierViewModel
                {
                    SUPL_CODE = u.SUPL_CODE,
                    SUPL_NAME = u.SUPL_NAME,
                    ADDRESS = u.ADDRESS,
                    Phone = u.MOBILE,
                    Balance = ((u.BALANCE ?? 0) + (u.OPENING ?? 0)),
                    AREA = u.AREA.AREA_NAME,
                    CITY = u.CITY.CITY_NAME,
                    UrduName = u.NAME_URDU
                }).ToList();
        }
    }
}