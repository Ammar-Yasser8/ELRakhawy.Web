using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class RawTransactionViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "رقم الإذن التسلسلي")]
        [MaxLength(50)]
        public string? TransactionId { get; set; }

        [Display(Name = "رقم الإذن الداخلي")]
        [MaxLength(50, ErrorMessage = "رقم الإذن الداخلي يجب ألا يتجاوز 50 حرف")]
        public string? InternalId { get; set; }

        [Display(Name = "رقم الإذن الخارجي")]
        [MaxLength(50, ErrorMessage = "رقم الإذن الخارجي يجب ألا يتجاوز 50 حرف")]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "الصنف مطلوب")]
        [Display(Name = "الصنف")]
        public int RawItemId { get; set; }
        public string? RawItemName { get; set; }
        public string? OriginRawName { get; set; }
        public List<SelectListItem> AvailableItems { get; set; } = new();

        // Inbound meters (وارد متر)
        [Range(0, double.MaxValue, ErrorMessage = "الوارد (متر) يجب أن يكون صفراً أو أكبر")]
        [Display(Name = "وارد (متر)")]
        public decimal InboundMeter { get; set; } = 0m;

        // Inbound kg (وارد كجم)
        [Range(0, double.MaxValue, ErrorMessage = "الوارد (كجم) يجب أن يكون صفراً أو أكبر")]
        [Display(Name = "وارد (كجم)")]
        public decimal InboundKg { get; set; } = 0m;

        // Outbound kg (صادر)
        [Range(0, double.MaxValue, ErrorMessage = "الصادر (كجم) يجب أن يكون صفراً أو أكبر")]
        [Display(Name = "صادر (كجم)")]
        public decimal OutboundKg { get; set; } = 0m;

        [Required(ErrorMessage = "العدد مطلوب")]
        [Range(0, int.MaxValue, ErrorMessage = "العدد يجب أن يكون صفراً أو أكبر")]
        [Display(Name = "عدد")]
        public int Count { get; set; } = 0;

        [Required(ErrorMessage = "نوع الجهة مطلوب")]
        [Display(Name = "نوع الجهة")]
        public int StakeholderTypeId { get; set; }
        public string? StakeholderTypeName { get; set; }
        public List<SelectListItem> StakeholderTypes { get; set; } = new();

        [Required(ErrorMessage = "الجهة مطلوبة")]
        [Display(Name = "الجهة")]
        public int StakeholderId { get; set; }
        public string? StakeholderName { get; set; }
        public List<SelectListItem> Stakeholders { get; set; } = new();

        [Required(ErrorMessage = "التعبئة مطلوبة")]
        [Display(Name = "التعبئة")]
        public int PackagingStyleId { get; set; }
        public string? PackagingStyleName { get; set; }
        public List<SelectListItem> PackagingStyles { get; set; } = new();

        [Display(Name = "رصيد الكمية")]
        public decimal QuantityBalance { get; set; }

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [Display(Name = "التاريخ")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Display(Name = "بيان")]
        [MaxLength(1000)]
        public string? Comment { get; set; }

        // true => form is an inbound entry (إضافة وارد), false => outbound (إضافة صادر)
        public bool IsInbound { get; set; } = true;

        // Additional UI properties
        [Display(Name = "الرصيد الحالي")]
        public decimal CurrentBalance { get; set; }

        [Display(Name = "رصيد العدد الحالي")]
        public int CurrentCountBalance { get; set; }

        // Conditional validation: require inbound values for inbound form, outbound values for outbound form.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsInbound)
            {
                // For inbound form at least one of InboundMeter or InboundKg must be > 0, and Count is present (Count already Required)
                if (InboundMeter <= 0m && InboundKg <= 0m)
                {
                    yield return new ValidationResult(
                        "في حالة إضافة وارد يجب إدخال قيمة وارد (متر أو كجم) أكبر من صفر.",
                        new[] { nameof(InboundMeter), nameof(InboundKg) });
                }
            }
            else
            {
                // For outbound form: OutboundKg > 0 or Count > 0 (Count is required by model but we allow zero, so ensure some outbound measure)
                if (OutboundKg <= 0m && Count <= 0)
                {
                    yield return new ValidationResult(
                        "في حالة إضافة صادر يجب إدخال قيمة صادر (كجم) أو عدد أكبر من صفر.",
                        new[] { nameof(OutboundKg), nameof(Count) });
                }
            }

            // Ensure PackagingStyles selection is valid (client-side should filter but server-side check)
            if (PackagingStyleId <= 0)
            {
                yield return new ValidationResult(
                    "التعبئة مطلوبة ويجب اختيار أسلوب تعبئة صالح.",
                    new[] { nameof(PackagingStyleId) });
            }

            // Ensure RawItemId selected
            if (RawItemId <= 0)
            {
                yield return new ValidationResult(
                    "الصنف مطلوب ويجب اختياره من القائمة.",
                    new[] { nameof(RawItemId) });
            }

            // Stakeholder checks
            if (StakeholderTypeId <= 0)
            {
                yield return new ValidationResult(
                    "نوع الجهة مطلوب ويجب اختياره.",
                    new[] { nameof(StakeholderTypeId) });
            }

            if (StakeholderId <= 0)
            {
                yield return new ValidationResult(
                    "الجهة مطلوبة ويجب اختيارها.",
                    new[] { nameof(StakeholderId) });
            }
        }
    }
}
