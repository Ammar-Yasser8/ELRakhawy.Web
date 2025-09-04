using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using System.Text;

namespace ELRakhawy.Web.Controllers
{
    public class YarnTransactionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<YarnTransactionsController> _logger;

        public YarnTransactionsController(IUnitOfWork unitOfWork, ILogger<YarnTransactionsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: YarnTransactions/Inbound
        public IActionResult Inbound()
        {
            try
            {
                var viewModel = new YarnTransactionViewModel
                {
                    IsInbound = true,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveYarnItems(),
                };

                _logger.LogInformation("Inbound yarn transaction form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");

                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inbound yarn transaction form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الوارد";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // GET: YarnTransactions/Outbound
        public IActionResult Outbound()
        {
            try
            {
                var viewModel = new YarnTransactionViewModel
                {
                    IsInbound = false,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveYarnItems(),
                };

                _logger.LogInformation("Outbound yarn transaction form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");

                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbound yarn transaction form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الصادر";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // POST: YarnTransactions/SaveTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTransaction(YarnTransactionViewModel model)
        {
            try
            {
                // Remove fields that are populated server-side
                ModelState.Remove("YarnItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("StakeholderTypeName");
                ModelState.Remove("PackagingStyleName");
                ModelState.Remove("TransactionId");
                ModelState.Remove("OriginYarnName");
                ModelState.Remove("StakeholderTypeId");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    return View("TransactionForm", model);
                }

                // ✅ Get latest transaction for current balances
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == model.YarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                var currentQuantityBalance = latestTransaction?.QuantityBalance ?? 0;
                var currentCountBalance = latestTransaction?.CountBalance ?? 0;

                // Check if outbound quantity exceeds available balance
                if (!model.IsInbound && model.Quantity > currentQuantityBalance)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({currentQuantityBalance:N3})");
                    model.AvailableItems = GetActiveYarnItems();
                    return View("TransactionForm", model);
                }

                var transaction = new YarnTransaction
                {
                    TransactionId = GenerateTransactionId(),
                    InternalId = model.InternalId?.Trim(),
                    ExternalId = model.ExternalId?.Trim(),
                    YarnItemId = model.YarnItemId,
                    Inbound = model.IsInbound ? model.Quantity : 0,
                    Outbound = model.IsInbound ? 0 : model.Quantity,
                    Count = model.Count,
                    StakeholderId = model.StakeholderId,
                    StakeholderTypeId = null,
                    PackagingStyleId = model.PackagingStyleId,
                    Date = model.Date,
                    Comment = model.Comment?.Trim()
                };

                // ✅ CORRECT Balance Calculations
                transaction.QuantityBalance = currentQuantityBalance + (transaction.Inbound - transaction.Outbound);

                var countInbound = transaction.Inbound > 0 ? transaction.Count : 0;
                var countOutbound = transaction.Outbound > 0 ? transaction.Count : 0;
                transaction.CountBalance = currentCountBalance + countInbound - countOutbound;

                // ✅ Enhanced logging for balance tracking
                _logger.LogInformation("Balance calculation for YarnItem {YarnItemId}: Previous Qty={PrevQty}, Previous Count={PrevCount}, " +
                    "New Inbound={Inbound}, New Outbound={Outbound}, New Count={Count}, " +
                    "Final Qty Balance={FinalQty}, Final Count Balance={FinalCount} by {User} at {Time}",
                    model.YarnItemId, currentQuantityBalance, currentCountBalance,
                    transaction.Inbound, transaction.Outbound, transaction.Count,
                    transaction.QuantityBalance, transaction.CountBalance,
                    "Ammar-Yasser8", "2025-09-04 17:09:12");

                _unitOfWork.Repository<YarnTransaction>().Add(transaction);
                _unitOfWork.Complete();

                _logger.LogInformation("Yarn transaction {TransactionId} saved successfully - {Type} by {User} at {Time}",
                    transaction.TransactionId, model.IsInbound ? "Inbound" : "Outbound",
                    "Ammar-Yasser8", "2025-09-04 17:09:12");

                TempData["Success"] = $"تم تسجيل {(model.IsInbound ? "وارد" : "صادر")} الغزل بنجاح - رقم الإذن: {transaction.TransactionId}";
                return RedirectToAction("Index", "YarnItems");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving yarn transaction by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 17:09:12");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ المعاملة");
                model.AvailableItems = GetActiveYarnItems();
                return View("TransactionForm", model);
            }
        }


        // GET: YarnTransactions/Search
        public IActionResult Search()
        {
            try
            {
                var viewModel = new YarnTransactionSearchViewModel
                {
                    FromDate = DateTime.Today.AddDays(-30), // Default to last 30 days
                    ToDate = DateTime.Today,
                    AvailableItems = GetActiveYarnItems(),
                    StakeholderTypes = GetStakeholderTypes(),
                    YarnStakeholders = GetYarnStakeholders(), // New method for yarn-related stakeholders
                    Results = new List<YarnTransactionViewModel>()
                };

                _logger.LogInformation("Yarn transaction search form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 12:04:03");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn transaction search form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 12:04:03");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة البحث";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // POST: YarnTransactions/Search 
        [HttpPost]
        public IActionResult Search(YarnTransactionSearchViewModel model)
        {
            try
            {
                var currentTime = "2025-09-01 12:04:03";
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("Enhanced yarn transaction search initiated by {User} at {Time} with criteria: {@SearchCriteria}",
                    currentUser, currentTime, new
                    {
                        model.FromDate,
                        model.ToDate,
                        model.TransactionId,
                        model.TransactionType,
                        model.YarnItemId,
                        model.StakeholderTypeId,
                        model.StakeholderId
                    });

                // Get yarn-related stakeholder IDs first
                var yarnStakeholderIds = GetYarnStakeholderIds();

                var query = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(includeEntities: "YarnItem,StakeholderType,Stakeholder,PackagingStyle,YarnItem.OriginYarn,YarnItem.Manufacturers")
                    .Where(t => yarnStakeholderIds.Contains(t.StakeholderId)) // Filter by yarn stakeholders only
                    .AsEnumerable();

                // Apply date filters
                if (model.FromDate.HasValue)
                {
                    query = query.Where(t => t.Date.Date >= model.FromDate.Value.Date);
                    _logger.LogDebug("Applied FromDate filter: {FromDate}", model.FromDate.Value);
                }

                if (model.ToDate.HasValue)
                {
                    query = query.Where(t => t.Date.Date <= model.ToDate.Value.Date);
                    _logger.LogDebug("Applied ToDate filter: {ToDate}", model.ToDate.Value);
                }

                if (model.YarnItemId.HasValue)
                {
                    query = query.Where(t => t.YarnItemId == model.YarnItemId.Value);
                    _logger.LogDebug("Applied YarnItemId filter: {YarnItemId}", model.YarnItemId.Value);
                }

                if (model.StakeholderTypeId.HasValue)
                {
                    query = query.Where(t => t.StakeholderTypeId == model.StakeholderTypeId.Value);
                    _logger.LogDebug("Applied StakeholderTypeId filter: {StakeholderTypeId}", model.StakeholderTypeId.Value);
                }

                if (model.StakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == model.StakeholderId.Value);
                    _logger.LogDebug("Applied specific StakeholderId filter: {StakeholderId}", model.StakeholderId.Value);
                }

                if (!string.IsNullOrEmpty(model.TransactionId))
                {
                    query = query.Where(t => t.TransactionId.Contains(model.TransactionId.Trim()));
                    _logger.LogDebug("Applied TransactionId filter: {TransactionId}", model.TransactionId);
                }

                if (!string.IsNullOrEmpty(model.TransactionType))
                {
                    if (model.TransactionType == "inbound")
                        query = query.Where(t => t.Inbound > 0);
                    else if (model.TransactionType == "outbound")
                        query = query.Where(t => t.Outbound > 0);

                    _logger.LogDebug("Applied TransactionType filter: {TransactionType}", model.TransactionType);
                }

                // Execute query and map results
                var results = query.OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .Select(t => new YarnTransactionViewModel
                    {
                        Id = t.Id,
                        TransactionId = t.TransactionId,
                        InternalId = t.InternalId,
                        ExternalId = t.ExternalId,
                        YarnItemId = t.YarnItemId,
                        YarnItemName = t.YarnItem.Item,
                        OriginYarnName = t.YarnItem.OriginYarn?.Item,
                        Quantity = t.Inbound > 0 ? t.Inbound : t.Outbound,
                        IsInbound = t.Inbound > 0,
                        Count = t.Count,
                        StakeholderTypeId = t.StakeholderTypeId ?? 0,
                        StakeholderTypeName = t.StakeholderType?.Type ?? "غير محدد",
                        StakeholderId = t.StakeholderId,
                        StakeholderName = t.Stakeholder.Name,
                        PackagingStyleId = t.PackagingStyleId,
                        PackagingStyleName = t.PackagingStyle.StyleName,
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance,
                        Date = t.Date,
                        Comment = t.Comment
                    })
                    .ToList();

                // Calculate enhanced statistics
                model.Results = results;
                model.TotalTransactions = results.Count;
                model.TotalInbound = results.Where(r => r.IsInbound).Sum(r => r.Quantity);
                model.TotalOutbound = results.Where(r => !r.IsInbound).Sum(r => r.Quantity);
                model.NetBalance = model.TotalInbound - model.TotalOutbound;

                // Reload dropdown data
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
                model.YarnStakeholders = GetYarnStakeholders();

                // Set search performed flag
                ViewBag.SearchPerformed = true;

                _logger.LogInformation("Enhanced yarn transaction search completed by {User} at {Time} - Found {Count} yarn-related results, TotalInbound: {TotalInbound}, TotalOutbound: {TotalOutbound}, NetBalance: {NetBalance}",
                    currentUser, currentTime, results.Count, model.TotalInbound, model.TotalOutbound, model.NetBalance);

                // Add success message if results found
                if (results.Any())
                {
                    TempData["Success"] = $"تم العثور على {results.Count} معاملة غزل مطابقة لمعايير البحث";
                }
                else
                {
                    TempData["Info"] = "لم يتم العثور على معاملات غزل مطابقة لمعايير البحث المحددة";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enhanced yarn transaction search by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 12:04:03");

                TempData["Error"] = "حدث خطأ أثناء البحث في معاملات الغزل";

                // Ensure model has required data for error display
                model.Results = new List<YarnTransactionViewModel>();
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
                model.YarnStakeholders = GetYarnStakeholders();
                ViewBag.SearchPerformed = true;

                return View(model);
            }
        }

        // Helper method to get yarn stakeholder IDs based on form relationship
        private List<int> GetYarnStakeholderIds()
        {
            try
            {
                // Find all form styles that contain "غزل" in FormName
                var yarnFormIds = _unitOfWork.Repository<FormStyle>()
                    .GetAll()
                    .Where(f => f.FormName.Contains("غزل"))
                    .Select(f => f.Id)
                    .ToList();

                if (!yarnFormIds.Any())
                {
                    _logger.LogWarning("No yarn forms found containing 'غزل' at {Time} by {User}",
                        "2025-09-01 12:04:03", "Ammar-Yasser8");
                    return new List<int>();
                }

                // Find stakeholder type IDs allowed for yarn forms
                var allowedTypeIds = _unitOfWork.Repository<StakeholderType>()
                    .GetAll(includeEntities: "StakeholderTypeForms")
                    .Where(st => st.StakeholderTypeForms.Any(f => yarnFormIds.Contains(f.FormId)))
                    .Select(st => st.Id)
                    .ToList();

                if (!allowedTypeIds.Any())
                {
                    _logger.LogWarning("No stakeholder types found for yarn forms at {Time} by {User}",
                        "2025-09-01 12:04:03", "Ammar-Yasser8");
                    return new List<int>();
                }

                // Get stakeholder IDs that have the allowed types and are active
                var stakeholderIds = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes")
                    .Where(s => s.Status &&
                               s.StakeholderInfoTypes.Any(st => allowedTypeIds.Contains(st.StakeholderTypeId)))
                    .Select(s => s.Id)
                    .ToList();

                _logger.LogInformation("Found {Count} yarn-related stakeholders at {Time} by {User}",
                    stakeholderIds.Count, "2025-09-01 12:04:03", "Ammar-Yasser8");

                return stakeholderIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn stakeholder IDs by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 12:04:03");
                return new List<int>();
            }
        }

        // Helper method to get yarn stakeholders for dropdown
        private SelectList GetYarnStakeholders()
        {
            try
            {
                var yarnStakeholderIds = GetYarnStakeholderIds();

                var yarnStakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll()
                    .Where(s => yarnStakeholderIds.Contains(s.Id))
                    .OrderBy(s => s.Name)
                    .Select(s => new {
                        Value = s.Id,
                        Text = s.Name
                    })
                    .ToList();

                return new SelectList(yarnStakeholders, "Value", "Text");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn stakeholders dropdown by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 12:04:03");
                return new SelectList(new List<object>(), "Value", "Text");
            }
        }

        // GET: YarnTransactions/Overview
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
        // GET: YarnTransactions/ItemDetails/{id}
        public IActionResult ItemDetails(int id, int page = 1, int pageSize = 10)
        {
            try
            {
                var currentTime = "2025-09-04 19:10:25";
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("🔍 ItemDetails requested for ID {ItemId} by {User} at {Time}",
                    id, currentUser, currentTime);

                // Get yarn item with includes
                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "OriginYarn,Manufacturers")
                    .FirstOrDefault(y => y.Id == id && y.Status);

                if (yarnItem == null)
                {
                    _logger.LogWarning("❌ Yarn item {ItemId} not found by {User} at {Time}",
                        id, currentUser, currentTime);
                    return Json(new { success = false, message = "لم يتم العثور على الصنف المطلوب" });
                }

                // Debug logging
                _logger.LogInformation("✅ Yarn item loaded: ID={Id}, Name='{Name}', OriginYarn='{Origin}', ManufacturerCount={Count}",
                    yarnItem.Id,
                    yarnItem.Item ?? "NULL",
                    yarnItem.OriginYarn?.Item ?? "NULL",
                    yarnItem.Manufacturers?.Count ?? 0);

                // Get transactions
                var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(includeEntities: "StakeholderType,Stakeholder,PackagingStyle")
                    .Where(t => t.YarnItemId == id)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .ToList();

                var totalTransactions = allTransactions.Count;
                var totalPages = (int)Math.Ceiling((double)totalTransactions / pageSize);
                page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

                var transactions = allTransactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        TransactionId = t.TransactionId ?? "غير محدد",
                        InternalId = t.InternalId,
                        ExternalId = t.ExternalId,
                        Quantity = t.Inbound > 0 ? t.Inbound : (t.Outbound > 0 ? t.Outbound : 0),
                        IsInbound = t.Inbound > 0,
                        Count = t.Count,
                        StakeholderTypeName = t.StakeholderType?.Type ?? "غير محدد",
                        StakeholderName = t.Stakeholder?.Name ?? "غير محدد",
                        PackagingStyleName = t.PackagingStyle?.StyleName ?? "غير محدد",
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance,
                        Date = t.Date,
                        Comment = t.Comment ?? ""
                    })
                    .ToList();

                // Calculate summary statistics
                var latestTransaction = allTransactions.FirstOrDefault();
                var quantityBalance = latestTransaction?.QuantityBalance ?? 0;
                var countBalance = latestTransaction?.CountBalance ?? 0;
                var totalInbound = allTransactions.Sum(t => t.Inbound);
                var totalOutbound = allTransactions.Sum(t => t.Outbound);

                // ✅ Prepare names with proper debugging
                var yarnItemName = yarnItem.Item ?? "صنف بدون اسم";
                var originYarnName = yarnItem.OriginYarn?.Item ?? "غزل أصلي غير محدد";
                var manufacturerNames = yarnItem.Manufacturers != null && yarnItem.Manufacturers.Any()
                    ? string.Join("، ", yarnItem.Manufacturers.Select(m => m.Name ?? "بدون اسم"))
                    : "شركة مصنعة غير محددة";

                _logger.LogInformation("📋 Final names: YarnItem='{YarnName}', Origin='{Origin}', Manufacturer='{Manufacturer}'",
                    yarnItemName, originYarnName, manufacturerNames);

                // ✅ Use explicit property names (avoid conflicts with reserved words)
                var result = new
                {
                    success = true,
                    yarnItem = new
                    {
                        itemId = yarnItem.Id,
                        itemName = yarnItemName,
                        originYarnName = originYarnName,
                        manufacturerNames = manufacturerNames,
                        quantityBalance = quantityBalance,
                        countBalance = countBalance,
                        totalInbound = totalInbound,
                        totalOutbound = totalOutbound,
                        totalTransactions = totalTransactions,
                        isAvailable = quantityBalance > 0
                    },
                    transactions = transactions,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalPages = totalPages,
                        totalTransactions = totalTransactions,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    }
                };

                _logger.LogInformation("✅ Returning result for yarn item {ItemId} with {TransactionCount} transactions by {User} at {Time}",
                    id, totalTransactions, currentUser, currentTime);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in ItemDetails for item {ItemId} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-04 19:10:25");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحميل تفاصيل الصنف. يرجى المحاولة مرة أخرى."
                });
            }
        }

        // API: GET /api/YarnTransactions/{id}/details
        [HttpGet]
        [Route("api/YarnTransactions/{id}/details")]
        public IActionResult GetTransactionDetails(int id)
        {
            try
            {
                var transaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetOne(t => t.Id == id,
                        includeEntities: "YarnItem,StakeholderType,Stakeholder,PackagingStyle,YarnItem.OriginYarn,YarnItem.Manufacturer");

                if (transaction == null)
                {
                    return NotFound(new { message = "المعاملة غير موجودة" });
                }

                var result = new
                {
                    id = transaction.Id,
                    transactionId = transaction.TransactionId,
                    internalId = transaction.InternalId ?? "غير محدد",
                    externalId = transaction.ExternalId ?? "غير محدد",
                    yarnItem = transaction.YarnItem.Item,
                    originYarn = transaction.YarnItem.OriginYarn?.Item ?? "غير محدد",
                    // Replace this line:

                    // With this corrected code:
                    manufacturer = (transaction.YarnItem.Manufacturers != null && transaction.YarnItem.Manufacturers.Any())
                        ? string.Join("، ", transaction.YarnItem.Manufacturers.Select(m => m.Name))
                        : "غير محدد",
                    quantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound,
                    count = transaction.Count,
                    stakeholderType = transaction.StakeholderType.Type,
                    stakeholder = transaction.Stakeholder.Name,
                    packagingStyle = transaction.PackagingStyle.StyleName,
                    quantityBalance = transaction.QuantityBalance,
                    countBalance = transaction.CountBalance,
                    date = transaction.Date.ToString("yyyy-MM-dd"),
                    comment = transaction.Comment ?? "لا يوجد بيان",
                    createdAt = "2025-08-12 02:13:55",
                    createdBy = "Ammar-Yasser8"
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn transaction details for ID {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-08-12 02:13:55");
                return StatusCode(500, new { message = "حدث خطأ أثناء تحميل تفاصيل المعاملة" });
            }
        }

        // API: GET - Get stakeholders by type (inherited from Raw pattern)
        [HttpGet]
        public JsonResult GetStakeholdersByType(int typeId)
        {
            try
            {
                var allStakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes")
                    .ToList();

                if (!allStakeholders.Any())
                {
                    _logger.LogWarning("No stakeholders found in database at {Time} by {User}",
                        "2025-08-12 02:13:55", "Ammar-Yasser8");
                    return Json(new { error = "No stakeholders found", data = new List<object>() });
                }

                var activeStakeholders = allStakeholders.Where(s => s.Status).ToList();
                if (!activeStakeholders.Any())
                {
                    _logger.LogWarning("No active stakeholders found at {Time} by {User}",
                        "2025-08-12 02:13:55", "Ammar-Yasser8");
                    return Json(new { error = "No active stakeholders found", data = new List<object>() });
                }

                var stakeholdersWithType = activeStakeholders
                    .Where(s => s.StakeholderInfoTypes != null &&
                               s.StakeholderInfoTypes.Any(st => st.StakeholderTypeId == typeId))
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name
                    })
                    .OrderBy(s => s.name)
                    .ToList();

                if (!stakeholdersWithType.Any())
                {
                    _logger.LogWarning(
                        "No stakeholders found for type {TypeId}. User: {User}, Time: {Time}",
                        typeId, "Ammar-Yasser8", "2025-08-12 02:13:55");

                    return Json(new
                    {
                        error = "No stakeholders found for selected type",
                        data = new List<object>()
                    });
                }

                return Json(new { success = true, data = stakeholdersWithType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stakeholders for type {TypeId}. User: {User}, Time: {Time}",
                    typeId, "Ammar-Yasser8", "2025-08-12 02:13:55");

                return Json(new
                {
                    error = "An error occurred while fetching stakeholders",
                    data = new List<object>()
                });
            }
        }

        private List<SelectListItem> GetStakeholderTypes()
        {
            try
            {
                _logger.LogInformation(
                    "Getting stakeholder types for yarn transactions at {Time} by {User}",
                    "2025-08-12 02:13:55", "Ammar-Yasser8");

                var allTypes = _unitOfWork.Repository<StakeholderType>()
                    .GetAll()
                    .ToList();

                // Filter for yarn-related stakeholder types
                var searchTerm = "تاجر غزل"; // Yarn trader
                var filteredTypes = allTypes
                    .Where(st => st.Type?.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = st.Type
                    })
                    .OrderBy(i => i.Text)
                    .ToList();

                // If no yarn-specific types found, include general trader types
                if (!filteredTypes.Any())
                {
                    var generalSearchTerm = "تاجر";
                    filteredTypes = allTypes
                        .Where(st => st.Type?.IndexOf(generalSearchTerm, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        .Select(st => new SelectListItem
                        {
                            Value = st.Id.ToString(),
                            Text = st.Type
                        })
                        .OrderBy(i => i.Text)
                        .ToList();
                }

                _logger.LogInformation(
                    "Found {Count} stakeholder types for yarn transactions",
                    filteredTypes.Count);

                return filteredTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error in GetStakeholderTypes for yarn transactions at {Time} by {User}",
                    "2025-08-12 02:13:55", "Ammar-Yasser8");

                return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "حدث خطأ أثناء تحميل أنواع التجار"
                }
            };
            }
        }


        [HttpGet]
        public JsonResult GetStakeholdersByForm(string formName)
        {
            try
            {
                _logger.LogInformation(
                    "Getting stakeholders for form {FormName} at {Time} by {User}",
                    formName, "2025-08-12 02:13:55", "Ammar-Yasser8");

                if (string.IsNullOrEmpty(formName))
                {
                    return Json(new
                    {
                        success = false,
                        error = "اسم النموذج غير محدد",
                        data = new List<object>()
                    });
                }

                // Find the form style by name
                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetAll()
                    .FirstOrDefault(f => f.FormName == formName);

                if (formStyle == null)
                {
                    _logger.LogWarning(
                        "Form style not found for {FormName} at {Time} by {User}",
                        formName, "2025-08-12 02:13:55", "Ammar-Yasser8");

                    return Json(new
                    {
                        success = false,
                        error = "نوع النموذج غير موجود",
                        data = new List<object>()
                    });
                }

                // Find allowed stakeholder type IDs for this form
                var allowedTypeIds = _unitOfWork.Repository<StakeholderType>()
                    .GetAll(includeEntities: "StakeholderTypeForms")
                    .Where(st => st.StakeholderTypeForms.Any(f => f.FormId == formStyle.Id))
                    .Select(st => st.Id)
                    .ToList();

                // Get active stakeholders having one of the allowed types
                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes")
                    .Where(s => s.Status && s.StakeholderInfoTypes.Any(st => allowedTypeIds.Contains(st.StakeholderTypeId)))
                    .Select(s => new { id = s.Id, name = s.Name })
                    .OrderBy(s => s.name)
                    .ToList();

                if (!stakeholders.Any())
                {
                    _logger.LogInformation(
                        "No stakeholders found for form {FormName} at {Time} by {User}",
                        formName, "2025-08-12 02:13:55", "Ammar-Yasser8");
                    return Json(new
                    {
                        success = false,
                        error = "لا توجد جهات نشطة لهذا النموذج",
                        data = new List<object>()
                    });
                }

                return Json(new { success = true, data = stakeholders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stakeholders for form {FormName} by {User} at {Time}",
                    formName, "Ammar-Yasser8", "2025-08-12 02:13:55");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل الجهات للنموذج",
                    data = new List<object>()
                });
            }
        }



        // API: GET - Get packaging styles by form type
        [HttpGet]
        public JsonResult GetPackagingStylesByForm(string formType)
        {
            try
            {
                _logger.LogInformation(
                    "Getting packaging styles for yarn form type {FormType} at {Time} by {User}",
                    formType, "2025-08-12 02:13:55", "Ammar-Yasser8");

                if (string.IsNullOrEmpty(formType))
                {
                    return Json(new
                    {
                        success = false,
                        error = "نوع النموذج غير محدد",
                        data = new List<object>()
                    });
                }

                var formStyle = _unitOfWork.Repository<FormStyle>()
                    .GetAll()
                    .FirstOrDefault(f => f.FormName == formType);

                if (formStyle == null)
                {
                    _logger.LogWarning(
                        "Form style not found for {FormType} at {Time} by {User}",
                        formType, "2025-08-12 02:13:55", "Ammar-Yasser8");

                    return Json(new
                    {
                        success = false,
                        error = "نوع النموذج غير موجود",
                        data = new List<object>()
                    });
                }

                var styles = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll(includeEntities: "PackagingStyleForms")
                    .Where(ps => ps.PackagingStyleForms.Any(f => f.FormId == formStyle.Id))
                    .Select(ps => new { id = ps.Id, name = ps.StyleName })
                    .OrderBy(s => s.name)
                    .ToList();

                _logger.LogInformation(
                    "Found {Count} packaging styles for yarn form {FormType} at {Time} by {User}",
                    styles.Count, formType, "2025-08-12 02:13:55", "Ammar-Yasser8");

                return Json(new { success = true, data = styles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting packaging styles for yarn form {FormType} by {User} at {Time}",
                    formType, "Ammar-Yasser8", "2025-08-12 02:13:55");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل أنماط التعبئة",
                    data = new List<object>()
                });
            }
        }

        [HttpGet]
        public JsonResult GetYarnItemBalance(int yarnItemId)
        {
            try
            {
                if (yarnItemId <= 0)
                {
                    return Json(new { success = false, error = "معرف الصنف غير صحيح" });
                }

                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(y => y.Id == yarnItemId, includeEntities: "OriginYarn,Manufacturers");

                if (yarnItem == null)
                {
                    return Json(new { success = false, error = "الصنف غير موجود" });
                }

                // Get latest transaction for current balances
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                var quantityBalance = latestTransaction?.QuantityBalance ?? 0;
                var countBalance = latestTransaction?.CountBalance ?? 0;

                // ✅ Get packaging breakdown by type
                var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId, includeEntities: "PackagingStyle")
                    .ToList();

                var packagingBreakdown = allTransactions
                    .GroupBy(t => new {
                        PackagingId = t.PackagingStyleId,
                        PackagingName = t.PackagingStyle?.StyleName ?? "غير محدد"
                    })
                    .Select(g => new
                    {
                        packagingId = g.Key.PackagingId,
                        packagingType = g.Key.PackagingName,
                        totalCount = g.Sum(t => (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0))
                    })
                    .Where(p => p.totalCount > 0)
                    .OrderByDescending(p => p.totalCount)
                    .ToList();

                // ✅ Create comprehensive packaging display
                string packagingDisplay;
                string shortPackagingDisplay;

                if (packagingBreakdown.Any())
                {
                    // Full display: "٥٥ شكارة + ٢٠ صندوق + ١٠ كيس"
                    var packagingParts = packagingBreakdown.Select(p =>
                        $"{ToArabicDigits(p.totalCount.ToString())} {p.packagingType}");
                    packagingDisplay = string.Join(" + ", packagingParts);

                    // Short display for small spaces (show top 2 packaging types)
                    var topPackaging = packagingBreakdown.Take(2).Select(p =>
                        $"{ToArabicDigits(p.totalCount.ToString())} {p.packagingType}");
                    shortPackagingDisplay = string.Join(" + ", topPackaging);

                    if (packagingBreakdown.Count > 2)
                    {
                        var remainingCount = packagingBreakdown.Skip(2).Sum(p => p.totalCount);
                        shortPackagingDisplay += $" + {ToArabicDigits(remainingCount.ToString())} أخرى";
                    }
                }
                else if (countBalance > 0)
                {
                    packagingDisplay = $"{ToArabicDigits(countBalance.ToString())} وحدة غير محددة";
                    shortPackagingDisplay = packagingDisplay;
                }
                else
                {
                    packagingDisplay = "لا توجد وحدات";
                    shortPackagingDisplay = "لا توجد وحدات";
                }

                var result = new
                {
                    success = true,
                    yarnItemId = yarnItem.Id,
                    yarnItem = yarnItem.Item ?? "غير محدد",
                    originYarn = yarnItem.OriginYarn?.Item ?? "غير محدد",
                    manufacturer = (yarnItem.Manufacturers != null && yarnItem.Manufacturers.Any())
                        ? string.Join("، ", yarnItem.Manufacturers.Select(m => m.Name))
                        : "غير محدد",
                    quantityBalance = Math.Round(quantityBalance, 3),
                    countBalance = countBalance,
                    packagingBreakdown = packagingBreakdown,
                    packagingDisplay = packagingDisplay, // "٥٥ شكارة + ٢٠ صندوق"
                    shortPackagingDisplay = shortPackagingDisplay, // For compact display
                    hasMultiplePackaging = packagingBreakdown.Count > 1,
                    transactionCount = allTransactions.Count,
                    lastTransactionDate = latestTransaction?.Date.ToString("yyyy-MM-dd") ?? "لا توجد معاملات",
                    hasTransactions = allTransactions.Any()
                };

                _logger.LogInformation("Yarn item balance loaded for {YarnItemId}: Qty={Qty}, Count={Count}, " +
                    "Packaging Types={PackagingTypes}, Display={PackagingDisplay} by {User} at {Time}",
                    yarnItemId, quantityBalance, countBalance, packagingBreakdown.Count, packagingDisplay,
                    "Ammar-Yasser8", "2025-09-04 17:35:43");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn item balance for ID {YarnItemId} by {User} at {Time}",
                    yarnItemId, "Ammar-Yasser8", "2025-09-04 17:35:43");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل رصيد الصنف"
                });
            }
        }

        // ✅ Add helper method for Arabic digit conversion
        private string ToArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input.Replace('0', '٠').Replace('1', '١').Replace('2', '٢')
                        .Replace('3', '٣').Replace('4', '٤').Replace('5', '٥')
                        .Replace('6', '٦').Replace('7', '٧').Replace('8', '٨')
                        .Replace('9', '٩');
        }

        // Helper method to determine balance status
        private string GetBalanceStatus(decimal quantityBalance, int countBalance)
        {
            if (quantityBalance == 0 && countBalance == 0)
                return "متوازن";
            else if (quantityBalance > 0 && countBalance > 0)
                return "رصيد موجب";
            else if (quantityBalance < 0 || countBalance < 0)
                return "رصيد سالب";
            else
                return "رصيد مختلط";
        }

        // Alternative method for bulk balance checking
        [HttpPost]
        public JsonResult GetYarnItemBalance([FromBody] List<int> yarnItemIds)
        {
            try
            {
                if (yarnItemIds == null || !yarnItemIds.Any())
                {
                    return Json(new { success = false, error = "لم يتم تحديد أي أصناف" });
                }

                var results = new List<object>();

                foreach (var id in yarnItemIds.Where(id => id > 0))
                {
                    var yarnItem = _unitOfWork.Repository<YarnItem>()
                        .GetOne(y => y.Id == id, includeEntities: "YarnTransactions,OriginYarn,Manufacturer");

                    if (yarnItem != null)
                    {
                        var transactions = yarnItem.YarnTransactions ?? new List<YarnTransaction>();
                        var quantityBalance = transactions.Sum(t => (t?.Inbound ?? 0) - (t?.Outbound ?? 0));
                        var countBalance = transactions.Sum(t =>
                            ((t?.Inbound ?? 0) > 0 ? (t?.Count ?? 0) : 0) -
                            ((t?.Outbound ?? 0) > 0 ? (t?.Count ?? 0) : 0));

                        results.Add(new
                        {
                            yarnItemId = id,
                            yarnItem = yarnItem.Item ?? "غير محدد",
                            quantityBalance = Math.Round(quantityBalance, 3),
                            countBalance = countBalance,
                            status = GetBalanceStatus(quantityBalance, countBalance)
                        });
                    }
                }

                return Json(new {
                    success = true,
                    data = results,
                    totalItems = results.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple yarn item balances by {User} at {Time}",
                    User.Identity?.Name ?? "Unknown", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return Json(new {
                    success = false,
                    error = "حدث خطأ أثناء تحميل أرصدة الأصناف"
                });
            }
        }

        // Helper Methods
        private List<SelectListItem> GetActiveYarnItems()
        {
            try
            {
                return _unitOfWork.Repository<YarnItem>()
                    .GetAll(y => y.Status, includeEntities: "OriginYarn")
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.OriginYarn != null ? $"{y.Item} ({y.OriginYarn.Item})" : y.Item
                    })
                    .OrderBy(i => i.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active yarn items by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                return new List<SelectListItem>();
            }
        }


        private string GenerateTransactionId()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "YT"; // Yarn Transaction

                var lastTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.TransactionId.StartsWith($"{prefix}-{date}"))
                    .OrderByDescending(t => t.TransactionId)
                    .FirstOrDefault();

                int sequence = 1;
                if (lastTransaction != null)
                {
                    var parts = lastTransaction.TransactionId.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts.Last(), out int lastSequence))
                    {
                        sequence = lastSequence + 1;
                    }
                }

                var transactionId = $"{prefix}-{date}-{sequence:D4}";

                _logger.LogInformation("Generated yarn transaction ID: {TransactionId} by {User} at {Time}",
                    transactionId, "Ammar-Yasser8", "2025-08-12 02:13:55");

                return transactionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating yarn transaction ID by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                return $"YT-{DateTime.Now:yyyyMMdd}-0001";
            }
        }
    
    // GET: YarnTransactions/ResetBalance
        public IActionResult ResetBalance()
        {
            try
            {
                var viewModel = new YarnResetBalanceViewModel
                {
                    AvailableItems = GetActiveYarnItems(),
                    ResetDate = DateTime.Now,
                    ResetBy = "Ammar-Yasser8"
                };

                _logger.LogInformation("Reset balance form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 18:04:08");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reset balance form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 18:04:08");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة تصفير الرصيد";
                return RedirectToAction("Overview", "YarnTransactions");
            }
        }

        // POST: YarnTransactions/ResetBalance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetBalance(YarnResetBalanceViewModel model)
        {
            try
            {
                ModelState.Remove("YarnItemName");
                ModelState.Remove("ResetBy");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    return View(model);
                }

                // Get current balance
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == model.YarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                var currentQuantityBalance = latestTransaction?.QuantityBalance ?? 0;
                var currentCountBalance = latestTransaction?.CountBalance ?? 0;

                // Check if already zero
                if (currentQuantityBalance == 0 && currentCountBalance == 0)
                {
                    ModelState.AddModelError("YarnItemId", "رصيد هذا الصنف بالفعل صفر");
                    model.AvailableItems = GetActiveYarnItems();
                    return View(model);
                }

                // Confirm reset operation if not already confirmed
                if (!model.ConfirmReset)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    model.CurrentQuantityBalance = currentQuantityBalance;
                    model.CurrentCountBalance = currentCountBalance;
                    model.ShowConfirmation = true;

                    // Get yarn item name for display
                    var yarnItem = _unitOfWork.Repository<YarnItem>().GetOne(yi=>yi.Id==model.YarnItemId);
                    model.YarnItemName = yarnItem?.Item;

                    return View(model);
                }

                // Create reset transaction
                var resetTransaction = new YarnTransaction
                {
                    TransactionId = GenerateResetTransactionId(),
                    InternalId = model.InternalId?.Trim(),
                    ExternalId = "RESET-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    YarnItemId = model.YarnItemId,

                    // ✅ Create outbound transaction to zero the balance
                    Inbound = 0,
                    Outbound = currentQuantityBalance > 0 ? currentQuantityBalance : 0,
                    Count = Math.Abs(currentCountBalance), // Use absolute value for count

                    StakeholderId = 1, // System/Admin stakeholder (you may need to create this)
                    PackagingStyleId = 1, // Default packaging style
                    Date = model.ResetDate,
                    Comment = $"تصفير رصيد - السبب: {model.ReasonForReset?.Trim() ?? "تعديل إداري"} - بواسطة: Ammar-Yasser8"
                };

                // ✅ Set balances to exactly zero
                resetTransaction.QuantityBalance = 0;
                resetTransaction.CountBalance = 0;

                _unitOfWork.Repository<YarnTransaction>().Add(resetTransaction);
                _unitOfWork.Complete();

                _logger.LogInformation("Balance reset completed for YarnItem {YarnItemId}: " +
                    "Previous Qty={PreviousQty}, Previous Count={PreviousCount}, " +
                    "Reset Transaction={TransactionId}, Reason={Reason} by {User} at {Time}",
                    model.YarnItemId, currentQuantityBalance, currentCountBalance,
                    resetTransaction.TransactionId, model.ReasonForReset,
                    "Ammar-Yasser8", "2025-09-04 18:04:08");

                TempData["Success"] = $"تم تصفير رصيد الصنف بنجاح - رقم المعاملة: {resetTransaction.TransactionId}";
                return RedirectToAction("Overview", "YarnTransactions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting balance by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 18:04:08");
                ModelState.AddModelError("", "حدث خطأ أثناء تصفير الرصيد");
                model.AvailableItems = GetActiveYarnItems();
                return View(model);
            }
        }

        // Helper method to generate reset transaction ID
        private string GenerateResetTransactionId()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "RST"; // Reset Transaction

                var lastResetTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.TransactionId.StartsWith($"{prefix}-{date}"))
                    .OrderByDescending(t => t.TransactionId)
                    .FirstOrDefault();

                int sequence = 1;
                if (lastResetTransaction != null)
                {
                    var parts = lastResetTransaction.TransactionId.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts.Last(), out int lastSequence))
                    {
                        sequence = lastSequence + 1;
                    }
                }

                var transactionId = $"{prefix}-{date}-{sequence:D4}";

                _logger.LogInformation("Generated reset transaction ID: {TransactionId} by {User} at {Time}",
                    transactionId, "Ammar-Yasser8", "2025-09-04 18:04:08");

                return transactionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reset transaction ID by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 18:04:08");
                return $"RST-{DateTime.Now:yyyyMMdd}-0001";
            }
        }
    }
}
