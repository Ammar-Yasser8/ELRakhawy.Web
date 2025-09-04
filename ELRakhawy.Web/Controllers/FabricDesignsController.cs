using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.EL.Controllers
{
    public class FabricDesignsController : Controller
    {
        private readonly ILogger<FabricDesignsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FabricDesignsController(ILogger<FabricDesignsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: FabricDesigns
        public IActionResult Index(string searchQuery = "", int? selectedStyleId = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new FabricDesignIndexViewModel
                {
                    SearchQuery = searchQuery,
                    SelectedStyleId = selectedStyleId,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Get filter options
                viewModel.StylesFilter = GetStylesForFilter();

                // Build query
                var query = _unitOfWork.Repository<FabricDesign>()
                    .GetAll(includeEntities: "Style")
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(f => f.Design.Contains(searchQuery) ||
                                           f.Comment.Contains(searchQuery) ||
                                           f.Style.Style.Contains(searchQuery));
                }

                if (selectedStyleId.HasValue)
                {
                    query = query.Where(f => f.StyleId == selectedStyleId.Value);
                }

                // Calculate pagination
                var allItems = query.ToList();
                viewModel.TotalItems = allItems.Count;
                viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalItems / pageSize);

                // Get paginated results
                var fabricDesigns = allItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FabricDesignViewModel
                    {
                        Id = f.Id,
                        Design = f.Design,
                        StyleId = f.StyleId,
                        StyleName = f.Style?.Style ?? "غير محدد",
                        Comment = f.Comment
                    })
                    .ToList();

                viewModel.FabricDesigns = fabricDesigns;

                _logger.LogInformation("Fabric designs index loaded by {User} at {Time} - Found {Count} designs",
                    "Ammar-Yasser8", "2025-09-02 19:18:10", viewModel.TotalItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric designs index by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل قائمة تصاميم القماش";
                return View(new FabricDesignIndexViewModel());
            }
        }

        // GET: FabricDesigns/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new FabricDesignViewModel
                {
                    Design = "مطبوع", // Default value as mentioned in requirements
                    AvailableStyles = GetAvailableStyles()
                };

                _logger.LogInformation("Fabric design create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric design create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج إضافة تصميم القماش";
                return RedirectToAction("Index");
            }
        }

        // GET: FabricDesigns/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var fabricDesign = _unitOfWork.Repository<FabricDesign>()
                    .GetAll(f => f.Id == id, includeEntities: "Style")
                    .FirstOrDefault();

                if (fabricDesign == null)
                {
                    TempData["Error"] = "تصميم القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricDesignViewModel
                {
                    Id = fabricDesign.Id,
                    Design = fabricDesign.Design,
                    StyleId = fabricDesign.StyleId,
                    StyleName = fabricDesign.Style?.Style ?? "غير محدد",
                    Comment = fabricDesign.Comment,
                    AvailableStyles = GetAvailableStyles()
                };

                _logger.LogInformation("Fabric design {Id} edit form loaded by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric design {Id} edit form by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج تعديل تصميم القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricDesigns/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(FabricDesignViewModel model)
        {
            try
            {
                // Remove server-side populated fields from validation
                ModelState.Remove("StyleName");

                if (!ModelState.IsValid)
                {
                    model.AvailableStyles = GetAvailableStyles();
                    return View("CreateEdit", model);
                }

                // Check for duplicate design name (excluding current item if editing)
                var existingDesign = _unitOfWork.Repository<FabricDesign>()
                    .GetAll(f => f.Design == model.Design && f.Id != model.Id)
                    .FirstOrDefault();

                if (existingDesign != null)
                {
                    ModelState.AddModelError("Design", "اسم التصميم موجود بالفعل");
                    model.AvailableStyles = GetAvailableStyles();
                    return View("CreateEdit", model);
                }
                // Verify style exists
                var style = _unitOfWork.Repository<FabricStyle>().GetOne(f=>f.Id==model.StyleId.Value);
                if (style == null)
                {
                    ModelState.AddModelError("StyleId", "الطراز المحدد غير موجود");
                    model.AvailableStyles = GetAvailableStyles();
                    return View("CreateEdit", model);
                }

                FabricDesign fabricDesign;
                bool isEdit = model.Id > 0;

                if (isEdit)
                {
                    // Update existing design
                    fabricDesign = _unitOfWork.Repository<FabricDesign>().GetOne(f=>f.Id==model.Id);
                    if (fabricDesign == null)
                    {
                        TempData["Error"] = "تصميم القماش غير موجود";
                        return RedirectToAction("Index");
                    }

                    fabricDesign.Design = model.Design.Trim();
                    fabricDesign.StyleId = model.StyleId.Value;
                    fabricDesign.Comment = model.Comment?.Trim();

                    _unitOfWork.Repository<FabricDesign>().Update(fabricDesign);
                }
                else
                {
                    // Create new design
                    fabricDesign = new FabricDesign
                    {
                        Design = model.Design.Trim(),
                        StyleId = model.StyleId.Value,
                        Comment = model.Comment?.Trim(),
                    };

                    _unitOfWork.Repository<FabricDesign>().Add(fabricDesign);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Fabric design {Action} successfully - ID: {Id}, Name: {Name} by {User} at {Time}",
                    isEdit ? "updated" : "created", fabricDesign.Id, fabricDesign.Design, "Ammar-Yasser8", "2025-09-02 19:18:10");

                TempData["Success"] = $"تم {(isEdit ? "تحديث" : "إضافة")} تصميم القماش '{fabricDesign.Design}' بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fabric design by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ تصميم القماش");
                model.AvailableStyles = GetAvailableStyles();
                return View("CreateEdit", model);
            }
        }

        // POST: FabricDesigns/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var fabricDesign = _unitOfWork.Repository<FabricDesign>().GetOne(f => f.Id == id);
                if (fabricDesign == null)
                {
                    return Json(new { success = false, message = "تصميم القماش غير موجود" });
                }

                // Check if design is used in fabric studios
                var studiosCount = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(s => s.DesignId == id).Count();

                if (studiosCount > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"لا يمكن حذف التصميم لأنه مستخدم في {studiosCount} استوديو"
                    });
                }

                _unitOfWork.Repository<FabricDesign>().Remove(fabricDesign);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric design {Id} ({Name}) deleted by {User} at {Time}",
                    id, fabricDesign.Design, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return Json(new
                {
                    success = true,
                    message = $"تم حذف تصميم القماش '{fabricDesign.Design}' بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fabric design {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف تصميم القماش" });
            }
        }

        #region Helper Methods

        private List<SelectListItem> GetAvailableStyles()
        {
            try
            {
                return _unitOfWork.Repository<FabricStyle>()
                    .GetAll()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Style
                    })
                    .OrderBy(s => s.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available styles by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetStylesForFilter()
        {
            try
            {
                return _unitOfWork.Repository<FabricStyle>()
                    .GetAll()
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Style
                    })
                    .OrderBy(s => s.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting styles for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                return new List<SelectListItem>();
            }
        }

        #endregion
    }
}