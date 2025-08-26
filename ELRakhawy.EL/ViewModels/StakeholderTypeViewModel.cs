using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class StakeholderTypeViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        public string? Comment { get; set; }

        [Required]
        public int FinancialTransactionTypeId { get; set; }

        public List<int> SelectedFormIds { get; set; } = new List<int>();

        public List<FinancialTransactionType> FinancialTransactionTypes { get; set; } = new List<FinancialTransactionType>();
        public List<FormStyle> AvailableForms { get; set; } = new List<FormStyle>();
    }
}
