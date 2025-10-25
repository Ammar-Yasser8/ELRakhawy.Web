using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class ManufacturersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ManufacturersController> _logger;

        public ManufacturersController(IUnitOfWork unitOfWork, ILogger<ManufacturersController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: All users can view (Viewer, Added, Editor, Clear, SuperAdmin)
        [HttpGet]
        [Authorize(Roles = "Viewer,Added,Editor,Clear,SuperAdmin")]
        public IActionResult GetAllManufacturers()
        {
            try
            {
                var manufacturers = _unitOfWork.Repository<Manufacturers>().GetAll();
                var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";
                var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";

                _logger.LogInformation("تم تحميل قائمة المصنعين بواسطة {User} ({Role}) في {Time}",
                    currentUser, currentRole, "2025-10-25 09:32:49");

                return View(manufacturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل قائمة المصنعين");
                return View(new List<Manufacturers>());
            }
        }

        // POST: Create - Only Added and SuperAdmin can create
        [HttpPost("Create")]
        [Authorize(Roles = "Added,SuperAdmin")]
        public IActionResult Create(Manufacturers manufacturer)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";
            var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";

            // Debugging
            System.Diagnostics.Debug.WriteLine($"Create method called with: {manufacturer.Name} by {currentUser} ({currentRole})");

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Check if manufacturer name already exists (case insensitive)
                    var existing = _unitOfWork.Repository<Manufacturers>()
                        .GetAll()
                        .FirstOrDefault(m => m.Name.ToLower() == manufacturer.Name.ToLower());

                    if (existing != null)
                    {
                        _logger.LogWarning("محاولة إضافة مصنع موجود بالفعل: {Name} بواسطة {User} في {Time}",
                            manufacturer.Name, currentUser, "2025-10-25 09:32:49");

                        return Json(new
                        {
                            success = false,
                            message = $"اسم المصنع '{manufacturer.Name}' موجود بالفعل"
                        });
                    }

                    // If not exist → Save
                    _unitOfWork.Repository<Manufacturers>().Add(manufacturer);
                    _unitOfWork.Complete();

                    _logger.LogInformation("تمت إضافة مصنع جديد: {Name} بواسطة {User} ({Role}) في {Time}",
                        manufacturer.Name, currentUser, currentRole, "2025-10-25 09:32:49");

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            id = manufacturer.Id,
                            name = manufacturer.Name,
                            description = manufacturer.Description
                        },
                        message = "تمت إضافة المصنع بنجاح"
                    });
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "خطأ في إضافة المصنع {Name} بواسطة {User}", manufacturer.Name, currentUser);
                    System.Diagnostics.Debug.WriteLine($"Exception in Create: {ex.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء إضافة المصنع: {ex.Message}" });
                }
            }

            // Log model state errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Model error: {error.ErrorMessage}");
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء إضافة المصنع. يرجى التحقق من البيانات المدخلة." });
        }

        [HttpPost("Edit")]
        [Authorize(Roles = "Editor,SuperAdmin")]
        public IActionResult Edit(Manufacturers manufacturer)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";
            var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";

            if (ModelState.IsValid)
            {
                try
                {
                    var existingManufacturer = _unitOfWork.Repository<Manufacturers>()
                        .GetOne(m => m.Id == manufacturer.Id);

                    if (existingManufacturer == null)
                    {
                        return Json(new { success = false, message = "المصنع غير موجود" });
                    }

                    // ✅ تحقق من الاسم المكرر
                    var duplicateName = _unitOfWork.Repository<Manufacturers>()
                        .GetAll()
                        .FirstOrDefault(m => m.Name.ToLower() == manufacturer.Name.ToLower() && m.Id != manufacturer.Id);

                    if (duplicateName != null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"اسم المصنع '{manufacturer.Name}' مستخدم بالفعل"
                        });
                    }

                    // ✅ حدّث القيم فقط بدون استبدال الكائن
                    existingManufacturer.Name = manufacturer.Name;
                    existingManufacturer.Description = manufacturer.Description;
                    existingManufacturer.Status = manufacturer.Status;

                    _unitOfWork.Complete();

                    _logger.LogInformation("تم تحديث المصنع: {Name} (ID: {Id}) بواسطة {User} ({Role}) في {Time}",
                        manufacturer.Name, manufacturer.Id, currentUser, currentRole, DateTime.Now);

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            id = existingManufacturer.Id,
                            name = existingManufacturer.Name,
                            description = existingManufacturer.Description
                        },
                        message = "تم تحديث المصنع بنجاح"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطأ في تحديث المصنع {Id} بواسطة {User}", manufacturer.Id, currentUser);
                    return Json(new { success = false, message = $"حدث خطأ أثناء تحديث المصنع: {ex.Message}" });
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء تحديث المصنع. يرجى التحقق من البيانات المدخلة." });
        }


        // POST: Delete - Only Clear and SuperAdmin can delete
        [HttpPost("Delete")]
        [Authorize(Roles = "Clear,SuperAdmin")]
        public IActionResult Delete(int id)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";
            var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";

            // Add debugging
            System.Diagnostics.Debug.WriteLine($"Delete method called with ID: {id} by {currentUser} ({currentRole})");

            try
            {
                var manufacturer = _unitOfWork.Repository<Manufacturers>().GetOne(m => m.Id == id);
                if (manufacturer == null)
                {
                    return Json(new { success = false, message = "المصنع غير موجود" });
                }

                // Store manufacturer info for logging before deletion
                var manufacturerName = manufacturer.Name;

                _unitOfWork.Repository<Manufacturers>().Remove(manufacturer);
                _unitOfWork.Complete();

                _logger.LogInformation("تم حذف المصنع: {Name} (ID: {Id}) بواسطة {User} ({Role}) في {Time}",
                    manufacturerName, id, currentUser, currentRole, "2025-10-25 09:32:49");

                return Json(new { success = true, message = "تم حذف المصنع بنجاح" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المصنع {Id} بواسطة {User}", id, currentUser);
                System.Diagnostics.Debug.WriteLine($"Exception in Delete: {ex.Message}");
                return Json(new { success = false, message = $"حدث خطأ أثناء حذف المصنع: {ex.Message}" });
            }
        }

        // GET: Get single manufacturer - All authenticated users can view
        [HttpGet("Get/{id}")]
        [Authorize(Roles = "Viewer,Added,Editor,Clear,SuperAdmin")]
        public IActionResult Get(int id)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";

            try
            {
                var manufacturer = _unitOfWork.Repository<Manufacturers>().GetOne(m => m.Id == id);
                if (manufacturer == null)
                {
                    return Json(new { success = false, message = "المصنع غير موجود" });
                }

                _logger.LogInformation("تم عرض تفاصيل المصنع: {Name} (ID: {Id}) بواسطة {User} في {Time}",
                    manufacturer.Name, id, currentUser, "2025-10-25 09:32:49");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = manufacturer.Id,
                        name = manufacturer.Name,
                        description = manufacturer.Description,
                        status = manufacturer.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحميل تفاصيل المصنع {Id}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل تفاصيل المصنع" });
            }
        }

        // GET: Check user permissions for frontend - Helper method
        [HttpGet("GetUserPermissions")]
        [Authorize]
        public IActionResult GetUserPermissions()
        {
            var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";

            var permissions = new
            {
                canView = IsInRole("Viewer,Added,Editor,Clear,SuperAdmin", currentRole),
                canAdd = IsInRole("Added,SuperAdmin", currentRole),
                canEdit = IsInRole("Editor,SuperAdmin", currentRole),
                canDelete = IsInRole("Clear,SuperAdmin", currentRole),
                currentUser = currentUser,
                currentRole = currentRole,
                currentTime = "2025-10-25 09:32:49"
            };

            return Json(new { success = true, data = permissions });
        }

        // Helper method to check if user role is in allowed roles
        private bool IsInRole(string allowedRoles, string userRole)
        {
            return allowedRoles.Split(',').Any(role => role.Trim() == userRole);
        }

        // GET: Get role-based actions for frontend
        [HttpGet("GetAvailableActions")]
        [Authorize]
        public IActionResult GetAvailableActions()
        {
            var currentRole = HttpContext.Session.GetString("UserRole") ?? "Viewer";
            var actions = new List<object>();

            // View action - available to all roles
            if (IsInRole("Viewer,Added,Editor,Clear,SuperAdmin", currentRole))
            {
                actions.Add(new { action = "view", icon = "fas fa-eye", color = "info", text = "عرض" });
            }

            // Add action - only for Added and SuperAdmin
            if (IsInRole("Added,SuperAdmin", currentRole))
            {
                actions.Add(new { action = "add", icon = "fas fa-plus", color = "success", text = "إضافة" });
            }

            // Edit action - only for Editor and SuperAdmin  
            if (IsInRole("Editor,SuperAdmin", currentRole))
            {
                actions.Add(new { action = "edit", icon = "fas fa-edit", color = "warning", text = "تعديل" });
            }

            // Delete action - only for Clear and SuperAdmin
            if (IsInRole("Clear,SuperAdmin", currentRole))
            {
                actions.Add(new { action = "delete", icon = "fas fa-trash", color = "danger", text = "حذف" });
            }

            return Json(new
            {
                success = true,
                data = actions,
                userRole = currentRole,
                timestamp = "2025-10-25 09:32:49"
            });
        }

        // POST: Bulk operations - Only SuperAdmin
        [HttpPost("BulkDelete")]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";

            try
            {
                if (ids == null || !ids.Any())
                {
                    return Json(new { success = false, message = "لم يتم تحديد عناصر للحذف" });
                }

                var manufacturers = _unitOfWork.Repository<Manufacturers>()
                    .GetAll()
                    .Where(m => ids.Contains(m.Id))
                    .ToList();

                if (!manufacturers.Any())
                {
                    return Json(new { success = false, message = "لم يتم العثور على المصنعين المحددين" });
                }

                var deletedNames = manufacturers.Select(m => m.Name).ToList();

                foreach (var manufacturer in manufacturers)
                {
                    _unitOfWork.Repository<Manufacturers>().Remove(manufacturer);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("تم حذف {Count} مصنعين بالجملة بواسطة {User} في {Time}: {Names}",
                    manufacturers.Count, currentUser, "2025-10-25 09:32:49", string.Join(", ", deletedNames));

                return Json(new
                {
                    success = true,
                    message = $"تم حذف {manufacturers.Count} مصنع بنجاح",
                    deletedCount = manufacturers.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في الحذف الجماعي بواسطة {User}", currentUser);
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف الجماعي" });
            }
        }

        // GET: Export data - Available to all authenticated users
        [HttpGet("Export")]
        [Authorize(Roles = "Viewer,Added,Editor,Clear,SuperAdmin")]
        public IActionResult Export()
        {
            var currentUser = HttpContext.Session.GetString("UserName") ?? "Unknown";

            try
            {
                var manufacturers = _unitOfWork.Repository<Manufacturers>().GetAll();

                _logger.LogInformation("تم تصدير بيانات المصنعين ({Count} مصنع) بواسطة {User} في {Time}",
                    manufacturers.Count(), currentUser, "2025-10-25 09:32:49");

                return Json(new
                {
                    success = true,
                    data = manufacturers.Select(m => new {
                        m.Id,
                        m.Name,
                        m.Description,
                        m.Status
                    }),
                    exportedBy = currentUser,
                    exportedAt = "2025-10-25 09:32:49"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تصدير البيانات بواسطة {User}", currentUser);
                return Json(new { success = false, message = "حدث خطأ أثناء تصدير البيانات" });
            }
        }
    }
}