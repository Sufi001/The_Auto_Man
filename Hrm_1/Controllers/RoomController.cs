using Inventory.Helper;
using Inventory.ViewModels.Room;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Inventory.Filters;

namespace Inventory.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        DataContext db;
        public RoomController()
        {
            db = new DataContext();
        }
        [Permission("Room Management")]
        public ActionResult Index()
        {
            RoomViewModel VM = new RoomViewModel();
            VM.RoomCategories = db.ROOM_CATEGORY.ToList();
            VM.RoomTypes = db.ROOM_TYPE.ToList();
            VM.RoomStatus = RoomStatus();
            VM.STATUS = "V";
            VM.roomsList = db.ROOMs.Include(x => x.ROOM_TYPE).Include(x => x.ROOM_CATEGORY).ToList();
            return View(VM);
        }
        public ActionResult Save(RoomViewModel VM)
        {
            if (VM != null)
            {
                try
                {
                    //if (db.ROOMs.Any(x => x.ROOM_NAME == VM.ROOM_NAME))
                    //{
                    //    return Content("Exist");
                    //}
                    ROOM room = new ROOM();
                    if (string.IsNullOrEmpty(VM.ROOM_CODE))
                    {
                        var code = db.ROOMs.Max(x => x.ROOM_CODE);
                        if (!string.IsNullOrEmpty(code))
                            code = CommonFunctions.GenerateCode(code, 4);
                        else
                            code = "0001";

                        room.ROOM_CODE = code;
                        room.DOC = CommonFunctions.GetDateTimeNow();
                        room.INSERTED_BY = CommonFunctions.GetUserName();
                    }
                    else
                    {
                        var roomFound = db.ROOMs.Where(x => x.ROOM_CODE == VM.ROOM_CODE).SingleOrDefault();

                        if (roomFound != null)
                            room = roomFound;
                        else
                            return Content("Wrong");


                    }

                    room.UPDATED_BY = CommonFunctions.GetUserName();
                    room.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    room.CATEGORY = VM.CATEGORY;
                    room.ROOM_NAME = VM.ROOM_NAME;
                    room.STATUS = VM.STATUS;
                    room.RATE = VM.RATE.HasValue ? VM.RATE.Value : 0;
                    room.TYPE = VM.TYPE;
                    if (string.IsNullOrEmpty(VM.ROOM_CODE))
                        db.ROOMs.Add(room);

                    db.SaveChanges();
                    return Content("OK");
                }
                catch (Exception ex)
                {
                    return Content("EX");
                }
            }
            else
                return Content("Wrong");

        }
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {

                try
                {
                    var isRoomExist = db.ROOMs.Where(x => x.ROOM_CODE == id).SingleOrDefault();
                    if (isRoomExist != null)
                    {
                        RoomViewModel VM = new RoomViewModel();

                        VM.RoomCategories = db.ROOM_CATEGORY.ToList();
                        VM.RoomTypes = db.ROOM_TYPE.ToList();
                        VM.RoomStatus = RoomStatus();
                        VM.roomsList = db.ROOMs.ToList();

                        VM.CATEGORY = isRoomExist.CATEGORY;
                        VM.ROOM_CODE = isRoomExist.ROOM_CODE;
                        VM.ROOM_NAME = isRoomExist.ROOM_NAME;
                        VM.STATUS = isRoomExist.STATUS;
                        VM.TYPE = isRoomExist.TYPE;

                        return View("Index",VM);
                    }
                    else
                    {
                        return Content("Invalid");
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            else
            {
                return Content("Invalid");
            }
        }
        public JsonResult GetRooms(string category, string type)
        {
            var roomList = db.ROOMs.Where(x => x.CATEGORY == category & x.TYPE == type).Select(x=> new {code = x.ROOM_CODE,name =x.ROOM_NAME}).ToList();

            var value = JsonConvert.SerializeObject(roomList, Formatting.Indented,new JsonSerializerSettings
               {
                   ReferenceLoopHandling = ReferenceLoopHandling.Ignore
               });

            return Json(value, JsonRequestBehavior.AllowGet);
        }
        [Permission("Type & Category")]
        public ActionResult Type_Categories()
        {
            ViewBag.Categories = db.ROOM_CATEGORY.ToList();
            ViewBag.Types = db.ROOM_TYPE.ToList();
            return View();
        }
        public ActionResult SaveCategory(ROOM_CATEGORY category)
        {
            try
            {
                if (category != null && !string.IsNullOrEmpty(category.NAME))
                {
                    var Exist = db.ROOM_CATEGORY.Any(x => x.NAME == category.NAME);
                    if (Exist)
                        return Content("Exist");

                    ROOM_CATEGORY cat = new ROOM_CATEGORY();
                    string code = string.Empty;
                    if (!string.IsNullOrEmpty(category.ROOM_CATEGORY_CODE))
                    {
                        var isCategoryExist = db.ROOM_CATEGORY.Where(x => x.ROOM_CATEGORY_CODE == category.ROOM_CATEGORY_CODE).SingleOrDefault();
                        if (isCategoryExist != null)
                            cat = isCategoryExist;
                    }
                    else
                    {
                        code = db.ROOM_CATEGORY.Max(x => x.ROOM_CATEGORY_CODE);
                        if (!string.IsNullOrEmpty(code))
                            code = CommonFunctions.GenerateCode(code, 2);
                        else
                            code = "01";

                        cat.ROOM_CATEGORY_CODE = code;
                    }
                    cat.NAME = category.NAME;

                    if (string.IsNullOrEmpty(category.ROOM_CATEGORY_CODE))
                        db.ROOM_CATEGORY.Add(cat);

                    db.SaveChanges();
                    return Content("OK");
                }
                else
                    return Content("Invalid");
            }
            catch (Exception)
            {
                return Content("EX");
            }
        }
        public ActionResult SaveType(ROOM_TYPE type)
        {
            try
            {
                if (type != null && !string.IsNullOrEmpty(type.NAME))
                {
                    var Exist = db.ROOM_TYPE.Any(x => x.NAME == type.NAME);
                    if (Exist)
                        return Content("Exist");

                    ROOM_TYPE rType = new ROOM_TYPE();
                    string code = string.Empty;
                    if (!string.IsNullOrEmpty(type.ROOM_TYPE_CODE))
                    {
                        var isTypeExist = db.ROOM_TYPE.Where(x => x.ROOM_TYPE_CODE == type.ROOM_TYPE_CODE).SingleOrDefault();
                        if (isTypeExist != null)
                            rType = isTypeExist;
                    }
                    else
                    {
                        code = db.ROOM_TYPE.Max(x => x.ROOM_TYPE_CODE);
                        if (!string.IsNullOrEmpty(code))
                            code = CommonFunctions.GenerateCode(code, 2);
                        else
                            code = "01";

                        rType.ROOM_TYPE_CODE = code;
                    }
                    rType.NAME = type.NAME;
                    if (string.IsNullOrEmpty(type.ROOM_TYPE_CODE))
                        db.ROOM_TYPE.Add(rType);

                    db.SaveChanges();
                    return Content("OK");
                }
                else
                    return Content("Invalid");
            }
            catch (Exception)
            {
                return Content("EX");
            }
        }
        public static IEnumerable<SelectListItem> RoomStatus()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "Occupied", Value = "O"},
                new SelectListItem{Text = "Vacant", Value = "V"},
                new SelectListItem{Text = "UnderService", Value = "S"},
            };
            return items;
        }
    }
}