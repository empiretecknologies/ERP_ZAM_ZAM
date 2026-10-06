using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class EmployeeSalaryRepository : IEmployeeSalaryRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public EmployeeSalaryRepository(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT E.ID,E.EMPLOYEE_CODE,E.FIRST_NAME,E.LAST_NAME,E.PHONE," +
                                   "D.GROUP_NAME AS DEPARTMENT_NAME,G.GROUP_NAME AS DESIGNATION_NAME,E.JOINING_DATE," +
                                   "CASE WHEN E.ASTATUS = 'Y' THEN 'Active' else 'In-Active' end As ASTATUS " +
                                   "FROM TBL_EMP_REG E " +
                                   "LEFT JOIN TBL_DEPARTMENTS D ON D.GROUP_CODE = E.DEPARTMENT_ID " +
                                   "LEFT JOIN TBL_DESIGNATION G ON G.GROUP_CODE = E.DESIGNATION_ID " +
                                   "WHERE E.DLT = 'T' ORDER BY E.ID DESC";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new EmpRegistration
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            EMPLOYEE_CODE = Convert.ToString(reader["EMPLOYEE_CODE"]),
                            FIRST_NAME = Convert.ToString(reader["FIRST_NAME"]),
                            LAST_NAME = Convert.ToString(reader["LAST_NAME"]),
                            PHONE = Convert.ToString(reader["PHONE"]),
                            DEPARTMENT_NAME = Convert.ToString(reader["DEPARTMENT_NAME"]),
                            DESIGNATION_NAME = Convert.ToString(reader["DESIGNATION_NAME"]),
                            JOINING_DATE = reader["JOINING_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["JOINING_DATE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                        };

                        jsonDataResult.Add(row);
                    }
                    reader.Close();

                    response.data = jsonDataResult;
                    response.msg = "";
                    response.msgType = 1;
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetEmployeeSalaryByEmployeeId(int employeeId, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT ID,EMPLOYEE_ID,BASIC_SALARY,HOUSE_ALLOWANCE,MEDICAL_ALLOWANCE,CONVEYANCE_ALLOWANCE,OVERTIME,BONUS," +
                                       "INCOME_TAX,LOAN_DEDUCTION,ADVANCE_DEDUCTION,GROSS_SALARY,TOTAL_DEDUCTION,NET_SALARY,EFFECTIVE_FROM,EFFECTIVE_TO " +
                                       "FROM " + table + " " +
                                       "WHERE MENU_ID = '" + common.MenuID + "' AND DLT = 'T' AND EMPLOYEE_ID = '" + employeeId + "' ORDER BY ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new EmployeeSalary
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                EMPLOYEE_ID = reader["EMPLOYEE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["EMPLOYEE_ID"]),
                                BASIC_SALARY = reader["BASIC_SALARY"] == DBNull.Value ? null : Convert.ToDouble(reader["BASIC_SALARY"]),
                                HOUSE_ALLOWANCE = reader["HOUSE_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["HOUSE_ALLOWANCE"]),
                                MEDICAL_ALLOWANCE = reader["MEDICAL_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["MEDICAL_ALLOWANCE"]),
                                CONVEYANCE_ALLOWANCE = reader["CONVEYANCE_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["CONVEYANCE_ALLOWANCE"]),
                                OVERTIME = reader["OVERTIME"] == DBNull.Value ? null : Convert.ToDouble(reader["OVERTIME"]),
                                BONUS = reader["BONUS"] == DBNull.Value ? null : Convert.ToDouble(reader["BONUS"]),
                                INCOME_TAX = reader["INCOME_TAX"] == DBNull.Value ? null : Convert.ToDouble(reader["INCOME_TAX"]),
                                LOAN_DEDUCTION = reader["LOAN_DEDUCTION"] == DBNull.Value ? null : Convert.ToDouble(reader["LOAN_DEDUCTION"]),
                                ADVANCE_DEDUCTION = reader["ADVANCE_DEDUCTION"] == DBNull.Value ? null : Convert.ToDouble(reader["ADVANCE_DEDUCTION"]),
                                GROSS_SALARY = reader["GROSS_SALARY"] == DBNull.Value ? null : Convert.ToDouble(reader["GROSS_SALARY"]),
                                TOTAL_DEDUCTION = reader["TOTAL_DEDUCTION"] == DBNull.Value ? null : Convert.ToDouble(reader["TOTAL_DEDUCTION"]),
                                NET_SALARY = reader["NET_SALARY"] == DBNull.Value ? null : Convert.ToDouble(reader["NET_SALARY"]),
                                EFFECTIVE_FROM = reader["EFFECTIVE_FROM"] == DBNull.Value ? null : Convert.ToDateTime(reader["EFFECTIVE_FROM"]),
                                EFFECTIVE_TO = reader["EFFECTIVE_TO"] == DBNull.Value ? null : Convert.ToDateTime(reader["EFFECTIVE_TO"]),
                            };

                            jsonDataResult.Add(row);
                        }
                        reader.Close();

                        response.data = jsonDataResult;
                        response.msg = "";
                        response.msgType = 1;
                    }
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage Save(CustomEmployeeSalary modelRecord, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    if (modelRecord.EMPLOYEE_ID == null || modelRecord.EMPLOYEE_ID == 0)
                    {
                        response.msg = "Please select employee.";
                        response.msgType = 2;
                        return response;
                    }
                    if (modelRecord.Detail == null || modelRecord.Detail.Count == 0)
                    {
                        response.msg = "No changes to save.";
                        response.msgType = 2;
                        return response;
                    }

                    var Ip = common.IPAddress;
                    var Computer = common.ComputerName;
                    var Postal = common.PostalCode;
                    var userid = common.Username;
                    string connectionString = new SQLService().getconnstring();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();
                        SqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        try
                        {
                            string query = "";
                            string now = CommonService.GetDateTime("Pakistan Standard Time");

                            command.CommandText = "SELECT ISNULL(MAX(ID), 0) + 1 FROM " + table;
                            int nextId = Convert.ToInt32(command.ExecuteScalar());

                            foreach (var item in modelRecord.Detail)
                            {
                                if (item.ID == null || item.ID == 0)
                                {
                                    query = "INSERT INTO " + table + " " +
                                            "(ID,EMPLOYEE_ID,BASIC_SALARY,HOUSE_ALLOWANCE,MEDICAL_ALLOWANCE,CONVEYANCE_ALLOWANCE,OVERTIME,BONUS," +
                                            "INCOME_TAX,LOAN_DEDUCTION,ADVANCE_DEDUCTION,GROSS_SALARY,TOTAL_DEDUCTION,NET_SALARY,EFFECTIVE_FROM,EFFECTIVE_TO," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,EDIT_POSTALCODE," +
                                            "ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + nextId + "','" + modelRecord.EMPLOYEE_ID + "'," + SqlNumber(item.BASIC_SALARY) + "," + SqlNumber(item.HOUSE_ALLOWANCE) + "," + SqlNumber(item.MEDICAL_ALLOWANCE) + "," + SqlNumber(item.CONVEYANCE_ALLOWANCE) + "," + SqlNumber(item.OVERTIME) + "," + SqlNumber(item.BONUS) + "," +
                                            SqlNumber(item.INCOME_TAX) + "," + SqlNumber(item.LOAN_DEDUCTION) + "," + SqlNumber(item.ADVANCE_DEDUCTION) + "," + SqlNumber(item.GROSS_SALARY) + "," + SqlNumber(item.TOTAL_DEDUCTION) + "," + SqlNumber(item.NET_SALARY) + "," + SqlDate(item.EFFECTIVE_FROM) + "," + SqlDate(item.EFFECTIVE_TO) + "," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'Y','" + common.MenuID + "','T')";
                                    nextId++;
                                }
                                else
                                {
                                    query = "UPDATE " + table + " SET BASIC_SALARY = " + SqlNumber(item.BASIC_SALARY) + @",
                                            HOUSE_ALLOWANCE = " + SqlNumber(item.HOUSE_ALLOWANCE) + @",
                                            MEDICAL_ALLOWANCE = " + SqlNumber(item.MEDICAL_ALLOWANCE) + @",
                                            CONVEYANCE_ALLOWANCE = " + SqlNumber(item.CONVEYANCE_ALLOWANCE) + @",
                                            OVERTIME = " + SqlNumber(item.OVERTIME) + @",
                                            BONUS = " + SqlNumber(item.BONUS) + @",
                                            INCOME_TAX = " + SqlNumber(item.INCOME_TAX) + @",
                                            LOAN_DEDUCTION = " + SqlNumber(item.LOAN_DEDUCTION) + @",
                                            ADVANCE_DEDUCTION = " + SqlNumber(item.ADVANCE_DEDUCTION) + @",
                                            GROSS_SALARY = " + SqlNumber(item.GROSS_SALARY) + @",
                                            TOTAL_DEDUCTION = " + SqlNumber(item.TOTAL_DEDUCTION) + @",
                                            NET_SALARY = " + SqlNumber(item.NET_SALARY) + @",
                                            EFFECTIVE_FROM = " + SqlDate(item.EFFECTIVE_FROM) + @",
                                            EFFECTIVE_TO = " + SqlDate(item.EFFECTIVE_TO) + @",
                                            EDIT_USER_ID = '" + userid + @"',
                                            EDIT_DATE = '" + now + @"',
                                            EDIT_COMPUTER_NAME = '" + Computer + @"',
                                            EDIT_IP_ADDRESS = '" + Ip + @"',
                                            EDIT_POSTALCODE = '" + Postal + @"'
                                            WHERE ID = '" + item.ID + "' AND EMPLOYEE_ID = '" + modelRecord.EMPLOYEE_ID + "' AND MENU_ID = '" + common.MenuID + "'";
                                }
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            response.msgType = 1;
                            response.msg = "Record Saved Successfully";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            string _catchMessage = ex.Message;
                            if (ex.InnerException != null)
                            {
                                _catchMessage += "<br/>" + ex.InnerException.Message;
                            }
                            response.msg = _catchMessage;
                            response.msgType = 2;
                        }
                    }
                }
                else
                {
                    response.data = "";
                    response.msg = "Something went wrong! please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }

        private string SqlNumber(double? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        private string SqlDate(DateTime? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return "'" + value.Value.ToString("yyyy-MM-dd") + "'";
        }
    }
}
