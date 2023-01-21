using Inventory.Encryption;
using Inventory.Models.FBR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Data.Entity;

namespace Inventory.Helper
{
    public class FBRHandler
    {

        //Model is changed in service according to new documentation

        public static void Index()
        {
            DataContext context= new DataContext();
            Invoice obj = new Invoice();

            var fbrDataList = context.FBR_DATA.ToList();
            if (fbrDataList.Count > 0)
            {
                foreach (var transMN in fbrDataList)
                {
                    var InvoiceData = context.TRANS_MN.Include(x => x.TRANS_DT).Where(x => x.TRANS_NO == transMN.INVOICE_NO).SingleOrDefault();

                    obj.POSID = 12345;
                    obj.FBRInvoiceNumber = string.Empty;
                    obj.USIN = InvoiceData.TRANS_NO;
                    obj.DateTime = InvoiceData.TRANS_DATE;
                    var customer = context.SUPPLIERs.Where(x => x.SUPL_CODE == InvoiceData.PARTY_CODE).SingleOrDefault();
                    if (customer != null)
                    {
                        obj.BuyerName = customer.SUPL_NAME;
                        obj.BuyerPhoneNumber = customer.PHONE;
                    }
                    else
                    {
                        obj.BuyerName = "";
                        obj.BuyerPhoneNumber = "";
                    }
                    
                    obj.TotalBillAMount = InvoiceData.CASH_AMT ?? 0;
                    obj.TotalQuantity = Convert.ToDouble(InvoiceData.TRANS_DT.Sum(x=>x.UNITS_SOLD));
                    obj.TotalSaleValue = InvoiceData.CASH_AMT ?? 0; //------------------------------------------------------
                    obj.TotalTaxCharged = Convert.ToSingle(InvoiceData.TRANS_DT.Sum(x => x.GST_AMOUNT));
                    obj.PaymentMode = 1;//1. Cash, 2.Card, 3.Gift Voucher, 4.Loyality Card, 5.Mixed----------------------------------

                    if (InvoiceData.TRANS_TYPE == "2")
                        obj.RefUSIN = string.Empty;//-----------------------------------------

                    else if (InvoiceData.TRANS_TYPE == "4")
                        obj.RefUSIN = string.Empty; // Reference Invoice Number ---------------------------------------

                    obj.InvoiceType = 1; //1. New, 2.Debit, 3.Credit

                    UserInfo u = new UserInfo();
                    u.UserName = "Admin";
                    u.Password = "Admin";
                    obj.User = u;
                    obj.Items = new List<InvoiceItems>();
                    foreach (var Detail in InvoiceData.TRANS_DT)
                    {
                        var product = context.PRODUCTS.Where(x=>x.BARCODE == Detail.BARCODE).SingleOrDefault();
                        InvoiceItems item = new InvoiceItems();
                        item.Discount = Detail.DIS_AMOUNT ?? 0;//----------------------------------------------------
                        item.InvoiceType = 1; // ---------------------------------------------


                        item.ItemCode = Detail.BARCODE;
                        item.ItemName = product.DESCRIPTION;
                        item.PCTCode = product.UAN_NO;
                        item.Quantity = Convert.ToDouble(Detail.UNITS_SOLD);


                        if (InvoiceData.TRANS_TYPE == "2")
                            item.RefUSIN = string.Empty;//-------------------------------------

                        else if (InvoiceData.TRANS_TYPE == "4")
                            item.RefUSIN = string.Empty; // Reference Invoice Number ---------------------------------------

                        item.SaleValue = (Detail.AMOUNT - Detail.DIS_AMOUNT) ?? 0;
                        item.TaxCharged = Detail.GST_AMOUNT;
                        item.TaxRate = Convert.ToSingle(product.GST_PER);
                        item.TotalAmount = Detail.AMOUNT;

                        obj.Items.Add(item);
                    }
                }

                var str = JsonConvert.SerializeObject(obj);

                var enObj = RijndaelManagedEncryption.EncryptRijndael(str);

                string id = JsonConvert.SerializeObject(enObj);
                try
                {
                    InvoiceResponseModel IRM = null;
                    string response = "";

                    string ApiUrl = string.Empty;
                    using (var client = new WebClient())
                    {
                        string baseAddress = "http://fbr.sinkclient.com/";
                        string endPoint = "api/FBR/CreateInvoice";
                        ApiUrl = baseAddress + endPoint;
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        client.Encoding = UTF8Encoding.UTF8;
                        var result = client.UploadString(ApiUrl, id);
                        IRM = JsonConvert.DeserializeObject<InvoiceResponseModel>(result);
                        response = JsonConvert.SerializeObject(IRM);
                    }
                }
                catch (Exception ex)
                {
                    //response = ex.Message;
                }
            }

            
        }

    }
}