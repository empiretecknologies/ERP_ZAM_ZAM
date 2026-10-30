using Empire_ERP.Core.Entities;

namespace Empire_ERP.Core.Interfaces
{
    public interface ISalaryGenerateService
    {
        MyHttpResponseMessage Load(SalaryGenerateRequest model, Common common);
        MyHttpResponseMessage Generate(SalaryGenerateRequest model, Common common);
    }
}
