using ELRakhawy.DAL.Services;
using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ELRakhawy.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, AuthService authService, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        // GET: User/Groups
        public async Task<IActionResult> Groups()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();

                // Group users by role
                var userGroups = users
                    .GroupBy(u => u.Role)
                    .OrderBy(g => g.Key)
                    .Select(g => new UserRoleGroupViewModel
                    {
                        Role = g.Key,
                        RoleName = GetRoleDisplayName(g.Key),
                        Count = g.Count()
                    })
                    .ToList();

                _logger.LogInformation("تم تحميل {GroupCount} مجموعات من المستخدمين", userGroups.Count);

                return View(userGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل مجموعات المستخدمين");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل مجموعات المستخدمين";
                return View(new List<UserRoleGroupViewModel>());
            }
        }

        // GET: User/Index
        public async Task<IActionResult> Index(UserRole? role = null)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();

                // Filter by role if provided
                if (role.HasValue)
                {
                    users = users.Where(u => u.Role == role.Value).ToList();
                    ViewBag.FilteredRole = GetRoleDisplayName(role.Value);
                    ViewBag.FilteredRoleValue = role.Value;
                }

                ViewBag.AllRoles = Enum.GetValues(typeof(UserRole))
                    .Cast<UserRole>()
                    .Select(r => new { Value = r, Text = GetRoleDisplayName(r) })
                    .ToList();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل قائمة المستخدمين");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل قائمة المستخدمين";
                return View(new List<AppUser>());
            }
        }

        // Helper method to get Arabic role names
        private string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.Viewer => "مشاهد",
                UserRole.Editor => "محرر",
                UserRole.Added => "مضاف",
                UserRole.Clear => "مسؤول",
                UserRole.SuperAdmin => "مدير عام",
                _ => role.ToString()
            };
        }
        // GET: User/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                var userDetails = new
                {
                    success = true,
                    data = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.FullName,
                        user.Email,
                        user.Role,
                        RoleNameAr = GetRoleNameArabic(user.Role),
                        user.CreatedAt,
                        CreatedAtFormatted = user.CreatedAt.ToString("yyyy/MM/dd HH:mm", new System.Globalization.CultureInfo("ar-SA")),
                    }
                };

                return Json(userDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل تفاصيل المستخدم {UserId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل تفاصيل المستخدم" });
            }
        }

        // POST: User/Create - Changed to use form data instead of JSON
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "البيانات المدخلة غير صحيحة", errors = errors });
                }

                // Check if email already exists
                if (!await _authService.IsEmailUniqueAsync(model.Email))
                {
                    return Json(new { success = false, message = "البريد الإلكتروني مستخدم بالفعل" });
                }

                // Create user using AuthService
                var success = await _authService.RegisterAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Password,
                    model.Role);

                if (success)
                {
                    _logger.LogInformation("تم إنشاء مستخدم جديد: {FirstName} {LastName} ({Email}) بواسطة {CurrentUser} في {Timestamp}",
                        model.FirstName, model.LastName, model.Email, "Ammar-Yasser8", DateTime.UtcNow);

                    return Json(new { success = true, message = "تم إنشاء المستخدم بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في إنشاء المستخدم" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء المستخدم");
                return Json(new { success = false, message = "حدث خطأ أثناء إنشاء المستخدم" });
            }
        }

        // GET: User/GetForEdit/{id}
        [HttpGet]
        public async Task<IActionResult> GetForEdit(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                var editData = new
                {
                    success = true,
                    data = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.Role,
                    }
                };

                return Json(editData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل بيانات المستخدم للتعديل {UserId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل بيانات المستخدم" });
            }
        }

        // POST: User/Edit - Changed to use form data instead of JSON
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "البيانات المدخلة غير صحيحة", errors = errors });
                }

                var user = await _userRepository.GetByIdAsync(model.Id);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                // Check if email is changed and already exists
                if (user.Email != model.Email && !await _authService.IsEmailUniqueAsync(model.Email))
                {
                    return Json(new { success = false, message = "البريد الإلكتروني مستخدم بالفعل" });
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Role = model.Role;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("تم تحديث المستخدم: {FirstName} {LastName} (ID: {UserId}) بواسطة {CurrentUser} في {Timestamp}",
                    user.FirstName, user.LastName, user.Id, "Ammar-Yasser8", DateTime.UtcNow);

                return Json(new { success = true, message = "تم تحديث المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المستخدم {UserId}", model?.Id);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث المستخدم" });
            }
        }

        // POST: User/ResetPassword - Changed to use form data instead of JSON
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "البيانات المدخلة غير صحيحة", errors = errors });
                }

                var user = await _userRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                // Reset password using AuthService
                var success = await _authService.ResetPasswordAsync(model.UserId, model.NewPassword, model.RequesterRole);

                if (success)
                {
                    _logger.LogInformation("تم إعادة تعيين كلمة مرور المستخدم: {UserName} (ID: {UserId}) بواسطة {CurrentUser} في {Timestamp}",
                        user.FullName, user.Id, "Ammar-Yasser8", DateTime.UtcNow);

                    return Json(new { success = true, message = "تم إعادة تعيين كلمة المرور بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في إعادة تعيين كلمة المرور" });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Json(new { success = false, message = "ليس لديك صلاحية لتنفيذ هذا الإجراء" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إعادة تعيين كلمة المرور للمستخدم {UserId}", model?.UserId);
                return Json(new { success = false, message = "حدث خطأ أثناء إعادة تعيين كلمة المرور" });
            }
        }

        // POST: User/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                // Store user info for logging before deletion
                var userName = user.FullName;
                var userEmail = user.Email;

                await _userRepository.DeleteAsync(id);

                _logger.LogInformation("تم حذف المستخدم: {UserName} ({Email}) بواسطة {CurrentUser} في {Timestamp}",
                    userName, userEmail, "Ammar-Yasser8", DateTime.UtcNow);

                return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستخدم {UserId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف المستخدم" });
            }
        }

        // GET: User/GetRoles
        [HttpGet]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = Enum.GetValues<UserRole>()
                    .Select(role => new
                    {
                        Value = role.ToString(),
                        Text = GetRoleNameArabic(role),
                        NumericValue = (int)role
                    })
                    .ToList();

                return Json(new { success = true, data = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل الأدوار");
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل الأدوار" });
            }
        }

        // Helper method to get Arabic role names
        private static string GetRoleNameArabic(UserRole role)
        {
            return role switch
            {
                UserRole.SuperAdmin => "مدير عام",
                UserRole.Editor => "محرر",
                UserRole.Viewer => "مشاهد",
                UserRole.Added => "مضاف",
                UserRole.Clear => "مسؤول",
                _ => "غير محدد"
            };
        }
    }

    // ViewModels for form data (removed IsActive)
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 50 حرف")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأخير يجب أن يكون أقل من 50 حرف")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "تنسيق البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدور مطلوب")]
        public UserRole Role { get; set; }
    }

    public class EditUserViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأول يجب أن يكون أقل من 50 حرف")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [StringLength(50, ErrorMessage = "الاسم الأخير يجب أن يكون أقل من 50 حرف")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "تنسيق البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "الدور مطلوب")]
        public UserRole Role { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public UserRole RequesterRole { get; set; } = UserRole.SuperAdmin;
    }
}