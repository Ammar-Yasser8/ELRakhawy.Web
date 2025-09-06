using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.Web.Controllers
{
    public class RawTransactionsController : Controller
    {
        private readonly ILogger<RawTransactionsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RawTransactionsController(ILogger<RawTransactionsController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: YarnTransactions/Inbound
        public IActionResult Inbound()
        {
            try
            {
                var viewModel = new RawTransactionViewModel
                {
                    IsInbound = true,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveRawItems(),
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
                return RedirectToAction("Index", "RawItems");
            }
        }
        // GET: YarnTransactions/Outbound
        public IActionResult Outbound()
        {
            try
            {
                var viewModel = new RawTransactionViewModel
                {
                    IsInbound = false,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveRawItems(),
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
                return RedirectToAction("Index", "RawItems");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTransaction(RawTransactionViewModel model)
        {
            try
            {
                var currentTime = "2025-09-06 17:35:01";
                var currentUser = "Ammar-Yasser8";

                _logger.LogInformation("🆕 Starting raw transaction save: Type={Type}, RawItemId={ItemId}, Quantity={Quantity}, Weight={Weight}, Count={Count} by {User} at {Time}",
                    model.IsInbound ? "Inbound" : "Outbound", model.RawItemId, model.Quantity, model.Weight, model.Count, currentUser, currentTime);

                // Remove server-side populated fields from validation
                ModelState.Remove("TransactionId");
                ModelState.Remove("RawItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("PackagingStyleName");
                ModelState.Remove("CurrentQuantityBalance");
                ModelState.Remove("CurrentCountBalance");
                ModelState.Remove("Date");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("⚠️ Model validation failed for raw transaction by {User} at {Time}. Errors: {Errors}",
                        currentUser, currentTime, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                    model.AvailableItems = GetActiveRawItems();
                    return View("TransactionForm", model);
                }

                // ✅ Calculate current balance for validation ONLY (not for corrupting data!)
                var currentBalance = CalculateCurrentBalance(model.RawItemId.Value);

                _logger.LogInformation("📊 Current balance for validation: Quantity={Quantity:N3}, Count={Count} by {User} at {Time}",
                    currentBalance.QuantityBalance, currentBalance.CountBalance, currentUser, currentTime);

                // Validate outbound transactions against available balance
                if (!model.IsInbound)
                {
                    var totalOutbound = model.Quantity + model.Weight;

                    if (totalOutbound > currentBalance.QuantityBalance)
                    {
                        var errorMsg = $"الكمية المطلوبة ({totalOutbound:N3}) أكبر من الرصيد المتاح ({currentBalance.QuantityBalance:N3})";
                        _logger.LogWarning("⚠️ Insufficient quantity balance: {Error} by {User} at {Time}", errorMsg, currentUser, currentTime);

                        ModelState.AddModelError("Quantity", errorMsg);
                        model.AvailableItems = GetActiveRawItems();
                        return View("TransactionForm", model);
                    }

                    if (model.Count > currentBalance.CountBalance)
                    {
                        var errorMsg = $"العدد المطلوب ({model.Count}) أكبر من رصيد العدد المتاح ({currentBalance.CountBalance})";
                        _logger.LogWarning("⚠️ Insufficient count balance: {Error} by {User} at {Time}", errorMsg, currentUser, currentTime);

                        ModelState.AddModelError("Count", errorMsg);
                        model.AvailableItems = GetActiveRawItems();
                        return View("TransactionForm", model);
                    }
                }

                // Validate positive quantities
                if (model.Quantity < 0 || model.Weight < 0)
                {
                    ModelState.AddModelError("Quantity", "الكمية والوزن يجب أن تكون أكبر من أو تساوي الصفر");
                    model.AvailableItems = GetActiveRawItems();
                    return View("TransactionForm", model);
                }

                if (model.Count <= 0)
                {
                    ModelState.AddModelError("Count", "العدد يجب أن يكون أكبر من الصفر");
                    model.AvailableItems = GetActiveRawItems();
                    return View("TransactionForm", model);
                }

                // Verify stakeholder exists
                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == model.StakeholderId.Value);

                if (stakeholder == null)
                {
                    _logger.LogWarning("⚠️ Stakeholder not found: {StakeholderId} by {User} at {Time}",
                        model.StakeholderId.Value, currentUser, currentTime);

                    ModelState.AddModelError("StakeholderId", "الجهة المحددة غير موجودة");
                    model.AvailableItems = GetActiveRawItems();
                    return View("TransactionForm", model);
                }

                // Verify packaging style exists (if required)
                if (model.PackagingStyleId.HasValue)
                {
                    var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                        .GetOne(p => p.Id == model.PackagingStyleId.Value);

                    if (packagingStyle == null)
                    {
                        _logger.LogWarning("⚠️ Packaging style not found: {PackagingStyleId} by {User} at {Time}",
                            model.PackagingStyleId.Value, currentUser, currentTime);

                        ModelState.AddModelError("PackagingStyleId", "نمط التعبئة المحدد غير موجود");
                        model.AvailableItems = GetActiveRawItems();
                        return View("TransactionForm", model);
                    }
                }

                // Create new transaction
                var transaction = new RawTransaction
                {
                    TransactionId = GenerateTransactionId(),
                    InternalId = model.InternalId?.Trim(),
                    ExternalId = model.ExternalId?.Trim(),
                    RawItemId = model.RawItemId.Value,
                    InboundMeter = model.IsInbound ? model.Quantity : 0,
                    InboundKg = model.IsInbound ? model.Weight : 0,
                    Outbound = model.IsInbound ? 0 : (model.Quantity + model.Weight),
                    Count = model.Count,
                    StakeholderId = model.StakeholderId.Value,
                    // Update the property to be nullable
                    PackagingStyleId = (int)model.PackagingStyleId, // Direct assignment since it's an int
                    Date = model.Date,
                  
                    Comment = model.Comment?.Trim()
                };

                _logger.LogInformation("📝 Created transaction object: ID={TransactionId}, InboundMeter={InboundMeter}, InboundKg={InboundKg}, Outbound={Outbound}, Count={Count} by {User} at {Time}",
                    transaction.TransactionId, transaction.InboundMeter, transaction.InboundKg, transaction.Outbound, transaction.Count, currentUser, currentTime);

                // ✅ CORRECT: Update balances for the NEW transaction
                transaction.UpdateBalances(currentBalance.QuantityBalance, currentBalance.CountBalance);

                _logger.LogInformation("📈 New transaction balance calculated: Quantity={Quantity:N3}, Count={Count} by {User} at {Time}",
                    transaction.QuantityBalance, transaction.CountBalance, currentUser, currentTime);

                // Add to database
                _unitOfWork.Repository<RawTransaction>().Add(transaction);
                _unitOfWork.Complete();

                _logger.LogInformation("✅ Raw transaction {TransactionId} saved successfully - {Type}, Final Balance: {FinalQuantity:N3}/{FinalCount} by {User} at {Time}",
                    transaction.TransactionId,
                    model.IsInbound ? "وارد" : "صادر",
                    transaction.QuantityBalance,
                    transaction.CountBalance,
                    currentUser,
                    currentTime);

                TempData["Success"] = $"تم تسجيل {(model.IsInbound ? "وارد" : "صادر")} الخام بنجاح - رقم الإذن: {transaction.TransactionId}";
                return RedirectToAction("Details", "RawTransactions", new { id = transaction.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error saving raw transaction by {User} at {Time}. Model: {@Model}",
                    "Ammar-Yasser8", "2025-09-06 17:35:01", new
                    {
                        model.IsInbound,
                        model.RawItemId,
                        model.Quantity,
                        model.Weight,
                        model.Count,
                        model.StakeholderId,
                        model.PackagingStyleId,
                        model.Date
                    });

                ModelState.AddModelError("", "حدث خطأ أثناء حفظ المعاملة. يرجى المحاولة مرة أخرى.");
                model.AvailableItems = GetActiveRawItems();
                return View("TransactionForm", model);
            }
        }

        // GET: RawTransactions/Overview
        public IActionResult Overview(int? rawItemId, int? stakeholderId, DateTime? fromDate, DateTime? toDate,
                                     string transactionType = "All", string searchQuery = "", int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new RawTransactionOverviewViewModel
                {
                    SelectedRawItemId = rawItemId,
                    SelectedStakeholderId = stakeholderId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TransactionType = transactionType,
                    SearchQuery = searchQuery,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Get filter options
                viewModel.RawItems = GetRawItemsForFilter();
                viewModel.Stakeholders = GetStakeholdersForFilter();

                // Build query
                var query = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(includeEntities: "RawItem,RawItem.Warp,RawItem.Weft,Stakeholder,PackagingStyle")
                    .AsQueryable();

                // Apply filters
                if (rawItemId.HasValue)
                {
                    query = query.Where(t => t.RawItemId == rawItemId.Value);
                }

                if (stakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == stakeholderId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.Date >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var endDate = toDate.Value.Date.AddDays(1);
                    query = query.Where(t => t.Date < endDate);
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(t =>
                        t.TransactionId.Contains(searchQuery) ||
                        t.InternalId.Contains(searchQuery) ||
                        t.ExternalId.Contains(searchQuery) ||
                        t.RawItem.Item.Contains(searchQuery) ||
                        t.Stakeholder.Name.Contains(searchQuery) ||
                        t.Comment.Contains(searchQuery));
                }

                if (transactionType != "All")
                {
                    if (transactionType == "Inbound")
                    {
                        query = query.Where(t => t.InboundMeter > 0 || t.InboundKg > 0);
                    }
                    else if (transactionType == "Outbound")
                    {
                        query = query.Where(t => t.Outbound > 0);
                    }
                }

                // Calculate summary statistics
                var allTransactions = query.ToList();
                CalculateBalancesForTransactions(allTransactions);

                viewModel.TotalInboundMeters = allTransactions.Where(t => t.InboundMeter > 0).Sum(t => t.InboundMeter);
                viewModel.TotalInboundKg = allTransactions.Where(t => t.InboundKg > 0).Sum(t => t.InboundKg);
                viewModel.TotalOutbound = allTransactions.Where(t => t.Outbound > 0).Sum(t => t.Outbound);
                viewModel.TotalCount = allTransactions.Where(t => t.IsInbound).Sum(t => t.Count);
                viewModel.TotalTransactions = allTransactions.Count;

                // Current balances (last transaction balances)
                var lastTransaction = allTransactions.OrderByDescending(t => t.Date).FirstOrDefault();
                viewModel.CurrentBalance = lastTransaction?.QuantityBalance ?? 0;
                viewModel.CurrentCountBalance = lastTransaction?.CountBalance ?? 0;

                // Get paginated results
                var totalCount = allTransactions.Count;
                var transactions = allTransactions
                    .OrderByDescending(t => t.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new RawTransactionSummaryDto
                    {
                        Id = t.Id,
                        TransactionId = t.TransactionId,
                        InternalId = t.InternalId,
                        ExternalId = t.ExternalId,
                        Date = t.Date,
                        RawItemName = t.RawItem?.Item ?? "غير محدد",
                        WarpName = t.RawItem?.Warp?.Item ?? "غير محدد",
                        WeftName = t.RawItem?.Weft?.Item ?? "غير محدد",
                        StakeholderName = t.Stakeholder?.Name ?? "غير محدد",
                        PackagingStyleName = t.PackagingStyle?.StyleName ?? "غير محدد",
                        InboundMeters = t.InboundMeter,
                        InboundKg = t.InboundKg,
                        Outbound = t.Outbound,
                        Count = t.Count,
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance,
                        Comment = t.Comment
                    })
                    .ToList();

                viewModel.Transactions = transactions;
                viewModel.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                _logger.LogInformation("Raw transactions overview loaded by {User} at {Time} - Found {Count} transactions",
                    "Ammar-Yasser8", "2025-09-02 12:37:41", totalCount);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading raw transactions overview by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 12:37:41");
                TempData["Error"] = "حدث خطأ أثناء تحميل عرض المعاملات";
                return RedirectToAction("Index", "RawItems");
            }
        }

        // GET: RawTransactions/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var transaction = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(t => t.Id == id, includeEntities: "RawItem,RawItem.Warp,RawItem.Weft,Stakeholder,PackagingStyle")
                    .FirstOrDefault();

                if (transaction == null)
                {
                    TempData["Error"] = "المعاملة غير موجودة";
                    return RedirectToAction("Overview");
                }

                // Calculate current balance for this transaction
                var previousTransactions = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(t => t.RawItemId == transaction.RawItemId && t.Date <= transaction.Date)
                    .OrderBy(t => t.Date)
                    .ToList();

                CalculateBalancesForTransactions(previousTransactions);

                // Get related transactions for the same raw item
                var relatedTransactions = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(t => t.RawItemId == transaction.RawItemId && t.Id != transaction.Id,
                           includeEntities: "Stakeholder,PackagingStyle")
                    .OrderByDescending(t => t.Date)
                    .Take(10)
                    .ToList();

                CalculateBalancesForTransactions(relatedTransactions);

                var viewModel = new RawTransactionDetailsViewModel
                {
                    Transaction = new RawTransactionSummaryDto
                    {
                        Id = transaction.Id,
                        TransactionId = transaction.TransactionId,
                        InternalId = transaction.InternalId,
                        ExternalId = transaction.ExternalId,
                        Date = transaction.Date,
                        RawItemName = transaction.RawItem?.Item ?? "غير محدد",
                        WarpName = transaction.RawItem?.Warp?.Item ?? "غير محدد",
                        WeftName = transaction.RawItem?.Weft?.Item ?? "غير محدد",
                        StakeholderName = transaction.Stakeholder?.Name ?? "غير محدد",
                        PackagingStyleName = transaction.PackagingStyle?.StyleName ?? "غير محدد",
                        InboundMeters = transaction.InboundMeter,
                        InboundKg = transaction.InboundKg,
                        Outbound = transaction.Outbound,
                        Count = transaction.Count,
                        QuantityBalance = transaction.QuantityBalance,
                        CountBalance = transaction.CountBalance,
                        Comment = transaction.Comment
                    },
                    RawItemDetails = $"{transaction.RawItem?.Item} - كود: {transaction.RawItem?.Id}",
                    WarpDetails = $"{transaction.RawItem?.Warp?.Item} ({transaction.RawItem?.Warp?.Item})",
                    WeftDetails = $"{transaction.RawItem?.Weft?.Item} ({transaction.RawItem?.Weft?.Item})",
                    RelatedTransactions = relatedTransactions.Select(t => new RawTransactionSummaryDto
                    {
                        Id = t.Id,
                        TransactionId = t.TransactionId,
                        Date = t.Date,
                        StakeholderName = t.Stakeholder?.Name ?? "غير محدد",
                        PackagingStyleName = t.PackagingStyle?.StyleName ?? "غير محدد",
                        InboundMeters = t.InboundMeter,
                        InboundKg = t.InboundKg,
                        Outbound = t.Outbound,
                        Count = t.Count,
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance
                    }).ToList(),
                    CurrentItemBalance = transaction.QuantityBalance,
                    CurrentItemCountBalance = transaction.CountBalance
                };

                _logger.LogInformation("Raw transaction details {TransactionId} viewed by {User} at {Time}",
                    transaction.TransactionId, "Ammar-Yasser8", "2025-09-02 12:37:41");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading raw transaction details {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 12:37:41");
                TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل المعاملة";
                return RedirectToAction("Overview");
            }
        }
        // POST: RawTransactions/ResetBalance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetBalance(int transactionId, string reason = "")
        {
            try
            {
                var currentTime = "2025-09-04 20:37:37";
                var currentUser = "Ammar-Yasser8";

                // Get the transaction first to find the raw item
                var sourceTransaction = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(t => t.Id == transactionId, includeEntities: "RawItem,RawItem.Warp,RawItem.Weft")
                    .FirstOrDefault();

                if (sourceTransaction == null)
                {
                    TempData["Error"] = "المعاملة غير موجودة";
                    return RedirectToAction("Overview");
                }

                var rawItemId = sourceTransaction.RawItemId;

                _logger.LogInformation("🔄 Reset balance requested for transaction {TransactionId}, raw item {RawItemId} by {User} at {Time}",
                    transactionId, rawItemId, currentUser, currentTime);

                // Calculate current balance for the raw item
                var currentBalance = CalculateCurrentBalance(rawItemId);

                _logger.LogInformation("📊 Current balance calculated: Quantity={Quantity}, Count={Count} for raw item {RawItemId}",
                    currentBalance.QuantityBalance, currentBalance.CountBalance, rawItemId);

                if (currentBalance.QuantityBalance == 0 && currentBalance.CountBalance == 0)
                {
                    TempData["Warning"] = "الرصيد الحالي صفر بالفعل - لا حاجة لإعادة التعيين";
                    return RedirectToAction("Details", new { id = transactionId });
                }

                // Get or create default stakeholder for adjustments
                var adjustmentStakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(s => s.Name.Contains("تسوية") || s.Name.Contains("إدارة") || s.Name.Contains("مخزن"))
                    .FirstOrDefault();

                if (adjustmentStakeholder == null)
                {
                    adjustmentStakeholder = new StakeholdersInfo
                    {
                        Name = "تسوية أرصدة المخزن",
                        Status = true,
                    };
                    _unitOfWork.Repository<StakeholdersInfo>().Add(adjustmentStakeholder);
                    _unitOfWork.Complete();

                    _logger.LogInformation("✅ Created adjustment stakeholder with ID {StakeholderId} by {User} at {Time}",
                        adjustmentStakeholder.Id, currentUser, currentTime);
                }

                // Get or create default packaging style for adjustments
                var adjustmentPackaging = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll(p => p.StyleName.Contains("تسوية") || p.StyleName.Contains("متنوع") || p.StyleName.Contains("عام"))
                    .FirstOrDefault();

                if (adjustmentPackaging == null)
                {
                    adjustmentPackaging = new PackagingStyles
                    {
                        StyleName = "تسوية المخزن",
                    };
                    _unitOfWork.Repository<PackagingStyles>().Add(adjustmentPackaging);
                    _unitOfWork.Complete();

                    _logger.LogInformation("✅ Created adjustment packaging style with ID {PackagingId} by {User} at {Time}",
                        adjustmentPackaging.Id, currentUser, currentTime);
                }

                // Create adjustment transaction to reset balance to zero
                var adjustmentTransaction = new RawTransaction
                {
                    TransactionId = GenerateAdjustmentTransactionId(),
                    InternalId = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                    ExternalId = null,
                    RawItemId = rawItemId,
                    InboundMeter = 0,
                    InboundKg = 0,
                    Outbound = Math.Max(0, currentBalance.QuantityBalance), // Outbound the current positive balance
                    Count = Math.Max(0, currentBalance.CountBalance), // Outbound the current positive count
                    StakeholderId = adjustmentStakeholder.Id,
                    PackagingStyleId = adjustmentPackaging.Id,
                    Date = DateTime.Now,
                    Comment = $"تسوية رصيد - إعادة تعيين الرصيد إلى صفر. الصنف: {sourceTransaction.RawItem?.Item}. السبب: {(string.IsNullOrEmpty(reason) ? "تسوية إدارية" : reason)}. الرصيد السابق: {currentBalance.QuantityBalance:N3} كمية، {currentBalance.CountBalance} عدد. المعاملة المرجعية: {sourceTransaction.TransactionId}. تم بواسطة: {currentUser} في {currentTime}"
                };

                // Update balances (should result in zero)
                adjustmentTransaction.UpdateBalances(currentBalance.QuantityBalance, currentBalance.CountBalance);

                _logger.LogInformation("📋 Adjustment transaction created: ID={TransactionId}, Outbound={Outbound}, Count={Count}, FinalBalance={FinalQuantity}/{FinalCount}",
                    adjustmentTransaction.TransactionId, adjustmentTransaction.Outbound, adjustmentTransaction.Count,
                    adjustmentTransaction.QuantityBalance, adjustmentTransaction.CountBalance);

                // Add to database
                _unitOfWork.Repository<RawTransaction>().Add(adjustmentTransaction);
                _unitOfWork.Complete();

                _logger.LogInformation("✅ Balance reset completed for raw item {RawItemId} - Transaction {TransactionId} created by {User} at {Time}. Previous balance: {PreviousQuantity}/{PreviousCount}",
                    rawItemId, adjustmentTransaction.TransactionId, currentUser, currentTime,
                    currentBalance.QuantityBalance, currentBalance.CountBalance);

                TempData["Success"] = $"تم إعادة تعيين الرصيد بنجاح - رقم معاملة التسوية: {adjustmentTransaction.TransactionId}";
                return RedirectToAction("Details", new { id = adjustmentTransaction.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error resetting balance for transaction {TransactionId} by {User} at {Time}",
                    transactionId, "Ammar-Yasser8", "2025-09-04 20:37:37");
                TempData["Error"] = "حدث خطأ أثناء إعادة تعيين الرصيد";
                return RedirectToAction("Details", new { id = transactionId });
            }
        }
        #region Helper Methods and AJAX Endpoints

        // Get list of active raw items for dropdown
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
                    "Ammar-Yasser8", "2025-09-01 22:38:16");
                return new List<SelectListItem>();
            }
        }

        // Get Stokholder List By from name 
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

        // Get Balance Info for a Raw Item
        [HttpGet]
        public JsonResult GetRawItemBalance(int rawItemId)
        {
            try
            {
                var balance = CalculateCurrentBalance(rawItemId);
                var rawItem = _unitOfWork.Repository<RawItem>()
                    .GetAll(includeEntities: "Warp,Weft")
                    .FirstOrDefault(r => r.Id == rawItemId);

                if (rawItem == null)
                {
                    return Json(new { success = false, error = "الصنف غير موجود" });
                }

                return Json(new
                {
                    success = true,
                    quantityBalance = balance.QuantityBalance,
                    countBalance = balance.CountBalance,
                    rawItemName = rawItem.Item,
                    warpName = rawItem.Warp.Item,
                    weftName = rawItem.Weft.Item
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting raw item balance for {RawItemId} by {User} at {Time}",
                    rawItemId, "Ammar-Yasser8", "2025-09-01 22:38:16");

                return Json(new { success = false, error = "حدث خطأ أثناء تحميل الأرصدة" });
            }
        }
        private (double QuantityBalance, int CountBalance) CalculateCurrentBalance(int rawItemId)
        {
            try
            {
                var transactions = _unitOfWork.Repository<RawTransaction>()
                    .GetAll(t => t.RawItemId == rawItemId)
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id) // Add secondary sort for consistency
                    .ToList();

                double quantityBalance = 0;
                int countBalance = 0;

                _logger.LogDebug("📊 Calculating balance for {TransactionCount} transactions of raw item {RawItemId} by {User} at {Time}",
                    transactions.Count, rawItemId, "Ammar-Yasser8", "2025-09-06 17:35:01");

                // ✅ CALCULATE balance WITHOUT modifying existing transactions
                foreach (var transaction in transactions)
                {
                    if (transaction.InboundMeter > 0 || transaction.InboundKg > 0)
                    {
                        // Inbound: Add to balance
                        quantityBalance += transaction.InboundMeter + transaction.InboundKg;
                        countBalance += transaction.Count;

                        _logger.LogTrace("➕ Inbound: +{Meters}m +{Kg}kg +{Count}count = Balance: {QBalance:N3}/{CBalance}",
                            transaction.InboundMeter, transaction.InboundKg, transaction.Count, quantityBalance, countBalance);
                    }
                    else if (transaction.Outbound > 0)
                    {
                        // Outbound: Subtract from balance
                        quantityBalance -= transaction.Outbound;
                        countBalance -= transaction.Count;

                        _logger.LogTrace("➖ Outbound: -{Outbound:N3} -{Count}count = Balance: {QBalance:N3}/{CBalance}",
                            transaction.Outbound, transaction.Count, quantityBalance, countBalance);
                    }

                    // ❗ CRITICAL: DON'T call transaction.UpdateBalances() here!
                    // That was corrupting your data!
                }

                // Ensure no negative balances
                quantityBalance = Math.Max(0, quantityBalance);
                countBalance = Math.Max(0, countBalance);

                _logger.LogInformation("💰 Final calculated balance for raw item {RawItemId}: Quantity={Quantity:N3}, Count={Count} by {User} at {Time}",
                    rawItemId, quantityBalance, countBalance, "Ammar-Yasser8", "2025-09-06 17:35:01");

                return (quantityBalance, countBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calculating balance for raw item {RawItemId} by {User} at {Time}",
                    rawItemId, "Ammar-Yasser8", "2025-09-06 17:35:01");
                return (0, 0);
            }
        }
        // Get Packaging Styles List By Form Name
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

        // Generate a new transaction Id 
        private string GenerateTransactionId()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "WAT"; // Yarn Transaction

                var lastTransaction = _unitOfWork.Repository<RawTransaction>()
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
                    "Ammar-Yasser8", "2025-09-02 12:37:41");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetStakeholdersForFilter()
        {
            try
            {
                return _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(s => s.Status)
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    })
                    .OrderBy(s => s.Text)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stakeholders for filter by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 12:37:41");
                return new List<SelectListItem>();
            }
        }

        /// <summary>
        /// Calculate balances for multiple transactions (for batch operations)
        /// Fixed at 2025-09-06 17:32:56 by Ammar-Yasser8
        /// </summary>
        /// <summary>
        /// Calculate balances for multiple transactions (use for data repair/migration)
        /// Updated at 2025-09-06 17:35:01 by Ammar-Yasser8
        /// </summary>
        private void CalculateBalancesForTransactions(List<RawTransaction> transactions)
        {
            try
            {
                if (!transactions.Any())
                {
                    _logger.LogDebug("📊 No transactions to process by {User} at {Time}",
                        "Ammar-Yasser8", "2025-09-06 17:35:01");
                    return;
                }

                // Group by RawItemId to calculate balances separately for each item
                var itemGroups = transactions.GroupBy(t => t.RawItemId);

                foreach (var group in itemGroups)
                {
                    var itemTransactions = group.OrderBy(t => t.Date).ThenBy(t => t.Id).ToList();
                    double quantityBalance = 0;
                    int countBalance = 0;

                    _logger.LogInformation("🔄 Processing {TransactionCount} transactions for raw item {RawItemId} by {User} at {Time}",
                        itemTransactions.Count, group.Key, "Ammar-Yasser8", "2025-09-06 17:35:01");

                    foreach (var transaction in itemTransactions)
                    {
                        // Update balance for this specific transaction
                        transaction.UpdateBalances(quantityBalance, countBalance);

                        // Use the updated balance for the next transaction
                        quantityBalance = transaction.QuantityBalance;
                        countBalance = transaction.CountBalance;

                        _logger.LogTrace("📈 Transaction {TransactionId}: Balance updated to {Quantity:N3}/{Count}",
                            transaction.TransactionId, quantityBalance, countBalance);
                    }

                    _logger.LogInformation("✅ Completed balance calculation for raw item {RawItemId}: Final={Quantity:N3}/{Count} by {User} at {Time}",
                        group.Key, quantityBalance, countBalance, "Ammar-Yasser8", "2025-09-06 17:35:01");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calculating balances for transactions by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-06 17:35:01");
                throw;
            }
        }
        // Helper method to generate adjustment transaction ID
        private string GenerateAdjustmentTransactionId()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "RAW-ADJ"; // Raw Adjustment Transaction

                var lastTransaction = _unitOfWork.Repository<RawTransaction>()
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

                _logger.LogInformation("Generated adjustment transaction ID: {TransactionId} by {User} at {Time}",
                    transactionId, "Ammar-Yasser8", "2025-09-02 13:07:13");

                return transactionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating adjustment transaction ID by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 13:07:13");
                return $"RAW-ADJ-{DateTime.Now:yyyyMMdd}-0001";
            }
        }

        #endregion

    }
}
