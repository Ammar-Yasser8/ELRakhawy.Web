using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    // Add this class
    public class FormSelectionViewModel
    {
        public int Id { get; set; }
        public string FormName { get; set; }
        public bool IsSelected { get; set; }
    }

    // Modified StakeholderTypeViewModel
    public class StakeholderTypeViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Type { get; set; }
        public string? Comment { get; set; }
        [Required]
        public int FinancialTransactionTypeId { get; set; }

        // Remove any validation attributes from this property
        public List<int> SelectedFormIds { get; set; } = new List<int>();

        public List<FinancialTransactionType> FinancialTransactionTypes { get; set; } = new List<FinancialTransactionType>();
        public List<FormStyle> AvailableForms { get; set; } = new List<FormStyle>();
    }
}
