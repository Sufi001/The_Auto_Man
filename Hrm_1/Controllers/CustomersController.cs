using Inventory.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using Inventory.ViewModel;
using Inventory.ViewModels;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        // GET: Customers

        DataContext db;
        public CustomersController()
        {
            db = new DataContext();
        }
        [Permission("Customer")]
        public ActionResult Customers()
        {
            CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
            obj.CustomerSupplierList = GetCustomers();
            obj.LOCATION = CommonFunctions.GetLocation();
            obj.LocationList = db.LOCATIONs.ToList();
            obj.CityList = db.CITies.ToList();
            obj.AreaList = db.AREAs.ToList();
            obj.Discountcategorylist = db.CUSTOMER_DISCOUNT_CATEGORY.ToList();
            return View(obj);
        }
        [HttpPost]
        public ActionResult save(CustomerSupplierViewModel VM)
        {
            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                VM.CustomerSupplierList = GetCustomers(); 
                VM.LOCATION = CommonFunctions.GetLocation();
                VM.LocationList = db.LOCATIONs.ToList();
                VM.CityList = db.CITies.ToList();
                VM.AreaList = db.AREAs.Where(x => x.AREA_CODE == VM.AREA).ToList();
                VM.Discountcategorylist = db.CUSTOMER_DISCOUNT_CATEGORY.ToList();

                if (!ModelState.IsValid)
                {
                    TempData["message"] = "fail";
                    return View("Customers", VM);
                }

                if (!string.IsNullOrEmpty(VM.Mobile))
                {
                    bool IsCustomerExistWithThisMobile = false;
                    if (string.IsNullOrEmpty(VM.SUPL_CODE))
                        IsCustomerExistWithThisMobile = db.SUPPLIERs.Any(x => x.MOBILE == VM.Mobile && x.PARTY_TYPE == Constants.CustomerSupplier.Customer);
                    else
                        IsCustomerExistWithThisMobile = db.SUPPLIERs.Any(x => x.MOBILE == VM.Mobile && x.SUPL_CODE != VM.SUPL_CODE && x.PARTY_TYPE == Constants.CustomerSupplier.Customer);

                    if (IsCustomerExistWithThisMobile)
                    {
                       TempData["message"] = "Users Already Exist With This Mobile Number";
                       return View("Customers", VM);
                    }
                }

                var Customer = new SUPPLIER();
                GL_CHART Account = new GL_CHART();

                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                {
                    Customer.SUPL_CODE = GetCustomerCode();
                    Customer.INSERTED_BY = CommonFunctions.GetUserName();
                    Customer.DOC = CommonFunctions.GetDateTimeNow();
                    Customer.PARTY_TYPE = Constants.CustomerSupplier.Customer;

                    GlChartController gl = new GlChartController();
                    Account.INSERTED_BY = CommonFunctions.GetUserName();
                    Account.DOC = DateTime.Now.Date;

                    Account.UPDATED_BY = CommonFunctions.GetUserName();
                    Account.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                    Account.ACCOUNT_TYPE = "A";
                    Account.ACCOUNT_CODE = gl.GetSubsidiaryControlCode("2010100000");
                    Account.MAIN_ACCOUNT = "2010100000";
                    Account.LEVEL_NO = "4";
                    Account.ACCOUNT_TITLE = VM.NAME;
                    Account.PARTY_TYPE = Constants.CustomerSupplier.Customer;
                    Account.TRANSFER_STATUS = "0";
                    Account.ACCOUNT_CATEGORY = null;
                    db.GL_CHART.Add(Account);
                    db.SaveChanges();

                    Customer.ACCOUNT_CODE = Account.ACCOUNT_CODE;
                }
                else
                {
                    Customer = db.SUPPLIERs.Single(c => c.SUPL_CODE == VM.SUPL_CODE && c.PARTY_TYPE == Constants.CustomerSupplier.Customer);
                    Customer.UPDATED_BY = CommonFunctions.GetUserName();
                    Customer.UPDATE_DATE = DateTime.Now;

                    var eventupdate = db.TRANS_MN.Where(x => x.PARTY_CODE == VM.SUPL_CODE).ToList();
                    if (eventupdate != null && eventupdate.Count > 0)
                    {
                        eventupdate.ForEach(x => x.MOB = VM.Mobile);
                        db.SaveChanges();
                    }

                    //Title Update On Customer Update
                    Account = db.GL_CHART.Where(x => x.ACCOUNT_CODE == Customer.ACCOUNT_CODE).SingleOrDefault();
                    if (Account != null)
                    {
                        Account.UPDATED_BY = CommonFunctions.GetUserName();
                        Account.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                        Account.ACCOUNT_TITLE = VM.NAME;
                        Account.TRANSFER_STATUS = "0";
                        db.SaveChanges();

                    }
                    //Incase Customer Already Exist But Does Not Have Any Account  
                    else
                    {
                        Account = new GL_CHART();
                        GlChartController gl = new GlChartController();
                        Account.INSERTED_BY = CommonFunctions.GetUserName();
                        Account.DOC = CommonFunctions.GetDateTimeNow();

                        Account.UPDATED_BY = CommonFunctions.GetUserName();
                        Account.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                        Account.ACCOUNT_TYPE = "A";
                        Account.ACCOUNT_CODE = gl.GetSubsidiaryControlCode("2010100000");
                        Account.MAIN_ACCOUNT = "2010100000";
                        Account.LEVEL_NO = "4";
                        Account.ACCOUNT_TITLE = VM.NAME;
                        Account.PARTY_TYPE = Constants.CustomerSupplier.Customer;
                        Account.TRANSFER_STATUS = "0";
                        Account.ACCOUNT_CATEGORY = null;
                        db.GL_CHART.Add(Account);
                        db.SaveChanges();

                        Customer.ACCOUNT_CODE = Account.ACCOUNT_CODE;
                    }
                }

                Customer.SUPL_NAME = VM.NAME;
                Customer.COMMENTS = VM.Comments;
                Customer.ADDRESS = VM.ADDRESS;
                Customer.PHONE = VM.Phone;
                Customer.MOBILE = VM.Mobile;
                Customer.EMAIL = VM.EMAIL;
                Customer.CNIC = VM.CNIC;
                Customer.STN_NO = VM.STRN;
                Customer.LOCATION = VM.LOCATION;
                Customer.NAME_URDU = VM.UrduName;
                Customer.AREA_CODE = VM.AREA;
                Customer.CITY_CODE = VM.CITY;
                Customer.TRANSFER_STATUS = "0";
                Customer.OPENING = VM.OpeningBalance;
                Customer.PAY_TERMS = VM.Terms;
                Customer.PHONE2 = VM.Phone2;
                Customer.CONTACT_PERSON = VM.Contact_Person;
                Customer.BUSINESS_NAME = VM.BusinessName;
                Customer.BUSINESS_URL = VM.BusinessUrl;
                Customer.LAT = VM.Latitude;
                Customer.LON = VM.Longitude;

                if (!string.IsNullOrEmpty(VM.DiscountCategory))
                    Customer.DISC_CATEGORY_ID = Convert.ToInt32(VM.DiscountCategory);
                
                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                    db.SUPPLIERs.Add(Customer);

                if (VM.Picture != null)
                {
                    var ImageNameWithExtension = VM.Picture.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = VM.Mobile + "." + GettingExtension[1];
                    var path = Server.MapPath("~/images/Customer/" + fileName);
                    VM.Picture.SaveAs(path);
                }

                if (VM.CnicPic != null)
                {
                    var ImageNameWithExtension = VM.CnicPic.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = VM.Mobile + "." + GettingExtension[1];
                    var path = Server.MapPath("~/images/Cnic/" + fileName);
                    VM.Picture.SaveAs(path);
                }

                db.SaveChanges();

                transaction.Commit();
                TempData["message"] = "success";
                return RedirectToAction("Customers");
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
            }
            TempData["message"] = "fail";
            return View("Customers", VM);
        }
        [HttpGet]
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            try
            {
                var customerFormViewModel = db.SUPPLIERs.Include(x => x.AREA).SingleOrDefault(c => c.SUPL_CODE == id && c.PARTY_TYPE == Constants.CustomerSupplier.Customer);
                if (customerFormViewModel == null)
                    return HttpNotFound();

                var Customer = new CustomerSupplierViewModel();
                Customer.SUPL_CODE = customerFormViewModel.SUPL_CODE;
                Customer.NAME = customerFormViewModel.SUPL_NAME;
                Customer.ADDRESS = customerFormViewModel.ADDRESS;
                Customer.Phone = customerFormViewModel.PHONE;
                Customer.Phone2 = customerFormViewModel.PHONE2;
                Customer.STRN = customerFormViewModel.STN_NO;
                Customer.CNIC = customerFormViewModel.CNIC;
                Customer.UrduName = customerFormViewModel.NAME_URDU;
                Customer.Mobile = customerFormViewModel.MOBILE;
                Customer.EMAIL = customerFormViewModel.EMAIL;
                Customer.account_code = customerFormViewModel.ACCOUNT_CODE;
                Customer.BusinessName = customerFormViewModel.BUSINESS_NAME;
                Customer.BusinessUrl = customerFormViewModel.BUSINESS_URL;
                Customer.Latitude = customerFormViewModel.LAT;
                Customer.Longitude = customerFormViewModel.LON;
                Customer.Comments = customerFormViewModel.COMMENTS;

                if (customerFormViewModel.DISC_CATEGORY_ID != null && customerFormViewModel.DISC_CATEGORY_ID > 0)
                    Customer.DiscountCategory = Convert.ToString(customerFormViewModel.DISC_CATEGORY_ID);
              
                Customer.LOCATION = customerFormViewModel.LOCATION;
                Customer.CITY = customerFormViewModel.CITY_CODE;
                Customer.AREA = customerFormViewModel.AREA_CODE;
                Customer.Balance = customerFormViewModel.BALANCE ?? 0;
                Customer.OpeningBalance = customerFormViewModel.OPENING ?? 0;
                Customer.Terms = customerFormViewModel.PAY_TERMS;
                Customer.Contact_Person = customerFormViewModel.CONTACT_PERSON;
                if (!string.IsNullOrEmpty(customerFormViewModel.AREA_CODE))
                    Customer.AREA_NAME = customerFormViewModel.AREA.AREA_NAME;
                    
                string value = String.Empty;
                value = JsonConvert.SerializeObject(Customer, Formatting.Indented,
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
                return Content(ex.ToString());
            }
        }
        public JsonResult GetBalance(string id)
        {
            //var balance = db.SUPPLIERs.Where(x => x.SUPL_CODE == id).Select(x => x.BALANCE).SingleOrDefault();
            var supplier = db.SUPPLIERs.Include(x => x.AREA).Where(x => x.SUPL_CODE == id).SingleOrDefault();

            SupplierViewModel supl = new SupplierViewModel();
            if (supplier!= null)
            {
                supl.Balance = (supplier.BALANCE ?? 0) + (supplier.OPENING ?? 0);
                if (!string.IsNullOrEmpty(supplier.AREA_CODE))
                    supl.SupplierArea = supplier.AREA.AREA_NAME;
            }

            return Json(supl, JsonRequestBehavior.AllowGet);
        }
        public string GetCustomerCode()
        {
            string s = db.SUPPLIERs.Max(u => u.SUPL_CODE);

            if (string.IsNullOrEmpty(s))
            {
                return "000001";
            }
            string code = CommonFunctions.GenerateCode(s, 6);
            return code;
        }
        public IEnumerable<CustomerSupplierViewModel> GetCustomers()
        {
            return (db.SUPPLIERs.Include(x => x.CITY).Include(x => x.AREA).Where(u => u.PARTY_TYPE == Constants.CustomerSupplier.Customer)
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
                }).ToList());
        }
    }
}