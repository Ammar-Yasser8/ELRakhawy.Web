using ELRakhawy.DAL.Services;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ELRakhawy.Web.Controllers
{
   public class AuthController : Controller
   {
       private readonly AuthService _authService;
       private readonly ILogger<AuthController> _logger;

       public AuthController(AuthService authService, ILogger<AuthController> logger)
       {
           _authService = authService;
           _logger = logger;
       }

        public IActionResult denied()
        {
            return View();
        }

        // GET: /Auth/Login
        [HttpGet]
       public IActionResult Login()
       {
           // Redirect if already logged in
           if (HttpContext.Session.GetString("UserId") != null)
           {
               return RedirectToAction("Index", "Home");
           }

           return View(new LoginViewModel());
       }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _authService.LoginAsync(model.Email, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                    _logger.LogWarning("فشل محاولة تسجيل دخول للبريد الإلكتروني: {Email} في {Timestamp}",
                        model.Email, DateTime.UtcNow);
                    return View(model);
                }

                // ✅ أنشئ Claims للمستخدم
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // لو عندك Roles زي Admin/User
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // ✅ تسجيل الدخول باستخدام الـ Cookies
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true, // يخلي الكوكيز تفضل بعد غلق المتصفح (اختياري)
                        ExpiresUtc = DateTime.UtcNow.AddHours(2)
                    });

                // ✅ ممكن تحتفظ بالبيانات في الـ Session لو محتاجها في الـ UI فقط
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);

                _logger.LogInformation("المستخدم {UserName} ({Email}) قام بتسجيل الدخول بنجاح في {Timestamp}",
                    user.FullName, user.Email, DateTime.UtcNow);

                // ✅ بعد الدخول - حوّله مثلاً إلى الصفحة الرئيسية
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تسجيل الدخول للمستخدم {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تسجيل الدخول. يرجى المحاولة مرة أخرى.");
                return View(model);
            }
        }

        // GET: /Auth/Register
        [HttpGet]
       public IActionResult Register()
       {
           return View(new RegisterViewModel());
       }

       // POST: /Auth/Register
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Register(RegisterViewModel model)
       {
           try
           {
               if (!ModelState.IsValid)
               {
                   return View(model);
               }

               var success = await _authService.RegisterAsync(model.FirstName, model.LastName, model.Email, model.Password, model.Role);

               if (!success)
               {
                   ModelState.AddModelError(string.Empty, "البريد الإلكتروني مستخدم بالفعل");
                   return View(model);
               }

               _logger.LogInformation("تم إنشاء حساب جديد للمستخدم {FirstName} {LastName} ({Email}) في {Timestamp}",
                   model.FirstName, model.LastName, model.Email, DateTime.UtcNow);

               TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح. يمكنك الآن تسجيل الدخول.";
               return RedirectToAction("Login");
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "خطأ في إنشاء حساب للمستخدم {Email}", model.Email);
               ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إنشاء الحساب. يرجى المحاولة مرة أخرى.");
               return View(model);
           }
       }

       // GET: /Auth/ResetPassword
       [HttpGet]
       public IActionResult ResetPassword()
       {
           return View(new ResetPasswordViewModel());
       }

       // POST: /Auth/ResetPassword
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
       {
           try
           {
               if (!ModelState.IsValid)
               {
                   return View(model);
               }

               var success = await _authService.ResetPasswordAsync(model.UserId, model.NewPassword, model.RequesterRole);

               if (!success)
               {
                   ModelState.AddModelError(string.Empty, "المستخدم غير موجود");
                   return View(model);
               }

               _logger.LogInformation("تم إعادة تعيين كلمة المرور للمستخدم {UserId} في {Timestamp}",
                   model.UserId, DateTime.UtcNow);

               TempData["SuccessMessage"] = "تم إعادة تعيين كلمة المرور بنجاح";
               return RedirectToAction("Login");
           }
           catch (UnauthorizedAccessException)
           {
               ModelState.AddModelError(string.Empty, "ليس لديك صلاحية لتنفيذ هذا الإجراء");
               return View(model);
           }
           catch (ArgumentException ex)
           {
               ModelState.AddModelError(string.Empty, ex.Message);
               return View(model);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "خطأ في إعادة تعيين كلمة المرور للمستخدم {UserId}", model.UserId);
               ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إعادة تعيين كلمة المرور. يرجى المحاولة مرة أخرى.");
               return View(model);
           }
       }

       // POST: /Auth/Logout
       [HttpPost]
       [ValidateAntiForgeryToken]
       public IActionResult Logout()
       {
           var userName = HttpContext.Session.GetString("UserName");
           HttpContext.Session.Clear();

           _logger.LogInformation("المستخدم {UserName} قام بتسجيل الخروج في {Timestamp}",
               userName ?? "غير معروف", DateTime.UtcNow);

           TempData["Message"] = "تم تسجيل الخروج بنجاح";
           return RedirectToAction("Login");
       }

       // GET: /Auth/AccessDenied
       [HttpGet]
       public IActionResult AccessDenied()
       {
           return View();
       }
   }

}
