using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class SalaryGenerateController : BaseController
    {
        public ISalaryGenerateService _salaryGenerateService { get; set; }
        public SalaryGenerateController(ISalaryGenerateService salaryGenerateService, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _salaryGenerateService = salaryGenerateService;
        }

        public IActionResult Index()
        {
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).MenuID);
            return View();
        }

        [HttpGet]
        public JsonResult Load(DateTime FROM_DATE, DateTime TO_DATE)
        {
            try
            {
                var model = new SalaryGenerateRequest
                {
                    FROM_DATE = FROM_DATE,
                    TO_DATE = TO_DATE
                };
                var data = _salaryGenerateService.Load(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Generate(SalaryGenerateRequest model)
        {
            try
            {
                var data = _salaryGenerateService.Generate(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
