using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IEmployeeAdvanceRepository
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetEmployeeAdvanceById(int id, Common common);
        MyHttpResponseMessage Save(EmployeeAdvance model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
    }
}
