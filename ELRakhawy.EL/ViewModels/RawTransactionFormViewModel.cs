using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class RawTransactionFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "رقم الإذن التسلسلي")]
        public string TransactionId { get; set; }

        [Display(Name = "رقم الإذن الداخلي")]
        public string? InternalId { get; set; }

        [Display(Name = "رقم الإذن الخارجي")]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الصنف")]
        [Display(Name = "الصنف")]
        public int? RawItemId { get; set; }

        [Display(Name = "اسم الصنف")]
        public string RawItemName { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الكمية")]
        [Range(0.001, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من الصفر")]
        [Display(Name = "الكمية (متر)")]
        public double Quantity { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الوزن")]
        [Range(0.001, double.MaxValue, ErrorMessage = "الوزن يجب أن يكون أكبر من الصفر")]
        [Display(Name = "الوزن (كجم)")]
        public double Weight { get; set; }

        [Required(ErrorMessage = "يرجى إدخال العدد")]
        [Range(0, int.MaxValue, ErrorMessage = "العدد يجب أن يكون صفر أو أكبر")]
        [Display(Name = "العدد")]
        public int Count { get; set; }

      

        [Required(ErrorMessage = "يرجى اختيار الجهة")]
        [Display(Name = "الجهة")]
        public int? StakeholderId { get; set; }

        [Display(Name = "اسم الجهة")]
        public string StakeholderName { get; set; }

        [Required(ErrorMessage = "يرجى اختيار التعبئة")]
        [Display(Name = "التعبئة")]
        public int? PackagingStyleId { get; set; }

        [Display(Name = "اسم التعبئة")]
        public string PackagingStyleName { get; set; }

        [Required(ErrorMessage = "يرجى اختيار التاريخ")]
        [Display(Name = "التاريخ")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "البيان")]
        public string? Comment { get; set; }

        // Form control
        [Required]
        public bool IsInbound { get; set; }

        // Dropdown lists for form
        public List<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stakeholders { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PackagingStyles { get; set; } = new List<SelectListItem>();

        

        // Current balance information (for display)
        public double CurrentQuantityBalance { get; set; }
        public int CurrentCountBalance { get; set; }
    }
}