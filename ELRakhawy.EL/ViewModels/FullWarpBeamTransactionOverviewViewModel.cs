using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FullWarpBeamTransactionOverviewViewModel
    {
        public List<FullWarpBeamTransactionSummaryDto> Transactions { get; set; } = new List<FullWarpBeamTransactionSummaryDto>();
        public List<SelectListItem> FullWarpBeamItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stakeholders { get; set; } = new List<SelectListItem>();

        // Filters
        public int? SelectedFullWarpBeamItemId { get; set; }
        public int? SelectedStakeholderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TransactionType { get; set; } // "All", "Inbound", "Outbound"
        public string SearchQuery { get; set; }

        // Summary Statistics
        public decimal TotalInbound { get; set; }
        public decimal TotalOutbound { get; set; }
        public decimal CurrentBalance { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalLengthInbound { get; set; }
        public int TotalLengthOutbound { get; set; }
        public int CurrentLengthBalance { get; set; }
    }

    public class FullWarpBeamTransactionSummaryDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string InternalId { get; set; }
        public string ExternalId { get; set; }
        public DateTime Date { get; set; }
        public string FullWarpBeamItemName { get; set; }
        public string StakeholderName { get; set; }
        public decimal Quantity { get; set; }
        public int Length { get; set; }
        public bool IsInbound { get; set; }
        public decimal QuantityBalance { get; set; }
        public int CountBalance { get; set; }
        public string Comment { get; set; }
        public string TransactionType => IsInbound ? "وارد" : "صادر";
        public string TransactionTypeClass => IsInbound ? "success" : "warning";
    }

    public class FullWarpBeamTransactionDetailsViewModel
    {
        public FullWarpBeamTransactionSummaryDto Transaction { get; set; }
        public string FullWarpBeamItemDetails { get; set; }
        public string OriginYarnName { get; set; }
        public string StakeholderTypeDisplayName { get; set; }
        public List<FullWarpBeamTransactionSummaryDto> RelatedTransactions { get; set; } = new List<FullWarpBeamTransactionSummaryDto>();
        public decimal CurrentItemBalance { get; set; }
        public int CurrentItemLengthBalance { get; set; }
    }
}
