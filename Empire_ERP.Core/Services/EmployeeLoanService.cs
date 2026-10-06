using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class EmployeeLoanService : IEmployeeLoanService
    {
        public IEmployeeLoanRepository _employeeLoanRepository { get; set; }
        public EmployeeLoanService(IEmployeeLoanRepository employeeLoanRepository)
        {
            _employeeLoanRepository = employeeLoanRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _employeeLoanRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(EmployeeLoan model, Common common)
        {
            model.INSTALLMENT_AMOUNT = model.LOAN_AMOUNT != null && model.TOTAL_INSTALLMENTS != null && model.TOTAL_INSTALLMENTS > 0
                ? Math.Round(model.LOAN_AMOUNT.Value / model.TOTAL_INSTALLMENTS.Value, 2)
                : null;
            return _employeeLoanRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _employeeLoanRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetEmployeeLoanById(int id, Common common)
        {
            return _employeeLoanRepository.GetEmployeeLoanById(id, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _employeeLoanRepository.Delete(id, common);
        }
    }
}
