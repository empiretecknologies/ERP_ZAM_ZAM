using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IEmployeeSalaryRepository
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetEmployeeSalaryByEmployeeId(int employeeId, Common common);
        MyHttpResponseMessage Save(CustomEmployeeSalary model, Common common);
    }
}
