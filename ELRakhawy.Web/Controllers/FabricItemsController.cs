using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.Web.Controllers
{
    public class FabricItemsController : Controller
    {
        private readonly ILogger<FabricItemsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FabricItemsController(ILogger<FabricItemsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: FabricItems
        public IActionResult Index(string searchQuery = "", int? selectedRawItemId = null,
                                  string statusFilter = "All", int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new FabricItemIndexViewModel
                {
                    SearchQuery = searchQuery,
                    SelectedRawItemId = selectedRawItemId,
                    StatusFilter = statusFilter,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Get filter options
                viewModel.RawItemsFilter = GetRawItemsForFilter();

                // Build query
                var query = _unitOfWork.Repository<FabricItem>()
                    .GetAll(includeEntities: "OriginRaw,OriginRaw.Warp,OriginRaw.Weft")
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(f => f.Item.Contains(searchQuery) ||
                                           f.Comment.Contains(searchQuery) ||
                                           f.OriginRaw.Item.Contains(searchQuery));
                }

                if (selectedRawItemId.HasValue)
                {
                    query = query.Where(f => f.OriginRawId == selectedRawItemId.Value);
                }

                if (statusFilter != "All")
                {
                    bool status = statusFilter == "Active";
                    query = query.Where(f => f.Status == status);
                }

                // Calculate statistics
                var allItems = query.ToList();
                viewModel.TotalItems = allItems.Count;
                viewModel.ActiveItemsCount = allItems.Count(f => f.Status);
                viewModel.InactiveItemsCount = allItems.Count(f => !f.Status);
                viewModel.TotalRawItemsUsed = allItems.Select(f => f.OriginRawId).Distinct().Count();

                // Get paginated results
                viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalItems / pageSize);
                var fabricItems = allItems
                    
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FabricItemViewModel
                    {
                        Id = f.Id,
                        Item = f.Item,
                        OriginRawId = f.OriginRawId,
                        OriginRawName = f.OriginRaw != null ?
                            $"{f.OriginRaw.Item} (سداء: {f.OriginRaw.Warp?.Item}, لحمة: {f.OriginRaw.Weft?.Item})" :
                            "غير محدد",
                        Status = f.Status,
                        Comment = f.Comment
                    })
                    .ToList();

                viewModel.FabricItems = fabricItems;

                _logger.LogInformation("Fabric items index loaded by {User} at {Time} - Found {Count} items",
                    "Ammar-Yasser8", "2025-09-02 17:23:45", viewModel.TotalItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric items index by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");
                TempData["Error"] = "حدث خطأ أثناء تحميل قائمة أصناف القماش";
                return View(new FabricItemIndexViewModel());
            }
        }

        // GET: FabricItems/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new FabricItemViewModel
                {
                    Status = true,
                    AvailableRawItems = GetActiveRawItems()
                };

                _logger.LogInformation("Fabric item create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric item create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج إضافة صنف القماش";
                return RedirectToAction("Index");
            }
        }

        // GET: FabricItems/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var fabricItem = _unitOfWork.Repository<FabricItem>()
                    .GetAll(f => f.Id == id, includeEntities: "OriginRaw,OriginRaw.Warp,OriginRaw.Weft")
                    .FirstOrDefault();

                if (fabricItem == null)
                {
                    TempData["Error"] = "صنف القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricItemViewModel
                {
                    Id = fabricItem.Id,
                    Item = fabricItem.Item,
                    OriginRawId = fabricItem.OriginRawId,
                    OriginRawName = fabricItem.OriginRaw != null ?
                        $"{fabricItem.OriginRaw.Item} (سداء: {fabricItem.OriginRaw.Warp?.Item}, لحمة: {fabricItem.OriginRaw.Weft?.Item})" :
                        "غير محدد",
                    Status = fabricItem.Status,
                    Comment = fabricItem.Comment,
                    AvailableRawItems = GetActiveRawItems()
                };

                _logger.LogInformation("Fabric item {Id} edit form loaded by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 17:23:45");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric item {Id} edit form by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 17:23:45");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج تعديل صنف القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricItems/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(FabricItemViewModel model)
        {
            try
            {
                // Remove server-side populated fields from validation
                ModelState.Remove("OriginRawName");

                if (!ModelState.IsValid)
                {
                    model.AvailableRawItems = GetActiveRawItems();
                    return View("CreateEdit", model);
                }

                // Check for duplicate item name (excluding current item if editing)
                var existingItem = _unitOfWork.Repository<FabricItem>()
                    .GetAll(f => f.Item == model.Item && f.Id != model.Id)
                    .FirstOrDefault();

                if (existingItem != null)
                {
                    ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
                    model.AvailableRawItems = GetActiveRawItems();
                    return View("CreateEdit", model);
                }

                var originRaw = _unitOfWork.Repository<RawItem>()
                    .GetOne(r => r.Id == model.OriginRawId!.Value);

                if (originRaw == null || !originRaw.Status)
                {
                    ModelState.AddModelError("OriginRawId", "الخام المكون المحدد غير موجود أو غير نشط");
                    model.AvailableRawItems = GetActiveRawItems();
                    return View("CreateEdit", model);
                }

                FabricItem fabricItem;
                bool isEdit = model.Id > 0;

                if (isEdit)
                {
                    // Update existing item
                    fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(f => f.Id == model.Id);
                    if (fabricItem == null) if (fabricItem == null)
                    {
                        TempData["Error"] = "صنف القماش غير موجود";
                        return RedirectToAction("Index");
                    }

                    fabricItem.Item = model.Item.Trim();
                    fabricItem.OriginRawId = model.OriginRawId.Value;
                    fabricItem.Status = model.Status;
                    fabricItem.Comment = model.Comment?.Trim();

                    _unitOfWork.Repository<FabricItem>().Update(fabricItem);
                }
                else
                {
                    // Create new item
                    fabricItem = new FabricItem
                    {
                        Item = model.Item.Trim(),
                        OriginRawId = model.OriginRawId.Value,
                        Status = model.Status,
                        Comment = model.Comment?.Trim(),
                       
                    };

                    _unitOfWork.Repository<FabricItem>().Add(fabricItem);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Fabric item {Action} successfully - ID: {Id}, Name: {Name} by {User} at {Time}",
                    isEdit ? "updated" : "created", fabricItem.Id, fabricItem.Item, "Ammar-Yasser8", "2025-09-02 17:23:45");

                TempData["Success"] = $"تم {(isEdit ? "تحديث" : "إضافة")} صنف القماش '{fabricItem.Item}' بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fabric item by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ صنف القماش");
                model.AvailableRawItems = GetActiveRawItems();
                return View("CreateEdit", model);
            }
        }

        // POST: FabricItems/ToggleStatus/5
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(f => f.Id == id);
                if (fabricItem == null)
                {
                    return Json(new { success = false, message = "صنف القماش غير موجود" });
                }

                fabricItem.Status = !fabricItem.Status;
                _unitOfWork.Repository<FabricItem>().Update(fabricItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric item {Id} status toggled to {Status} by {User} at {Time}",
                    id, fabricItem.Status, "Ammar-Yasser8", "2025-09-02 17:23:45");

                return Json(new
                {
                    success = true,
                    newStatus = fabricItem.Status,
                    statusText = fabricItem.StatusText,
                    statusClass = fabricItem.StatusClass,
                    statusIcon = fabricItem.StatusIcon,
                    message = $"تم تحديث حالة صنف القماش إلى '{fabricItem.StatusText}'"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling fabric item {Id} status by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 17:23:45");
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الحالة" });
            }
        }

        // POST: FabricItems/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(f => f.Id==id);
                if (fabricItem == null)
                {
                    return Json(new { success = false, message = "صنف القماش غير موجود" });
                }

                // Check if fabric item is used in any transactions or other entities
                // Add checks here based on your business requirements
                // For example: fabric transactions, production records, etc.

                _unitOfWork.Repository<FabricItem>().Remove(fabricItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric item {Id} ({Name}) deleted by {User} at {Time}",
                    id, fabricItem.Item, "Ammar-Yasser8", "2025-09-02 17:23:45");

                return Json(new
                {
                    success = true,
                    message = $"تم حذف صنف القماش '{fabricItem.Item}' بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fabric item {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 17:23:45");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف صنف القماش" });
            }
        }

        #region Helper Methods

        private List<SelectListItem> GetActiveRawItems()
        {
            try
            {
                return _unitOfWork.Repository<RawItem>()
                    .GetAll(r => r.Status, includeEntities: "Warp,Weft")
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = $"{r.Item} (سداء: {r.Warp.Item}, لحمة: {r.Weft.Item})"
                    })
                    .OrderBy(r => r.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active raw items by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetRawItemsForFilter()
        {
            try
            {
                return _unitOfWork.Repository<RawItem>()
                    .GetAll(r => r.Status, includeEntities: "Warp,Weft")
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = $"{r.Item} (سداء: {r.Warp.Item}, لحمة: {r.Weft.Item})"
                    })
                    .OrderBy(r => r.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting raw items for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 17:23:45");
                return new List<SelectListItem>();
            }
        }

        #endregion
    }
}
