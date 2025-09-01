using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    // YarnOverviewViewModel.cs
    public class YarnOverviewViewModel
    {
        public List<YarnOverviewItemViewModel> OverviewItems { get; set; } = new List<YarnOverviewItemViewModel>();
        public bool AvailableOnly { get; set; }
        public int TotalItems { get; set; }
        public int AvailableItems { get; set; }
        public decimal TotalQuantityBalance { get; set; }
        public int TotalCountBalance { get; set; }
        public DateTime LastUpdated { get; set; }

        // Summary statistics
        public decimal PercentageAvailable => TotalItems > 0 ? (decimal)AvailableItems / TotalItems * 100 : 0;
        public int OutOfStockItems => TotalItems - AvailableItems;
    }

    // YarnOverviewItemViewModel.cs
    public class YarnOverviewItemViewModel
    {
        public int YarnItemId { get; set; }
        public string YarnItemName { get; set; }
        public string OriginYarnName { get; set; }
        public string ManufacturerNames { get; set; } // Changed from ManufacturerName
        public decimal QuantityBalance { get; set; }
        public int CountBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public int TotalTransactions { get; set; }
        public int InboundTransactions { get; set; }
        public int OutboundTransactions { get; set; }
        public bool IsAvailable { get; set; }
        public bool Status { get; set; }

        // Helper properties
        public string AvailabilityStatus => IsAvailable ? "متاح" : "غير متاح";
        public string AvailabilityClass => IsAvailable ? "text-success" : "text-danger";
        public string LastTransactionText => LastTransactionDate?.ToString("dd/MM/yyyy") ?? "لا توجد معاملات";
        public int DaysSinceLastTransaction => LastTransactionDate?.Date != null ?
            (DateTime.Today - LastTransactionDate.Value.Date).Days : -1;
    }
}
