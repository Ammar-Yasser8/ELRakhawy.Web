using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FabricDesign
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التصميم مطلوب")]
        [MaxLength(200, ErrorMessage = "التصميم يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "التصميم")]
        public required string Design { get; set; }

        [Required(ErrorMessage = "الطراز مطلوب")]
        [Display(Name = "الطراز")]
        public int StyleId { get; set; }

        [ForeignKey("StyleId")]
        public virtual FabricStyle? Style { get; set; }

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }
    }
}
