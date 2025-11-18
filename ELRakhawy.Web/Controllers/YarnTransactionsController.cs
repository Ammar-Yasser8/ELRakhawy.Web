using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
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
                ModelState.Remove("Date");

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
                    StakeholderTypeId = model.StakeholderTypeId,
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
                var newViewModel = new YarnTransactionViewModel
                {
                    IsInbound = model.IsInbound,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveYarnItems()
                };
                return View("TransactionForm", newViewModel);
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

        // GET: YarnTransactions/Edit/{id}
        // GET: YarnTransactions/Edit/{id}
        public IActionResult Edit(int id)
        {
            try
            {
                var transaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetOne(t => t.Id == id,
                        includeEntities: "YarnItem,Stakeholder,StakeholderType,PackagingStyle");

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for edit by {User} at {Time}",
                        id, "Ammar-Yasser8", "2025-11-17 17:12:13");
                    TempData["Error"] = "لم يتم العثور على المعاملة المطلوبة";
                    return RedirectToAction("Index", "YarnItems");
                }

                // Business rule validation
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == transaction.YarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                if (latestTransaction?.Id != transaction.Id)
                {
                    _logger.LogWarning("Attempt to edit non-latest transaction {TransactionId} by {User} at {Time}",
                        transaction.TransactionId, "Ammar-Yasser8", "2025-11-17 17:12:13");
                    TempData["Error"] = "يمكن تعديل آخر معاملة فقط لكل صنف غزل";
                    return RedirectToAction("Index", "YarnItems");
                }

                var viewModel = new YarnTransactionViewModel
                {
                    Id = transaction.Id,
                    TransactionId = transaction.TransactionId,
                    InternalId = transaction.InternalId,
                    ExternalId = transaction.ExternalId,
                    YarnItemId = transaction.YarnItemId,
                    YarnItemName = transaction.YarnItem?.Item ?? "",
                    Quantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound,
                    IsInbound = transaction.Inbound > 0,
                    Count = transaction.Count,
                    StakeholderTypeId = transaction.StakeholderTypeId ?? 0,
                    StakeholderTypeName = transaction.StakeholderType?.Type ?? "",
                    StakeholderId = transaction.StakeholderId,
                    StakeholderName = transaction.Stakeholder?.Name ?? "",
                    PackagingStyleId = transaction.PackagingStyleId,
                    PackagingStyleName = transaction.PackagingStyle?.StyleName ?? "",
                    Date = transaction.Date,
                    Comment = transaction.Comment,

                    // ✅ POPULATE ALL DROPDOWN DATA for edit mode
                    AvailableItems = GetActiveYarnItems(),
                    PackagingStyles = GetPackagingStyleSelectList(),
                    StakeholderTypes = GetStakeholderTypeSelectList(),
                    // ✅ Load stakeholders for the current stakeholder type
                    Stakeholders = transaction.StakeholderTypeId.HasValue
                        ? GetStakeholdersByTypeSelectList(transaction.StakeholderTypeId.Value)
                        : new List<SelectListItem>()
                };

                // Set original values for balance calculation
                viewModel.SetOriginalValues(transaction);

                _logger.LogInformation("Edit form loaded for transaction {TransactionId} with all dropdown data populated by {User} at {Time}",
                    transaction.TransactionId, "Ammar-Yasser8", "2025-11-17 17:12:13");

                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for transaction {TransactionId} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-11-17 17:12:13");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج التعديل";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // POST: YarnTransactions/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(YarnTransactionViewModel model)
        {
            try
            {
                // Remove server-side populated fields from validation
                ModelState.Remove("YarnItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("StakeholderTypeName");
                ModelState.Remove("PackagingStyleName");
                ModelState.Remove("TransactionId");
                ModelState.Remove("OriginYarnName");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    model.PackagingStyles = GetPackagingStyleSelectList();
                    return View("TransactionForm", model);
                }

                var existingTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetOne(t => t.Id == model.Id); 
                if (existingTransaction == null)
                {
                    TempData["Error"] = "لم يتم العثور على المعاملة المطلوبة";
                    return RedirectToAction("Index", "YarnItems");
                }

                // Verify this is still the latest transaction (prevent race conditions)
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == existingTransaction.YarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                if (latestTransaction?.Id != existingTransaction.Id)
                {
                    TempData["Error"] = "لا يمكن تعديل هذه المعاملة لأنها لم تعد آخر معاملة";
                    return RedirectToAction("Index", "YarnItems");
                }

                // ✅ STEP 1: Calculate the balance before the original transaction
                var previousQuantityBalance = existingTransaction.QuantityBalance - (existingTransaction.Inbound - existingTransaction.Outbound);
                var previousCountBalance = existingTransaction.CountBalance -
                    ((existingTransaction.Inbound > 0 ? existingTransaction.Count : 0) -
                     (existingTransaction.Outbound > 0 ? existingTransaction.Count : 0));

                // ✅ STEP 2: Calculate new transaction values
                var newInbound = model.IsInbound ? model.Quantity : 0;
                var newOutbound = model.IsInbound ? 0 : model.Quantity;
                var newCountInbound = newInbound > 0 ? model.Count : 0;
                var newCountOutbound = newOutbound > 0 ? model.Count : 0;

                // ✅ STEP 3: Calculate new balances
                var newQuantityBalance = previousQuantityBalance + (newInbound - newOutbound);
                var newCountBalance = previousCountBalance + newCountInbound - newCountOutbound;

                // ✅ STEP 4: Validate new balances (prevent negative balances for outbound)
                if (!model.IsInbound && newQuantityBalance < 0)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({previousQuantityBalance + newInbound:N3})");
                    model.AvailableItems = GetActiveYarnItems();
                    model.PackagingStyles = GetPackagingStyleSelectList();
                    return View("TransactionForm", model);
                }

                if (!model.IsInbound && newCountBalance < 0)
                {
                    ModelState.AddModelError("Count",
                        $"العدد المطلوب ({model.Count}) أكبر من الرصيد المتاح ({previousCountBalance + newCountInbound})");
                    model.AvailableItems = GetActiveYarnItems();
                    model.PackagingStyles = GetPackagingStyleSelectList();
                    return View("TransactionForm", model);
                }

                // ✅ STEP 5: Update the transaction
                existingTransaction.InternalId = model.InternalId?.Trim();
                existingTransaction.ExternalId = model.ExternalId?.Trim();
                existingTransaction.YarnItemId = model.YarnItemId;
                existingTransaction.Inbound = newInbound;
                existingTransaction.Outbound = newOutbound;
                existingTransaction.Count = model.Count;
                existingTransaction.StakeholderId = model.StakeholderId;
                existingTransaction.StakeholderTypeId = model.StakeholderTypeId;
                existingTransaction.PackagingStyleId = model.PackagingStyleId;
                existingTransaction.Date = model.Date;
                existingTransaction.Comment = model.Comment?.Trim();
                existingTransaction.QuantityBalance = newQuantityBalance;
                existingTransaction.CountBalance = newCountBalance;

                // ✅ Enhanced logging for edit operation
                _logger.LogInformation("Transaction {TransactionId} edited - " +
                    "Original: Inbound={OrigInbound}, Outbound={OrigOutbound}, Count={OrigCount}, QtyBal={OrigQtyBal}, CountBal={OrigCountBal} | " +
                    "Updated: Inbound={NewInbound}, Outbound={NewOutbound}, Count={NewCount}, QtyBal={NewQtyBal}, CountBal={NewCountBal} | " +
                    "Previous Balance: Qty={PrevQty}, Count={PrevCount} by {User} at {Time}",
                    existingTransaction.TransactionId,
                    model.OriginalIsInbound ? model.OriginalQuantity : 0,
                    model.OriginalIsInbound ? 0 : model.OriginalQuantity,
                    model.OriginalCount,
                    model.OriginalQuantityBalance,
                    model.OriginalCountBalance,
                    newInbound, newOutbound, model.Count, newQuantityBalance, newCountBalance,
                    previousQuantityBalance, previousCountBalance,
                    "Ammar-Yasser8", "2025-11-17 16:53:36");

                _unitOfWork.Repository<YarnTransaction>().Update(existingTransaction);
                _unitOfWork.Complete();

                _logger.LogInformation("Transaction {TransactionId} updated successfully by {User} at {Time}",
                    existingTransaction.TransactionId, "Ammar-Yasser8", "2025-11-17 16:53:36");

                TempData["Success"] = $"تم تعديل معاملة {(model.IsInbound ? "وارد" : "صادر")} الغزل بنجاح - رقم الإذن: {existingTransaction.TransactionId}";

                return RedirectToAction("Index", "YarnItems");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating yarn transaction {TransactionId} by {User} at {Time}",
                    model.TransactionId, "Ammar-Yasser8", "2025-11-17 16:53:36");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث المعاملة");
                model.AvailableItems = GetActiveYarnItems();
                model.PackagingStyles = GetPackagingStyleSelectList();
                return View("TransactionForm", model);
            }
        }
        // Get: YarnTransactions/Edit/5
        // Add these methods to your YarnTransactionsController

        private List<SelectListItem> GetPackagingStyleSelectList()
        {
            try
            {
                var packagingStyles = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll()
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.StyleName
                    })
                    .ToList();

                _logger.LogInformation("Loaded {Count} packaging styles for dropdown by {User} at {Time}",
                    packagingStyles.Count, "Ammar-Yasser8", "2025-11-17 17:04:43");

                return packagingStyles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading packaging styles for dropdown by {User} at {Time}",
                    "Ammar-Yasser8", "2025-11-17 17:04:43");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetStakeholderTypeSelectList()
        {
            try
            {
                var stakeholderTypes = _unitOfWork.Repository<StakeholderType>()
                    .GetAll()
                    .Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = st.Type.ToString()
                    })
                    .ToList();

                _logger.LogInformation("Loaded {Count} stakeholder types for dropdown by {User} at {Time}",
                    stakeholderTypes.Count, "Ammar-Yasser8", "2025-11-17 17:04:43");

                return stakeholderTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stakeholder types for dropdown by {User} at {Time}",
                    "Ammar-Yasser8", "2025-11-17 17:04:43");
                return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetStakeholdersByTypeSelectList(int stakeholderTypeId)
        {
            try
            {
                if (stakeholderTypeId <= 0)
                    return new List<SelectListItem>();

                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(s => s.StakeholderInfoTypes.Any(st => st.StakeholderTypeId == stakeholderTypeId) && s.Status) // Check if any stakeholder type matches                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    })
                    .ToList();

                _logger.LogInformation("Loaded {Count} stakeholders for type {TypeId} by {User} at {Time}",
                    stakeholders.Count, stakeholderTypeId, "Ammar-Yasser8", "2025-11-17 17:04:43");

                return stakeholders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stakeholders for type {TypeId} by {User} at {Time}",
                    stakeholderTypeId, "Ammar-Yasser8", "2025-11-17 17:04:43");
                return new List<SelectListItem>();
            }
        }
        
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
                    YarnStakeholders = GetYarnStakeholders().ToList(),
                    PackagingStyles = GetPackagingStyles(), // ✅ Add packaging styles
                    Results = new List<YarnTransactionViewModel>()
                };

                _logger.LogInformation("Yarn transaction search form with Arabic dates loaded by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn transaction search form by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة البحث";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // POST: YarnTransactions/Search - Enhanced with all fields
        [HttpPost]
        public IActionResult Search(YarnTransactionSearchViewModel model)
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var currentUser = "Ammar-Yasser8";

                // ✅ Convert Arabic numbers to Latin for database search
                model = ConvertArabicNumbersToLatin(model);

                _logger.LogInformation("Enhanced yarn transaction search with Arabic/Latin conversion initiated by {User} at {Time} with criteria: {@SearchCriteria}",
                    currentUser, currentTime, new
                    {
                        model.TransactionId,
                        model.FromDate,
                        model.ToDate,
                        model.InternalId,
                        model.ExternalId,
                        model.TransactionType,
                        model.YarnItemId,
                        model.StakeholderTypeId,
                        model.StakeholderId,
                        model.PackagingStyleId,
                        model.MinQuantity,
                        model.MaxQuantity,
                        model.MinCount,
                        model.MaxCount,
                        model.CommentSearch
                    });

                // Get yarn-related stakeholder IDs first
                var yarnStakeholderIds = GetYarnStakeholderIds();

                var query = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(includeEntities: "YarnItem,StakeholderType,Stakeholder,PackagingStyle,YarnItem.OriginYarn,YarnItem.Manufacturers")
                    .Where(t => yarnStakeholderIds.Contains(t.StakeholderId))
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

                // ✅ Enhanced ID searches with both Arabic and Latin matching
                if (!string.IsNullOrEmpty(model.InternalId))
                {
                    var searchTerm = model.InternalId.Trim();
                    var arabicSearchTerm = ConvertToArabicDigits(searchTerm);
                    var latinSearchTerm = ConvertToLatinDigits(searchTerm);

                    query = query.Where(t => !string.IsNullOrEmpty(t.InternalId) &&
                        (t.InternalId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.InternalId.Contains(arabicSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.InternalId.Contains(latinSearchTerm, StringComparison.OrdinalIgnoreCase)));

                    _logger.LogDebug("Applied InternalId filter: Original={Original}, Arabic={Arabic}, Latin={Latin}",
                        searchTerm, arabicSearchTerm, latinSearchTerm);
                }

                if (!string.IsNullOrEmpty(model.ExternalId))
                {
                    var searchTerm = model.ExternalId.Trim();
                    var arabicSearchTerm = ConvertToArabicDigits(searchTerm);
                    var latinSearchTerm = ConvertToLatinDigits(searchTerm);

                    query = query.Where(t => !string.IsNullOrEmpty(t.ExternalId) &&
                        (t.ExternalId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.ExternalId.Contains(arabicSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.ExternalId.Contains(latinSearchTerm, StringComparison.OrdinalIgnoreCase)));

                    _logger.LogDebug("Applied ExternalId filter: Original={Original}, Arabic={Arabic}, Latin={Latin}",
                        searchTerm, arabicSearchTerm, latinSearchTerm);
                }

                if (model.YarnItemId.HasValue)
                {
                    query = query.Where(t => t.YarnItemId == model.YarnItemId.Value);
                }

                if (model.StakeholderTypeId.HasValue)
                {
                    query = query.Where(t => t.StakeholderTypeId == model.StakeholderTypeId.Value);
                }

                if (model.StakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == model.StakeholderId.Value);
                }

                if (model.PackagingStyleId.HasValue)
                {
                    query = query.Where(t => t.PackagingStyleId == model.PackagingStyleId.Value);
                }

                // ✅ Quantity range filters (already converted to Latin)
                if (model.MinQuantity.HasValue)
                {
                    query = query.Where(t => (t.Inbound + t.Outbound) >= model.MinQuantity.Value);
                }

                if (model.MaxQuantity.HasValue)
                {
                    query = query.Where(t => (t.Inbound + t.Outbound) <= model.MaxQuantity.Value);
                }

                // ✅ Count range filters (already converted to Latin)
                if (model.MinCount.HasValue)
                {
                    query = query.Where(t => t.Count >= model.MinCount.Value);
                }

                if (model.MaxCount.HasValue)
                {
                    query = query.Where(t => t.Count <= model.MaxCount.Value);
                }

                // ✅ Comment search with Arabic/Latin support
                if (!string.IsNullOrEmpty(model.CommentSearch))
                {
                    var commentSearchTerm = model.CommentSearch.Trim();
                    var arabicCommentTerm = ConvertToArabicDigits(commentSearchTerm);
                    var latinCommentTerm = ConvertToLatinDigits(commentSearchTerm);

                    query = query.Where(t => !string.IsNullOrEmpty(t.Comment) &&
                        (t.Comment.Contains(commentSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.Comment.Contains(arabicCommentTerm, StringComparison.OrdinalIgnoreCase) ||
                         t.Comment.Contains(latinCommentTerm, StringComparison.OrdinalIgnoreCase)));
                }

                // Transaction type filter
                if (!string.IsNullOrEmpty(model.TransactionType))
                {
                    if (model.TransactionType == "inbound")
                        query = query.Where(t => t.Inbound > 0);
                    else if (model.TransactionType == "outbound")
                        query = query.Where(t => t.Outbound > 0);
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

                // Calculate statistics
                model.Results = results;
                model.TotalTransactions = results.Count;
                model.TotalInbound = results.Where(r => r.IsInbound).Sum(r => r.Quantity);
                model.TotalOutbound = results.Where(r => !r.IsInbound).Sum(r => r.Quantity);
                model.NetBalance = model.TotalInbound - model.TotalOutbound;

                // Reload dropdown data
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
                model.YarnStakeholders = GetYarnStakeholders().ToList();
                model.PackagingStyles = GetPackagingStyles();

                ViewBag.SearchPerformed = true;

                _logger.LogInformation("Enhanced search completed by {User} at {Time} - Found {Count} results",
                    currentUser, currentTime, results.Count);

                if (results.Any())
                {
                    TempData["Success"] = $"تم العثور على {ConvertToArabicDigits(results.Count.ToString())} معاملة غزل مطابقة لمعايير البحث";
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
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                TempData["Error"] = "حدث خطأ أثناء البحث في معاملات الغزل";

                model.Results = new List<YarnTransactionViewModel>();
                model.AvailableItems = GetActiveYarnItems();
                model.StakeholderTypes = GetStakeholderTypes();
                model.YarnStakeholders = GetYarnStakeholders().ToList();
                model.PackagingStyles = GetPackagingStyles();
                ViewBag.SearchPerformed = true;

                return View(model);
            }
        }

        // ✅ Helper methods for Arabic/Latin number conversion
        private string ConvertToArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var arabicDigits = new char[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
            var result = input.ToCharArray();

            for (int i = 0; i < result.Length; i++)
            {
                if (char.IsDigit(result[i]))
                {
                    result[i] = arabicDigits[result[i] - '0'];
                }
            }

            return new string(result);
        }

        private string ConvertToLatinDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var arabicToLatin = new Dictionary<char, char>
    {
        { '٠', '0' }, { '١', '1' }, { '٢', '2' }, { '٣', '3' }, { '٤', '4' },
        { '٥', '5' }, { '٦', '6' }, { '٧', '7' }, { '٨', '8' }, { '٩', '9' }
    };

            var result = input.ToCharArray();

            for (int i = 0; i < result.Length; i++)
            {
                if (arabicToLatin.ContainsKey(result[i]))
                {
                    result[i] = arabicToLatin[result[i]];
                }
            }

            return new string(result);
        }

        private YarnTransactionSearchViewModel ConvertArabicNumbersToLatin(YarnTransactionSearchViewModel model)
        {
            // Convert string fields that might contain Arabic numbers
            if (!string.IsNullOrEmpty(model.InternalId))
                model.InternalId = ConvertToLatinDigits(model.InternalId);

            if (!string.IsNullOrEmpty(model.ExternalId))
                model.ExternalId = ConvertToLatinDigits(model.ExternalId);

            if (!string.IsNullOrEmpty(model.CommentSearch))
                model.CommentSearch = ConvertToLatinDigits(model.CommentSearch);

            // Note: Numeric fields (MinQuantity, MaxQuantity, MinCount, MaxCount) 
            // are automatically handled by model binding, but we can add extra safety

            return model;
        }
        // ✅ Helper method to get packaging styles
        private List<SelectListItem> GetPackagingStyles()
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
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");
                    return new List<SelectListItem>();
                }

                // Get packaging styles that are related to yarn forms
                var yarnPackagingStyles = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll(includeEntities: "PackagingStyleForms")
                    .Where(ps => ps.PackagingStyleForms.Any(psf => yarnFormIds.Contains(psf.FormId)))
                    .Select(ps => new SelectListItem
                    {
                        Value = ps.Id.ToString(),
                        Text = ps.StyleName
                    })
                    .OrderBy(ps => ps.Text)
                    .ToList();

                _logger.LogInformation("Loaded {Count} yarn-related packaging styles for dropdown by {User} at {Time}",
                    yarnPackagingStyles.Count, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return yarnPackagingStyles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn-related packaging styles by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new List<SelectListItem>();
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
                        Id = t.Id,
                        TransactionId = t.TransactionId ?? "غير محدد",
                        InternalId = t.InternalId,
                        ExternalId = t.ExternalId,
                        Quantity = t.Inbound > 0 ? t.Inbound : (t.Outbound > 0 ? t.Outbound : 0),
                        IsInbound = t.Inbound > 0,
                        Count = t.Count,
                        StakeholderType = t.StakeholderType?.Type ?? "غير محدد", // Make sure this is included
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
                        includeEntities: "YarnItem,YarnItem.OriginYarn,YarnItem.Manufacturers,Stakeholder,StakeholderType,PackagingStyle");

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found by {User} at {Time}",
                        id, "Ammar-Yasser8", "2025-11-17 19:13:14");
                    return NotFound(new { message = "المعاملة غير موجودة" });
                }

                var result = new
                {
                    id = transaction.Id,
                    transactionId = transaction.TransactionId ?? "غير محدد",
                    internalId = transaction.InternalId ?? "غير محدد",
                    externalId = transaction.ExternalId ?? "غير محدد",

                    // Yarn item details
                    yarnItemId = transaction.YarnItemId,
                    yarnItemName = transaction.YarnItem?.Item ?? "غير محدد",
                    originYarnName = transaction.YarnItem?.OriginYarn?.Item ?? "غير محدد",
                    manufacturerNames = transaction.YarnItem?.Manufacturers != null && transaction.YarnItem.Manufacturers.Any()
                        ? string.Join("، ", transaction.YarnItem.Manufacturers.Select(m => m.Name))
                        : "غير محدد",

                    // Transaction amounts
                    quantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound,
                    inbound = transaction.Inbound,
                    outbound = transaction.Outbound,
                    count = transaction.Count,
                    isInbound = transaction.Inbound > 0,

                    // Stakeholder information
                    stakeholderTypeId = transaction.StakeholderTypeId,
                    stakeholderTypeName = transaction.StakeholderType?.Type ?? "غير محدد",
                    stakeholderId = transaction.StakeholderId,
                    stakeholderName = transaction.Stakeholder?.Name ?? "غير محدد",

                    // ✅ Packaging information - Fixed property names
                    packagingStyleId = transaction.PackagingStyleId,
                    packagingStyleName = transaction.PackagingStyle?.StyleName ?? "غير محدد",
                    packagingType = transaction.PackagingStyle?.StyleName ?? "غير محدد", // Alternative property name

                    // Balance information
                    quantityBalance = transaction.QuantityBalance,
                    countBalance = transaction.CountBalance,

                    // Date and metadata
                    date = transaction.Date,
                    dateFormatted = transaction.Date.ToString("yyyy-MM-dd"),
                    comment = transaction.Comment ?? "لا يوجد بيان",

                    // Audit information
                    createdAt = "2025-11-17 19:13:14",
                    createdBy = "Ammar-Yasser8",

                    // ✅ Additional computed properties for JavaScript
                    displayDate = transaction.Date.ToString("dd/MM/yyyy"),
                    displayTime = transaction.Date.ToString("HH:mm"),
                    transactionTypeText = transaction.Inbound > 0 ? "وارد" : "صادر",

                    // Success flag
                    success = true
                };

                _logger.LogInformation("Transaction details retrieved successfully for ID {TransactionId} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-11-17 19:13:14");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn transaction details for ID {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-11-17 19:13:14");
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحميل تفاصيل المعاملة",
                    error = ex.Message
                });
            }
        }

        // API: GET - Get stakeholders by type (inherited from Raw pattern)
        [HttpGet]
        public JsonResult GetStakeholdersByType(int typeId)
        {
            try
            {
                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes.StakeholderType")
                    .Where(s => s.Status) // Only Active
                    .Where(s => s.StakeholderInfoTypes.Any(t => t.StakeholderTypeId == typeId))
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        typeName = s.StakeholderInfoTypes
                                    .Where(t => t.StakeholderTypeId == typeId)
                                    .Select(t => t.StakeholderType.Type)
                                    .FirstOrDefault()
                    })
                    .OrderBy(s => s.name)
                    .ToList();

                if (!stakeholders.Any())
                {
                    return Json(new { error = "No stakeholders found for selected type", data = new List<object>() });
                }

                return Json(new { success = true, data = stakeholders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stakeholders for type {typeId}");
                return Json(new { error = "An error occurred while fetching stakeholders", data = new List<object>() });
            }
        }

        // Helper method to get stakeholder types
        private List<SelectListItem> GetStakeholderTypes()
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
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");
                    return new List<SelectListItem>();
                }

                // Get stakeholder types that are related to yarn forms
                var yarnStakeholderTypes = _unitOfWork.Repository<StakeholderType>()
                    .GetAll(includeEntities: "StakeholderTypeForms")
                    .Where(st => st.StakeholderTypeForms.Any(stf => yarnFormIds.Contains(stf.FormId)))
                    .Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = st.Type
                    })
                    .OrderBy(s => s.Text)
                    .ToList();

                _logger.LogInformation("Loaded {Count} yarn-related stakeholder types for dropdown by {User} at {Time}",
                    yarnStakeholderTypes.Count, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return yarnStakeholderTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn-related stakeholder types by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return new List<SelectListItem>();
            }
        }
        // API: GET - Get stakeholder type for selected stakeholder
        [HttpGet]
        public JsonResult GetStakeholderType(int stakeholderId)
        {
            try
            {
                _logger.LogInformation(
                    "Getting stakeholder type for stakeholder {StakeholderId} at {Time} by {User}",
                    stakeholderId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                if (stakeholderId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = "معرف الجهة غير صحيح",
                        data = new { stakeholderTypeId = 0, stakeholderTypeName = "" }
                    });
                }

                // Get stakeholder with their types
                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == stakeholderId, includeEntities: "StakeholderInfoTypes.StakeholderType");

                if (stakeholder == null)
                {
                    _logger.LogWarning(
                        "Stakeholder not found for ID {StakeholderId} at {Time} by {User}",
                        stakeholderId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                    return Json(new
                    {
                        success = false,
                        error = "الجهة غير موجودة",
                        data = new { stakeholderTypeId = 0, stakeholderTypeName = "" }
                    });
                }

                // Get the first stakeholder type (assuming one primary type per stakeholder)
                var stakeholderType = stakeholder.StakeholderInfoTypes?.FirstOrDefault()?.StakeholderType;

                if (stakeholderType == null)
                {
                    _logger.LogWarning(
                        "No stakeholder type found for stakeholder {StakeholderId} at {Time} by {User}",
                        stakeholderId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                    return Json(new
                    {
                        success = false,
                        error = "لم يتم العثور على نوع الجهة",
                        data = new { stakeholderTypeId = 0, stakeholderTypeName = "" }
                    });
                }

                _logger.LogInformation(
                    "Found stakeholder type {TypeId}: {TypeName} for stakeholder {StakeholderId} at {Time} by {User}",
                    stakeholderType.Id, stakeholderType.Type, stakeholderId,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        stakeholderTypeId = stakeholderType.Id,
                        stakeholderTypeName = stakeholderType.Type,
                        stakeholderName = stakeholder.Name
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stakeholder type for stakeholder {StakeholderId} at {Time} by {User}",
                    stakeholderId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل نوع الجهة",
                    data = new { stakeholderTypeId = 0, stakeholderTypeName = "" }
                });
            }
        }

        [HttpGet]
        public JsonResult GetStackholderTypeByForm(string fromName)
        {
            try
            {
                if (string.IsNullOrEmpty(fromName))
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
                    .FirstOrDefault(f => f.FormName == fromName);

                if (formStyle == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "نوع النموذج غير موجود",
                        data = new List<object>()
                    });
                }

                // ✅ Return Id + Name directly (NO second request needed)
                var allowedTypes = _unitOfWork.Repository<StakeholderType>()
                    .GetAll(includeEntities: "StakeholderTypeForms")
                    .Where(st => st.StakeholderTypeForms.Any(f => f.FormId == formStyle.Id))
                    .Select(st => new { id = st.Id, name = st.Type })
                    .ToList();

                if (!allowedTypes.Any())
                {
                    return Json(new
                    {
                        success = false,
                        error = "لا توجد أنواع جهات مرتبطة بهذا النموذج",
                        data = new List<object>()
                    });
                }

                return Json(new { success = true, data = allowedTypes });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل أنواع الجهات للنموذج",
                    data = new List<object>()
                });
            }
        }

        // Get stakeholders by stakeholder type ID
        [HttpGet]
        public JsonResult GetStackholderByGetType(int typeId)
        {
            try
            {
                _logger.LogInformation(
                    "Getting stakeholders for type {TypeId} at {Time} by {User}",
                    typeId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                if (typeId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = "معرف نوع الجهة غير صحيح",
                        data = new List<object>()
                    });
                }

                // Get active stakeholders that have the specified type
                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes")
                    .Where(s => s.Status &&
                               s.StakeholderInfoTypes.Any(st => st.StakeholderTypeId == typeId))
                    .Select(s => new {
                        id = s.Id,
                        name = s.Name
                    })
                    .OrderBy(s => s.name)
                    .ToList();

                if (!stakeholders.Any())
                {
                    _logger.LogInformation(
                        "No stakeholders found for type {TypeId} at {Time} by {User}",
                        typeId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                    return Json(new
                    {
                        success = false,
                        error = "لا توجد جهات نشطة لهذا النوع",
                        data = new List<object>()
                    });
                }

                _logger.LogInformation(
                    "Found {Count} stakeholders for type {TypeId} at {Time} by {User}",
                    stakeholders.Count, typeId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Ammar-Yasser8");

                return Json(new
                {
                    success = true,
                    data = stakeholders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting stakeholders for type {TypeId} by {User} at {Time}",
                    typeId, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل الجهات لهذا النوع",
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

                var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId, includeEntities: "PackagingStyle")
                    .OrderBy(t => t.Id)
                    .ToList();

                if (!allTransactions.Any())
                {
                    return Json(new
                    {
                        success = true,
                        yarnItemId = yarnItem.Id,
                        yarnItem = yarnItem.Item ?? "غير محدد",
                        originYarn = yarnItem.OriginYarn?.Item ?? "غير محدد",
                        manufacturer = (yarnItem.Manufacturers != null && yarnItem.Manufacturers.Any())
                            ? string.Join("، ", yarnItem.Manufacturers.Select(m => m.Name))
                            : "غير محدد",
                        totalQuantityBalance = 0,
                        totalCountBalance = 0,
                        calculatedTotalWeight = 0,
                        calculatedTotalCount = 0,
                        packagingBreakdown = new List<object>(),
                        packagingDisplay = "لا توجد وحدات",
                        shortPackagingDisplay = "لا توجد وحدات",
                        detailedBalanceDisplay = "لا يوجد رصيد",
                        hasMultiplePackaging = false,
                        packagingTypesCount = 0,
                        transactionCount = 0,
                        lastTransactionDate = "لا توجد معاملات",
                        hasTransactions = false,
                        balanceMatches = true,
                        balanceDiscrepancy = 0
                    });
                }

                // آخر معاملة
                var latestTransaction = allTransactions.Last();

                // إذا كانت آخر معاملة Reset (رصيد صفر)
                if (latestTransaction.Inbound == 0 && latestTransaction.Outbound == 0)
                {
                    var actualQuantityBalance = latestTransaction.QuantityBalance;
                    var actualCountBalance = latestTransaction.CountBalance;

                    return Json(new
                    {
                        success = true,
                        yarnItemId = yarnItem.Id,
                        yarnItem = yarnItem.Item ?? "غير محدد",
                        originYarn = yarnItem.OriginYarn?.Item ?? "غير محدد",
                        manufacturer = (yarnItem.Manufacturers != null && yarnItem.Manufacturers.Any())
                            ? string.Join("، ", yarnItem.Manufacturers.Select(m => m.Name))
                            : "غير محدد",

                        totalQuantityBalance = actualQuantityBalance,
                        totalCountBalance = actualCountBalance,

                        calculatedTotalWeight = actualQuantityBalance,
                        calculatedTotalCount = actualCountBalance,

                        recordedQuantityBalance = actualQuantityBalance,
                        recordedCountBalance = actualCountBalance,

                        packagingBreakdown = new List<object>(),

                        packagingDisplay = "لا توجد وحدات",
                        shortPackagingDisplay = "لا توجد وحدات",
                        detailedBalanceDisplay = "لا يوجد رصيد",

                        hasMultiplePackaging = false,
                        packagingTypesCount = 0,
                        transactionCount = allTransactions.Count,
                        lastTransactionDate = latestTransaction.Date.ToString("yyyy-MM-dd"),
                        hasTransactions = true,

                        balanceMatches = true,
                        balanceDiscrepancy = 0
                    });
                }

                // الحساب الطبيعي إذا لم تكن آخر معاملة Reset
                var packagingBreakdown = allTransactions
                    .GroupBy(t => new
                    {
                        PackagingId = t.PackagingStyleId,
                        PackagingName = t.PackagingStyle?.StyleName ?? "غير محدد"
                    })
                    .Select(g =>
                    {
                        var totalInbound = g.Where(t => t.Inbound > 0).Sum(t => t.Inbound);
                        var totalOutbound = g.Where(t => t.Outbound > 0).Sum(t => t.Outbound);
                        var inboundCount = g.Where(t => t.Inbound > 0).Sum(t => t.Count);
                        var outboundCount = g.Where(t => t.Outbound > 0).Sum(t => t.Count);

                        decimal averageInboundPerUnit = inboundCount > 0 ? totalInbound / inboundCount : 0m;
                        decimal averageOutboundPerUnit = outboundCount > 0 ? totalOutbound / outboundCount : 0m;

                        decimal specificWeight = totalInbound - totalOutbound;
                        int totalCount = inboundCount - outboundCount;

                        var transactionDetails = g.Select(t => new
                        {
                            date = t.Date,
                            inbound = Math.Round(t.Inbound, 3),
                            outbound = Math.Round(t.Outbound, 3),
                            count = t.Count,
                            unitWeight = t.Count > 0 ? Math.Round((t.Inbound > 0 ? t.Inbound : t.Outbound) / (decimal)t.Count, 3) : 0m
                        }).OrderBy(td => td.date).ToList();

                        return new
                        {
                            packagingId = g.Key.PackagingId,
                            packagingType = g.Key.PackagingName,
                            totalInbound,
                            totalOutbound,
                            inboundCount,
                            outboundCount,
                            specificWeight,
                            totalCount,
                            averageInboundPerUnit = Math.Round(averageInboundPerUnit, 3),
                            averageOutboundPerUnit = Math.Round(averageOutboundPerUnit, 3),
                            transactionDetails
                        };
                    })
                    .OrderByDescending(p => p.specificWeight)
                    .ToList();

                var calculatedTotalWeight = Math.Round(packagingBreakdown.Sum(p => p.specificWeight), 3);
                var calculatedTotalCount = packagingBreakdown.Sum(p => p.totalCount);

                var recordedQuantityBalance = latestTransaction.QuantityBalance;
                var recordedCountBalance = latestTransaction.CountBalance;

                var balanceMatches = Math.Abs(calculatedTotalWeight - recordedQuantityBalance) < 0.001m &&
                                     calculatedTotalCount == recordedCountBalance;

                var actualQuantityBalanceNormal = recordedQuantityBalance;
                var actualCountBalanceNormal = recordedCountBalance;

                // Build display strings
                string packagingDisplay;
                string shortPackagingDisplay;
                string detailedBalanceDisplay;

                if (packagingBreakdown.Any())
                {
                    var positivePackaging = packagingBreakdown.Where(p => p.specificWeight > 0 || p.totalCount > 0).ToList();
                    var negativePackaging = packagingBreakdown.Where(p => p.specificWeight < 0 || p.totalCount < 0).ToList();

                    var packagingParts = positivePackaging.Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType} ({ToArabicDigits(p.totalCount.ToString())} وحدة)")
                        .ToList();

                    if (negativePackaging.Any())
                    {
                        var negativeParts = negativePackaging.Select(p =>
                            $"⚠️ {ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType} ({ToArabicDigits(p.totalCount.ToString())} وحدة)");
                        packagingParts.AddRange(negativeParts);
                    }

                    packagingDisplay = string.Join(" + ", packagingParts);

                    var weightParts = positivePackaging.Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}").ToList();

                    if (negativePackaging.Any())
                    {
                        var negativeWeightParts = negativePackaging.Select(p =>
                            $"⚠️ {ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}");
                        weightParts.AddRange(negativeWeightParts);
                    }

                    detailedBalanceDisplay =
                        $"الرصيد الإجمالي: {ToArabicDigits(Math.Round(actualQuantityBalanceNormal, 3).ToString())} كجم\n" +
                        string.Join(" + ", weightParts);

                    var topPackaging = positivePackaging.Take(2).Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}").ToList();

                    shortPackagingDisplay = string.Join(" + ", topPackaging);
                    if (packagingBreakdown.Count > 2)
                    {
                        var remaining = packagingBreakdown.Skip(2);
                        var remainingWeight = remaining.Sum(p => p.specificWeight);
                        var remainingCount = remaining.Sum(p => p.totalCount);
                        shortPackagingDisplay +=
                            $" + {ToArabicDigits(Math.Round(remainingWeight, 2).ToString())} كجم أخرى ({ToArabicDigits(remainingCount.ToString())} وحدة)";
                    }
                }
                else
                {
                    packagingDisplay = "لا توجد وحدات";
                    shortPackagingDisplay = "لا توجد وحدات";
                    detailedBalanceDisplay = "لا يوجد رصيد";
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

                    totalQuantityBalance = actualQuantityBalanceNormal,
                    totalCountBalance = actualCountBalanceNormal,

                    calculatedTotalWeight = calculatedTotalWeight,
                    calculatedTotalCount = calculatedTotalCount,

                    recordedQuantityBalance = recordedQuantityBalance,
                    recordedCountBalance = recordedCountBalance,

                    packagingBreakdown = packagingBreakdown.Select(p => new
                    {
                        packagingId = p.packagingId,
                        packagingType = p.packagingType,
                        totalCount = p.totalCount,
                        specificWeight = Math.Round(p.specificWeight, 3),
                        averageInboundPerUnit = Math.Round(p.averageInboundPerUnit, 3),
                        averageOutboundPerUnit = Math.Round(p.averageOutboundPerUnit, 3),
                        totalInbound = Math.Round(p.totalInbound, 3),
                        totalOutbound = Math.Round(p.totalOutbound, 3),
                        inboundCount = p.inboundCount,
                        outboundCount = p.outboundCount,
                        isNegative = p.specificWeight < 0 || p.totalCount < 0,
                        displayText = $"{ToArabicDigits(p.totalCount.ToString())} {p.packagingType} ({ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم)",
                        transactionDetails = p.transactionDetails
                    }),

                    packagingDisplay = packagingDisplay,
                    shortPackagingDisplay = shortPackagingDisplay,
                    detailedBalanceDisplay = detailedBalanceDisplay,

                    hasMultiplePackaging = packagingBreakdown.Count > 1,
                    packagingTypesCount = packagingBreakdown.Count,
                    transactionCount = allTransactions.Count,
                    lastTransactionDate = latestTransaction?.Date.ToString("yyyy-MM-dd") ?? "لا توجد معاملات",
                    hasTransactions = allTransactions.Any(),

                    balanceMatches = balanceMatches,
                    balanceDiscrepancy = balanceMatches ? null : new
                    {
                        weightDifference = Math.Round(calculatedTotalWeight - recordedQuantityBalance, 3),
                        countDifference = calculatedTotalCount - recordedCountBalance,
                        message = "الرصيد المحسوب يختلف عن الرصيد المسجل في آخر معاملة"
                    }
                };

                _logger.LogInformation("Yarn item balance loaded for {YarnItemId}: Total={Total}, Calculated={Calculated}, Types={Types}, Match={Match}",
                    yarnItemId, actualQuantityBalanceNormal, calculatedTotalWeight, packagingBreakdown.Count, balanceMatches);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn item balance for ID {YarnItemId}", yarnItemId);
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

        // GET: YarnTransactions/ResetPackagingBalance
        public IActionResult ResetPackagingBalance()
        {
            try
            {
                var viewModel = new YarnResetPackagingBalanceViewModel
                {
                    AvailableItems = GetActiveYarnItems(),
                    AvailablePackagingStyles = GetActivePackagingStyles(),
                    ResetDate = DateTime.Now,
                    ResetBy = "Ammar-Yasser8"
                };

                _logger.LogInformation("Reset packaging balance form loaded by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reset packaging balance form by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة تعديل رصيد التعبئة";
                return RedirectToAction("Overview");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPackagingBalance(int yarnItemId, int packagingStyleId, decimal newQuantity, int newCount, string reason = "")
        {
            try
            {
                var currentTime = DateTime.Now;
                var currentUser = "Ammar-Yasser8";

                // تسجيل القيم المستلمة للتأكد من صحتها
                _logger.LogInformation("ResetPackagingBalance called: yarnItemId={YarnItemId}, packagingStyleId={PackagingStyleId}, newQuantity={NewQuantity}, newCount={NewCount}",
                    yarnItemId, packagingStyleId, newQuantity, newCount);

                if (newQuantity < 0 || newCount < 0)
                    return Json(new { success = false, error = "القيم يجب أن تكون >= 0" });

                var yarnItem = _unitOfWork.Repository<YarnItem>().GetOne(y => y.Id == yarnItemId);
                if (yarnItem == null)
                    return Json(new { success = false, error = "الصنف غير موجود" });

                var packaging = _unitOfWork.Repository<PackagingStyles>().GetOne(p => p.Id == packagingStyleId);
                if (packaging == null)
                    return Json(new { success = false, error = "نوع التعبئة غير موجود" });

                // ===== الطريقة الصحيحة: حساب الرصيد الفعلي الحالي للتعبئة المحددة =====

                // جلب كل المعاملات الخاصة بهذه التعبئة المحددة
                var packagingTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId && t.PackagingStyleId == packagingStyleId)
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToList();

                // حساب الرصيد الفعلي الحالي للتعبئة المحددة من خلال جمع كل المعاملات
                decimal currentPkgQty = 0m;
                int currentPkgCount = 0;

                foreach (var tx in packagingTransactions)
                {
                    // فقط نحسب المعاملات التي ليست معاملات تعديل سابقة
                    // أو نحسب كل المعاملات بما فيها التعديلات
                    currentPkgQty += tx.Inbound - tx.Outbound;

                    // حساب العدد بناءً على نوع المعاملة
                    if (tx.Inbound > 0)
                        currentPkgCount += tx.Count;
                    else if (tx.Outbound > 0)
                        currentPkgCount -= tx.Count;
                }

                currentPkgQty = Math.Round(currentPkgQty, 3);

                _logger.LogInformation("Packaging {PackagingId} current balance calculated: Qty={Qty}, Count={Count}, Total transactions={TxCount}",
                    packagingStyleId, currentPkgQty, currentPkgCount, packagingTransactions.Count);

                // ===== حساب الرصيد الإجمالي الحالي للصنف =====

                var lastOverallTx = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                decimal currentTotalQty = lastOverallTx?.QuantityBalance ?? 0m;
                int currentTotalCount = lastOverallTx?.CountBalance ?? 0;

                // احتساب الفرق المطلوب تطبيقه
                var qtyDiff = Math.Round(newQuantity - currentPkgQty, 3);
                var countDiff = newCount - currentPkgCount;

                _logger.LogInformation("Calculated differences: qtyDiff={QtyDiff}, countDiff={CountDiff}", qtyDiff, countDiff);

                // إذا لا يوجد تغيير — لا تنشئ معاملة
                if (qtyDiff == 0m && countDiff == 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = $"الرصيد الحالي للتعبئة '{packaging.StyleName}' هو بالفعل {newQuantity} كجم / {newCount} وحدة (لا يوجد تغيير)"
                    });
                }

                // جهز معاملة التعديل
                var adjustmentTx = new YarnTransaction
                {
                    TransactionId = GenerateResetTransactionId(),
                    InternalId = $"PKG-ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                    ExternalId = null,
                    YarnItemId = yarnItemId,
                    PackagingStyleId = packagingStyleId,
                    StakeholderId = 1021, // stakeholder for adjustments
                    Date = DateTime.Now,
                    Comment = $"تعديل رصيد التعبئة ({packaging.StyleName}) من {currentPkgQty}/{currentPkgCount} إلى {newQuantity}/{newCount}" +
                              (string.IsNullOrWhiteSpace(reason) ? "" : $" - السبب: {reason}")
                };

                // تحديد نوع المعاملة (Inbound أو Outbound)
                if (qtyDiff > 0m)
                {
                    adjustmentTx.Inbound = qtyDiff;
                    adjustmentTx.Outbound = 0m;
                }
                else // qtyDiff < 0
                {
                    adjustmentTx.Inbound = 0m;
                    adjustmentTx.Outbound = Math.Abs(qtyDiff);
                }

                // العدد (Count) — استخدم الفرق المطلق
                adjustmentTx.Count = Math.Abs(countDiff);

                // حساب الرصيد الإجمالي الجديد بعد تطبيق الفرق
                var newTotalQty = currentTotalQty + qtyDiff;
                var newTotalCount = currentTotalCount + countDiff;

                adjustmentTx.QuantityBalance = Math.Round(newTotalQty, 3);
                adjustmentTx.CountBalance = newTotalCount;

                // التحقق من عدم وجود رصيد سالب
                if (adjustmentTx.QuantityBalance < 0 || adjustmentTx.CountBalance < 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = $"لا يمكن تطبيق التعديل: سيؤدي إلى رصيد سالب (الرصيد الإجمالي الحالي: {currentTotalQty}/{currentTotalCount})"
                    });
                }

                // أضف المعاملة واحفظ
                _unitOfWork.Repository<YarnTransaction>().Add(adjustmentTx);
                _unitOfWork.Complete();

                return Json(new
                {
                    success = true,
                    transactionId = adjustmentTx.TransactionId,
                    oldPackagingBalance = $"{currentPkgQty} كجم / {currentPkgCount} وحدة",
                    newPackagingBalance = $"{newQuantity} كجم / {newCount} وحدة",
                    oldTotalBalance = $"{currentTotalQty} كجم / {currentTotalCount} وحدة",
                    newTotalBalance = $"{adjustmentTx.QuantityBalance} كجم / {adjustmentTx.CountBalance} وحدة",
                    message = $"تم ضبط رصيد التعبئة '{packaging.StyleName}' من {currentPkgQty}/{currentPkgCount} إلى {newQuantity}/{newCount}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting packaging balance for yarnItem {YarnItemId}, packaging {PackagingId}", yarnItemId, packagingStyleId);
                return Json(new { success = false, error = "حدث خطأ أثناء تعديل رصيد التعبئة: " + ex.Message });
            }
        }
        // Helper method to get active packaging styles
        private List<SelectListItem> GetActivePackagingStyles()
        {
            var packagingStyles = _unitOfWork.Repository<PackagingStyles>()
                .GetAll()
                .OrderBy(ps => ps.StyleName)
                .Select(ps => new SelectListItem
                {
                    Value = ps.Id.ToString(),
                    Text = ps.StyleName
                })
                .ToList();

            return packagingStyles;
        }

        // Helper method to generate reset transaction ID
        private string GenerateResetTransactionId()
        {
            try
            {
                var date = DateTime.Now.ToString("yyyyMMdd");
                var prefix = "PKG-ADJ"; // Packaging Adjustment Transaction

                var lastAdjustmentTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.TransactionId.StartsWith($"{prefix}-{date}"))
                    .OrderByDescending(t => t.TransactionId)
                    .FirstOrDefault();

                int sequence = 1;
                if (lastAdjustmentTransaction != null)
                {
                    var parts = lastAdjustmentTransaction.TransactionId.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts.Last(), out int lastSequence))
                    {
                        sequence = lastSequence + 1;
                    }
                }

                var transactionId = $"{prefix}-{date}-{sequence:D4}";

                _logger.LogInformation("Generated packaging adjustment transaction ID: {TransactionId} by {User} at {Time}",
                    transactionId, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return transactionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating packaging adjustment transaction ID by {User} at {Time}",
                    "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return $"PKG-ADJ-{DateTime.Now:yyyyMMdd}-0001";
            }
        }

        // API endpoint to get packaging balance
        [HttpGet]
        public JsonResult GetPackagingBalance(int yarnItemId, int packagingStyleId)
        {
            try
            {
                if (yarnItemId <= 0 || packagingStyleId <= 0)
                {
                    return Json(new { success = false, error = "معرف الصنف أو التعبئة غير صحيح" });
                }

                // Get all transactions for this yarn item with this packaging style
                var packagingTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId && t.PackagingStyleId == packagingStyleId)
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToList();

                decimal currentQuantity = 0;
                int currentCount = 0;

                if (packagingTransactions.Any())
                {
                    currentQuantity = packagingTransactions.Sum(t =>
                        (t.Inbound > 0 ? t.Inbound : 0) - (t.Outbound > 0 ? t.Outbound : 0));
                    currentCount = packagingTransactions.Sum(t =>
                        (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));
                }

                return Json(new
                {
                    success = true,
                    currentQuantity = Math.Round(currentQuantity, 3),
                    currentCount = currentCount,
                    transactionCount = packagingTransactions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting packaging balance for yarn {YarnItemId}, packaging {PackagingId}",
                    yarnItemId, packagingStyleId);
                return Json(new { success = false, error = "حدث خطأ أثناء تحميل رصيد التعبئة" });
            }
        }

        public class YarnResetPackagingBalanceViewModel
        {
            [Required(ErrorMessage = "يجب اختيار صنف الغزل")]
            [Display(Name = "صنف الغزل")]
            public int YarnItemId { get; set; }

            public string? YarnItemName { get; set; }

            [Required(ErrorMessage = "يجب اختيار نوع التعبئة")]
            [Display(Name = "نوع التعبئة")]
            public int PackagingStyleId { get; set; }

            public string? PackagingStyleName { get; set; }

            [Required(ErrorMessage = "يجب إدخال الكمية المطلوبة")]
            [Range(0, double.MaxValue, ErrorMessage = "الكمية يجب أن تكون موجبة أو صفر")]
            [Display(Name = "الكمية المطلوبة (كجم)")]
            public decimal DesiredQuantityBalance { get; set; }

            [Required(ErrorMessage = "يجب إدخال العدد المطلوب")]
            [Range(0, int.MaxValue, ErrorMessage = "العدد يجب أن يكون موجب أو صفر")]
            [Display(Name = "العدد المطلوب")]
            public int DesiredCountBalance { get; set; }

            [Display(Name = "الرصيد الحالي للتعبئة - الكمية")]
            public decimal CurrentPackagingQuantity { get; set; }

            [Display(Name = "الرصيد الحالي للتعبئة - العدد")]
            public int CurrentPackagingCount { get; set; }

            [Display(Name = "الرصيد الإجمالي الحالي - الكمية")]
            public decimal CurrentTotalQuantity { get; set; }

            [Display(Name = "الرصيد الإجمالي الحالي - العدد")]
            public int CurrentTotalCount { get; set; }

            [Display(Name = "سبب التعديل")]
            [StringLength(500, ErrorMessage = "سبب التعديل يجب أن لا يتجاوز 500 حرف")]
            public string? ReasonForReset { get; set; }

            [Display(Name = "تاريخ التعديل")]
            public DateTime ResetDate { get; set; }

            [Display(Name = "المستخدم")]
            public string? ResetBy { get; set; }

            public bool ShowConfirmation { get; set; }

            public bool ConfirmReset { get; set; }

            public List<SelectListItem> AvailableItems { get; set; } = new List<SelectListItem>();

            public List<SelectListItem> AvailablePackagingStyles { get; set; } = new List<SelectListItem>();
        }

    }
}
