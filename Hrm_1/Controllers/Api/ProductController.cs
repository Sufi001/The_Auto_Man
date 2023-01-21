using Inventory.ViewModels.Item;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class ProductController : ApiController
    {
        public IHttpActionResult GetProduct(string id, string UanNo, string Name)
        {
            var context = new DataContext();
            List<ItemViewModel> productViewModel = new List<ItemViewModel>();
            if (!string.IsNullOrEmpty(id))
            {
                var productList = context.PRODUCTS.Where(u => u.BARCODE == id).Select(product => new ItemViewModel
                {
                    Barcode = product.BARCODE,
                    PackQty = product.PACK_QTY ?? 0,
                    PackRetail = product.PACK_RATE ?? (product.UNIT_RETAIL ?? 0),
                    Ctnpcs = product.CTN_PCS,
                    Description = product.DESCRIPTION,
                    Uanno = product.UAN_NO,
                    Cost = product.UNIT_COST ?? 0,
                    Retail = product.UNIT_RETAIL ?? 0,
                    UnitSize = product.UNIT_SIZE ?? 0,
                    UrduName = product.URDU

                }).ToList();
                productViewModel = productList;
            }
            else if (!string.IsNullOrEmpty(UanNo))
            {
                var barCode = context.PRODUCT_UAN_LIST.Where(x => x.UAN_NO == UanNo).Select(x => x.BARCODE).SingleOrDefault();
                if (!string.IsNullOrEmpty(barCode))
                {
                    var productList = context.PRODUCTS.Where(u => u.BARCODE == barCode).Select(product => new ItemViewModel
                    {
                        Barcode = product.BARCODE,
                        PackQty = product.PACK_QTY ?? 0,
                        PackRetail = product.PACK_RATE ?? (product.UNIT_RETAIL ?? 0),
                        Ctnpcs = product.CTN_PCS,
                        Description = product.DESCRIPTION,
                        Uanno = product.UAN_NO,
                        Cost = product.UNIT_COST ?? 0,
                        Retail = product.UNIT_RETAIL ?? 0,
                        UnitSize = product.UNIT_SIZE ?? 0,
                        UrduName = product.URDU

                    }).ToList();
                    productViewModel = productList;
                }
            }
            else if (!string.IsNullOrEmpty(Name))
            {
                var productList = context.PRODUCTS.Where(u => u.DESCRIPTION.Contains(Name)).Select(product => new ItemViewModel
                {
                    Barcode = product.BARCODE,
                    PackQty = product.PACK_QTY ?? 0,
                    PackRetail = product.PACK_RATE ?? (product.UNIT_RETAIL ?? 0),
                    Ctnpcs = product.CTN_PCS,
                    Description = product.DESCRIPTION,
                    Uanno = product.UAN_NO,
                    Cost = product.UNIT_COST ?? 0,
                    Retail = product.UNIT_RETAIL ?? 0,
                    UnitSize = product.UNIT_SIZE ?? 0,
                    UrduName = product.URDU
                }).ToList();
                productViewModel = productList;
            }

            else
            {
                productViewModel = null;
            }

            var value = JsonConvert.SerializeObject(productViewModel, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value);
            return jsonResult;
        }
    }
}
