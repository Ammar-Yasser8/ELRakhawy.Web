using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FabricDesignViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التصميم مطلوب")]
        [MaxLength(200, ErrorMessage = "التصميم يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "التصميم")]
        public string Design { get; set; }

        [Required(ErrorMessage = "الطراز مطلوب")]
        [Display(Name = "الطراز")]
        public int? StyleId { get; set; }

        [Display(Name = "اسم الطراز")]
        public string StyleName { get; set; }

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string ? Comment { get; set; }

        // Dropdown lists
        public List<SelectListItem> AvailableStyles { get; set; } = new List<SelectListItem>();

        // Helper properties
        public bool IsEdit => Id > 0;
        public string FormTitle => IsEdit ? "تعديل تصميم القماش" : "إضافة تصميم قماش جديد";
        public bool IsPrinted => Design?.Contains("مطبوع") == true;
    }

    public class FabricDesignIndexViewModel
    {
        public List<FabricDesignViewModel> FabricDesigns { get; set; } = new List<FabricDesignViewModel>();
        public List<SelectListItem> StylesFilter { get; set; } = new List<SelectListItem>();
        public string SearchQuery { get; set; }
        public int? SelectedStyleId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Helper properties
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasItems => FabricDesigns.Any();
    }
}
