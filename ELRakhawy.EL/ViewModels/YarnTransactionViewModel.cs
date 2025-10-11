using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class YarnTransactionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "رقم الإذن التسلسلي")]
        public string TransactionId { get; set; }

        [Display(Name = "رقم الإذن الداخلي")]
        [MaxLength(50)]
        public string? InternalId { get; set; }

        [Display(Name = "رقم الإذن الخارجي")]
        [MaxLength(50)]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "صنف الغزل مطلوب")]
        [Display(Name = "صنف الغزل")]
        public int YarnItemId { get; set; }
        public string YarnItemName { get; set; }
        public string OriginYarnName { get; set; }
        public List<SelectListItem> AvailableItems { get; set; } = new();

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون الكمية صفر أو أكبر")]
        [Display(Name = "الكمية")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "العدد مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون العدد رقماً صحيحاً موجباً")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "يجب إدخال عدد صحيح بدون كسور")]
        public int Count { get; set; }

        [Required(ErrorMessage = "نوع الجهة مطلوب")]
        [Display(Name = "نوع الجهة")]
        public int StakeholderTypeId { get; set; }
        public string StakeholderTypeName { get; set; }
        public List<SelectListItem> StakeholderTypes { get; set; } = new();

        [Required(ErrorMessage = "الجهة مطلوبة")]
        [Display(Name = "الجهة")]
        public int StakeholderId { get; set; }
        public string StakeholderName { get; set; }
        public List<SelectListItem> Stakeholders { get; set; } = new();

        [Required(ErrorMessage = "التعبئة مطلوبة")]
        [Display(Name = "التعبئة")]
        public int PackagingStyleId { get; set; }
        public string PackagingStyleName { get; set; }
        public List<SelectListItem> PackagingStyles { get; set; } = new();

        [Display(Name = "رصيد الكمية")]
        public decimal QuantityBalance { get; set; }

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [Display(Name = "التاريخ")]
        public DateTime Date { get; set; }

        [Display(Name = "بيان")]
        public string? Comment { get; set; }

        public bool IsInbound { get; set; }

        // Additional properties for yarn-specific features
        [Display(Name = "الرصيد الحالي")]
        public decimal CurrentBalance { get; set; }

        [Display(Name = "رصيد العدد الحالي")]
        public int CurrentCountBalance { get; set; }

    }
    public class YarnTransactionSearchViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionId { get; set; }
        public string? InternalId { get; set; }  // ✅ New
        public string? ExternalId { get; set; }   // ✅ New
        public string? TransactionType { get; set; }
        public int? YarnItemId { get; set; }
        public int? StakeholderTypeId { get; set; }
        public int? StakeholderId { get; set; }
        public int? PackagingStyleId { get; set; }  // ✅ New
        public decimal? MinQuantity { get; set; }   // ✅ New
        public decimal? MaxQuantity { get; set; }   // ✅ New
        public int? MinCount { get; set; }          // ✅ New  
        public int? MaxCount { get; set; }          // ✅ New
        public string? CommentSearch { get; set; }  // ✅ New

        // Existing properties
        public List<SelectListItem> AvailableItems { get; set; } = new();
        public List<SelectListItem> StakeholderTypes { get; set; } = new();
        public List<SelectListItem> YarnStakeholders { get; set; } = new();
        public List<SelectListItem> PackagingStyles { get; set; } = new(); // ✅ New
        public List<YarnTransactionViewModel> Results { get; set; } = new();

        // Statistics
        public int TotalTransactions { get; set; }
        public decimal TotalInbound { get; set; }
        public decimal TotalOutbound { get; set; }
        public decimal NetBalance { get; set; }
    }
}
