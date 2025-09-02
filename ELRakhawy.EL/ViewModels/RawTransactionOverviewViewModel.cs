using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class RawTransactionOverviewViewModel
    {
        public List<RawTransactionSummaryDto> Transactions { get; set; } = new List<RawTransactionSummaryDto>();
        public List<SelectListItem> RawItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stakeholders { get; set; } = new List<SelectListItem>();

        // Filters
        public int? SelectedRawItemId { get; set; }
        public int? SelectedStakeholderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TransactionType { get; set; } = "All"; // "All", "Inbound", "Outbound"
        public string SearchQuery { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }

        // Summary Statistics
        public double TotalInboundMeters { get; set; }
        public double TotalInboundKg { get; set; }
        public double TotalOutbound { get; set; }
        public double CurrentBalance { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalCount { get; set; }
        public int CurrentCountBalance { get; set; }

        // Helper properties
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public double TotalInbound => TotalInboundMeters + TotalInboundKg;
    }

    public class RawTransactionSummaryDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string InternalId { get; set; }
        public string ExternalId { get; set; }
        public DateTime Date { get; set; }
        public string RawItemName { get; set; }
        public string WarpName { get; set; }
        public string WeftName { get; set; }
        public string StakeholderName { get; set; }
        public string PackagingStyleName { get; set; }

        // Transaction amounts
        public double InboundMeters { get; set; }
        public double InboundKg { get; set; }
        public double Outbound { get; set; }
        public int Count { get; set; }

        // Balances
        public double QuantityBalance { get; set; }
        public int CountBalance { get; set; }

        public string Comment { get; set; }

        // Helper properties
        public bool IsInbound => InboundMeters > 0 || InboundKg > 0;
        public double TotalInbound => InboundMeters + InboundKg;
        public string TransactionType => IsInbound ? "وارد" : "صادر";
        public string TransactionTypeClass => IsInbound ? "success" : "warning";
        public string TransactionTypeIcon => IsInbound ? "fa-arrow-down" : "fa-arrow-up";
    }
    public class RawTransactionDetailsViewModel
    {
        public RawTransactionSummaryDto Transaction { get; set; }
        public string RawItemDetails { get; set; }
        public string WarpDetails { get; set; }
        public string WeftDetails { get; set; }
        public List<RawTransactionSummaryDto> RelatedTransactions { get; set; } = new List<RawTransactionSummaryDto>();
        public double CurrentItemBalance { get; set; }
        public int CurrentItemCountBalance { get; set; }

        // Add raw item ID for reset functionality
        public int RawItemId { get; set; }

        // Helper properties
        public bool HasRelatedTransactions => RelatedTransactions.Any();
        public int RelatedTransactionsCount => RelatedTransactions.Count;
        public bool HasPositiveBalance => CurrentItemBalance > 0 || CurrentItemCountBalance > 0;
    }
}
