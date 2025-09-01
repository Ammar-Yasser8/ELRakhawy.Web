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

       

        public IActionResult Inbound()
        {
            var model = new FullWarpBeamTransactionViewModel
            {
                IsInbound = true,
                Date = DateTime.Now,
                AvailableItems = GetActiveFullWarpBeamItems()

            };
            _logger.LogInformation("Inbound yarn transaction form loaded by {User} at {Time}",
                   "Ammar-Yasser8", "2025-08-12 02:13:55");

            return View("TransactionForm", model);

        }
        public IActionResult Outbound()
        {
            var model = new FullWarpBeamTransactionViewModel
            {
                IsInbound = false,
                Date = DateTime.Now,
                AvailableItems = GetActiveFullWarpBeamItems()
            };
            _logger.LogInformation("Outbound yarn transaction form loaded by {User} at {Time}",
                   "Ammar-Yasser8", "2025-08-12 02:13:55");
            return View("TransactionForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveTransaction(FullWarpBeamTransactionViewModel model)
        {
            try
            {
                ModelState.Remove("FullWarpBeamItemName");
                ModelState.Remove("StakeholderName");
                ModelState.Remove("QuantityBalance");
                ModelState.Remove("CountBalance");
                ModelState.Remove("Date");
                ModelState.Remove("TransactionId");

                if (!ModelState.IsValid)
                {
                    model.AvailableItems = GetActiveFullWarpBeamItems();
                    return View("TransactionForm", model);
                }

                var currentBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == model.FullWarpBeamItemId)
                    .Sum(t => t.Inbound - t.Outbound);

                var previousCountBalance = _unitOfWork.Repository<FullWarpBeamTransaction>()
                    .GetAll(t => t.FullWarpBeamItemId == model.FullWarpBeamItemId)
                    .Sum(t => (t.Inbound > 0 ? 1 : 0) - (t.Outbound > 0 ? 1 : 0));

                if (!model.IsInbound && model.Quantity > currentBalance)
                {
                    ModelState.AddModelError("Quantity",
                        $"الكمية المطلوبة ({model.Quantity:N3}) أكبر من الرصيد المتاح ({currentBalance:N3})");
                    model.AvailableItems = GetActiveFullWarpBeamItems();
                    return View("TransactionForm", model);
                }

                var transaction = new FullWarpBeamTransaction
                {
                    TransactionId = GenerateTransactionId(),
                    InternalId = model.InternalId,
                    ExternalId = model.ExternalId,
                    FullWarpBeamItemId = model.FullWarpBeamItemId.Value,
                    Inbound = model.IsInbound ? model.Quantity : 0,
                    Outbound = model.IsInbound ? 0 : model.Quantity,
                    Length = model.Length,
                    StakeholderId = model.StakeholderId ?? 0,
                    Date = model.Date,
                    Comment = model.Comment?.Trim(),
                    QuantityBalance = currentBalance + (model.IsInbound ? model.Quantity : -model.Quantity),
                    CountBalance = previousCountBalance + (model.IsInbound ? 1 : -1)
                };

                _unitOfWork.Repository<FullWarpBeamTransaction>().Add(transaction);
                _unitOfWork.Complete();
                TempData["Success"] = $"تم تسجيل {(model.IsInbound ? "وارد" : "صادر")} السداة الكاملة بنجاح - رقم الإذن: {transaction.TransactionId}";
                return RedirectToAction("Index", "FullWarpBeamItems");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ المعاملة");
                model.AvailableItems = GetActiveFullWarpBeamItems();
                
                return View("TransactionForm", model);
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

        

       
        #endregion

    }
}
