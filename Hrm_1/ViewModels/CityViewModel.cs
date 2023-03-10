using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Inventory.ViewModels
{
    public class CityViewModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}