using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELRakhawy.Web.Controllers
{
    public class FullWarpBeamTransationsController : Controller
    {
        private readonly ILogger<FullWarpBeamTransationsController> _logger;
        private readonly IUnitOfWork _unitOfWork;   
        public FullWarpBeamTransationsController(ILogger<FullWarpBeamTransationsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }



        // GET: FullWarpBeamTransactions/Inbound
        public IActionResult Inbound()
        {
            try
            {
                var viewModel = new FullWarpBeamTransactionViewModel
                {
                    IsInbound = true,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveFullWarpBeamItems()
                };

                _logger.LogInformation("Inbound full warp beam transaction form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");

                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inbound full warp beam transaction form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الوارد";
                return RedirectToAction("Index", "FullWarpBeamItems");
            }
        }

        // GET: FullWarpBeamTransactions/Outbound
        public IActionResult Outbound()
        {
            try
            {
                var viewModel = new FullWarpBeamTransactionViewModel
                {
                    IsInbound = false,
                    Date = DateTime.Now,
                    AvailableItems = GetActiveFullWarpBeamItems()
                };

                _logger.LogInformation("Outbound full warp beam transaction form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");

                return View("TransactionForm", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading outbound full warp beam transaction form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الصادر";
                return RedirectToAction("Index", "FullWarpBeamItems");
            }
        }

        // POST: FullWarpBeamTransactions/SaveTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTransaction(FullWarpBeamTransactionViewModel model)
        {
            try
            {
                // Remove fields that are populated server-side
                ModelState.Remove("FullWarpBeamItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("TransactionId");
                ModelState.Remove("QuantityBalance");
                ModelState.Remove("CountBalance");
                ModelState.Remove("Date");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveFullWarpBeamItems();
                    return View("TransactionForm", model);
                }

                // Calculate current balance
                var currentBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == model.FullWarpBeamItemId)
                    .Sum(t => t.Inbound - t.Outbound);

                // Check if outbound quantity exceeds available balance
                if (!model.IsInbound && model.Quantity > currentBalance)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({currentBalance:N3})");
                    model.AvailableItems = GetActiveFullWarpBeamItems();
                    return View("TransactionForm", model);
                }

                // Get stakeholder with type information
                var stakeholderInfo = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes.StakeholderType")
                    .FirstOrDefault(s => s.Id == model.StakeholderId);

                var stakeholderTypeId = stakeholderInfo?.StakeholderInfoTypes?.FirstOrDefault()?.StakeholderTypeId ?? 1;

                var transaction = new FullWarpBeamTransaction
                {
                    TransactionId = GenerateTransactionId(),
                    InternalId = model.InternalId?.Trim(),
                    ExternalId = model.ExternalId?.Trim(),
                    FullWarpBeamItemId = model.FullWarpBeamItemId.Value,
                    Inbound = model.IsInbound ? model.Quantity : 0,
                    Outbound = model.IsInbound ? 0 : model.Quantity,
                    Length = model.Length,
                    StakeholderId = model.StakeholderId.Value,
                    Date = model.Date,
                    Comment = model.Comment?.Trim()
                };

                // Calculate balances
                var previousCountBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == model.FullWarpBeamItemId)
                    .Sum(t => (t.Inbound > 0 ? 1 : 0) - (t.Outbound > 0 ? 1 : 0));

                transaction.QuantityBalance = currentBalance + (transaction.Inbound - transaction.Outbound);
                transaction.CountBalance = previousCountBalance + (transaction.Inbound > 0 ? 1 : -1);

                _unitOfWork.Repository<FullWarpBeamTransaction>().Add(transaction);
                _unitOfWork.Complete();

                _logger.LogInformation("Full warp beam transaction {TransactionId} saved successfully - {Type} by {User} at {Time}",
                    transaction.TransactionId, model.IsInbound ? "Inbound" : "Outbound",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");

                TempData["Success"] = $"تم تسجيل {(model.IsInbound ? "وارد" : "صادر")} السداة الكاملة بنجاح - رقم الإذن: {transaction.TransactionId}";
                return RedirectToAction("Index", "FullWarpBeam");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving full warp beam transaction by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 13:42:26");

                ModelState.AddModelError("", "حدث خطأ أثناء حفظ المعاملة");
                model.AvailableItems = GetActiveFullWarpBeamItems();
                return View("TransactionForm", model);
            }
        }


        // Overview of transactions

        // GET: FullWarpBeamTransactions/Overview
        public IActionResult Overview(FullWarpBeamTransactionOverviewViewModel filters = null)
        {
            try
            {
                if (filters == null)
                {
                    filters = new FullWarpBeamTransactionOverviewViewModel
                    {
                        FromDate = DateTime.Now.AddMonths(-1),
                        ToDate = DateTime.Now,
                        TransactionType = "All"
                    };
                }

                var query = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(includeEntities: "FullWarpBeamItem,FullWarpBeamItem.OriginYarn,Stakeholder");

                // Apply filters
                if (filters.SelectedFullWarpBeamItemId.HasValue)
                {
                    query = query.Where(t => t.FullWarpBeamItemId == filters.SelectedFullWarpBeamItemId.Value)
                               .ToList();
                }

                if (filters.SelectedStakeholderId.HasValue)
                {
                    query = query.Where(t => t.StakeholderId == filters.SelectedStakeholderId.Value)
                               .ToList();
                }

                if (filters.FromDate.HasValue)
                {
                    query = query.Where(t => t.Date >= filters.FromDate.Value)
                               .ToList();
                }

                if (filters.ToDate.HasValue)
                {
                    query = query.Where(t => t.Date <= filters.ToDate.Value)
                               .ToList();
                }

                if (!string.IsNullOrEmpty(filters.TransactionType) && filters.TransactionType != "All")
                {
                    bool isInbound = filters.TransactionType == "Inbound";
                    query = query.Where(t => (t.Inbound > 0) == isInbound)
                               .ToList();
                }

                if (!string.IsNullOrEmpty(filters.SearchQuery))
                {
                    var searchLower = filters.SearchQuery.ToLower();
                    query = query.Where(t =>
                        t.TransactionId.ToLower().Contains(searchLower) ||
                        (t.InternalId != null && t.InternalId.ToLower().Contains(searchLower)) ||
                        (t.ExternalId != null && t.ExternalId.ToLower().Contains(searchLower)) ||
                        t.FullWarpBeamItem.Item.ToLower().Contains(searchLower) ||
                        t.Stakeholder.Name.ToLower().Contains(searchLower) ||
                        (t.Comment != null && t.Comment.ToLower().Contains(searchLower))
                    ).ToList();
                }

                var transactions = query.OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .Select(t => new FullWarpBeamTransactionSummaryDto
                    {
                        Id = t.Id,
                        TransactionId = t.TransactionId,
                        InternalId = t.InternalId,
                        ExternalId = t.ExternalId,
                        Date = t.Date,
                        FullWarpBeamItemName = t.FullWarpBeamItem.Item,
                        StakeholderName = t.Stakeholder.Name,
                        Quantity = t.Inbound > 0 ? t.Inbound : t.Outbound,
                        Length = t.Length,
                        IsInbound = t.Inbound > 0,
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance,
                        Comment = t.Comment
                    })
                    .ToList();

                // Calculate summary statistics
                var allTransactions = _unitOfWork.Repository<FullWarpBeamTransaction>().GetAll();

                filters.TotalInbound = allTransactions.Sum(t => t.Inbound);
                filters.TotalOutbound = allTransactions.Sum(t => t.Outbound);
                filters.CurrentBalance = filters.TotalInbound - filters.TotalOutbound;
                filters.TotalTransactions = allTransactions.Count();
                filters.TotalLengthInbound = allTransactions.Where(t => t.Inbound > 0).Sum(t => t.Length);
                filters.TotalLengthOutbound = allTransactions.Where(t => t.Outbound > 0).Sum(t => t.Length);
                filters.CurrentLengthBalance = filters.TotalLengthInbound - filters.TotalLengthOutbound;

                filters.Transactions = transactions;
                filters.FullWarpBeamItems = GetActiveFullWarpBeamItems();
                filters.Stakeholders = GetAllStakeholders();

                _logger.LogInformation("Full warp beam transactions overview loaded by {User} at {Time} - {Count} transactions found",
                    "Ammar-Yasser8", "2025-09-01 15:16:06", transactions.Count);

                return View(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading full warp beam transactions overview by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 15:16:06");
                TempData["Error"] = "حدث خطأ أثناء تحميل معاملات السداة الكاملة";
                return RedirectToAction("Index", "FullWarpBeam");
            }
        }

        // GET: FullWarpBeamTransactions/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var transaction = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(includeEntities: "FullWarpBeamItem,FullWarpBeamItem.OriginYarn,Stakeholder")
                    .FirstOrDefault(t => t.Id == id);

                if (transaction == null)
                {
                    TempData["Error"] = "المعاملة غير موجودة";
                    return RedirectToAction("Overview");
                }

                var relatedTransactions = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == transaction.FullWarpBeamItemId && t.Id != id,
                           includeEntities: "Stakeholder")
                    .OrderByDescending(t => t.Date)
                    .Take(10)
                    .Select(t => new FullWarpBeamTransactionSummaryDto
                    {
                        Id = t.Id,
                        TransactionId = t.TransactionId,
                        Date = t.Date,
                        StakeholderName = t.Stakeholder.Name,
                        Quantity = t.Inbound > 0 ? t.Inbound : t.Outbound,
                        Length = t.Length,
                        IsInbound = t.Inbound > 0,
                        QuantityBalance = t.QuantityBalance,
                        CountBalance = t.CountBalance
                    })
                    .ToList();

                // Calculate current balances for this item
                var currentItemBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == transaction.FullWarpBeamItemId)
                    .Sum(t => t.Inbound - t.Outbound);

                var currentItemLengthBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == transaction.FullWarpBeamItemId)
                    .Sum(t => (t.Inbound > 0 ? t.Length : 0) - (t.Outbound > 0 ? t.Length : 0));

                var viewModel = new FullWarpBeamTransactionDetailsViewModel
                {
                    Transaction = new FullWarpBeamTransactionSummaryDto
                    {
                        Id = transaction.Id,
                        TransactionId = transaction.TransactionId,
                        InternalId = transaction.InternalId,
                        ExternalId = transaction.ExternalId,
                        Date = transaction.Date,
                        FullWarpBeamItemName = transaction.FullWarpBeamItem.Item,
                        StakeholderName = transaction.Stakeholder.Name,
                        Quantity = transaction.Inbound > 0 ? transaction.Inbound : transaction.Outbound,
                        Length = transaction.Length,
                        IsInbound = transaction.Inbound > 0,
                        QuantityBalance = transaction.QuantityBalance,
                        CountBalance = transaction.CountBalance,
                        Comment = transaction.Comment
                    },
                    FullWarpBeamItemDetails = transaction.FullWarpBeamItem.Item,
                    OriginYarnName = transaction.FullWarpBeamItem.OriginYarn?.Item ?? "غير محدد",
                    RelatedTransactions = relatedTransactions,
                    CurrentItemBalance = currentItemBalance,
                    CurrentItemLengthBalance = currentItemLengthBalance
                };

                _logger.LogInformation("Full warp beam transaction details {TransactionId} viewed by {User} at {Time}",
                    transaction.TransactionId, "Ammar-Yasser8", "2025-09-01 15:16:06");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading full warp beam transaction details {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-01 15:16:06");
                TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل المعاملة";
                return RedirectToAction("Overview");
            }
        }

        // GET: FullWarpBeamTransactions/GetTransactionsByItem/5
        [HttpGet]
        public JsonResult GetTransactionsByItem(int itemId, int page = 1, int pageSize = 10)
        {
            try
            {
                var transactions = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == itemId, includeEntities: "StakeholdersInfo")
                    .OrderByDescending(t => t.Date)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        id = t.Id,
                        transactionId = t.TransactionId,
                        date = t.Date.ToString("yyyy-MM-dd"),
                        stakeholder = t.Stakeholder.Name,
                        quantity = t.Inbound > 0 ? t.Inbound : t.Outbound,
                        length = t.Length,
                        isInbound = t.Inbound > 0,
                        quantityBalance = t.QuantityBalance,
                        type = t.Inbound > 0 ? "وارد" : "صادر",
                        typeClass = t.Inbound > 0 ? "success" : "warning"
                    })
                    .ToList();

                var totalCount = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == itemId)
                    .Count();

                return Json(new
                {
                    success = true,
                    data = transactions,
                    totalCount = totalCount,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for item {ItemId} by {User} at {Time}",
                    itemId, "Ammar-Yasser8", "2025-09-01 15:16:06");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل المعاملات"
                });
            }
        }

        // Helper method to get all stakeholders
        private List<SelectListItem> GetAllStakeholders()
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
                _logger.LogError(ex, "Error getting all stakeholders by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-01 15:16:06");
                return new List<SelectListItem>();
            }
        }






        #region Helpers
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

        // Helper method to get active FullWarpBeam items
        private List<SelectListItem> GetActiveFullWarpBeamItems()
        {
            try
            {
                return _unitOfWork.Repository<FullWarpBeam>()
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
                var prefix = "FWB"; // Full WarpBeam Transaction

                var lastTransaction = _unitOfWork.Repository<FullWarpBeamTransaction>()
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

        // ADD THIS MISSING ACTION
        [HttpGet]
        public JsonResult GetFullWarpBeamItemBalance(int fullWarpBeamItemId)
        {
            try
            {
                _logger.LogInformation("Getting balance for full warp beam item {ItemId} by {User} at {Time}",
                    fullWarpBeamItemId, "Ammar-Yasser8", "2025-09-01 14:34:10");

                var transactions = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == fullWarpBeamItemId)
                    .ToList();

                var item = _unitOfWork.Repository<FullWarpBeam>()
                    .GetAll(includeEntities: "OriginYarn")
                    .FirstOrDefault(i => i.Id == fullWarpBeamItemId);

                if (item == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "الصنف غير موجود"
                    });
                }

                var quantityBalance = transactions.Sum(t => t.Inbound - t.Outbound);
                var lengthBalance = transactions.Sum(t => (t.Inbound > 0 ? t.Length : 0) - (t.Outbound > 0 ? t.Length : 0));
                var countBalance = transactions.Sum(t => (t.Inbound > 0 ? 1 : 0) - (t.Outbound > 0 ? 1 : 0));

                var lastTransaction = transactions.OrderByDescending(t => t.Date).FirstOrDefault();

                var result = new
                {
                    success = true,
                    quantityBalance = quantityBalance,
                    lengthBalance = lengthBalance,
                    countBalance = countBalance,
                    transactionCount = transactions.Count,
                    lastTransactionDate = lastTransaction?.Date.ToString("yyyy-MM-dd") ?? "",
                    fullWarpBeamItem = item.Item ?? "غير معروف",
                    originYarn = item.OriginYarn?.Item ?? "غير معروف",
                    warpType = item.OriginYarn?.Item ?? "غير معروف",
                    manufacturer = "غير محدد",
                    yarnCount = "غير محدد"
                };

                _logger.LogInformation("Balance retrieved successfully for item {ItemId}: Quantity={Quantity}, Length={Length}",
                    fullWarpBeamItemId, quantityBalance, lengthBalance);

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full warp beam item balance for {ItemId} by {User} at {Time}",
                    fullWarpBeamItemId, "Ammar-Yasser8", "2025-09-01 14:34:10");

                return Json(new
                {
                    success = false,
                    error = "حدث خطأ أثناء تحميل الأرصدة"
                });
            }
        }

        #endregion

    }
}
