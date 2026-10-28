using Empire_ERP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire_ERP.Core.Interfaces
{
    public interface ICustomerAttendanceRepository
	{
		MyHttpResponseMessage SyncAndGetData(Common common);
		MyHttpResponseMessage GetDataAttendance(string fromDate, string toDate, Common common);
        MyHttpResponseMessage GetCustomerAttendanceDetails(int code, Common common);
        MyHttpResponseMessage GetCustomerAttendanceDetailByCode(int code, Common common);
		MyHttpResponseMessage Save(CustomerAttendance CustomerAttendance, Common common);
		MyHttpResponseMessage GetCustomerAttendanceByCode(int code, Common common);
		MyHttpResponseMessage Delete(int code, Common common);
	}
}