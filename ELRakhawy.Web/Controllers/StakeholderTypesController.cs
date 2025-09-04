using ELRakhawy.EL.Interfaces;
using ELRakhawy.EL.Models;
using ELRakhawy.EL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class StakeholderTypesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StakeholderTypesController> _logger;
        public StakeholderTypesController(IUnitOfWork unitOfWork, ILogger<StakeholderTypesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;

        }
        // GET: StakeholderTypes
        public IActionResult Index()
        {
            var types = _unitOfWork.Repository<StakeholderType>()
                .GetAll(includeEntities: "FinancialTransactionType,StakeholderTypeForms,StakeholderTypeForms.Form")
                .ToList();
            return View(types);
        }

        // GET: StakeholderTypes/Create
        public IActionResult Create()
        {
            var viewModel = new StakeholderTypeViewModel
            {
                FinancialTransactionTypes = _unitOfWork.Repository<FinancialTransactionType>().GetAll().ToList(),
                AvailableForms = _unitOfWork.Repository<FormStyle>().GetAll().ToList()
            };
            return View(viewModel);

        }
        // POST: StakeholderTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StakeholderTypeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if stakeholder type with same name exists
                var existingType = _unitOfWork.Repository<StakeholderType>()
                    .GetOne(st => st.Type.ToLower() == viewModel.Type.ToLower());

                if (existingType != null)
                {
                    ModelState.AddModelError("Type", "A stakeholder type with this name already exists.");
                }
                else
                {
                    var stakeholderType = new StakeholderType
                    {
                        Type = viewModel.Type,
                        Comment = viewModel.Comment,
                        FinancialTransactionTypeId = viewModel.FinancialTransactionTypeId
                    };
                    _unitOfWork.Repository<StakeholderType>().Add(stakeholderType);
                    _unitOfWork.Complete();
                    // Add selected forms
                    foreach (var formId in viewModel.SelectedFormIds)
                    {
                        var stakeholderTypeForm = new StakeholderTypeForm
                        {
                            StakeholderTypeId = stakeholderType.Id,
                            FormId = formId
                        };
                        _unitOfWork.Repository<StakeholderTypeForm>().Add(stakeholderTypeForm);
                    }
                    _unitOfWork.Complete();
                    return RedirectToAction(nameof(Index));
                }
            }
            // If model state is invalid or type exists, repopulate the view model
            viewModel.FinancialTransactionTypes = _unitOfWork.Repository<FinancialTransactionType>().GetAll().ToList();
            viewModel.AvailableForms = _unitOfWork.Repository<FormStyle>().GetAll().ToList();
            return View(viewModel);
        }

        // GET: StakeholderTypes/Edit/5
        public IActionResult Edit(int id)
        {
            var stakeholderType = _unitOfWork.Repository<StakeholderType>()
                .GetOne(st => st.Id == id, includeEntities: "FinancialTransactionType,StakeholderTypeForms,StakeholderTypeForms.Form");

            if (stakeholderType == null)
            {
                return NotFound();
            }

            var viewModel = new StakeholderTypeViewModel
            {
                Id = stakeholderType.Id,
                Type = stakeholderType.Type,
                Comment = stakeholderType.Comment,
                FinancialTransactionTypeId = stakeholderType.FinancialTransactionTypeId,
                SelectedFormIds = stakeholderType.StakeholderTypeForms.Select(stf => stf.FormId).ToList(),
                FinancialTransactionTypes = _unitOfWork.Repository<FinancialTransactionType>().GetAll().ToList(),
                AvailableForms = _unitOfWork.Repository<FormStyle>().GetAll().ToList()
            };

            return View(viewModel);
        }

        // POST: StakeholderTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StakeholderTypeViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var stakeholderType = _unitOfWork.Repository<StakeholderType>()
                    .GetOne(st => st.Id == id, includeEntities: "StakeholderTypeForms");

                if (stakeholderType == null)
                {
                    return NotFound();
                }

                stakeholderType.Type = viewModel.Type;
                stakeholderType.Comment = viewModel.Comment;
                stakeholderType.FinancialTransactionTypeId = viewModel.FinancialTransactionTypeId;

                _unitOfWork.Repository<StakeholderType>().Update(stakeholderType);

                // Remove existing form associations
                var existingForms = _unitOfWork.Repository<StakeholderTypeForm>()
                    .GetAll(stf => stf.StakeholderTypeId == id);
                _unitOfWork.Repository<StakeholderTypeForm>().RemoveRange(existingForms);

                // Add new form associations
                foreach (var formId in viewModel.SelectedFormIds)
                {
                    var stakeholderTypeForm = new StakeholderTypeForm
                    {
                        StakeholderTypeId = stakeholderType.Id,
                        FormId = formId
                    };
                    _unitOfWork.Repository<StakeholderTypeForm>().Add(stakeholderTypeForm);
                }

                _unitOfWork.Complete();
                return RedirectToAction(nameof(Index));
            }

            // If model state is invalid, repopulate the view model
            viewModel.FinancialTransactionTypes = _unitOfWork.Repository<FinancialTransactionType>().GetAll().ToList();
            viewModel.AvailableForms = _unitOfWork.Repository<FormStyle>().GetAll().ToList();
            return View(viewModel);
        }

        // GET: StakeholderTypes/Delete/5
        public IActionResult Delete(int id)
        {
            var stakeholderType = _unitOfWork.Repository<StakeholderType>()
                .GetOne(st => st.Id == id, "FinancialTransactionType,StakeholderTypeForms,StakeholderTypeForms.Form");

            if (stakeholderType == null)
            {
                return NotFound();
            }

            return View(stakeholderType);
        }

        // POST: StakeholderTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var stakeholderType = _unitOfWork.Repository<StakeholderType>()
                .GetOne(st => st.Id == id, "StakeholderTypeForms");

            if (stakeholderType == null)
            {
                return NotFound();
            }

            // Remove associated forms first
            var associatedForms = _unitOfWork.Repository<StakeholderTypeForm>()
                .GetAll(stf => stf.StakeholderTypeId == id);
            _unitOfWork.Repository<StakeholderTypeForm>().RemoveRange(associatedForms);

            _unitOfWork.Repository<StakeholderType>().Remove(stakeholderType);
            _unitOfWork.Complete();

            return RedirectToAction(nameof(Index));
        }
    }
}
