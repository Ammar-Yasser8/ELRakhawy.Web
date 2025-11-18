using ELRakhawy.EL.Models;
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
        [MaxLength(50, ErrorMessage = "رقم الإذن الداخلي يجب ألا يتجاوز 50 حرف")]
        public string? InternalId { get; set; }

        [Display(Name = "رقم الإذن الخارجي")]
        [MaxLength(50, ErrorMessage = "رقم الإذن الخارجي يجب ألا يتجاوز 50 حرف")]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "صنف الغزل مطلوب")]
        [Display(Name = "صنف الغزل")]
        public int YarnItemId { get; set; }

        [Display(Name = "اسم صنف الغزل")]
        public string YarnItemName { get; set; } = "";

        [Display(Name = "الصنف الأصلي")]
        public string OriginYarnName { get; set; } = "";

        public List<SelectListItem> AvailableItems { get; set; } = new();

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون الكمية صفر أو أكبر")]
        [Display(Name = "الكمية")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "العدد مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون العدد رقماً صحيحاً موجباً")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "يجب إدخال عدد صحيح بدون كسور")]
        [Display(Name = "العدد")]
        public int Count { get; set; }

        [Required(ErrorMessage = "نوع الجهة مطلوب")]
        [Display(Name = "نوع الجهة")]
        public int StakeholderTypeId { get; set; }

        [Display(Name = "اسم نوع الجهة")]
        public string StakeholderTypeName { get; set; } = "";

        public List<SelectListItem> StakeholderTypes { get; set; } = new();

        [Required(ErrorMessage = "الجهة مطلوبة")]
        [Display(Name = "الجهة")]
        public int StakeholderId { get; set; }

        [Display(Name = "اسم الجهة")]
        public string StakeholderName { get; set; } = "";

        public List<SelectListItem> Stakeholders { get; set; } = new();

        [Required(ErrorMessage = "التعبئة مطلوبة")]
        [Display(Name = "التعبئة")]
        public int PackagingStyleId { get; set; }

        [Display(Name = "اسم التعبئة")]
        public string PackagingStyleName { get; set; } = "";

        public List<SelectListItem> PackagingStyles { get; set; } = new();

        [Display(Name = "رصيد الكمية")]
        public decimal QuantityBalance { get; set; }

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [Display(Name = "التاريخ")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Display(Name = "بيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        [Display(Name = "نوع المعاملة")]
        public bool IsInbound { get; set; }

        // ✅ NEW: Edit-specific properties for proper balance calculations
        [Display(Name = "الرصيد الأصلي للكمية")]
        public decimal OriginalQuantityBalance { get; set; }

        [Display(Name = "الرصيد الأصلي للعدد")]
        public int OriginalCountBalance { get; set; }

        [Display(Name = "الكمية الأصلية")]
        public decimal OriginalQuantity { get; set; }

        [Display(Name = "العدد الأصلي")]
        public int OriginalCount { get; set; }

        [Display(Name = "نوع المعاملة الأصلية")]
        public bool OriginalIsInbound { get; set; }

        [Display(Name = "صنف الغزل الأصلي")]
        public int OriginalYarnItemId { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "منشئ بواسطة")]
        public string CreatedBy { get; set; } = "Ammar-Yasser8";

        [Display(Name = "تاريخ التعديل")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "معدل بواسطة")]
        public string? ModifiedBy { get; set; }

        // ✅ Current balance properties (for display purposes)
        [Display(Name = "الرصيد الحالي")]
        public decimal CurrentBalance { get; set; }

        [Display(Name = "رصيد العدد الحالي")]
        public int CurrentCountBalance { get; set; }

        // ✅ Helper properties for better UX
        [Display(Name = "وضع التعديل")]
        public bool IsEditMode => Id > 0;

        [Display(Name = "عنوان النموذج")]
        public string FormTitle
        {
            get
            {
                if (IsEditMode)
                    return IsInbound ? "تعديل وارد غزل" : "تعديل صادر غزل";
                else
                    return IsInbound ? "إضافة وارد غزل" : "إضافة صادر غزل";
            }
        }

        [Display(Name = "وصف المعاملة")]
        public string TransactionTypeDescription => IsInbound ? "وارد" : "صادر";

        [Display(Name = "أيقونة النوع")]
        public string TransactionIcon => IsInbound ? "fa-arrow-down" : "fa-arrow-up";

        [Display(Name = "لون النوع")]
        public string TransactionColor => IsInbound ? "success" : "warning";

        // ✅ Balance difference properties for edit mode display
        [Display(Name = "فرق الكمية")]
        public decimal QuantityDifference => IsEditMode ? Quantity - OriginalQuantity : 0;

        [Display(Name = "فرق العدد")]
        public int CountDifference => IsEditMode ? Count - OriginalCount : 0;

        [Display(Name = "تغيير نوع المعاملة")]
        public bool IsTransactionTypeChanged => IsEditMode && IsInbound != OriginalIsInbound;

        [Display(Name = "تغيير صنف الغزل")]
        public bool IsYarnItemChanged => IsEditMode && YarnItemId != OriginalYarnItemId;

        // ✅ Validation helper properties
        [Display(Name = "الرصيد المتوقع للكمية")]
        public decimal ExpectedQuantityBalance
        {
            get
            {
                if (!IsEditMode) return CurrentBalance;

                var previousBalance = OriginalQuantityBalance - (OriginalIsInbound ? OriginalQuantity : -OriginalQuantity);
                return previousBalance + (IsInbound ? Quantity : -Quantity);
            }
        }

        [Display(Name = "الرصيد المتوقع للعدد")]
        public int ExpectedCountBalance
        {
            get
            {
                if (!IsEditMode) return CurrentCountBalance;

                var previousBalance = OriginalCountBalance - (OriginalIsInbound ? OriginalCount : -OriginalCount);
                return previousBalance + (IsInbound ? Count : -Count);
            }
        }

        // ✅ Display helper methods
        public string GetQuantityChangeDescription()
        {
            if (!IsEditMode) return "";

            var diff = QuantityDifference;
            if (diff == 0) return "لا يوجد تغيير";

            return diff > 0 ? $"+{diff:N3} كجم" : $"{diff:N3} كجم";
        }

        public string GetCountChangeDescription()
        {
            if (!IsEditMode) return "";

            var diff = CountDifference;
            if (diff == 0) return "لا يوجد تغيير";

            return diff > 0 ? $"+{diff} وحدة" : $"{diff} وحدة";
        }

        public string GetBalanceChangeDescription()
        {
            if (!IsEditMode) return "";

            var qtyChange = ExpectedQuantityBalance - OriginalQuantityBalance;
            var countChange = ExpectedCountBalance - OriginalCountBalance;

            var parts = new List<string>();

            if (qtyChange != 0)
                parts.Add($"الكمية: {(qtyChange > 0 ? "+" : "")}{qtyChange:N3} كجم");

            if (countChange != 0)
                parts.Add($"العدد: {(countChange > 0 ? "+" : "")}{countChange} وحدة");

            return parts.Any() ? string.Join(" | ", parts) : "لا يوجد تغيير في الرصيد";
        }

        // ✅ Audit trail properties
        [Display(Name = "سجل التغييرات")]
        public List<string> ChangeLog { get; set; } = new();

        public void AddToChangeLog(string change)
        {
            ChangeLog.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {change}");
        }

        // ✅ Validation summary for complex edit scenarios
        public List<string> GetEditWarnings()
        {
            var warnings = new List<string>();

            if (!IsEditMode) return warnings;

            if (IsTransactionTypeChanged)
                warnings.Add("تم تغيير نوع المعاملة من " +
                    (OriginalIsInbound ? "وارد" : "صادر") + " إلى " +
                    (IsInbound ? "وارد" : "صادر"));

            if (IsYarnItemChanged)
                warnings.Add("تم تغيير صنف الغزل - تحقق من صحة الرصيد");

            if (ExpectedQuantityBalance < 0)
                warnings.Add($"سينتج عن هذا التعديل رصيد كمية سالب: {ExpectedQuantityBalance:N3} كجم");

            if (ExpectedCountBalance < 0)
                warnings.Add($"سينتج عن هذا التعديل رصيد عدد سالب: {ExpectedCountBalance} وحدة");

            return warnings;
        }

        // ✅ Method to populate original values from existing transaction
        public void SetOriginalValues(YarnTransaction transaction)
        {
            OriginalQuantityBalance = transaction.QuantityBalance;
            OriginalCountBalance = transaction.CountBalance;
            OriginalQuantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound;
            OriginalCount = transaction.Count;
            OriginalIsInbound = transaction.Inbound > 0;
            OriginalYarnItemId = transaction.YarnItemId;
            CreatedDate = transaction.Date;

            // If it's an edit, set modification tracking
            if (Id > 0)
            {
                ModifiedDate = DateTime.UtcNow;
                ModifiedBy = "Ammar-Yasser8";
            }
        }
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
