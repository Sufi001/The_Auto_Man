using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class SendMailController : Controller
    {
        // GET: SendMail
        public ActionResult Index()
        {
            return View();
        }

        public void Send()
        {
            try
            {
                //MailMessage message = new MailMessage();
                //SmtpClient smtp = new SmtpClient("webmail.softvalley.com.pk", 25);
                //message.From = new MailAddress("Sales@softvalley.com.pk", "Softvalley");
                //message.To.Add(new MailAddress(model.Email, model.Name));
                ////message.Bcc.Add(new MailAddress(Config.AdminEmail, model.Name));
                //message.Subject = "Order Mail";
                //message.IsBodyHtml = true; //to make message body as html  
                //message.Body = CreateHtml(model, saleNo);
                //message.BodyEncoding = Encoding.UTF8;
                //smtp.UseDefaultCredentials = false;
                //smtp.EnableSsl = false;
                //smtp.Credentials = new NetworkCredential(Config.AdminEmail, Config.AdminEmailPass);
                ////smtp.Credentials = new NetworkCredential("Sales@softvalley.com.pk", "Sales@#$123123786");
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}