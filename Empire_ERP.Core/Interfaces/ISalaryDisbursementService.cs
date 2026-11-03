using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalaryDisbursementService
    {
        MyHttpResponseMessage GetChartOfAccounts(Common common);
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage Save(List<SalaryDisbursement> model, Common common);
        MyHttpResponseMessage GetSalaryDisbursementByCode(int code, Common common);
        MyHttpResponseMessage GetSalaryDisbursementDetailsByCode(int code, Common common);
        MyHttpResponseMessage Delete(int code, Common common);
        MyHttpResponseMessage DeleteSalaryDisbursementDetailByCode(int tranID, int code, Common common);
        MyHttpResponseMessage GetPayrollMonths();
        MyHttpResponseMessage GetEmployeesByPayrollMonth(string payrollMonth);
        MyHttpResponseMessage GetPayslipData(int tranId, int dtCode, Common common);
    }
}
