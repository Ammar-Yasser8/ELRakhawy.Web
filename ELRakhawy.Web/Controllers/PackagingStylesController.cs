using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ELRakhawy.Web.Controllers
{
    public class PackagingStylesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PackagingStylesController> _logger;
        private readonly string _currentUser = "Ammar-Yasser8";
        private readonly string _currentTime = "2025-08-16 19:45:59";

        public PackagingStylesController(IUnitOfWork unitOfWork, ILogger<PackagingStylesController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region MVC Actions (Original)

        // GET: PackagingStyles
        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Loading packaging styles index page by {User} at {Time}", _currentUser, _currentTime);

                var packagingStyles = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll(includeEntities: "PackagingStyleForms,PackagingStyleForms.Form");

                _logger.LogInformation("Loaded {Count} packaging styles for index by {User} at {Time}",
                    packagingStyles.Count(), _currentUser, _currentTime);

                return View(packagingStyles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving packaging styles by {User} at {Time}",
                    _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد أنماط التعبئة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // GET: PackagingStyles/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                _logger.LogInformation("Loading packaging style details for ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                if (packagingStyle == null)
                {
                    _logger.LogWarning("Packaging style with ID {Id} not found by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                _logger.LogInformation("Loaded packaging style details for '{StyleName}' (ID: {Id}) by {User} at {Time}",
                    packagingStyle.StyleName, id, _currentUser, _currentTime);

                return View(packagingStyle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving packaging style details for ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد تفاصيل نمط التعبئة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // GET: PackagingStyles/Create
        public IActionResult Create()
        {
            try
            {
                _logger.LogInformation("Loading create packaging style form by {User} at {Time}", _currentUser, _currentTime);

                var forms = _unitOfWork.Repository<FormStyle>().GetAll();

                var viewModel = new PackagingStyleViewModel
                {
                    AvailableForms = forms.Select(f => new FormStyleViewModel
                    {
                        Id = f.Id,
                        FormName = f.FormName,
                        IsSelected = false
                    }).ToList()
                };

                _logger.LogInformation("Loaded create form with {FormsCount} available forms by {User} at {Time}",
                    forms.Count(), _currentUser, _currentTime);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create packaging style form by {User} at {Time}",
                    _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء تحميل نموذج إنشاء نمط التعبئة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // POST: PackagingStyles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PackagingStyleViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                LoadAvailableFormsForViewModel(viewModel);
                return View(viewModel);
            }

            try
            {
                _logger.LogInformation("Creating packaging style '{StyleName}' by {User} at {Time}",
                    viewModel.StyleName, _currentUser, _currentTime);

                var existingStyle = _unitOfWork.Repository<PackagingStyles>()
                   .GetOne(p => p.StyleName.Trim().ToLower() == viewModel.StyleName.Trim().ToLower());

                if (existingStyle != null)
                {
                    _logger.LogWarning("Duplicate packaging style name '{StyleName}' attempted by {User} at {Time}",
                        viewModel.StyleName, _currentUser, _currentTime);
                    ModelState.AddModelError("StyleName", "اسم التعبئة موجود بالفعل");
                    LoadAvailableFormsForViewModel(viewModel);
                    return View(viewModel);
                }

                // Create new packaging style
                var packagingStyle = new PackagingStyles
                {
                    StyleName = viewModel.StyleName.Trim(),
                    Comment = viewModel.Comment?.Trim() ?? string.Empty
                };

                // Add packaging style to repository
                _unitOfWork.Repository<PackagingStyles>().Add(packagingStyle);
                _unitOfWork.Complete();  // Save to get the new ID

                // Add form relationships if any selected
                if (viewModel.SelectedFormIds?.Count > 0)
                {
                    var relationships = new List<PackagingStyleForms>(viewModel.SelectedFormIds.Count);
                    foreach (var formId in viewModel.SelectedFormIds)
                    {
                        relationships.Add(new PackagingStyleForms
                        {
                            PackagingStyleId = packagingStyle.Id,
                            FormId = formId
                        });
                    }

                    _unitOfWork.Repository<PackagingStyleForms>().AddRange(relationships);
                    _unitOfWork.Complete();
                }

                _logger.LogInformation("Packaging style '{StyleName}' created successfully with ID {Id} by {User} at {Time}",
                    viewModel.StyleName, packagingStyle.Id, _currentUser, _currentTime);

                TempData["Success"] = "تم إنشاء نمط التعبئة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating packaging style: {StyleName} by {User} at {Time}",
                    viewModel.StyleName, _currentUser, _currentTime);
                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء نمط التعبئة");

                LoadAvailableFormsForViewModel(viewModel);
                return View(viewModel);
            }
        }

        private void LoadAvailableFormsForViewModel(PackagingStyleViewModel viewModel)
        {
            var forms = _unitOfWork.Repository<FormStyle>().GetAll();
            var selectedFormIds = viewModel.SelectedFormIds;

            viewModel.AvailableForms = new List<FormStyleViewModel>(forms.Count);
            foreach (var form in forms)
            {
                viewModel.AvailableForms.Add(new FormStyleViewModel
                {
                    Id = form.Id,
                    FormName = form.FormName,
                    IsSelected = selectedFormIds?.Contains(form.Id) ?? false
                });
            }
        }

        // GET: PackagingStyles/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                _logger.LogInformation("Loading edit form for packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms");

                if (packagingStyle == null)
                {
                    _logger.LogWarning("Packaging style with ID {Id} not found for edit by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound();
                }

                // Get all related form IDs for this packaging style
                var relatedFormIds = packagingStyle.PackagingStyleForms
                    .Select(psf => psf.FormId)
                    .ToList();

                // Get all available forms
                var allForms = _unitOfWork.Repository<FormStyle>().GetAll();

                var viewModel = new PackagingStyleViewModel
                {
                    Id = packagingStyle.Id,
                    StyleName = packagingStyle.StyleName,
                    Comment = packagingStyle.Comment,
                    SelectedFormIds = relatedFormIds,
                    AvailableForms = allForms.Select(f => new FormStyleViewModel
                    {
                        Id = f.Id,
                        FormName = f.FormName,
                        IsSelected = relatedFormIds.Contains(f.Id)
                    }).ToList()
                };

                _logger.LogInformation("Loaded edit form for packaging style '{StyleName}' (ID: {Id}) by {User} at {Time}",
                    packagingStyle.StyleName, id, _currentUser, _currentTime);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving packaging style for edit, ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد نمط التعبئة للتعديل. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // POST: PackagingStyles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PackagingStyleViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var forms = _unitOfWork.Repository<FormStyle>().GetAll();
                viewModel.AvailableForms = forms.Select(f => new FormStyleViewModel
                {
                    Id = f.Id,
                    FormName = f.FormName,
                    IsSelected = viewModel.SelectedFormIds?.Contains(f.Id) ?? false
                }).ToList();

                return View(viewModel);
            }

            try
            {
                _logger.LogInformation("Updating packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                // Check if packaging style exists
                var packagingStyle = _unitOfWork.Repository<PackagingStyles>().GetOne(p => p.Id == id);
                if (packagingStyle == null)
                {
                    return NotFound();
                }

                // Check if name already exists (excluding current style)
                var existingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.StyleName.Trim().ToLower() == viewModel.StyleName.Trim().ToLower() && p.Id != id);

                if (existingStyle != null)
                {
                    ModelState.AddModelError("StyleName", "اسم التعبئة موجود بالفعل");

                    var forms = _unitOfWork.Repository<FormStyle>().GetAll();
                    viewModel.AvailableForms = forms.Select(f => new FormStyleViewModel
                    {
                        Id = f.Id,
                        FormName = f.FormName,
                        IsSelected = viewModel.SelectedFormIds?.Contains(f.Id) ?? false
                    }).ToList();

                    return View(viewModel);
                }

                // Update packaging style properties
                packagingStyle.StyleName = viewModel.StyleName.Trim();
                packagingStyle.Comment = viewModel.Comment?.Trim() ?? string.Empty;

                _unitOfWork.Repository<PackagingStyles>().Update(packagingStyle);

                // Remove existing form relationships
                var existingRelationships = _unitOfWork.Repository<PackagingStyleForms>()
                    .GetAll(p => p.PackagingStyleId == id);

                if (existingRelationships?.Any() == true)
                {
                    _unitOfWork.Repository<PackagingStyleForms>().RemoveRange(existingRelationships);
                }

                // Add new form relationships
                if (viewModel.SelectedFormIds?.Any() == true)
                {
                    var newRelationships = viewModel.SelectedFormIds.Select(formId => new PackagingStyleForms
                    {
                        PackagingStyleId = id,
                        FormId = formId
                    }).ToList();

                    _unitOfWork.Repository<PackagingStyleForms>().AddRange(newRelationships);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Packaging style '{StyleName}' (ID: {Id}) updated successfully by {User} at {Time}",
                    viewModel.StyleName, id, _currentUser, _currentTime);

                TempData["Success"] = "تم تحديث نمط التعبئة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating packaging style ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث نمط التعبئة");

                var forms = _unitOfWork.Repository<FormStyle>().GetAll();
                viewModel.AvailableForms = forms.Select(f => new FormStyleViewModel
                {
                    Id = f.Id,
                    FormName = f.FormName,
                    IsSelected = viewModel.SelectedFormIds?.Contains(f.Id) ?? false
                }).ToList();

                return View(viewModel);
            }
        }

        // GET: PackagingStyles/Delete/5
        public IActionResult Delete(int id)
        {
            try
            {
                _logger.LogInformation("Loading delete confirmation for packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                if (packagingStyle == null)
                {
                    return NotFound();
                }

                return View(packagingStyle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving packaging style for delete, ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد نمط التعبئة للحذف. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // POST: PackagingStyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Deleting packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>().GetOne(p => p.Id == id);
                if (packagingStyle == null)
                {
                    return NotFound();
                }

                string styleName = packagingStyle.StyleName;
                _unitOfWork.Repository<PackagingStyles>().Remove(packagingStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("Packaging style '{StyleName}' (ID: {Id}) deleted successfully by {User} at {Time}",
                    styleName, id, _currentUser, _currentTime);

                TempData["Success"] = "تم حذف نمط التعبئة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting packaging style ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء حذف نمط التعبئة. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }

        // GET: PackagingStyles/GetFormsByPackagingStyle/5
        [HttpGet]
        public IActionResult GetFormsByPackagingStyle(int id)
        {
            try
            {
                _logger.LogInformation("Getting forms for packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                if (packagingStyle == null)
                {
                    return NotFound();
                }

                var forms = packagingStyle.PackagingStyleForms
                    .Select(psf => new { id = psf.FormId, name = psf.Form.FormName })
                    .ToList();

                return Json(forms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving forms for packaging style ID: {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, "حدث خطأ أثناء استرداد الواجهات المرتبطة بنمط التعبئة");
            }
        }

        #endregion

        #region API Endpoints

        // GET: api/PackagingStyles
        [HttpGet]
        [Route("api/PackagingStyles")]
        public async Task<IActionResult> GetAllApi()
        {
            try
            {
                _logger.LogInformation("API: Getting all packaging styles by {User} at {Time}", _currentUser, _currentTime);

                var packagingStyles = _unitOfWork.Repository<PackagingStyles>()
                    .GetAll(includeEntities: "PackagingStyleForms,PackagingStyleForms.Form");

                var result = packagingStyles.Select(ps => new
                {
                    id = ps.Id,
                    styleName = ps.StyleName,
                    comment = ps.Comment,
                    formsCount = ps.PackagingStyleForms?.Count ?? 0,
                    forms = ps.PackagingStyleForms?.Select(psf => new
                    {
                        id = psf.FormId,
                        name = psf.Form.FormName
                    }).ToList() ,
                    createdAt = _currentTime,
                    createdBy = _currentUser
                }).ToList();

                _logger.LogInformation("API: Retrieved {Count} packaging styles successfully by {User} at {Time}",
                    result.Count, _currentUser, _currentTime);

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
                _logger.LogError(ex, "API: Error getting all packaging styles by {User} at {Time}", _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد أنماط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        // GET: api/PackagingStyles/5
        [HttpGet]
        [Route("api/PackagingStyles/{id}")]
        public async Task<IActionResult> GetDetailsApi(int id)
        {
            try
            {
                _logger.LogInformation("API: Getting packaging style details for ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                if (packagingStyle == null)
                {
                    _logger.LogWarning("API: Packaging style with ID {Id} not found by {User} at {Time}",
                        id, _currentUser, _currentTime);
                    return NotFound(new
                    {
                        success = false,
                        message = "نمط التعبئة غير موجود",
                        timestamp = _currentTime
                    });
                }

                var result = new
                {
                    id = packagingStyle.Id,
                    styleName = packagingStyle.StyleName,
                    comment = packagingStyle.Comment,
                    formsCount = packagingStyle.PackagingStyleForms?.Count ?? 0,
                    forms = packagingStyle.PackagingStyleForms?.Select(psf => new
                    {
                        id = psf.FormId,
                        name = psf.Form.FormName,
                        isSelected = true
                    }).ToList() ,
                    timestamp = _currentTime,
                    user = _currentUser
                };

                _logger.LogInformation("API: Retrieved packaging style details for ID {Id} successfully by {User} at {Time}",
                    id, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    data = result,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting packaging style details for ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد تفاصيل نمط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        // GET: api/PackagingStyles/GetAvailableForms
        [HttpGet]
        [Route("api/PackagingStyles/GetAvailableForms")]
        public async Task<IActionResult> GetAvailableFormsApi()
        {
            try
            {
                _logger.LogInformation("API: Getting available forms by {User} at {Time}", _currentUser, _currentTime);

                var forms = _unitOfWork.Repository<FormStyle>().GetAll();

                var result = forms.Select(f => new
                {
                    id = f.Id,
                    formName = f.FormName,
                    isSelected = false
                }).ToList();

                _logger.LogInformation("API: Retrieved {Count} available forms successfully by {User} at {Time}",
                    result.Count, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    data = result,
                    count = result.Count,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting available forms by {User} at {Time}", _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد الواجهات المتاحة",
                    timestamp = _currentTime
                });
            }
        }

        // POST: api/PackagingStyles
        [HttpPost]
        [Route("api/PackagingStyles")]
        public async Task<IActionResult> CreateApi([FromBody] PackagingStyleApiRequest request)
        {
            try
            {
                _logger.LogInformation("API: Creating packaging style '{StyleName}' by {User} at {Time}",
                    request.StyleName, _currentUser, _currentTime);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "بيانات غير صحيحة",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)),
                        timestamp = _currentTime
                    });
                }

                // Check for duplicate name
                var existingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.StyleName.Trim().ToLower() == request.StyleName.Trim().ToLower());

                if (existingStyle != null)
                {
                    _logger.LogWarning("API: Duplicate packaging style name '{StyleName}' attempted by {User} at {Time}",
                        request.StyleName, _currentUser, _currentTime);
                    return BadRequest(new
                    {
                        success = false,
                        message = "اسم نمط التعبئة موجود بالفعل",
                        timestamp = _currentTime
                    });
                }

                // Create new packaging style
                var packagingStyle = new PackagingStyles
                {
                    StyleName = request.StyleName.Trim(),
                    Comment = request.Comment?.Trim() ?? string.Empty
                };

                _unitOfWork.Repository<PackagingStyles>().Add(packagingStyle);
                _unitOfWork.Complete(); // Save to get the new ID

                // Add form relationships if any selected
                if (request.SelectedFormIds?.Any() == true)
                {
                    var relationships = request.SelectedFormIds.Select(formId => new PackagingStyleForms
                    {
                        PackagingStyleId = packagingStyle.Id,
                        FormId = formId
                    }).ToList();

                    _unitOfWork.Repository<PackagingStyleForms>().AddRange(relationships);
                    _unitOfWork.Complete();
                }

                // Get the created packaging style with forms
                var createdStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == packagingStyle.Id, "PackagingStyleForms,PackagingStyleForms.Form");

                var result = new
                {
                    id = createdStyle.Id,
                    styleName = createdStyle.StyleName,
                    comment = createdStyle.Comment,
                    formsCount = createdStyle.PackagingStyleForms?.Count ?? 0,
                    forms = createdStyle.PackagingStyleForms?.Select(psf => new
                    {
                        id = psf.FormId,
                        name = psf.Form.FormName
                    }).ToList(),
                    createdAt = _currentTime,
                    createdBy = _currentUser
                };

                _logger.LogInformation("API: Packaging style '{StyleName}' created successfully with ID {Id} by {User} at {Time}",
                    request.StyleName, packagingStyle.Id, _currentUser, _currentTime);

                return Created($"api/PackagingStyles/{packagingStyle.Id}", new
                {
                    success = true,
                    message = "تم إنشاء نمط التعبئة بنجاح",
                    data = result,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error creating packaging style '{StyleName}' by {User} at {Time}",
                    request?.StyleName, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء إنشاء نمط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        // PUT: api/PackagingStyles/5
        [HttpPut]
        [Route("api/PackagingStyles/{id}")]
        public async Task<IActionResult> UpdateApi(int id, [FromBody] PackagingStyleApiRequest request)
        {
            try
            {
                _logger.LogInformation("API: Updating packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "بيانات غير صحيحة",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)),
                        timestamp = _currentTime
                    });
                }

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>().GetOne(p => p.Id == id);
                if (packagingStyle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نمط التعبئة غير موجود",
                        timestamp = _currentTime
                    });
                }

                // Check for duplicate name (excluding current style)
                var existingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.StyleName.Trim().ToLower() == request.StyleName.Trim().ToLower() && p.Id != id);

                if (existingStyle != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "اسم نمط التعبئة موجود بالفعل",
                        timestamp = _currentTime
                    });
                }

                // Update packaging style properties
                packagingStyle.StyleName = request.StyleName.Trim();
                packagingStyle.Comment = request.Comment?.Trim() ?? string.Empty;

                _unitOfWork.Repository<PackagingStyles>().Update(packagingStyle);

                // Remove existing form relationships
                var existingRelationships = _unitOfWork.Repository<PackagingStyleForms>()
                    .GetAll(p => p.PackagingStyleId == id);

                if (existingRelationships?.Any() == true)
                {
                    _unitOfWork.Repository<PackagingStyleForms>().RemoveRange(existingRelationships);
                }

                // Add new form relationships
                if (request.SelectedFormIds?.Any() == true)
                {
                    var newRelationships = request.SelectedFormIds.Select(formId => new PackagingStyleForms
                    {
                        PackagingStyleId = id,
                        FormId = formId
                    }).ToList();

                    _unitOfWork.Repository<PackagingStyleForms>().AddRange(newRelationships);
                }

                _unitOfWork.Complete();

                // Get updated packaging style with forms
                var updatedStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                var result = new
                {
                    id = updatedStyle.Id,
                    styleName = updatedStyle.StyleName,
                    comment = updatedStyle.Comment,
                    formsCount = updatedStyle.PackagingStyleForms?.Count ?? 0,
                    forms = updatedStyle.PackagingStyleForms?.Select(psf => new
                    {
                        id = psf.FormId,
                        name = psf.Form.FormName
                    }).ToList(),
                    updatedAt = _currentTime,
                    updatedBy = _currentUser
                };

                _logger.LogInformation("API: Packaging style ID {Id} updated successfully by {User} at {Time}",
                    id, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    message = "تم تحديث نمط التعبئة بنجاح",
                    data = result,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error updating packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء تحديث نمط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        // DELETE: api/PackagingStyles/5
        [HttpDelete]
        [Route("api/PackagingStyles/{id}")]
        public async Task<IActionResult> DeleteApi(int id)
        {
            try
            {
                _logger.LogInformation("API: Deleting packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>().GetOne(p => p.Id == id);
                if (packagingStyle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نمط التعبئة غير موجود",
                        timestamp = _currentTime
                    });
                }

                string styleName = packagingStyle.StyleName;
                _unitOfWork.Repository<PackagingStyles>().Remove(packagingStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("API: Packaging style '{StyleName}' (ID: {Id}) deleted successfully by {User} at {Time}",
                    styleName, id, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    message = $"تم حذف نمط التعبئة '{styleName}' بنجاح",
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error deleting packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء حذف نمط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        // GET: api/PackagingStyles/5/forms
        [HttpGet]
        [Route("api/PackagingStyles/{id}/forms")]
        public async Task<IActionResult> GetFormsByPackagingStyleApi(int id)
        {
            try
            {
                _logger.LogInformation("API: Getting forms for packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);

                var packagingStyle = _unitOfWork.Repository<PackagingStyles>()
                    .GetOne(p => p.Id == id, "PackagingStyleForms,PackagingStyleForms.Form");

                if (packagingStyle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نمط التعبئة غير موجود",
                        timestamp = _currentTime
                    });
                }

                var forms = packagingStyle.PackagingStyleForms?.Select(psf => new
                {
                    id = psf.FormId,
                    name = psf.Form.FormName,
                    packagingStyleId = psf.PackagingStyleId
                }).ToList() ;

                _logger.LogInformation("API: Retrieved {Count} forms for packaging style ID {Id} by {User} at {Time}",
                    forms.Count, id, _currentUser, _currentTime);

                return Ok(new
                {
                    success = true,
                    data = forms,
                    count = forms.Count,
                    packagingStyleId = id,
                    packagingStyleName = packagingStyle.StyleName,
                    timestamp = _currentTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting forms for packaging style ID {Id} by {User} at {Time}",
                    id, _currentUser, _currentTime);
                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ أثناء استرداد الواجهات المرتبطة بنمط التعبئة",
                    timestamp = _currentTime
                });
            }
        }

        #endregion
    }

    // API Request Model
    public class PackagingStyleApiRequest
    {
        [Required(ErrorMessage = "اسم نمط التعبئة مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم نمط التعبئة يجب ألا يتجاوز 200 حرف")]
        public string StyleName { get; set; }

        [MaxLength(1000, ErrorMessage = "التعليق يجب ألا يتجاوز 1000 حرف")]
        public string? Comment { get; set; }

        public List<int>? SelectedFormIds { get; set; }
    }
}
