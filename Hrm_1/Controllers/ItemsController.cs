using Inventory.Filters;
using Inventory.Helper;
using Inventory.ViewModels;
using Inventory.ViewModels.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Inventory.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        readonly DataContext db;
        CloudinaryController cloudinaryCont;
        public ItemsController()
        {
            db = new DataContext();
            ViewBag.Department = db.DEPARTMENTs.ToList();
            ViewBag.Supplier = db.SUPPLIERs.Where(x => x.PARTY_TYPE == Constants.CustomerSupplier.Supplier).ToList();
            cloudinaryCont = new CloudinaryController();
        }
        [Permission("Item Entry")]
        public ActionResult Index()
        {
            ViewBag.UpdateMode = false;

            if (TempData["Message"] != null)
                ViewBag.Message = TempData["Message"].ToString();

            //var lit = db.PRODUCTS.ToList();
            //foreach(var item in lit)
            //{
            //    var path = Server.MapPath("~/images/ProductImages1/" +item.BARCODE + ".jpg");
            //    if ((System.IO.File.Exists(path)))
            //    {
            //        cloudinaryCont.SaveinCloudinary(path, item.BARCODE);
            //    }
            //var path1 = Server.MapPath("~/images/ProductImages1/" + item.BARCODE + "-A.jpg");
            //    if ((System.IO.File.Exists(path)))
            //    {
            //        cloudinaryCont.SaveinCloudinary(path, item.BARCODE+"-A");
            //    }

            //}


            ViewBag.SelectedGroup = db.GROUPS.ToList();
            ViewBag.SelectedSubGroup = db.SUB_GROUPS.ToList();

            ViewBag.Barcode = GetNewBarCode();

            ItemRegistration vm = new ItemRegistration();
            vm.CtnPcs = 1;
            return View(vm);
        }
        public JsonResult CkeckUanExist(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json("Null", JsonRequestBehavior.AllowGet);
            }
            id = id.Trim();
            var IsExist = db.PRODUCT_UAN_LIST.Any(x => x.UAN_NO == id);
            if (IsExist)
                return Json("True", JsonRequestBehavior.AllowGet);
            else
                return Json("False", JsonRequestBehavior.AllowGet);
        }
        public string GetNewBarCode()
        {
            string s = db.PRODUCTS.Max(u => u.BARCODE);
            if (s == "" || s == null)
                return "000001";
            else
                return (Convert.ToInt32(s) + 1).ToString().PadLeft(6, '0');
        }
        [HttpPost]
        public ActionResult Save(ItemRegistration Item)
        {
            ViewBag.UpdateMode = false;
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Model State is Not Valid";
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                return View("Index", Item);
            }
            var rfCode = db.PRODUCTS.Any(x => x.REFERENCE_CODE == Item.ReferenceCode);
            if (rfCode)
            {
                ViewBag.Message = "Reference Code Is Already Exist";
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                return View("Index", Item);
            }
            string UserName = CommonFunctions.GetUserName();

            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                PRODUCT product = new PRODUCT();
                product.BARCODE = GetNewBarCode();
                product.DEPT_CODE = Item.DeptCode;
                product.GROUP_CODE = Item.GroupCode;
                product.SRGOUP_CODE = Item.SubGroupCode;
                product.DESCRIPTION = Item.Description;
                if (Item.Description.Length >= 30)
                    product.SHORT_DESC = Item.Description.Substring(0, 29);
                else
                    product.SHORT_DESC = Item.Description;

                product.UNIT_COST = Item.UnitCost;
                product.UNIT_RETAIL = Item.UnitRetail;
                product.TYPE = Item.Type;
                product.PACK_QTY = Item.PackQuantity;
                product.URDU = Item.UrduName;
                product.PACK_RATE = Item.PackRetail;

                if (Item.IsRfCodeRequired == "1")
                {
                    product.REFERENCE_CODE = product.BARCODE;
                }
                else
                {
                    product.REFERENCE_CODE = Item.ReferenceCode;
                }





                product.IMG_DESC = Item.ImgDesc;
                product.IMG_DESC2 = Item.ImgDesc2;
                product.COMMENTS = Item.Comments;
                product.MIN_QTY = Item.MinQuantity;
                product.MAX_QTY = Item.MaxQuantity;
                //product.BULK_RETAIL = Item.BulkRetail;
                product.MARKET_RETAIL = Item.MarketRetail;
                product.MIN_RETAIL = Item.MinRetail;
                product.CTN_PCS = Item.CtnPcs == null ? 1 : Item.CtnPcs;
                product.GST = Item.GSTType;
                product.GST_PER = Item.GST_Percentage;
                product.TRANSFER_STATUS = "0";
                if (Item.UanNoList != null && Item.UanNoList.Count > 0)
                    product.UAN_NO = Item.UanNoList.FirstOrDefault();
                else
                    product.UAN_NO = product.BARCODE;

                
                product.REORDER_LVL = Item.ReorderLevel;
                product.INSERTED_BY = UserName;
                product.DOC = CommonFunctions.GetDateTimeNow();
                product.STATUS = Item.Status;

                product.ACTIVE = Item.Active ?? false;
                product.PROMOTION = Item.Offer == true ? "a" : "i" ;
                product.PROM_START_DTE = Item.StartOn;
                product.PROM_END_DTE = Item.EndOn;
                product.PROMO_PER = Item.PromoPercentage;

                product.UPDATED_BY = UserName;
                product.UPDATE_DATE = CommonFunctions.GetDateTimeNow();


                product.SEARCHING_WORDS = Item.SearchingKeywords;
                product.BRAND_ID = Item.Brand;

                if (Item.Picture != null)
                {
                    var ImageNameWithExtension = Item.Picture.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    //string fileName = product.BARCODE + "." + GettingExtension[1];
                    string fileName = product.BARCODE + "." + "jpg";

                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture.SaveAs(path);

                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE);

                }

                if (Item.Picture2 != null)
                {
                    var ImageNameWithExtension = Item.Picture2.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-A" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture2.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE);

                }
                if (Item.Picture3 != null)
                {
                    var ImageNameWithExtension = Item.Picture3.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-B" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture3.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE);

                }
                if (Item.Picture4 != null)
                {
                    var ImageNameWithExtension = Item.Picture4.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-C" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture4.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE);

                }

                db.PRODUCTS.Add(product);
                db.SaveChanges();

                if (Item.UanNoList != null && Item.UanNoList.Count > 0)
                {
                    List<PRODUCT_UAN_LIST> ProUanList = new List<PRODUCT_UAN_LIST>();
                    foreach (var item in Item.UanNoList)
                    {
                        PRODUCT_UAN_LIST ProUan = new PRODUCT_UAN_LIST();
                        ProUan.BARCODE = product.BARCODE;
                        ProUan.UAN_NO = item;
                        ProUan.TRANSFER_STATUS = "0";
                        ProUanList.Add(ProUan);
                    }
                    db.PRODUCT_UAN_LIST.AddRange(ProUanList);
                }
                else
                {
                    PRODUCT_UAN_LIST ProUan = new PRODUCT_UAN_LIST();
                    ProUan.BARCODE = product.BARCODE;
                    ProUan.UAN_NO = ProUan.BARCODE;
                    ProUan.TRANSFER_STATUS = "0";
                    db.PRODUCT_UAN_LIST.Add(ProUan);
                }
                db.SaveChanges();

                SUPPLIER_PRODUCTS SupplierProduct = new SUPPLIER_PRODUCTS();
                SupplierProduct.ACTIVE_DATE = CommonFunctions.GetDateTimeNow();
                SupplierProduct.BARCODE = product.BARCODE;
                SupplierProduct.DOC = CommonFunctions.GetDateTimeNow();
                SupplierProduct.INSERTED_BY = UserName;
                SupplierProduct.UPDATED_BY = UserName;
                SupplierProduct.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                SupplierProduct.STATUS = "1";
                SupplierProduct.SUPL_CODE = Item.SupplierCode;
                SupplierProduct.TRANSFER_STATUS = "0";

                db.SUPPLIER_PRODUCTS.Add(SupplierProduct);
                db.SaveChanges();

                transaction.Commit();
                TempData["Message"] = "Product Added Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception dbEx)
            {
                transaction.Rollback();
                ViewBag.Message = "Exception";
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                return View("Index", Item);
            }
        }
        [HttpGet]
        [Permission("Edit Document")]
        public ActionResult Edit(string id)
        {
            ViewBag.UpdateMode = true;

            ViewBag.Barcode = id;

            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Message = "Parameter Null";
                return View("List");
            }

            if (TempData["Message"] != null)
                ViewBag.Message = TempData["Message"];

            var Found = db.PRODUCTS.Where(x => x.BARCODE == id).SingleOrDefault();
            if (Found == null)
            {
                ViewBag.Message = "Product Not Found";
                return RedirectToAction("Index");
            }
            ItemRegistration Item = new ItemRegistration();

            Item.Barcode = Found.BARCODE;
            Item.DeptCode = Found.DEPT_CODE;
            Item.GroupCode = Found.GROUP_CODE;
            Item.SubGroupCode = Found.SRGOUP_CODE;
            Item.Description = Found.DESCRIPTION;
            Item.UnitCost = Found.UNIT_COST;
            Item.UnitRetail = Found.UNIT_RETAIL;
            Item.PackQuantity = Found.PACK_QTY;

            Item.ReferenceCode = Found.REFERENCE_CODE;
            Item.ImgDesc = Found.IMG_DESC;
            Item.ImgDesc2 = Found.IMG_DESC2;
            Item.Comments = Found.COMMENTS;

            Item.PackRetail = Found.PACK_RATE;
            Item.MinQuantity = Found.MIN_QTY;
            Item.MaxQuantity = Found.MAX_QTY;
            Item.UrduName = Found.URDU;
            Item.MarketRetail = Found.MARKET_RETAIL;
            Item.MinRetail = Found.MIN_RETAIL;
            Item.CtnPcs = Found.CTN_PCS;
            Item.GSTType = Found.GST;
            Item.GST_Percentage = Found.GST_PER;
            Item.Type = Found.TYPE;
            Item.ReorderLevel = Found.REORDER_LVL;
            Item.GroupCode = Found.GROUP_CODE;
            Item.SubGroupCode = Found.SRGOUP_CODE;
            Item.Image = "false";
            var path = Server.MapPath("~/images/ProductImages/" + Found.BARCODE + ".jpg");
            var picExist = new FileInfo(path);
            if (picExist.Exists)
            {
                Item.Image = "true";
            }
            Item.Image1 = "false";
            var path1 = Server.MapPath("~/images/ProductImages/" + Found.BARCODE + "-A.jpg");
            var pic1Exist = new FileInfo(path1);
            if (pic1Exist.Exists)
            {
                Item.Image1 = "true";
            }
            Item.Image2 = "false";
            var path2 = Server.MapPath("~/images/ProductImages/" + Found.BARCODE + "-B.jpg");
            var pic2Exist = new FileInfo(path2);
            if (pic2Exist.Exists)
            {
                Item.Image2 = "true";
            }
            Item.Image3 = "false";
            var path3 = Server.MapPath("~/images/ProductImages/" + Found.BARCODE + "-C.jpg");
            var pic3Exist = new FileInfo(path3);
            if (pic3Exist.Exists)
            {
                Item.Image3 = "true";
            }


            Item.Active = Found.ACTIVE ?? false;
            Item.Status = Found.STATUS;
            Item.Offer = Found.PROMOTION == "a" ? true : false;
            Item.StartOn = Found.PROM_START_DTE;
            Item.EndOn = Found.PROM_END_DTE;
            Item.PromoPercentage = Found.PROMO_PER;

            Item.SearchingKeywords = Found.SEARCHING_WORDS;
            Item.Brand = Found.BRAND_ID;

            Item.SupplierCode = db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == Found.BARCODE && x.STATUS == "1").Select(x => x.SUPL_CODE).SingleOrDefault();

            var prodBalance = db.PROD_BALANCE.SingleOrDefault(u => u.BARCODE == id);
            if (prodBalance != null)
            {
                var QuantityInHand = prodBalance.CURRENT_QTY ?? 0;
                var AvgAmount = prodBalance.AVG_COST;
                var InHandAmount = QuantityInHand * Found.UNIT_COST ?? 0;
                Item.OnHandAmt = InHandAmount;
                Item.OnHandQty = QuantityInHand;
                Item.AvgCost = AvgAmount;
            }

            var productSuppliser = db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == Found.BARCODE).Select(x => new { x.SUPL_CODE, x.STATUS }).ToList();
            List<SUPPLIER> SuppliersList = new List<SUPPLIER>();
            if (productSuppliser.Count > 0)
            {
                foreach (var pSupp in productSuppliser)
                {
                    var supplier = db.SUPPLIERs.Where(x => x.SUPL_CODE == pSupp.SUPL_CODE).SingleOrDefault();
                    if (supplier != null)
                    {
                        supplier.STATUS = pSupp.STATUS;
                        SuppliersList.Add(supplier);
                    }
                }
            }

            Item.UanNoList = db.PRODUCT_UAN_LIST.Where(x => x.BARCODE == id).Select(x => x.UAN_NO).ToList();
            ViewBag.ProductSuppliers = SuppliersList;
            ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode && x.DEPT_CODE == Item.DeptCode).ToList();
            ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode && x.GROUP_CODE == Item.GroupCode && x.DEPT_CODE == Item.DeptCode).ToList();
            return View("Index", Item);
        }
        [HttpPost]
        [Permission("Edit Document")]
        public ActionResult Edit(ItemRegistration Item)
        {
            ViewBag.UpdateMode = true;
            if (!ModelState.IsValid)
            {
                Item.UanNoList = db.PRODUCT_UAN_LIST.Where(x => x.BARCODE == Item.Barcode).Select(x => x.UAN_NO).ToList();
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                TempData["Message"] = "Model State is Not Valid";
                return RedirectToAction("Edit", Item.Barcode);
            }

            var rfCode = db.PRODUCTS.Any(x => x.REFERENCE_CODE == Item.ReferenceCode && x.BARCODE != Item.Barcode);
            if (rfCode)
            {
                Item.UanNoList = db.PRODUCT_UAN_LIST.Where(x => x.BARCODE == Item.Barcode).Select(x => x.UAN_NO).ToList();
                TempData["Message"] = "Reference Code Is Already Exist";
                ViewBag.Message = "Reference Code Is Already Exist";
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                return View("Index" ,Item);
                //return RedirectToAction("Edit","Items" ,new { id = Item.Barcode });
            }

            string UserName = CommonFunctions.GetUserName();

            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                var product = db.PRODUCTS.Where(x => x.BARCODE == Item.Barcode).SingleOrDefault();
                product.DEPT_CODE = Item.DeptCode;
                product.GROUP_CODE = Item.GroupCode;
                product.SRGOUP_CODE = Item.SubGroupCode;
                product.DESCRIPTION = Item.Description;
                if (Item.Description.Length >= 30)
                    product.SHORT_DESC = Item.Description.Substring(0, 29);
                else
                    product.SHORT_DESC = Item.Description;
                product.TYPE = Item.Type;

                product.REFERENCE_CODE = Item.ReferenceCode;
                product.IMG_DESC = Item.ImgDesc;
                product.IMG_DESC2 = Item.ImgDesc2;
                product.COMMENTS = Item.Comments;

                product.UNIT_COST = Item.UnitCost;
                product.UNIT_RETAIL = Item.UnitRetail;
                product.PACK_QTY = Item.PackQuantity;
                product.PACK_RATE = Item.PackRetail;
                product.REORDER_LVL = Item.ReorderLevel;
                product.MIN_QTY = Item.MinQuantity;
                product.MAX_QTY = Item.MaxQuantity;
                product.URDU = Item.UrduName;
                product.MARKET_RETAIL = Item.MarketRetail;
                product.MIN_RETAIL = Item.MinRetail;
                product.CTN_PCS = Item.CtnPcs == null ? 1 : Item.CtnPcs;
                product.GST = Item.GSTType;
                product.GST_PER = Item.GST_Percentage;
                product.TRANSFER_STATUS = "0";

                if (Item.UanNoList != null && Item.UanNoList.Count > 0)
                    product.UAN_NO = Item.UanNoList.FirstOrDefault();
                else
                    product.UAN_NO = product.BARCODE;



                product.ACTIVE = Item.Active ?? false;
                product.UPDATED_BY = UserName;
                product.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                product.STATUS = Item.Status;
                product.PROMOTION = Item.Offer == true ? "a" : "i";
                product.PROM_START_DTE = Item.StartOn;
                product.PROM_END_DTE = Item.EndOn;
                product.PROMO_PER = Item.PromoPercentage;

                product.SEARCHING_WORDS = Item.SearchingKeywords;
                product.BRAND_ID = Item.Brand;


                if (Item.Picture != null)
                {
                    var ImageNameWithExtension = Item.Picture.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    //string fileName = product.BARCODE + "." + GettingExtension[GettingExtension.Length - 1];
                    string fileName = product.BARCODE + "." + "jpg";

                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE);




                }
                else if (Item.Picture == null && Item.Image == "false")
                {
                    var path = Server.MapPath("~/images/ProductImages/");
                    path = path + product.BARCODE;
                    if (System.IO.File.Exists(path + ".jpg"))
                        System.IO.File.Delete(path + ".jpg");
                    else if (System.IO.File.Exists(path + ".jpeg"))
                        System.IO.File.Delete(path + ".jpeg");
                    else if (System.IO.File.Exists(path + ".png"))
                        System.IO.File.Delete(path + ".png");

                }


                if (Item.Picture2 != null)
                {
                    var ImageNameWithExtension = Item.Picture2.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-A" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture2.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE +"-A");

                }
                else if (Item.Picture2 == null && Item.Image1 == "false")
                {
                    var path = Server.MapPath("~/images/ProductImages/");
                    path = path + product.BARCODE;
                    if (System.IO.File.Exists(path + "-A.jpg"))
                        System.IO.File.Delete(path + "-A.jpg");
                    else if (System.IO.File.Exists(path + "-A.jpeg"))
                        System.IO.File.Delete(path + "-A.jpeg");
                    else if (System.IO.File.Exists(path + "-A.png"))
                        System.IO.File.Delete(path + "-A.png");
                }
                if (Item.Picture3 != null)
                {
                    var ImageNameWithExtension = Item.Picture3.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-B" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture3.SaveAs(path);
                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE+"-B");

                }
                else if (Item.Picture3 == null && Item.Image2 == "false")
                {
                    var path = Server.MapPath("~/images/ProductImages/");
                    path = path + product.BARCODE;
                    if (System.IO.File.Exists(path + "-B.jpg"))
                        System.IO.File.Delete(path + "-B.jpg");
                    else if (System.IO.File.Exists(path + "-B.jpeg"))
                        System.IO.File.Delete(path + "-B.jpeg");
                    else if (System.IO.File.Exists(path + "-B.png"))
                        System.IO.File.Delete(path + "-B.png");
                }

                if (Item.Picture4 != null)
                {
                    var ImageNameWithExtension = Item.Picture4.FileName;
                    var GettingExtension = ImageNameWithExtension.Split('.');
                    string fileName = product.BARCODE + "-C" + "." + "jpg";
                    var path = Server.MapPath("~/images/ProductImages/" + fileName);
                    Item.Picture4.SaveAs(path);

                    cloudinaryCont.SaveinCloudinary(path, product.BARCODE+"-C");

                }
                else if (Item.Picture4 == null && Item.Image3 == "false")
                {
                    var path = Server.MapPath("~/images/ProductImages/");
                    path = path + product.BARCODE;
                    if (System.IO.File.Exists(path + "-C.jpg"))
                        System.IO.File.Delete(path + "-C.jpg");
                    else if (System.IO.File.Exists(path + "-C.jpeg"))
                        System.IO.File.Delete(path + "-C.jpeg");
                    else if (System.IO.File.Exists(path + "-C.png"))
                        System.IO.File.Delete(path + "-C.png");
                }

                db.SaveChanges();

                if (Item.UanNoList != null && Item.UanNoList.Count > 0)
                {
                    var UanList = db.PRODUCT_UAN_LIST.Where(x => x.BARCODE == Item.Barcode).ToList();
                    db.PRODUCT_UAN_LIST.RemoveRange(UanList);
                    db.SaveChanges();

                    List<PRODUCT_UAN_LIST> ProUanList = new List<PRODUCT_UAN_LIST>();
                    foreach (var item in Item.UanNoList)
                    {
                        PRODUCT_UAN_LIST ProUan = new PRODUCT_UAN_LIST();
                        ProUan.BARCODE = product.BARCODE;
                        ProUan.UAN_NO = item;
                        ProUan.TRANSFER_STATUS = "0";
                        ProUanList.Add(ProUan);
                    }
                    db.PRODUCT_UAN_LIST.AddRange(ProUanList);
                }
                else
                {
                    PRODUCT_UAN_LIST ProUan = new PRODUCT_UAN_LIST();
                    ProUan.BARCODE = product.BARCODE;
                    ProUan.UAN_NO = ProUan.BARCODE;
                    ProUan.TRANSFER_STATUS = "0";
                    db.PRODUCT_UAN_LIST.Add(ProUan);
                }
                db.SaveChanges();

                db.Database.ExecuteSqlCommand("UPDATE SUPPLIER_PRODUCTS SET [STATUS] = '0', TRANSFER_STATUS = '0' WHERE BARCODE ={0}", Item.Barcode);
                db.SaveChanges();

                var supplierExist = db.SUPPLIER_PRODUCTS.Where(x => x.BARCODE == Item.Barcode && x.SUPL_CODE == Item.SupplierCode).SingleOrDefault();
                if (supplierExist != null)
                {
                    supplierExist.STATUS = "1";
                    supplierExist.TRANSFER_STATUS = "0";
                    supplierExist.UPDATED_BY = UserName;
                    supplierExist.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                }
                else
                {
                    SUPPLIER_PRODUCTS SupplierProduct = new SUPPLIER_PRODUCTS();
                    SupplierProduct.ACTIVE_DATE = CommonFunctions.GetDateTimeNow();
                    SupplierProduct.BARCODE = product.BARCODE;
                    SupplierProduct.DOC = CommonFunctions.GetDateTimeNow();
                    SupplierProduct.UPDATED_BY = UserName;
                    SupplierProduct.UPDATE_DATE = CommonFunctions.GetDateTimeNow();
                    SupplierProduct.INSERTED_BY = UserName;
                    SupplierProduct.STATUS = "1";
                    SupplierProduct.SUPL_CODE = Item.SupplierCode;
                    SupplierProduct.TRANSFER_STATUS = "0";
                    db.SUPPLIER_PRODUCTS.Add(SupplierProduct);
                }
                db.SaveChanges();

                transaction.Commit();
                TempData["Message"] = "Product Updated Successfully";
                return RedirectToAction("Edit", new { id = Item.Barcode });
            }
            catch (Exception Ex)
            {
                transaction.Rollback();
                ViewBag.SelectedGroup = db.GROUPS.Where(x => x.GROUP_CODE == Item.GroupCode).ToList();
                ViewBag.SelectedSubGroup = db.SUB_GROUPS.Where(x => x.SGROUP_CODE == Item.SubGroupCode).ToList();
                ViewBag.Message = "Exception";
                return View("Index", Item);
            }
        }
        public JsonResult FillGroup(string id)
        {
            var group = (from a in db.GROUPS where a.DEPT_CODE == id select new { a.GROUP_CODE, a.GROUP_NAME }).ToList();//db.DESIGNATIONs.Where(c => c.DEPT_CODE == id).ToList();
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
        public JsonResult FillSubGroup(string id)
        {
            var subGroup = (from a in db.SUB_GROUPS where a.DEPT_CODE == id.Substring(0, 2) && a.GROUP_CODE == id.Substring(2, 2) select new { a.SGROUP_CODE, a.SGROUP_NAME }).ToList();//db.DESIGNATIONs.Where(c => c.DEPT_CODE == id).ToList();
            var value = JsonConvert.SerializeObject(subGroup, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            var jsonResult = Json(value, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        [Permission("Item List")]
        public ActionResult List()
        {
            var products = db.PRODUCTS.Select(x => new ItemViewModel
            {
                Barcode = x.BARCODE,
                ReferenceCode = x.REFERENCE_CODE,
                Cost = x.UNIT_COST,
                Ctnpcs = x.CTN_PCS,
                Description = x.DESCRIPTION,
                Retail = x.UNIT_RETAIL,
                Uanno = x.UAN_NO,
                UrduName = x.URDU,
                Status = x.STATUS,
            }).ToList().OrderByDescending(x => x.Barcode);
            return View(products);
        }

        public static List<SelectListItem> GSTTypes = new List<SelectListItem>()
        {
            new SelectListItem() {Text="Select Tax Type", Value=""},
            new SelectListItem() {Text="General", Value="G"},
            new SelectListItem() { Text="Retails", Value="R"},
            new SelectListItem() { Text="Exempt", Value="E"}
        };
        public JsonResult CkeckRefCodeExist(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json("Null", JsonRequestBehavior.AllowGet);
            }
            id = id.Trim();
            var IsExist = db.PRODUCTS.Any(x => x.REFERENCE_CODE == id);
            if (IsExist)
                return Json("True", JsonRequestBehavior.AllowGet);
            else
                return Json("False", JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public void AddColours(string itemCode, int[] colours)
        //{
        //    int[] coloursArray = colours;//.Split(',');
        //    coloursArray = coloursArray.Distinct().ToArray();
        //    //int[] Oldcolours = db.Product_Colours.Where(u => u.BARCODE == itemCode).Select(u => u.COLOUR_ID).ToList();
        //    var colourss = db.Colours.ToList();
        //    for (int i = 0; i < coloursArray.Count(); i++)
        //    {
        //        //Product_Colours obj = new Product_Colours();
        //        //obj.BARCODE = itemCode;
        //        //obj.COLOUR_ID = Convert.ToInt16(coloursArray[i]);
        //        //db.Product_Colours.Add(obj);
        //        Product_Colours obj = new Product_Colours();
        //        var colourid = coloursArray[i];
        //        var data = db.Product_Colours.Where(x => x.BARCODE == itemCode && x.COLOUR_ID == colourid).FirstOrDefault();
        //        if (data == null)
        //        {

        //            //obj.BARCODE = itemCode;
        //            //obj.COLOUR_ID = Convert.ToInt16(coloursArray[i]);
        //            //db.Product_Colours.Add(obj);
        //            var coloursss = colourss.Where(x => x.id == colourid).FirstOrDefault();
        //            if (coloursss != null)
        //            {
        //                obj.BARCODE = itemCode;
        //                obj.COLOUR_ID = Convert.ToInt16(coloursArray[i]);
        //                db.Product_Colours.Add(obj);
        //            }
        //        }
        //    }

        //    //db.SaveChanges();

        //}
        //public ActionResult Item_Invoice()
        //{
        //    return View();
        //}
        //public ActionResult ReturnItem()
        //{
        //    return View();
        //}


        public ActionResult ReviewsList()
        {
            var products = db.PRODUCT_REVIEW.Include(x => x.PRODUCT).Select(x => new ItemViewModel
            {
                Barcode = x.BARCODE,
                Review = x.REVIEWS,
                Description = x.PRODUCT.DESCRIPTION,
                Rating = x.RATING,
                Mail = x.EMAIL,
            }).ToList().OrderByDescending(x => x.Barcode);
            return View(products);
        }
        public string ReviewStatus(string bcode, string mail)
        {
            var review = db.PRODUCT_REVIEW.Where(x => x.BARCODE == bcode && x.EMAIL == mail).SingleOrDefault();

            if (review != null)
            {
                var ret = "";

                if (review.STATUS == "1")
                {
                    review.STATUS = "0";
                    ret = "Product Review State Now Is Deactive";
                }
                else if (review.STATUS == "0")
                {
                    review.STATUS = "1";
                    ret = "Product Review State Now Is Active";
                }



                db.SaveChanges();
                return ret;
            }
            else
            {
                return "Item not found";
            }
        }

    }
}