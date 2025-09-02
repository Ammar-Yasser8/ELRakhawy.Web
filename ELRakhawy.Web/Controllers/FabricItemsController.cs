using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class FabricItemsController : Controller
    {
        private readonly ILogger<FabricItemsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public FabricItemsController(ILogger<FabricItemsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // GET: FabricItems
        public IActionResult Index()
        {
            var items = _unitOfWork.Repository<FabricItem>().GetAll(includeEntities: "OriginRaw");
            if (items is null || !items.Any())
            {
                _logger.LogInformation("No FabricItems found.");
            }
            return View(items);
        }

        // GET: FabricItems/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(
                f => f.Id == id,
                includeEntities: "OriginRaw"
            );

            if (fabricItem == null)
            {
                return NotFound();
            }

            return View(fabricItem);
        }

        // GET: FabricItems/Create
        public IActionResult Create()
        {
            ViewBag.RawItems = _unitOfWork.Repository<RawItem>().GetAll(r => r.Status == true).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FabricItem fabricItem)
        {
            ModelState.Remove("OriginRaw");
            
            // Check uniqueness
            if (_unitOfWork.Repository<FabricItem>().GetAll().Any(f => f.Item == fabricItem.Item))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Repository<FabricItem>().Add(fabricItem);
                    _unitOfWork.Complete();
                    
                    _logger.LogInformation("FabricItem '{Item}' created successfully", fabricItem.Item);
                    TempData["SuccessMessage"] = "تم إضافة صنف القماش بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating FabricItem '{Item}'", fabricItem.Item);
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إضافة الصنف. يرجى المحاولة مرة أخرى.");
                }
            }

            // Repopulate the dropdown if validation fails
            ViewBag.RawItems = _unitOfWork.Repository<RawItem>().GetAll(r => r.Status == true).ToList();
            return View(fabricItem);
        }

        // GET: FabricItems/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(
                f => f.Id == id,
                includeEntities: "OriginRaw"
            );

            if (fabricItem == null)
            {
                return NotFound();
            }

            ViewBag.RawItems = _unitOfWork.Repository<RawItem>().GetAll(r => r.Status == true).ToList();
            return View(fabricItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FabricItem fabricItem)
        {
            if (id != fabricItem.Id)
            {
                return NotFound();
            }

            ModelState.Remove("OriginRaw");
            
            // Check uniqueness
            if (_unitOfWork.Repository<FabricItem>().GetAll().Any(f => f.Item == fabricItem.Item && f.Id != fabricItem.Id))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingFabricItem = _unitOfWork.Repository<FabricItem>().GetOne(
                        f => f.Id == id,
                        includeEntities: "OriginRaw"
                    );

                    if (existingFabricItem == null)
                    {
                        return NotFound();
                    }

                    // Update only the properties that changed
                    existingFabricItem.Item = fabricItem.Item;
                    existingFabricItem.OriginRawId = fabricItem.OriginRawId;
                    existingFabricItem.Status = fabricItem.Status;
                    existingFabricItem.Comment = fabricItem.Comment;

                    _unitOfWork.Repository<FabricItem>().Update(existingFabricItem);
                    _unitOfWork.Complete();

                    _logger.LogInformation("FabricItem '{Item}' with ID {Id} updated successfully", fabricItem.Item, id);
                    TempData["SuccessMessage"] = "تم تحديث صنف القماش بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating FabricItem with ID {FabricItemId}", id);
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء تحديث الصنف. يرجى المحاولة مرة أخرى.");
                }
            }

            ViewBag.RawItems = _unitOfWork.Repository<RawItem>().GetAll(r => r.Status == true).ToList();
            return View(fabricItem);
        }

        // GET: FabricItems/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(
                f => f.Id == id,
                includeEntities: "OriginRaw"
            );

            if (fabricItem == null)
            {
                return NotFound();
            }

            return View(fabricItem);
        }

        // POST: FabricItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var fabricItem = _unitOfWork.Repository<FabricItem>().GetOne(f => f.Id == id);

                if (fabricItem == null)
                {
                    return NotFound();
                }

                _unitOfWork.Repository<FabricItem>().Remove(fabricItem);
                _unitOfWork.Complete();

                _logger.LogInformation("FabricItem with ID {Id} deleted successfully", id);
                TempData["SuccessMessage"] = "تم حذف صنف القماش بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FabricItem with ID {FabricItemId}", id);
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الصنف. يرجى المحاولة مرة أخرى.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}