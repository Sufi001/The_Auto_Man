using Inventory.Helper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Inventory.ViewModel;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [Authorize]
    public class StaffMembersController : Controller
    {
        readonly DataContext db;
        public StaffMembersController()
        {
            db = new DataContext();
        }
        [Permission("Sales Staff")]
        public ActionResult Index()
        {
            CustomerSupplierViewModel obj = new CustomerSupplierViewModel();
            obj.CustomerSupplierList = GetStaffList();
            obj.LOCATION = CommonFunctions.GetLocation();
            obj.LocationList = db.LOCATIONs.ToList();
            obj.CityList = db.CITies.ToList();
            obj.AreaList = db.AREAs.ToList();
            obj.StaffStatusRoles = db.STAFF_STATUS.ToList();
            return View(obj);
        }
        public ActionResult Save(CustomerSupplierViewModel VM)
        {
            try
            {
                VM.CustomerSupplierList = GetStaffList();
                VM.LOCATION = CommonFunctions.GetLocation();
                VM.LocationList = db.LOCATIONs.ToList();
                VM.CityList = db.CITies.ToList();
                VM.AreaList = db.AREAs.ToList();
                VM.StaffStatusRoles = db.STAFF_STATUS.ToList();

                if (!ModelState.IsValid)
                {
                    TempData["message"] = "fail";
                    return View("Index", VM);
                }

                if (!string.IsNullOrEmpty(VM.Mobile))
                {
                    bool IsCustomerExistWithThisMobile = false;
                    if (string.IsNullOrEmpty(VM.SUPL_CODE))
                        IsCustomerExistWithThisMobile = db.STAFF_MEMBER.Any(x => x.MOBILE == VM.Mobile);
                    else
                        IsCustomerExistWithThisMobile = db.STAFF_MEMBER.Any(x => x.MOBILE == VM.Mobile && x.SUPL_CODE != VM.SUPL_CODE);

                    if (IsCustomerExistWithThisMobile)
                    {
                        TempData["message"] = "Staff Member Already Exist With This Mobile Number";
                        return View("Customers", VM);
                    }
                }
             
                var staff = new STAFF_MEMBER();
                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                {
                    staff.SUPL_CODE = GetStaffCode();
                    staff.INSERTED_BY = CommonFunctions.GetUserName();
                    staff.DOC = CommonFunctions.GetDateTimeNow();
                }
                else
                {
                    staff = db.STAFF_MEMBER.SingleOrDefault(c => c.SUPL_CODE == VM.SUPL_CODE);
                }


                staff.UPDATED_BY = CommonFunctions.GetUserName();
                staff.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                staff.SUPL_NAME = VM.NAME;
                staff.party_type = "s";
                staff.ADDRESS = VM.ADDRESS;
                staff.PHONE = VM.Phone;
                staff.MOBILE = VM.Mobile;
                staff.EMAIL = VM.EMAIL;
                staff.account_code = VM.account_code;
                staff.LOCATION = VM.LOCATION;
                staff.Transfer_Status = "0";
                staff.opening = VM.OpeningBalance;
                staff.PAY_TERMS = VM.Terms;
                staff.CONTACT_PERSON = VM.Contact_Person;
                staff.PHONE2 = VM.Phone2;
                staff.STATUS = VM.StaffRole;

                if (string.IsNullOrEmpty(VM.SUPL_CODE))
                    db.STAFF_MEMBER.Add(staff);
                   
                db.SaveChanges();

                TempData["message"] = "success";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
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
            return View("Index", VM);
        }
        [HttpGet]
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            try
            {
                var customerFormViewModel = db.STAFF_MEMBER.SingleOrDefault(c => c.SUPL_CODE == id && c.party_type == "s");
                if (customerFormViewModel == null)
                    return HttpNotFound();
                var staff = new CustomerSupplierViewModel();
                staff.SUPL_CODE = customerFormViewModel.SUPL_CODE;
                staff.NAME = customerFormViewModel.SUPL_NAME;
                staff.ADDRESS = customerFormViewModel.ADDRESS;
                staff.Phone = customerFormViewModel.PHONE;
                staff.Phone2 = customerFormViewModel.PHONE2;
                staff.Mobile = customerFormViewModel.MOBILE;
                staff.EMAIL = customerFormViewModel.EMAIL;
                staff.account_code = customerFormViewModel.account_code;
                staff.LOCATION = customerFormViewModel.LOCATION;
                staff.Balance = customerFormViewModel.balance;
                staff.OpeningBalance = customerFormViewModel.opening;
                staff.Terms = customerFormViewModel.PAY_TERMS;
                staff.Contact_Person = customerFormViewModel.CONTACT_PERSON;
                staff.StaffRole = customerFormViewModel.STATUS;
                var value = JsonConvert.SerializeObject(staff, Formatting.Indented,
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
        public string GetStaffCode()
        {
            string s = db.STAFF_MEMBER.Max(u => u.SUPL_CODE);

            if (string.IsNullOrEmpty(s))
            {
                return "000001";
            }
            string code = CommonFunctions.GenerateCode(s, 6);
            return code;
        }
        private IEnumerable<CustomerSupplierViewModel> GetStaffList()
        {
            return db.STAFF_MEMBER.Select(u => new CustomerSupplierViewModel
            {
                SUPL_CODE = u.SUPL_CODE,
                SUPL_NAME = u.SUPL_NAME,
                ADDRESS = u.ADDRESS,
                Phone = u.MOBILE,
                Balance = ((u.opening ?? 0) + (u.balance ?? 0)),
            }).ToList();
        }
    }
}