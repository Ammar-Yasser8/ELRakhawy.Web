using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class FabricStyleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الطراز مطلوب")]
        [MaxLength(200, ErrorMessage = "الطراز يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "الطراز")]
        public string Style { get; set; }

        [Display(Name = "البيان")]
        [MaxLength(1000, ErrorMessage = "البيان يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        // Helper properties
        public bool IsEdit => Id > 0;
        public string FormTitle => IsEdit ? "تعديل طراز القماش" : "إضافة طراز قماش جديد";
    }

    public class FabricStyleIndexViewModel
    {
        public List<FabricStyleViewModel> FabricStyles { get; set; } = new List<FabricStyleViewModel>();
        public string SearchQuery { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Helper properties
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasItems => FabricStyles.Any();
    }
}
