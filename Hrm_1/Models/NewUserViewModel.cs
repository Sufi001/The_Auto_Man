using Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.Models
{
    public class NewUserViewModel
    {
        public string UserId { get; set; }
        public string UserCode { get; set; }
        [Required(ErrorMessage = "User Name field is Required")]
        [MaxLength(40, ErrorMessage = "Enter 40 or Less than 40 Characters")]
        [RegularExpression(@"^([a-zA-Z]+ +)*[a-zA-Z ]+$", ErrorMessage = "Enter Name Only in Alphabets")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password field is Required")]
        //[MinLength(8, ErrorMessage = "Password's Minimum Length is 8")]
        //[MaxLength(15, ErrorMessage = "Password's Maximum Length is 15")]
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Location field is Required")]
        [MaxLength(100, ErrorMessage = "Location's Maximum Length is 100 Characters")]
        public string Location { get; set; }
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email is not valid")]
        [MaxLength(50, ErrorMessage = "Enter less than 50 Characters")]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        public string Email { get; set; }
        [Display(Name = "Remember Me")]
        public bool Remember { get; set; }
        public IEnumerable<LOCATION> LocationList { get; set; }
        public List<PERMISSION> AllPermissions { get; set; }
        public List<string> SelectedPermissions { get; set; }
    }
}