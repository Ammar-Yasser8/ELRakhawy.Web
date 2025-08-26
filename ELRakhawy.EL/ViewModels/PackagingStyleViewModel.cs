using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class PackagingStyleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم التعبئة مطلوب")]
        [StringLength(100, ErrorMessage = "اسم التعبئة يجب ألا يتجاوز 100 حرف")]
        [Display(Name = "التعبئة")]
        public string StyleName { get; set; } = string.Empty;

        [Display(Name = "بيان")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "الواجهات المرتبطة مطلوبة")]
        [Display(Name = "الواجهات المرتبطة")]
        public List<int> SelectedFormIds { get; set; } = new List<int>();

        public List<FormStyleViewModel> AvailableForms { get; set; } = new List<FormStyleViewModel>();
    }

    public class FormStyleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العملية التجارية مطلوب")]
        [StringLength(100, ErrorMessage = "اسم العملية التجارية يجب أن يكون أقل من 100 حرف")]
        [Display(Name = "اسم العملية التجارية")]
        public string FormName { get; set; }

        public bool IsSelected { get; set; }

        // Properties for business operation suggestions
        public List<string> BusinessOperationForms { get; set; } = new List<string>();
        public List<string> ExistingFormNames { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();

        // Properties for usage information
        public bool IsInUse { get; set; }
        public int UsageCount { get; set; }
    }
}
