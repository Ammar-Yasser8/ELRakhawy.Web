using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class PackagingStyles
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "اسم التعبئة يجب ألا يتجاوز 100 حرف")]
        public string StyleName { get; set; } = string.Empty;
        public string? Comment { get; set; } = string.Empty;
        public ICollection<PackagingStyleForms> PackagingStyleForms { get; set; }

    }
}
