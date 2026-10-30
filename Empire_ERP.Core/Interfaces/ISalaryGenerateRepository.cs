using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalaryGenerateRepository
    {
        MyHttpResponseMessage Load(SalaryGenerateRequest model, Common common);
        MyHttpResponseMessage Generate(SalaryGenerateRequest model, Common common);
    }
}
