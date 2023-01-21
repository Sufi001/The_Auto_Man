using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.ViewModels
{
    public class BranchViewModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Address { get; set; }
        public string CityCode { get; set; }
        [StringLength(30)]
        public string Email { get; set; }
        public string Status { get; set; }
        public IEnumerable<SelectListItem> CityList
        {
            get
            {
                return new DataContext().CITies.Select(x => new SelectListItem { Value = x.CITY_CODE, Text = x.CITY_NAME }).ToList();

            }
        }
        //public IEnumerable<SelectListItem> StatusList
        //{
        //    get
        //    {
        //        return new List<SelectListItem> {
        //                new SelectListItem{ Value = "", Text = "" },
        //                new SelectListItem{ Value = "", Text = "" }
        //                };

        //    }
        //}
    }
}