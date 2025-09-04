using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.EL.Controllers
{
    public class FabricStudiosController : Controller
    {
        private readonly ILogger<FabricStudiosController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FabricStudiosController(ILogger<FabricStudiosController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: FabricStudios
        public IActionResult Index(string searchQuery = "", int? selectedItemId = null, int? selectedColorId = null,
                                  int? selectedDesignId = null, string statusFilter = "All", string typeFilter = "All",
                                  int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new FabricStudioIndexViewModel
                {
                    SearchQuery = searchQuery,
                    SelectedItemId = selectedItemId,
                    SelectedColorId = selectedColorId,
                    SelectedDesignId = selectedDesignId,
                    StatusFilter = statusFilter,
                    TypeFilter = typeFilter,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Get filter options
                viewModel.ItemsFilter = GetItemsForFilter();
                viewModel.ColorsFilter = GetColorsForFilter();
                viewModel.DesignsFilter = GetDesignsForFilter();

                // Build query
                var query = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(includeEntities: "Item,Color,Design")
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(f => f.Item.Item.Contains(searchQuery) ||
                                           f.Color.Color.Contains(searchQuery) ||
                                           f.Design.Design.Contains(searchQuery) ||
                                           f.Comment.Contains(searchQuery));
                }

                if (selectedItemId.HasValue)
                {
                    query = query.Where(f => f.ItemId == selectedItemId.Value);
                }

                if (selectedColorId.HasValue)
                {
                    query = query.Where(f => f.ColorId == selectedColorId.Value);
                }

                if (selectedDesignId.HasValue)
                {
                    query = query.Where(f => f.DesignId == selectedDesignId.Value);
                }

                if (statusFilter != "All")
                {
                    bool status = statusFilter == "Active";
                    query = query.Where(f => f.Status == status);
                }

                if (typeFilter != "All")
                {
                    if (typeFilter == "Color")
                    {
                        query = query.Where(f => f.ColorId.HasValue && !f.DesignId.HasValue);
                    }
                    else if (typeFilter == "Design")
                    {
                        query = query.Where(f => f.DesignId.HasValue && !f.ColorId.HasValue);
                    }
                }

                // Calculate statistics
                var allItems = query.ToList();
                viewModel.TotalItems = allItems.Count;
                viewModel.ActiveItemsCount = allItems.Count(f => f.Status);
                viewModel.InactiveItemsCount = allItems.Count(f => !f.Status);
                viewModel.ColorBasedCount = allItems.Count(f => f.ColorId.HasValue);
                viewModel.DesignBasedCount = allItems.Count(f => f.DesignId.HasValue);

                // Get paginated results
                viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalItems / pageSize);
                var fabricStudios = allItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FabricStudioViewModel
                    {
                        Id = f.Id,
                        ItemId = f.ItemId,
                        ItemName = f.Item?.Item ?? "غير محدد",
                        ColorId = f.ColorId,
                        ColorName = f.Color?.Color ?? "",
                        DesignId = f.DesignId,
                        DesignName = f.Design?.Design ?? "",
                        ImagePath = f.ImagePath,
                        WatermarkedImagePath = f.WatermarkedImagePath,
                        Status = f.Status,
                        Comment = f.Comment
                    })
                    .ToList();

                viewModel.FabricStudios = fabricStudios;

                _logger.LogInformation("Fabric studios index loaded by {User} at {Time} - Found {Count} studios",
                    "Ammar-Yasser8", "2025-09-02 19:20:14", viewModel.TotalItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric studios index by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                TempData["Error"] = "حدث خطأ أثناء تحميل قائمة استوديو القماش";
                return View(new FabricStudioIndexViewModel());
            }
        }

        // GET: FabricStudios/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new FabricStudioViewModel
                {
                    Status = true,
                    AvailableItems = GetActiveItems(),
                    AvailableColors = GetActiveColors(),
                    AvailableDesigns = GetActiveDesigns()
                };

                _logger.LogInformation("Fabric studio create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric studio create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج إضافة استوديو القماش";
                return RedirectToAction("Index");
            }
        }

        // GET: FabricStudios/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var fabricStudio = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(f => f.Id == id, includeEntities: "Item,Color,Design")
                    .FirstOrDefault();

                if (fabricStudio == null)
                {
                    TempData["Error"] = "استوديو القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricStudioViewModel
                {
                    Id = fabricStudio.Id,
                    ItemId = fabricStudio.ItemId,
                    ItemName = fabricStudio.Item?.Item ?? "غير محدد",
                    ColorId = fabricStudio.ColorId,
                    ColorName = fabricStudio.Color?.Color ?? "",
                    DesignId = fabricStudio.DesignId,
                    DesignName = fabricStudio.Design?.Design ?? "",
                    ImagePath = fabricStudio.ImagePath,
                    WatermarkedImagePath = fabricStudio.WatermarkedImagePath,
                    Status = fabricStudio.Status,
                    Comment = fabricStudio.Comment,
                    AvailableItems = GetActiveItems(),
                    AvailableColors = GetActiveColors(),
                    AvailableDesigns = GetActiveDesigns()
                };

                _logger.LogInformation("Fabric studio {Id} edit form loaded by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric studio {Id} edit form by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج تعديل استوديو القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricStudios/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(FabricStudioViewModel model)
        {
            try
            {
                // Remove server-side populated fields from validation
                ModelState.Remove("ItemName");
                ModelState.Remove("ColorName");
                ModelState.Remove("DesignName");
                ModelState.Remove("ImagePath");
                ModelState.Remove("WatermarkedImagePath");

                // Custom validation: either Color OR Design must be selected, not both
                if ((!model.ColorId.HasValue && !model.DesignId.HasValue) ||
                    (model.ColorId.HasValue && model.DesignId.HasValue))
                {
                    ModelState.AddModelError("", "يجب اختيار إما اللون أو التصميم، وليس كلاهما");
                }

                // Validate image is required when design is selected
                if (model.DesignId.HasValue && model.ImageFile == null && string.IsNullOrEmpty(model.ImagePath))
                {
                    ModelState.AddModelError("ImageFile", "البوستر مطلوب عند اختيار التصميم");
                }

                // Validate watermarked image is always required
                if (model.WatermarkedImageFile == null && string.IsNullOrEmpty(model.WatermarkedImagePath))
                {
                    ModelState.AddModelError("WatermarkedImageFile", "الصورة مطلوبة");
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveItems();
                    model.AvailableColors = GetActiveColors();
                    model.AvailableDesigns = GetActiveDesigns();
                    return View("CreateEdit", model);
                }

                // Check for unique constraints
                var duplicateCheck = CheckForDuplicates(model);
                if (!duplicateCheck.IsValid)
                {
                    ModelState.AddModelError("", duplicateCheck.ErrorMessage);
                    model.AvailableItems = GetActiveItems();
                    model.AvailableColors = GetActiveColors();
                    model.AvailableDesigns = GetActiveDesigns();
                    return View("CreateEdit", model);
                }

                // Verify item exists and is active
                var item = _unitOfWork.Repository<FabricItem>().GetOne(f=>f.Id==model.ItemId.Value);
                if (item == null || !item.Status)
                {
                    ModelState.AddModelError("ItemId", "الصنف المحدد غير موجود أو غير نشط");
                    model.AvailableItems = GetActiveItems();
                    model.AvailableColors = GetActiveColors();
                    model.AvailableDesigns = GetActiveDesigns();
                    return View("CreateEdit", model);
                }

                // Handle file uploads
                var imageUploadResult = HandleImageUploads(model);
                if (!imageUploadResult.IsSuccess)
                {
                    ModelState.AddModelError("", imageUploadResult.ErrorMessage);
                    model.AvailableItems = GetActiveItems();
                    model.AvailableColors = GetActiveColors();
                    model.AvailableDesigns = GetActiveDesigns();
                    return View("CreateEdit", model);
                }

                FabricStudio fabricStudio;
                bool isEdit = model.Id > 0;

                if (isEdit)
                {
                    // Update existing studio
                    fabricStudio = _unitOfWork.Repository<FabricStudio>().GetOne(f=>f.Id==model.Id);
                    if (fabricStudio == null)
                    {
                        TempData["Error"] = "استوديو القماش غير موجود";
                        return RedirectToAction("Index");
                    }

                    // Delete old images if new ones are uploaded
                    if (model.ImageFile != null && !string.IsNullOrEmpty(fabricStudio.ImagePath))
                    {
                        DeleteImageFile(fabricStudio.ImagePath);
                    }
                    if (model.WatermarkedImageFile != null && !string.IsNullOrEmpty(fabricStudio.WatermarkedImagePath))
                    {
                        DeleteImageFile(fabricStudio.WatermarkedImagePath);
                    }

                    fabricStudio.ItemId = model.ItemId.Value;
                    fabricStudio.ColorId = model.ColorId;
                    fabricStudio.DesignId = model.DesignId;
                    fabricStudio.ImagePath = imageUploadResult.ImagePath ?? fabricStudio.ImagePath;
                    fabricStudio.WatermarkedImagePath = imageUploadResult.WatermarkedImagePath ?? fabricStudio.WatermarkedImagePath;
                    fabricStudio.Status = model.Status;
                    fabricStudio.Comment = model.Comment?.Trim();

                    _unitOfWork.Repository<FabricStudio>().Update(fabricStudio);
                }
                else
                {
                    // Create new studio
                    fabricStudio = new FabricStudio
                    {
                        ItemId = model.ItemId.Value,
                        ColorId = model.ColorId,
                        DesignId = model.DesignId,
                        ImagePath = imageUploadResult.ImagePath,
                        WatermarkedImagePath = imageUploadResult.WatermarkedImagePath,
                        Status = model.Status,
                        Comment = model.Comment?.Trim(),
                    
                    };

                    _unitOfWork.Repository<FabricStudio>().Add(fabricStudio);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Fabric studio {Action} successfully - ID: {Id}, Item: {ItemId}, Type: {Type} by {User} at {Time}",
                    isEdit ? "updated" : "created", fabricStudio.Id, fabricStudio.ItemId,
                    fabricStudio.StudioType, "Ammar-Yasser8", "2025-09-02 19:20:14");

                TempData["Success"] = $"تم {(isEdit ? "تحديث" : "إضافة")} استوديو القماش بنجاح";
                return RedirectToAction("Details", new { id = fabricStudio.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fabric studio by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ استوديو القماش");
                model.AvailableItems = GetActiveItems();
                model.AvailableColors = GetActiveColors();
                model.AvailableDesigns = GetActiveDesigns();
                return View("CreateEdit", model);
            }
        }

        // GET: FabricStudios/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var fabricStudio = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(f => f.Id == id, includeEntities: "Item,Item.OriginRaw,Color,Color.Style,Design,Design.Style")
                    .FirstOrDefault();

                if (fabricStudio == null)
                {
                    TempData["Error"] = "استوديو القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricStudioDetailsViewModel
                {
                    Id = fabricStudio.Id,
                    ItemId = fabricStudio.ItemId,
                    ItemName = fabricStudio.Item?.Item ?? "غير محدد",
                    OriginRawName = fabricStudio.Item?.OriginRaw?.Item ?? "غير محدد",
                    ColorId = fabricStudio.ColorId,
                    ColorName = fabricStudio.Color?.Color ?? "",
                    ColorStyleName = fabricStudio.Color?.Style?.Style ?? "",
                    DesignId = fabricStudio.DesignId,
                    DesignName = fabricStudio.Design?.Design ?? "",
                    DesignStyleName = fabricStudio.Design?.Style?.Style ?? "",
                    ImagePath = fabricStudio.ImagePath,
                    WatermarkedImagePath = fabricStudio.WatermarkedImagePath,
                    Status = fabricStudio.Status,
                    Comment = fabricStudio.Comment,
                    StudioType = fabricStudio.StudioType,
                    DisplayName = fabricStudio.DisplayName
                };

                _logger.LogInformation("Fabric studio {Id} details viewed by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric studio {Id} details by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");
                TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل استوديو القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricStudios/ToggleStatus/5
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var fabricStudio = _unitOfWork.Repository<FabricStudio>().GetOne(f=> f.Id == id);
                if (fabricStudio == null)
                {
                    return Json(new { success = false, message = "استوديو القماش غير موجود" });
                }

                fabricStudio.Status = !fabricStudio.Status;
                _unitOfWork.Repository<FabricStudio>().Update(fabricStudio);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric studio {Id} status toggled to {Status} by {User} at {Time}",
                    id, fabricStudio.Status, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return Json(new
                {
                    success = true,
                    newStatus = fabricStudio.Status,
                    statusText = fabricStudio.StatusText,
                    statusClass = fabricStudio.StatusClass,
                    statusIcon = fabricStudio.StatusIcon,
                    message = $"تم تحديث حالة استوديو القماش إلى '{fabricStudio.StatusText}'"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling fabric studio {Id} status by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الحالة" });
            }
        }

        // POST: FabricStudios/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var fabricStudio = _unitOfWork.Repository<FabricStudio>().GetOne(f => f.Id == id);
                if (fabricStudio == null)
                {
                    return Json(new { success = false, message = "استوديو القماش غير موجود" });
                }

                // Delete associated images
                if (!string.IsNullOrEmpty(fabricStudio.ImagePath))
                {
                    DeleteImageFile(fabricStudio.ImagePath);
                }
                if (!string.IsNullOrEmpty(fabricStudio.WatermarkedImagePath))
                {
                    DeleteImageFile(fabricStudio.WatermarkedImagePath);
                }

                _unitOfWork.Repository<FabricStudio>().Remove(fabricStudio);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric studio {Id} deleted by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return Json(new
                {
                    success = true,
                    message = "تم حذف استوديو القماش بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fabric studio {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:20:14");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف استوديو القماش" });
            }
        }

        // GET: FabricStudios/Extract
        public IActionResult Extract(int itemId)
        {
            try
            {
                var item = _unitOfWork.Repository<FabricItem>()
                    .GetAll(i => i.Id == itemId && i.Status, includeEntities: "OriginRaw")
                    .FirstOrDefault();

                if (item == null)
                {
                    TempData["Error"] = "الصنف غير موجود أو غير نشط";
                    return RedirectToAction("Index");
                }

                var studios = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(s => s.ItemId == itemId && s.Status, includeEntities: "Color,Design")
                    .OrderBy(s => s.Color != null ? s.Color.Color : s.Design.Design)
                    .ToList();

                var viewModel = new FabricStudioExtractViewModel
                {
                    ItemId = itemId,
                    ItemName = item.Item,
                    OriginRawName = item.OriginRaw?.Item ?? "غير محدد",
                    Studios = studios.Select(s => new FabricStudioSummaryDto
                    {
                        Id = s.Id,
                        ColorName = s.Color?.Color,
                        DesignName = s.Design?.Design,
                        ImagePath = s.ImagePath,
                        WatermarkedImagePath = s.WatermarkedImagePath,
                        StudioType = s.StudioType,
                        DisplayName = s.DisplayName
                    }).ToList()
                };

                _logger.LogInformation("Fabric studio extraction for item {ItemId} viewed by {User} at {Time}",
                    itemId, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting fabric studios for item {ItemId} by {User} at {Time}",
                    itemId, "Ammar-Yasser8", "2025-09-02 19:20:14");
                TempData["Error"] = "حدث خطأ أثناء استخراج بيانات استوديو القماش";
                return RedirectToAction("Index");
            }
        }

        #region Helper Methods

        private List<SelectListItem> GetActiveItems()
        {
            try
            {
                return _unitOfWork.Repository<FabricItem>()
                    .GetAll(i => i.Status, includeEntities: "OriginRaw")
                    .Select(i => new SelectListItem
                    {
                        Value = i.Id.ToString(),
                        Text = $"{i.Item} ({i.OriginRaw?.Item})"
                    })
                    .OrderBy(i => i.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active items by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetActiveColors()
        {
            try
            {
                return _unitOfWork.Repository<FabricColor>()
                    .GetAll(includeEntities: "Style")
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Color} ({c.Style?.Style})"
                    })
                    .OrderBy(c => c.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active colors by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetActiveDesigns()
        {
            try
            {
                return _unitOfWork.Repository<FabricDesign>()
                    .GetAll(includeEntities: "Style")
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Design} ({d.Style?.Style})"
                    })
                    .OrderBy(d => d.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active designs by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetItemsForFilter()
        {
            try
            {
                return _unitOfWork.Repository<FabricItem>()
                    .GetAll(i => i.Status)
                    .Select(i => new SelectListItem
                    {
                        Value = i.Id.ToString(),
                        Text = i.Item
                    })
                    .OrderBy(i => i.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetColorsForFilter()
        {
            try
            {
                return _unitOfWork.Repository<FabricColor>()
                    .GetAll()
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Color
                    })
                    .OrderBy(c => c.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting colors for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetDesignsForFilter()
        {
            try
            {
                return _unitOfWork.Repository<FabricDesign>()
                    .GetAll()
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Design
                    })
                    .OrderBy(d => d.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting designs for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return new List<SelectListItem>();
            }
        }

        private (bool IsValid, string ErrorMessage) CheckForDuplicates(FabricStudioViewModel model)
        {
            try
            {
                if (model.ColorId.HasValue)
                {
                    // Check Item + Color uniqueness
                    var existingColorStudio = _unitOfWork.Repository<FabricStudio>()
                        .GetAll(s => s.ItemId == model.ItemId && s.ColorId == model.ColorId && s.Id != model.Id)
                        .FirstOrDefault();

                    if (existingColorStudio != null)
                    {
                        return (false, "يوجد بالفعل استوديو لهذا الصنف مع نفس اللون");
                    }
                }

                if (model.DesignId.HasValue)
                {
                    // Check Item + Design uniqueness
                    var existingDesignStudio = _unitOfWork.Repository<FabricStudio>()
                        .GetAll(s => s.ItemId == model.ItemId && s.DesignId == model.DesignId && s.Id != model.Id)
                        .FirstOrDefault();

                    if (existingDesignStudio != null)
                    {
                        return (false, "يوجد بالفعل استوديو لهذا الصنف مع نفس التصميم");
                    }
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for duplicates by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return (false, "حدث خطأ أثناء التحقق من التكرار");
            }
        }

        private (bool IsSuccess, string ImagePath, string WatermarkedImagePath, string ErrorMessage) HandleImageUploads(FabricStudioViewModel model)
        {
            try
            {
                string imagePath = model.ImagePath;
                string watermarkedImagePath = model.WatermarkedImagePath;

                // Handle image upload (required only for designs)
                if (model.ImageFile != null)
                {
                    var imageResult = SaveUploadedFile(model.ImageFile, "fabric-images");
                    if (!imageResult.IsSuccess)
                    {
                        return (false, null, null, imageResult.ErrorMessage);
                    }
                    imagePath = imageResult.FilePath;
                }

                // Handle watermarked image upload (always required)
                if (model.WatermarkedImageFile != null)
                {
                    var watermarkedResult = SaveUploadedFile(model.WatermarkedImageFile, "fabric-watermarked");
                    if (!watermarkedResult.IsSuccess)
                    {
                        return (false, null, null, watermarkedResult.ErrorMessage);
                    }
                    watermarkedImagePath = watermarkedResult.FilePath;
                }

                return (true, imagePath, watermarkedImagePath, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling image uploads by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return (false, null, null, "حدث خطأ أثناء رفع الصور");
            }
        }

        private (bool IsSuccess, string FilePath, string ErrorMessage) SaveUploadedFile(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return (false, null, "الملف غير صالح");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return (false, null, "نوع الملف غير مدعوم. الأنواع المدعومة: JPG, PNG, GIF, BMP");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return (false, null, "حجم الملف يجب ألا يتجاوز 5 ميجابايت");
                }

                // Create upload directory
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                var relativePath = $"/uploads/{folder}/{uniqueFileName}";

                _logger.LogInformation("File uploaded successfully: {FilePath} by {User} at {Time}",
                    relativePath, "Ammar-Yasser8", "2025-09-02 19:20:14");

                return (true, relativePath, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving uploaded file by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:20:14");
                return (false, null, "حدث خطأ أثناء حفظ الملف");
            }
        }

        private void DeleteImageFile(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return;

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Image file deleted: {FilePath} by {User} at {Time}",
                        imagePath, "Ammar-Yasser8", "2025-09-02 19:20:14");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting image file {FilePath} by {User} at {Time}",
                    imagePath, "Ammar-Yasser8", "2025-09-02 19:20:14");
            }
        }

        #endregion
    }
}