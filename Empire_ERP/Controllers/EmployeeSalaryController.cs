using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class EmployeeSalaryController : BaseController
    {
        public IEmployeeSalaryService _employeeSalaryService { get; set; }
        public EmployeeSalaryController(IEmployeeSalaryService employeeSalaryService, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _employeeSalaryService = employeeSalaryService;
        }

        public IActionResult Index()
        {
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).MenuID);
            return View();
        }

        [HttpGet]
        public JsonResult QuickSearch()
        {
            try
            {
                var data = _employeeSalaryService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetEmployeeSalaryByEmployeeId(int employeeId)
        {
            var data = _employeeSalaryService.GetEmployeeSalaryByEmployeeId(employeeId, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpPost]
        public JsonResult Save(CustomEmployeeSalary model)
        {
            try
            {
                var data = _employeeSalaryService.Save(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
