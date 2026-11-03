using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class EmpManualAttRepository : IEmpManualAttRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public EmpManualAttRepository(IMenuRepository menuRepository)
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
                        string query = "SELECT A.TRAN_ID,A.MACHINE_ID,E.ID AS EMPLOYEE_ID," +
                                       "LTRIM(RTRIM(ISNULL(E.FIRST_NAME,'') + ' ' + ISNULL(E.LAST_NAME,''))) AS EMPLOYEE_NAME," +
                                       "A.punch_time,A.SHIFT_STATUS,A.ADD_USER_ID," +
                                       "A.ADD_DATE,A.ADD_COMPUTER_NAME,A.ADD_IP_ADDRESS,A.EDIT_USER_ID," +
                                       "A.EDIT_DATE,A.EDIT_COMPUTER_NAME,A.EDIT_IP_ADDRESS,A.ADD_POSTALCODE," +
                                       "A.EDIT_POSTALCODE,CASE WHEN A.ASTATUS = 'Y' THEN 'Active' else 'In-Active' end As ASTATUS " +
                                       "FROM " + table + " A " +
                                       "LEFT JOIN TBL_EMP_REG E ON E.MACHINE_ID = A.MACHINE_ID AND E.DLT = 'T' " +
                                       "WHERE A.MENU_ID = '" + common.MenuID + "' AND A.DLT = 'T' ORDER BY A.TRAN_ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new EmpManualAtt
                            {
                                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                                MACHINE_ID = reader["MACHINE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["MACHINE_ID"]),
                                EMPLOYEE_ID = reader["EMPLOYEE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["EMPLOYEE_ID"]),
                                EMPLOYEE_NAME = Convert.ToString(reader["EMPLOYEE_NAME"]),
                                punch_time = reader["punch_time"] == DBNull.Value ? null : Convert.ToDateTime(reader["punch_time"]),
                                SHIFT_STATUS = Convert.ToString(reader["SHIFT_STATUS"]),
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

        public MyHttpResponseMessage Save(EmpManualAtt modelRecord, Common common)
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
                            string machineId = modelRecord.MACHINE_ID == null ? "NULL" : modelRecord.MACHINE_ID.ToString();
                            string punchTime = modelRecord.punch_time == null ? "NULL" : "'" + modelRecord.punch_time.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            string shiftStatus = SqlValue(modelRecord.SHIFT_STATUS);

                            if (modelRecord.SHIFT_STATUS == "Check In" && modelRecord.MACHINE_ID != null && modelRecord.punch_time != null)
                            {
                                string punchDate = modelRecord.punch_time.Value.ToString("yyyy-MM-dd");
                                string duplicateCheckQuery = "SELECT COUNT(*) FROM " + table +
                                    " WHERE MACHINE_ID = " + machineId +
                                    " AND SHIFT_STATUS = 'Check In'" +
                                    " AND CAST(punch_time AS DATE) = '" + punchDate + "'" +
                                    " AND DLT = 'T' AND MENU_ID = '" + common.MenuID + "'";

                                if (modelRecord.TRAN_ID != null && modelRecord.TRAN_ID != 0)
                                {
                                    duplicateCheckQuery += " AND TRAN_ID <> '" + modelRecord.TRAN_ID + "'";
                                }

                                command.CommandText = duplicateCheckQuery;
                                int checkInCount = (int)command.ExecuteScalar();
                                if (checkInCount > 0)
                                {
                                    transaction.Rollback();
                                    response.msg = "Employee already has Check In for this date.";
                                    response.msgType = 2;
                                    return response;
                                }
                            }

                            if (modelRecord.TRAN_ID == null || modelRecord.TRAN_ID == 0)
                            {
                                string nextId = GenerateNextId(common);
                                query = "INSERT INTO " + table + " " +
                                            "(TRAN_ID,MACHINE_ID,punch_time,SHIFT_STATUS," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,EDIT_POSTALCODE," +
                                            "ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + nextId + "'," + machineId + "," + punchTime + "," + shiftStatus + "," +
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
                                query = "UPDATE " + table + " SET MACHINE_ID = " + machineId + @",
                                        punch_time = " + punchTime + @",
                                        SHIFT_STATUS = " + shiftStatus + @",
                                        EDIT_USER_ID = '" + userid + @"',
                                        EDIT_DATE = '" + now + @"',
                                        EDIT_COMPUTER_NAME = '" + Computer + @"',
                                        EDIT_IP_ADDRESS = '" + Ip + @"',
                                        EDIT_POSTALCODE = '" + Postal + @"',
                                        ASTATUS = '" + modelRecord.ASTATUS + @"'
                                        WHERE TRAN_ID = '" + modelRecord.TRAN_ID + "' AND MENU_ID = '" + common.MenuID + "'";

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
                    string maxIdQuery = "SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM " + table;
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

        public MyHttpResponseMessage GetEmpManualAttById(int id, Common common)
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
                        string query = "SELECT A.TRAN_ID,A.MACHINE_ID,E.ID AS EMPLOYEE_ID,A.punch_time,A.SHIFT_STATUS,A.ASTATUS " +
                                       "FROM " + table + " A " +
                                       "LEFT JOIN TBL_EMP_REG E ON E.MACHINE_ID = A.MACHINE_ID AND E.DLT = 'T' " +
                                       "WHERE A.MENU_ID = '" + common.MenuID + "' AND A.DLT = 'T' AND A.TRAN_ID = '" + id + "'";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            var row = new EmpManualAtt
                            {
                                TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                                MACHINE_ID = reader["MACHINE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["MACHINE_ID"]),
                                EMPLOYEE_ID = reader["EMPLOYEE_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["EMPLOYEE_ID"]),
                                punch_time = reader["punch_time"] == DBNull.Value ? null : Convert.ToDateTime(reader["punch_time"]),
                                SHIFT_STATUS = Convert.ToString(reader["SHIFT_STATUS"]),
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
                            string query = "UPDATE " + table + " SET DLT = 'F' WHERE TRAN_ID = '" + id + "' AND MENU_ID = '" + common.MenuID + "'";
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
