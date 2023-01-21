using Inventory.Helper;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace Inventory.Controllers.Api
{
    public class DepartmentController : ApiController
    {

        private readonly DataContext db;
        public DepartmentController()
        {
            db = new DataContext();
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateDepartment(DEPARTMENT dep)//, string username
        {
            try
            {
                var v = new DEPARTMENT();
                var firstgroups = db.DEPARTMENTs.Where(x => x.DEPT_NAME == dep.DEPT_NAME).ToList();
                if (firstgroups.Count > 0)
                    return Ok("exist");

                string s = db.DEPARTMENTs.Max(u => u.DEPT_CODE);
                if (string.IsNullOrEmpty(s))
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.DEPT_CODE = "01";
                }
                else
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.DEPT_CODE = CommonFunctions.GenerateCode(s, 2);
                }

                v.DEPT_NAME = dep.DEPT_NAME;
                v.TRANSFER_STATUS = "0";
                v.STATUS = "A";
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                db.DEPARTMENTs.Add(v);
                db.SaveChanges();
                //var _dep = db.DEPARTMENTs.ToList().OrderBy(u => u.DOC).ToList();
                //return Ok(_dep);
                return Ok(v.DEPT_CODE);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return Ok("ex");
            }
        }
    }
}
