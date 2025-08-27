using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class RawItem
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Item { get; set; }

        // Warp: FK to FullWarpBeam
        [Required]
        public int WarpId { get; set; }
        public virtual FullWarpBeam Warp { get; set; }

        // Weft: FK to YarnItem
        [Required]
        public int WeftId { get; set; }
        public virtual YarnItem Weft { get; set; }

        [Required]
        public bool Status { get; set; } = true;

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
