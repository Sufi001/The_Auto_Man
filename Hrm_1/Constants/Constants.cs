using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Security;

namespace Inventory.Constants
{
    public class Constants
    {
        public static readonly string GSTSwitch = "true";

        public static readonly string ActiveDocument = "A";

        public static readonly string TransferIn = "I";
        public static readonly string TransferOut = "O";

        public static readonly string PurchaseDocument = "G";
        public static readonly string AdjustmentDocument = "S";
        public static readonly string PurchaseReturnDocument = "R";

        public static readonly string SalesDocument = "IN";
        public static readonly string SalesReturnDocument = "IR";

        public static readonly string WastDocument = "W";
        public static readonly string GainDocument = "G";

        public static readonly string Companyname = "The Auto Man";
        public static readonly string CompanyAddress = ConfigurationManager.AppSettings["Address"];
        public static readonly string CompanyPhone = ConfigurationManager.AppSettings["Phone"];
        public static readonly string Title = "The Auto Man";
        public static readonly string CompanyEmail = "main";

        public static readonly string SuperUserName = "Afzaal Ahmad";
        public static readonly string SuperUserPassword = "AfzaalAhmad123";
    }
    public class TransType
    {
        public static readonly string Sales = "2";
        //public static readonly string SalesReturn = "2";
        public static readonly string SalesReturn = "4";
    }
    public class SalesPage
    {
        public static readonly string Index = "Index"; //Inventory Sales Page
        public static readonly string Booking = "Booking"; //Inventory Calender Booking Page
        public static readonly string FaceBook = "FaceBook"; //
        public static readonly string OrderBooking = "OrderBooking"; // Order Booking Page 
        public static readonly string WebStore = "WebStore"; // Online Shiping Page 
    }
    public class PaymentType
    {
        public static readonly string Cash = "1";
        public static readonly string Card = "2";
    }
    public class DocumentStatus
    {
        public static readonly string UnauthorizedDocument = "0";
        public static readonly string Pending = "1";
        public static readonly string Deleted = "2";
        public static readonly string AuthorizedDocument = "3";
        public static readonly string Processing = "4";
        public static readonly string Dispatch = "5";
        public static readonly string Completed = "6";
        public static readonly string Cancelled = "7";
    }
    public class SystemAdminInfo
    {
        public static readonly string SystemAdministratorName = "SoftvalleyAdministrator";
        public static readonly string SystemAdministratorPassword = "Admin3310@Softvalley.com";
    }
    public class CustomerSupplier
    {
        public static readonly string ActiveSupplier = "A";
        public static readonly string InActiveSupplier = "I";
        public static readonly string Supplier = "s";
        public static readonly string Customer = "c";
    }
    public class RoomStatus
    {
        public static readonly string Vacant = "V";
        public static readonly string Occupied = "O";
        public static readonly string UnderService = "S";
    }
}