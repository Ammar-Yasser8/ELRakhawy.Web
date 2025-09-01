using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FullWarpBeamTransactionViewModel
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string? InternalId { get; set; }
        public string? ExternalId { get; set; }
        [Required]
        public int? FullWarpBeamItemId { get; set; }
        public string? FullWarpBeamItemName { get; set; }
        public List<SelectListItem> AvailableItems { get; set; } = new();

        [Required]
        [Range(0.001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Length { get; set; }

        [Required]
        public int? StakeholderId { get; set; }
        public string? StakeholderName { get; set; }
        public List<SelectListItem> Stakeholders { get; set; } = new();

        public decimal QuantityBalance { get; set; }
        public int CountBalance { get; set; }

        [Required]
        public DateTime Date { get; set; }
        public string? Comment { get; set; }

        public bool IsInbound { get; set; }
    }
}

