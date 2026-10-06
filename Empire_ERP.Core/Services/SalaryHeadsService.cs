using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class SalaryHeadsService : ISalaryHeadsService
    {
        public ISalaryHeadsRepository _salaryHeadsRepository { get; set; }
        public SalaryHeadsService(ISalaryHeadsRepository salaryHeadsRepository)
        {
            _salaryHeadsRepository = salaryHeadsRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _salaryHeadsRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(SalaryHeads model, Common common)
        {
            return _salaryHeadsRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _salaryHeadsRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetSalaryHeadsById(int id, Common common)
        {
            return _salaryHeadsRepository.GetSalaryHeadsById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _salaryHeadsRepository.Delete(id, common);
        }
    }
}
