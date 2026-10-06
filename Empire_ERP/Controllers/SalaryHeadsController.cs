using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class SalaryHeadsController : BaseController
    {
        public ISalaryHeadsService _salaryHeadsService { get; set; }
        public SalaryHeadsController(ISalaryHeadsService salaryHeadsService, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _salaryHeadsService = salaryHeadsService;
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
                var data = _salaryHeadsService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetSalaryHeadsById(int id)
        {
            var data = _salaryHeadsService.GetSalaryHeadsById(id, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpPost]
        public JsonResult Save(SalaryHeads model)
        {
            try
            {
                var data = _salaryHeadsService.Save(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var data = _salaryHeadsService.Delete(id, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetChartAccounts()
        {
            try
            {
                var data = DropdownService.SalaryHeadsChartDropdown();
                return Json(new { data = data, msgType = 1 });
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(new { data = _catchMessage, msgType = 2 });
            }
        }
    }
}
