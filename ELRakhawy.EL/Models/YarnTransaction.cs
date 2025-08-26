using System.ComponentModel.DataAnnotations;

namespace ELRakhawy.EL.Models
{
    public class YarnTransaction
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الإذن التسلسلي مطلوب")]
        [MaxLength(50, ErrorMessage = "رقم الإذن التسلسلي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن التسلسلي")]
        public string TransactionId { get; set; }

        [MaxLength(50, ErrorMessage = "رقم الإذن الداخلي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن الداخلي")]
        public string? InternalId { get; set; }

        [MaxLength(50, ErrorMessage = "رقم الإذن الخارجي يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "رقم الإذن الخارجي")]
        public string? ExternalId { get; set; }

        [Required(ErrorMessage = "الصنف مطلوب")]
        [Display(Name = "الصنف")]
        public int YarnItemId { get; set; }
        public virtual YarnItem YarnItem { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون كمية الوارد صفر أو أكبر")]
        [Display(Name = "وارد")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Inbound { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "يجب أن تكون كمية الصادر صفر أو أكبر")]
        [Display(Name = "صادر")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Outbound { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون العدد صفر أو أكبر")]
        [Display(Name = "عدد")]
        public int Count { get; set; } = 0;

        [Display(Name = "نوع الجهة")]
        public int? StakeholderTypeId { get; set; }
        public virtual StakeholderType? StakeholderType { get; set; }

        [Required(ErrorMessage = "الجهة مطلوبة")]
        [Display(Name = "الجهة")]
        public int StakeholderId { get; set; }
        public virtual StakeholdersInfo Stakeholder { get; set; }

        [Required(ErrorMessage = "التعبئة مطلوبة")]
        [Display(Name = "التعبئة")]
        public int PackagingStyleId { get; set; }
        public virtual PackagingStyles PackagingStyle { get; set; }

        // navigation property for Manufacturers
        [Display(Name = "الشركة المصنعة")]
        public int? ManufacturerId { get; set; } // Optional
        public virtual Manufacturers? Manufacturer { get; set; }


        [Display(Name = "رصيد الكمية")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal QuantityBalance { get; set; }

        [Display(Name = "رصيد العدد")]
        public int CountBalance { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [Display(Name = "التاريخ")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Date { get; set; }

        [Display(Name = "بيان")]
        public string? Comment { get; set; }

        // Helper method to calculate balances
        public void UpdateBalances(decimal previousQuantityBalance, int previousCountBalance)
        {
            QuantityBalance = previousQuantityBalance + Inbound - Outbound;
            CountBalance = previousCountBalance +
                (Inbound > 0 ? Count : 0) -
                (Outbound > 0 ? Count : 0);
        }
    }
}