using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class EmployeeSalaryService : IEmployeeSalaryService
    {
        public IEmployeeSalaryRepository _employeeSalaryRepository { get; set; }
        public EmployeeSalaryService(IEmployeeSalaryRepository employeeSalaryRepository)
        {
            _employeeSalaryRepository = employeeSalaryRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _employeeSalaryRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage GetEmployeeSalaryByEmployeeId(int employeeId, Common common)
        {
            return _employeeSalaryRepository.GetEmployeeSalaryByEmployeeId(employeeId, common);
        }

        public MyHttpResponseMessage Save(CustomEmployeeSalary model, Common common)
        {
            if (model.Detail != null)
            {
                foreach (var item in model.Detail)
                {
                    item.GROSS_SALARY = (item.BASIC_SALARY ?? 0) + (item.HOUSE_ALLOWANCE ?? 0) + (item.MEDICAL_ALLOWANCE ?? 0)
                                      + (item.CONVEYANCE_ALLOWANCE ?? 0) + (item.OVERTIME ?? 0) + (item.BONUS ?? 0);
                    item.TOTAL_DEDUCTION = (item.INCOME_TAX ?? 0) + (item.LOAN_DEDUCTION ?? 0) + (item.ADVANCE_DEDUCTION ?? 0);
                    item.NET_SALARY = item.GROSS_SALARY - item.TOTAL_DEDUCTION;
                }
            }
            return _employeeSalaryRepository.Save(model, common);
        }
    }
}
