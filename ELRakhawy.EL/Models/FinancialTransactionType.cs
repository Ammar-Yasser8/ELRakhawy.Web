using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FinancialTransactionType
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Type { get; set; }

        public string? Comment { get; set; }

        public ICollection<StakeholderType> StakeholderTypes { get; set; } = new List<StakeholderType>();

    }
}
