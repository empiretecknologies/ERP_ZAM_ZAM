using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class BillOfMaterialRepository : IBillOfMaterialRepository
    {
        public IBranchRepository _branchRepository { get; set; }
        public IMenuService _menuRepository { get; set; }
        public BillOfMaterialRepository(IBranchRepository branchRepository, IMenuService menuRepository)
        {
            _branchRepository = branchRepository;
            _menuRepository = menuRepository;
        }

        public MyHttpResponseMessage QuickSearch(Common common, Menu menu)
		{
			MyHttpResponseMessage response = new MyHttpResponseMessage();
			try
			{
				string? table = menu.TABLE1;
				List<object> jsonDataResult = new List<object>();
				using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
				{
                    string query = "SELECT A.TRAN_ID, A.V_DATE, A.VOUCHER_NO,IM.ITEM_CODE," +
                                    "IM.ITEM_NAME, A.REF, A.COST, P.GROUP_CODE AS PROCESS_CODE, P.GROUP_NAME AS PROCESS, A.BQTY AS BATCH_QTY, A.REMARKS," +
                                    "A.ADD_USER_ID, A.ADD_DATE, A.ADD_COMPUTER_NAME, A.ADD_IP_ADDRESS," +
                                    "A.EDIT_USER_ID, A.EDIT_DATE, A.EDIT_COMPUTER_NAME, A.EDIT_IP_ADDRESS," +
                                    "A.ADD_POSTALCODE, A.EDIT_POSTALCODE, CASE WHEN A.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS " +
									$"FROM {table} A " +
                                    "LEFT OUTER JOIN TBL_ITEMSMASTER IM ON A.ITEM_CODE = IM.ITEM_CODE " +
                                    "LEFT OUTER JOIN TBL_PROCESS P ON A.PROCESS = P.GROUP_CODE " +
                                    $"WHERE A.DLT = 'T' AND A.ASTATUS = 'Y' AND A.BCODE = '" + common.Branch + "' AND A.PERIOD_ID = '" + common.Period + "' ORDER BY A.TRAN_ID DESC";
                    SqlCommand command = new SqlCommand(query, connection);
					connection.Open();
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						var row = new
						{
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            ITEM_NAME = Convert.ToString(reader["ITEM_NAME"]),
                            REF = Convert.ToString(reader["REF"]),
                            COST = Convert.ToString(reader["COST"]),
                            PROCESS_CODE = Convert.ToInt32(reader["PROCESS_CODE"]),
                            PROCESS = Convert.ToString(reader["PROCESS"]),
                            BQTY = Convert.ToString(reader["BATCH_QTY"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                            ADD_DATE = Convert.ToDateTime(reader["ADD_DATE"]),
                            ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                            ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                            EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                            EDIT_DATE = Convert.ToDateTime(reader["EDIT_DATE"]),
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
            catch (Exception ex)
            {

            }
            return 0;
        }

        private string GenerateVoucherNo(Common common, int code, string vDate, Menu menu)
        {
            try
            {
                string? prefix = menu.PERFIX, shortName = string.Empty;
                int voucherLength = Convert.ToInt32(menu.VOUCHER_LEN);
                var branchData = _branchRepository.GetBranchByCode(common.Branch);
                if (branchData.data != null)
                {
                    var branch = (Branch)branchData.data;
                    shortName = branch.B_SHORT_NAME;
                }

                if (!String.IsNullOrWhiteSpace(shortName) && !String.IsNullOrWhiteSpace(prefix) && voucherLength > 0 && code > 0)
                {
                    string paddedVoucherValue = code.ToString().PadLeft(voucherLength, '0');
                    return $"{shortName}/{prefix}/{Convert.ToDateTime(vDate).ToString("yy-MM")}/{paddedVoucherValue}";
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        private int GenerateNextDetailId(SqlCommand command, Menu menu)
        {
            try
            {
                string? table = menu.TABLE2;
                string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM {table}";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public MyHttpResponseMessage Save(CustomBillOfMaterial modelRecord, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1, detailTable = menu.TABLE2;
                var ip = common.IPAddress;
                var computerName = common.ComputerName;
                var postalCode = common.PostalCode;
                var username = common.Username;
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
                        string query = "", detailQuery = "", voucherNo = string.Empty;
                        bool IsMasterAdded = true, IsNew = false;
                        int code = 0;

                        if (modelRecord.Master.TRAN_ID == null || modelRecord.Master.TRAN_ID == 0)
                        {
                            IsNew = true;
                            code = GenerateNextId(common, command, menu);

                            if (code > 0)
                            {
                                modelRecord.Master.TRAN_ID = code;
                                voucherNo = GenerateVoucherNo(common, code, CommonService.GetDateTime("Pakistan Standard Time"), menu);
                                if (String.IsNullOrWhiteSpace(voucherNo))
                                {
                                    IsMasterAdded = false;
                                }
                            }
                            else
                            {
                                IsMasterAdded = false;
                            }

                            query = $"INSERT INTO {table} " +
                                    "(TRAN_ID, V_DATE, VOUCHER_NO, ITEM_CODE, REF, " +
                                    "COST, PROCESS, BQTY, REMARKS, BCODE, " +
                                    "PERIOD_ID, ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, " +
                                    "ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, " +
                                    "EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS, " +
                                    "MENU_ID, DLT) " +
                                    $"VALUES " +
                                    $"('{code}', '{modelRecord.Master.V_DATE}', '{voucherNo}'," +
                                    $"'{modelRecord.Master.ITEM_CODE}', '{modelRecord.Master.REF}', '{modelRecord.Master.COST}', " +
                                    $"'{modelRecord.Master.PROCESS}', '{modelRecord.Master.BQTY}', " +
                                    $"'{modelRecord.Master.REMARKS}', '{branch}', '{periodID}'," +
                                    $"'{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}'," +
                                    $"'{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}'," +
                                    $"'{computerName}', '{ip}', " +
                                    $"'{postalCode}', '{postalCode}', " +
                                    $"'{modelRecord.Master.ASTATUS}', '{menuID}', 'T')";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            query = $"UPDATE {table} " +
                                    $"SET V_DATE = '{modelRecord.Master.V_DATE}', " +
                                    $"ITEM_CODE = '{modelRecord.Master.ITEM_CODE}', " +
                                    $"REF = '{modelRecord.Master.REF}', " +
                                    $"COST = '{modelRecord.Master.COST}', " +
                                    $"PROCESS = '{modelRecord.Master.PROCESS}', " +
                                    $"BQTY = '{modelRecord.Master.BQTY}', " +
                                    $"REMARKS = '{modelRecord.Master.REMARKS}', " +
                                    $"EDIT_USER_ID = '{modelRecord.Master.EDIT_USER_ID}', " +
                                    $"EDIT_DATE = '{modelRecord.Master.EDIT_DATE}', " +
                                    $"EDIT_COMPUTER_NAME = '{modelRecord.Master.EDIT_COMPUTER_NAME}', " +
                                    $"EDIT_IP_ADDRESS = '{modelRecord.Master.EDIT_IP_ADDRESS}', " +
                                    $"EDIT_POSTALCODE = '{modelRecord.Master.EDIT_POSTALCODE}', " +
                                    $"ASTATUS = '{modelRecord.Master.ASTATUS}' " +
                                    $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }

                        var isDetailAdded = true;

                        if (modelRecord.Detail.Count > 0)
                        {
                            detailQuery = $"UPDATE {detailTable} SET DLT = 'F'" +
                            $" WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";
                            command.CommandText = detailQuery;
                            command.ExecuteNonQuery();
                        }
                        foreach (var item in modelRecord.Detail.ToList())
                        {
                            try
                            {
                                if (item.DT_CODE == null || item.DT_CODE == 0)
                                {
                                    int detailCode = GenerateNextDetailId(command, menu);
                                    if (detailCode > 0)
                                    {
                                        detailQuery = $"INSERT INTO {detailTable} " +
                                                       "(TRAN_ID, DT_CODE, ITEM_CODE, QTY, UNIT, " +
                                                       "RATE, AMT, LOSS, DT_DESC, " +
                                                       "BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE, " +
                                                       "ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, " +
                                                       "EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, " +
                                                       "ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT) " +
                                                       $"VALUES " +
                                                       $"('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.ITEM_CODE}', " +
                                                       $"'{item.QTY}', '{item.UNIT}', " +
                                                       $"'{item.RATE}', '{item.AMT}', '{item.LOSS}', " +
                                                       $"'{item.DT_DESC}', '{branch}', '{periodID}', '{username}', " +
                                                       $"'{CommonService.GetDateTime("Pakistan Standard Time")}', '{computerName}', " +
                                                       $"'{ip}', '{username}', '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                                       $"'{computerName}', '{ip}', '{postalCode}', " +
                                                       $"'{postalCode}', '{menuID}', 'T')";

                                        command.CommandText = detailQuery;
                                        command.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        isDetailAdded = false;
                                    }
                                }
                                else
                                {
                                    detailQuery = $"UPDATE {detailTable} " +
                                                  $"SET ITEM_CODE = '{item.ITEM_CODE}', " +
                                                  $"QTY = '{item.QTY}', " +
                                                  $"UNIT = '{item.UNIT}', " +
                                                  $"RATE = '{item.RATE}', " +
                                                  $"AMT = '{item.AMT}', " +
                                                  $"LOSS = '{item.LOSS}', " +
                                                  $"DT_DESC = '{item.DT_DESC}', " +
                                                  $"EDIT_USER_ID = '{username}', " +
                                                  $"EDIT_DATE = '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                                  $"EDIT_COMPUTER_NAME = '{computerName}', " +
                                                  $"EDIT_IP_ADDRESS = '{ip}', " +
                                                  $"EDIT_POSTALCODE = '{postalCode}', " +
                                                  $"DLT = 'T', " +
                                                  $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND DT_CODE = '{item.DT_CODE}' AND BCODE = '{branch}' AND PERIOD_ID = '{periodID}'";

                                    command.CommandText = detailQuery;
                                    command.ExecuteNonQuery();
                                }
                            }
                            catch (Exception)
                            {
                                isDetailAdded = false;
                            }
                        }

                        if (IsMasterAdded && isDetailAdded)
                        {
                            transaction.Commit();
                            response.data = new
                            {
                                code = IsNew ? code : modelRecord.Master.TRAN_ID,
                                voucherNo = IsNew ? voucherNo : modelRecord.Master.VOUCHER_NO,
                            };
                            response.msgType = 1;
                            response.msg = IsNew ? "Record Added Successfully" : "Record Updated Successfully";
                        }
                        else
                        {
                            transaction.Rollback();
                            response.data = "";
                            response.msg = "Something went wrong! please try again later.";
                            response.msgType = 2;
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
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msgType = 2;
                response.msg = _catchMessage;
            }
            return response;
        }

        public MyHttpResponseMessage CopyRecord(CopyRecord record, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? table = menu.TABLE1;
                string? table2 = menu.TABLE2;
                string connectionString = new SQLService().getconnstring();

                BillOfMaterial billOfMaterial = new BillOfMaterial();
                List<BillOfMaterialDetail> billOfMaterialDetailList = new List<BillOfMaterialDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        billOfMaterial = new BillOfMaterial
                        {
                            TRAN_ID = 0,
                            V_DATE = record.V_DATE,
                            REF = Convert.ToString(reader["REF"]),
                            COST = Convert.ToDouble(reader["COST"]),
                            LOSS = Convert.ToDouble(reader["LOSS"]),
                            BQTY = Convert.ToDouble(reader["BQTY"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                        };
                    }

                    reader.Close();

                    SqlCommand detail_Command = new SqlCommand(detailQuery, connection);
                    SqlDataReader detail_Reader = detail_Command.ExecuteReader();
                    while (detail_Reader.Read())
                    {
                        var row = new BillOfMaterialDetail
                        {
                            ITEM_CODE = Convert.ToInt32(detail_Reader["ITEM_CODE"]),
                            QTY = Convert.ToDouble(detail_Reader["QTY"]),
                            UNIT = Convert.ToInt32(detail_Reader["UNIT"]),
                            QTY2 = Convert.ToDouble(detail_Reader["QTY2"]),
                            BAL_QTY = Convert.ToDouble(detail_Reader["BAL_QTY"]),
                            RATE = Convert.ToInt32(detail_Reader["RATE"]),
                            AMT = Convert.ToInt32(detail_Reader["AMT"]),
                            DT_DESC = Convert.ToString(detail_Reader["DT_DESC"]),
                            CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"]),
                        };
                        billOfMaterialDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customBillOfMaterial = new CustomBillOfMaterial
                {
                    Master = billOfMaterial,
                    Detail = billOfMaterialDetailList
                };

                response = this.Save(customBillOfMaterial, common, menu);

                if (response.msgType == 1)
                {
                    response.msg = "Record Copied Successfully";
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

        public MyHttpResponseMessage GetBillOfMaterialByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT TRAN_ID, V_DATE, VOUCHER_NO," +
                                   "ITEM_CODE, COST, PROCESS, BQTY, REF, REMARKS, ASTATUS " +
                                   $"FROM {table} WHERE DLT = 'T' AND TRAN_ID = '{code}' AND BCODE = '{common.Branch}' " +
                                   $"AND PERIOD_ID = '{common.Period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            TRAN_ID = Convert.ToString(reader["TRAN_ID"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                            VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                            ITEM_CODE = Convert.ToString(reader["ITEM_CODE"]),
                            COST = Convert.ToString(reader["COST"]),
                            PROCESS = Convert.ToString(reader["PROCESS"]),
                            BQTY = Convert.ToString(reader["BQTY"]),
                            REF = Convert.ToString(reader["REF"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
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

        public MyHttpResponseMessage GetBillOfMaterialDetailByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE2;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT DT_CODE, ITEM_CODE, QTY, " +
                                   "UNIT, RATE, AMT, LOSS, DT_DESC " +
                                   $"FROM {table} WHERE DLT = 'T' AND TRAN_ID = '{code}' AND BCODE = '{common.Branch}' " +
                                   $"AND PERIOD_ID = '{common.Period}' ORDER BY DT_CODE DESC";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            DT_CODE = Convert.ToString(reader["DT_CODE"]),
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            QTY = Convert.ToString(reader["QTY"]),
                            UNIT = Convert.ToInt32(reader["UNIT"]),
                            RATE = Convert.ToString(reader["RATE"]),
                            AMT = Convert.ToString(reader["AMT"]),
                            LOSS = Convert.ToString(reader["LOSS"]),
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

        public MyHttpResponseMessage DeleteBillOfMaterialDetailByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE2;
                var branch = common.Branch;
                var period = common.Period;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"UPDATE {table} SET DLT = 'F'" +
                                   $" WHERE DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    response.msgType = 1;
                    response.msg = "Record Deleted Successfully";
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
        //public MyHttpResponseMessage GetDataForReport(BillOfMaterialRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    BillOfMaterialRDLCReport masterData = new BillOfMaterialRDLCReport();
        //    CustomBillOfMaterialForPrintReport reportData = new CustomBillOfMaterialForPrintReport();
        //    var Menu = _menuRepository.GetMenu(common.MenuID);
        //    string? table = string.Empty, detailTable = string.Empty;
        //    if (Menu.data != null)
        //    {
        //        var menu = (Menu)Menu.data;
        //        table = menu.TABLE1;
        //        detailTable = menu.TABLE2;
        //    }
        //    try
        //    {
        //        string topQuery = "", query = "";
        //        query = @$"Select M.V_DATE , M.VOUCHER_NO , IT.ITEM_NAME AS MITEM_NAME , M.REF , M.COST , M.LOSS ,
        //                TP.GROUP_NAME AS PROCESS , M.BQTY , M.REMARKS , ID.ITEM_NAME AS DITEM_NAME , D.DT_DESC , D.QTY , TU.GROUP_NAME AS UNIT,
        //                D.QTY2 , D.BAL_QTY , D.RATE , D.AMT
        //                from {table} M
        //                LEFT OUTER JOIN {detailTable} D
        //                ON D.TRAN_ID = M.TRAN_ID AND M.BCODE = D.BCODE AND M.PERIOD_ID = D.PERIOD_ID
        //                LEFT OUTER JOIN TBL_ITEMSMASTER IT
        //                ON IT.ITEM_CODE = M.ITEM_CODE 
        //                LEFT OUTER JOIN TBL_ITEMSMASTER ID
        //                ON ID.ITEM_CODE = D.ITEM_CODE 
        //                LEFT OUTER JOIN TBL_PROCESS TP
        //                ON TP.GROUP_CODE = M.PROCESS
        //                LEFT OUTER JOIN TBL_UNIT TU
        //                ON TU.GROUP_CODE = D.UNIT
        //                WHERE M.BCODE = '{common.Branch}' And M.PERIOD_ID = '{common.Period}' AND M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.DLT = 'T' AND D.DLT = 'T'";

        //        masterData.COMPANY_NAME = currentCompany.C_NAME;
        //        masterData.COMPANY_ADDRESS = currentCompany.C_ADDRESS;
        //        masterData.COMPANY_PHONE = currentCompany.C_TEL;
        //        masterData.COMPANY_LOGO = currentCompany.C_LOGO;
        //        masterData.COMPANY_WATER = currentCompany.C_WATER;
        //        masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
        //        masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
        //        masterData.SIG1 = $"{menuDetails.MENU_SIG1}";
        //        masterData.SIG2 = $"{menuDetails.MENU_SIG2}";
        //        masterData.SIG3 = $"{menuDetails.MENU_SIG3}";
        //        masterData.SIG4 = $"{menuDetails.MENU_SIG4}";
        //        masterData.MENU_TERMS = $"{menuDetails.MENU_TERMS}";

        //        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //        {
        //            SqlCommand command = new SqlCommand(query, connection);
        //            connection.Open();
        //            SqlDataReader reader = command.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
        //                masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
        //                masterData.MITEM_NAME = Convert.ToString(reader["MITEM_NAME"]);
        //                masterData.REF = Convert.ToString(reader["REF"]);
        //                masterData.COST = Convert.ToString(reader["COST"]);
        //                masterData.LOSS = Convert.ToString(reader["LOSS"]);
        //                masterData.PROCESS = Convert.ToString(reader["PROCESS"]);
        //                masterData.BQTY = Convert.ToString(reader["BQTY"]);
        //                masterData.REMARKS = Convert.ToString(reader["REMARKS"]);
        //            }
        //            reader.Close();
        //        }
        //        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //        {
        //            SqlCommand command = new SqlCommand(query, connection);
        //            connection.Open();
        //            SqlDataReader reader = command.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                DataRow dataRow = dataTable.NewRow();
        //                dataRow["Item"] = Convert.ToString(reader["DITEM_NAME"]);
        //                dataRow["Description"] = Convert.ToString(reader["DT_DESC"]);
        //                dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
        //                dataRow["Qty"] = Convert.ToInt32(reader["QTY"]);
        //                dataRow["Qty2"] = Convert.ToInt32(reader["Qty2"]);
        //                dataRow["Rate"] = Convert.ToString(reader["Rate"]);
        //                dataRow["Bal_Qty"] = Convert.ToString(reader["BAL_QTY"]);
        //                dataRow["Amount"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
        //                dataTable.Rows.Add(dataRow);
        //            }
        //            reader.Close();
        //        }
        //        reportData.Master = masterData;
        //        reportData.Detail = dataTable;
        //        response.data = reportData;
        //        response.msg = "";
        //        response.msgType = 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        string _catchMessage = ex.Message;
        //        if (ex.InnerException != null)
        //        {
        //            _catchMessage += "<br/>" + ex.InnerException.Message;
        //        }
        //        response.msg = _catchMessage;
        //        response.msgType = 2;
        //    }
        //    return response;
        //}
        public MyHttpResponseMessage GetDataForReport(BillOfMaterialRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            BillOfMaterialRDLCReport masterData = new BillOfMaterialRDLCReport();
            CustomBillOfMaterialForPrintReport reportData = new CustomBillOfMaterialForPrintReport();
            var Menu = _menuRepository.GetMenu(common.MenuID);
            string? table = string.Empty, detailTable = string.Empty, dCType = string.Empty, pickMaster = string.Empty, pickDetail = string.Empty;
            if (Menu.data != null)
            {
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
                detailTable = menu.TABLE2;
                pickMaster = menu.PICK_TABLE_MASTER;
                pickDetail = menu.PICK_TABLE_DETAIL;
                dCType = menu.DCTYPE;
            }
            try
            {
                string query = "";

                if (menuDetails.REPORT_NAME == "BillOfMaterial")
                {


                    //query = $@"EXEC PROC_PRINT '{menuDetails.REPORT_NAME}','{table}','{detailTable}','{pickMaster}','{pickDetail}','{common.Branch}','{common.Period}','{modelRecord.TRAN_ID}','{common.Username}',''";
                    query = $@"EXEC PROC_PRINT '{table}','{detailTable}','','','{common.Branch}','{common.Period}','{modelRecord.TRAN_ID}','','','{menuDetails.REPORT_NAME}'";

                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
                            masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
                            masterData.COMPANY_NAME = reader["C_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["C_NAME"]);
                            masterData.B_NAME = reader["B_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["B_NAME"]);
                            masterData.B_TERMS = reader["B_TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["B_TERMS"]);
                            masterData.COMPANY_ADDRESS = reader["B_ADDRESS"] == DBNull.Value ? "" : Convert.ToString(reader["B_ADDRESS"]);
                            masterData.COMPANY_PHONE = reader["B_TEL"] == DBNull.Value ? "" : Convert.ToString(reader["B_TEL"]);
                            masterData.B_WEBSITE = reader["B_WEBSITE"] == DBNull.Value ? "" : Convert.ToString(reader["B_WEBSITE"]);
                            masterData.EMAIL = reader["EMAIL"] == DBNull.Value ? "" : Convert.ToString(reader["EMAIL"]);
                            masterData.B_GST = reader["B_GST"] == DBNull.Value ? "" : Convert.ToString(reader["B_GST"]);
                            masterData.B_NTN = reader["B_NTN"] == DBNull.Value ? "" : Convert.ToString(reader["B_NTN"]);
                            masterData.SIG1 = reader["MENU_SIG1"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG1"]);
                            masterData.SIG2 = reader["MENU_SIG2"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG2"]); ;
                            masterData.SIG3 = reader["MENU_SIG3"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG3"]); ;
                            masterData.SIG4 = reader["MENU_SIG4"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_SIG4"]); ;
                            masterData.MENU_TERMS = reader["MENU_TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["MENU_TERMS"]); ;
                            masterData.COMPANY_LOGO = currentCompany.C_LOGO;
                            masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                            masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                            masterData.USER = reader["USER_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["USER_NAME"]);
                            masterData.STATUS = Convert.ToString(reader["ASTATUS"]);
                            masterData.MITEM_NAME = reader["MITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["MITEM_NAME"]);
                            masterData.REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]);
                            masterData.COST = reader["COST"] == DBNull.Value ? "" : Convert.ToString(reader["COST"]);
                            masterData.PROCESS = reader["PROCESS"] == DBNull.Value ? "" : Convert.ToString(reader["PROCESS"]);
                            masterData.BQTY = reader["BQTY"] == DBNull.Value ? "": Convert.ToString(reader["BQTY"]); 
                            masterData.REMARKS = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]);
                     
                            //masterData.DEL_DATE = reader["DEL_DATE"] == DBNull.Value ? "" : Convert.ToString(reader["DEL_DATE"]);

                            //masterData.DC_TYPE = dCType;
                        }
                        reader.Close();
                    }
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            DataRow dataRow = dataTable.NewRow();
                            dataRow["ITEM"] = Convert.ToString(reader["DITEM_NAME"]);
                            dataRow["QTY"] = Convert.ToString(reader["QTY"]);
                            dataRow["UNIT"] = Convert.ToString(reader["UNIT"]);
                            dataRow["RATE"] = Convert.ToString(reader["RATE"]);
                            dataRow["AMT"] = Convert.ToString(reader["AMT"]);
                            dataRow["LOSS"] = Convert.ToString(reader["LOSS"]);
                            dataRow["DT_DESC"] = Convert.ToString(reader["DT_DESC"]);
                            dataTable.Rows.Add(dataRow);
                        }
                        reader.Close();
                    }
                }
                reportData.Master = masterData;
                reportData.Detail = dataTable;
                response.data = reportData;
                response.msg = "";
                response.msgType = 1;
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

    }
}














