using System.ComponentModel.DataAnnotations;

namespace ELRakhawy.EL.Models
{
    public class YarnTransaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الإذن التسلسلي مطلوب")]
        [MaxLength(50, ErrorMessage = "رقم الإذن التسلسلي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن التسلسلي")]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "رقم الإذن الداخلي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن الداخلي")]
        public string? InternalId { get; set; }

        [MaxLength(50, ErrorMessage = "رقم الإذن الخارجي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن الخارجي")]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "الصنف مطلوب")]
        [Display(Name = "الصنف")]
        public int YarnItemId { get; set; }
        public virtual YarnItem? YarnItem { get; set; }

        // Quantity fields (كيلو) — independent
        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون كمية الوارد صفر أو أكبر")]
        [Display(Name = "وارد (كجم)")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Inbound { get; set; } = 0m;

        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون كمية الصادر صفر أو أكبر")]
        [Display(Name = "صادر (كجم)")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Outbound { get; set; } = 0m;

        // Count fields (units/boxes) — independent
        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون العدد صفر أو أكبر")]
        [Display(Name = "عدد (وحدات)")]
        public int Count { get; set; } = 0;

        [Display(Name = "نوع الجهة")]
        public int? StakeholderTypeId { get; set; }
        public virtual StakeholderType? StakeholderType { get; set; }

        [Required(ErrorMessage = "الجهة مطلوبة")]
        [Display(Name = "الجهة")]
        public int StakeholderId { get; set; }
        public virtual StakeholdersInfo? Stakeholder { get; set; }

        [Required(ErrorMessage = "التعبئة مطلوبة")]
        [Display(Name = "التعبئة")]
        public int PackagingStyleId { get; set; }
        public virtual PackagingStyles? PackagingStyle { get; set; }

        // Optional manufacturer
        [Display(Name = "الشركة المصنعة")]
        public int? ManufacturerId { get; set; }
        public virtual Manufacturers? Manufacturer { get; set; }

        // Balances — maintained independently
        [Display(Name = "رصيد الكمية (كجم)")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal QuantityBalance { get; set; } = 0m;

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; } = 0;

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [Display(Name = "التاريخ")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "بيان")]
        public string? Comment { get; set; }

        /// <summary>
        /// Update balances independently:
        /// QuantityBalance = previousQuantityBalance + Inbound - Outbound
        /// CountBalance    = previousCountBalance + (Inbound>0 ? Count : 0) - (Outbound>0 ? Count : 0)
        /// </summary>
        /// <param name="previousQuantityBalance">previous quantity balance (kg)</param>
        /// <param name="previousCountBalance">previous count balance (units)</param>
        public void UpdateBalances(decimal previousQuantityBalance, int previousCountBalance)
        {
            QuantityBalance = previousQuantityBalance + Inbound - Outbound;

            // Count is applied only when Count is provided with an inbound or outbound
            CountBalance = previousCountBalance
                + (Inbound > 0 ? Count : 0)
                - (Outbound > 0 ? Count : 0);
        }
    }
}