using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class SalaryGenerateService : ISalaryGenerateService
    {
        public ISalaryGenerateRepository _salaryGenerateRepository { get; set; }
        public SalaryGenerateService(ISalaryGenerateRepository salaryGenerateRepository)
        {
            _salaryGenerateRepository = salaryGenerateRepository;
        }

        public MyHttpResponseMessage Load(SalaryGenerateRequest model, Common common)
        {
            return _salaryGenerateRepository.Load(model, common);
        }

        public MyHttpResponseMessage Generate(SalaryGenerateRequest model, Common common)
        {
            return _salaryGenerateRepository.Generate(model, common);
        }
    }
}
