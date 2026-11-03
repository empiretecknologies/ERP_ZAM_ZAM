using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface IEmpManualAttRepository
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetEmpManualAttById(int id, Common common);
        MyHttpResponseMessage Save(EmpManualAtt model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
    }
}
