using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FullWarpBeamTransaction
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string TransactionId { get; set; }
        [MaxLength(50)]
        public string? InternalId { get; set; }
        [MaxLength(50)]
        public string? ExternalId { get; set; }
        [Required]
        public int FullWarpBeamItemId { get; set; }
        public virtual FullWarpBeam FullWarpBeamItem { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Inbound { get; set; } = 0;
        [Range(0, double.MaxValue)]
        public decimal Outbound { get; set; } = 0;
        [Range(0, int.MaxValue)]
        public int Length { get; set; } = 0;

        [Required]
        public int StakeholderId { get; set; }
        public virtual StakeholdersInfo Stakeholder { get; set; }

        public decimal QuantityBalance { get; set; }
        public int CountBalance { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string? Comment { get; set; }
    }
}
