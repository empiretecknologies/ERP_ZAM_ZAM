using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class SalaryDisbursementRepository : ISalaryDisbursementRepository
    {
        public IBranchRepository _branchRepository { get; set; }
        public SalaryDisbursementRepository(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public MyHttpResponseMessage GetChartOfAccounts(int? pType, Common common)
        {
            string nature = "1,2";
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<KeyValuePair<int, string?>> dropdown = new List<KeyValuePair<int, string?>>();
                using (SqlConnection db = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = @$" SELECT 
                                      CH.ACT_CODE,CH.ACT_NAME 
                                    FROM TBL_ROLE R 
                                    LEFT OUTER JOIN TBL_CHART CH
                                       ON (
                                          (R.SHOW_SELECTED = 1 AND CH.ACT_CODE = R.RMENU_ID)
                                          OR 
                                          (R.SHOW_SELECTED = 0 AND 
                                           CH.ACT_CODE NOT IN 
                                             (SELECT DISTINCT RMENU_ID FROM TBL_ROLE WHERE ROLE_ID = {common.RoleID} AND MODULE_ID = 3) 
                                          )
                                      ) 
                                      LEFT OUTER JOIN TBL_CHART PCH
                                    ON PCH.ACT_CODE = CH.ACT_PARENT_CODE
                                    WHERE R.ROLE_TYPE = '{common.RoleType}'
                                      AND R.MODULE_ID = 3 
                                      AND CH.DLT = 'T' 
                                      AND CH.ASTATUS = 'Y'
                                      AND R.ROLE_ID = {common.RoleID}
                                      AND CH.ACT_NATURE IN ({nature}) AND CH.ACT_TYPE = 'S' AND CH.DLT = 'T' AND CH.ASTATUS = 'Y'
                                    GROUP BY CH.ACT_CODE,CH.ACT_NAME 
                                      UNION ALL
                                      SELECT CH.ACT_CODE,CH.ACT_NAME 
                                      FROM TBL_CHART CH
                                      LEFT OUTER JOIN TBL_CHART PCH
                                    ON PCH.ACT_CODE = CH.ACT_PARENT_CODE
                                    WHERE '{common.RoleType}' = 'A'  
                                    AND CH.ACT_NATURE IN ({nature}) AND CH.ACT_TYPE = 'S' AND CH.DLT = 'T' AND CH.ASTATUS = 'Y'
                                    UNION ALL
                                            SELECT CH.ACT_CODE,CH.ACT_NAME 
		                                    FROM TBL_CHART CH 
                                      LEFT OUTER JOIN TBL_CHART PCH
                                    ON PCH.ACT_CODE = CH.ACT_PARENT_CODE
		                                    WHERE '{common.RoleType}' = 'U'  
                                    AND CH.ACT_NATURE IN ({nature}) AND CH.ACT_TYPE = 'S' AND CH.DLT = 'T' AND CH.ASTATUS = 'Y'
                                          AND CH.DLT = 'T' 
                                          AND CH.ASTATUS = 'Y' AND  
                                           (SELECT 
                                     COUNT(R.MODULE_ID) FROM TBL_ROLE R LEFT OUTER JOIN TBL_CHART M ON R.RMENU_ID = M.ACT_CODE 
                                     WHERE R.ROLE_TYPE = '{common.RoleType}'
                                          AND R.MODULE_ID = 3 
                                          AND M.DLT = 'T' 
                                          AND M.ASTATUS = 'Y'
                                          AND R.ROLE_ID = {common.RoleID}
                                        AND CH.ACT_NATURE IN ({nature}) AND CH.ACT_TYPE = 'S' AND CH.DLT = 'T' AND CH.ASTATUS = 'Y'
                                          ) < 1 ";
                    using (SqlCommand command = new SqlCommand(query, db))
                    {
                        db.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    dropdown.Add(new KeyValuePair<int, string?>(Convert.ToInt32(reader["ACT_CODE"]), Convert.ToString(reader["ACT_NAME"])));
                                }
                            }
                        }
                    }
                }
                response.data = dropdown;
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

        public MyHttpResponseMessage GetPayrollMonths()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> dropdown = new List<object>();
                HashSet<string> distinctMonths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = @"SELECT PAYROLL_MONTH, MAX(ADD_DATE) AS LAST_DATE
                                     FROM TBL_SALARY_GENERATION
                                     WHERE ISNULL(DLT,'T') = 'T' AND PAYROLL_MONTH IS NOT NULL
                                     GROUP BY PAYROLL_MONTH
                                     ORDER BY MAX(ADD_DATE) DESC";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string monthText = FormatPayrollMonth(reader["PAYROLL_MONTH"]);
                        if (!String.IsNullOrWhiteSpace(monthText) && distinctMonths.Add(monthText))
                        {
                            dropdown.Add(new { key = monthText, value = monthText });
                        }
                    }
                    reader.Close();
                }
                response.data = dropdown;
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

        public MyHttpResponseMessage GetEmployeesByPayrollMonth(string payrollMonth)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> rows = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = @"SELECT EMPLOYEE_CODE, EMP_NAME, GROSS_SALARY, TOTAL_ADVANCE, LOAN_INSTALLMENT, NET_SALARY, PAYROLL_MONTH
                                     FROM TBL_SALARY_GENERATION
                                     WHERE ISNULL(DLT,'T') = 'T'
                                     ORDER BY EMP_NAME";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string rowMonth = FormatPayrollMonth(reader["PAYROLL_MONTH"]);
                        if (!String.Equals(rowMonth, payrollMonth, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        rows.Add(new
                        {
                            key = reader["EMPLOYEE_CODE"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EMPLOYEE_CODE"]),
                            value = Convert.ToString(reader["EMP_NAME"]),
                            EMP_CODE = reader["EMPLOYEE_CODE"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EMPLOYEE_CODE"]),
                            EMP_NAME = Convert.ToString(reader["EMP_NAME"]),
                            GROSS_SALARY = reader["GROSS_SALARY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["GROSS_SALARY"]),
                            ADVANCE = reader["TOTAL_ADVANCE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TOTAL_ADVANCE"]),
                            LOAN = reader["LOAN_INSTALLMENT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["LOAN_INSTALLMENT"]),
                            NET_SALARY = reader["NET_SALARY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["NET_SALARY"])
                        });
                    }
                    reader.Close();
                }
                response.data = rows;
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

        public MyHttpResponseMessage QuickSearch(MyHttpResponseMessage Menu, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var menu = (Menu)Menu.data;
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();

                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT * FROM (" +
                                   "SELECT A.TRAN_ID, A.DT_CODE, A.V_DATE, A.VOUCHER_NO, " +
                                   "CH.ACT_NAME AS BOOK_TYPE, A.REMARKS, A.EMP_CODE, A.GROSS_SALARY, A.ADVANCE, A.LOAN, A.NET_SALARY, " +
                                   "A.CHQ_NO, A.CHQ_DATE, A.PAYROLL_MONTH, A.ADD_USER_ID, A.ADD_DATE, A.ADD_COMPUTER_NAME," +
                                   "A.ADD_IP_ADDRESS, A.EDIT_USER_ID, A.EDIT_DATE, A.EDIT_COMPUTER_NAME, " +
                                   "A.EDIT_IP_ADDRESS, A.ADD_POSTALCODE, A.EDIT_POSTALCODE, " +
                                   "CASE WHEN A.ASTATUS = 'Y' THEN 'Active' WHEN A.ASTATUS = 'N' THEN 'In-Active' ELSE '' END ASTATUS," +
                                   "SG.EMP_NAME, " +
                                   "ROW_NUMBER() OVER (PARTITION BY A.TRAN_ID ORDER BY A.ADD_DATE) AS ROW_NUM  " +
                                   "FROM " + table + " A " +
                                   "LEFT OUTER JOIN TBL_CHART CH ON CH.ACT_CODE = A.BOOK_TYPE " +
                                   "LEFT OUTER JOIN TBL_SALARY_GENERATION SG ON SG.EMPLOYEE_CODE = A.EMP_CODE " +
                                   "AND SG.PAYROLL_MONTH = FORMAT(A.PAYROLL_MONTH, 'MMMM yyyy') AND SG.DLT = 'T' " +
                                   "WHERE A.DLT = 'T' AND A.BCODE = '" + common.Branch + "' AND A.PERIOD_ID = '" + common.Period + "') T ORDER BY TRAN_ID DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            DT_CODE = Convert.ToInt32(reader["DT_CODE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            BOOK_TYPE = Convert.ToString(reader["BOOK_TYPE"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            EMP_CODE = reader["EMP_CODE"] == DBNull.Value ? null : Convert.ToString(reader["EMP_CODE"]),
                            EMP_NAME = Convert.ToString(reader["EMP_NAME"]),
                            GROSS_SALARY = reader["GROSS_SALARY"] == DBNull.Value ? "0" : Convert.ToString(reader["GROSS_SALARY"]),
                            ADVANCE = reader["ADVANCE"] == DBNull.Value ? "0" : Convert.ToString(reader["ADVANCE"]),
                            LOAN = reader["LOAN"] == DBNull.Value ? "0" : Convert.ToString(reader["LOAN"]),
                            NET_SALARY = reader["NET_SALARY"] == DBNull.Value ? "0" : Convert.ToString(reader["NET_SALARY"]),
                            CHQ_NO = Convert.ToString(reader["CHQ_NO"]),
                            CHQ_DATE = reader["CHQ_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("yyyy-MM-dd"),
                            PAYROLL_MONTH = reader["PAYROLL_MONTH"] == DBNull.Value ? null : Convert.ToDateTime(reader["PAYROLL_MONTH"]).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                            ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                            ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]).ToString("yyyy-MM-dd"),
                            ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                            ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                            EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                            EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("yyyy-MM-dd"),
                            EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                            EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
                            ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                            EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
                        };
                        jsonDataResult.Add(row);
                    }
                    reader.Close();
                }

                response.data = jsonDataResult;
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

        public MyHttpResponseMessage Save(List<SalaryDisbursement> modelRecord, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                if (modelRecord == null || modelRecord.Count == 0)
                {
                    response.msg = "No detail record found to save.";
                    response.msgType = 2;
                    return response;
                }

                string? table = menu.TABLE1;
                if (String.IsNullOrWhiteSpace(table))
                {
                    response.msg = "Table not configured in menu.";
                    response.msgType = 2;
                    return response;
                }

                var ip = common.IPAddress;
                var computerName = common.ComputerName;
                var postalCode = common.PostalCode;
                var userid = common.Username;
                var branch = common.Branch;
                var periodID = common.Period;
                var menuID = common.MenuID;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    try
                    {
                        string query = "", voucherNo = string.Empty, errorMsg = string.Empty;
                        bool IsMasterAdded = true, IsDetailAdded = true, IsNew = false;
                        int code = 0;
                        string now = CommonService.GetDateTime("Pakistan Standard Time");

                        foreach (var item in modelRecord)
                        {
                            try
                            {
                                if (item.CHQ_DATE == null)
                                {
                                    item.CHQ_DATE = item.V_DATE;
                                }

                                string empCode = item.EMP_CODE == null ? "NULL" : item.EMP_CODE.ToString();
                                string grossSalary = item.GROSS_SALARY == null ? "NULL" : item.GROSS_SALARY.Value.ToString(CultureInfo.InvariantCulture);
                                string advance = item.ADVANCE == null ? "NULL" : item.ADVANCE.Value.ToString(CultureInfo.InvariantCulture);
                                string loan = item.LOAN == null ? "NULL" : item.LOAN.Value.ToString(CultureInfo.InvariantCulture);
                                string netSalary = item.NET_SALARY == null ? "NULL" : item.NET_SALARY.Value.ToString(CultureInfo.InvariantCulture);
                                string payrollMonth = item.PAYROLL_MONTH == null ? "NULL" : "'" + item.PAYROLL_MONTH.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string chqDate = item.CHQ_DATE == null ? "NULL" : "'" + item.CHQ_DATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                string bookType = item.BOOK_TYPE == null ? "NULL" : item.BOOK_TYPE.ToString();
                                string vDate = item.V_DATE == null ? "NULL" : "'" + item.V_DATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                                if (item.TRAN_ID == null || item.TRAN_ID == 0)
                                {
                                    IsNew = true;
                                    if (code == 0)
                                    {
                                        code = GenerateNextId(common, command, menu);
                                        if (code <= 0)
                                        {
                                            IsMasterAdded = false;
                                            errorMsg = "Unable to generate transaction ID.";
                                            break;
                                        }
                                        voucherNo = GenerateVoucherNo(common, code, now, menu);
                                        if (String.IsNullOrWhiteSpace(voucherNo))
                                        {
                                            voucherNo = "SD/" + Convert.ToDateTime(now).ToString("yy-MM") + "/" + code.ToString().PadLeft(5, '0');
                                        }
                                    }

                                    item.DT_CODE = GenerateNextDetailId(command, menu);
                                    if (item.DT_CODE > 0)
                                    {
                                        query = $"INSERT INTO {table} " +
                                                "(TRAN_ID, V_DATE, VOUCHER_NO, BOOK_TYPE, REMARKS, BCODE, PERIOD_ID, " +
                                                "ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, " +
                                                "EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS, MENU_ID, " +
                                                "DLT, DT_CODE, EMP_CODE, GROSS_SALARY, ADVANCE, LOAN, NET_SALARY, CHQ_NO, CHQ_DATE, DT_DESC, PAYROLL_MONTH) " +
                                                $"VALUES " +
                                                $"('{code}', {vDate}, '{voucherNo}', {bookType}, {SqlValue(item.REMARKS)}, '{branch}', " +
                                                $"'{periodID}', '{userid}', '{now}', " +
                                                $"'{computerName}', '{ip}', " +
                                                $"'{userid}', '{now}', '{computerName}', " +
                                                $"'{ip}', '{postalCode}', '{postalCode}', " +
                                                $"'{item.ASTATUS}', '{menuID}', 'T', '{item.DT_CODE}', " +
                                                $"{empCode}, {grossSalary}, {advance}, {loan}, {netSalary}, " +
                                                $"{SqlValue(item.CHQ_NO)}, {chqDate}, {SqlValue(item.DT_DESC)}, {payrollMonth})";
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        IsDetailAdded = false;
                                        errorMsg = "Unable to generate detail ID.";
                                        break;
                                    }
                                }
                                else
                                {
                                    if (item.DT_CODE == null || item.DT_CODE == 0)
                                    {
                                        item.DT_CODE = GenerateNextDetailId(command, menu);
                                        if (item.DT_CODE > 0)
                                        {
                                            query = $"INSERT INTO {table} " +
                                                    "(TRAN_ID, V_DATE, VOUCHER_NO, BOOK_TYPE, REMARKS, BCODE, PERIOD_ID, " +
                                                    "ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, " +
                                                    "EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS, MENU_ID, " +
                                                    "DLT, DT_CODE, EMP_CODE, GROSS_SALARY, ADVANCE, LOAN, NET_SALARY, CHQ_NO, CHQ_DATE, DT_DESC, PAYROLL_MONTH) " +
                                                    $"VALUES " +
                                                    $"('{item.TRAN_ID}', {vDate}, {SqlValue(item.VOUCHER_NO)}, {bookType}, {SqlValue(item.REMARKS)}, '{branch}', " +
                                                    $"'{periodID}', '{userid}', '{now}', " +
                                                    $"'{computerName}', '{ip}', " +
                                                    $"'{userid}', '{now}', '{computerName}', " +
                                                    $"'{ip}', '{postalCode}', '{postalCode}', " +
                                                    $"'{item.ASTATUS}', '{menuID}', 'T', '{item.DT_CODE}', " +
                                                    $"{empCode}, {grossSalary}, {advance}, {loan}, {netSalary}, " +
                                                    $"{SqlValue(item.CHQ_NO)}, {chqDate}, {SqlValue(item.DT_DESC)}, {payrollMonth})";
                                            command.CommandText = query;
                                            command.ExecuteNonQuery();
                                        }
                                        else
                                        {
                                            IsDetailAdded = false;
                                            errorMsg = "Unable to generate detail ID.";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        query = $"UPDATE {table} SET DLT = 'F' " +
                                               $"WHERE TRAN_ID = '{item.TRAN_ID}' AND DT_CODE = '{item.DT_CODE}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();

                                        query = $"UPDATE {table} SET " +
                                                $"V_DATE = {vDate}, " +
                                                $"BOOK_TYPE = {bookType}, " +
                                                $"REMARKS = {SqlValue(item.REMARKS)}, " +
                                                $"EDIT_USER_ID = '{userid}', " +
                                                $"EDIT_DATE = '{now}', " +
                                                $"EDIT_COMPUTER_NAME = '{computerName}', " +
                                                $"EDIT_IP_ADDRESS = '{ip}', " +
                                                $"EDIT_POSTALCODE = '{postalCode}', " +
                                                $"ASTATUS = '{item.ASTATUS}', " +
                                                $"DLT = 'T', " +
                                                $"EMP_CODE = {empCode}, " +
                                                $"GROSS_SALARY = {grossSalary}, " +
                                                $"ADVANCE = {advance}, " +
                                                $"LOAN = {loan}, " +
                                                $"NET_SALARY = {netSalary}, " +
                                                $"CHQ_NO = {SqlValue(item.CHQ_NO)}, " +
                                                $"CHQ_DATE = {chqDate}, " +
                                                $"DT_DESC = {SqlValue(item.DT_DESC)}, " +
                                                $"PAYROLL_MONTH = {payrollMonth} " +
                                                $"WHERE TRAN_ID = {item.TRAN_ID} AND DT_CODE = '{item.DT_CODE}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                IsDetailAdded = false;
                                errorMsg = GetErrorMessage(ex);
                                break;
                            }
                        }

                        if (IsMasterAdded && IsDetailAdded)
                        {
                            transaction.Commit();
                            response.data = new
                            {
                                code = IsNew ? code : 0,
                                voucherNo = IsNew ? voucherNo : "",
                            };
                            response.msgType = 1;
                            response.msg = IsNew ? "Record Added Successfully" : "Record Updated Successfully";
                        }
                        else
                        {
                            transaction.Rollback();
                            response.data = "";
                            response.msg = String.IsNullOrWhiteSpace(errorMsg) ? "Something went wrong! please try again later." : errorMsg;
                            response.msgType = 2;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        response.msg = GetErrorMessage(ex);
                        response.msgType = 2;
                    }
                }
            }
            catch (Exception ex)
            {
                response.msgType = 2;
                response.msg = GetErrorMessage(ex);
            }
            return response;
        }

        public MyHttpResponseMessage GetSalaryDisbursementByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT TOP 1 TRAN_ID, V_DATE, VOUCHER_NO, BOOK_TYPE, REMARKS, ASTATUS, PAYROLL_MONTH " +
                                   "FROM " + table + " " +
                                   "WHERE DLT = 'T' AND TRAN_ID = '" + code + "' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "' ORDER BY DT_CODE";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            BOOK_TYPE = reader["BOOK_TYPE"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["BOOK_TYPE"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            PAYROLL_MONTH = reader["PAYROLL_MONTH"] == DBNull.Value ? null : Convert.ToDateTime(reader["PAYROLL_MONTH"]).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                        };
                        jsonDataResult.Add(row);
                    }
                    reader.Close();
                }

                response.data = jsonDataResult;
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

        public MyHttpResponseMessage GetSalaryDisbursementDetailsByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = @$"SELECT C.TRAN_ID, C.DT_CODE, C.EMP_CODE, C.GROSS_SALARY, C.ADVANCE, C.LOAN, C.NET_SALARY,
                                    C.CHQ_NO, C.CHQ_DATE, C.DT_DESC, SG.EMP_NAME
                                    FROM {table} C
                                    LEFT OUTER JOIN TBL_SALARY_GENERATION SG ON SG.EMPLOYEE_CODE = C.EMP_CODE
                                    AND SG.PAYROLL_MONTH = FORMAT(C.PAYROLL_MONTH, 'MMMM yyyy') AND SG.DLT = 'T'
                                    WHERE C.DLT='T' AND C.TRAN_ID='{code}' AND C.BCODE='{common.Branch}' AND C.PERIOD_ID='{common.Period}'
                                    ORDER BY C.DT_CODE DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            EMP_CODE = reader["EMP_CODE"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["EMP_CODE"]),
                            EMP_NAME = Convert.ToString(reader["EMP_NAME"]),
                            GROSS_SALARY = reader["GROSS_SALARY"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["GROSS_SALARY"]),
                            ADVANCE = reader["ADVANCE"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["ADVANCE"]),
                            LOAN = reader["LOAN"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["LOAN"]),
                            NET_SALARY = reader["NET_SALARY"] == DBNull.Value ? (double?)null : Convert.ToDouble(reader["NET_SALARY"]),
                            CHQ_NO = Convert.ToString(reader["CHQ_NO"]),
                            CHQ_DATE = reader["CHQ_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["CHQ_DATE"]).ToString("yyyy-MM-dd"),
                            DT_DESC = Convert.ToString(reader["DT_DESC"]),
                        };
                        jsonDataResult.Add(row);
                    }
                    reader.Close();
                }
                response.data = jsonDataResult;
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

        public MyHttpResponseMessage Delete(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? table = menu.TABLE1;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    response.msgType = 1;
                    response.msg = "Record Deleted Successfully";
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage DeleteSalaryDisbursementDetailByCode(int tranID, int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                var branch = common.Branch;
                var period = common.Period;
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F' " +
                                   $"WHERE TRAN_ID = '{tranID}' AND DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();

                    response.msgType = 1;
                    response.msg = "Record Deleted Successfully";
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetPayslipData(int tranId, int dtCode, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = $@"SELECT TOP 1
                        P.TRAN_ID, P.DT_CODE, P.VOUCHER_NO, P.V_DATE, P.EMP_CODE, P.PAYROLL_MONTH,
                        P.GROSS_SALARY AS PAY_GROSS, P.ADVANCE AS PAY_ADVANCE, P.LOAN AS PAY_LOAN, P.NET_SALARY AS PAY_NET,
                        CH.ACT_NAME AS PAYMENT_MODE,
                        SG.EMP_NAME, SG.EMPLOYEE_CODE, SG.DEPARTMENT, SG.DESIGNATION, SG.PAYROLL_MONTH AS SG_PAYROLL_MONTH,
                        SG.WORKING_DAYS, SG.PRESENT_DAYS, SG.SUNDAY_DAYS, SG.ABSENT_DAYS,
                        SG.TOTAL_LATE_HOURS, SG.AVG_IN_TIME,
                        SG.BASIC_SALARY, SG.HOUSE_ALLOWANCE, SG.MEDICAL_ALLOWANCE, SG.CONVEYANCE_ALLOWANCE,
                        SG.OVERTIME, SG.BONUS, SG.GROSS_SALARY, SG.PER_DAY_SALARY, SG.ABSENT_DEDUCTION,
                        SG.INCOME_TAX, SG.TOTAL_ADVANCE, SG.LOAN_INSTALLMENT, SG.NET_SALARY,
                        E.EMPLOYEE_CODE AS EMP_REG_CODE, E.EMP_PIC,
                        LTRIM(RTRIM(ISNULL(E.FIRST_NAME,'') + ' ' + ISNULL(E.LAST_NAME,''))) AS EMP_REG_NAME,
                        D.GROUP_NAME AS EMP_DEPARTMENT, G.GROUP_NAME AS EMP_DESIGNATION
                    FROM {table} P
                    LEFT OUTER JOIN TBL_CHART CH ON CH.ACT_CODE = P.BOOK_TYPE
                    LEFT OUTER JOIN TBL_SALARY_GENERATION SG ON SG.EMPLOYEE_CODE = P.EMP_CODE
                        AND SG.DLT = 'T'
                        AND (
                            SG.PAYROLL_MONTH = FORMAT(P.PAYROLL_MONTH, 'MMMM yyyy')
                            OR TRY_CONVERT(datetime, SG.PAYROLL_MONTH) = P.PAYROLL_MONTH
                            OR FORMAT(TRY_CONVERT(datetime, CONVERT(nvarchar(100), SG.PAYROLL_MONTH)), 'MMMM yyyy') = FORMAT(P.PAYROLL_MONTH, 'MMMM yyyy')
                        )
                    LEFT OUTER JOIN TBL_EMP_REG E ON (E.ID = P.EMP_CODE OR E.EMPLOYEE_CODE = CONVERT(nvarchar(50), P.EMP_CODE)) AND E.DLT = 'T'
                    LEFT OUTER JOIN TBL_DEPARTMENTS D ON D.GROUP_CODE = E.DEPARTMENT_ID
                    LEFT OUTER JOIN TBL_DESIGNATION G ON G.GROUP_CODE = E.DESIGNATION_ID
                    WHERE P.DLT = 'T' AND P.TRAN_ID = '{tranId}' AND P.DT_CODE = '{dtCode}'
                      AND P.BCODE = '{common.Branch}' AND P.PERIOD_ID = '{common.Period}'";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        response.msg = "Payslip record not found. Please save the voucher first.";
                        response.msgType = 2;
                        return response;
                    }

                    DateTime? payrollMonth = reader["PAYROLL_MONTH"] == DBNull.Value ? null : Convert.ToDateTime(reader["PAYROLL_MONTH"]);
                    string payrollText = FormatPayrollMonth(reader["SG_PAYROLL_MONTH"]);
                    if (String.IsNullOrWhiteSpace(payrollText) && payrollMonth != null)
                    {
                        payrollText = payrollMonth.Value.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                    }

                    string periodText = "";
                    if (payrollMonth != null)
                    {
                        var start = new DateTime(payrollMonth.Value.Year, payrollMonth.Value.Month, 1);
                        var end = start.AddMonths(1).AddDays(-1);
                        periodText = start.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) + " – " + end.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
                    }

                    double gross = reader["GROSS_SALARY"] == DBNull.Value
                        ? (reader["PAY_GROSS"] == DBNull.Value ? 0 : Convert.ToDouble(reader["PAY_GROSS"]))
                        : Convert.ToDouble(reader["GROSS_SALARY"]);
                    double advance = reader["PAY_ADVANCE"] == DBNull.Value
                        ? (reader["TOTAL_ADVANCE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TOTAL_ADVANCE"]))
                        : Convert.ToDouble(reader["PAY_ADVANCE"]);
                    double loan = reader["PAY_LOAN"] == DBNull.Value
                        ? (reader["LOAN_INSTALLMENT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["LOAN_INSTALLMENT"]))
                        : Convert.ToDouble(reader["PAY_LOAN"]);
                    double absentDeduction = reader["ABSENT_DEDUCTION"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ABSENT_DEDUCTION"]);
                    double incomeTax = reader["INCOME_TAX"] == DBNull.Value ? 0 : Convert.ToDouble(reader["INCOME_TAX"]);
                    double totalDeductions = absentDeduction + advance + loan + incomeTax;
                    double net = reader["PAY_NET"] == DBNull.Value
                        ? (reader["NET_SALARY"] == DBNull.Value ? Math.Max(gross - totalDeductions, 0) : Convert.ToDouble(reader["NET_SALARY"]))
                        : Convert.ToDouble(reader["PAY_NET"]);

                    string empName = Convert.ToString(reader["EMP_NAME"]);
                    if (String.IsNullOrWhiteSpace(empName))
                    {
                        empName = Convert.ToString(reader["EMP_REG_NAME"]);
                    }
                    string empCodeDisplay = Convert.ToString(reader["EMP_REG_CODE"]);
                    if (String.IsNullOrWhiteSpace(empCodeDisplay))
                    {
                        empCodeDisplay = reader["EMPLOYEE_CODE"] == DBNull.Value ? Convert.ToString(reader["EMP_CODE"]) : Convert.ToString(reader["EMPLOYEE_CODE"]);
                    }
                    string department = Convert.ToString(reader["DEPARTMENT"]);
                    if (String.IsNullOrWhiteSpace(department))
                    {
                        department = Convert.ToString(reader["EMP_DEPARTMENT"]);
                    }
                    string designation = Convert.ToString(reader["DESIGNATION"]);
                    if (String.IsNullOrWhiteSpace(designation))
                    {
                        designation = Convert.ToString(reader["EMP_DESIGNATION"]);
                    }

                    string avgInTime = "";
                    if (reader["AVG_IN_TIME"] != DBNull.Value)
                    {
                        if (reader["AVG_IN_TIME"] is TimeSpan ts)
                        {
                            avgInTime = DateTime.Today.Add(ts).ToString("hh:mm tt");
                        }
                        else
                        {
                            avgInTime = Convert.ToString(reader["AVG_IN_TIME"]) ?? "";
                        }
                    }

                    var payslip = new SalaryDisbursementPayslip
                    {
                        TRAN_ID = Convert.ToInt32(reader["TRAN_ID"]),
                        DT_CODE = Convert.ToInt32(reader["DT_CODE"]),
                        VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                        V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]),
                        PAYROLL_MONTH = payrollMonth,
                        PAYROLL_MONTH_TEXT = payrollText,
                        PAYROLL_PERIOD = periodText,
                        PAYMENT_MODE = String.IsNullOrWhiteSpace(Convert.ToString(reader["PAYMENT_MODE"])) ? "Cash" : Convert.ToString(reader["PAYMENT_MODE"]),
                        EMP_CODE = reader["EMP_CODE"] == DBNull.Value ? null : Convert.ToInt32(reader["EMP_CODE"]),
                        EMPLOYEE_CODE = empCodeDisplay,
                        EMP_NAME = empName,
                        EMP_PIC = Convert.ToString(reader["EMP_PIC"]),
                        DEPARTMENT = department,
                        DESIGNATION = designation,
                        WORKING_DAYS = reader["WORKING_DAYS"] == DBNull.Value ? null : Convert.ToDouble(reader["WORKING_DAYS"]),
                        PRESENT_DAYS = reader["PRESENT_DAYS"] == DBNull.Value ? null : Convert.ToDouble(reader["PRESENT_DAYS"]),
                        SUNDAY_DAYS = reader["SUNDAY_DAYS"] == DBNull.Value ? null : Convert.ToDouble(reader["SUNDAY_DAYS"]),
                        ABSENT_DAYS = reader["ABSENT_DAYS"] == DBNull.Value ? null : Convert.ToDouble(reader["ABSENT_DAYS"]),
                        TOTAL_LATE_HOURS = Convert.ToString(reader["TOTAL_LATE_HOURS"]),
                        AVG_IN_TIME = avgInTime,
                        BASIC_SALARY = reader["BASIC_SALARY"] == DBNull.Value ? null : Convert.ToDouble(reader["BASIC_SALARY"]),
                        HOUSE_ALLOWANCE = reader["HOUSE_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["HOUSE_ALLOWANCE"]),
                        MEDICAL_ALLOWANCE = reader["MEDICAL_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["MEDICAL_ALLOWANCE"]),
                        CONVEYANCE_ALLOWANCE = reader["CONVEYANCE_ALLOWANCE"] == DBNull.Value ? null : Convert.ToDouble(reader["CONVEYANCE_ALLOWANCE"]),
                        OVERTIME = reader["OVERTIME"] == DBNull.Value ? null : Convert.ToDouble(reader["OVERTIME"]),
                        BONUS = reader["BONUS"] == DBNull.Value ? null : Convert.ToDouble(reader["BONUS"]),
                        GROSS_SALARY = gross,
                        PER_DAY_SALARY = reader["PER_DAY_SALARY"] == DBNull.Value ? null : Convert.ToDouble(reader["PER_DAY_SALARY"]),
                        ABSENT_DEDUCTION = absentDeduction,
                        INCOME_TAX = incomeTax,
                        ADVANCE = advance,
                        LOAN = loan,
                        TOTAL_DEDUCTIONS = totalDeductions,
                        NET_SALARY = net,
                        NET_SALARY_WORDS = AmountToWords(net)
                    };

                    reader.Close();
                    response.data = payslip;
                    response.msg = "";
                    response.msgType = 1;
                }
            }
            catch (Exception ex)
            {
                response.msg = GetErrorMessage(ex);
                response.msgType = 2;
            }
            return response;
        }

        private string AmountToWords(double amount)
        {
            try
            {
                long rupees = (long)Math.Floor(amount);
                int paisa = (int)Math.Round((amount - rupees) * 100);
                string words = NumberToWords(rupees) + " Rupees";
                if (paisa > 0)
                {
                    words += " and " + NumberToWords(paisa) + " Paisa";
                }
                return words + " Only";
            }
            catch
            {
                return "";
            }
        }

        private string NumberToWords(long number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";
            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }
            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "") words += "and ";
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += "-" + unitsMap[number % 10];
                }
            }
            return words.Trim();
        }

        private int GenerateNextId(Common common, SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE1;
                string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {table} WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
            }
            return 0;
        }

        private string GenerateVoucherNo(Common common, int code, string vDate, Menu menu)
        {
            try
            {
                string? prefix = menu.PERFIX, shortName = string.Empty;
                int voucherLength = 5;
                if (!String.IsNullOrWhiteSpace(menu.VOUCHER_LEN))
                {
                    int.TryParse(menu.VOUCHER_LEN, out voucherLength);
                    if (voucherLength <= 0) voucherLength = 5;
                }
                if (String.IsNullOrWhiteSpace(prefix))
                {
                    prefix = "SD";
                }

                var branchData = _branchRepository.GetBranchByCode(common.Branch);
                if (branchData.data != null)
                {
                    var branch = (Branch)branchData.data;
                    shortName = branch.B_SHORT_NAME;
                }
                if (String.IsNullOrWhiteSpace(shortName))
                {
                    shortName = "BR";
                }

                if (code > 0)
                {
                    string paddedVoucherValue = code.ToString().PadLeft(voucherLength, '0');
                    return $"{shortName}/{prefix}/{Convert.ToDateTime(vDate).ToString("yy-MM")}/{paddedVoucherValue}";
                }
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        private int GenerateNextDetailId(SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE1;
                string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM {table}";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception)
            {
            }
            return 0;
        }

        private string SqlValue(string? value)
        {
            if (value == null)
            {
                return "NULL";
            }
            return "'" + value.Replace("'", "''") + "'";
        }

        private string FormatPayrollMonth(object? rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
            {
                return string.Empty;
            }

            if (rawValue is DateTime dateValue)
            {
                return dateValue.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            }

            string rawText = Convert.ToString(rawValue)?.Trim() ?? string.Empty;
            if (String.IsNullOrWhiteSpace(rawText))
            {
                return string.Empty;
            }

            if (DateTime.TryParse(rawText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDate)
                || DateTime.TryParse(rawText, out parsedDate))
            {
                return parsedDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            }

            return rawText;
        }

        private string GetErrorMessage(Exception ex)
        {
            string _catchMessage = ex.Message;
            if (ex.InnerException != null)
            {
                _catchMessage += "<br/>" + ex.InnerException.Message;
            }
            return _catchMessage;
        }
    }
}
