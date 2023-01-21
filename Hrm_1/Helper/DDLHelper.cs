using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Inventory.Helper
{
    public class DDLHelper : Controller
    {
        private readonly DataContext context;
        public DDLHelper()
        {
            context = new DataContext();
        }
        public JsonResult FillGroup(string id)
        {
            var group = (from a in context.GROUPS where a.DEPT_CODE == id select new { a.GROUP_CODE, a.GROUP_NAME }).ToList();
            int count = group.Count();
            var value = JsonConvert.SerializeObject(@group, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        
    }
}