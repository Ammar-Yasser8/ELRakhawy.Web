using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class StakeholderType
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Type { get; set; }

        public string? Comment { get; set; }

        public int FinancialTransactionTypeId { get; set; }
        public FinancialTransactionType FinancialTransactionType { get; set; }

        public ICollection<StakeholderTypeForm> StakeholderTypeForms { get; set; } = new List<StakeholderTypeForm>();
        public ICollection<StakeholderInfoType> StakeholderInfoTypes { get; set; } = new List<StakeholderInfoType>();

    }
}
