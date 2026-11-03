using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalaryDisbursementRepository
    {
        MyHttpResponseMessage GetChartOfAccounts(int? pType, Common common);
        MyHttpResponseMessage QuickSearch(MyHttpResponseMessage Menu, Common common);
        MyHttpResponseMessage Save(List<SalaryDisbursement> model, Common common, Menu menu);
        MyHttpResponseMessage GetSalaryDisbursementByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage GetSalaryDisbursementDetailsByCode(int code, Common common, Menu menu);
        MyHttpResponseMessage Delete(int code, Common common, Menu menu);
        MyHttpResponseMessage DeleteSalaryDisbursementDetailByCode(int tranID, int code, Common common, Menu menu);
        MyHttpResponseMessage GetPayrollMonths();
        MyHttpResponseMessage GetEmployeesByPayrollMonth(string payrollMonth);
        MyHttpResponseMessage GetPayslipData(int tranId, int dtCode, Common common, Menu menu);
    }
}
