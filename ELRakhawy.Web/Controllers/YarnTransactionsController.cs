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
                ModelState.Remove("StakeholderTypeId");
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
        // Get: YarnTransactions/Edit/5
        // Keep your GET Edit action exactly as is:
        public IActionResult Edit(int id)
        {
            try
            {
                var transaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetOne(t => t.Id == id, includeEntities: "YarnItem,Stakeholder,PackagingStyle");
                if (transaction == null)
                {
                    TempData["Error"] = "المعاملة غير موجودة!";
                    return RedirectToAction("Index", "YarnItems");
                }
                var viewModel = new YarnTransactionViewModel
                {
                    Id = transaction.Id,
                    TransactionId = transaction.TransactionId,
                    InternalId = transaction.InternalId,
                    ExternalId = transaction.ExternalId,
                    YarnItemId = transaction.YarnItemId,
                    YarnItemName = transaction.YarnItem.Item,
                    Quantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound,
                    IsInbound = transaction.Inbound > 0,
                    Count = transaction.Count,
                    StakeholderId = transaction.StakeholderId,
                    StakeholderName = transaction.Stakeholder.Name,
                    PackagingStyleId = transaction.PackagingStyleId,
                    PackagingStyleName = transaction.PackagingStyle.StyleName,
                    Date = transaction.Date,
                    Comment = transaction.Comment,
                    AvailableItems = GetActiveYarnItems()
                };
                _logger.LogInformation("Yarn transaction edit form loaded for TransactionId {TransactionId} by {User} at {Time}",
                    transaction.TransactionId, "Ammar-Yasser8", "2025-09-04 17:09:12");
                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading yarn transaction edit form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 17:09:12");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة التعديل";
                return RedirectToAction("Index", "YarnItems");
            }
        }

        // Keep your POST Edit action exactly as is:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(YarnTransactionViewModel model)
        {
            try
            {
                // ✅ إزالة الحقول اللي السيرفر بيحسبها
                ModelState.Remove("YarnItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("StakeholderTypeName");
                ModelState.Remove("PackagingStyleName");
                ModelState.Remove("TransactionId");
                ModelState.Remove("OriginYarnName");
                ModelState.Remove("StakeholderTypeId");
                ModelState.Remove("Date");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveYarnItems();
                    return View("TransactionForm", model);
                }

                // ✅ جلب المعاملة القديمة
                var oldTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetOne(t => t.Id == model.Id);

                if (oldTransaction == null)
                {
                    TempData["Error"] = "المعاملة غير موجودة!";
                    return RedirectToAction("Index", "YarnItems");
                }

                // ✅ جلب آخر معاملة سابقة لهذا العنصر
                var previousLatest = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == model.YarnItemId && t.Id != model.Id)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                var currentQuantityBalance = previousLatest?.QuantityBalance ?? 0;
                var currentCountBalance = previousLatest?.CountBalance ?? 0;

                // ✅ تحقق الرصيد لو العملية Outbound
                if (!model.IsInbound && model.Quantity > currentQuantityBalance)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({currentQuantityBalance:N3})");
                    model.AvailableItems = GetActiveYarnItems();
                    return View("TransactionForm", model);
                }

                // ✅ تحديث نفس السجل (مش إضافة واحد جديد)
                oldTransaction.InternalId = model.InternalId?.Trim();
                oldTransaction.ExternalId = model.ExternalId?.Trim();
                oldTransaction.YarnItemId = model.YarnItemId;
                oldTransaction.Inbound = model.IsInbound ? model.Quantity : 0;
                oldTransaction.Outbound = model.IsInbound ? 0 : model.Quantity;
                oldTransaction.Count = model.Count;
                oldTransaction.StakeholderId = model.StakeholderId;
                oldTransaction.PackagingStyleId = model.PackagingStyleId;
                oldTransaction.Date = model.Date;
                oldTransaction.Comment = model.Comment?.Trim();

                // ✅ إعادة احتساب الرصيد بناءً على أحدث قيم
                oldTransaction.QuantityBalance = currentQuantityBalance + (oldTransaction.Inbound - oldTransaction.Outbound);

                var countInbound = oldTransaction.Inbound > 0 ? oldTransaction.Count : 0;
                var countOutbound = oldTransaction.Outbound > 0 ? oldTransaction.Count : 0;
                oldTransaction.CountBalance = currentCountBalance + countInbound - countOutbound;

                _unitOfWork.Repository<YarnTransaction>().Update(oldTransaction);
                _unitOfWork.Complete();

                TempData["Success"] = $"تم تعديل {(model.IsInbound ? "وارد" : "صادر")} الغزل بنجاح - رقم الإذن: {oldTransaction.TransactionId}";
                return RedirectToAction("Index", "YarnItems");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing yarn transaction by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-04 17:09:12");
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل المعاملة");
                model.AvailableItems = GetActiveYarnItems();
                return View("TransactionForm", model);
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
                return _unitOfWork.Repository<PackagingStyles>()
                    .GetAll()
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.StyleName
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting packaging styles");
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

                // Get all transactions with packaging details for accurate calculation
                var allTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId, includeEntities: "PackagingStyle")
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
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

                // ✅ Calculate packaging breakdown - KEEP NEGATIVE VALUES to show discrepancies
                var packagingBreakdown = allTransactions
                    .GroupBy(t => new {
                        PackagingId = t.PackagingStyleId,
                        PackagingName = t.PackagingStyle?.StyleName ?? "غير محدد"
                    })
                    .Select(g => new
                    {
                        packagingId = g.Key.PackagingId,
                        packagingType = g.Key.PackagingName,

                        // ✅ Calculate remaining count (can be negative!)
                        totalCount = g.Sum(t => (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0)),

                        // ✅ Calculate remaining weight (can be negative!)
                        specificWeight = g.Sum(t => (t.Inbound > 0 ? t.Inbound : 0) - (t.Outbound > 0 ? t.Outbound : 0)),

                        // ✅ Calculate average weight per unit
                        averageWeightPerUnit = g.Where(t => t.Count > 0).Any() ?
                            g.Where(t => t.Count > 0).Average(t =>
                                (t.Inbound > 0 ? t.Inbound : t.Outbound) / (decimal)t.Count) : 0,

                        // ✅ Track inbound and outbound separately for analysis
                        totalInbound = g.Where(t => t.Inbound > 0).Sum(t => t.Inbound),
                        totalOutbound = g.Where(t => t.Outbound > 0).Sum(t => t.Outbound),
                        inboundCount = g.Where(t => t.Inbound > 0).Sum(t => t.Count),
                        outboundCount = g.Where(t => t.Outbound > 0).Sum(t => t.Count),

                        transactionDetails = g.Select(t => new {
                            date = t.Date,
                            inbound = t.Inbound,
                            outbound = t.Outbound,
                            count = t.Count,
                            unitWeight = t.Count > 0 ? Math.Round(
                                (t.Inbound > 0 ? t.Inbound : t.Outbound) / (decimal)t.Count, 3) : 0
                        }).ToList()
                    })
                    .OrderByDescending(p => p.specificWeight) // Positive balances first
                    .ToList();

                // ✅ Calculate totals (including negative values)
                var calculatedTotalWeight = packagingBreakdown.Sum(p => p.specificWeight);
                var calculatedTotalCount = packagingBreakdown.Sum(p => p.totalCount);

                // ✅ Get latest transaction for comparison
                var latestTransaction = allTransactions.LastOrDefault();
                var recordedQuantityBalance = latestTransaction?.QuantityBalance ?? 0;
                var recordedCountBalance = latestTransaction?.CountBalance ?? 0;

                // ✅ Check if recorded balance matches calculated balance
                var balanceMatches = Math.Abs(calculatedTotalWeight - recordedQuantityBalance) < 0.001m &&
                                   calculatedTotalCount == recordedCountBalance;

                // ✅ Use recorded balance as the TRUE total (from last transaction)
                var actualQuantityBalance = recordedQuantityBalance;
                var actualCountBalance = recordedCountBalance;

                // ✅ Create packaging display
                string packagingDisplay;
                string shortPackagingDisplay;
                string detailedBalanceDisplay;

                if (packagingBreakdown.Any())
                {
                    // ✅ Separate positive and negative packaging items
                    var positivePackaging = packagingBreakdown.Where(p => p.specificWeight > 0 || p.totalCount > 0).ToList();
                    var negativePackaging = packagingBreakdown.Where(p => p.specificWeight < 0 || p.totalCount < 0).ToList();

                    // Display positive items
                    var packagingParts = positivePackaging.Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم " +
                        $"{p.packagingType} ({ToArabicDigits(p.totalCount.ToString())} وحدة)");

                    // Add negative items with warning
                    if (negativePackaging.Any())
                    {
                        var negativeParts = negativePackaging.Select(p =>
                            $"⚠️ {ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم " +
                            $"{p.packagingType} ({ToArabicDigits(p.totalCount.ToString())} وحدة)");
                        packagingParts = packagingParts.Concat(negativeParts);
                    }

                    packagingDisplay = string.Join(" + ", packagingParts);

                    // Detailed balance display
                    var weightParts = positivePackaging.Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}");

                    if (negativePackaging.Any())
                    {
                        var negativeWeightParts = negativePackaging.Select(p =>
                            $"⚠️ {ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}");
                        weightParts = weightParts.Concat(negativeWeightParts);
                    }

                    detailedBalanceDisplay =
                        $"الرصيد الإجمالي: {ToArabicDigits(Math.Round(actualQuantityBalance, 3).ToString())} كجم\n" +
                        string.Join(" + ", weightParts);

                    // Short display
                    var topPackaging = positivePackaging.Take(2).Select(p =>
                        $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}");
                    shortPackagingDisplay = string.Join(" + ", topPackaging);

                    if (packagingBreakdown.Count > 2)
                    {
                        var remainingItems = packagingBreakdown.Skip(2);
                        var remainingWeight = remainingItems.Sum(p => p.specificWeight);
                        var remainingCount = remainingItems.Sum(p => p.totalCount);
                        shortPackagingDisplay +=
                            $" + {ToArabicDigits(Math.Round(remainingWeight, 2).ToString())} كجم أخرى " +
                            $"({ToArabicDigits(remainingCount.ToString())} وحدة)";
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

                    // ✅ Return recorded balance as the TRUE total
                    totalQuantityBalance = Math.Round(actualQuantityBalance, 3),
                    totalCountBalance = actualCountBalance,

                    // ✅ Also return calculated totals for comparison
                    calculatedTotalWeight = Math.Round(calculatedTotalWeight, 3),
                    calculatedTotalCount = calculatedTotalCount,

                    // ✅ Return recorded balances
                    recordedQuantityBalance = Math.Round(recordedQuantityBalance, 3),
                    recordedCountBalance = recordedCountBalance,

                    // ✅ Packaging breakdown (includes negative values!)
                    packagingBreakdown = packagingBreakdown.Select(p => new {
                        packagingId = p.packagingId,
                        packagingType = p.packagingType,
                        totalCount = p.totalCount, // Can be negative
                        specificWeight = Math.Round(p.specificWeight, 3), // Can be negative
                        averageWeightPerUnit = Math.Round(p.averageWeightPerUnit, 3),

                        // ✅ Add inbound/outbound totals for context
                        totalInbound = Math.Round(p.totalInbound, 3),
                        totalOutbound = Math.Round(p.totalOutbound, 3),
                        inboundCount = p.inboundCount,
                        outboundCount = p.outboundCount,

                        // ✅ Flag if negative
                        isNegative = p.specificWeight < 0 || p.totalCount < 0,

                        displayText = $"{ToArabicDigits(p.totalCount.ToString())} {p.packagingType} " +
                                     $"({ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم)",
                        weightOnlyText = $"{ToArabicDigits(Math.Round(p.specificWeight, 2).ToString())} كجم {p.packagingType}",
                        countOnlyText = $"{ToArabicDigits(p.totalCount.ToString())} {p.packagingType}",

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

                    // ✅ Balance verification
                    balanceMatches = balanceMatches,
                    balanceDiscrepancy = balanceMatches ? null : new
                    {
                        weightDifference = Math.Round(calculatedTotalWeight - recordedQuantityBalance, 3),
                        countDifference = calculatedTotalCount - recordedCountBalance,
                        message = "الرصيد المحسوب يختلف عن الرصيد المسجل في آخر معاملة"
                    }
                };

                _logger.LogInformation("Yarn item balance loaded for {YarnItemId}: " +
                    "Total={Total}, Calculated={Calculated}, Packaging Types={Types}, Match={Match}",
                    yarnItemId, actualQuantityBalance, calculatedTotalWeight, packagingBreakdown.Count, balanceMatches);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yarn item balance for ID {YarnItemId}",
                    yarnItemId);

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

        // POST: YarnTransactions/ResetPackagingBalance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPackagingBalance(int yarnItemId, int packagingStyleId, decimal desiredQuantityBalance, int desiredCountBalance, string reason = "")
        {
            try
            {
                var currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var currentUser = "Ammar-Yasser8";

                // Validate inputs
                if (desiredQuantityBalance < 0 || desiredCountBalance < 0)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, error = "القيم المطلوبة يجب أن تكون موجبة أو صفر" });
                    }
                    TempData["Error"] = "القيم المطلوبة يجب أن تكون موجبة أو صفر";
                    return RedirectToAction("ResetPackagingBalance");
                }

                // Get the yarn item
                var yarnItem = _unitOfWork.Repository<YarnItem>()
                    .GetOne(yi => yi.Id == yarnItemId);

                if (yarnItem == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, error = "الصنف غير موجود" });
                    }
                    TempData["Error"] = "الصنف غير موجود";
                    return RedirectToAction("Overview");
                }

                // Get the packaging style
                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(ps => ps.Id == packagingStyleId);

                if (packagingStyle == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, error = "نوع التعبئة غير موجود" });
                    }
                    TempData["Error"] = "نوع التعبئة غير موجود";
                    return RedirectToAction("ResetPackagingBalance");
                }

                _logger.LogInformation("🔄 Reset packaging balance requested for yarn item {YarnItemId}, packaging {PackagingId} to Qty={DesiredQty}, Count={DesiredCount} by {User} at {Time}",
                    yarnItemId, packagingStyleId, desiredQuantityBalance, desiredCountBalance, currentUser, currentTime);

                // Get all transactions for this yarn item with this packaging style
                var packagingTransactions = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId && t.PackagingStyleId == packagingStyleId)
                    .OrderBy(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToList();

                // Calculate current balance for this specific packaging
                decimal currentPackagingQuantity = 0;
                int currentPackagingCount = 0;

                if (packagingTransactions.Any())
                {
                    currentPackagingQuantity = packagingTransactions.Sum(t =>
                        (t.Inbound > 0 ? t.Inbound : 0) - (t.Outbound > 0 ? t.Outbound : 0));
                    currentPackagingCount = packagingTransactions.Sum(t =>
                        (t.Inbound > 0 ? t.Count : 0) - (t.Outbound > 0 ? t.Count : 0));
                }

                _logger.LogInformation("📊 Current packaging balance: Quantity={Quantity}, Count={Count} for yarn item {YarnItemId}, packaging {PackagingId}",
                    currentPackagingQuantity, currentPackagingCount, yarnItemId, packagingStyleId);

                // Calculate the difference needed
                var quantityDifference = desiredQuantityBalance - currentPackagingQuantity;
                var countDifference = desiredCountBalance - currentPackagingCount;

                // Check if adjustment is needed
                if (quantityDifference == 0 && countDifference == 0)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, error = "رصيد التعبئة الحالي يطابق القيم المطلوبة بالفعل" });
                    }
                    TempData["Info"] = "رصيد التعبئة الحالي يطابق القيم المطلوبة بالفعل";
                    return RedirectToAction("ResetPackagingBalance");
                }

                _logger.LogInformation("📊 Packaging adjustment needed: Qty Diff={QtyDiff}, Count Diff={CountDiff}",
                    quantityDifference, countDifference);

                // Get overall current balance for the yarn item (UNCHANGED)
                var latestTransaction = _unitOfWork.Repository<YarnTransaction>()
                    .GetAll(t => t.YarnItemId == yarnItemId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefault();

                var currentTotalQuantity = latestTransaction?.QuantityBalance ?? 0;
                var currentTotalCount = latestTransaction?.CountBalance ?? 0;

                // Get or create adjustment stakeholder
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

                // Create adjustment transaction for this specific packaging
                var adjustmentTransaction = new YarnTransaction
                {
                    TransactionId = GenerateResetTransactionId(),
                    InternalId = $"PKG-ADJ-{DateTime.Now:yyyyMMddHHmmss}",
                    ExternalId = null,
                    YarnItemId = yarnItemId,
                    PackagingStyleId = packagingStyleId,
                    StakeholderId = adjustmentStakeholder.Id,
                    Date = DateTime.Now,
                    Comment = $"تعديل رصيد تعبئة محددة (مع تطبيق الفرق على الرصيد الإجمالي) - الصنف: {yarnItem?.Item}. نوع التعبئة: {packagingStyle?.StyleName}. السبب: {(string.IsNullOrEmpty(reason) ? "تسوية إدارية" : reason)}. الرصيد السابق للتعبئة: {currentPackagingQuantity:N3} كمية، {currentPackagingCount} عدد. الرصيد المطلوب للتعبئة: {desiredQuantityBalance:N3} كمية، {desiredCountBalance} عدد. تم بواسطة: {currentUser} في {currentTime}"
                };

                // Set inbound/outbound based on quantity difference
                if (quantityDifference > 0)
                {
                    // Need to add quantity (Inbound)
                    adjustmentTransaction.Inbound = quantityDifference;
                    adjustmentTransaction.Outbound = 0;
                }
                else if (quantityDifference < 0)
                {
                    // Need to remove quantity (Outbound)
                    adjustmentTransaction.Inbound = 0;
                    adjustmentTransaction.Outbound = Math.Abs(quantityDifference);
                }
                else
                {
                    // No quantity change
                    adjustmentTransaction.Inbound = 0;
                    adjustmentTransaction.Outbound = 0;
                }

                // Set count based on difference
                adjustmentTransaction.Count = Math.Abs(countDifference);

                // ✅ ADD the difference to total balance
                // If reducing packaging (negative difference), total will also reduce
                // If increasing packaging (positive difference), total will also increase
                var newTotalQuantity = currentTotalQuantity + quantityDifference;
                var newTotalCount = currentTotalCount + countDifference;

                adjustmentTransaction.QuantityBalance = newTotalQuantity;
                adjustmentTransaction.CountBalance = newTotalCount;

                _logger.LogInformation("📋 Packaging adjustment transaction created: ID={TransactionId}, Packaging={PackagingId}, Inbound={Inbound}, Outbound={Outbound}, Count={Count}, NewTotalBalance={NewQuantity}/{NewCount} (Applied difference)",
                    adjustmentTransaction.TransactionId, packagingStyleId, adjustmentTransaction.Inbound,
                    adjustmentTransaction.Outbound, adjustmentTransaction.Count,
                    adjustmentTransaction.QuantityBalance, adjustmentTransaction.CountBalance);

                // Add to database
                _unitOfWork.Repository<YarnTransaction>().Add(adjustmentTransaction);
                _unitOfWork.Complete();

                _logger.LogInformation("✅ Packaging balance adjustment completed for yarn item {YarnItemId}, packaging {PackagingId} - Transaction {TransactionId} created by {User} at {Time}. Packaging: {OldQty}/{OldCount} → {NewQty}/{NewCount}. Total: {OldTotal}/{OldTotalCount} → {NewTotal}/{NewTotalCount} (Difference applied)",
                    yarnItemId, packagingStyleId, adjustmentTransaction.TransactionId, currentUser, currentTime,
                    currentPackagingQuantity, currentPackagingCount, desiredQuantityBalance, desiredCountBalance,
                    currentTotalQuantity, currentTotalCount, newTotalQuantity, newTotalCount);

                // Return JSON for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        transactionId = adjustmentTransaction.TransactionId,
                        message = $"تم تعديل رصيد التعبئة بنجاح - رقم معاملة التسوية: {adjustmentTransaction.TransactionId}",
                        detailsUrl = Url.Action("Overview")
                    });
                }

                TempData["Success"] = $"تم تعديل رصيد التعبئة بنجاح (تم تطبيق الفرق على الرصيد الإجمالي) - رقم معاملة التسوية: {adjustmentTransaction.TransactionId}";
                return RedirectToAction("Overview");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error adjusting packaging balance for yarn item {YarnItemId}, packaging {PackagingId} by {User} at {Time}",
                    yarnItemId, packagingStyleId, "Ammar-Yasser8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "حدث خطأ أثناء تعديل رصيد التعبئة: " + ex.Message });
                }

                TempData["Error"] = "حدث خطأ أثناء تعديل رصيد التعبئة";
                return RedirectToAction("ResetPackagingBalance");
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
