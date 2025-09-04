using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class FabricStylesController : Controller
    {
        private readonly ILogger<FabricStylesController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FabricStylesController(ILogger<FabricStylesController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: FabricStyles
        public IActionResult Index(string searchQuery = "", int page = 1, int pageSize = 20)
        {
            try
            {
                var viewModel = new FabricStyleIndexViewModel
                {
                    SearchQuery = searchQuery,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                // Build query
                var query = _unitOfWork.Repository<FabricStyle>()
                    .GetAll()
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(f => f.Style.Contains(searchQuery) ||
                                           f.Comment.Contains(searchQuery));
                }

                // Calculate pagination
                var allItems = query.ToList();
                viewModel.TotalItems = allItems.Count;
                viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalItems / pageSize);

                // Get paginated results
                var fabricStyles = allItems
                    
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FabricStyleViewModel
                    {
                        Id = f.Id,
                        Style = f.Style,
                        Comment = f.Comment
                    })
                    .ToList();

                viewModel.FabricStyles = fabricStyles;

                _logger.LogInformation("Fabric styles index loaded by {User} at {Time} - Found {Count} styles",
                    "Ammar-Yasser8", "2025-09-02 19:18:10", viewModel.TotalItems);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric styles index by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل قائمة طرز القماش";
                return View(new FabricStyleIndexViewModel());
            }
        }

        // GET: FabricStyles/Create
        public IActionResult Create()
        {
            try
            {
                var viewModel = new FabricStyleViewModel();

                _logger.LogInformation("Fabric style create form loaded by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric style create form by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج إضافة طراز القماش";
                return RedirectToAction("Index");
            }
        }

        // GET: FabricStyles/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var fabricStyle = _unitOfWork.Repository<FabricStyle>().GetOne(f => f.Id == id);

                if (fabricStyle == null)
                {
                    TempData["Error"] = "طراز القماش غير موجود";
                    return RedirectToAction("Index");
                }

                var viewModel = new FabricStyleViewModel
                {
                    Id = fabricStyle.Id,
                    Style = fabricStyle.Style,
                    Comment = fabricStyle.Comment
                };

                _logger.LogInformation("Fabric style {Id} edit form loaded by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fabric style {Id} edit form by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                TempData["Error"] = "حدث خطأ أثناء تحميل نموذج تعديل طراز القماش";
                return RedirectToAction("Index");
            }
        }

        // POST: FabricStyles/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(FabricStyleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateEdit", model);
                }

                // Check for duplicate style name (excluding current item if editing)
                var existingStyle = _unitOfWork.Repository<FabricStyle>()
                    .GetAll(f => f.Style == model.Style && f.Id != model.Id)
                    .FirstOrDefault();

                if (existingStyle != null)
                {
                    ModelState.AddModelError("Style", "اسم الطراز موجود بالفعل");
                    return View("CreateEdit", model);
                }

                FabricStyle fabricStyle;
                bool isEdit = model.Id > 0;

                if (isEdit)
                {
                    // Update existing style
                    fabricStyle = _unitOfWork.Repository<FabricStyle>().GetOne(f=>f.Id==model.Id);
                    if (fabricStyle == null)
                    {
                        TempData["Error"] = "طراز القماش غير موجود";
                        return RedirectToAction("Index");
                    }

                    fabricStyle.Style = model.Style.Trim();
                    fabricStyle.Comment = model.Comment?.Trim();
                    

                    _unitOfWork.Repository<FabricStyle>().Update(fabricStyle);
                }
                else
                {
                    // Create new style
                    fabricStyle = new FabricStyle
                    {
                        Style = model.Style.Trim(),
                        Comment = model.Comment?.Trim(),
                        
                    };

                    _unitOfWork.Repository<FabricStyle>().Add(fabricStyle);
                }

                _unitOfWork.Complete();

                _logger.LogInformation("Fabric style {Action} successfully - ID: {Id}, Name: {Name} by {User} at {Time}",
                    isEdit ? "updated" : "created", fabricStyle.Id, fabricStyle.Style, "Ammar-Yasser8", "2025-09-02 19:18:10");

                TempData["Success"] = $"تم {(isEdit ? "تحديث" : "إضافة")} طراز القماش '{fabricStyle.Style}' بنجاح";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving fabric style by {User} at {Time}",
                    "Ammar-Yasser8", "2025-09-02 19:18:10");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ طراز القماش");
                return View("CreateEdit", model);
            }
        }

        // POST: FabricStyles/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var fabricStyle = _unitOfWork.Repository<FabricStyle>().GetOne(f=>f.Id ==id);
                if (fabricStyle == null)
                {
                    return Json(new { success = false, message = "طراز القماش غير موجود" });
                }

                // Check if style is used in colors or designs
                var colorsCount = _unitOfWork.Repository<FabricColor>()
                    .GetAll(c => c.StyleId == id).Count();
                var designsCount = _unitOfWork.Repository<FabricDesign>()
                    .GetAll(d => d.StyleId == id).Count();

                if (colorsCount > 0 || designsCount > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"لا يمكن حذف الطراز لأنه مستخدم في {colorsCount} لون و {designsCount} تصميم"
                    });
                }

                _unitOfWork.Repository<FabricStyle>().Remove(fabricStyle);
                _unitOfWork.Complete();

                _logger.LogInformation("Fabric style {Id} ({Name}) deleted by {User} at {Time}",
                    id, fabricStyle.Style, "Ammar-Yasser8", "2025-09-02 19:18:10");

                return Json(new
                {
                    success = true,
                    message = $"تم حذف طراز القماش '{fabricStyle.Style}' بنجاح"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fabric style {Id} by {User} at {Time}",
                    id, "Ammar-Yasser8", "2025-09-02 19:18:10");
                return Json(new { success = false, message = "حدث خطأ أثناء حذف طراز القماش" });
            }
        }
    }
}
