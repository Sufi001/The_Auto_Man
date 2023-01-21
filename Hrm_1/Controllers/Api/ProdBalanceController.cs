using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class ProdBalanceController : ApiController
    {
        public IHttpActionResult GetProductBalance(string id)
        {
            string[] arr = id.Split(',');
            string barcode = arr[0];
            int colour = Convert.ToInt32(arr[1]);

            var context = new DataContext();
            var prodBalance = context.PROD_BALANCE.SingleOrDefault(u => u.BARCODE == barcode);
            if (prodBalance == null)
            {
                return Json(0);
            }
            decimal qtyinHand = prodBalance.CURRENT_QTY ?? 0;

            var value = JsonConvert.SerializeObject(qtyinHand, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value);
            return jsonResult;
        }

    }
}
