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
        public string Item { get; set; }

        [Required(ErrorMessage = "الخام المكون مطلوب")]
        [Display(Name = "الخام المكون")]
        public int OriginRawId { get; set; }

        [ForeignKey("OriginRawId")]
        public virtual RawItem OriginRaw { get; set; }

        [Required(ErrorMessage = "حالة الصنف مطلوبة")]
        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true;

        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        [Display(Name = "بيان")]
        [DataType(DataType.MultilineText)]
        public string? Comment { get; set; }
    }
}