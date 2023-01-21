using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory.ViewModels
{
    public class LoginViewModel
    {
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Location { get; set; }
        [Display(Name = "remember me")]
        public bool remember { get; set; }
    }
}