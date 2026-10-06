using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class EmployeeAdvanceService : IEmployeeAdvanceService
    {
        public IEmployeeAdvanceRepository _employeeAdvanceRepository { get; set; }
        public ICashReceiptVoucherRepository _cashReceiptVoucherRepository { get; set; }
        public EmployeeAdvanceService(IEmployeeAdvanceRepository employeeAdvanceRepository, ICashReceiptVoucherRepository cashReceiptVoucherRepository)
        {
            _employeeAdvanceRepository = employeeAdvanceRepository;
            _cashReceiptVoucherRepository = cashReceiptVoucherRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _employeeAdvanceRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(EmployeeAdvance model, Common common)
        {
            return _employeeAdvanceRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _employeeAdvanceRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetEmployeeAdvanceById(int id, Common common)
        {
            return _employeeAdvanceRepository.GetEmployeeAdvanceById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _employeeAdvanceRepository.Delete(id, common);
        }

        public MyHttpResponseMessage GetBookTypes(Common common)
        {
            return _cashReceiptVoucherRepository.GetChartOfAccounts(null, common);
        }
    }
}
