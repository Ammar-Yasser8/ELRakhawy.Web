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
        public IActionResult Overview(bool availableOnly = false)
        {
            try
            {
                var currentTime = "2025-09-04 18:59:00";
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("Yarn overview page loaded by {User} at {Time} - Available only: {AvailableOnly}",
                    currentUser, currentTime, availableOnly);

                // Get all active yarn items with their transactions and manufacturers
                var yarnItems = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "OriginYarn,Manufacturers")
                    .Where(y => y.Status)
                    .ToList();

                var overviewItems = new List<YarnOverviewItemViewModel>();

                foreach (var yarnItem in yarnItems)
                {
                    // ✅ Get latest transaction for current balances (consistent with other methods)
                    var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                        .GetAll(t => t.YarnItemId == yarnItem.Id)
                        .OrderByDescending(t => t.Date)
                        .ThenByDescending(t => t.Id)
                        .FirstOrDefault();

                    // ✅ Use running balance approach
                    var quantityBalance = latestTransaction?.QuantityBalance ?? 0;
                    var countBalance = latestTransaction?.CountBalance ?? 0;

                    // Get all transactions for statistics
                    var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                        .GetAll(t => t.YarnItemId == yarnItem.Id)
                        .ToList();

                    // Get latest transaction date
                    var lastTransactionDate = allTransactions.Any() ?
                        allTransactions.Max(t => t.Date) : (DateTime?)null;

                    // Get transaction counts
                    var totalTransactions = allTransactions.Count;
                    var inboundTransactions = allTransactions.Count(t => t.Inbound > 0);
                    var outboundTransactions = allTransactions.Count(t => t.Outbound > 0);

                    // ✅ Get manufacturer names properly
                    var manufacturerNames = yarnItem.Manufacturers != null && yarnItem.Manufacturers.Any()
                        ? string.Join("، ", yarnItem.Manufacturers.Select(m => m.Name))
                        : "غير محدد";

                    // ✅ Get origin yarn name properly
                    var originYarnName = yarnItem.OriginYarn?.Item ?? "غير محدد";

                    var overviewItem = new YarnOverviewItemViewModel
                    {
                        YarnItemId = yarnItem.Id,
                        YarnItemName = yarnItem.Item ?? "غير محدد",
                        OriginYarnName = originYarnName,
                        ManufacturerNames = manufacturerNames,
                        QuantityBalance = quantityBalance,
                        CountBalance = countBalance,
                        LastTransactionDate = lastTransactionDate,
                        TotalTransactions = totalTransactions,
                        InboundTransactions = inboundTransactions,
                        OutboundTransactions = outboundTransactions,
                        IsAvailable = quantityBalance > 0,
                        Status = yarnItem.Status
                    };

                    overviewItems.Add(overviewItem);

                    _logger.LogDebug("Processed yarn item {YarnItemId}: {YarnName}, Origin: {Origin}, Manufacturer: {Manufacturer}, Qty: {Qty}, Count: {Count}",
                        yarnItem.Id, yarnItem.Item, originYarnName, manufacturerNames, quantityBalance, countBalance);
                }

                // Apply available filter if requested
                if (availableOnly)
                {
                    overviewItems = overviewItems.Where(item => item.IsAvailable).ToList();
                }

                // Order by quantity balance descending, then by yarn item name
                overviewItems = overviewItems
                    .OrderByDescending(item => item.QuantityBalance)
                    .ThenBy(item => item.YarnItemName)
                    .ToList();

                var viewModel = new YarnOverviewViewModel
                {
                    OverviewItems = overviewItems,
                    AvailableOnly = availableOnly,
                    TotalItems = overviewItems.Count,
                    AvailableItems = overviewItems.Count(item => item.IsAvailable),
                    TotalQuantityBalance = overviewItems.Sum(item => item.QuantityBalance),
                    TotalCountBalance = overviewItems.Sum(item => item.CountBalance),
                    LastUpdated = DateTime.Now
                };

                _logger.LogInformation("Yarn overview completed by {User} at {Time} - Found {Count} items, Available: {Available}, Total Qty: {TotalQty}, Total Count: {TotalCount}",
                    currentUser, currentTime, viewModel.TotalItems, viewModel.AvailableItems, viewModel.TotalQuantityBalance, viewModel.TotalCountBalance);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn overview by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 18:59:00");
                TempData["Error"] = "حدث خطأ أثناء تحميل نظرة عامة على الغزل";
                return RedirectToAction("Index", "YarnItems");
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
            // تحقق من التكرار
            var existingItem = _unitOfWork.Repository<YarnItem>()
                .GetOne(r => r.Item.Trim().ToLower() == model.Item.Trim().ToLower());
            if (existingItem != null)
                ModelState.AddModelError("Item", "هذا الصنف موجود بالفعل");

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(ModelState.ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ));

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

            if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
            {
                yarnItem.Manufacturers = _unitOfWork.Repository<Manufacturers>()
                    .GetAll(m => model.ManufacturerIds.Contains(m.Id) && m.Status)
                    .ToList();
            }

            _unitOfWork.Repository<YarnItem>().Add(yarnItem);
            _unitOfWork.Complete();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, data = new { id = yarnItem.Id, item = yarnItem.Item } });

            TempData["Success"] = "تم إضافة الصنف بنجاح";
            return RedirectToAction(nameof(Index));
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
            if (id != model.Id)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "معرف الصنف غير صحيح" });

                TempData["Error"] = "معرف الصنف غير صحيح";
                return RedirectToAction(nameof(Index));
            }

            var yarnItem = _unitOfWork.Repository<YarnItem>()
                .GetOne(y => y.Id == id, includeEntities: "Manufacturers,OriginYarn,DerivedYarns");

            if (yarnItem == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "الصنف المطلوب غير موجود" });

                TempData["Error"] = "الصنف المطلوب غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // تحقق من التكرار
            var existingItem = _unitOfWork.Repository<YarnItem>()
                .GetOne(r => r.Item.Trim().ToLower() == model.Item.Trim().ToLower() && r.Id != id);
            if (existingItem != null)
                ModelState.AddModelError("Item", "هذا الصنف موجود بالفعل");

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(ModelState.ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ));

                model.Manufacturers = GetManufacturersSelectList();
                model.OriginYarns = GetOriginYarnsSelectList(id);
                return View(model);
            }

            // تحديث البيانات
            yarnItem.Item = model.Item.Trim();
            yarnItem.Status = model.Status;
            yarnItem.Comment = model.Comment?.Trim();
            yarnItem.OriginYarnId = model.OriginYarnId;

            yarnItem.Manufacturers.Clear();
            if (model.ManufacturerIds != null && model.ManufacturerIds.Any())
            {
                var selectedManufacturers = _unitOfWork.Repository<Manufacturers>()
                    .GetAll(m => model.ManufacturerIds.Contains(m.Id) && m.Status)
                    .ToList();
                foreach (var manufacturer in selectedManufacturers)
                    yarnItem.Manufacturers.Add(manufacturer);
            }

            _unitOfWork.Repository<YarnItem>().Update(yarnItem);
            _unitOfWork.Complete();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, data = new { id = yarnItem.Id, item = yarnItem.Item } });

            TempData["Success"] = $"تم تحديث الصنف '{model.Item}' بنجاح";
            return RedirectToAction(nameof(Index));
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

        [HttpGet]
        [Route("api/YarnItems/GetManufacturers")]
        public IActionResult GetManufacturers()
        {
            try
            {
                var manufacturers = _unitOfWork.Repository<Manufacturers>()
                    .GetAll()
                    .Where(m => m.Status) // Only active manufacturers
                    .OrderBy(m => m.Name)
                    .Select(m => new
                    {
                        id = m.Id,
                        name = m.Name,
                        status = m.Status
                    })
                    .ToList();
                return Ok(new { success = true, data = manufacturers });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manufacturers via API by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 00:50:19");
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل قائمة الشركات المصنعة" });


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


        [HttpGet]
        [Route("api/YarnItems/{id}")]
        public IActionResult GetYarnItem(int id)
        {
            var yarnItem = _unitOfWork.Repository<YarnItem>()
                .GetOne(y => y.Id == id, includeEntities: "Manufacturers,OriginYarn");

            if (yarnItem == null)
                return Json(new { success = false, message = "الصنف غير موجود" });

            return Json(new
            {
                success = true,
                data = new
                {
                    id = yarnItem.Id,
                    item = yarnItem.Item,
                    status = yarnItem.Status,
                    comment = yarnItem.Comment,
                    manufacturerIds = yarnItem.Manufacturers.Select(m => m.Id).ToList(),
                    originYarnId = yarnItem.OriginYarnId,
                    originYarnName = yarnItem.OriginYarn?.Item
                }
            });
        }

        // Model for status update
        public class StatusUpdateModel
        {
            public bool Status { get; set; }
        }
    }
}
