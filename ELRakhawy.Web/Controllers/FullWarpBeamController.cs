using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class FullWarpBeamController : Controller
    {
        private readonly ILogger<FullWarpBeamController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public FullWarpBeamController(ILogger<FullWarpBeamController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var beams = _unitOfWork.Repository<FullWarpBeam>().GetAll(includeEntities: "OriginYarn") ?? new List<FullWarpBeam>();
            if (!beams.Any())
            {
                _logger.LogInformation("No FullWarpBeams found.");
            }
            return View(beams);
        }

        // create Get
        public IActionResult Create()
        {
            var yarnItems = _unitOfWork.Repository<YarnItem>().GetAll(y => y.Status == true).ToList();
            ViewBag.OriginYarns = yarnItems;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FullWarpBeam beam)
        {
            // Check uniqueness
            if (_unitOfWork.Repository<FullWarpBeam>().GetAll().Any(f => f.Item == beam.Item))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Repository<FullWarpBeam>().Add(beam);
                _unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OriginYarns = _unitOfWork.Repository<YarnItem>().GetAll().Where(y => y.Status).ToList();
            return View(beam);
        }

        // Edit Get
        public IActionResult Edit(int id)
        {
            // Load the FullWarpBeam with the specified ID including the related OriginYarn
            var beam = _unitOfWork.Repository<FullWarpBeam>().GetOne(
                f => f.Id == id,
                includeEntities: "OriginYarn"
            );

            if (beam == null)
            {
                return NotFound();
            }

            // Load active YarnItems for the dropdown
            var yarnItems = _unitOfWork.Repository<YarnItem>()
                .GetAll(y => y.Status == true)
                .ToList();

            ViewBag.OriginYarns = yarnItems;
            return View(beam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FullWarpBeam beam)
        {
            if (id != beam.Id)
            {
                return BadRequest();
            }

            // Check uniqueness
            if (_unitOfWork.Repository<FullWarpBeam>().GetAll()
                .Any(f => f.Item == beam.Item && f.Id != beam.Id))
            {
                ModelState.AddModelError("Item", "اسم الصنف موجود بالفعل");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Detach any tracked entity with the same key before updating
                    var existingBeam = _unitOfWork.Repository<FullWarpBeam>().GetOne(f => f.Id == id);
                    if (existingBeam != null)
                    {
                        // Manually map updated fields to the tracked entity
                        existingBeam.Item = beam.Item;
                        existingBeam.OriginYarnId = beam.OriginYarnId;
                        existingBeam.Status = beam.Status;
                        existingBeam.Comment = beam.Comment;

                        _unitOfWork.Repository<FullWarpBeam>().Update(existingBeam);
                        _unitOfWork.Complete();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating FullWarpBeam with ID {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the record.");
                }
            }

            // Reload the dropdown data if validation fails
            ViewBag.OriginYarns = _unitOfWork.Repository<YarnItem>()
                .GetAll(y => y.Status == true)
                .ToList();

            return View(beam);
        }
       
        // Delete Get
        public IActionResult Delete(int id)
        {
            var beam = _unitOfWork.Repository<FullWarpBeam>().GetOne(f => f.Id == id,includeEntities: "OriginYarn");
            if (beam == null)
            {
                return NotFound();
            }
            return View(beam);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var beam = _unitOfWork.Repository<FullWarpBeam>().GetOne(f => f.Id == id);
            if (beam == null)
            {
                return NotFound();
            }
            try
            {
                _unitOfWork.Repository<FullWarpBeam>().Remove(beam);
                _unitOfWork.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FullWarpBeam with ID {Id}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the record.");
                return View(beam);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
