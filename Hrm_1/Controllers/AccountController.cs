using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Security;
using Inventory.Models;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using Inventory.Helper;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        DataContext db;
        public AccountController()
        {
            db = new DataContext();
            ViewBag.Locations = db.LOCATIONs.ToList();
        }
        public ActionResult Domain()
        {
            return View();
        }
        public ActionResult Login()
        {
            string cookieName = FormsAuthentication.FormsCookieName; //Find cookie name
            HttpCookie authCookie = HttpContext.Request.Cookies[cookieName]; //Get the cookie by it's name
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value); //Decrypt it
                string UserName = ticket.Name;
                Session["username"] = UserName;
                if (UserName != null)
                    return RedirectToAction("Index", "DashBoard");

            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel viewmodel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Login"/*, obj*/);

                var userPermissions = "";

                //string _u = db.USERS.Where(u => u.USER_NAME == viewmodel.UserName && u.LOC_ID == viewmodel.Location).Select(u => u.USER_PASSWD).FirstOrDefault();
                var user = db.USERS.Include(x => x.USER_PERMISSION).Where(u => 
                u.USER_NAME == viewmodel.UserName
                && u.LOC_ID == viewmodel.Location
                && u.USER_TYPE != "W"
                && u.USER_PASSWD.Equals(viewmodel.Password)).SingleOrDefault();
                if (user == null && viewmodel.UserName == Constants.SystemAdminInfo.SystemAdministratorName && viewmodel.Password.Equals(Constants.SystemAdminInfo.SystemAdministratorPassword))
                {
                    user = new USER();
                    user.USER_NAME = "System Admin";
                    user.USER_ID = "0000000000";
                    var PermissionsList = db.PERMISSIONs.ToList();
                    PermissionsList.Add(new PERMISSION { PERMISSION_ID = PermissionsList.Count + 1, NAME = "ClientConfiguration" });

                    foreach (var permission in PermissionsList)
                        userPermissions += permission.NAME + ",";

                }
                if (user != null)
                {
                    

                    Session["username"] = user.USER_NAME;

                    HttpCookie myCookie = new HttpCookie("UserSettings");
                    myCookie.Value = user.USER_ID; //"nl";

                    HttpCookie Location = new HttpCookie("Location");
                    Location.Value = viewmodel.Location.ToString();

                    if (string.IsNullOrEmpty(userPermissions))
                    {
                        foreach (var permission in user.USER_PERMISSION)
                            userPermissions += permission.PERMISSION.NAME + ",";
                    }

                    if (viewmodel.remember)
                    {
                        myCookie.Expires = DateTime.Now.AddDays(30);
                        Location.Expires = DateTime.Now.AddDays(30);
                        FormsAuthenticationTicket PersistentAuthTicket = new FormsAuthenticationTicket(1, user.USER_NAME, DateTime.Now, DateTime.Now.AddDays(30), true, userPermissions);
                        string PersistentEncryptedAuthTicket = FormsAuthentication.Encrypt(PersistentAuthTicket);
                        HttpCookie PersistentCookie = new HttpCookie(FormsAuthentication.FormsCookieName, PersistentEncryptedAuthTicket);
                        PersistentCookie.Expires = PersistentAuthTicket.Expiration;
                        //Response.Cookies.Set(cookie);
                        Response.Cookies.Add(PersistentCookie);
                    }
                    else
                    {
                        myCookie.Expires = DateTime.Now.AddDays(30);
                        Location.Expires = DateTime.Now.AddDays(30);
                        FormsAuthenticationTicket NonPersistentAuthTicket = new FormsAuthenticationTicket(1, user.USER_NAME, DateTime.Now, DateTime.Now.AddMinutes(360), false, userPermissions);
                        string NonPersistentEncryptedAuthTicket = FormsAuthentication.Encrypt(NonPersistentAuthTicket);
                        HttpCookie NonPersistentCookie = new HttpCookie(FormsAuthentication.FormsCookieName, NonPersistentEncryptedAuthTicket);
                        NonPersistentCookie.Expires = NonPersistentAuthTicket.Expiration;
                        Response.Cookies.Add(NonPersistentCookie);
                    }
                    Response.Cookies.Add(myCookie);
                    Response.Cookies.Add(Location);

                    return RedirectToAction("Index", "DashBoard");
                }
                else
                {
                    ViewBag.Message = "Invalid credentials.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong.";
                return View("Login");
            }

        }
       
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Session.Remove("username");
            if (Response.Cookies["Location"] != null)
            {
                HttpCookie cooks = new HttpCookie("Location");
                cooks.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cooks);


            }
            if (Response.Cookies["UserSettings"] != null)
            {
                HttpCookie cooks = new HttpCookie("UserSettings");
                cooks.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cooks);


            }
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [Permission("User")]
        public ActionResult Index()
        {
            var UsersList = db.USERS.Where(x => x.USER_TYPE == "U").ToList();
            var VM = new NewUserViewModel();
            VM.AllPermissions = PermissionList();
            return View(VM);
        }
        [HttpPost]
        public JsonResult Save(NewUserViewModel user)
        {
            if (user == null)
            {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                USER newUser = new USER();

                bool isUserNameExist = db.USERS.Any(u => u.USER_NAME.Trim().ToUpper() == user.UserName.Trim().ToUpper() && u.USER_ID != user.UserCode);
                if (isUserNameExist == true)
                {
                    user.AllPermissions = PermissionList();
                    return Json("Already Exist", JsonRequestBehavior.AllowGet);
                }



                if (string.IsNullOrEmpty(user.UserCode))
                {
                    newUser.USER_ID = GetUserId();
                    newUser.INSERTED_BY = CommonFunctions.GetUserName();
                    newUser.DOC = CommonFunctions.GetDateTimeNow();
                    newUser.USER_TYPE = "U";
                }
                else
                {
                    newUser = db.USERS.SingleOrDefault(x => x.USER_ID == user.UserCode);
                   
                }

                newUser.USER_NAME = user.UserName.Trim().ToUpper();

                if (!user.Password.Equals(user.ConfirmPassword))
                    return Json("pass", JsonRequestBehavior.AllowGet);

                newUser.UPDATED_BY = CommonFunctions.GetUserName();
                newUser.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                newUser.USER_PASSWD = user.Password.Trim();
                newUser.LOC_ID = CommonFunctions.GetLocation();
                newUser.STATUS = "1";
                newUser.EMAIL = user.Email;

                if (string.IsNullOrEmpty(user.UserCode))
                {
                    db.USERS.Add(newUser);
                }
                db.SaveChanges();

                if (user.SelectedPermissions.Count >= 1)
                {
                    var uPermissions = db.USER_PERMISSION.Where(x => x.USER_ID == newUser.USER_ID).ToList();
                    if (uPermissions.Count >= 1)
                    {
                        db.USER_PERMISSION.RemoveRange(uPermissions);
                        db.SaveChanges();
                    }
                    foreach (var item in user.SelectedPermissions)
                    {
                        USER_PERMISSION UP = new USER_PERMISSION { PERMISSION_ID = Convert.ToInt32(item), USER_ID = newUser.USER_ID };
                        db.USER_PERMISSION.Add(UP);
                    }
                }

                db.SaveChanges();
                transaction.Commit();
                return Json("success");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                user.AllPermissions = PermissionList();
                return Json("Exception", JsonRequestBehavior.AllowGet);
            }
        }
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            var Found = db.USERS.Include(c => c.USER_PERMISSION).Where(c => c.USER_ID == id).SingleOrDefault();
            var user = new NewUserViewModel();
            user.AllPermissions = db.PERMISSIONs.ToList();
            if (Found == null)
            {
                ViewBag.message = "User Not Found";
                return View("Index", user);
            }

            List<string> per = new List<string>();
            foreach (var item in Found.USER_PERMISSION)
            {
                string selectedPermissions = item.PERMISSION_ID.ToString();
                per.Add(selectedPermissions);
            }
            user.UserId = Found.USER_ID;
            user.SelectedPermissions = per;
            user.UserCode = Found.USER_ID;
            user.UserName = Found.USER_NAME;
            user.Email = Found.EMAIL;
            user.Password = Found.USER_PASSWD;
            user.ConfirmPassword = Found.USER_PASSWD;
            ViewBag.Update = "Update";
            return View("index", user);
        }
        public ActionResult UserList()
        {
            var UserList = db.USERS.Where(x => x.USER_TYPE != "A" && x.USER_TYPE != "W").Select(x => new AllUsers { UserCode = x.USER_ID, UserName = x.USER_NAME, Email = x.EMAIL, Password = x.USER_PASSWD }).ToList();
            return View(UserList);
        }
        public ActionResult WebUserList()
        {
            var UserList = db.USERS.Where(x => x.USER_TYPE == "W").Select(x => new AllUsers { UserCode = x.USER_ID, UserName = x.USER_NAME, Email = x.EMAIL, Password = x.USER_PASSWD }).ToList();
            return View("UserList", UserList);
        }
        public ActionResult TillUserList()
        {
            var UserList = db.TILL_USERS.Select(x => new AllUsers { UserCode = x.USER_ID, UserName = x.USER_NAME, Type = "Till", Password = x.USER_PASSWD }).ToList();
            return View("UserList", UserList);
        }
        public ActionResult Delete(string id)
        {
            var Found = db.USERS.Where(c => c.USER_ID == id).SingleOrDefault();
            if (Found == null)
            {
                ViewBag.message = "User Not Found";
                return RedirectToAction("List");
            }
            try
            {
                var up = db.USER_PERMISSION.Where(X => X.USER_ID == id).ToList();
                if (up.Count > 0)
                {
                    db.USER_PERMISSION.RemoveRange(up);
                }
                db.USERS.Remove(Found);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("List");
        }
        public string GetUserId()
        {
            var LastId = db.USERS.Max(c => c.USER_ID);
            if (String.IsNullOrEmpty(LastId))
            {
                return "0000000001";
            }
            var userId = LastId.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return "0000000001";
            }
            return (Convert.ToInt32(userId) + 1).ToString().PadLeft(10, '0');
        }
        public List<PERMISSION> PermissionList()
        {
            var permissions = db.PERMISSIONs.ToList();
            List<PERMISSION> perm = new List<PERMISSION>();
            foreach (var item in permissions)
            {
                var per = new PERMISSION();
                per.PERMISSION_ID = item.PERMISSION_ID;
                per.NAME = item.NAME;
                perm.Add(per);
            }
            return perm;
        }
        public ActionResult ClientConfiguration()
        {
            var ViewModel = new ClientConfiguration();
            var ClientConfig = db.CLIENT_CONFIGURATION.FirstOrDefault();
            if (ClientConfig != null)
            {
                ViewModel.SendInvoiceDataToFBR = ClientConfig.CAN_SEND_DATA_TO_FBR;
                ViewModel.SendEmail = ClientConfig.CAN_SEND_EMAIL;
                ViewModel.SendSMS = ClientConfig.CAN_SEND_SMS;
                ViewModel.SendWhatsApp = ClientConfig.CAN_SEND_WHATSAPP;
                ViewModel.ClientId = ClientConfig.CLIENT_CONFIGURATION_ID;
            }

            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult ClientConfiguration(ClientConfiguration config)
        {
            if (!ModelState.IsValid)
                return Content("Model State Is Not Valid");

            try
            {
                CLIENT_CONFIGURATION CC = new CLIENT_CONFIGURATION();
                if (!string.IsNullOrEmpty(config.ClientId.ToString()) && config.ClientId > 0)
                {
                    var found = db.CLIENT_CONFIGURATION.Where(x => x.CLIENT_CONFIGURATION_ID == config.ClientId).SingleOrDefault();
                    found.CAN_SEND_DATA_TO_FBR = config.SendInvoiceDataToFBR;
                    found.CAN_SEND_SMS = config.SendSMS;
                    found.CAN_SEND_WHATSAPP = config.SendWhatsApp;
                    found.CAN_SEND_EMAIL = config.SendEmail;
                }
                else
                {
                    CC.CAN_SEND_DATA_TO_FBR = config.SendInvoiceDataToFBR;
                    CC.CAN_SEND_SMS = config.SendSMS;
                    CC.CAN_SEND_WHATSAPP = config.SendWhatsApp;
                    CC.CAN_SEND_EMAIL = config.SendEmail;
                    db.CLIENT_CONFIGURATION.Add(CC);
                }
                db.SaveChanges();
                return Content("Data saved successfully");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
       
    }
}