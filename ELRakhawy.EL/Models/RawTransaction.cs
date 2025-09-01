using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    //public class RawTransaction
    //{
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "رقم الإذن التسلسلي مطلوب")]
    //    [MaxLength(50, ErrorMessage = "رقم الإذن التسلسلي يجب ألا يتجاوز 50 حرف")]
    //    [Display(Name = "رقم الإذن التسلسلي")]
    //    public required string TransactionId { get; set; }

    //    [MaxLength(50, ErrorMessage = "رقم الإذن الداخلي يجب ألا يتجاوز 50 حرف")]
    //    [Display(Name = "رقم الإذن الداخلي")]
    //    public string? InternalId { get; set; }

    //    [MaxLength(50, ErrorMessage = "رقم الإذن الخارجي يجب ألا يتجاوز 50 حرف")]
    //    [Display(Name = "رقم الإذن الخارجي")]
    //    public string? ExternalId { get; set; }

    //    [Required(ErrorMessage = "الصنف مطلوب")]
    //    [Display(Name = "الصنف")]
    //    public int RawItemId { get; set; }
    //    [ForeignKey("RawItemId")]
    //    public required virtual RawItem RawItem { get; set; }

    //    [Range(0, double.MaxValue, ErrorMessage = "الوارد (متر) يجب أن يكون صفراً أو أكثر")]
    //    [Display(Name = "وارد (متر)")]
    //    public double InboundMeter { get; set; }

    //    [Range(0, double.MaxValue, ErrorMessage = "الوارد (كجم) يجب أن يكون صفراً أو أكثر")]
    //    [Display(Name = "وارد (كجم)")]
    //    public double InboundKg { get; set; }

    //    [Range(0, double.MaxValue, ErrorMessage = "الصادر يجب أن يكون صفراً أو أكثر")]
    //    [Display(Name = "صادر")]
    //    public double Outbound { get; set; }

    //    [Range(0, int.MaxValue, ErrorMessage = "العدد يجب أن يكون صفراً أو أكثر")]
    //    [Display(Name = "عدد")]
    //    public int Count { get; set; }

    //    [Required(ErrorMessage = "نوع الجهة مطلوب")]
    //    [Display(Name = "نوع الجهة")]
    //    public int StakeholderTypeId { get; set; }
    //    [ForeignKey("StakeholderTypeId")]
    //    public virtual StakeholderType StakeholderType { get; set; }

    //    [Required(ErrorMessage = "الجهة مطلوبة")]
    //    [Display(Name = "الجهة")]
    //    public int StakeholderId { get; set; }
    //    [ForeignKey("StakeholderId")]
    //    public virtual StakeholdersInfo Stakeholder { get; set; }

    //    [Required(ErrorMessage = "التعبئة مطلوبة")]
    //    [Display(Name = "التعبئة")]
    //    public int PackagingStyleId { get; set; }
    //    [ForeignKey("PackagingStyleId")]
    //    public virtual PackagingStyles PackagingStyle { get; set; }

    //    [Display(Name = "رصيد الكمية")]
    //    [NotMapped]
    //    public double QuantityBalance { get; set; }

    //    [Display(Name = "رصيد العدد")]
    //    [NotMapped]
    //    public int CountBalance { get; set; }

    //    [Required(ErrorMessage = "التاريخ مطلوب")]
    //    [Display(Name = "التاريخ")]
    //    public DateTime Date { get; set; }

    //    [Display(Name = "بيان")]
    //    [MaxLength(1000)]
    //    public string? Comment { get; set; }


    //    public void UpdateBalances(double previousQuantityBalance, int previousCountBalance)
    //    {
    //        QuantityBalance = previousQuantityBalance + InboundMeter + InboundKg - Outbound;
    //        CountBalance = previousCountBalance +
    //            ((InboundMeter > 0 || InboundKg > 0) ? Count : 0) -
    //            (Outbound > 0 ? Count : 0);
    //    }
    //}

}
