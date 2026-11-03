using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SalaryGenerateRepository : ISalaryGenerateRepository
    {
        private const string TableName = "TBL_SALARY_GENERATION";

        public IMenuRepository _menuRepository { get; set; }
        public SalaryGenerateRepository(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public MyHttpResponseMessage Load(SalaryGenerateRequest model, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                if (model.FROM_DATE == null || model.TO_DATE == null)
                {
                    response.msg = "Please select From Date and To Date.";
                    response.msgType = 2;
                    return response;
                }

                if (model.FROM_DATE > model.TO_DATE)
                {
                    response.msg = "From Date cannot be greater than To Date.";
                    response.msgType = 2;
                    return response;
                }

                DateTime payrollMonth = new DateTime(model.FROM_DATE.Value.Year, model.FROM_DATE.Value.Month, 1);
                if (IsFuturePayrollMonth(payrollMonth))
                {
                    response.msg = "This month is not eligible for Salary Generate.";
                    response.msgType = 2;
                    return response;
                }

                string fromDate = model.FROM_DATE.Value.ToString("yyyy-MM-dd");
                string toDate = model.TO_DATE.Value.ToString("yyyy-MM-dd");
                string payrollMonthText = payrollMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                bool isGenerated = IsSalaryAlreadyGenerated(payrollMonthText);

                List<SalaryGenerate> rows = GetSalaryFromSp(fromDate, toDate, model.FROM_DATE, model.TO_DATE);
                foreach (var row in rows)
                {
                    row.IS_GENERATED = isGenerated;
                    row.PAYROLL_MONTH = payrollMonthText;
                }

                response.data = new
                {
                    rows = rows,
                    isGenerated = isGenerated
                };
                response.msg = "";
                response.msgType = 1;
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage Generate(SalaryGenerateRequest model, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                if (model.FROM_DATE == null || model.TO_DATE == null)
                {
                    response.msg = "Please select From Date and To Date.";
                    response.msgType = 2;
                    return response;
                }

                if (model.FROM_DATE > model.TO_DATE)
                {
                    response.msg = "From Date cannot be greater than To Date.";
                    response.msgType = 2;
                    return response;
                }

                DateTime payrollMonth = new DateTime(model.FROM_DATE.Value.Year, model.FROM_DATE.Value.Month, 1);
                if (IsFuturePayrollMonth(payrollMonth))
                {
                    response.msg = "This month is not eligible for Salary Generate.";
                    response.msgType = 2;
                    return response;
                }

                string payrollMonthText = payrollMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                if (IsSalaryAlreadyGenerated(payrollMonthText))
                {
                    response.msg = "Salary already generated for this month.";
                    response.msgType = 2;
                    return response;
                }

                string fromDate = model.FROM_DATE.Value.ToString("yyyy-MM-dd");
                string toDate = model.TO_DATE.Value.ToString("yyyy-MM-dd");
                var Ip = common.IPAddress;
                var Computer = common.ComputerName;
                var Postal = common.PostalCode;
                var userid = common.Username;
                string addDate = CommonService.GetDateTime("Pakistan Standard Time");

                List<SalaryGenerate> rows = GetSalaryFromSp(fromDate, toDate, model.FROM_DATE, model.TO_DATE);
                if (rows.Count == 0)
                {
                    response.data = new { rows = rows, isGenerated = false };
                    response.msg = "No salary data found for selected period.";
                    response.msgType = 2;
                    return response;
                }

                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    try
                    {
                        foreach (var item in rows)
                        {
                            command.CommandText = "INSERT INTO " + TableName + " " +
                                "(EMP_PIC,MACHINE_ID,EMPLOYEE_CODE,EMP_NAME,DESIGNATION,DEPARTMENT,PAYROLL_MONTH," +
                                "PRESENT_DAYS,SUNDAY_DAYS,WORKING_DAYS,ABSENT_DAYS," +
                                "BASIC_SALARY,HOUSE_ALLOWANCE,MEDICAL_ALLOWANCE,CONVEYANCE_ALLOWANCE," +
                                "OVERTIME,BONUS,GROSS_SALARY,PER_DAY_SALARY,ABSENT_DEDUCTION," +
                                "INCOME_TAX,TOTAL_ADVANCE,LOAN_INSTALLMENT,TOTAL_LATE_HOURS,AVG_IN_TIME,NET_SALARY," +
                                "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME," +
                                "EDIT_IP_ADDRESS,ADD_POSTALCODE,EDIT_POSTALCODE,ASTATUS,MENU_ID,DLT) VALUES (" +
                                SqlString(item.EMP_PIC) + "," +
                                SqlInt(item.MACHINE_ID) + "," +
                                SqlInt(item.EMPLOYEE_CODE) + "," +
                                SqlString(item.EMP_NAME) + "," +
                                SqlString(item.DESIGNATION) + "," +
                                SqlString(item.DEPARTMENT) + "," +
                                SqlString(payrollMonthText) + "," +
                                SqlInt(item.PRESENT_DAYS) + "," +
                                SqlInt(item.SUNDAY_DAYS) + "," +
                                SqlInt(item.WORKING_DAYS) + "," +
                                SqlInt(item.ABSENT_DAYS) + "," +
                                SqlNumber(item.BASIC_SALARY) + "," +
                                SqlNumber(item.HOUSE_ALLOWANCE) + "," +
                                SqlNumber(item.MEDICAL_ALLOWANCE) + "," +
                                SqlNumber(item.CONVEYANCE_ALLOWANCE) + "," +
                                SqlNumber(item.OVERTIME) + "," +
                                SqlNumber(item.BONUS) + "," +
                                SqlNumber(item.GROSS_SALARY) + "," +
                                SqlNumber(item.PER_DAY_SALARY) + "," +
                                SqlNumber(item.ABSENT_DEDUCTION) + "," +
                                SqlNumber(item.INCOME_TAX) + "," +
                                SqlNumber(item.TOTAL_ADVANCE) + "," +
                                SqlNumber(item.LOAN_INSTALLMENT) + "," +
                                SqlString(item.TOTAL_LATE_HOURS) + "," +
                                SqlTime(item.AVG_IN_TIME) + "," +
                                SqlNumber(item.NET_SALARY) + "," +
                                "'" + userid + "'," +
                                "'" + addDate + "'," +
                                "'" + Computer + "'," +
                                "'" + Ip + "'," +
                                "'" + userid + "'," +
                                "'" + addDate + "'," +
                                "'" + Computer + "'," +
                                "'" + Ip + "'," +
                                "'" + Postal + "'," +
                                "'" + Postal + "'," +
                                "'Y'," +
                                SqlNumber(common.MenuID) + "," +
                                "'T')";
                            command.ExecuteNonQuery();
                            item.PAYROLL_MONTH = payrollMonthText;
                            item.IS_GENERATED = true;
                        }

                        transaction.Commit();
                        response.data = new { rows = rows, isGenerated = true };
                        response.msg = "Salary Generated Successfully";
                        response.msgType = 1;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.data = new { rows = rows, isGenerated = false };
                        response.msg = GetErrorMessage(ex);
                        response.msgType = 2;
                    }
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        private List<SalaryGenerate> GetSalaryFromSp(string fromDate, string toDate, DateTime? fromDateValue, DateTime? toDateValue)
        {
            List<SalaryGenerate> rows = new List<SalaryGenerate>();
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                connection.Open();
                string spQuery = "EXEC SALARY_GEN '" + fromDate + "','" + toDate + "'";
                using (SqlCommand spCommand = new SqlCommand(spQuery, connection))
                {
                    using (SqlDataReader reader = spCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(MapRow(reader, fromDateValue, toDateValue));
                        }
                    }
                }
            }
            return rows;
        }

        private bool IsFuturePayrollMonth(DateTime payrollMonth)
        {
            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return payrollMonth > currentMonth;
        }

        private bool IsSalaryAlreadyGenerated(string payrollMonthText)
        {
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                string query = "SELECT COUNT(1) FROM " + TableName +
                               " WHERE DLT = 'T' AND PAYROLL_MONTH = " + SqlString(payrollMonthText);
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private SalaryGenerate MapRow(SqlDataReader reader, DateTime? fromDate, DateTime? toDate)
        {
            string payrollMonthDisplay = fromDate.HasValue
                ? fromDate.Value.ToString("MMMM yyyy", CultureInfo.InvariantCulture)
                : ReadString(reader, "PAYROLL_MONTH");

            return new SalaryGenerate
            {
                FROM_DATE = fromDate,
                TO_DATE = toDate,
                EMP_PIC = ReadString(reader, "EMP_PIC"),
                MACHINE_ID = ReadString(reader, "MACHINE_ID"),
                EMPLOYEE_CODE = ReadString(reader, "EMPLOYEE_CODE"),
                EMP_NAME = ReadString(reader, "EMP_NAME"),
                DESIGNATION = ReadString(reader, "DESIGNATION"),
                DEPARTMENT = ReadString(reader, "DEPARTMENT"),
                PAYROLL_MONTH = payrollMonthDisplay,
                PRESENT_DAYS = ReadDouble(reader, "PRESENT_DAYS"),
                SUNDAY_DAYS = ReadDouble(reader, "SUNDAY_DAYS"),
                WORKING_DAYS = ReadDouble(reader, "WORKING_DAYS"),
                ABSENT_DAYS = ReadDouble(reader, "ABSENT_DAYS"),
                BASIC_SALARY = ReadDouble(reader, "BASIC_SALARY"),
                HOUSE_ALLOWANCE = ReadDouble(reader, "HOUSE_ALLOWANCE"),
                MEDICAL_ALLOWANCE = ReadDouble(reader, "MEDICAL_ALLOWANCE"),
                CONVEYANCE_ALLOWANCE = ReadDouble(reader, "CONVEYANCE_ALLOWANCE"),
                OVERTIME = ReadDouble(reader, "OVERTIME"),
                BONUS = ReadDouble(reader, "BONUS"),
                GROSS_SALARY = ReadDouble(reader, "GROSS_SALARY"),
                PER_DAY_SALARY = ReadDouble(reader, "PER_DAY_SALARY"),
                ABSENT_DEDUCTION = ReadDouble(reader, "ABSENT_DEDUCTION"),
                INCOME_TAX = ReadDouble(reader, "INCOME_TAX"),
                TOTAL_ADVANCE = ReadDouble(reader, "TOTAL_ADVANCE"),
                LOAN_INSTALLMENT = ReadDouble(reader, "LOAN_INSTALLMENT"),
                TOTAL_LATE_HOURS = ReadString(reader, "TOTAL_LATE_HOURS"),
                AVG_IN_TIME = ReadString(reader, "AVG_IN_TIME"),
                NET_SALARY = ReadDouble(reader, "NET_SALARY"),
            };
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private string? ReadString(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
            {
                return null;
            }
            return Convert.ToString(reader[columnName]);
        }

        private double? ReadDouble(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName) || reader[columnName] == DBNull.Value)
            {
                return null;
            }
            return Convert.ToDouble(reader[columnName]);
        }

        private string SqlNumber(double? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        private string SqlNumber(int? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        private string SqlInt(double? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return Convert.ToInt32(Math.Round(value.Value)).ToString(CultureInfo.InvariantCulture);
        }

        private string SqlInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "NULL";
            }
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result.ToString(CultureInfo.InvariantCulture);
            }
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double dbl))
            {
                return Convert.ToInt32(Math.Round(dbl)).ToString(CultureInfo.InvariantCulture);
            }
            return "NULL";
        }

        private string SqlTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "NULL";
            }

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan time))
            {
                return "'" + time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture) + "'";
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                return "'" + dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "'";
            }

            return SqlString(value);
        }

        private string SqlString(string? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return "'" + value.Replace("'", "''") + "'";
        }

        private string GetErrorMessage(Exception ex)
        {
            string message = ex.Message;
            if (ex.InnerException != null)
            {
                message += "<br/>" + ex.InnerException.Message;
            }
            return message;
        }
    }
}
