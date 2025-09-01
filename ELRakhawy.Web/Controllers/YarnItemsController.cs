using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.Web.Controllers
{
    public class YarnItemsController : Controller
    {
        private readonly ILogger<YarnItemsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public YarnItemsController(ILogger<YarnItemsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: YarnItems/Index
        public IActionResult Index()
        {
            try
            {
                var yarnItems = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "YarnTransactions,Manufacturers,OriginYarn")
                    .Select(item => new YarnItemViewModel
                    {
                        Id = item.Id,
                        Item = item.Item,
                        Status = item.Status,
                        Comment = item.Comment,
                        CurrentBalance = item.YarnTransactions.Sum(t => t.Inbound - t.Outbound),
                        CurrentCountBalance = item.YarnTransactions.Sum(t =>
                            (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0)),
                        ManufacturerNames = item.Manufacturers.Select(m => m.Name).ToList(),
                        ManufacturerIds = item.Manufacturers.Select(m => m.Id).ToList(),
                        OriginYarnName = item.OriginYarn != null ? item.OriginYarn.Item : null,
                        OriginYarnId = item.OriginYarnId
                    })
                    .ToList();

                _logger.LogInformation("Index page loaded successfully with {Count} items by {User} at {Time}",
                    yarnItems.Count, "Ammar-Yasser8", "2025-08-12 00:50:19");

                return View(yarnItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving yarn items in Index action by user {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return View(new List<YarnItemViewModel>());
            }
        }

        // Get : yarnItems/Overviwe 
        public IActionResult Overview()
        {
            try
            {
                var yarnItems = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "YarnTransactions,Manufacturers,OriginYarn")
                    .Select(item => new YarnItemViewModel
                    {
                        Id = item.Id,
                        Item = item.Item,
                        Status = item.Status,
                        Comment = item.Comment,
                        CurrentBalance = item.YarnTransactions.Sum(t => t.Inbound - t.Outbound),
                        CurrentCountBalance = item.YarnTransactions.Sum(t =>
                            (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0)),
                        ManufacturerNames = item.Manufacturers.Select(m => m.Name).ToList(),
                        ManufacturerIds = item.Manufacturers.Select(m => m.Id).ToList(),
                        OriginYarnName = item.OriginYarn != null ? item.OriginYarn.Item : null,
                        OriginYarnId = item.OriginYarnId
                    })
                    .ToList();
                _logger.LogInformation("Overview page loaded successfully with {Count} items by {User} at {Time}",
                    yarnItems.Count, "Ammar-Yasser8", "2025-08-12 00:50:19");
                return View(yarnItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving yarn items in Overview action by user {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return View(new List<YarnItemViewModel>());
            }
        }

        // GET: YarnItems/Create
        public IActionResult Create()
        {
            try
            {
                var model = new YarnItemViewModel
                {
                    Status = true,
                    Manufacturers = GetManufacturersSelectList(),
                    OriginYarns = GetOriginYarnsSelectList()
                };

                _logger.LogInformation("Create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");
                TempData["Error"] = "حدث خطأ أثناء تحميل النموذج";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(YarnItemViewModel model)
        {
            try
            {
                // Check for duplicate item name
                var existingItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(r => r.Item.Trim().ToLower() == model.Item.Trim().ToLower());

                if (existingItem != null)
                {
                    ModelState.AddModelError("Item", "هذا الصنف موجود بالفعل");
                }

                // Validate selected manufacturers (many-to-many)
                if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
                {
                    foreach (var manufacturerId in model.ManufacturerIds)
                    {
                        var manufacturer = _unitOfWork.Repository<Manufacturers>()
                            .GetOne(m => m.Id == manufacturerId && m.Status);
                        if (manufacturer == null)
                        {
                            ModelState.AddModelError("ManufacturerIds", $"الشركة المصنعة بالرقم {manufacturerId} غير موجودة أو غير نشطة");
                        }
                    }
                }

                // Validate origin yarn exists if selected
                if (model.OriginYarnId.HasValue)
                {
                    var originYarnItem = _unitOfWork.Repository<YarnItem>()
                        .GetOne(y => y.Id == model.OriginYarnId.Value && y.Status);
                    if (originYarnItem == null)
                    {
                        ModelState.AddModelError("OriginYarnId", "الغزل المكون المحدد غير موجود أو غير نشط");
                        _logger.LogWarning("Origin yarn item with ID {OriginYarnId} not found during create by {User} at {Time}",
                            model.OriginYarnId.Value, "Ammar-Yasser8", "2025-08-12 01:39:58");
                    }
                }

                if (!ModelState.IsValid)
                {
                    model.Manufacturers = GetManufacturersSelectList();
                    model.OriginYarns = GetOriginYarnsSelectList();
                    return View(model);
                }

                var yarnItem = new YarnItem
                {
                    Item = model.Item.Trim(),
                    Status = model.Status,
                    Comment = model.Comment?.Trim(),
                    OriginYarnId = model.OriginYarnId
                };

                // Assign manufacturers (many-to-many)
                if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
                {
                    yarnItem.Manufacturers = _unitOfWork.Repository<Manufacturers>()
                        .GetAll(m => model.ManufacturerIds.Contains(m.Id) && m.Status)
                        .ToList();
                }

                _unitOfWork.Repository<YarnItem>().Add(yarnItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Yarn item '{Item}' created successfully with OriginYarn {OriginYarnId} by {User} at {Time}",
                    model.Item, model.OriginYarnId, "Ammar-Yasser8", "2025-08-12 01:33:24");

                TempData["Success"] = "تم إضافة الصنف بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating yarn item by {User} at {Time}: {Message}",
                    "Ammar-Yasser8", "2025-08-12 01:33:24", ex.Message);

                if (ex.InnerException?.Message.Contains("FK_YarnItems") == true)
                {
                    ModelState.AddModelError("OriginYarnId", "الغزل المكون المحدد غير صحيح أو غير موجود في قاعدة البيانات");
                }
                else
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء حفظ البيانات");
                }

                model.Manufacturers = GetManufacturersSelectList();
                model.OriginYarns = GetOriginYarnsSelectList();
                return View(model);
            }
        }
        // GET: YarnItems/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(y => y.Id == id, includeEntities: "Manufacturers,OriginYarn,YarnTransactions,DerivedYarns");

                if (yarnItem == null)
                {
                    _logger.LogWarning("Yarn item with ID {Id} not found for edit by {User} at {Time}",
                        id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    TempData["Error"] = "الصنف المطلوب غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                var model = new YarnItemViewModel
                {
                    Id = yarnItem.Id,
                    Item = yarnItem.Item,
                    Status = yarnItem.Status,
                    Comment = yarnItem.Comment,
                    ManufacturerNames = yarnItem.Manufacturers.Select(m => m.Name).ToList(),
                    ManufacturerIds = yarnItem.Manufacturers.Select(m => m.Id).ToList(),
                    OriginYarnId = yarnItem.OriginYarnId,
                    OriginYarnName = yarnItem.OriginYarn?.Item,
                    CurrentBalance = yarnItem.YarnTransactions.Sum(t => t.Inbound - t.Outbound),
                    CurrentCountBalance = yarnItem.YarnTransactions.Sum(t =>
                        (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0)),
                    Manufacturers = GetManufacturersSelectList(),
                    OriginYarns = GetOriginYarnsSelectList(id) // Exclude current item from origin yarns
                };

                _logger.LogInformation("Edit form loaded for yarn item '{Item}' (ID: {Id}) by {User} at {Time}. Hierarchy Level: {Level}, Path: {Path}",
                    yarnItem.Item, id, "Ammar-Yasser8", "2025-08-12 14:22:41",
                    yarnItem.GetHierarchyLevel(), yarnItem.GetHierarchyPath());

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for yarn item {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: YarnItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, YarnItemViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    _logger.LogWarning("ID mismatch in edit request: URL ID {UrlId} vs Model ID {ModelId} by {User} at {Time}",
                        id, model.Id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    TempData["Error"] = "معرف الصنف غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(y => y.Id == id, includeEntities: "Manufacturers,OriginYarn,DerivedYarns");

                if (yarnItem == null)
                {
                    _logger.LogWarning("Yarn item with ID {Id} not found during edit by {User} at {Time}",
                        id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    TempData["Error"] = "الصنف المطلوب غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                // Enhanced validation: Check for duplicate item name (excluding current item)
                var existingItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(r => r.Item.Trim().ToLower() == model.Item.Trim().ToLower() && r.Id != id);

                if (existingItem != null)
                {
                    ModelState.AddModelError("Item", "هذا الصنف موجود بالفعل");
                    _logger.LogWarning("Duplicate item name '{Item}' attempted by {User} at {Time}",
                        model.Item, "Ammar-Yasser8", "2025-08-12 14:22:41");
                }

                // Enhanced validation: Validate all selected manufacturers (many-to-many)
                if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
                {
                    foreach (var manufacturerId in model.ManufacturerIds)
                    {
                        var manufacturer = _unitOfWork.Repository<Manufacturers>()
                            .GetOne(m => m.Id == manufacturerId && m.Status);
                        if (manufacturer == null)
                        {
                            ModelState.AddModelError("ManufacturerIds", $"الشركة المصنعة بالرقم {manufacturerId} غير موجودة أو غير نشطة");
                            _logger.LogWarning("Invalid manufacturer ID {ManufacturerId} for yarn item {Id} by {User} at {Time}",
                                manufacturerId, id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                        }
                    }
                }

                // ENHANCED: Validate origin yarn with circular reference check
                if (model.OriginYarnId.HasValue)
                {
                    var originYarnItem = _unitOfWork.Repository<YarnItem>()
                        .GetOne(y => y.Id == model.OriginYarnId.Value && y.Status);

                    if (originYarnItem == null)
                    {
                        ModelState.AddModelError("OriginYarnId", "الغزل المكون المحدد غير موجود أو غير نشط");
                        _logger.LogWarning("Invalid origin yarn ID {OriginYarnId} for yarn item {Id} by {User} at {Time}",
                            model.OriginYarnId.Value, id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    }
                    else if (originYarnItem.Id == model.Id)
                    {
                        ModelState.AddModelError("OriginYarnId", "لا يمكن أن يكون الصنف مكون من نفسه");
                        _logger.LogWarning("Self-reference attempted for yarn item {Id} by {User} at {Time}",
                            id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    }
                    // ENHANCED: Check for circular dependency using service
                    else if (WouldCreateCircularReference(model.Id, model.OriginYarnId.Value))
                    {
                        ModelState.AddModelError("OriginYarnId", "يوجد تداخل دائري في الغزل المكون - هذا التحديث سيؤدي إلى مرجع دائري");
                        _logger.LogWarning("Circular reference detected for yarn item {Id} with origin {OriginYarnId} by {User} at {Time}",
                            id, model.OriginYarnId.Value, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    }
                }

                if (!ModelState.IsValid)
                {
                    model.Manufacturers = GetManufacturersSelectList();
                    model.OriginYarns = GetOriginYarnsSelectList(id);
                    return View(model);
                }

                // Store original values for logging
                var originalValues = new
                {
                    Item = yarnItem.Item,
                    OriginYarnId = yarnItem.OriginYarnId,
                    ManufacturerIds = yarnItem.Manufacturers.Select(m => m.Id).ToList(),
                    Status = yarnItem.Status
                };

                // Update the yarn item
                yarnItem.Item = model.Item.Trim();
                yarnItem.Status = model.Status;
                yarnItem.Comment = model.Comment?.Trim();
                yarnItem.OriginYarnId = model.OriginYarnId;

                // Update manufacturers (many-to-many)
                yarnItem.Manufacturers.Clear();
                if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
                {
                    var selectedManufacturers = _unitOfWork.Repository<Manufacturers>()
                        .GetAll(m => model.ManufacturerIds.Contains(m.Id) && m.Status)
                        .ToList();
                    foreach (var manufacturer in selectedManufacturers)
                    {
                        yarnItem.Manufacturers.Add(manufacturer);
                    }
                }

                _unitOfWork.Repository<YarnItem>().Update(yarnItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Yarn item '{Item}' (ID: {Id}) updated successfully by {User} at {Time}. Changes: Name: {NameChanged}, Origin: {OriginOld}→{OriginNew}, Manufacturers: {ManufacturersOld}→{ManufacturersNew}, Status: {StatusOld}→{StatusNew}",
                    model.Item, id, "Ammar-Yasser8", "2025-08-12 14:22:41",
                    originalValues.Item != model.Item,
                    originalValues.OriginYarnId, model.OriginYarnId,
                    string.Join(",", originalValues.ManufacturerIds), string.Join(",", model.ManufacturerIds ?? new List<int>()),
                    originalValues.Status, model.Status);

                TempData["Success"] = $"تم تحديث الصنف '{model.Item}' بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating yarn item {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-08-12 14:22:41");

                if (ex.InnerException?.Message.Contains("FK_YarnItems") == true)
                {
                    ModelState.AddModelError("OriginYarnId", "الغزل المكون المحدد غير صحيح أو غير موجود في قاعدة البيانات");
                }
                else
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء حفظ البيانات");
                }

                model.Manufacturers = GetManufacturersSelectList();
                model.OriginYarns = GetOriginYarnsSelectList(id);
                return View(model);
            }
        }

        // API: DELETE /api/YarnItems/{id}
        [HttpDelete]
        [Route("api/YarnItems/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(y => y.Id == id, includeEntities: "YarnTransactions,DerivedYarns");

                if (yarnItem == null)
                {
                    _logger.LogWarning("Yarn item with ID {Id} not found for deletion by {User} at {Time}",
                        id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                    return NotFound(new { message = "الصنف غير موجود" });
                }

                // ENHANCED: Use model's built-in method for deletion safety check
                if (!yarnItem.CanBeDeleted())
                {
                    var reasons = new List<string>();

                    if (yarnItem.YarnTransactions.Any())
                    {
                        reasons.Add($"يحتوي على {yarnItem.YarnTransactions.Count} معاملة");
                    }

                    if (yarnItem.DerivedYarns.Any())
                    {
                        var dependentNames = string.Join(", ", yarnItem.DerivedYarns.Take(3).Select(y => y.Item));
                        var remainingCount = Math.Max(0, yarnItem.DerivedYarns.Count - 3);
                        reasons.Add($"مستخدم كغزل مكون للأصناف: {dependentNames}" +
                            (remainingCount > 0 ? $" و{remainingCount} آخرين" : ""));
                    }

                    _logger.LogWarning("Cannot delete yarn item '{Item}' (ID: {Id}) by {User} at {Time}. Reasons: {Reasons}",
                        yarnItem.Item, id, "Ammar-Yasser8", "2025-08-12 14:22:41", string.Join("; ", reasons));

                    return BadRequest(new
                    {
                        message = "لا يمكن حذف الصنف للأسباب التالية:",
                        reasons = reasons
                    });
                }

                // Safe to delete - handle relationships manually due to NO ACTION cascade
                string itemName = yarnItem.Item;
                int hierarchyLevel = yarnItem.GetHierarchyLevel();
                string hierarchyPath = yarnItem.GetHierarchyPath();

                // ENHANCED: Manually handle relationships due to NO ACTION cascade
                var referencingItems = _unitOfWork.Repository<YarnItem>()
                    .GetAll(y => y.OriginYarnId == id);

                foreach (var referencingItem in referencingItems)
                {
                    _logger.LogInformation("Setting OriginYarnId to null for yarn item '{Item}' (ID: {Id}) due to deletion of origin yarn '{OriginItem}' by {User} at {Time}",
                        referencingItem.Item, referencingItem.Id, itemName, "Ammar-Yasser8", "2025-08-12 14:22:41");

                    referencingItem.OriginYarnId = null;
                    _unitOfWork.Repository<YarnItem>().Update(referencingItem);
                }

                _unitOfWork.Repository<YarnItem>().Remove(yarnItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Yarn item '{Item}' (ID: {Id}) deleted successfully by {User} at {Time}. Hierarchy Level: {Level}, Path: {Path}, References Updated: {ReferencesCount}",
                    itemName, id, "Ammar-Yasser8", "2025-08-12 14:22:41", hierarchyLevel, hierarchyPath, referencingItems.Count());

                return Json(new
                {
                    message = $"تم حذف الصنف '{itemName}' بنجاح",
                    referencesUpdated = referencingItems.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting yarn item {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-08-12 14:22:41");
                return StatusCode(500, new { message = "حدث خطأ أثناء حذف الصنف" });
            }
        }

        // ENHANCED: Helper method for circular reference checking
        private bool WouldCreateCircularReference(int itemId, int newOriginYarnId)
        {
            try
            {
                var visited = new HashSet<int> { itemId };
                return CheckCircularReferenceRecursive(newOriginYarnId, visited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking circular reference for item {ItemId} and origin {OriginId} by {User} at {Time}",
                    itemId, newOriginYarnId, "Ammar-Yasser8", "2025-08-12 14:22:41");
                return true; // Err on the side of caution
            }
        }

        private bool CheckCircularReferenceRecursive(int yarnId, HashSet<int> visited)
        {
            if (visited.Contains(yarnId))
                return true;

            visited.Add(yarnId);

            var yarnItem = _unitOfWork.Repository<YarnItem>()
                .GetOne(y => y.Id == yarnId);

            if (yarnItem?.OriginYarnId.HasValue == true)
            {
                return CheckCircularReferenceRecursive(yarnItem.OriginYarnId.Value, visited);
            }

            return false;
        }

        // ENHANCED: Updated helper method for origin yarns dropdown
        private IEnumerable<SelectListItem> GetOriginYarnsSelectList(int? excludeId = null)
        {
            try
            {
                var originYarns = _unitOfWork.Repository<YarnItem>()
                    .GetAll(y => y.Status && (!excludeId.HasValue || y.Id != excludeId.Value))
                    .OrderBy(y => y.GetHierarchyLevel()) // Order by hierarchy level first
                    .ThenBy(y => y.Item)
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        // ENHANCED: Show hierarchy in dropdown
                        Text = new string('　', y.GetHierarchyLevel() * 2) + y.Item
                    })
                    .ToList();

                originYarns.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- اختر الغزل المكون --"
                });

                _logger.LogInformation("Loaded {Count} origin yarns for dropdown (excluding {ExcludeId}) by {User} at {Time}",
                    originYarns.Count - 1, excludeId, "Ammar-Yasser8", "2025-08-12 14:22:41");

                return originYarns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading origin yarns list by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 14:22:41");
                return new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- خطأ في تحميل البيانات --" }
        };
            }
        }

        // API: GET /api/YarnItems/GetOriginYarns
        [HttpGet]
        [Route("api/YarnItems/GetOriginYarns")]
        public IActionResult GetOriginYarns(int? excludeId = null)
        {
            try
            {
                var originYarns = _unitOfWork.Repository<YarnItem>()
                    .GetAll(y => y.Status && (!excludeId.HasValue || y.Id != excludeId.Value),
                            includeEntities: "OriginYarn")
                    .ToList() // Execute query first
                    .OrderBy(y => y.GetHierarchyLevel())
                    .ThenBy(y => y.Item)
                    .Select(y => new {
                        value = y.Id.ToString(),
                        text = new string('　', y.GetHierarchyLevel() * 2) + y.Item,
                        level = y.GetHierarchyLevel(),
                        path = y.GetHierarchyPath()
                    })
                    .ToList();

                _logger.LogInformation("API: Origin yarns list loaded with {Count} items (excluding {ExcludeId}) by {User} at {Time}",
                    originYarns.Count, excludeId, "Ammar-Yasser8", "2025-08-12 14:22:41");

                return Json(new { success = true, data = originYarns });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading origin yarns via API by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 14:22:41");
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل قائمة الغزل المكون" });
            }
        }

        

        // API: PUT /api/YarnItems/{id}/status
        [HttpPut]
        [Route("api/YarnItems/{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            try
            {
                var yarnItem = _unitOfWork.Repository<YarnItem>().GetOne(y => y.Id == id);
                if (yarnItem == null)
                {
                    return NotFound(new { message = "الصنف غير موجود" });
                }

                yarnItem.Status = model.Status;
                _unitOfWork.Repository<YarnItem>().Update(yarnItem);
                _unitOfWork.Complete();

                _logger.LogInformation("Status updated for yarn item '{Item}' (ID: {Id}) to {Status} by {User} at {Time}",
                    yarnItem.Item, id, model.Status ? "Active" : "Inactive", "Ammar-Yasser8", "2025-08-12 00:50:19");

                return Json(new
                {
                    message = $"تم تحديث حالة '{yarnItem.Item}' بنجاح",
                    status = model.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for yarn item {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-08-12 00:50:19");
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث الحالة" });
            }
        }

       
        // Helper methods for dropdown lists
        private IEnumerable<SelectListItem> GetManufacturersSelectList()
        {
            try
            {
                var manufacturers = _unitOfWork.Repository<Manufacturers>()
                    .GetAll()
                    .Where(m => m.Status) // Only active manufacturers
                    .OrderBy(m => m.Name)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name
                    })
                    .ToList();

                // Add default option
                manufacturers.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- اختر الشركة المصنعة --"
                });

                return manufacturers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manufacturers list by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");
                return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- خطأ في تحميل البيانات --" }
            };
            }
        }

       
        // Helper method to check for circular dependency
        private bool HasCircularDependency(int itemId, int originYarnId)
        {
            try
            {
                var visited = new HashSet<int>();
                return CheckCircularDependencyRecursive(itemId, originYarnId, visited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking circular dependency for item {ItemId} and origin {OriginId}",
                    itemId, originYarnId);
                return false;
            }
        }

        private bool CheckCircularDependencyRecursive(int targetId, int currentId, HashSet<int> visited)
        {
            if (currentId == targetId)
                return true;

            if (visited.Contains(currentId))
                return false;

            visited.Add(currentId);

            var currentItem = _unitOfWork.Repository<YarnItem>().GetOne(y => y.Id == currentId);
            if (currentItem?.OriginYarnId.HasValue == true)
            {
                return CheckCircularDependencyRecursive(targetId, currentItem.OriginYarnId.Value, visited);
            }

            return false;
        }

        // Model for status update
        public class StatusUpdateModel
        {
            public bool Status { get; set; }
        }
    }
}
