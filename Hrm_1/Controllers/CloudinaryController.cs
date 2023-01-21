using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Inventory.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class CloudinaryController : Controller
    {
        // GET: Cloudinary
        public ActionResult Index()
        {
            return View();
        }


        public string SaveinCloudinary(string filepath, string barCode)
        {
            try
            {
                string CloudName = Config.CloudName;
                string CloudAPI = Config.CloudAPI;
                string CloudAPISecret = Config.CloudAPISecret;
                string CloudURL = Config.CloudURL;


                Account account = new Account(CloudName,CloudAPI,CloudAPISecret);

                Cloudinary cloudinary = new Cloudinary(account);
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(filepath),
                    PublicId = "my_folder/VapeNew/"+ barCode,
                    Overwrite = true,
                    //NotificationUrl = "https://mysite.example.com/my_notification_endpoint"
                };
                var uploadResult = cloudinary.Upload(uploadParams);

                //string chk = cloudinary.Api.UrlImgUp.BuildUrl("my_folder/VapeNew/" + barCode+".jpg");

                return "OK";
            }
            catch(Exception ex)
            {
                return ex.Message.ToString();
            }
        }
             
    }
}