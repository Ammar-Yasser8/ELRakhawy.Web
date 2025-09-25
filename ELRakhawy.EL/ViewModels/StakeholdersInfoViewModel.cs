using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class StakeholdersInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // بدلاً من ContactNumbers (String واحدة)
        public string CountryCode { get; set; }
        public string ContactNumber { get; set; }

        public bool Status { get; set; }
        public string? Comment { get; set; }

        public List<int> SelectedTypeIds { get; set; } = new();
        public int? PrimaryTypeId { get; set; }
        public IEnumerable<StakeholderType> AvailableTypes { get; set; } = new List<StakeholderType>();
    }


}
