using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.EL.ViewModels
{
    public class YarnItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الصنف يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الصنف")]
        public string Item { get; set; }

        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; }

        [Display(Name = "بيان")]
        public string? Comment { get; set; }

        [Display(Name = "الرصيد الحالي")]
        public decimal CurrentBalance { get; set; }

        [Display(Name = "رصيد العدد الحالي")]
        public int CurrentCountBalance { get; set; }

        // Updated for many-to-many
        [Display(Name = "الشركات المصنعة")]
        public List<string> ManufacturerNames { get; set; } = new List<string>();

        [Display(Name = "الشركات المصنعة")]
        public List<int> ManufacturerIds { get; set; } = new List<int>();

        [Display(Name = "الغزل المكون")]
        public string? OriginYarnName { get; set; }

        [Display(Name = "الغزل المكون")]
        public int? OriginYarnId { get; set; }

        // For dropdown lists
        public IEnumerable<SelectListItem>? Manufacturers { get; set; }
        public IEnumerable<SelectListItem>? OriginYarns { get; set; }
    }
}
