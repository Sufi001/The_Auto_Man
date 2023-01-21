using Inventory.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Inventory.ViewModels
{
    public class RegisterViewModel
    {
        DataContext db;
        public RegisterViewModel()
        {
            db = new DataContext();
        }
        public string Id { get; set; }

        [Required]

        [Display(Name = "User Name")]
        [Index(IsUnique = true)]
        [MinLength(5, ErrorMessage = "Username must be atleast 6 characters.")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Super User Name")]
        [Index(IsUnique = true)]
        [MinLength(5, ErrorMessage = "Username must be atleast 6 characters.")]
        public string SuperUserName  { get; set; }
        public string ConfirmSuperUserName  { get; set; }
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        [DataType(DataType.Password)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8}$",
       // [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$",
        [RegularExpression (@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,15}$",
         //   ErrorMessage = @"Passwords must have at least one non letter or digit character. Passwords must have at least one lowercase ('a'-'z'). Passwords must have at least one uppercase ('A'-'Z').")]
         ErrorMessage = @"Passwords must have at least one digit character [0,9]. Passwords must have at least one lowercase ('a'-'z'). Passwords must have at least one uppercase ('A'-'Z').")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8}$",
        // [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$",
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,15}$",
      //   ErrorMessage = @"Passwords must have at least one non letter or digit character. Passwords must have at least one lowercase ('a'-'z'). Passwords must have at least one uppercase ('A'-'Z').")]
         ErrorMessage = @"Passwords must have at least one digit character [0,9]. Passwords must have at least one lowercase ('a'-'z'). Passwords must have at least one uppercase ('A'-'Z').")]
        [Display(Name = "Password")]
        public string SuperUserPassword  { get; set; }
        public string ConfirmSuperUserPassword  { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string Status { get; set; }
        public List<SEC_FUNCTIONS> Statuss { get; set; }
        public IEnumerable<LOCATION> LocationList { get; set; }
        public string GetUserCode(int i)
        {
            string s = db.USERS.Max(u => u.USER_ID);
            if (s == "" || s == null)
            {
                return "0000000001";
            }
            string code = CommonFunctions.GenerateCode(s, i);
            return code;
        }
        public List<AllUsers> AllUsersList { get; set; }
        public List<AllUsers> getAlluserslist()
        {
          //  List<AllUsers> Allusers = new List<AllUsers>();
            var data = (from alluser in db.USERS
                        join status in db.SEC_FUNCTIONS
                        on alluser.USER_TYPE equals status.CODE
                        select new AllUsers { UserName = alluser.USER_NAME, Email = alluser.EMAIL, UserStatus = status.DSC,UserCode=alluser.USER_ID }).ToList();

            return data;
        }
    }
   public class AllUsers
    {
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string UserStatus { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
    }
}