using ELRakhawy.EL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELRakhawy.Web.Controllers
{
    public class RawTransactionsController : Controller
    {
        private readonly ILogger<RawTransactionsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RawTransactionsController(ILogger<RawTransactionsController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        
    }
}
