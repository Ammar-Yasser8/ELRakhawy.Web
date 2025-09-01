using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FullWarpBeamViewModel
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public int? OriginYarnId { get; set; }
        public string? OriginYarnName { get; set; }
        public bool Status { get; set; }
        public string? Comment { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
