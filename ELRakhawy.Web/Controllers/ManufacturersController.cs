using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    [Route("[controller]")]
    public class ManufacturersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ManufacturersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAllManufacturers()
        {
            var manufacturers = _unitOfWork.Repository<Manufacturers>().GetAll();
            return View(manufacturers);
        }

        [HttpPost("Create")]
        public IActionResult Create(Manufacturers manufacturer)
        {
            // Debugging
            System.Diagnostics.Debug.WriteLine($"Create method called with: {manufacturer.Name}");

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Check if manufacturer name already exists (case insensitive)
                    var existing = _unitOfWork.Repository<Manufacturers>()
                        .GetAll()
                        .FirstOrDefault(m => m.Name.ToLower() == manufacturer.Name.ToLower());

                    if (existing != null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"اسم المصنع '{manufacturer.Name}' موجود بالفعل"
                        });
                    }

                    // If not exist → Save
                    _unitOfWork.Repository<Manufacturers>().Add(manufacturer);
                    _unitOfWork.Complete();

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            id = manufacturer.Id,
                            name = manufacturer.Name,
                            description = manufacturer.Description
                        },
                        message = "تمت إضافة المصنع بنجاح"
                    });
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in Create: {ex.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء إضافة المصنع: {ex.Message}" });
                }
            }

            // Log model state errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Model error: {error.ErrorMessage}");
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء إضافة المصنع. يرجى التحقق من البيانات المدخلة." });
        }

        [HttpPost("Edit")]
        public IActionResult Edit(Manufacturers manufacturer)
        {
            // Add debugging
            System.Diagnostics.Debug.WriteLine($"Edit method called with ID: {manufacturer.Id}, Name: {manufacturer.Name}");

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Repository<Manufacturers>().Update(manufacturer);
                    _unitOfWork.Complete();

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            id = manufacturer.Id,
                            name = manufacturer.Name,
                            description = manufacturer.Description
                        },
                        message = "تم تحديث المصنع بنجاح"
                    });
                }
                catch (System.Exception ex)
                {
                    // Log the exception
                    System.Diagnostics.Debug.WriteLine($"Exception in Edit: {ex.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء تحديث المصنع: {ex.Message}" });
                }
            }

            // Log model state errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Model error: {error.ErrorMessage}");
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء تحديث المصنع. يرجى التحقق من البيانات المدخلة." });
        }

        [HttpPost("Delete")]
        public IActionResult Delete(int id)
        {
            // Add debugging
            System.Diagnostics.Debug.WriteLine($"Delete method called with ID: {id}");

            try
            {
                var manufacturer = _unitOfWork.Repository<Manufacturers>().GetOne(m => m.Id == id);
                if (manufacturer == null)
                {
                    return Json(new { success = false, message = "المصنع غير موجود" });
                }

                _unitOfWork.Repository<Manufacturers>().Remove(manufacturer);
                _unitOfWork.Complete();
                return Json(new { success = true, message = "تم حذف المصنع بنجاح" });
            }
            catch (System.Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Exception in Delete: {ex.Message}");
                return Json(new { success = false, message = $"حدث خطأ أثناء حذف المصنع: {ex.Message}" });
            }
        }


    }
}
