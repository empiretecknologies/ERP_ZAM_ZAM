using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class EmpManualAttService : IEmpManualAttService
    {
        public IEmpManualAttRepository _empManualAttRepository { get; set; }
        public EmpManualAttService(IEmpManualAttRepository empManualAttRepository)
        {
            _empManualAttRepository = empManualAttRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _empManualAttRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(EmpManualAtt model, Common common)
        {
            return _empManualAttRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _empManualAttRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetEmpManualAttById(int id, Common common)
        {
            return _empManualAttRepository.GetEmpManualAttById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _empManualAttRepository.Delete(id, common);
        }
    }
}
