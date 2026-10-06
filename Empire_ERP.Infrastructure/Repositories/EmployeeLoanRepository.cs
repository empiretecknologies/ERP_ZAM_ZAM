using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class EmployeeLoanRepository : IEmployeeLoanRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public EmployeeLoanRepository(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
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
                        string query = "SELECT L.ID,L.EMPLOYEE_ID,LTRIM(RTRIM(ISNULL(E.FIRST_NAME,'') + ' ' + ISNULL(E.LAST_NAME,''))) AS EMPLOYEE_NAME," +
                                       "L.LOAN_DATE,L.LOAN_AMOUNT,L.INSTALLMENT_AMOUNT,L.TOTAL_INSTALLMENTS,L.START_DEDUCTION_DATE,L.REMARKS,L.ADD_USER_ID," +
                                       "L.ADD_DATE,L.ADD_COMPUTER_NAME,L.ADD_IP_ADDRESS,L.EDIT_USER_ID," +
                                       "L.EDIT_DATE,L.EDIT_COMPUTER_NAME,L.EDIT_IP_ADDRESS,L.ADD_POSTALCODE," +
                                       "L.EDIT_POSTALCODE,CASE WHEN L.ASTATUS = 'Y' THEN 'Active' else 'In-Active' end As ASTATUS " +
                                       "FROM " + table + " L " +
                                       "LEFT JOIN TBL_EMP_REG E ON E.ID = L.EMPLOYEE_ID " +
                                       "WHERE L.MENU_ID = '" + common.MenuID + "' AND L.DLT = 'T' ORDER BY L.ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new EmployeeLoan
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                EMPLOYEE_ID = reader["EMPLOYEE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["EMPLOYEE_ID"]),
                                EMPLOYEE_NAME = Convert.ToString(reader["EMPLOYEE_NAME"]),
                                LOAN_DATE = reader["LOAN_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["LOAN_DATE"]),
                                LOAN_AMOUNT = reader["LOAN_AMOUNT"] == DBNull.Value ? null : Convert.ToDouble(reader["LOAN_AMOUNT"]),
                                INSTALLMENT_AMOUNT = reader["INSTALLMENT_AMOUNT"] == DBNull.Value ? null : Convert.ToDouble(reader["INSTALLMENT_AMOUNT"]),
                                TOTAL_INSTALLMENTS = reader["TOTAL_INSTALLMENTS"] == DBNull.Value ? null : Convert.ToInt32(reader["TOTAL_INSTALLMENTS"]),
                                START_DEDUCTION_DATE = reader["START_DEDUCTION_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["START_DEDUCTION_DATE"]),
                                REMARKS = Convert.ToString(reader["REMARKS"]),
                                ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                                ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]),
                                ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                                ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                                EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                                EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]),
                                EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                                EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
                                ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                                EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
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

        public MyHttpResponseMessage Save(EmployeeLoan modelRecord, Common common)
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
                            string employeeId = modelRecord.EMPLOYEE_ID == null ? "NULL" : modelRecord.EMPLOYEE_ID.ToString();
                            string loanDate = modelRecord.LOAN_DATE == null ? "NULL" : "'" + modelRecord.LOAN_DATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            string loanAmount = modelRecord.LOAN_AMOUNT == null ? "NULL" : modelRecord.LOAN_AMOUNT.Value.ToString(CultureInfo.InvariantCulture);
                            string installmentAmount = modelRecord.INSTALLMENT_AMOUNT == null ? "NULL" : modelRecord.INSTALLMENT_AMOUNT.Value.ToString(CultureInfo.InvariantCulture);
                            string totalInstallments = modelRecord.TOTAL_INSTALLMENTS == null ? "NULL" : modelRecord.TOTAL_INSTALLMENTS.ToString();
                            string startDeductionDate = modelRecord.START_DEDUCTION_DATE == null ? "NULL" : "'" + modelRecord.START_DEDUCTION_DATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                            if (modelRecord.ID == null || modelRecord.ID == 0)
                            {
                                string nextId = GenerateNextId(common);
                                query = "INSERT INTO " + table + " " +
                                            "(ID,EMPLOYEE_ID,LOAN_DATE,LOAN_AMOUNT,INSTALLMENT_AMOUNT,TOTAL_INSTALLMENTS,START_DEDUCTION_DATE,REMARKS," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,EDIT_POSTALCODE," +
                                            "ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + nextId + "'," + employeeId + "," + loanDate + "," + loanAmount + "," + installmentAmount + "," + totalInstallments + "," + startDeductionDate + "," + SqlValue(modelRecord.REMARKS) + "," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'" + modelRecord.ASTATUS + "','" + common.MenuID + "','T')";
                                command.CommandText = query;
                                command.ExecuteNonQuery();

                                transaction.Commit();
                                response.msgType = 1;
                                response.msg = "Record Added Successfully";
                            }
                            else
                            {
                                query = "UPDATE " + table + " SET EMPLOYEE_ID = " + employeeId + @",
                                        LOAN_DATE = " + loanDate + @",
                                        LOAN_AMOUNT = " + loanAmount + @",
                                        INSTALLMENT_AMOUNT = " + installmentAmount + @",
                                        TOTAL_INSTALLMENTS = " + totalInstallments + @",
                                        START_DEDUCTION_DATE = " + startDeductionDate + @",
                                        REMARKS = " + SqlValue(modelRecord.REMARKS) + @",
                                        EDIT_USER_ID = '" + userid + @"',
                                        EDIT_DATE = '" + now + @"',
                                        EDIT_COMPUTER_NAME = '" + Computer + @"',
                                        EDIT_IP_ADDRESS = '" + Ip + @"',
                                        EDIT_POSTALCODE = '" + Postal + @"',
                                        ASTATUS = '" + modelRecord.ASTATUS + @"'
                                        WHERE ID = '" + modelRecord.ID + "' AND MENU_ID = '" + common.MenuID + "'";

                                command.CommandText = query;
                                command.ExecuteNonQuery();

                                transaction.Commit();
                                response.msgType = 1;
                                response.msg = "Record Updated Successfully";
                            }
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

        public string GenerateNextId(Common common)
        {
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
                    string maxIdQuery = "SELECT ISNULL(MAX(ID), 0) + 1 FROM " + table;
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        SqlCommand command = new SqlCommand(maxIdQuery, connection);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        int nextId = Convert.ToInt32(result);
                        return Convert.ToString(nextId);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public MyHttpResponseMessage GetEmployeeLoanById(int id, Common common)
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
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT ID,EMPLOYEE_ID,LOAN_DATE,LOAN_AMOUNT,INSTALLMENT_AMOUNT,TOTAL_INSTALLMENTS,START_DEDUCTION_DATE,REMARKS,ASTATUS " +
                                       "FROM " + table + " " +
                                       "WHERE MENU_ID = '" + common.MenuID + "' AND DLT = 'T' AND ID = '" + id + "'";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            var row = new EmployeeLoan
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                EMPLOYEE_ID = reader["EMPLOYEE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["EMPLOYEE_ID"]),
                                LOAN_DATE = reader["LOAN_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["LOAN_DATE"]),
                                LOAN_AMOUNT = reader["LOAN_AMOUNT"] == DBNull.Value ? null : Convert.ToDouble(reader["LOAN_AMOUNT"]),
                                INSTALLMENT_AMOUNT = reader["INSTALLMENT_AMOUNT"] == DBNull.Value ? null : Convert.ToDouble(reader["INSTALLMENT_AMOUNT"]),
                                TOTAL_INSTALLMENTS = reader["TOTAL_INSTALLMENTS"] == DBNull.Value ? null : Convert.ToInt32(reader["TOTAL_INSTALLMENTS"]),
                                START_DEDUCTION_DATE = reader["START_DEDUCTION_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["START_DEDUCTION_DATE"]),
                                REMARKS = Convert.ToString(reader["REMARKS"]),
                                ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            };

                            response.msg = "";
                            response.msgType = 1;
                            response.data = row;
                        }
                        reader.Close();
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

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msg = "Data not found in our records";
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
                    if (id == 0)
                    {
                        response.msg = "ID is not in numeric format";
                        response.msgType = 2;
                    }
                    else
                    {
                        string connectionString = new SQLService().getconnstring();
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "UPDATE " + table + " SET DLT = 'F' WHERE ID = '" + id + "' AND MENU_ID = '" + common.MenuID + "'";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.ExecuteNonQuery();
                            response.msg = "Record Deleted Successfully";
                            response.msgType = 1;
                        }
                    }
                }
                else
                {
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

        private string SqlValue(string? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return "'" + value.Replace("'", "''") + "'";
        }
    }
}
