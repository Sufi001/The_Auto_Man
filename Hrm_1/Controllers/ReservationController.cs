using Inventory.Filters;
using Inventory.Helper;
using Inventory.Models;
using Inventory.ViewModels;
using Inventory.ViewModels.Reservation;
using Inventory.ViewModels.Room;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Inventory.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        //Reservation Status => 0 => Reserved
        //Reservation Status => 1 => Cancel
        //Reservation Status => 3 => Authorized.
        //Room Status will Change on Save New detail Record, delete detail record and document authorization

        private readonly DataContext db;
        public ReservationController()
        {
            db = new DataContext();
        }
        // GET: Reservation
        [Permission("Reservation")]
        public ActionResult Index()
        {
            ReservationMain main = new ReservationMain();
            main.DOC_DATE = DateTime.Now;
            main.Cities = db.CITies.ToList();
            main.Countries = db.COUNTRies.ToList();
            main.IdTypes = IdTypes();

            ReservationDetail detail = new ReservationDetail();
            detail.RoomCategoryList = db.ROOM_CATEGORY.ToList();
            detail.RoomTypeList = db.ROOM_TYPE.ToList();
            detail.RoomList = db.ROOMs.ToList();

            ReservationViewModel Vm = new ReservationViewModel();
            Vm.main = main;
            Vm.detail = detail;
            Vm.itemList = db.PRODUCTS.Where(x => x.TYPE == "P").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
            Vm.amenityList = db.PRODUCTS.Where(x => x.TYPE == "S").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
            Vm.detailList = new List<ReservationDetail>();
            Vm.reservationItems = new List<ReservationItems>();
            Vm.reservationAmenities = new List<ReservationItems>();

            ReportsController obj = new ReportsController();

            return View(Vm);
        }
        public ActionResult Save(ReservationViewModel Data)
        {

            RESERVATION_MAIN maintable = new RESERVATION_MAIN();
            SUPPLIER customer = new SUPPLIER();
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Data.main.PHONE = Trim(Data.main.PHONE);
                    Data.main.ID_NO = Trim(Data.main.ID_NO);

                    var isCustomerExist = db.SUPPLIERs.Where(x => x.PHONE == Data.main.PHONE || x.PHONE2 == Data.main.PHONE || x.MOBILE == Data.main.PHONE || x.SUPL_CODE == Data.main.SUPL_CODE).FirstOrDefault();
                    if (isCustomerExist == null)
                    {
                        customer.SUPL_CODE = CustomerCode(6);
                        customer.SUPL_NAME = Trim(Data.main.FIRST_NAME) + " " + Trim(Data.main.LAST_NAME);
                        customer.ADDRESS = string.IsNullOrEmpty(Data.main.ADDRESS) ? "Address Not Specified" : Trim(Data.main.ADDRESS);
                        customer.PARTY_TYPE = Constants.CustomerSupplier.Customer;
                        customer.DOC = DateTime.Now;
                        customer.INSERTED_BY = CommonFunctions.GetUserName();
                        customer.PHONE = Trim(Data.main.PHONE);
                        customer.PHONE2 = Trim(Data.main.PHONE);
                        customer.MOBILE = Trim(Data.main.PHONE);
                        db.SUPPLIERs.Add(customer);
                        db.SaveChanges();
                        Data.main.SUPL_CODE = customer.SUPL_CODE;
                    }
                    else
                        Data.main.SUPL_CODE = isCustomerExist.SUPL_CODE;


                    var maxId = db.RESERVATION_MAIN.Max(x => (long?)x.RES_ID);
                    if (maxId != null)
                        maxId = maxId + 1;
                    else
                        maxId = 1;

                    if (Data.main.RES_ID == 0 || Data.main.RES_ID == null)
                    {
                        maintable.RES_ID = maxId.Value;
                        maintable.DOC_DATE = Data.main.DOC_DATE;
                        maintable.INSERTED_BY = CommonFunctions.GetUserName();
                    }
                    else
                    {
                        maintable = db.RESERVATION_MAIN.Where(x => x.RES_ID == Data.main.RES_ID && x.STATUS != "3").SingleOrDefault();
                        if (maintable == null)
                            return Content("Wrong");
                        else
                        {
                            maintable.UPDATED_BY = CommonFunctions.GetUserName();
                            maintable.UPDATED_DATE = DateTime.Now;
                        }
                    }
                    maintable.ADDRESS = Trim(Data.main.ADDRESS);
                    maintable.AMOUNT_PAID = Data.main.AMOUNT_PAID;
                    maintable.BALANCE = Data.main.BALANCE;
                    maintable.CITY = Trim(Data.main.CITY);
                    maintable.COMPANY = Trim(Data.main.COMPANY);
                    maintable.COUNTRY = Trim(Data.main.COUNTRY);
                    maintable.DISCOUNT = Data.main.DISCOUNT;
                    maintable.DOB = Data.main.DOB;
                    maintable.E_MAIL = Trim(Data.main.E_MAIL);
                    maintable.FIRST_NAME = Trim(Data.main.FIRST_NAME);
                    maintable.ID_NO = Trim(Data.main.ID_NO);
                    maintable.ID_TYPE = Trim(Data.main.ID_TYPE);
                    maintable.STATUS = "0";
                    maintable.LAST_NAME = Trim(Data.main.LAST_NAME);
                    maintable.PHONE = (Data.main.PHONE);
                    maintable.SUPL_CODE = Trim(Data.main.SUPL_CODE);
                    maintable.TOTAL_AMT = Data.main.TOTAL_AMT;
                    if (Data.main.RES_ID <= 0 && Data.main.RES_ID != null)
                        db.RESERVATION_MAIN.Add(maintable);

                    if (Data.main.RES_ID > 0 && Data.main.RES_ID != null)
                    {
                        var List = db.RESERVATION_DETAIL.Where(x => x.RES_ID == Data.main.RES_ID && x.STATUS != Constants.DocumentStatus.AuthorizedDocument).ToList();

                        //foreach (var roomCode in List)
                        //{
                        //    var room = db.ROOMs.Where(x => x.ROOM_CODE == roomCode.ROOM_CODE).SingleOrDefault();
                        //    room.STATUS = Constants.RoomStatus.Vacant;
                        //}
                        //db.SaveChanges();

                        if (List.Count > 0)
                            db.RESERVATION_DETAIL.RemoveRange(List);
                        db.SaveChanges();

                    }
                    if (Data.detailList != null)
                    {
                        foreach (var item in Data.detailList)
                        {
                            if (item.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            {
                                RESERVATION_DETAIL detailtable = new RESERVATION_DETAIL();
                                detailtable.ADULTS = item.ADULTS;
                                detailtable.CHECKIN_DATETIME = item.CHECKIN_DATETIME.Value;
                                detailtable.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME.Value;
                                detailtable.CHILDREN = item.CHILDREN;
                                detailtable.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                                detailtable.GST = item.GST;
                                detailtable.OTHER_CHARGES = item.OTHER_CHARGES;
                                detailtable.RATE_PER_DAY = item.RATE_PER_DAY;
                                detailtable.RES_ID = maintable.RES_ID;
                                detailtable.RES_STS = Trim(item.RES_STS);
                                detailtable.REVS_DAYS = item.REVS_DAYS;
                                detailtable.STATUS = "0";
                                detailtable.ROOM_CODE = Trim(item.ROOM_CODE);
                                detailtable.SERVICES_CHARGES = item.SERVICES_CHARGES;
                                detailtable.TABLES_CODE = Trim(item.TABLES_CODE);
                                detailtable.TOTAL_AMT = item.TOTAL_AMT;
                                db.RESERVATION_DETAIL.Add(detailtable);
                            }
                        }
                        db.SaveChanges();
                    }

                    if (Data.main.RES_ID > 0 && Data.main.RES_ID != null)
                    {
                        var ProList = db.AMENITY_PRODUCT_DETAIL.Where(x => x.RES_ID == maintable.RES_ID && x.TYPE == "P" && x.STATUS != Constants.DocumentStatus.AuthorizedDocument).ToList();
                        if (ProList.Count > 0 && ProList != null)
                            db.AMENITY_PRODUCT_DETAIL.RemoveRange(ProList);
                    }

                    if (Data.reservationItems != null && Data.reservationItems.Count > 0)
                    {
                        foreach (var item in Data.reservationItems)
                        {
                            if (item.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            {
                                AMENITY_PRODUCT_DETAIL pro = new AMENITY_PRODUCT_DETAIL();
                                pro.AMOUNT = item.AMOUNT;
                                pro.BARCODE = item.BARCODE;
                                pro.TYPE = "P";
                                pro.DATETIME = item.DATETIME;
                                pro.COMMENTS = Trim(item.COMMENTS);
                                pro.QUANTITY = item.QUANTITY;
                                pro.RES_ID = maintable.RES_ID;
                                pro.ROOM_CODE = item.ROOM_CODE;
                                pro.STATUS = "0";
                                db.AMENITY_PRODUCT_DETAIL.Add(pro);
                            }
                        }
                        db.SaveChanges();
                    }

                    if (Data.main.RES_ID > 0 && Data.main.RES_ID != null)
                    {
                        var AmnList = db.AMENITY_PRODUCT_DETAIL.Where(x => x.RES_ID == maintable.RES_ID && x.TYPE == "S" && x.STATUS != Constants.DocumentStatus.AuthorizedDocument).ToList();
                        if (AmnList.Count > 0 && AmnList != null)
                            db.AMENITY_PRODUCT_DETAIL.RemoveRange(AmnList);
                    }

                    if (Data.reservationAmenities != null && Data.reservationAmenities.Count > 0)
                    {
                        foreach (var item in Data.reservationAmenities)
                        {
                            if (item.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            {
                                AMENITY_PRODUCT_DETAIL amenity = new AMENITY_PRODUCT_DETAIL();
                                amenity.AMOUNT = item.AMOUNT;
                                amenity.BARCODE = item.BARCODE;
                                amenity.TYPE = "S";
                                amenity.DATETIME = item.DATETIME;
                                amenity.COMMENTS = Trim(item.COMMENTS);
                                amenity.QUANTITY = item.QUANTITY;
                                amenity.RES_ID = maintable.RES_ID;
                                amenity.ROOM_CODE = item.ROOM_CODE;
                                amenity.STATUS = "0";
                                db.AMENITY_PRODUCT_DETAIL.Add(amenity);
                            }
                        }
                        db.SaveChanges();
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    return Content(maintable.RES_ID.ToString());
                }
                catch (DbEntityValidationException e)
                {
                    var returnStr = string.Empty;
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            returnStr += "Field: " + ve.PropertyName + "Error:" + ve.ErrorMessage;
                        }
                    }
                    transaction.Rollback();
                    return Content("ex1" + e);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Content("ex2" + ex);
                }
            }
        }
        public ActionResult List()
        {
            var reservation = db.RESERVATION_MAIN.Include(x => x.CITY1).ToList();
            List<ReservationMain> list = new List<ReservationMain>();
            foreach (var item in reservation)
            {
                ReservationMain obj = new ReservationMain();
                obj.RES_ID = item.RES_ID;
                obj.FIRST_NAME = item.FIRST_NAME + item.LAST_NAME;
                obj.DOC_DATE = item.DOC_DATE;
                obj.CITY = item.CITY1.CITY_NAME;
                obj.TOTAL_AMT = item.TOTAL_AMT;
                obj.STATUS = item.STATUS == "3" ? "Authorized" : (item.STATUS == "1" ? "Cancel" : "Unauthorize");
                list.Add(obj);
            }
            return View(list);
        }
        [Permission("Edit Document")]
        public ActionResult Edit(int id)
        {
            ViewBag.Update = true;

            ReservationViewModel vm = new ReservationViewModel();
            var reservation = db.RESERVATION_MAIN
                .Include(X => X.AMENITY_PRODUCT_DETAIL)
                .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.PRODUCT))
                .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.ROOM))

                .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY)
                .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY.Select(x => x.PRODUCT))
                .Include(X => X.AMENITY_PRODUCT_DETAIL_HISTORY.Select(x => x.ROOM))

                .Include(x => x.RESERVATION_DETAIL)
                .Include(x => x.RESERVATION_DETAIL.Select(y => y.ROOM))

                .Include(x => x.RESERVATION_DETAIL_HISTORY)
                .Include(x => x.RESERVATION_DETAIL_HISTORY.Select(y => y.ROOM))
                .Where(x => x.RES_ID == id).SingleOrDefault();
            if (reservation != null)
            {
                ReservationMain main = new ReservationMain();
                List<ReservationDetail> detaillist = new List<ReservationDetail>();
                List<ReservationItems> productList = new List<ReservationItems>();
                List<ReservationItems> amenityList = new List<ReservationItems>();
                main.ADDRESS = reservation.ADDRESS;
                main.AMOUNT_PAID = reservation.AMOUNT_PAID;
                main.BALANCE = reservation.BALANCE;
                main.Cities = db.CITies.ToList();
                main.CITY = reservation.CITY;
                main.COMPANY = reservation.COMPANY;
                main.Countries = db.COUNTRies.ToList();
                main.COUNTRY = reservation.COUNTRY;
                main.DISCOUNT = reservation.DISCOUNT;
                main.DOB = reservation.DOB;
                main.DOC_DATE = reservation.DOC_DATE;
                main.E_MAIL = reservation.E_MAIL;
                main.FIRST_NAME = reservation.FIRST_NAME;
                main.IdTypes = IdTypes();
                main.ID_NO = reservation.ID_NO;
                main.LAST_NAME = reservation.LAST_NAME;
                main.PHONE = reservation.PHONE;
                main.RES_ID = reservation.RES_ID;
                main.STATUS = reservation.STATUS;
                main.SUPL_CODE = reservation.SUPL_CODE;
                main.TOTAL_AMT = reservation.TOTAL_AMT;

                foreach (var item in reservation.RESERVATION_DETAIL)
                {
                    ReservationDetail detail = new ReservationDetail();
                    detail.ADULTS = item.ADULTS;
                    detail.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
                    detail.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
                    detail.CHILDREN = item.CHILDREN;
                    detail.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                    detail.GST = item.GST;
                    detail.OTHER_CHARGES = item.OTHER_CHARGES;
                    detail.RATE_PER_DAY = item.RATE_PER_DAY;
                    detail.RES_ID = item.RES_ID;
                    detail.RES_STS = item.RES_STS;
                    detail.REVS_DAYS = item.REVS_DAYS;
                    detail.ROOM_CATEGORY = item.ROOM.CATEGORY;
                    detail.ROOM_CODE = item.ROOM.ROOM_CODE;
                    detail.ROOM_NAME = item.ROOM.ROOM_NAME;
                    detail.ROOM_TYPE = item.ROOM.TYPE;
                    detail.SERVICES_CHARGES = item.SERVICES_CHARGES;
                    detail.TABLES_CODE = item.TABLES_CODE;
                    detail.TOTAL_AMT = item.TOTAL_AMT;
                    detail.STATUS = item.STATUS;
                    detaillist.Add(detail);
                }
                foreach (var item in reservation.RESERVATION_DETAIL_HISTORY)
                {
                    ReservationDetail detail = new ReservationDetail();
                    detail.ADULTS = item.ADULTS;
                    detail.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
                    detail.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
                    detail.CHILDREN = item.CHILDREN;
                    detail.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                    detail.GST = item.GST;
                    detail.OTHER_CHARGES = item.OTHER_CHARGES;
                    detail.RATE_PER_DAY = item.RATE_PER_DAY;
                    detail.RES_ID = item.RES_ID;
                    detail.RES_STS = item.RES_STS;
                    detail.REVS_DAYS = item.REVS_DAYS;
                    detail.ROOM_CATEGORY = item.ROOM.CATEGORY;
                    detail.ROOM_CODE = item.ROOM.ROOM_CODE;
                    detail.ROOM_NAME = item.ROOM.ROOM_NAME;
                    detail.ROOM_TYPE = item.ROOM.TYPE;
                    detail.SERVICES_CHARGES = item.SERVICES_CHARGES;
                    detail.TABLES_CODE = item.TABLES_CODE;
                    detail.TOTAL_AMT = item.TOTAL_AMT;
                    detail.STATUS = item.STATUS;
                    detaillist.Add(detail);
                }
                foreach (var item in reservation.AMENITY_PRODUCT_DETAIL)
                {
                    ReservationItems prod = new ReservationItems();
                    prod.AMOUNT = item.AMOUNT;
                    prod.BARCODE = item.BARCODE;
                    prod.DATETIME = item.DATETIME;
                    prod.DESCRIPTION = item.PRODUCT.DESCRIPTION;
                    prod.COMMENTS = item.COMMENTS;
                    prod.QUANTITY = item.QUANTITY;
                    prod.RES_ID = item.RES_ID;
                    prod.ROOM_CODE = item.ROOM_CODE;
                    prod.ROOM_NAME = item.ROOM.ROOM_NAME;
                    prod.UNIT_RETAIL = item.PRODUCT.UNIT_RETAIL;
                    prod.STATUS = item.STATUS;
                    if (item.TYPE == "P")
                        productList.Add(prod);
                    else
                        amenityList.Add(prod);
                }
                foreach (var item in reservation.AMENITY_PRODUCT_DETAIL_HISTORY)
                {
                    ReservationItems prod = new ReservationItems();
                    prod.AMOUNT = item.AMOUNT;
                    prod.BARCODE = item.BARCODE;
                    prod.DATETIME = item.DATETIME;
                    prod.DESCRIPTION = item.PRODUCT.DESCRIPTION;
                    prod.COMMENTS = item.COMMENTS;
                    prod.QUANTITY = item.QUANTITY;
                    prod.RES_ID = item.RES_ID;
                    prod.ROOM_CODE = item.ROOM_CODE;
                    prod.ROOM_NAME = item.ROOM.ROOM_NAME;
                    prod.UNIT_RETAIL = item.PRODUCT.UNIT_RETAIL;
                    prod.STATUS = item.STATUS;
                    if (item.TYPE == "P")
                        productList.Add(prod);
                    else
                        amenityList.Add(prod);
                }

                ReservationDetail detailData = new ReservationDetail();
                detailData.RoomCategoryList = db.ROOM_CATEGORY.ToList();
                detailData.RoomTypeList = db.ROOM_TYPE.ToList();
                detailData.RoomList = db.ROOMs.ToList();

                vm.main = main;
                vm.detail = detailData;
                vm.detailList = detaillist;
                vm.itemList = db.PRODUCTS.Where(x => x.TYPE == "P").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
                vm.amenityList = db.PRODUCTS.Where(x => x.TYPE == "S").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
                vm.reservationItems = productList;
                vm.reservationAmenities = amenityList;
                return View("Index", vm);
            }
            else
                return Content("Not Found");
        }
        //public ActionResult Edit(int id)
        //{
        //    ViewBag.Update = true;

        //    ReservationViewModel vm = new ReservationViewModel();
        //    var reservation = db.RESERVATION_MAIN
        //        .Include(X => X.AMENITY_PRODUCT_DETAIL)
        //        .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.PRODUCT))
        //        .Include(X => X.AMENITY_PRODUCT_DETAIL.Select(x => x.ROOM))
        //        .Include(x => x.RESERVATION_DETAIL)
        //        .Include(x => x.RESERVATION_DETAIL.Select(y => y.ROOM))
        //        .Where(x => x.RES_ID == id).SingleOrDefault();
        //    if (reservation != null)
        //    {
        //        ReservationMain main = new ReservationMain();
        //        List<ReservationDetail> detaillist = new List<ReservationDetail>();
        //        List<ReservationItems> productList = new List<ReservationItems>();
        //        List<ReservationItems> amenityList = new List<ReservationItems>();
        //        main.ADDRESS = reservation.ADDRESS;
        //        main.AMOUNT_PAID = reservation.AMOUNT_PAID;
        //        main.BALANCE = reservation.BALANCE;
        //        main.Cities = db.CITies.ToList();
        //        main.CITY = reservation.CITY;
        //        main.COMPANY = reservation.COMPANY;
        //        main.Countries = db.COUNTRies.ToList();
        //        main.COUNTRY = reservation.COUNTRY;
        //        main.DISCOUNT = reservation.DISCOUNT;
        //        main.DOB = reservation.DOB;
        //        main.DOC_DATE = reservation.DOC_DATE;
        //        main.E_MAIL = reservation.E_MAIL;
        //        main.FIRST_NAME = reservation.FIRST_NAME;
        //        main.IdTypes = IdTypes();
        //        main.ID_NO = reservation.ID_NO;
        //        main.LAST_NAME = reservation.LAST_NAME;
        //        main.PHONE = reservation.PHONE;
        //        main.RES_ID = reservation.RES_ID;
        //        main.STATUS = reservation.STATUS;
        //        main.SUPL_CODE = reservation.SUPL_CODE;
        //        main.TOTAL_AMT = reservation.TOTAL_AMT;

        //        foreach (var item in reservation.RESERVATION_DETAIL)
        //        {
        //            ReservationDetail detail = new ReservationDetail();
        //            detail.ADULTS = item.ADULTS;
        //            detail.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
        //            detail.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
        //            detail.CHILDREN = item.CHILDREN;
        //            detail.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
        //            detail.GST = item.GST;
        //            detail.OTHER_CHARGES = item.OTHER_CHARGES;
        //            detail.RATE_PER_DAY = item.RATE_PER_DAY;
        //            detail.RES_ID = item.RES_ID;
        //            detail.RES_STS = item.RES_STS;
        //            detail.REVS_DAYS = item.REVS_DAYS;
        //            detail.ROOM_CATEGORY = item.ROOM.CATEGORY;
        //            detail.ROOM_CODE = item.ROOM.ROOM_CODE;
        //            detail.ROOM_NAME = item.ROOM.ROOM_NAME;
        //            detail.ROOM_TYPE = item.ROOM.TYPE;
        //            detail.SERVICES_CHARGES = item.SERVICES_CHARGES;
        //            detail.TABLES_CODE = item.TABLES_CODE;
        //            detail.TOTAL_AMT = item.TOTAL_AMT;
        //            detail.STATUS = item.STATUS;
        //            detaillist.Add(detail);
        //        }
        //        foreach (var item in reservation.AMENITY_PRODUCT_DETAIL)
        //        {
        //            ReservationItems prod = new ReservationItems();
        //            prod.AMOUNT = item.AMOUNT;
        //            prod.BARCODE = item.BARCODE;
        //            prod.DATETIME = item.DATETIME;
        //            prod.DESCRIPTION = item.PRODUCT.DESCRIPTION;
        //            prod.QUANTITY = item.QUANTITY;
        //            prod.RES_ID = item.RES_ID;
        //            prod.ROOM_CODE = item.ROOM_CODE;
        //            prod.ROOM_NAME = item.ROOM.ROOM_NAME;
        //            prod.UNIT_RETAIL = item.PRODUCT.UNIT_RETAIL;
        //            prod.STATUS = item.STATUS;
        //            if (item.TYPE == "P")
        //                productList.Add(prod);
        //            else
        //                amenityList.Add(prod);
        //        }

        //        ReservationDetail detailData = new ReservationDetail();
        //        detailData.RoomCategoryList = db.ROOM_CATEGORY.ToList();
        //        detailData.RoomTypeList = db.ROOM_TYPE.ToList();
        //        detailData.RoomList = db.ROOMs.ToList();

        //        vm.main = main;
        //        vm.detail = detailData;
        //        vm.detailList = detaillist;
        //        vm.itemList = db.PRODUCTS.Where(x => x.TYPE == "P").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
        //        vm.amenityList = db.PRODUCTS.Where(x => x.TYPE == "S").Select(x => new ReservationItems { BARCODE = x.BARCODE, DESCRIPTION = x.DESCRIPTION, UNIT_RETAIL = x.UNIT_RETAIL }).ToList();
        //        vm.reservationItems = productList;
        //        vm.reservationAmenities = amenityList;
        //        return View("Index", vm);
        //    }
        //    else
        //        return Content("Not Found");
        //}
        public static IEnumerable<SelectListItem> IdTypes()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem{Text = "CNIC", Value = "C"},
                new SelectListItem{Text = "Driving Licence", Value = "D"},
                new SelectListItem{Text = "Other", Value = "O"},
            };
            return items;
        }
        public JsonResult GetEvents()
        {
            var dateTimeNow = DateTime.Now.AddDays(-1);

            var vm = (from main in db.RESERVATION_MAIN.Where(x => x.STATUS != "3").AsEnumerable()
                      join detail in db.RESERVATION_DETAIL.AsEnumerable() on main.RES_ID equals detail.RES_ID
                      where (detail.CHECKIN_DATETIME > dateTimeNow && main.STATUS == "1") || (detail.CHECKIN_DATETIME > dateTimeNow && main.STATUS != "3" && main.STATUS != "1")
                      select new
                      {
                          Id = detail.RES_ID,
                          Name = main.FIRST_NAME + main.LAST_NAME,
                          CheckIn = detail.CHECKIN_DATETIME.Date,
                          CheckOut = detail.CHECKOUT_DATETIME,
                          Status = main.STATUS,
                          Room = detail.ROOM_CODE
                      }
                ).ToList();

            //var vm = (from main in db.RESERVATION_MAIN.Where(x => x.STATUS != "3")
            //          join detail in db.RESERVATION_DETAIL on main.RES_ID equals detail.RES_ID
            //          where (detail.CHECKIN_DATETIME > dateTimeNow && main.STATUS == "1") || (detail.CHECKIN_DATETIME > dateTimeNow && main.STATUS != "3" && main.STATUS != "1")
            //          select new
            //          {
            //              Id = detail.RES_ID,
            //              Name = main.FIRST_NAME + main.LAST_NAME,
            //              CheckIn = detail.CHECKIN_DATETIME,
            //              CheckOut = detail.CHECKOUT_DATETIME,
            //              Status = main.STATUS,
            //              Room = detail.ROOM_CODE
            //          }
            //    ).ToList();

            string value = String.Empty;
            value = JsonConvert.SerializeObject(vm, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        public static string Trim(string str)
        {
            if (!string.IsNullOrEmpty(str))
                return str.Trim();
            else
                return str;

        }
        public string CustomerCode(int i)
        {
            string s = db.SUPPLIERs/*.Where(u => u.PARTY_TYPE == prefix)*/.Max(u => u.SUPL_CODE);

            if (string.IsNullOrEmpty(s))
            {
                return "000001";
            }
            string code = CommonFunctions.GenerateCode(s, i);
            return code;
        }
        public ActionResult CancelBooking(int id)
        {
            if (id != null && id > 0)
            {
                try
                {
                    var reservation = db.RESERVATION_MAIN.Where(x => x.RES_ID == id).SingleOrDefault();
                    if (reservation != null)
                    {
                        reservation.STATUS = Constants.DocumentStatus.Pending;
                        db.SaveChanges();
                        return Content("OK");
                    }
                    else
                        return Content("EX");
                }
                catch (Exception)
                {
                    return Content("EX");
                }
            }
            else
                return Content("EX");
        }
        [Permission("Authorize")]
        public ActionResult Authorize(ReservationViewModel vm)
        {
            if (vm == null || vm.main == null || vm.detailList == null || string.IsNullOrEmpty(vm.main.RES_ID.ToString()) || vm.main.RES_ID == 0 || vm.main.RES_ID == null)
                return Content("Fail");
            ReservationController resSave = new ReservationController();
            resSave.Save(vm);
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {

                try
                {
                    var main = db.RESERVATION_MAIN.Where(x => x.RES_ID == vm.main.RES_ID).SingleOrDefault();
                    if (main != null)
                    {
                        main.STATUS = "3";
                        db.SaveChanges();
                    }

                    var detailList = db.RESERVATION_DETAIL.Where(x => x.RES_ID == vm.main.RES_ID).ToList();
                    detailList.ForEach(x => x.STATUS = Constants.DocumentStatus.AuthorizedDocument);
                    db.SaveChanges();

                    var amenityProductList = db.AMENITY_PRODUCT_DETAIL.Where(x => x.RES_ID == vm.main.RES_ID).ToList();
                    amenityProductList.ForEach(x => x.STATUS = Constants.DocumentStatus.AuthorizedDocument);
                    db.SaveChanges();
                    ReservationDetailMigration();
                    AminityProductToMigration();

                    transaction.Commit();
                    return Content("3");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string s = ex.ToString();
                    return Content("EX");
                }
            }
        }
        [Permission("Authorize")]
        public ActionResult AuthrizeSingleReservation(ReservationDetail detail)
        {
            if (detail != null)
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var IsAuthorizedReservationExist = db.RESERVATION_DETAIL.Where(x => x.RES_ID == detail.RES_ID && x.ROOM_CODE == detail.ROOM_CODE).SingleOrDefault();
                        if (IsAuthorizedReservationExist == null)
                        {
                            IsAuthorizedReservationExist = new RESERVATION_DETAIL();
                            IsAuthorizedReservationExist.ADULTS = detail.ADULTS;
                            IsAuthorizedReservationExist.CHECKIN_DATETIME = detail.CHECKIN_DATETIME.Value;
                            IsAuthorizedReservationExist.CHECKOUT_DATETIME = detail.CHECKOUT_DATETIME.Value;
                            IsAuthorizedReservationExist.CHILDREN = detail.CHILDREN;
                            IsAuthorizedReservationExist.ESTIMATED_CHECKOUT_DATETIME = detail.ESTIMATED_CHECKOUT_DATETIME;
                            IsAuthorizedReservationExist.GST = detail.GST;
                            IsAuthorizedReservationExist.OTHER_CHARGES = detail.OTHER_CHARGES;
                            IsAuthorizedReservationExist.RATE_PER_DAY = detail.RATE_PER_DAY;
                            IsAuthorizedReservationExist.RES_ID = detail.RES_ID;
                            IsAuthorizedReservationExist.ROOM_CODE = detail.ROOM_CODE;
                            IsAuthorizedReservationExist.SERVICES_CHARGES = detail.SERVICES_CHARGES;
                            IsAuthorizedReservationExist.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                            IsAuthorizedReservationExist.TOTAL_AMT = detail.TOTAL_AMT;
                            db.RESERVATION_DETAIL.Add(IsAuthorizedReservationExist);
                        }
                        else
                        {
                            if (IsAuthorizedReservationExist.STATUS != Constants.DocumentStatus.AuthorizedDocument)
                            {
                                IsAuthorizedReservationExist.ADULTS = detail.ADULTS;
                                IsAuthorizedReservationExist.CHECKIN_DATETIME = detail.CHECKIN_DATETIME.Value;
                                IsAuthorizedReservationExist.CHECKOUT_DATETIME = detail.CHECKOUT_DATETIME.Value;
                                IsAuthorizedReservationExist.CHILDREN = detail.CHILDREN;
                                IsAuthorizedReservationExist.ESTIMATED_CHECKOUT_DATETIME = detail.ESTIMATED_CHECKOUT_DATETIME;
                                IsAuthorizedReservationExist.GST = detail.GST;
                                IsAuthorizedReservationExist.OTHER_CHARGES = detail.OTHER_CHARGES;
                                IsAuthorizedReservationExist.RATE_PER_DAY = detail.RATE_PER_DAY;
                                IsAuthorizedReservationExist.RES_ID = detail.RES_ID;
                                IsAuthorizedReservationExist.ROOM_CODE = detail.ROOM_CODE;
                                IsAuthorizedReservationExist.SERVICES_CHARGES = detail.SERVICES_CHARGES;
                                IsAuthorizedReservationExist.STATUS = Constants.DocumentStatus.AuthorizedDocument;
                                IsAuthorizedReservationExist.TOTAL_AMT = detail.TOTAL_AMT;
                            }
                            else
                            {
                                return Json("Wrong", JsonRequestBehavior.AllowGet);
                            }
                        }
                        db.SaveChanges();

                        var AminityProductList = db.AMENITY_PRODUCT_DETAIL.Where(x => x.RES_ID == detail.RES_ID && x.ROOM_CODE == detail.ROOM_CODE).ToList();
                        if (AminityProductList.Count > 0 && AminityProductList != null)
                            AminityProductList.ForEach(x => x.STATUS = Constants.DocumentStatus.AuthorizedDocument);
                        db.SaveChanges();
                        ReservationDetailMigration();
                        AminityProductToMigration();
                        transaction.Commit();
                        return Json("OK", JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json("Ex", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("Wrong", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetRooms(string category, string type, DateTime checkIn, DateTime checkOut, long? res_Id)
        {
            //var In = DateTime.ParseExact(checkIn, "dd/MM/yyyy hh:mm:ss tt", null);
            //var Out = DateTime.ParseExact(checkOut, "dd/MM/yyyy hh:mm:ss tt", null);
            
            ///Selecting Rooms And Their ReservationDetail Against Selected Category And Type
            var roomList = db.ROOMs.Where(x => x.CATEGORY == category & x.TYPE == type).Select(x => new RoomDropDownModel { detail = x.RESERVATION_DETAIL, code = x.ROOM_CODE, name = x.ROOM_NAME, rate = x.RATE }).ToList();
            ///Declaring Array For Room Code That Are Reserved and Then Removed that Room From Drop Down Data
            List<string> roomCodes = new List<string>();
            ///Iterate Through Every Room
            foreach (var room in roomList)
            {
                ///Checking Room Detail for reservation 
                foreach (var detail in room.detail)
                {
                    ///This Check Us For Selecting Same Room In Case Of Edit Mode, when a detail room is selected for editing, to select already reserved room agaist same reservation selected for editing
                    if (res_Id != null && res_Id > 0 && detail.RES_ID == res_Id)
                    {
                        ///if selected room checkout date is between a reservation then show warning message
                        if (checkOut.Date >= detail.CHECKIN_DATETIME.Date)
                        {
                            room.alertMessage = "This Room Has A Booking On " + detail.CHECKIN_DATETIME.ToString() + " Against Reservation: " + detail.RES_ID.ToString();
                        }
                    }
                    else
                    {
                        ///if selected room checkin date is between a reservation then room code is selected and then to be removed from list
                        if (checkIn.Date >= detail.CHECKIN_DATETIME.Date && checkIn.Date <= detail.CHECKOUT_DATETIME.Date)
                        {
                            roomCodes.Add(room.code);
                        }
                        ///if selected room checkout date is between a reservation then show warning message
                        else if (checkOut.Date >= detail.CHECKIN_DATETIME.Date)
                        {
                            room.alertMessage = "This Room Has A Booking On " + detail.CHECKIN_DATETIME.ToString() + " Against Reservation: " + detail.RES_ID.ToString();
                        }
                    }
                }
                room.detail = null;
            }
            
            ///remove room that are already reserved
            foreach (var room in roomCodes)
            {
                var r = roomList.Where(x => x.code == room).SingleOrDefault();
                roomList.Remove(r);
            }

            var value = JsonConvert.SerializeObject(roomList, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Json(value, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult GetRooms(string category, string type, DateTime checkIn, DateTime checkOut)
        //{
        //    var roomList = db.ROOMs.Where(x => x.CATEGORY == category & x.TYPE == type).Select(x => new {code = x.ROOM_CODE, name = x.ROOM_NAME }).ToList();

        //    var value = JsonConvert.SerializeObject(roomList, Formatting.Indented, new JsonSerializerSettings
        //    {
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //    });
        //    return Json(value, JsonRequestBehavior.AllowGet);
        //}
        public void ReservationDetailMigration()
        {
            var list = db.RESERVATION_DETAIL.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument).ToList();
            if (list.Count > 0 && list != null)
            {
                foreach (var item in list)
                {
                    RESERVATION_DETAIL_HISTORY newObject = new RESERVATION_DETAIL_HISTORY();
                    newObject.ADULTS = item.ADULTS;
                    newObject.CHECKIN_DATETIME = item.CHECKIN_DATETIME;
                    newObject.CHECKOUT_DATETIME = item.CHECKOUT_DATETIME;
                    newObject.CHILDREN = item.CHILDREN;
                    newObject.ESTIMATED_CHECKOUT_DATETIME = item.ESTIMATED_CHECKOUT_DATETIME;
                    newObject.GST = item.GST;
                    newObject.OTHER_CHARGES = item.OTHER_CHARGES;
                    newObject.RATE_PER_DAY = item.RATE_PER_DAY;
                    newObject.RES_ID = item.RES_ID;
                    newObject.RES_STS = item.RES_STS;
                    newObject.REVS_DAYS = item.REVS_DAYS;
                    newObject.STATUS = item.STATUS;
                    newObject.ROOM_CODE = item.ROOM_CODE;
                    newObject.SERVICES_CHARGES = item.SERVICES_CHARGES;
                    newObject.TABLES_CODE = item.TABLES_CODE;
                    newObject.TOTAL_AMT = item.TOTAL_AMT;
                    db.RESERVATION_DETAIL_HISTORY.Add(newObject);
                }
                db.SaveChanges();

                db.RESERVATION_DETAIL.RemoveRange(list);
                db.SaveChanges();
            }
        }
        public void AminityProductToMigration()
        {
            var list = db.AMENITY_PRODUCT_DETAIL.Where(x => x.STATUS == Constants.DocumentStatus.AuthorizedDocument).ToList();
            if (list.Count > 0 && list != null)
            {
                foreach (var item in list)
                {
                    AMENITY_PRODUCT_DETAIL_HISTORY pro = new AMENITY_PRODUCT_DETAIL_HISTORY();
                    pro.AMOUNT = item.AMOUNT;
                    pro.BARCODE = item.BARCODE;
                    pro.TYPE = item.TYPE;
                    pro.DATETIME = item.DATETIME;
                    pro.QUANTITY = item.QUANTITY;
                    pro.RES_ID = item.RES_ID;
                    pro.ROOM_CODE = item.ROOM_CODE;
                    pro.STATUS = item.STATUS;
                    pro.REMARKS = item.REMARKS;
                    pro.COMMENTS = item.COMMENTS;
                    db.AMENITY_PRODUCT_DETAIL_HISTORY.Add(pro);
                }
                db.SaveChanges();

                db.AMENITY_PRODUCT_DETAIL.RemoveRange(list);
                db.SaveChanges();
            }
        }

    }


}