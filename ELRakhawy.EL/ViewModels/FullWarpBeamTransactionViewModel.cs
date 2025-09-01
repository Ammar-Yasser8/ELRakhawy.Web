using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FullWarpBeamTransactionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "رقم الاذن التسلسلي")]
        public string TransactionId { get; set; }

        [Display(Name = "رقم الاذن الداخلي")]
        public string InternalId { get; set; }

        [Display(Name = "رقم الاذن الخارجي")]
        public string ExternalId { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الصنف")]
        [Display(Name = "الصنف")]
        public int? FullWarpBeamItemId { get; set; }

        [Display(Name = "اسم الصنف")]
        public string FullWarpBeamItemName { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الكمية")]
        [Range(0.001, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من الصفر")]
        [Display(Name = "الكمية")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "يرجى إدخال الطول")]
        [Range(0, int.MaxValue, ErrorMessage = "الطول يجب أن يكون صفر أو أكبر")]
        [Display(Name = "الطول")]
        public int Length { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الجهة")]
        [Display(Name = "الجهة")]
        public int? StakeholderId { get; set; }

        [Display(Name = "اسم الجهة")]
        public string StakeholderName { get; set; }

        [Display(Name = "رصيد الكمية")]
        public decimal QuantityBalance { get; set; }

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; }

        [Required(ErrorMessage = "يرجى اختيار التاريخ")]
        [Display(Name = "التاريخ")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "بيان")]
        public string ? Comment { get; set; }

        [Required]
        public bool IsInbound { get; set; }

        // Dropdown lists
        public List<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stakeholders { get; set; } = new List<SelectListItem>();

    }
}

