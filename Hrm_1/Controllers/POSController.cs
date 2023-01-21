using Inventory.Encryption;
using Inventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    public class POSController : Controller
    {
        DataContext context;
        public POSController()
        {
            context = new DataContext();
        }
        public ActionResult Registration()
        {
            ViewBag.Cities = context.CITies.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Save(POSRegistrationViewModel Data)
        {
            ViewBag.Cities = context.CITies.ToList();
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Model State Is Not Valid";
                return View("Registration", Data);
            }
            FBR_POS_ID POS;
            var POSExist = context.FBR_POS_ID.Where(x => x.POS_ID == Data.POSID).SingleOrDefault();
            if (POSExist != null)
                POS = POSExist;
            else
            {
                POS = new FBR_POS_ID();
                POS.POS_ID = Data.POSID;
            }
            POS.NTN = Data.NTN;
            POS.BUSINESS_NAME = Data.BusinessName;
            POS.BRANCH_NAME = Data.BranchName;
            POS.BUSINESS_ADDRESS = Data.BusinessAddress;
            POS.CITY = Data.City;
            POS.IP_ADDRESS = Data.IPAddress;
            POS.LATITUDE = Data.Latitude;
            POS.LONGITUDE = Data.Longitude;
            POS.MODE = Data.Mode;
            POS.USER_ID = Data.UserId;
            var UserPass = RijndaelManagedEncryption.EncryptRijndael(Data.Password);
            POS.PASSWORD = UserPass;
            POS.TILL_NO = "01";

            if (POSExist == null)
            { 
                context.FBR_POS_ID.Add(POS);
                ViewBag.Message = "POS Successfully Registered.";
            }
            else
            {
                ViewBag.Message = "POS Info Successfully Updated.";
            }
            context.SaveChanges();
            
            return View("Registration");
        }
        public ActionResult Edit(string id)
        {
            ViewBag.Cities = context.CITies.ToList();
            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Message = "Invalid Id";
                return View("List");
            }

            var POSExist = context.FBR_POS_ID.Where(x => x.POS_ID == id).SingleOrDefault();
            if (POSExist != null)
            {
                POSRegistrationViewModel POS = new POSRegistrationViewModel();
                POS.POSID = POSExist.POS_ID;
                POS.NTN = POSExist.NTN;
                POS.BusinessName = POSExist.BUSINESS_NAME;
                POS.BranchName = POSExist.BRANCH_NAME;
                POS.BusinessAddress = POSExist.BUSINESS_ADDRESS;
                POS.City = POSExist.CITY;
                POS.IPAddress = POSExist.IP_ADDRESS;
                POS.Latitude = POSExist.LATITUDE;
                POS.Longitude = POSExist.LONGITUDE;
                POS.Mode = POSExist.MODE;
                POS.UserId = POSExist.USER_ID;
                var UserPass = RijndaelManagedEncryption.DecryptRijndael(POSExist.PASSWORD);
                POS.Password = UserPass;
                POS.TillNo = POSExist.TILL_NO;
                return View("Registration", POS);
            }
            else
            {
                ViewBag.Message = "Object Not Found";
                return View("List");
            }

        }
        public ActionResult List()
        {
            var POSes = context.FBR_POS_ID.ToList();
            return View(POSes);
        }
    }
}