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

        [Required]
        public string Name { get; set; }

        public bool Status { get; set; }

        public string? ContactNumbers { get; set; }

        public string? Comment { get; set; }

        public List<int> SelectedTypeIds { get; set; } = new List<int>();
        public int? PrimaryTypeId { get; set; }

        public List<StakeholderType> AvailableTypes { get; set; } = new List<StakeholderType>();
    }
}
