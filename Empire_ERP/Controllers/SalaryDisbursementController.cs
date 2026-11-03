using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class SalaryDisbursementController : BaseController
    {
        public ISalaryDisbursementService _salaryDisbursementService { get; set; }
        public IPeriodService _periodService { get; set; }
        public ICompanyService _companyService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SalaryDisbursementController(
            IMenuService menuService,
            ISalaryDisbursementService salaryDisbursementService,
            IPeriodService periodService,
            ICompanyService companyService,
            IWebHostEnvironment hostingEnvironment,
            IBaseService baseService) : base(menuService, baseService)
        {
            _salaryDisbursementService = salaryDisbursementService;
            _periodService = periodService;
            _companyService = companyService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            var common = CommonHelper.GetValues(HttpContext);
            ViewBag.Permissions = common.RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(common.RoleID, common.MenuID);
            var response = _menuService.GetMenu(common.MenuID);
            if (response.msgType == 1)
            {
                ViewBag.DATA_CLEAR = ((Menu)response.data).DATA_CLEAR;
            }
            ViewBag.Limit = CommonHelper.GetLimitByMenueID(common.MenuID);
            var periodInfo = _periodService.GetPeriodById(Convert.ToInt32(common.Period));
            ViewBag.StartDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpGet]
        public JsonResult GetBookTypes()
        {
            try
            {
                var data = _salaryDisbursementService.GetChartOfAccounts(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { data = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetPayrollMonths()
        {
            try
            {
                var data = _salaryDisbursementService.GetPayrollMonths();
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { data = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetEmployeesByPayrollMonth(string payrollMonth)
        {
            try
            {
                var data = _salaryDisbursementService.GetEmployeesByPayrollMonth(payrollMonth);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { data = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetSalaryDisbursements()
        {
            try
            {
                var data = _salaryDisbursementService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { data = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult Save(List<SalaryDisbursement> modelRecord)
        {
            try
            {
                var data = _salaryDisbursementService.Save(modelRecord, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { msg = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetSalaryDisbursementByCode(int code)
        {
            try
            {
                var data = _salaryDisbursementService.GetSalaryDisbursementByCode(code, CommonHelper.GetValues(HttpContext));
                var detailData = _salaryDisbursementService.GetSalaryDisbursementDetailsByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(new { Master = data, Detail = detailData });
            }
            catch (Exception ex)
            {
                return Json(new { msg = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpGet]
        public JsonResult GetSalaryDisbursementDetailsByCode(int code)
        {
            try
            {
                var data = _salaryDisbursementService.GetSalaryDisbursementDetailsByCode(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { msg = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult Delete(int code)
        {
            try
            {
                var data = _salaryDisbursementService.Delete(code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { msg = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult DeleteSalaryDisbursementDetailByCode(int tranID, int code)
        {
            try
            {
                var data = _salaryDisbursementService.DeleteSalaryDisbursementDetailByCode(tranID, code, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { msg = GetErrorMessage(ex), msgType = 2 });
            }
        }

        [HttpPost]
        public JsonResult GetPrintReport(int tranId, int dtCode)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                if (tranId <= 0 || dtCode <= 0)
                {
                    response.msg = "Please save the record before printing.";
                    response.msgType = 2;
                    return Json(response);
                }

                var common = CommonHelper.GetValues(HttpContext);
                var payslipResponse = _salaryDisbursementService.GetPayslipData(tranId, dtCode, common);
                if (payslipResponse.msgType != 1 || payslipResponse.data == null)
                {
                    response.msg = String.IsNullOrWhiteSpace(payslipResponse.msg) ? "Unable to load payslip data." : payslipResponse.msg;
                    response.msgType = 2;
                    return Json(response);
                }

                var payslip = (SalaryDisbursementPayslip)payslipResponse.data;
                var companyResponse = _companyService.GetCompanyByCode(common.Company);
                if (companyResponse.msgType == 1 && companyResponse.data != null)
                {
                    var company = (Company)companyResponse.data;
                    payslip.COMPANY_NAME = company.C_NAME;
                    payslip.COMPANY_ADDRESS = company.C_ADDRESS;
                    payslip.COMPANY_PHONE = company.C_TEL;
                    payslip.COMPANY_LOGO = company.C_LOGO;
                }

                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\SalaryDisbursement");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string safeEmp = SanitizeFileName(payslip.EMP_NAME ?? "Employee");
                string safeVoucher = SanitizeFileName(payslip.VOUCHER_NO ?? tranId.ToString());
                string fileName = $"{safeVoucher}_{safeEmp}_{dtCode}.pdf";
                string fullPath = Path.Combine(uploadsFolder, fileName);

                SalaryDisbursementPdfService.GeneratePayslip(payslip, fullPath, _hostingEnvironment.WebRootPath);

                response.data = $"/Client/SalaryDisbursement/{fileName}";
                response.msg = "";
                response.msgType = 1;
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return Json(response);
        }

        private string SanitizeFileName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }
            return value.Replace(" ", "_").Replace("/", "-").Replace("\\", "-");
        }

        private string GetErrorMessage(Exception ex)
        {
            string _catchMessage = ex.Message;
            if (ex.InnerException != null)
            {
                _catchMessage += "<br/>" + ex.InnerException.Message;
            }
            return _catchMessage;
        }
    }
}
