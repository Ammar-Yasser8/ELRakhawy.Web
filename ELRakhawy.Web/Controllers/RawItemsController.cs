using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class RawItemsController : Controller
    {
        private readonly ILogger<RawItemsController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public RawItemsController(ILogger<RawItemsController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        // GET: RawItems
        public IActionResult Index()
        {
            var items = _unitOfWork.Repository<RawItem>().GetAll(includeEntities: "Warp,Weft");
            if (items is null || !items.Any())
            {
                _logger.LogInformation("No FullWarpBeams found.");
            }   
            return View(items);
        }

        // GET: RawItems/Create
        public IActionResult Create()
        {
            ViewBag.Warps = _unitOfWork.Repository<FullWarpBeam>().GetAll(f => f.Status == true).ToList();
            ViewBag.Wefts = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RawItem rawItem)
        {
            ModelState.Remove("Warp");
            ModelState.Remove("Weft");
            // Check uniqueness
            if (_unitOfWork.Repository<RawItem>().GetAll().Any(r => r.Item == rawItem.Item))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Repository<RawItem>().Add(rawItem);
                _unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate the dropdowns if validation fails
            ViewBag.Warps = _unitOfWork.Repository<FullWarpBeam>().GetAll(f => f.Status == true).ToList();
            ViewBag.Wefts = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();

            return View(rawItem);
        }

        // GET: RawItems/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }
            var rawItem = _unitOfWork.Repository<RawItem>().GetOne(r => r.Id == id,includeEntities: "Warp,Weft");
            if (rawItem == null)
            {
                return NotFound();
            }
            ViewBag.Warps = _unitOfWork.Repository<FullWarpBeam>().GetAll(f => f.Status == true).ToList();
            ViewBag.Wefts = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();
            return View(rawItem);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, RawItem rawItem)
        {
            if (id != rawItem.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Warp");
            ModelState.Remove("Weft");
            // Check uniqueness
            if (_unitOfWork.Repository<RawItem>().GetAll().Any(r => r.Item == rawItem.Item && r.Id != rawItem.Id))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingRawItem = _unitOfWork.Repository<RawItem>().GetOne(r => r.Id == id,includeEntities: "Warp,Weft");

                    if (existingRawItem == null)
                    {
                        return NotFound();
                    }

                    // Update only the properties that changed
                    existingRawItem.Item = rawItem.Item;
                    existingRawItem.WarpId = rawItem.WarpId;
                    existingRawItem.WeftId = rawItem.WeftId;
                    existingRawItem.Status = rawItem.Status;
                    _unitOfWork.Repository<RawItem>().Update(existingRawItem);
                    _unitOfWork.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating RawItem with ID {RawItemId}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the item. Please try again.");
                    ViewBag.Warps = _unitOfWork.Repository<FullWarpBeam>().GetAll(f => f.Status == true).ToList();
                    ViewBag.Wefts = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();
                    return View(rawItem);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Warps = _unitOfWork.Repository<FullWarpBeam>().GetAll(f => f.Status == true).ToList();
            ViewBag.Wefts = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();
            return View(rawItem);
        }
        // GET: RawItems/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var rawItem = _unitOfWork.Repository<RawItem>().GetOne(
                r => r.Id == id,
                includeEntities: "Warp,Weft"
            );

            if (rawItem == null)
            {
                return NotFound();
            }

            return View(rawItem);
        }

        // POST: RawItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var rawItem = _unitOfWork.Repository<RawItem>().GetOne(r => r.Id == id);

                if (rawItem == null)
                {
                    return NotFound();
                }

                _unitOfWork.Repository<RawItem>().Remove(rawItem);
                _unitOfWork.Complete();

                TempData["SuccessMessage"] = "Raw item deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RawItem with ID {RawItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the item. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

    }
}
