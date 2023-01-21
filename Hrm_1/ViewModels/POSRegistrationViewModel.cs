using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class POSRegistrationViewModel
    {
        [Required]
        [StringLength(100)]
        public string POSID { get; set; }
        [Required]
        [StringLength(100)]

        public string NTN { get; set; }
        [Required]
        [StringLength(100)]

        public string BusinessName { get; set; }
        [Required]
        [StringLength(100)]

        public string BranchName { get; set; }
        [Required]
        [StringLength(100)]

        public string BusinessAddress { get; set; }
        [Required]
        [StringLength(100)]

        public string City { get; set; }
        [Required]
        [StringLength(100)]

        public string IPAddress { get; set; }
        [Required]
        [StringLength(100)]

        public string Latitude { get; set; }
        [Required]
        [StringLength(100)]

        public string Longitude { get; set; }
        [Required]
        [StringLength(100)]

        public string Mode { get; set; }
        [Required]
        [StringLength(100)]

        public string UserId { get; set; }
        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        [StringLength(4)]
        public string TillNo { get; set; }
    }
}