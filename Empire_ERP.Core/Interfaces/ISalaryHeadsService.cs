using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalaryHeadsService
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetSalaryHeadsById(int id, Common common);
        MyHttpResponseMessage Save(SalaryHeads model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
    }
}
