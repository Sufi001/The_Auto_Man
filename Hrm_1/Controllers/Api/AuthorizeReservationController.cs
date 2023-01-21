//using CustomerRec;
//using Inventory.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Http;
//using System.Data.Entity;

//namespace Inventory.Controllers.Api
//{
//    public class AuthorizeReservationController : ApiController
//    {
//        readonly DataContext db;
//        public AuthorizeReservationController()
//        {
//            db = new DataContext();
//        }
//        [System.Web.Mvc.HttpPost]
//        public IHttpActionResult Authorize(ReservationViewModel vm)
//        {

            
//                if (vm == null || vm.main == null || vm.detailList == null || string.IsNullOrEmpty(vm.main.RES_ID.ToString()) || vm.main.RES_ID == 0 || vm.main.RES_ID == null)
//                    return BadRequest();
//                ReservationController resSave = new ReservationController();
//                resSave.Save(vm);
//            using (DbContextTransaction transaction = db.Database.BeginTransaction())
//            {

//            try
//            {
//                var main = db.RESERVATION_MAIN.Where(x => x.RES_ID == vm.main.RES_ID).SingleOrDefault();
//                if (main != null)
//                {
//                    main.STATUS = "3";
//                    db.SaveChanges();
//                }

//                foreach (var item in vm.detailList)
//                {
//                    var room = db.ROOMs.Where(x => x.ROOM_CODE == item.ROOM_CODE).SingleOrDefault();
//                    if (room != null)
//                    {
//                        room.STATUS = "V";
//                        db.SaveChanges();
//                    }
//                }
//                transaction.Commit();
//                return Ok("3");
//            }
//            catch (System.Exception ex)
//            {
//                transaction.Rollback();
//                string s = ex.ToString();
//                return BadRequest(ex.ToString());
//            }
//            }
//        }
//    }
//}
