//using Newtonsoft.Json;
//using System;
//using System.Linq;
//using System.Web.Http;

//namespace Inventory.Controllers.Api
//{
//    public class WarehouseController : ApiController
//    {
//        private readonly DataContext db;
//        public WarehouseController()
//        {
//            db = new DataContext();
//        }

//        [System.Web.Http.HttpPost]
//        public IHttpActionResult CreateWarehouse(Warehouse warehouse)//, string txt_loc
//        {
//            try
//            {
//                var v = new Warehouse();
//                CommonFunctions obj1 = new CommonFunctions();
//                if (string.IsNullOrEmpty(warehouse.Name) || string.IsNullOrEmpty(warehouse.loc_id))
//                {
//                    return BadRequest("Warehouse name can not be empty.");
//                }
//                string s = _context.Warehouses.Max(u => u.id);
//                if (string.IsNullOrEmpty(s))
//                {
//                    s = "01";
//                    v.id = s;
//                }
//                else
//                {
//                    v.id = obj1.GenerateCode(s, 2);
//                }
//                v.Name = warehouse.Name;
//                v.loc_id = warehouse.loc_id;
//                db.Warehouses.Add(v);
//                db.SaveChanges();
//                var warehouseList = db.Warehouses.Where(u => u.loc_id == warehouse.loc_id).Select(x => new { x.id, x.Name }).ToList().OrderBy(u => u.id);
//                return Ok(warehouseList);
//            }
//            catch (Exception ex)
//            {
//                string s = ex.ToString();
//                return null;
//            }
//        }

//        [System.Web.Http.HttpGet]
//        public IHttpActionResult FillWarehouse(string id)
//        {
//            var warehouseList = (from a in db.Warehouses where a.loc_id == id select new { a.id, a.Name }).ToList();//_context.DESIGNATIONs.Where(c => c.DEPT_CODE == id).ToList();

//            var value = JsonConvert.SerializeObject(warehouseList, Formatting.Indented,
//                new JsonSerializerSettings
//                {
//                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//                });
//            return Json(value);

//        }
//    }
//}
