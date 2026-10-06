using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class EmpRegistrationService : IEmpRegistrationService
    {
        public IEmpRegistrationRepository _empRegistrationRepository { get; set; }
        public EmpRegistrationService(IEmpRegistrationRepository empRegistrationRepository)
        {
            _empRegistrationRepository = empRegistrationRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _empRegistrationRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(EmpRegistration model, Common common)
        {
            return _empRegistrationRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _empRegistrationRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetEmpRegistrationById(int id, Common common)
        {
            return _empRegistrationRepository.GetEmpRegistrationById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _empRegistrationRepository.Delete(id, common);
        }
    }
}
