using ELRakhawy.EL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELRakhawy.EL.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        [Display(Name = "معرف المستخدم")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        [Display(Name = "كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور الجديدة مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقتين")]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        [Display(Name = "دور الطالب")]
        public UserRole RequesterRole { get; set; } = UserRole.SuperAdmin;
    }
}
