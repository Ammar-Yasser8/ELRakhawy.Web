using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FabricStudio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الصنف مطلوب")]
        [Display(Name = "الصنف")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public virtual FabricItem? Item { get; set; }

        [Display(Name = "اللون")]
        public int? ColorId { get; set; }

        [ForeignKey("ColorId")]
        public virtual FabricColor? Color { get; set; }

        [Display(Name = "التصميم")]
        public int? DesignId { get; set; }

        [ForeignKey("DesignId")]
        public virtual FabricDesign? Design { get; set; }

        [Display(Name = "البوستر")]
        [MaxLength(500, ErrorMessage = "مسار البوستر يجب ألا يتجاوز 500 حرف")]
        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "الصورة مطلوبة")]
        [Display(Name = "الصورة")]
        [MaxLength(500, ErrorMessage = "مسار الصورة يجب ألا يتجاوز 500 حرف")]
        public required string WatermarkedImagePath { get; set; }

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

        [NotMapped]
        public string StudioType => ColorId.HasValue ? "لون" : (DesignId.HasValue ? "تصميم" : "غير محدد");

        [NotMapped]
        public string DisplayName
        {
            get
            {
                var itemName = Item?.Item ?? "غير محدد";
                var colorName = Color?.Color ?? "";
                var designName = Design?.Design ?? "";

                if (!string.IsNullOrEmpty(colorName))
                    return $"{itemName} - {colorName}";
                else if (!string.IsNullOrEmpty(designName))
                    return $"{itemName} - {designName}";
                else
                    return itemName;
            }
        }

        [NotMapped]
        public bool HasValidConfiguration => (ColorId.HasValue && !DesignId.HasValue) || (!ColorId.HasValue && DesignId.HasValue);

        [NotMapped]
        public bool RequiresImage => DesignId.HasValue;
    }
}
