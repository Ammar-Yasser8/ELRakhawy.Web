using ELRakhawy.DAL.Services;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


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
                    return View(model);

                var user = await _authService.LoginAsync(model.Email, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                // 🔒 Check if user already has an active session
                if (!string.IsNullOrEmpty(user.CurrentSessionToken))
                {
                    _logger.LogWarning("⚠️ محاولة تسجيل دخول مرفوضة - المستخدم {UserName} لديه جلسة نشطة بالفعل",
                        user.FullName);

                    ModelState.AddModelError(string.Empty,
                        "لديك جلسة نشطة بالفعل على جهاز آخر. يرجى تسجيل الخروج أولاً أو الانتظار حتى انتهاء الجلسة.");

                    return View(model);
                }

                // ✅ Generate new SessionToken
                var sessionToken = Guid.NewGuid().ToString();

                // ✅ Update in database
                await _authService.UpdateUserSessionAsync(user.Id, sessionToken);

                _logger.LogWarning("🔑 تسجيل دخول جديد - المستخدم: {UserId}, البريد: {Email}, Token: {Token}",
                    user.Id, user.Email, sessionToken);

                // ✅ Create Claims
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("SessionToken", sessionToken)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddHours(2)
                    });

                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);

                _logger.LogInformation("المستخدم {UserName} قام بتسجيل الدخول بنجاح في {Timestamp}",
                    user.FullName, DateTime.UtcNow);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تسجيل الدخول للمستخدم {Email}", model.Email);
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
        // GET: /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout(string? reason = null)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId != null)
            {
                await _authService.UpdateUserSessionAsync(int.Parse(userId), null);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            if (reason == "timeout")
                TempData["ErrorMessage"] = "تم تسجيل خروجك تلقائيًا بعد 20 دقيقة من عدم النشاط.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId != null)
            {
                await _authService.UpdateUserSessionAsync(int.Parse(userId), null); // حذف الـ Token
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            _logger.LogInformation("تم تسجيل الخروج بنجاح في {Timestamp}", DateTime.UtcNow);
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
