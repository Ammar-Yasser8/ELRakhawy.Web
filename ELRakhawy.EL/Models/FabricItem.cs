using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FabricItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الصنف يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الصنف")]
        public required string Item { get; set; }

        [Required(ErrorMessage = "الخام المكون مطلوب")]
        [Display(Name = "الخام المكون")]
        public int OriginRawId { get; set; }

        [ForeignKey("OriginRawId")]
        public virtual RawItem? OriginRaw { get; set; }

        [Required(ErrorMessage = "الحالة مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        
        // Helper properties
        [NotMapped]
        public string StatusText => Status ? "نشط" : "غير نشط";

        [NotMapped]
        public string StatusClass => Status ? "success" : "secondary";

        [NotMapped]
        public string StatusIcon => Status ? "fa-check-circle" : "fa-times-circle";
    }
}
