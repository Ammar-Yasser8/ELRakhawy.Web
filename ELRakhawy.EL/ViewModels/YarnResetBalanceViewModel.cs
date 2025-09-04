using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class YarnResetBalanceViewModel
    {
        [Required(ErrorMessage = "يجب اختيار صنف الغزل")]
        [Display(Name = "صنف الغزل")]
        public int YarnItemId { get; set; }

        public string YarnItemName { get; set; }

        [Display(Name = "رقم الإذن الداخلي")]
        public string InternalId { get; set; }

        [Required(ErrorMessage = "يجب تحديد تاريخ التصفير")]
        [Display(Name = "تاريخ التصفير")]
        public DateTime ResetDate { get; set; }

        [Required(ErrorMessage = "يجب ذكر سبب التصفير")]
        [Display(Name = "سبب التصفير")]
        [StringLength(500, ErrorMessage = "سبب التصفير يجب أن يكون أقل من 500 حرف")]
        public string ReasonForReset { get; set; }

        [Display(Name = "تأكيد التصفير")]
        public bool ConfirmReset { get; set; }

        public string ResetBy { get; set; }

        // Display properties
        public decimal CurrentQuantityBalance { get; set; }
        public int CurrentCountBalance { get; set; }
        public bool ShowConfirmation { get; set; }

        public List<SelectListItem> AvailableItems { get; set; } = new();
    }
}
