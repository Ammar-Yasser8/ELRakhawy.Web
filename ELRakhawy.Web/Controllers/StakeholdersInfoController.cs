using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class StakeholdersInfoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StakeholdersInfoController> _logger;
        private readonly string _currentUser = "Ammar-Yasser8";
        private readonly string _currentTime = "2025-08-19 18:59:31";

        public StakeholdersInfoController(IUnitOfWork unitOfWork, ILogger<StakeholdersInfoController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region MVC Actions (Original)
        public IActionResult Index()
        {
            try
            {
                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes,StakeholderInfoTypes.StakeholderType,StakeholderInfoTypes.StakeholderType.FinancialTransactionType")
                    .ToList();

                _logger.LogInformation("Retrieved {Count} stakeholders at {Time} by {User}",
                    stakeholders.Count, _currentTime, _currentUser);

                return View(stakeholders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stakeholders at {Time} by {User}", _currentTime, _currentUser);
                TempData["Error"] = "حدث خطأ أثناء تحميل البيانات";
                return View(new List<StakeholdersInfo>());
            }
        }

        // GET: StakeholdersInfo/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new StakeholdersInfoViewModel
                {
                    Status = true, // Default to Active
                    CountryCode = "+20", // Default country code
                    AvailableTypes = _unitOfWork.Repository<StakeholderType>()
                        .GetAll(includeEntities: "FinancialTransactionType")
                        .ToList()
                };

                ViewBag.Action = "Create";
                return PartialView("_CreateOrEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create view at {Time} by {User}", _currentTime, _currentUser);
                return StatusCode(500, "خطأ في تحميل الفورم");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StakeholdersInfoViewModel viewModel)
        {
            try
            {
                // Validation
                if (_unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(s => s.Name.Trim().ToLower() == viewModel.Name.Trim().ToLower())
                    .Any())
                {
                    ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                }

                if (viewModel.PrimaryTypeId.HasValue &&
                    !viewModel.SelectedTypeIds.Contains(viewModel.PrimaryTypeId.Value))
                {
                    ModelState.AddModelError("PrimaryTypeId", "النوع الرئيسي يجب أن يكون من ضمن الأنواع المختارة");
                }
                else if (!viewModel.PrimaryTypeId.HasValue && viewModel.SelectedTypeIds.Any())
                {
                    viewModel.PrimaryTypeId = viewModel.SelectedTypeIds.First();
                }

                if (!ModelState.IsValid)
                {
                    // Return validation errors as JSON
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { success = false, errors = errors });
                }

                // Save
                var stakeholder = new StakeholdersInfo
                {
                    Name = viewModel.Name.Trim(),
                    Status = viewModel.Status,
                    ContactNumbers = $"{viewModel.CountryCode}{viewModel.ContactNumber}",
                    Comment = viewModel.Comment?.Trim(),
                };

                _unitOfWork.Repository<StakeholdersInfo>().Add(stakeholder);
                _unitOfWork.Complete();

                if (viewModel.SelectedTypeIds.Any())
                {
                    var relations = viewModel.SelectedTypeIds.Select(typeId => new StakeholderInfoType
                    {
                        StakeholdersInfoId = stakeholder.Id,
                        StakeholderTypeId = typeId,
                        IsPrimary = typeId == viewModel.PrimaryTypeId,
                    }).ToList();

                    _unitOfWork.Repository<StakeholderInfoType>().AddRange(relations);
                    _unitOfWork.Complete();
                }

                return Json(new { success = true, message = "تم إضافة الجهة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stakeholder at {Time} by {User}", _currentTime, _currentUser);
                return Json(new { success = false, message = "حدث خطأ أثناء الحفظ" });
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == id, "StakeholderInfoTypes");

                if (stakeholder == null) return NotFound();

                // Parse country code and contact number
                string countryCode = "+20"; // Default
                string contactNumber = stakeholder.ContactNumbers ?? "";

                // Extract country code from contact numbers
                if (!string.IsNullOrEmpty(stakeholder.ContactNumbers))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(stakeholder.ContactNumbers, @"^(\+\d{1,4})(.*)");
                    if (match.Success)
                    {
                        countryCode = match.Groups[1].Value;
                        contactNumber = match.Groups[2].Value;
                    }
                }

                var viewModel = new StakeholdersInfoViewModel
                {
                    Id = stakeholder.Id,
                    Name = stakeholder.Name,
                    Status = stakeholder.Status,
                    CountryCode = countryCode,
                    ContactNumber = contactNumber,
                    Comment = stakeholder.Comment,
                    SelectedTypeIds = stakeholder.StakeholderInfoTypes
                        .Select(st => st.StakeholderTypeId)
                        .ToList(),
                    PrimaryTypeId = stakeholder.StakeholderInfoTypes
                        .FirstOrDefault(st => st.IsPrimary)?.StakeholderTypeId,
                    AvailableTypes = _unitOfWork.Repository<StakeholderType>()
                        .GetAll(includeEntities: "FinancialTransactionType")
                        .ToList()
                };

                ViewBag.Action = "Edit";
                return PartialView("_CreateOrEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit view for StakeholdersInfo ID: {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);
                return StatusCode(500, "خطأ في تحميل الفورم");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StakeholdersInfoViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Id) return NotFound();

                if (_unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(s => s.Name.Trim().ToLower() == viewModel.Name.Trim().ToLower() && s.Id != id)
                    .Any())
                {
                    ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                }

                if (viewModel.PrimaryTypeId.HasValue &&
                    !viewModel.SelectedTypeIds.Contains(viewModel.PrimaryTypeId.Value))
                {
                    ModelState.AddModelError("PrimaryTypeId", "النوع الرئيسي يجب أن يكون من ضمن الأنواع المختارة");
                }

                if (!ModelState.IsValid)
                {
                    // Return validation errors as JSON
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { success = false, errors = errors });
                }

                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == id, "StakeholderInfoTypes");
                if (stakeholder == null) return NotFound();

                stakeholder.Name = viewModel.Name.Trim();
                stakeholder.Status = viewModel.Status;
                stakeholder.ContactNumbers = $"{viewModel.CountryCode}{viewModel.ContactNumber}".Trim();
                stakeholder.Comment = viewModel.Comment?.Trim();

                var existingTypes = _unitOfWork.Repository<StakeholderInfoType>()
                    .GetAll(st => st.StakeholdersInfoId == id)
                    .ToList();
                _unitOfWork.Repository<StakeholderInfoType>().RemoveRange(existingTypes);

                if (viewModel.SelectedTypeIds.Any())
                {
                    var newTypes = viewModel.SelectedTypeIds.Select(typeId => new StakeholderInfoType
                    {
                        StakeholdersInfoId = id,
                        StakeholderTypeId = typeId,
                        IsPrimary = typeId == viewModel.PrimaryTypeId
                    }).ToList();

                    _unitOfWork.Repository<StakeholderInfoType>().AddRange(newTypes);
                }

                _unitOfWork.Complete();

                return Json(new { success = true, message = "تم تحديث الجهة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating StakeholdersInfo ID: {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);

                return Json(new { success = false, message = "حدث خطأ أثناء التحديث" });
            }
        }

        // Add this method for loading stakeholders data
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities:"StakeholderInfoTypes.StakeholderType,StakeholderInfoTypes.StakeholderType.FinancialTransactionType")
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        status = s.Status,
                        contactNumbers = s.ContactNumbers,
                        comment = s.Comment,
                        types = s.StakeholderInfoTypes.Select(st => new
                        {
                            id = st.StakeholderTypeId,
                            typeName = st.StakeholderType.Type,
                            isPrimary = st.IsPrimary,
                            financialTransactionType = st.StakeholderType.FinancialTransactionType?.Type
                        }).ToList()
                    })
                    .ToList();

                return Json(new { success = true, data = stakeholders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stakeholders data at {Time} by {User}", _currentTime, _currentUser);
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل البيانات" });
            }
        }
        #endregion

        #region API Endpoints

        // GET: api/StakeholdersInfo
        [HttpGet]
        [Route("api/StakeholdersInfo")]
        public async Task<IActionResult> GetAllApi()
        {
            try
            {
                _logger.LogInformation("API: Getting all stakeholders at {Time} by {User}", _currentTime, _currentUser);

                var stakeholders = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetAll(includeEntities: "StakeholderInfoTypes,StakeholderInfoTypes.StakeholderType,StakeholderInfoTypes.StakeholderType.FinancialTransactionType")
                    .ToList();

                var result = stakeholders.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    status = s.Status,
                    contactNumbers = s.ContactNumbers,
                    comment = s.Comment,
                    types = s.StakeholderInfoTypes.Select(st => new
                    {
                        id = st.StakeholderTypeId,
                        typeName = st.StakeholderType.Type,
                        isPrimary = st.IsPrimary,
                        financialTransactionType = st.StakeholderType.FinancialTransactionType?.Type
                    }).ToList(),
                    createdAt = _currentTime,
                    createdBy = _currentUser
                }).ToList();

                _logger.LogInformation("API: Retrieved {Count} stakeholders successfully at {Time} by {User}",
                    result.Count, _currentTime, _currentUser);

                return Ok(new
                {
                    success = true,
                    data = result,
                    count = result.Count,
                    timestamp = _currentTime,
                    user = _currentUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting all stakeholders at {Time} by {User}", _currentTime, _currentUser);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد بيانات الجهات",
                    timestamp = _currentTime
                });
            }
        }

        // GET: api/StakeholdersInfo/5
        [HttpGet]
        [Route("api/StakeholdersInfo/{id}")]
        public async Task<IActionResult> GetDetailsApi(int id)
        {
            try
            {
                _logger.LogInformation("API: Getting stakeholder details for ID {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);

                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == id, "StakeholderInfoTypes,StakeholderInfoTypes.StakeholderType,StakeholderInfoTypes.StakeholderType.FinancialTransactionType");

                if (stakeholder == null)
                {
                    _logger.LogWarning("API: Stakeholder with ID {Id} not found at {Time} by {User}",
                        id, _currentTime, _currentUser);
                    return NotFound(new
                    {
                        success = false,
                        message = "الجهة غير موجودة",
                        timestamp = _currentTime
                    });
                }

                var result = new
                {
                    id = stakeholder.Id,
                    name = stakeholder.Name,
                    status = stakeholder.Status,
                    contactNumbers = stakeholder.ContactNumbers,
                    comment = stakeholder.Comment,
                    types = stakeholder.StakeholderInfoTypes.Select(st => new
                    {
                        id = st.StakeholderTypeId,
                        typeName = st.StakeholderType.Type,
                        isPrimary = st.IsPrimary,
                        financialTransactionType = st.StakeholderType.FinancialTransactionType?.Type
                    }).ToList(),
                    timestamp = _currentTime,
                    user = _currentUser
                };

                _logger.LogInformation("API: Retrieved stakeholder details for ID {Id} successfully at {Time} by {User}",
                    id, _currentTime, _currentUser);

                return Ok(new
                {
                    success = true,
                    data = result,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting stakeholder details for ID {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد تفاصيل الجهة",
                    timestamp = _currentTime
                });
            }
        }

        // PUT: api/StakeholdersInfo/5/status
        [HttpPut]
        [Route("api/StakeholdersInfo/{id}/status")]
        public async Task<IActionResult> UpdateStatusApi(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                _logger.LogInformation("API: Updating status for stakeholder ID {Id} to {Status} at {Time} by {User}",
                    id, request.Status, _currentTime, _currentUser);

                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>().GetOne(s => s.Id == id);
                if (stakeholder == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "الجهة غير موجودة",
                        timestamp = _currentTime
                    });
                }

                stakeholder.Status = request.Status;
                _unitOfWork.Repository<StakeholdersInfo>().Update(stakeholder);
                _unitOfWork.Complete();

                _logger.LogInformation("API: Status updated successfully for stakeholder ID {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);

                return Ok(new
                {
                    success = true,
                    message = $"تم تحديث حالة {stakeholder.Name} إلى {(request.Status ? "نشط" : "غير نشط")}",
                    data = new { id = stakeholder.Id, status = stakeholder.Status },
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error updating status for stakeholder ID {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحديث حالة الجهة",
                    timestamp = _currentTime
                });
            }
        }

        // DELETE API endpoint for AJAX calls
        [HttpDelete]
        [Route("/StakeholdersInfo/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var stakeholder = _unitOfWork.Repository<StakeholdersInfo>()
                    .GetOne(s => s.Id == id, "StakeholderInfoTypes");

                if (stakeholder == null)
                {
                    _logger.LogWarning("Delete attempt for non-existent StakeholdersInfo ID: {Id} at {Time} by {User}",
                        id, _currentTime, _currentUser);
                    return NotFound(new { message = "الجهة غير موجودة" });
                }

                // Check for related records if needed
                // var hasRelatedRecords = _unitOfWork.Repository<RelatedEntity>()
                //     .GetAll(r => r.StakeholdersInfoId == id)
                //     .Any();
                // if (hasRelatedRecords) {
                //     return BadRequest(new { message = "لا يمكن حذف هذه الجهة لوجود بيانات مرتبطة بها" });
                // }

                // Remove related types first
                var relatedTypes = _unitOfWork.Repository<StakeholderInfoType>()
                    .GetAll(st => st.StakeholdersInfoId == id)
                    .ToList();
                _unitOfWork.Repository<StakeholderInfoType>().RemoveRange(relatedTypes);

                // Remove the stakeholder
                _unitOfWork.Repository<StakeholdersInfo>().Remove(stakeholder);
                _unitOfWork.Complete();

                _logger.LogInformation("Deleted StakeholdersInfo ID: {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);

                return Ok(new
                {
                    success = true,
                    message = $"تم حذف الجهة '{stakeholder.Name}' بنجاح",
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting StakeholdersInfo ID: {Id} at {Time} by {User}",
                    id, _currentTime, _currentUser);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء حذف الجهة",
                    timestamp = _currentTime
                });
            }
        }

        #endregion
    }

    // Request models for API
    public class UpdateStatusRequest
    {
        public bool Status { get; set; }
    }
}