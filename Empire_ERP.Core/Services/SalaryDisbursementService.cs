using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class SalaryDisbursementService : ISalaryDisbursementService
    {
        public ISalaryDisbursementRepository _salaryDisbursementRepository { get; set; }
        public IMenuService _menuService { get; set; }
        public SalaryDisbursementService(ISalaryDisbursementRepository salaryDisbursementRepository, IMenuService menuService)
        {
            _salaryDisbursementRepository = salaryDisbursementRepository;
            _menuService = menuService;
        }

        public MyHttpResponseMessage GetChartOfAccounts(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                int? pType = 0;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    pType = menu.PTYPE;
                }

                return _salaryDisbursementRepository.GetChartOfAccounts(pType, common);
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    return _salaryDisbursementRepository.QuickSearch(Menu, common);
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage Save(List<SalaryDisbursement> model, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    response = _salaryDisbursementRepository.Save(model, common, menu);
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetSalaryDisbursementByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    response = _salaryDisbursementRepository.GetSalaryDisbursementByCode(code, common, menu);
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetSalaryDisbursementDetailsByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    response = _salaryDisbursementRepository.GetSalaryDisbursementDetailsByCode(code, common, menu);
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage Delete(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    if (code == 0)
                    {
                        response.msg = "ID is not in numeric format";
                        response.msgType = 2;
                    }
                    else
                    {
                        response = _salaryDisbursementRepository.Delete(code, common, menu);
                    }
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage DeleteSalaryDisbursementDetailByCode(int tranID, int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                Menu menu = new Menu();
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }
                if (!String.IsNullOrWhiteSpace(table))
                {
                    if (code == 0)
                    {
                        response.msg = "ID is not in numeric format";
                        response.msgType = 2;
                    }
                    else
                    {
                        response = _salaryDisbursementRepository.DeleteSalaryDisbursementDetailByCode(tranID, code, common, menu);
                    }
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetPayrollMonths()
        {
            return _salaryDisbursementRepository.GetPayrollMonths();
        }

        public MyHttpResponseMessage GetEmployeesByPayrollMonth(string payrollMonth)
        {
            return _salaryDisbursementRepository.GetEmployeesByPayrollMonth(payrollMonth);
        }

        public MyHttpResponseMessage GetPayslipData(int tranId, int dtCode, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuService.GetMenu(common.MenuID);
                string? table = string.Empty;
                Menu menu = new Menu();
                if (Menu.data != null)
                {
                    menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    response = _salaryDisbursementRepository.GetPayslipData(tranId, dtCode, common, menu);
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
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
