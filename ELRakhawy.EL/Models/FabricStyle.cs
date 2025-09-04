using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FabricStyle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الطراز مطلوب")]
        [MaxLength(200, ErrorMessage = "الطراز يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الطراز")]
        public required string Style { get; set; }

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; } = null;
    }
}
