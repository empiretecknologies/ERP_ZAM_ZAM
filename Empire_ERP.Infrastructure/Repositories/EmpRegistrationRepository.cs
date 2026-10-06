using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class EmpRegistrationRepository : IEmpRegistrationRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public EmpRegistrationRepository(IMenuRepository menuRepository)
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
                        string query = "SELECT E.ID,E.EMPLOYEE_CODE,E.EMP_PIC,E.FIRST_NAME,E.LAST_NAME,E.EMAIL,E.PHONE," +
                                       "D.GROUP_NAME AS DEPARTMENT_NAME,G.GROUP_NAME AS DESIGNATION_NAME,E.MACHINE_ID,E.JOINING_DATE,E.ADD_USER_ID," +
                                       "E.ADD_DATE,E.ADD_COMPUTER_NAME,E.ADD_IP_ADDRESS,E.EDIT_USER_ID," +
                                       "E.EDIT_DATE,E.EDIT_COMPUTER_NAME,E.EDIT_IP_ADDRESS,E.ADD_POSTALCODE," +
                                       "E.EDIT_POSTALCODE,CASE WHEN E.ASTATUS = 'Y' THEN 'Active' else 'In-Active' end As ASTATUS " +
                                       "FROM " + table + " E " +
                                       "LEFT JOIN TBL_DEPARTMENTS D ON D.GROUP_CODE = E.DEPARTMENT_ID " +
                                       "LEFT JOIN TBL_DESIGNATION G ON G.GROUP_CODE = E.DESIGNATION_ID " +
                                       "WHERE E.MENU_ID = '" + common.MenuID + "' AND E.DLT = 'T' ORDER BY E.ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new EmpRegistration
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                EMPLOYEE_CODE = Convert.ToString(reader["EMPLOYEE_CODE"]),
                                EMP_PIC = Convert.ToString(reader["EMP_PIC"]),
                                FIRST_NAME = Convert.ToString(reader["FIRST_NAME"]),
                                LAST_NAME = Convert.ToString(reader["LAST_NAME"]),
                                EMAIL = Convert.ToString(reader["EMAIL"]),
                                PHONE = Convert.ToString(reader["PHONE"]),
                                DEPARTMENT_NAME = Convert.ToString(reader["DEPARTMENT_NAME"]),
                                DESIGNATION_NAME = Convert.ToString(reader["DESIGNATION_NAME"]),
                                MACHINE_ID = reader["MACHINE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["MACHINE_ID"]),
                                JOINING_DATE = reader["JOINING_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["JOINING_DATE"]),
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

        public MyHttpResponseMessage Save(EmpRegistration modelRecord, Common common)
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
                            string Duplicationquery = "";
                            string now = CommonService.GetDateTime("Pakistan Standard Time");
                            string machineId = modelRecord.MACHINE_ID == null ? "NULL" : modelRecord.MACHINE_ID.ToString();
                            string departmentId = modelRecord.DEPARTMENT_ID == null ? "NULL" : modelRecord.DEPARTMENT_ID.ToString();
                            string designationId = modelRecord.DESIGNATION_ID == null ? "NULL" : modelRecord.DESIGNATION_ID.ToString();
                            string joiningDate = modelRecord.JOINING_DATE == null ? "NULL" : "'" + modelRecord.JOINING_DATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                            if (modelRecord.ID == null || modelRecord.ID == 0)
                            {
                                string nextId = GenerateNextId(common);
                                query = "INSERT INTO " + table + " " +
                                            "(ID,EMPLOYEE_CODE,EMP_PIC,FIRST_NAME,LAST_NAME,EMAIL,PHONE,DEPARTMENT_ID,DESIGNATION_ID,MACHINE_ID,JOINING_DATE," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,EDIT_POSTALCODE," +
                                            "ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + nextId + "'," + SqlValue(modelRecord.EMPLOYEE_CODE) + "," + SqlValue(modelRecord.EMP_PIC) + "," + SqlValue(modelRecord.FIRST_NAME) + "," + SqlValue(modelRecord.LAST_NAME) + "," + SqlValue(modelRecord.EMAIL) + "," + SqlValue(modelRecord.PHONE) + "," + departmentId + "," + designationId + "," + machineId + "," + joiningDate + "," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'" + userid + "','" + now + "','" + Computer + "','" + Ip + "','" + Postal + "'," +
                                            "'" + modelRecord.ASTATUS + "','" + common.MenuID + "','T')";
                                command.CommandText = query;
                                command.ExecuteNonQuery();

                                Duplicationquery = "SELECT COUNT(*) FROM " + table + " WHERE EMPLOYEE_CODE = " + SqlValue(modelRecord.EMPLOYEE_CODE) + " AND MENU_ID = '" + common.MenuID + "' AND DLT = 'T'";
                                command.CommandText = Duplicationquery;
                                int count = (int)command.ExecuteScalar();
                                if (count == 1)
                                {
                                    transaction.Commit();
                                    response.msgType = 1;
                                    response.msg = "Record Added Successfully";
                                }
                                else
                                {
                                    transaction.Rollback();
                                    response.msg = "Employee Code Already Exist !....";
                                    response.msgType = 2;
                                }
                            }
                            else
                            {
                                query = "UPDATE " + table + " SET EMPLOYEE_CODE = " + SqlValue(modelRecord.EMPLOYEE_CODE) + @",
                                        EMP_PIC = " + SqlValue(modelRecord.EMP_PIC) + @",
                                        FIRST_NAME = " + SqlValue(modelRecord.FIRST_NAME) + @",
                                        LAST_NAME = " + SqlValue(modelRecord.LAST_NAME) + @",
                                        EMAIL = " + SqlValue(modelRecord.EMAIL) + @",
                                        PHONE = " + SqlValue(modelRecord.PHONE) + @",
                                        DEPARTMENT_ID = " + departmentId + @",
                                        DESIGNATION_ID = " + designationId + @",
                                        MACHINE_ID = " + machineId + @",
                                        JOINING_DATE = " + joiningDate + @",
                                        EDIT_USER_ID = '" + userid + @"',
                                        EDIT_DATE = '" + now + @"',
                                        EDIT_COMPUTER_NAME = '" + Computer + @"',
                                        EDIT_IP_ADDRESS = '" + Ip + @"',
                                        EDIT_POSTALCODE = '" + Postal + @"',
                                        ASTATUS = '" + modelRecord.ASTATUS + @"'
                                        WHERE ID = '" + modelRecord.ID + "' AND MENU_ID = '" + common.MenuID + "'";

                                command.CommandText = query;
                                command.ExecuteNonQuery();

                                Duplicationquery = "SELECT COUNT(*) FROM " + table + " WHERE EMPLOYEE_CODE = " + SqlValue(modelRecord.EMPLOYEE_CODE) + " AND MENU_ID = '" + common.MenuID + "' AND DLT = 'T'";
                                command.CommandText = Duplicationquery;
                                int count = (int)command.ExecuteScalar();
                                if (count == 1)
                                {
                                    transaction.Commit();
                                    response.msgType = 1;
                                    response.msg = "Record Updated Successfully";
                                }
                                else
                                {
                                    transaction.Rollback();
                                    response.msg = "Employee Code Already Exist !....";
                                    response.msgType = 2;
                                }
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

        public MyHttpResponseMessage GetEmpRegistrationById(int id, Common common)
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
                        string query = "SELECT ID,EMPLOYEE_CODE,EMP_PIC,FIRST_NAME,LAST_NAME,EMAIL,PHONE,DEPARTMENT_ID,DESIGNATION_ID,MACHINE_ID,JOINING_DATE,ASTATUS " +
                                       "FROM " + table + " " +
                                       "WHERE MENU_ID = '" + common.MenuID + "' AND DLT = 'T' AND ID = '" + id + "'";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            var row = new EmpRegistration
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                EMPLOYEE_CODE = Convert.ToString(reader["EMPLOYEE_CODE"]),
                                EMP_PIC = Convert.ToString(reader["EMP_PIC"]),
                                FIRST_NAME = Convert.ToString(reader["FIRST_NAME"]),
                                LAST_NAME = Convert.ToString(reader["LAST_NAME"]),
                                EMAIL = Convert.ToString(reader["EMAIL"]),
                                PHONE = Convert.ToString(reader["PHONE"]),
                                DEPARTMENT_ID = reader["DEPARTMENT_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["DEPARTMENT_ID"]),
                                DESIGNATION_ID = reader["DESIGNATION_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["DESIGNATION_ID"]),
                                MACHINE_ID = reader["MACHINE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["MACHINE_ID"]),
                                JOINING_DATE = reader["JOINING_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["JOINING_DATE"]),
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
