using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.Models
{
    public class FormStyle
    {
        public int Id { get; set; }
        [Required]
        public string FormName { get; set; } = string.Empty;
        public ICollection<PackagingStyleForms> PackagingStyleForms { get; set; }
        public ICollection<StakeholderTypeForm> StakeholderTypeForms { get; set; }

    }
}
