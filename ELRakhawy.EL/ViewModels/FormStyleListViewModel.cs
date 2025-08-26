using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FormStyleListViewModel
    {
        public int Id { get; set; }
        public string FormName { get; set; }
        public int UsageCount { get; set; }
        public string FormCategory { get; set; }
    }

    public class FormStyleDetailsViewModel
    {
        public int Id { get; set; }
        public string FormName { get; set; }
        public string FormCategory { get; set; }
        public List<PackagingStyles> PackagingStyles { get; set; } = new List<PackagingStyles>();
        public int UsageCount { get; set; }
        public string CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class FormStyleDeleteViewModel
    {
        public int Id { get; set; }
        public string FormName { get; set; }
        public string FormCategory { get; set; }
        public bool IsInUse { get; set; }
        public int UsageCount { get; set; }
        public List<PackagingStyles> RelatedPackagingStyles { get; set; } = new List<PackagingStyles>();
    }
}
