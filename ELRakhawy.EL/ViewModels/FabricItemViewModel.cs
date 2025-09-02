using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FabricItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الصنف يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الصنف")]
        public string Item { get; set; }

        [Required(ErrorMessage = "الخام المكون مطلوب")]
        [Display(Name = "الخام المكون")]
        public int? OriginRawId { get; set; }

        [Display(Name = "اسم الخام المكون")]
        public string OriginRawName { get; set; }

        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string Comment { get; set; }

        // Dropdown lists
        public List<SelectListItem> AvailableRawItems { get; set; } = new List<SelectListItem>();

        // Helper properties
        public string StatusText => Status ? "نشط" : "غير نشط";
        public string StatusClass => Status ? "success" : "secondary";
        public string StatusIcon => Status ? "fa-check-circle" : "fa-times-circle";
        public bool IsEdit => Id > 0;
        public string FormTitle => IsEdit ? "تعديل صنف قماش" : "إضافة صنف قماش جديد";
    }

    public class FabricItemIndexViewModel
    {
        public List<FabricItemViewModel> FabricItems { get; set; } = new List<FabricItemViewModel>();
        public List<SelectListItem> RawItemsFilter { get; set; } = new List<SelectListItem>();

        // Filter properties
        public string SearchQuery { get; set; }
        public int? SelectedRawItemId { get; set; }
        public string StatusFilter { get; set; } = "All"; // "All", "Active", "Inactive"

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Statistics
        public int ActiveItemsCount { get; set; }
        public int InactiveItemsCount { get; set; }
        public int TotalRawItemsUsed { get; set; }

        // Helper properties
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasItems => FabricItems.Any();
    
    }
}
