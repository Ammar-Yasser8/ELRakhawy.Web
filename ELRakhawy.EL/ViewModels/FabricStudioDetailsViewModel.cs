using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    // Add these to your ViewModels namespace

    public class FabricStudioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الصنف مطلوب")]
        [Display(Name = "الصنف")]
        public int? ItemId { get; set; }

        [Display(Name = "اسم الصنف")]
        public string ItemName { get; set; }

        [Display(Name = "اللون")]
        public int? ColorId { get; set; }

        [Display(Name = "اسم اللون")]
        public string ColorName { get; set; }

        [Display(Name = "التصميم")]
        public int? DesignId { get; set; }

        [Display(Name = "اسم التصميم")]
        public string DesignName { get; set; }

        [Display(Name = "البوستر")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "مسار البوستر")]
        public string ImagePath { get; set; }

        [Display(Name = "الصورة")]
        public IFormFile? WatermarkedImageFile { get; set; }

        [Display(Name = "مسار الصورة")]
        public string WatermarkedImagePath { get; set; }

        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        // Dropdown lists
        public List<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableColors { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableDesigns { get; set; } = new List<SelectListItem>();

        // Helper properties
        public bool IsEdit => Id > 0;
        public string FormTitle => IsEdit ? "تعديل استوديو القماش" : "إضافة استوديو قماش جديد";
        public string StatusText => Status ? "نشط" : "غير نشط";
        public string StatusClass => Status ? "success" : "secondary";
        public string StatusIcon => Status ? "fa-check-circle" : "fa-times-circle";
        public string StudioType => ColorId.HasValue ? "لون" : (DesignId.HasValue ? "تصميم" : "غير محدد");
        public bool HasDesign => DesignId.HasValue;
        public bool HasColor => ColorId.HasValue;
        public bool RequiresImage => DesignId.HasValue;
    }

    public class FabricStudioIndexViewModel
    {
        public List<FabricStudioViewModel> FabricStudios { get; set; } = new List<FabricStudioViewModel>();
        public List<SelectListItem> ItemsFilter { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ColorsFilter { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DesignsFilter { get; set; } = new List<SelectListItem>();

        // Filter properties
        public string SearchQuery { get; set; }
        public int? SelectedItemId { get; set; }
        public int? SelectedColorId { get; set; }
        public int? SelectedDesignId { get; set; }
        public string StatusFilter { get; set; } = "All"; // "All", "Active", "Inactive"
        public string TypeFilter { get; set; } = "All"; // "All", "Color", "Design"

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Statistics
        public int ActiveItemsCount { get; set; }
        public int InactiveItemsCount { get; set; }
        public int ColorBasedCount { get; set; }
        public int DesignBasedCount { get; set; }

        // Helper properties
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasItems => FabricStudios.Any();
        public double ActivePercentage => TotalItems > 0 ? (double)ActiveItemsCount / TotalItems * 100 : 0;
        public double ColorBasedPercentage => TotalItems > 0 ? (double)ColorBasedCount / TotalItems * 100 : 0;
        public double DesignBasedPercentage => TotalItems > 0 ? (double)DesignBasedCount / TotalItems * 100 : 0;
    }

    public class FabricStudioDetailsViewModel
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string OriginRawName { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; }
        public string ColorStyleName { get; set; }
        public int? DesignId { get; set; }
        public string DesignName { get; set; }
        public string DesignStyleName { get; set; }
        public string ImagePath { get; set; }
        public string WatermarkedImagePath { get; set; }
        public bool Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string StudioType { get; set; }
        public string DisplayName { get; set; }

        // Helper properties
        public string StatusText => Status ? "نشط" : "غير نشط";
        public string StatusClass => Status ? "success" : "secondary";
        public string StatusIcon => Status ? "fa-check-circle" : "fa-times-circle";
        public bool HasColor => ColorId.HasValue;
        public bool HasDesign => DesignId.HasValue;
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
        public bool HasWatermarkedImage => !string.IsNullOrEmpty(WatermarkedImagePath);
        public string TypeIcon => StudioType == "لون" ? "fa-palette" : "fa-paint-brush";
        public string TypeClass => StudioType == "لون" ? "primary" : "info";
        public string FormattedCreatedAt => CreatedAt.ToString("yyyy-MM-dd HH:mm");
        public string FormattedUpdatedAt => UpdatedAt.ToString("yyyy-MM-dd HH:mm");
    }

    public class FabricStudioExtractViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string OriginRawName { get; set; }
        public List<FabricStudioSummaryDto> Studios { get; set; } = new List<FabricStudioSummaryDto>();

        // Helper properties
        public bool HasStudios => Studios.Any();
        public int TotalStudios => Studios.Count;
        public int ColorsCount => Studios.Count(s => !string.IsNullOrEmpty(s.ColorName));
        public int DesignsCount => Studios.Count(s => !string.IsNullOrEmpty(s.DesignName));
        public bool HasColors => ColorsCount > 0;
        public bool HasDesigns => DesignsCount > 0;
        public string SummaryText => $"إجمالي {TotalStudios} استوديو ({ColorsCount} لون، {DesignsCount} تصميم)";
    }

    public class FabricStudioSummaryDto
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public string DesignName { get; set; }
        public string ImagePath { get; set; }
        public string WatermarkedImagePath { get; set; }
        public string StudioType { get; set; }
        public string DisplayName { get; set; }

        // Helper properties
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
        public bool HasWatermarkedImage => !string.IsNullOrEmpty(WatermarkedImagePath);
        public string TypeIcon => StudioType == "لون" ? "fa-palette" : "fa-paint-brush";
        public string TypeClass => StudioType == "لون" ? "primary" : "info";
        public bool IsColorBased => !string.IsNullOrEmpty(ColorName);
        public bool IsDesignBased => !string.IsNullOrEmpty(DesignName);
        public string PrimaryName => IsColorBased ? ColorName : DesignName;
    }

    public class FabricStudioFilterViewModel
    {
        public List<SelectListItem> Items { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Colors { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Designs { get; set; } = new List<SelectListItem>();

        public int? SelectedItemId { get; set; }
        public int? SelectedColorId { get; set; }
        public int? SelectedDesignId { get; set; }
        public string StatusFilter { get; set; } = "All";
        public string TypeFilter { get; set; } = "All";
        public string SearchQuery { get; set; }
    }

    public class FabricStudioValidationViewModel
    {
        public int? ItemId { get; set; }
        public int? ColorId { get; set; }
        public int? DesignId { get; set; }
        public int CurrentStudioId { get; set; }

        public bool IsValidConfiguration => (ColorId.HasValue && !DesignId.HasValue) ||
                                          (!ColorId.HasValue && DesignId.HasValue);

        public string ValidationMessage => IsValidConfiguration ?
            "التكوين صحيح" :
            "يجب اختيار إما اللون أو التصميم، وليس كلاهما";
    }
}
