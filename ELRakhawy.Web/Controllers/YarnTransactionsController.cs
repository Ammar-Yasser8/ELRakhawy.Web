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
                    StakeholderTypes = GetStakeholderTypes()
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
                    StakeholderTypes = GetStakeholderTypes()
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

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    model.StakeholderTypes = GetStakeholderTypes();
                    return View("TransactionForm", model);
                }

                // Calculate current balance
                var currentBalance = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == model.YarnItemId)
                    .Sum(t => t.Inbound - t.Outbound);

                // Check if outbound quantity exceeds available balance
                if (!model.IsInbound && model.Quantity > currentBalance)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({currentBalance:N3})");
                    model.AvailableItems = GetActiveYarnItems();
                    model.StakeholderTypes = GetStakeholderTypes();
                    return View("TransactionForm", model);
                }

                var transaction = new YarnTransaction
                {
                    TransactionId = GenerateTransactionId(),
                    InternalId = model.InternalId,
                    ExternalId = model.ExternalId,
                    YarnItemId = model.YarnItemId,
                    Inbound = model.IsInbound ? model.Quantity : 0,
                    Outbound = model.IsInbound ? 0 : model.Quantity,
                    Count = model.Count,
                    
                    StakeholderId = model.StakeholderId,
                    PackagingStyleId = model.PackagingStyleId,
                    Date = model.Date,
                    Comment = model.Comment?.Trim()
                };

                // Calculate balances
                var previousCountBalance = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == model.YarnItemId)
                    .Sum(t => (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));

                transaction.QuantityBalance = currentBalance + (transaction.Inbound - transaction.Outbound);
                transaction.CountBalance = previousCountBalance +
                    (transaction.Inbound > 0 ? transaction.Count : 0) -
                    (transaction.Outbound > 0 ? transaction.Count : 0);

                _unitOfWork.Repository<YarnTransaction>().Add(transaction);
                _unitOfWork.Complete();

                _logger.LogInformation("Yarn transaction {TransactionId} saved successfully - {Type} by {User} at {Time}",
                    transaction.TransactionId, model.IsInbound ? "Inbound" : "Outbound",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");

                TempData["Success"] = $"تم تسجيل {(model.IsInbound ? "وارد" : "صادر")} الغزل بنجاح - رقم الإذن: {transaction.TransactionId}";
                return RedirectToAction("Index", "YarnItems");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving yarn transaction by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ المعاملة");
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
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
                    Results = new List<YarnTransactionViewModel>()
                };

                _logger.LogInformation("Yarn transaction search form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn transaction search form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 02:13:55");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة البحث";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // GET: YarnTransactions/Overview
        public IActionResult Overview(bool availableOnly = false)
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("Yarn overview page loaded by {User} at {Time} - Available only: {AvailableOnly}",
                    currentUser, currentTime, availableOnly);

                // Get all active yarn items with their transactions
                var yarnItems = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "OriginYarn,Manufacturer")
                    .Where(y => y.Status)
                    .ToList();

                var overviewItems = new List<YarnOverviewItemViewModel>();

                foreach (var yarnItem in yarnItems)
                {
                    // Get all transactions for this yarn item
                    var transactions = _unitOfWork.Repository<YarnTransaction>()
                        .GetAll(t => t.YarnItemId == yarnItem.Id)
                        .ToList();

                    // Calculate balances
                    var quantityBalance = transactions.Sum(t => t.Inbound - t.Outbound);
                    var countBalance = transactions.Sum(t =>
                        (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));

                    // Get latest transaction date
                    var lastTransactionDate = transactions.Any() ?
                        transactions.Max(t => t.Date) : (DateTime?)null;

                    // Get transaction counts
                    var totalTransactions = transactions.Count;
                    var inboundTransactions = transactions.Count(t => t.Inbound > 0);
                    var outboundTransactions = transactions.Count(t => t.Outbound > 0);

                    var overviewItem = new YarnOverviewItemViewModel
                    {
                        YarnItemId = yarnItem.Id,
                        YarnItemName = yarnItem.Item,
                        OriginYarnName = yarnItem.OriginYarn?.Item,
                        ManufacturerName = yarnItem.Manufacturer?.Name,
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

                _logger.LogInformation("Yarn overview completed by {User} at {Time} - Found {Count} items, Available: {Available}",
                    currentUser, currentTime, viewModel.TotalItems, viewModel.AvailableItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn overview by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TempData["Error"] = "حدث خطأ أثناء تحميل نظرة عامة على الغزل";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // GET: YarnTransactions/ItemDetails/{id}
        public IActionResult ItemDetails(int id, int page = 1, int pageSize = 10)
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("Yarn item details requested for item {ItemId} by {User} at {Time}",
                    id, currentUser, currentTime);

                // Get yarn item details
                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetAll(includeEntities: "OriginYarn,Manufacturer")
                    .FirstOrDefault(y => y.Id == id && y.Status);

                if (yarnItem == null)
                {
                    return Json(new { success = false, message = "لم يتم العثور على الصنف المطلوب" });
                }

                // Get transactions with pagination
                var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(includeEntities: "StakeholderType,Stakeholder,PackagingStyle")
                    .Where(t => t.YarnItemId == id)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .ToList();

                var totalTransactions = allTransactions.Count;
                var transactions = allTransactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        transactionId = t.TransactionId,
                        internalId = t.InternalId,
                        externalId = t.ExternalId,
                        quantity = t.Inbound > 0 ? t.Inbound : (t.Outbound > 0 ? t.Outbound : 0),
                        isInbound = t.Inbound > 0,
                        count = t.Count,
                        stakeholderTypeName = t.StakeholderType?.Type ?? "غير محدد",
                        stakeholderName = t.Stakeholder?.Name ?? "غير محدد",
                        packagingStyleName = t.PackagingStyle?.StyleName ?? "غير محدد",
                        quantityBalance = t.QuantityBalance,
                        date = t.Date,
                        comment = t.Comment
                    })
                    .ToList();

                // Calculate summary statistics
                var quantityBalance = allTransactions.Sum(t => t.Inbound - t.Outbound);
                var countBalance = allTransactions.Sum(t =>
                    (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));
                var totalInbound = allTransactions.Sum(t => t.Inbound);
                var totalOutbound = allTransactions.Sum(t => t.Outbound);

                var result = new
                {
                    success = true,
                    yarnItem = new
                    {
                        id = yarnItem.Id,
                        name = yarnItem.Item,
                        originYarn = yarnItem.OriginYarn?.Item,
                        manufacturer = yarnItem.Manufacturer?.Name,
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
                        totalPages = (int)Math.Ceiling((double)totalTransactions / pageSize),
                        totalTransactions = totalTransactions,
                        hasNextPage = page * pageSize < totalTransactions,
                        hasPreviousPage = page > 1
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn item details for item {ItemId} by {User} at {Time}",
                    id, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل تفاصيل الصنف" });
            }
        }

        // POST: YarnTransactions/ShareWhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShareWhatsApp(bool availableOnly = false)
        {
            try
            {
                var overviewData = GetOverviewData(availableOnly);
                var message = GenerateWhatsAppMessage(overviewData);

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating WhatsApp message by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return Json(new { success = false, message = "حدث خطأ أثناء إنشاء رسالة واتساب" });
            }
        }

        // Helper method to get overview data
        private List<YarnOverviewItemViewModel> GetOverviewData(bool availableOnly)
        {
            var yarnItems = _unitOfWork.Repository<YarnItem>()
                .GetAll(includeEntities: "OriginYarn,Manufacturer")
                .Where(y => y.Status)
                .ToList();

            var overviewItems = new List<YarnOverviewItemViewModel>();

            foreach (var yarnItem in yarnItems)
            {
                var transactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItem.Id)
                    .ToList();

                var quantityBalance = transactions.Sum(t => t.Inbound - t.Outbound);
                var countBalance = transactions.Sum(t =>
                    (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));

                var overviewItem = new YarnOverviewItemViewModel
                {
                    YarnItemId = yarnItem.Id,
                    YarnItemName = yarnItem.Item,
                    OriginYarnName = yarnItem.OriginYarn?.Item,
                    ManufacturerName = yarnItem.Manufacturer?.Name,
                    QuantityBalance = quantityBalance,
                    CountBalance = countBalance,
                    IsAvailable = quantityBalance > 0,
                    TotalTransactions = transactions.Count,
                    LastTransactionDate = transactions.Any() ? transactions.Max(t => t.Date) : (DateTime?)null
                };

                if (!availableOnly || overviewItem.IsAvailable)
                {
                    overviewItems.Add(overviewItem);
                }
            }

            return overviewItems.OrderByDescending(item => item.QuantityBalance)
                               .ThenBy(item => item.YarnItemName)
                               .ToList();
        }

 
        private byte[] GenerateExcelOverview(List<YarnOverviewItemViewModel> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Yarn Overview");

                // Add headers
                worksheet.Cells[1, 1].Value = "صنف الغزل";
                worksheet.Cells[1, 2].Value = "الغزل الأصلي";
                // Add more headers...

                // Add data
                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = data[i].YarnItemName;
                    worksheet.Cells[i + 2, 2].Value = data[i].OriginYarnName;
                    // Add more data...
                }

                return package.GetAsByteArray();
            }
        }

        private string GenerateWhatsAppMessage(List<YarnOverviewItemViewModel> data)
        {
            var message = new StringBuilder();
            message.AppendLine("📊 *نظرة عامة على أرصدة الغزل*");
            message.AppendLine($"📅 تاريخ التقرير: {DateTime.Now:dd/MM/yyyy HH:mm}");
            message.AppendLine("━━━━━━━━━━━━━━━━━━━━");

            var availableItems = data.Where(item => item.IsAvailable).ToList();
            message.AppendLine($"📈 إجمالي الأصناف: {data.Count}");
            message.AppendLine($"✅ الأصناف المتاحة: {availableItems.Count}");
            message.AppendLine($"📦 إجمالي الكمية: {data.Sum(item => item.QuantityBalance):N2}");
            message.AppendLine();

            if (availableItems.Any())
            {
                message.AppendLine("*الأصناف المتاحة:*");
                foreach (var item in availableItems.Take(10))
                {
                    message.AppendLine($"• {item.YarnItemName}: {item.QuantityBalance:N2}");
                }

                if (availableItems.Count > 10)
                {
                    message.AppendLine($"... و {availableItems.Count - 10} صنف آخر");
                }
            }

            return message.ToString();
        }


        // POST: YarnTransactions/Search 
        [HttpPost]
        public IActionResult Search(YarnTransactionSearchViewModel model)
        {
            try
            {
                var currentTime = "2025-08-12 11:37:18";
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

                var query = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(includeEntities: "YarnItem,StakeholderType,Stakeholder,PackagingStyle,YarnItem.OriginYarn,YarnItem.Manufacturer")
                    .AsEnumerable();

                // Apply enhanced filters
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
                    _logger.LogDebug("Applied StakeholderId filter: {StakeholderId}", model.StakeholderId.Value);
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

                // Set search performed flag
                ViewBag.SearchPerformed = true;

                _logger.LogInformation("Enhanced yarn transaction search completed by {User} at {Time} - Found {Count} results, TotalInbound: {TotalInbound}, TotalOutbound: {TotalOutbound}, NetBalance: {NetBalance}",
                    currentUser, currentTime, results.Count, model.TotalInbound, model.TotalOutbound, model.NetBalance);

                // Add success message if results found
                if (results.Any())
                {
                    TempData["Success"] = $"تم العثور على {results.Count} معاملة مطابقة لمعايير البحث";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in enhanced yarn transaction search by {User} at {Time}",
                    "Ammar-Yasser8", "2025-08-12 11:37:18");

                TempData["Error"] = "حدث خطأ أثناء البحث في معاملات الغزل";

                // Ensure model has required data for error display
                model.Results = new List<YarnTransactionViewModel>();
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
                ViewBag.SearchPerformed = true;

                return View(model);
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
                    manufacturer = transaction.YarnItem.Manufacturer?.Name ?? "غير محدد",
                    type = transaction.Inbound > 0 ? "وارد" : "صادر",
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

        // API: GET - Get yarn item current balance
         // API: GET - Get yarn item current balance
        [HttpGet]
        public JsonResult GetYarnItemBalance(int yarnItemId)
        {
            try
            {
                // Validate input
                if (yarnItemId <= 0)
                {
                    return Json(new { success = false, error = "معرف الصنف غير صحيح" });
                }

                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(y => y.Id == yarnItemId, includeEntities: "YarnTransactions,OriginYarn,Manufacturer");
        
                if (yarnItem == null)
                {
                    return Json(new { success = false, error = "الصنف غير موجود" });
                }

                // Safe calculation of balances with null-safe operations
                var transactions = yarnItem.YarnTransactions ?? new List<YarnTransaction>();
        
                var quantityBalance = transactions.Sum(t => (t?.Inbound ?? 0) - (t?.Outbound ?? 0));
                var countBalance = transactions.Sum(t => 
                    ((t?.Inbound ?? 0) > 0 ? (t?.Count ?? 0) : 0) - 
                    ((t?.Outbound ?? 0) > 0 ? (t?.Count ?? 0) : 0));

                // Safe property access with null checks
                var result = new
                {
                    success = true,
                    yarnItemId = yarnItem.Id,
                    yarnItem = yarnItem.Item ?? "غير محدد",
                    originYarn = yarnItem.OriginYarn?.Item ?? "غير محدد",
                    manufacturer = yarnItem.Manufacturer?.Name ?? "غير محدد",
                    quantityBalance = Math.Round(quantityBalance, 3),
                    countBalance = countBalance,
                    transactionCount = transactions.Count(),
                    lastTransactionDate = transactions.Any() ?
                        transactions.OrderByDescending(t => t.Date).First().Date.ToString("yyyy-MM-dd") :
                        "لا توجد معاملات",
                    hasTransactions = transactions.Any(),
                    status = GetBalanceStatus(quantityBalance, countBalance),
                    details = new
                    {
                        totalInbound = Math.Round(transactions.Sum(t => t?.Inbound ?? 0), 3),
                        totalOutbound = Math.Round(transactions.Sum(t => t?.Outbound ?? 0), 3),
                        inboundCount = transactions.Sum(t => (t?.Inbound ?? 0) > 0 ? (t?.Count ?? 0) : 0),
                        outboundCount = transactions.Sum(t => (t?.Outbound ?? 0) > 0 ? (t?.Count ?? 0) : 0)
                    }
                };

                return Json(result);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Null reference error getting yarn item balance for ID {YarnItemId} by {User} at {Time}",
                    yarnItemId, User.Identity?.Name ?? "Unknown", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        
                return Json(new { 
                    success = false, 
                    error = "حدث خطأ في البيانات - قد تكون بعض المعلومات مفقودة",
                    errorCode = "NULL_REFERENCE"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn item balance for ID {YarnItemId} by {User} at {Time}",
                    yarnItemId, User.Identity?.Name ?? "Unknown", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        
                return Json(new { 
                    success = false, 
                    error = "حدث خطأ أثناء تحميل رصيد الصنف",
                    errorCode = "GENERAL_ERROR"
                });
            }
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
    }
}
