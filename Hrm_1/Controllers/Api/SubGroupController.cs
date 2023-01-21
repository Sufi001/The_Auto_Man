using Inventory.Helper;
using System;
using System.Linq;
using System.Web.Http;

namespace Inventory.Controllers.Api
{
    public class SubGroupController : ApiController
    {
        private readonly DataContext db;
        public SubGroupController()
        {
            db = new DataContext();
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CreateSubGroup(SUB_GROUPS subGroups)
        {
            try
            {
                if (string.IsNullOrEmpty(subGroups.SGROUP_NAME)|| string.IsNullOrEmpty(subGroups.GROUP_CODE)|| string.IsNullOrEmpty(subGroups.DEPT_CODE))
                    return BadRequest();

                var v = new SUB_GROUPS();
                var firstgroups = db.SUB_GROUPS.Where(x => x.SGROUP_NAME == subGroups.SGROUP_NAME && x.DEPT_CODE == subGroups.DEPT_CODE && x.GROUP_CODE == subGroups.GROUP_CODE).ToList();
                if (firstgroups.Count > 0)
                    return Ok("exist");

                string s = db.SUB_GROUPS.Where(x=>x.GROUP_CODE==subGroups.GROUP_CODE&&x.DEPT_CODE==subGroups.DEPT_CODE).Max(u => u.SGROUP_CODE);
                if (string.IsNullOrEmpty(s))
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.SGROUP_CODE = "01";
                }
                else
                {
                    v.DOC = CommonFunctions.GetDateTimeNow();
                    v.INSERTED_BY = CommonFunctions.GetUserName();
                    v.SGROUP_CODE = CommonFunctions.GenerateCode(s, 2);
                }

                v.SGROUP_NAME = subGroups.SGROUP_NAME;
                v.DEPT_CODE = subGroups.DEPT_CODE;
                v.GROUP_CODE = subGroups.GROUP_CODE;
                v.STATUS = "A";
                v.TRANSFER_STATUS = "0";

                v.UPDATED_BY = CommonFunctions.GetUserName();
                v.UPDATE_DATE = CommonFunctions.GetDateTimeNow();

                db.SUB_GROUPS.Add(v);
                db.SaveChanges();
                //var _subGroups = db.SUB_GROUPS.Where(u => u.GROUP_CODE == subGroups.GROUP_CODE && u.DEPT_CODE == subGroups.DEPT_CODE).ToList().OrderBy(u => u.DOC).ToList();
                //return Ok(_subGroups);
                return Ok(v.SGROUP_CODE);
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return Ok("ex");
            }
        }
    }
}
