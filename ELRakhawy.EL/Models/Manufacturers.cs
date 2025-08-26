using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class Manufacturers
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الشركة يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "اسم الشركة")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "الحالة")]
        public bool Status { get; set; } = true; // نشط بشكل افتراضي

        // Navigation properties
        public virtual ICollection<YarnItem> YarnItems { get; set; } = new List<YarnItem>();
    }
}
