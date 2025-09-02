using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class FormStylesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FormStylesController> _logger;
        private readonly string _currentUser = "Ammar-Yasser8";
        private readonly string _currentTime = "2025-08-19 20:30:37";

        // Predefined business operation form types for suggestions
        private readonly List<string> _businessOperationForms = new List<string>
        {
            // Yarn Operations (عمليات الغزل)
            "اضافة وارد غزل", "اضافة صادر غزل", "تعديل مخزون غزل", "جرد غزل", "تحويل غزل",
            "فحص جودة غزل", "تصنيف غزل", "تخزين غزل", "اتلاف غزل", "ارجاع غزل",
            
            // Fabric Operations (عمليات الأقمشة)
            "اضافة وارد قماش", "اضافة صادر قماش", "تعديل مخزون قماش", "جرد قماش", "تحويل قماش",
            "فحص جودة قماش", "تصنيف قماش", "تخزين قماش", "اتلاف قماش", "ارجاع قماش",
            
            // Raw Materials Operations (عمليات الخام)
            "ااضافة وارد خام", "اضافة صادر خام", "تعديل مخزون خام", "جرد خام",
            "فحص جودة خام", "تخزين خام", "اتلاف خام", "ارجاع خام",
            
            // Finished Products Operations (عمليات المنتجات النهائية)
            "اضافة وارد منتجات", "اضافة صادر منتجات", "تعديل مخزون منتجات", "جرد منتجات",
            "فحص جودة منتجات", "تعبئة منتجات", "شحن منتجات", "ارجاع منتجات",
            
            // Accessories Operations (عمليات الإكسسوارات)
            "اضافة وارد اكسسوارات", "اضافة صادر اكسسوارات", "تعديل مخزون اكسسوارات", "جرد اكسسوارات",
            "فحص جودة اكسسوارات", "تخزين اكسسوارات", "اتلاف اكسسوارات", "ارجاع اكسسوارات",
            
            // Dyes and Chemicals Operations (عمليات الأصباغ والكيماويات)
            "اضافة وارد اصباغ", "اضافة صادر اصباغ", "تعديل مخزون اصباغ", "جرد اصباغ",
            "اضافة وارد كيماويات", "اضافة صادر كيماويات", "تعديل مخزون كيماويات", "جرد كيماويات",
            
            // Production Operations (عمليات الإنتاج)
            "بدء عملية انتاج", "ايقاف عملية انتاج", "استكمال عملية انتاج", "مراقبة انتاج",
            "تحويل انتاج", "فحص انتاج", "تعبئة انتاج", "اعتماد انتاج",
            
            // Quality Control Operations (عمليات مراقبة الجودة)
            "فحص جودة واردات", "فحص جودة صادرات", "اعتماد جودة", "رفض جودة",
            "تقرير جودة", "متابعة جودة", "تحسين جودة", "معايرة اجهزة فحص",
            
            // Warehouse Operations (عمليات المخازن)
            "نقل بين مخازن", "تنظيم مخازن", "تنظيف مخازن", "صيانة مخازن",
            "جرد دوري", "جرد مفاجئ", "تحديث مواقع تخزين", "تتبع مخزون",
            
            // Financial Operations (العمليات المالية)
            "اضافة فاتورة شراء", "اضافة فاتورة بيع", "تسديد مستحقات", "تحصيل مستحقات",
            "اضافة مصروفات", "اضافة ايرادات", "تسوية حسابات", "اقفال مالي",
            
            // Administrative Operations (العمليات الإدارية)
            "اضافة موظف", "تعديل بيانات موظف", "حذف موظف", "تقييم اداء",
            "اضافة اجازة", "اعتماد اجازة", "حساب رواتب", "تقرير حضور",
            
            // Maintenance Operations (عمليات الصيانة)
            "صيانة دورية", "صيانة طارئة", "تغيير قطع غيار", "معايرة اجهزة",
            "فحص سلامة", "تقرير صيانة", "جدولة صيانة", "متابعة اعطال"
        };

        public FormStylesController(IUnitOfWork unitOfWork, ILogger<FormStylesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Standard CRUD Operations

        // GET: FormStyles
        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Retrieving business operation forms list by {User} at {Time}", _currentUser, _currentTime);

                var formStyles = _unitOfWork.Repository<FormStyle>().GetAll();

                var forms = formStyles.Select(form =>
                {
                    var usageCount = _unitOfWork.Repository<PackagingStyleForms>()
                        .GetAll(psf => psf.FormId == form.Id).Count();

                    return new FormStyleListViewModel
                    {
                        Id = form.Id,
                        FormName = form.FormName,
                        UsageCount = usageCount,
                        FormCategory = GetFormCategory(form.FormName)
                    };
                }).OrderBy(f => f.FormCategory).ThenBy(f => f.FormName).ToList();

                _logger.LogInformation("Retrieved {Count} business operation forms by {User} at {Time}",
                    forms.Count, _currentUser, _currentTime);

                return View(forms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving business operation forms by {User} at {Time}",
                    _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد قائمة العمليات التجارية. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // GET: FormStyles/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation("Getting details for business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms,PackagingStyleForms.PackagingStyle");

                if (formStyle == null)
                {
                    _logger.LogWarning("Business operation form not found for ID {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                // Get packaging styles that use this form
                var packagingStyles = formStyle.PackagingStyleForms
                    .Select(psf => psf.PackagingStyle)
                    .ToList();

                var viewModel = new FormStyleDetailsViewModel
                {
                    Id = formStyle.Id,
                    FormName = formStyle.FormName,
                    FormCategory = GetFormCategory(formStyle.FormName),
                    PackagingStyles = packagingStyles,
                    UsageCount = packagingStyles.Count,
                    CreatedAt = _currentTime,
                    CreatedBy = _currentUser
                };
                
                _logger.LogInformation("Retrieved details for business operation form '{FormName}' (ID: {Id}) by {User} at {Time}",
                    formStyle.FormName, id, _currentUser, _currentTime);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving business operation form details for ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد تفاصيل العملية التجارية. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // GET: FormStyles/Create
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Loading create business operation form view by {User} at {Time}", _currentUser, _currentTime);

                var viewModel = new FormStyleViewModel
                {
                    BusinessOperationForms = _businessOperationForms.OrderBy(f => f).ToList(),
                    ExistingFormNames = GetExistingFormNames()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create business operation form view by {User} at {Time}",
                    _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء تحميل صفحة إنشاء العملية التجارية.");
            }
        }

        // POST: FormStyles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FormStyleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                LoadFormSuggestions(viewModel);
                return View(viewModel);
            }

            try
            {
                _logger.LogInformation("Creating business operation form '{FormName}' by {User} at {Time}",
                    viewModel.FormName, _currentUser, _currentTime);

                // Check if form name already exists (case-insensitive)
                var existingForm = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.FormName.Trim().ToLower() == viewModel.FormName.Trim().ToLower());

                if (existingForm != null)
                {
                    _logger.LogWarning("Duplicate business operation form name attempted: '{FormName}' by {User} at {Time}",
                        viewModel.FormName, _currentUser, _currentTime);
                    ModelState.AddModelError("FormName", "اسم العملية التجارية موجود بالفعل");
                    LoadFormSuggestions(viewModel);
                    return View(viewModel);
                }

                var formStyle = new FormStyle
                {
                    FormName = viewModel.FormName.Trim()
                };

                _unitOfWork.Repository<FormStyle>().Add(formStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("Business operation form '{FormName}' created successfully with ID {Id} by {User} at {Time}",
                    viewModel.FormName, formStyle.Id, _currentUser, _currentTime);

                TempData["Success"] = "تم إنشاء العملية التجارية بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating business operation form: {FormName} by {User} at {Time}",
                    viewModel.FormName, _currentUser, _currentTime);
                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء العملية التجارية");
                LoadFormSuggestions(viewModel);
                return View(viewModel);
            }
        }

        // GET: FormStyles/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                _logger.LogInformation("Loading edit view for business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms");

                if (formStyle == null)
                {
                    _logger.LogWarning("Business operation form not found for edit, ID: {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }
                
                // Check if form is in use
                var isInUse = formStyle.PackagingStyleForms.Any();
                var usageCount = formStyle.PackagingStyleForms.Count;

                var viewModel = new FormStyleViewModel
                {
                    Id = formStyle.Id,
                    FormName = formStyle.FormName,
                    BusinessOperationForms = _businessOperationForms.OrderBy(f => f).ToList(),
                    ExistingFormNames = GetExistingFormNames().Where(name => name != formStyle.FormName).ToList(),
                    IsInUse = isInUse,
                    UsageCount = usageCount
                };

                ViewBag.FormCategory = GetFormCategory(formStyle.FormName);
                ViewBag.IsInUse = isInUse;
                ViewBag.UsageCount = usageCount;

                _logger.LogInformation("Loaded edit view for business operation form '{FormName}' (ID: {Id}) by {User} at {Time}",
                    formStyle.FormName, id, _currentUser, _currentTime);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading edit view for business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التعديل";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: FormStyles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FormStyleViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                _logger.LogWarning("ID mismatch in edit request - URL ID: {UrlId}, Model ID: {ModelId} by {User} at {Time}",
                    id, viewModel.Id, _currentUser, _currentTime);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                LoadFormSuggestions(viewModel);
                LoadFormUsageInfo(viewModel);
                return View(viewModel);
            }

            try
            {
                _logger.LogInformation("Updating business operation form ID {Id} with name '{FormName}' by {User} at {Time}",
                    id, viewModel.FormName, _currentUser, _currentTime);

                // Check if form style exists
                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms");

                if (formStyle == null)
                {
                    _logger.LogWarning("Business operation form not found for update, ID: {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                // Store original name for logging
                var originalName = formStyle.FormName;

                // Check if name already exists (excluding current form)
                var existingForm = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.FormName.Trim().ToLower() == viewModel.FormName.Trim().ToLower() && f.Id != id);

                if (existingForm != null)
                {
                    _logger.LogWarning("Duplicate business operation form name attempted in edit: '{FormName}' by {User} at {Time}",
                        viewModel.FormName, _currentUser, _currentTime);
                    ModelState.AddModelError("FormName", "اسم العملية التجارية موجود بالفعل");
                    LoadFormSuggestions(viewModel);
                    LoadFormUsageInfo(viewModel);
                    return View(viewModel);
                }

                // Update the form name
                formStyle.FormName = viewModel.FormName.Trim();
                _unitOfWork.Repository<FormStyle>().Update(formStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("Business operation form updated successfully - ID: {Id}, Original: '{OriginalName}', New: '{NewName}' by {User} at {Time}",
                    id, originalName, formStyle.FormName, _currentUser, _currentTime);

                TempData["Success"] = "تم تحديث العملية التجارية بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث العملية التجارية");
                LoadFormSuggestions(viewModel);
                LoadFormUsageInfo(viewModel);
                return View(viewModel);
            }
        }

        // GET: FormStyles/Delete/5
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation("Loading delete confirmation for business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms,PackagingStyleForms.PackagingStyle");

                if (formStyle == null)
                {
                    _logger.LogWarning("Business operation form not found for delete, ID: {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                // Check if form is in use
                var isInUse = formStyle.PackagingStyleForms.Any();
                var usageCount = formStyle.PackagingStyleForms.Count;
                var relatedPackagingStyles = formStyle.PackagingStyleForms
                    .Select(psf => psf.PackagingStyle)
                    .ToList();

                var viewModel = new FormStyleDeleteViewModel
                {
                    Id = formStyle.Id,
                    FormName = formStyle.FormName,
                    FormCategory = GetFormCategory(formStyle.FormName),
                    IsInUse = isInUse,
                    UsageCount = usageCount,
                    RelatedPackagingStyles = relatedPackagingStyles
                };

                ViewBag.IsInUse = isInUse;
                ViewBag.UsageCount = usageCount;

                _logger.LogInformation("Loaded delete confirmation for business operation form '{FormName}' (ID: {Id}), Usage: {UsageCount} by {User} at {Time}",
                    formStyle.FormName, id, usageCount, _currentUser, _currentTime);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading delete confirmation for business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الحذف";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: FormStyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms");

                if (formStyle == null)
                {
                    _logger.LogWarning("Business operation form not found for deletion, ID: {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                // Store form name for logging
                var formName = formStyle.FormName;

                // Check if form is in use
                if (formStyle.PackagingStyleForms.Any())
                {
                    var usageCount = formStyle.PackagingStyleForms.Count;
                    _logger.LogWarning("Attempt to delete business operation form '{FormName}' (ID: {Id}) which is in use by {UsageCount} packaging styles by {User} at {Time}",
                        formName, id, usageCount, _currentUser, _currentTime);

                    TempData["Error"] = $"لا يمكن حذف العملية التجارية '{formName}' لأنها مستخدمة في {usageCount} من أنماط التعبئة";
                    return RedirectToAction(nameof(Index));
                }

                // Remove the form style
                _unitOfWork.Repository<FormStyle>().Remove(formStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("Business operation form '{FormName}' (ID: {Id}) deleted successfully by {User} at {Time}",
                    formName, id, _currentUser, _currentTime);

                TempData["Success"] = $"تم حذف العملية التجارية '{formName}' بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                TempData["Error"] = "حدث خطأ أثناء حذف العملية التجارية";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE API endpoint for AJAX calls
        [HttpDelete]
        [Route("/FormStyles/{id}")]
        public IActionResult DeleteApi(int id)
        {
            try
            {
                _logger.LogInformation("API: Attempting to delete business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == id, "PackagingStyleForms");

                if (formStyle == null)
                {
                    _logger.LogWarning("API: Business operation form not found for deletion, ID: {Id} by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound(new
                    {
                        success = false,
                        message = "العملية التجارية غير موجودة",
                        timestamp = _currentTime
                    });
                }

                var formName = formStyle.FormName;

                // Check if form is in use
                if (formStyle.PackagingStyleForms.Any())
                {
                    var usageCount = formStyle.PackagingStyleForms.Count;
                    _logger.LogWarning("API: Attempt to delete business operation form '{FormName}' (ID: {Id}) which is in use by {UsageCount} packaging styles by {User} at {Time}",
                        formName, id, usageCount, _currentUser, _currentTime);

                    return BadRequest(new
                    {
                        success = false,
                        message = $"لا يمكن حذف العملية التجارية '{formName}' لأنها مستخدمة في {usageCount} من أنماط التعبئة",
                        usageCount = usageCount,
                        timestamp = _currentTime
                    });
                }

                // Remove the form style
                _unitOfWork.Repository<FormStyle>().Remove(formStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("API: Business operation form '{FormName}' (ID: {Id}) deleted successfully by {User} at {Time}",
                    formName, id, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    message = $"تم حذف العملية التجارية '{formName}' بنجاح",
                    deletedFormName = formName,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error occurred while deleting business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء حذف العملية التجارية",
                    timestamp = _currentTime
                });
            }
        }

        // GET: FormStyles/CheckUsage/5
        [HttpGet]
        public IActionResult CheckUsage(int id)
        {
            try
            {
                _logger.LogInformation("Checking usage for business operation form ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyleForms = _unitOfWork.Repository<PackagingStyleForms>()
                    .GetAll(psf => psf.FormId == id, "PackagingStyle");

                if (!packagingStyleForms.Any())
                {
                    return Json(new
                    {
                        isUsed = false,
                        count = 0,
                        timestamp = _currentTime
                    });
                }

                var packagingStyles = packagingStyleForms
                    .Select(psf => new {
                        id = psf.PackagingStyleId,
                        name = psf.PackagingStyle.StyleName
                    })
                    .ToList();

                _logger.LogInformation("Business operation form ID {Id} is used by {Count} packaging styles by {User} at {Time}",
                    id, packagingStyles.Count, _currentUser, _currentTime);

                return Json(new
                {
                    isUsed = true,
                    count = packagingStyles.Count,
                    packagingStyles = packagingStyles,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking usage for business operation form ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    error = "حدث خطأ أثناء التحقق من استخدام العملية التجارية",
                    timestamp = _currentTime
                });
            }
        }

        #endregion

        #region API Endpoints for Smart Suggestions

        // API: GET - Get business operation form suggestions based on input
        [HttpGet]
        public JsonResult GetBusinessOperationSuggestions(string input)
        {
            try
            {
                _logger.LogInformation("Getting business operation suggestions for input '{Input}' by {User} at {Time}",
                    input, _currentUser, _currentTime);

                if (string.IsNullOrWhiteSpace(input))
                {
                    return Json(new { success = false, suggestions = new List<string>() });
                }

                input = input.Trim().ToLower();
                var suggestions = new List<BusinessOperationSuggestion>();

                // 1. Exact matches from business operations
                var exactMatches = _businessOperationForms
                    .Where(form => form.ToLower().Contains(input))
                    .Select(form => new BusinessOperationSuggestion
                    {
                        Name = form,
                        Type = "operation",
                        Category = GetFormCategory(form),
                        Confidence = form.ToLower() == input ? 100 : 85
                    })
                    .ToList();

                // 2. Similar matches from existing forms
                var existingForms = GetExistingFormNames();
                var existingMatches = existingForms
                    .Where(form => form.ToLower().Contains(input) ||
                                  CalculateLevenshteinDistance(form.ToLower(), input) <= 2)
                    .Select(form => new BusinessOperationSuggestion
                    {
                        Name = form,
                        Type = "existing",
                        Category = GetFormCategory(form),
                        Confidence = form.ToLower().Contains(input) ? 80 : 60
                    })
                    .ToList();

                // 3. Fuzzy matches for potential typos
                var fuzzyMatches = _businessOperationForms
                    .Where(form => !form.ToLower().Contains(input) &&
                                  CalculateLevenshteinDistance(form.ToLower(), input) <= 3)
                    .Select(form => new BusinessOperationSuggestion
                    {
                        Name = form,
                        Type = "fuzzy",
                        Category = GetFormCategory(form),
                        Confidence = 50
                    })
                    .ToList();

                // Combine and sort by confidence
                suggestions.AddRange(exactMatches);
                suggestions.AddRange(existingMatches);
                suggestions.AddRange(fuzzyMatches);

                var result = suggestions
                    .GroupBy(s => s.Name)
                    .Select(g => g.OrderByDescending(s => s.Confidence).First())
                    .OrderByDescending(s => s.Confidence)
                    .Take(10)
                    .ToList();

                _logger.LogInformation("Found {Count} business operation suggestions for input '{Input}' by {User} at {Time}",
                    result.Count, input, _currentUser, _currentTime);

                return Json(new { success = true, suggestions = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting business operation suggestions for '{Input}' by {User} at {Time}",
                    input, _currentUser, _currentTime);
                return Json(new { success = false, error = "حدث خطأ أثناء البحث عن الاقتراحات" });
            }
        }

        // API: GET - Get categorized business operation suggestions
        [HttpGet]
        public JsonResult GetCategorizedBusinessOperationSuggestions()
        {
            try
            {
                _logger.LogInformation("Getting categorized business operation suggestions by {User} at {Time}",
                    _currentUser, _currentTime);

                var categorizedSuggestions = new
                {
                    // Materials Operations (عمليات المواد)
                    materials = new
                    {
                        yarn = _businessOperationForms.Where(f => f.Contains("غزل")).OrderBy(f => f).ToList(),
                        fabric = _businessOperationForms.Where(f => f.Contains("قماش")).OrderBy(f => f).ToList(),
                        rawMaterials = _businessOperationForms.Where(f => f.Contains("مواد خام")).OrderBy(f => f).ToList(),
                        accessories = _businessOperationForms.Where(f => f.Contains("اكسسوارات")).OrderBy(f => f).ToList(),
                        dyes = _businessOperationForms.Where(f => f.Contains("اصباغ") || f.Contains("كيماويات")).OrderBy(f => f).ToList()
                    },

                    // Operation Types (أنواع العمليات)
                    operations = new
                    {
                        inbound = _businessOperationForms.Where(f => f.Contains("وارد")).OrderBy(f => f).ToList(),
                        outbound = _businessOperationForms.Where(f => f.Contains("صادر")).OrderBy(f => f).ToList(),
                        inventory = _businessOperationForms.Where(f => f.Contains("جرد") || f.Contains("مخزون")).OrderBy(f => f).ToList(),
                        quality = _businessOperationForms.Where(f => f.Contains("فحص") || f.Contains("جودة")).OrderBy(f => f).ToList(),
                        production = _businessOperationForms.Where(f => f.Contains("انتاج") || f.Contains("عملية")).OrderBy(f => f).ToList()
                    },

                    // Business Functions (الوظائف التجارية)
                    business = new
                    {
                        financial = _businessOperationForms.Where(f => f.Contains("فاتورة") || f.Contains("مستحقات") || f.Contains("مصروفات") || f.Contains("ايرادات")).OrderBy(f => f).ToList(),
                        warehouse = _businessOperationForms.Where(f => f.Contains("مخازن") || f.Contains("تخزين") || f.Contains("نقل")).OrderBy(f => f).ToList(),
                        administrative = _businessOperationForms.Where(f => f.Contains("موظف") || f.Contains("اجازة") || f.Contains("رواتب")).OrderBy(f => f).ToList(),
                        maintenance = _businessOperationForms.Where(f => f.Contains("صيانة") || f.Contains("معايرة") || f.Contains("اعطال")).OrderBy(f => f).ToList()
                    }
                };

                return Json(new { success = true, categories = categorizedSuggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categorized business operation suggestions by {User} at {Time}",
                    _currentUser, _currentTime);
                return Json(new { success = false, error = "حدث خطأ أثناء تحميل الاقتراحات" });
            }
        }

        // API: GET - Generate operation forms for specific material type
        [HttpGet]
        public JsonResult GenerateOperationFormsForMaterial(string materialType)
        {
            try
            {
                _logger.LogInformation("Generating operation forms for material type '{MaterialType}' by {User} at {Time}",
                    materialType, _currentUser, _currentTime);

                if (string.IsNullOrWhiteSpace(materialType))
                {
                    return Json(new { success = false, error = "نوع المادة غير محدد" });
                }

                var baseOperations = new List<string>
                {
                    "اضافة وارد", "اضافة صادر", "تعديل مخزون", "جرد", "تحويل",
                    "فحص جودة", "تصنيف", "تخزين", "اتلاف", "ارجاع"
                };

                var generatedForms = baseOperations
                    .Select(operation => $"{operation} {materialType}")
                    .ToList();

                return Json(new { success = true, forms = generatedForms, materialType = materialType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating operation forms for material type '{MaterialType}' by {User} at {Time}",
                    materialType, _currentUser, _currentTime);
                return Json(new { success = false, error = "حدث خطأ أثناء توليد النماذج" });
            }
        }

        #endregion

        #region Helper Methods

        private List<string> GetExistingFormNames()
        {
            return _unitOfWork.Repository<FormStyle>()
                .GetAll()
                .Select(f => f.FormName)
                .OrderBy(name => name)
                .ToList();
        }

        private string GetFormCategory(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return "غير محدد";

            formName = formName.ToLower();

            // Material categories
            if (formName.Contains("غزل")) return "عمليات الغزل";
            if (formName.Contains("قماش")) return "عمليات الأقمشة";
            if (formName.Contains("مواد خام")) return "عمليات المواد الخام";
            if (formName.Contains("منتجات")) return "عمليات المنتجات";
            if (formName.Contains("اكسسوارات")) return "عمليات الإكسسوارات";
            if (formName.Contains("اصباغ") || formName.Contains("كيماويات")) return "عمليات الأصباغ والكيماويات";

            // Operation categories
            if (formName.Contains("انتاج")) return "عمليات الإنتاج";
            if (formName.Contains("جودة") || formName.Contains("فحص")) return "عمليات مراقبة الجودة";
            if (formName.Contains("مخازن") || formName.Contains("تخزين")) return "عمليات المخازن";
            if (formName.Contains("فاتورة") || formName.Contains("مالي")) return "العمليات المالية";
            if (formName.Contains("موظف") || formName.Contains("اجازة")) return "العمليات الإدارية";
            if (formName.Contains("صيانة")) return "عمليات الصيانة";

            return "عمليات عامة";
        }

        private static int CalculateLevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1)) return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
            if (string.IsNullOrEmpty(s2)) return s1.Length;

            var distance = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++) distance[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) distance[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(
                        distance[i - 1, j] + 1,      // deletion
                        distance[i, j - 1] + 1),     // insertion
                        distance[i - 1, j - 1] + cost); // substitution
                }
            }

            return distance[s1.Length, s2.Length];
        }

        private void LoadFormSuggestions(FormStyleViewModel viewModel)
        {
            viewModel.BusinessOperationForms = _businessOperationForms.OrderBy(f => f).ToList();
            viewModel.ExistingFormNames = GetExistingFormNames();
        }

        private void LoadFormUsageInfo(FormStyleViewModel viewModel)
        {
            if (viewModel.Id > 0)
            {
                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetOne(f => f.Id == viewModel.Id, "PackagingStyleForms");

                if (formStyle != null)
                {
                    viewModel.IsInUse = formStyle.PackagingStyleForms.Any();
                    viewModel.UsageCount = formStyle.PackagingStyleForms.Count;
                }
            }
        }

        #endregion
    }

    // Helper class for business operation suggestions
    public class BusinessOperationSuggestion
    {
        public string Name { get; set; }
        public string Type { get; set; } // "operation", "existing", "fuzzy"
        public string Category { get; set; }
        public int Confidence { get; set; }
    }
}