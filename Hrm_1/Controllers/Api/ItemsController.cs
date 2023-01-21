using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class ItemsController : ApiController
    {
        [HttpGet]
        //public IEnumerable<string> GetEmployees(string query = null)
        public IHttpActionResult GetSuppliers(string query = "")
        {
            DataContext context = new DataContext();
            string[] arr = query.Split(',');
            string q = arr[0];
            string barcode = "";
            if (arr.Length == 2)
            {
                barcode = arr[1];
            }
            if (barcode == "")
            {
                var moviesQuery = context.SUPPLIERs.Where(c => c.SUPL_NAME.Contains(q) && c.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
                return Ok(moviesQuery);

            }

            var suppliersList = (from a in context.SUPPLIERs
                                 let sp = (from x in context.SUPPLIER_PRODUCTS where x.BARCODE == barcode select x.SUPL_CODE)
                                 where (a.SUPL_NAME.Contains(q) && a.PARTY_TYPE == "s" &&
                                 !sp.Contains(a.SUPL_CODE))
                                 select a).ToList();
            return Ok(suppliersList);
        }


        //[System.Web.Http.HttpPost]
        //public IHttpActionResult CreateRentals(ItemFormViewModel newRental)
        //{
        //    try
        //    {
        //        DataContext context = new DataContext();

        //        var customer = context.PRODUCTS.Single(
        //        c => c.BARCODE == newRental.BARCODE);


        //        var movies = context.SUPPLIERs.Where(
        //        m => newRental._Suppliersid.Contains(m.SUPL_CODE)).ToList();
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.ToString();

        //        return null;
        //    }
        //}

    }
}
