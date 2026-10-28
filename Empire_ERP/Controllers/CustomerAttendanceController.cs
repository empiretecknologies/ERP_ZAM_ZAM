using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class CustomerAttendanceController : BaseController
	{
		public ICustomerAttendanceService _customerAttendanceService { get; set; }
        public IPeriodService _periodService { get; set; }
        public CustomerAttendanceController(ICustomerAttendanceService CustomerAttendanceService, IMenuService menuService, IPeriodService periodService,IBaseService baseService) : base(menuService,baseService)
		{
			_customerAttendanceService = CustomerAttendanceService;
			_periodService = periodService;
		}

		public IActionResult Index()
		{
            var common = CommonHelper.GetValues(HttpContext);
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(common.RoleID, common.MenuID);
            var periodInfo = _periodService.GetPeriodById(Convert.ToInt32(common.Period));
            ViewBag.StartDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");
            return View();
		}

		[HttpGet]
		public JsonResult SyncAndGetData(string fromDate, string toDate)
		{
			try
			{
				var data = _customerAttendanceService.SyncAndGetData(CommonHelper.GetValues(HttpContext));
				var gridData = _customerAttendanceService.GetDataAttendance(fromDate, toDate, CommonHelper.GetValues(HttpContext));
                return Json(gridData);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}

        [HttpGet]
        public JsonResult GetDataAttendance(string fromDate, string toDate)
        {
            try
            {
                var data = _customerAttendanceService.GetDataAttendance(fromDate, toDate, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
        public JsonResult GetCustomerAttendanceDetails(int code)
        {
            try
            {
                var data = _customerAttendanceService.GetCustomerAttendanceDetails(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                return Json(_catchMessage);
            }
        }

        [HttpGet]
		public JsonResult GetCurrencies()
		{
			try
			{
				var data = DropdownService.CurrencyDropdownWithControlName();
				return Json(data);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}

		[HttpGet]
		public JsonResult GetCustomerAttendanceDetailByCode(int code)
		{
			try
			{
				var data = _customerAttendanceService.GetCustomerAttendanceDetailByCode(code, CommonHelper.GetValues(HttpContext));
				return Json(data);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}

		[HttpPost]
		public JsonResult Save(CustomerAttendance CustomerAttendance)
		{
			try
			{
				var data = _customerAttendanceService.Save(CustomerAttendance, CommonHelper.GetValues(HttpContext));
				return Json(data);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}

		[HttpGet]
		public JsonResult GetCustomerAttendanceByCode(int code)
		{
			try
			{
				var data = _customerAttendanceService.GetCustomerAttendanceByCode(code, CommonHelper.GetValues(HttpContext));
				return Json(data);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}

		[HttpPost]
		public JsonResult Delete(int code)
		{
			try
			{
				var data = _customerAttendanceService.Delete(code, CommonHelper.GetValues(HttpContext));
				return Json(data);
			}
			catch (Exception ex)
			{
				string _catchMessage = ex.Message;
				if (ex.InnerException != null)
				{
					_catchMessage += "<br/>" + ex.InnerException.Message;
				}
				return Json(_catchMessage);
			}
		}
	}
}