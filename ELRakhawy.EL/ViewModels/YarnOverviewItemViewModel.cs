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
        public List<YarnOverviewItemViewModel> OverviewItems { get; set; } = new();
        public bool AvailableOnly { get; set; }
        public int TotalItems { get; set; }
        public int AvailableItems { get; set; }
        public decimal TotalQuantityBalance { get; set; }
        public int TotalCountBalance { get; set; }
        public DateTime LastUpdated { get; set; }

        // ✅ Add percentage calculation
        public decimal PercentageAvailable => TotalItems > 0 ? ((decimal)AvailableItems / TotalItems) * 100 : 0;
    }

    // YarnOverviewItemViewModel.cs
    public class YarnOverviewItemViewModel
    {
        public int YarnItemId { get; set; }
        public string YarnItemName { get; set; }
        public string OriginYarnName { get; set; } // ✅ Should display origin yarn name
        public string ManufacturerNames { get; set; } // ✅ Should display manufacturer names
        public decimal QuantityBalance { get; set; }
        public int CountBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public int TotalTransactions { get; set; }
        public int InboundTransactions { get; set; }
        public int OutboundTransactions { get; set; }
        public bool IsAvailable { get; set; }
        public bool Status { get; set; }

        // ✅ Computed properties for display
        public string LastTransactionText => LastTransactionDate?.ToString("dd/MM/yyyy") ?? "لا توجد معاملات";
        public int DaysSinceLastTransaction => LastTransactionDate.HasValue
            ? (DateTime.Now - LastTransactionDate.Value).Days
            : -1;
        public string AvailabilityStatus => IsAvailable ? "متاح" : "غير متاح";
    }
}
