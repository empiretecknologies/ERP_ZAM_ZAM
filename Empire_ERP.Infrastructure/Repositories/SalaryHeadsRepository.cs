using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SalaryHeadsRepository : ISalaryHeadsRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public SalaryHeadsRepository(IMenuRepository menuRepository)
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
                        string query = "SELECT S.ID," +
                                       "C1.ACT_NAME AS BASIC_SALARY_NAME,C2.ACT_NAME AS HOUSE_ALLOWANCE_NAME,C3.ACT_NAME AS MEDICAL_ALLOWANCE_NAME," +
                                       "C4.ACT_NAME AS CONVEYANCE_ALLOWANCE_NAME,C5.ACT_NAME AS OVERTIME_NAME,C6.ACT_NAME AS BONUS_NAME," +
                                       "C7.ACT_NAME AS INCOME_TAX_NAME,C8.ACT_NAME AS LOAN_DEDUCTION_NAME,C9.ACT_NAME AS ADVANCE_DEDUCTION_NAME," +
                                       "S.ADD_USER_ID,S.ADD_DATE,S.ADD_COMPUTER_NAME,S.ADD_IP_ADDRESS,S.EDIT_USER_ID," +
                                       "S.EDIT_DATE,S.EDIT_COMPUTER_NAME,S.EDIT_IP_ADDRESS,S.ADD_POSTALCODE," +
                                       "S.EDIT_POSTALCODE,CASE WHEN S.ASTATUS = 'Y' THEN 'Active' else 'In-Active' end As ASTATUS " +
                                       "FROM " + table + " S " +
                                       "LEFT JOIN TBL_CHART C1 ON C1.ACT_CODE = S.BASIC_SALARY_COA_ID " +
                                       "LEFT JOIN TBL_CHART C2 ON C2.ACT_CODE = S.HOUSE_ALLOWANCE_COA_ID " +
                                       "LEFT JOIN TBL_CHART C3 ON C3.ACT_CODE = S.MEDICAL_ALLOWANCE_COA_ID " +
                                       "LEFT JOIN TBL_CHART C4 ON C4.ACT_CODE = S.CONVEYANCE_ALLOWANCE_COA_ID " +
                                       "LEFT JOIN TBL_CHART C5 ON C5.ACT_CODE = S.OVERTIME_COA_ID " +
                                       "LEFT JOIN TBL_CHART C6 ON C6.ACT_CODE = S.BONUS_COA_ID " +
                                       "LEFT JOIN TBL_CHART C7 ON C7.ACT_CODE = S.INCOME_TAX_COA_ID " +
                                       "LEFT JOIN TBL_CHART C8 ON C8.ACT_CODE = S.LOAN_DEDUCTION_COA_ID " +
                                       "LEFT JOIN TBL_CHART C9 ON C9.ACT_CODE = S.ADVANCE_DEDUCTION_COA_ID " +
                                       "WHERE S.MENU_ID = '" + common.MenuID + "' AND S.DLT = 'T' ORDER BY S.ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new SalaryHeads
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                BASIC_SALARY_NAME = Convert.ToString(reader["BASIC_SALARY_NAME"]),
                                HOUSE_ALLOWANCE_NAME = Convert.ToString(reader["HOUSE_ALLOWANCE_NAME"]),
                                MEDICAL_ALLOWANCE_NAME = Convert.ToString(reader["MEDICAL_ALLOWANCE_NAME"]),
                                CONVEYANCE_ALLOWANCE_NAME = Convert.ToString(reader["CONVEYANCE_ALLOWANCE_NAME"]),
                                OVERTIME_NAME = Convert.ToString(reader["OVERTIME_NAME"]),
                                BONUS_NAME = Convert.ToString(reader["BONUS_NAME"]),
                                INCOME_TAX_NAME = Convert.ToString(reader["INCOME_TAX_NAME"]),
                                LOAN_DEDUCTION_NAME = Convert.ToString(reader["LOAN_DEDUCTION_NAME"]),
                                ADVANCE_DEDUCTION_NAME = Convert.ToString(reader["ADVANCE_DEDUCTION_NAME"]),
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

        public MyHttpResponseMessage Save(SalaryHeads modelRecord, Common common)
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
                            if (modelRecord.ID == null || modelRecord.ID == 0)
                            {
                                query = "INSERT INTO " + table + " " +
                                            "(ID,BASIC_SALARY_COA_ID,HOUSE_ALLOWANCE_COA_ID,MEDICAL_ALLOWANCE_COA_ID,CONVEYANCE_ALLOWANCE_COA_ID," +
                                            "OVERTIME_COA_ID,BONUS_COA_ID,INCOME_TAX_COA_ID,LOAN_DEDUCTION_COA_ID,ADVANCE_DEDUCTION_COA_ID," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,EDIT_POSTALCODE," +
                                            "ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + GenerateNextId(common) + "'," + IntValue(modelRecord.BASIC_SALARY_COA_ID) + "," + IntValue(modelRecord.HOUSE_ALLOWANCE_COA_ID) + "," + IntValue(modelRecord.MEDICAL_ALLOWANCE_COA_ID) + "," + IntValue(modelRecord.CONVEYANCE_ALLOWANCE_COA_ID) + "," +
                                            IntValue(modelRecord.OVERTIME_COA_ID) + "," + IntValue(modelRecord.BONUS_COA_ID) + "," + IntValue(modelRecord.INCOME_TAX_COA_ID) + "," + IntValue(modelRecord.LOAN_DEDUCTION_COA_ID) + "," + IntValue(modelRecord.ADVANCE_DEDUCTION_COA_ID) + "," +
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
                                query = "UPDATE " + table + " SET BASIC_SALARY_COA_ID = " + IntValue(modelRecord.BASIC_SALARY_COA_ID) + @",
                                        HOUSE_ALLOWANCE_COA_ID = " + IntValue(modelRecord.HOUSE_ALLOWANCE_COA_ID) + @",
                                        MEDICAL_ALLOWANCE_COA_ID = " + IntValue(modelRecord.MEDICAL_ALLOWANCE_COA_ID) + @",
                                        CONVEYANCE_ALLOWANCE_COA_ID = " + IntValue(modelRecord.CONVEYANCE_ALLOWANCE_COA_ID) + @",
                                        OVERTIME_COA_ID = " + IntValue(modelRecord.OVERTIME_COA_ID) + @",
                                        BONUS_COA_ID = " + IntValue(modelRecord.BONUS_COA_ID) + @",
                                        INCOME_TAX_COA_ID = " + IntValue(modelRecord.INCOME_TAX_COA_ID) + @",
                                        LOAN_DEDUCTION_COA_ID = " + IntValue(modelRecord.LOAN_DEDUCTION_COA_ID) + @",
                                        ADVANCE_DEDUCTION_COA_ID = " + IntValue(modelRecord.ADVANCE_DEDUCTION_COA_ID) + @",
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
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public MyHttpResponseMessage GetSalaryHeadsById(int id, Common common)
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
                        string query = "SELECT ID,BASIC_SALARY_COA_ID,HOUSE_ALLOWANCE_COA_ID,MEDICAL_ALLOWANCE_COA_ID,CONVEYANCE_ALLOWANCE_COA_ID," +
                                       "OVERTIME_COA_ID,BONUS_COA_ID,INCOME_TAX_COA_ID,LOAN_DEDUCTION_COA_ID,ADVANCE_DEDUCTION_COA_ID,ASTATUS " +
                                       "FROM " + table + " " +
                                       "WHERE MENU_ID = '" + common.MenuID + "' AND DLT = 'T' AND ID = '" + id + "'";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            var row = new SalaryHeads
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                BASIC_SALARY_COA_ID = ReadInt(reader["BASIC_SALARY_COA_ID"]),
                                HOUSE_ALLOWANCE_COA_ID = ReadInt(reader["HOUSE_ALLOWANCE_COA_ID"]),
                                MEDICAL_ALLOWANCE_COA_ID = ReadInt(reader["MEDICAL_ALLOWANCE_COA_ID"]),
                                CONVEYANCE_ALLOWANCE_COA_ID = ReadInt(reader["CONVEYANCE_ALLOWANCE_COA_ID"]),
                                OVERTIME_COA_ID = ReadInt(reader["OVERTIME_COA_ID"]),
                                BONUS_COA_ID = ReadInt(reader["BONUS_COA_ID"]),
                                INCOME_TAX_COA_ID = ReadInt(reader["INCOME_TAX_COA_ID"]),
                                LOAN_DEDUCTION_COA_ID = ReadInt(reader["LOAN_DEDUCTION_COA_ID"]),
                                ADVANCE_DEDUCTION_COA_ID = ReadInt(reader["ADVANCE_DEDUCTION_COA_ID"]),
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

        private string IntValue(int? value)
        {
            return value == null ? "NULL" : value.Value.ToString();
        }

        private int? ReadInt(object value)
        {
            return value == DBNull.Value ? null : Convert.ToInt32(value);
        }
    }
}
