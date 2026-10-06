using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IEmployeeLoanRepository
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetEmployeeLoanById(int id, Common common);
        MyHttpResponseMessage Save(EmployeeLoan model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
    }
}
