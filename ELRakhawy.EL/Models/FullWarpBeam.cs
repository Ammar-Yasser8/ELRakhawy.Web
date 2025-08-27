using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FullWarpBeam
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Item { get; set; }

        // Origin Yarn: FK to YarnItem
        public int? OriginYarnId { get; set; }
        public virtual YarnItem? OriginYarn { get; set; }

        [Required]
        public bool Status { get; set; } = true;

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
