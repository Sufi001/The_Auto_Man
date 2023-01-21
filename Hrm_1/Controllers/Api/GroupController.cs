using Inventory.Helper;
using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class GroupController : ApiController
    {
        private DataContext db;
        public GroupController()
        {
            db = new DataContext();
        }
        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateGroup(GROUP group)//, string username
        {
            try
            {
                if (string.IsNullOrEmpty(group.DEPT_CODE) || string.IsNullOrEmpty(group.GROUP_NAME))
                    return BadRequest();
                var v = new GROUP();

                var firstgroups = db.GROUPS.Where(x => x.GROUP_NAME == group.GROUP_NAME && x.DEPT_CODE == group.DEPT_CODE).ToList();
                if (firstgroups.Count > 0)
                    return Ok("exist");

                string s = db.GROUPS.Where(x => x.DEPT_CODE == group.DEPT_CODE).Max(x => x.GROUP_CODE);
                if (string.IsNullOrEmpty(s))
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.GROUP_CODE = "01";
                }
                else
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.GROUP_CODE = CommonFunctions.GenerateCode(s, 2);
                }

                v.GROUP_NAME = group.GROUP_NAME;
                v.DEPT_CODE = group.DEPT_CODE;
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.TRANSFER_STATUS = "0";
                v.STATUS = "A";
                db.GROUPS.Add(v);
                db.SaveChanges();
                //var _group = db.GROUPS.Where(u => u.DEPT_CODE == group.DEPT_CODE).ToList().OrderBy(u => u.DOC).ToList();
                //return Ok(_group);
                return Ok(v.GROUP_CODE);

            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return Ok("ex");
            }
        }
    }
}
