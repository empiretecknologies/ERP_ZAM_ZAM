using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Services
{
    public class CustomerAttendanceService : ICustomerAttendanceService
	{
        public ICustomerAttendanceRepository _customerAttendanceRepository { get; set; }
        public CustomerAttendanceService(ICustomerAttendanceRepository CustomerAttendanceRepository)
        {
			_customerAttendanceRepository = CustomerAttendanceRepository;
        }

        public MyHttpResponseMessage SyncAndGetData(Common common)
        {
            return _customerAttendanceRepository.SyncAndGetData(common);
        }

        public MyHttpResponseMessage GetDataAttendance(string fromDate, string toDate, Common common)
        {
            return _customerAttendanceRepository.GetDataAttendance(fromDate, toDate, common);
        }

        public MyHttpResponseMessage GetCustomerAttendanceDetails(int code, Common common)
        {
            return _customerAttendanceRepository.GetCustomerAttendanceDetails(code, common);
        }

        public MyHttpResponseMessage GetCustomerAttendanceDetailByCode(int code, Common common)
		{
			return _customerAttendanceRepository.GetCustomerAttendanceDetailByCode(code, common);
		}

		public MyHttpResponseMessage Save(CustomerAttendance CustomerAttendance, Common common)
        {
            return _customerAttendanceRepository.Save(CustomerAttendance, common);
        }

        public MyHttpResponseMessage GetCustomerAttendanceByCode(int code, Common common)
        {
            return _customerAttendanceRepository.GetCustomerAttendanceByCode(code, common);
		}

		public MyHttpResponseMessage Delete(int code, Common common)
		{
			return _customerAttendanceRepository.Delete(code, common);
		}
	}
}