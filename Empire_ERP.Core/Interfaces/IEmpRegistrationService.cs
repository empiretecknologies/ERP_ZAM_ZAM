using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IEmpRegistrationService
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetEmpRegistrationById(int id, Common common);
        MyHttpResponseMessage Save(EmpRegistration model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
    }
}
