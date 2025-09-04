using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.EL.Controllers
{
    public class FabricColorsController : Controller
    {
        private readonly ILogger<FabricColorsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FabricColorsController(ILogger<FabricColorsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: FabricColors
        public IActionResult Index(string searchQuery = "", int? selectedStyleId = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new FabricColorIndexViewModel
                {
                    SearchQuery = searchQuery,
                    SelectedStyleId = selectedStyleId,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Get filter options
                viewModel.StylesFilter = GetStylesForFilter();

                // Build query
                var query = _unitOfWork.Repository<FabricColor>()
                    .GetAll(includeEntities: "Style")
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(f => f.Color.Contains(searchQuery) ||
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
                var fabricColors = allItems
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FabricColorViewModel
                    {
                        Id = f.Id,
                        Color = f.Color,
                        StyleId = f.StyleId,
                        StyleName = f.Style?.Style ?? "غير محدد",
                        Comment = f.Comment
                    })
                    .ToList();

                viewModel.FabricColors = fabricColors;

                _logger.LogInformation("Fabric colors index loaded by {User} at {Time} - Found {Count} colors",
                    "Ammar-Yasser8", "2025-09-02 19:18:10", viewModel.TotalItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric colors index by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل قائمة ألوان القماش";
                return View(new FabricColorIndexViewModel());
            }
        }

        // GET: FabricColors/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new FabricColorViewModel
                {
                    AvailableStyles = GetAvailableStyles()
                };

                _logger.LogInformation("Fabric color create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric color create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج إضافة لون القماش";
                return RedirectToAction("Index");
            }
        }

        // GET: FabricColors/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var fabricColor = _unitOfWork.Repository<FabricColor>()
                    .GetAll(f => f.Id == id, includeEntities: "Style")
                    .FirstOrDefault();

                if (fabricColor == null)
                {
                    TempData["Error"] = "لون القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricColorViewModel
                {
                    Id = fabricColor.Id,
                    Color = fabricColor.Color,
                    StyleId = fabricColor.StyleId,
                    StyleName = fabricColor.Style?.Style ?? "غير محدد",
                    Comment = fabricColor.Comment,
                    AvailableStyles = GetAvailableStyles()
                };

                _logger.LogInformation("Fabric color {Id} edit form loaded by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric color {Id} edit form by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج تعديل لون القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricColors/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(FabricColorViewModel model)
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

                // Check for duplicate color name (excluding current item if editing)
                var existingColor = _unitOfWork.Repository<FabricColor>()
                    .GetAll(f => f.Color == model.Color && f.Id != model.Id)
                    .FirstOrDefault();

                if (existingColor != null)
                {
                    ModelState.AddModelError("Color", "اسم اللون موجود بالفعل");
                    model.AvailableStyles = GetAvailableStyles();
                    return View("CreateEdit", model);
                }

                // Verify style exists
                var style = _unitOfWork.Repository<FabricStyle>().GetOne(f => f.Id == model.StyleId);
                if (style == null)
                {
                    ModelState.AddModelError("StyleId", "الطراز المحدد غير موجود");
                    model.AvailableStyles = GetAvailableStyles();
                    return View("CreateEdit", model);
                }

                FabricColor fabricColor;
                bool isEdit = model.Id > 0;

                if (isEdit)
                {
                    // Update existing color
                    fabricColor = _unitOfWork.Repository<FabricColor>().GetOne(f => f.Id == model.StyleId);
                    if (fabricColor == null)
                    {
                        TempData["Error"] = "لون القماش غير موجود";
                        return RedirectToAction("Index");
                    }

                    fabricColor.Color = model.Color.Trim();
                    fabricColor.StyleId = model.StyleId.Value;
                    fabricColor.Comment = model.Comment?.Trim();
                   

                    _unitOfWork.Repository<FabricColor>().Update(fabricColor);
                }
                else
                {
                    // Create new color
                    fabricColor = new FabricColor
                    {
                        Color = model.Color.Trim(),
                        StyleId = model.StyleId.Value,
                        Comment = model.Comment?.Trim(),
                       
                    };

                    _unitOfWork.Repository<FabricColor>().Add(fabricColor);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Fabric color {Action} successfully - ID: {Id}, Name: {Name} by {User} at {Time}",
                    isEdit ? "updated" : "created", fabricColor.Id, fabricColor.Color, "Ammar-Yasser8", "2025-09-02 19:18:10");

                TempData["Success"] = $"تم {(isEdit ? "تحديث" : "إضافة")} لون القماش '{fabricColor.Color}' بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fabric color by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ لون القماش");
                model.AvailableStyles = GetAvailableStyles();
                return View("CreateEdit", model);
            }
        }

        // POST: FabricColors/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var fabricColor = _unitOfWork.Repository<FabricColor>().GetOne(f => f.Id == id);
                if (fabricColor == null)
                {
                    return Json(new { success = false, message = "لون القماش غير موجود" });
                }

                // Check if color is used in fabric studios
                var studiosCount = _unitOfWork.Repository<FabricStudio>()
                    .GetAll(s => s.ColorId == id).Count();

                if (studiosCount > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"لا يمكن حذف اللون لأنه مستخدم في {studiosCount} استوديو"
                    });
                }

                _unitOfWork.Repository<FabricColor>().Remove(fabricColor);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric color {Id} ({Name}) deleted by {User} at {Time}",
                    id, fabricColor.Color, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return Json(new
                {
                    success = true,
                    message = $"تم حذف لون القماش '{fabricColor.Color}' بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fabric color {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف لون القماش" });
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