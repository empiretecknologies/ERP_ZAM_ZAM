using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Runtime.Intrinsics.Arm;
using System;
using System.Data;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public IBranchRepository _branchRepository { get; set; }

        public PurchaseOrderRepository(IMenuRepository menuRepository, IBranchRepository branchRepository)
        {
            _menuRepository = menuRepository;
            _branchRepository = branchRepository;
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
                        string query = "SELECT TRAN_ID, V_DATE, VOUCHER_NO," +
                                       "DEP,REF, POD, TRANSPORT_TYPE, REMARKS, BCODE, PERIOD_ID," +
									   "ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS," +
									   "EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS," +
									   "ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS " +
									   $"FROM {table} WHERE DLT = 'T' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "' ORDER BY TRAN_ID DESC";
                        SqlCommand command = new SqlCommand(query, connection);
						connection.Open();
						SqlDataReader reader = command.ExecuteReader();
						while (reader.Read())
						{
							var row = new
							{
								ID = Convert.ToString(reader["TRAN_ID"]),
                                ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                DEP = Convert.ToString(reader["DEP"]),
                                REF = Convert.ToString(reader["REF"]),
                                POD = Convert.ToString(reader["POD"]),
                                TRANSPORT_TYPE = Convert.ToString(reader["TRANSPORT_TYPE"]),
                                REMARKS = Convert.ToString(reader["REMARKS"]),
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

        public MyHttpResponseMessage GetPurchaseOrderByCode(int code, Common common)
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
                        string query = "SELECT TRAN_ID, V_DATE, VOUCHER_NO, PARTY_CODE, ACT_CODE, DOC, DEP, ORDER_TYPE, TERMS, SCODE, SACODE, COMM_AMT, COMM, COMM_TYPE, " +
                            "REF, POD, TRANSPORT_TYPE, REF_DATE, CURR_CODE, CRATE, WAREHOUSE, DEL_DATE, REMARKS, ASTATUS " +
                                       $"FROM {table} WHERE DLT = 'T' AND TRAN_ID = '{code}' AND BCODE = '{common.Branch}' " +
                                       $"AND PERIOD_ID = '{common.Period}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                ID = Convert.ToString(reader["TRAN_ID"]),
                                ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                DEP = Convert.ToString(reader["DEP"]),
                                REF = Convert.ToString(reader["REF"]),
                                POD = Convert.ToString(reader["POD"]),
                                TRANSPORT_TYPE = Convert.ToString(reader["TRANSPORT_TYPE"]),
                                DOC = Convert.ToString(reader["DOC"]),
                                REMARKS = Convert.ToString(reader["REMARKS"]),
                                PARTY_CODE = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                ORDER_TYPE = Convert.ToString(reader["ORDER_TYPE"]),
                                TERMS = Convert.ToString(reader["TERMS"]),
                                SCODE = $"{Convert.ToString(reader["SCODE"])}{Convert.ToString(reader["SACODE"])}",
                                COMM_AMT = Convert.ToString(reader["COMM_AMT"]),
                                COMM = Convert.ToString(reader["COMM"]),
                                COMM_TYPE = Convert.ToString(reader["COMM_TYPE"]),
                                REF_DATE = reader["REF_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["REF_DATE"]).ToString("yyyy-MM-dd"),
                                CURR_CODE = Convert.ToString(reader["CURR_CODE"]),
                                CRATE = Convert.ToString(reader["CRATE"]),
                                WAREHOUSE = Convert.ToString(reader["WAREHOUSE"]),
                                DEL_DATE = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("yyyy-MM-dd"),

                        }
                        ;
                            jsonDataResult.Add(row);
                        }
                        reader.Close();
                    }

                    response.data = jsonDataResult;
                    response.msg = "";
                    response.msgType = 1;
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

        public MyHttpResponseMessage GetPurchaseOrderDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }
                
                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT DT_CODE, ITEM_CODE, QTY," +
                                       "UNIT, QTY2, BAL_QTY, DEL_DATE, DT_DESC," +
                                       "COLOR, SIZE, GRADE, " +
                                       "PRIOIRTY, REQ_TYPE," +
                                       "BCODE, PERIOD_ID, CHK, RATE, AMT, DISC, DISC_AMT, TAX, TAX_AMT, NET_AMT " +
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
                                QTY2 = Convert.ToString(reader["QTY2"]),
                                BAL_QTY = Convert.ToString(reader["BAL_QTY"]),
                                DT_DESC = Convert.ToString(reader["DT_DESC"]),
                                COLOR = Convert.ToInt32(reader["COLOR"]),
                                SIZE = Convert.ToInt32(reader["SIZE"]),
                                GRADE = Convert.ToInt32(reader["GRADE"]),
                                PRIORITY = Convert.ToString(reader["PRIOIRTY"]),
                                REQ_TYPE = Convert.ToString(reader["REQ_TYPE"]),
                                RATE = Convert.ToString(reader["RATE"]),
                                AMT = Convert.ToString(reader["AMT"]),
                                DISC = Convert.ToString(reader["DISC"]),
                                DISC_AMT = Convert.ToString(reader["DISC_AMT"]),
                                TAX = Convert.ToString(reader["TAX"]),
                                TAX_AMT = Convert.ToString(reader["TAX_AMT"]),
                                NET_AMT = Convert.ToString(reader["NET_AMT"]),
                                CHK = Convert.ToString(reader["CHK"]),
                                //POD = Convert.ToString(reader["POD"]),
                                //ORDER_NO = Convert.ToString(reader["ORDER_NO"]),
                                //PARTY_CODE = Convert.ToInt32(reader["PARTY_CODE"]),
                                //ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                                DEL_DATE = (reader["DEL_DATE"] == DBNull.Value || Convert.ToDateTime(reader["DEL_DATE"]) <= new DateTime(1900, 1, 2)) ? "" : Convert.ToDateTime(reader["DEL_DATE"]).ToString("yyyy-MM-dd"),
                                //SHIP_DATE = (reader["SHIP_DATE"] == DBNull.Value || Convert.ToDateTime(reader["SHIP_DATE"]) <= new DateTime(1900, 1, 2)) ? "" : Convert.ToDateTime(reader["SHIP_DATE"]).ToString("yyyy-MM-dd"),
                                CHK1 = Convert.ToString(reader["CHK"]) == "1" ? true : false,
                                //PARTY_DDL = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",

                            };
                            jsonDataResult.Add(row);
                        }
                        reader.Close();
                    }

                    response.data = jsonDataResult;
                    response.msg = "";
                    response.msgType = 1;
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

        private int GenerateNextId(Common common, SqlCommand command)
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
                    string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {table} WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                    command.CommandText = maxIdQuery;
                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private string GenerateVoucherNo(Common common, int code, string vDate)
        {
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? prefix = string.Empty, shortName = string.Empty;
                int voucherLength = 0;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    voucherLength = Convert.ToInt32(menu.VOUCHER_LEN);
                    prefix = menu.PERFIX;
                }

                var branchData = _branchRepository.GetBranchByCode(common.Branch);
                if (branchData.data != null)
                {
                    var branch = (Branch)branchData.data;
                    shortName = branch.B_SHORT_NAME;
                }

                if (!String.IsNullOrWhiteSpace(shortName) && !String.IsNullOrWhiteSpace(prefix) && voucherLength > 0 && code > 0)
                {
                    //string paddedVoucherValue = "0".ToString().PadLeft(voucherLength - 1, '0') + code;
                    string paddedVoucherValue = code.ToString().PadLeft(voucherLength, '0');
                    return $"{shortName}/{prefix}/{Convert.ToDateTime(vDate).ToString("yy-MM")}/{paddedVoucherValue}";
                }
            }
            catch (Exception ex)
            {

            }
            return string.Empty;
        }

        private int GenerateNextDetailId(Common common, SqlCommand command)
        {
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM {table}";
                    command.CommandText = maxIdQuery;
                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public MyHttpResponseMessage Save(CustomPurchaseOrder modelRecord, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                }

                if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(detailTable))
                {
                    var Ip = common.IPAddress;
                    var Computer = common.ComputerName;
                    var Postal = common.PostalCode;
                    var username = common.Username;
                    var branch = common.Branch;
                    var period = common.Period;
                    var menuID = common.MenuID;
                    string connectionString = new SQLService().getconnstring();
                    List<CustomPartyType> partiesData = DropdownService.CustomPartyTypeDropdownWithAccountCode(common.RoleID, common.RoleType);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();
                        SqlCommand command = connection.CreateCommand();
                        command.Transaction = transaction;
                        try
                        {
                            var partyInformation = partiesData.ToList().Where(p => p.customizedKey == modelRecord.Master.PARTY_CODE).FirstOrDefault();
                            var salesmanInformation = partiesData.ToList().Where(p => p.customizedKey == modelRecord.Master.SCODE).FirstOrDefault();

                            if (partyInformation != null)
                            {
                                modelRecord.Master.PARTY_CODE = Convert.ToString(partyInformation.key);
                                modelRecord.Master.ACT_CODE = partyInformation.accountCode;
                            }

                            if (salesmanInformation != null)
                            {
                                modelRecord.Master.SCODE = Convert.ToString(salesmanInformation.key);
                                modelRecord.Master.SACODE = salesmanInformation.accountCode;
                            }

                            string query = "", detailQuery= "", voucherNo = string.Empty;
                            bool IsMasterAdded = true, IsNew = false;
                            int code = 0;
                            if (modelRecord.Master.TRAN_ID == null || modelRecord.Master.TRAN_ID == 0)
                            {
                                IsNew = true;
                                code = GenerateNextId(common, command);
                                
                                if (code > 0)
                                {
                                    modelRecord.Master.TRAN_ID = code;
                                    voucherNo = GenerateVoucherNo(common, code, CommonService.GetDateTime("Pakistan Standard Time"));
                                    if (String.IsNullOrWhiteSpace(voucherNo))
                                    {
                                        IsMasterAdded = false;
                                    }
                                }
                                else
                                {
                                    IsMasterAdded = false;
                                }

                                query = $"INSERT INTO {table}" +
                                        "(TRAN_ID, V_DATE, VOUCHER_NO, PARTY_CODE, ACT_CODE, DOC, DEP, " +
                                        "ORDER_TYPE, TERMS, SCODE, SACODE," +
                                        "COMM_AMT, COMM, COMM_TYPE, REF, TRANSPORT_TYPE, REF_DATE, CURR_CODE, " +
                                        "CRATE, WAREHOUSE, REMARKS, BCODE, PERIOD_ID," +
                                        "ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME," +
                                        "ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE," +
                                        "EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE," +
                                        "EDIT_POSTALCODE, ASTATUS, MENU_ID, DLT)" +
                                        "VALUES" +
                                        "('" + code + "','" + modelRecord.Master.V_DATE + "','" + voucherNo + "','" + modelRecord.Master.PARTY_CODE + "','" + modelRecord.Master.ACT_CODE + "','" + modelRecord.Master.DOC + "','" + modelRecord.Master.DEP + "'," +
                                        "'" + modelRecord.Master.ORDER_TYPE + "','" + modelRecord.Master.TERMS + "','" + modelRecord.Master.SCODE + "','" + modelRecord.Master.SACODE  + "'," +
                                        "'" + modelRecord.Master.COMM_AMT + "','" + modelRecord.Master.COMM + "','" + modelRecord.Master.COMM_TYPE + "','" + modelRecord.Master.REF + "','" + modelRecord.Master.TRANSPORT_TYPE + "','" + modelRecord.Master.REF_DATE + "','" + modelRecord.Master.CURR_CODE + "'," +
                                        "'" + modelRecord.Master.CRATE + "','" + modelRecord.Master.WAREHOUSE + "','" + modelRecord.Master.REMARKS + "','" + branch + "','" + period + "'," +
                                        "'" + username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "','" + Computer + "'," +
                                        "'" + Ip + "','" + username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                        "'" + Computer + "','" + Ip + "','" + Postal + "','" + Postal + "','" + modelRecord.Master.ASTATUS + "','" + menuID + "','T')";
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                query = $"UPDATE {table} SET V_DATE = '" + modelRecord.Master.V_DATE + @"',
                                                PARTY_CODE = '" + modelRecord.Master.PARTY_CODE + @"',
                                                ACT_CODE = '" + modelRecord.Master.ACT_CODE + @"',
                                                DOC = '" + modelRecord.Master.DOC + @"',
                                                DEP = '" + modelRecord.Master.DEP + @"',
                                                ORDER_TYPE = '" + modelRecord.Master.ORDER_TYPE + @"',
                                                TERMS = '" + modelRecord.Master.TERMS + @"',
                                                SCODE = '" + modelRecord.Master.SCODE + @"',
                                                SACODE = '" + modelRecord.Master.SACODE + @"',
                                                COMM_AMT = '" + modelRecord.Master.COMM_AMT + @"',
                                                COMM = '" + modelRecord.Master.COMM + @"',
                                                COMM_TYPE = '" + modelRecord.Master.COMM_TYPE + @"',
                                                REF = '" + modelRecord.Master.REF + @"',
                                                TRANSPORT_TYPE = '" + modelRecord.Master.TRANSPORT_TYPE + @"',
                                                REF_DATE = '" + modelRecord.Master.REF_DATE + @"',
                                                CURR_CODE = '" + modelRecord.Master.CURR_CODE + @"',
                                                CRATE = '" + modelRecord.Master.CRATE + @"',
                                                WAREHOUSE = '" + modelRecord.Master.WAREHOUSE + @"',
                                                REMARKS = '" + modelRecord.Master.REMARKS + @"',
                                                EDIT_USER_ID = '" + username + @"',
                                                EDIT_DATE = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
                                                EDIT_COMPUTER_NAME = '" + Computer + @"',
                                                EDIT_IP_ADDRESS = '" + Ip + @"',
                                                EDIT_POSTALCODE = '" + Postal + @"',
                                                ASTATUS = '" + modelRecord.Master.ASTATUS + @"'
                                                WHERE TRAN_ID = '" + modelRecord.Master.TRAN_ID + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }

                            var isDetailAdded = true;

                            if (modelRecord.Detail.Count > 0)
                            {
                                detailQuery = $"UPDATE {detailTable} SET DLT = 'F'" +
                                $" WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                                command.CommandText = detailQuery;
                                command.ExecuteNonQuery();
                            }
                            foreach (var item in modelRecord.Detail.ToList())
                            {
                                try
                                {
                                    if (item.DT_CODE == null || item.DT_CODE == 0)
                                    {
                                        int detailCode = GenerateNextDetailId(common, command);
                                        if (detailCode > 0)
                                        {
                                            detailQuery = $"INSERT INTO {detailTable}" +
                                                           "(TRAN_ID,DT_CODE,ITEM_CODE,QTY," +
                                                           "UNIT,QTY2,BAL_QTY,DT_DESC," +
                                                           "COLOR,SIZE,GRADE,RATE, AMT, DISC, DISC_AMT, TAX, TAX_AMT, NET_AMT, DEL_DATE, " +
                                                           "PRIOIRTY, REQ_TYPE,BCODE," +
                                                           "PERIOD_ID,ADD_USER_ID,ADD_DATE," +
                                                           "ADD_COMPUTER_NAME,ADD_IP_ADDRESS,EDIT_USER_ID," +
                                                           "EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS," +
                                                           "ADD_POSTALCODE,EDIT_POSTALCODE," +
                                                           "MENU_ID,DLT,CHK)" +
                                                           "VALUES" +
                                                           "('" + modelRecord.Master.TRAN_ID + "','" + detailCode + "','" + item.ITEM_CODE + "','" + item.QTY + "'," +
                                                           "'" + item.UNIT + "','" + item.QTY2 + "','" + item.BAL_QTY + "','" + item.DT_DESC + "'," +
                                                           "'" + item.COLOR + "','" + item.SIZE + "','" + item.GRADE + "','" + item.RATE + "'," +
                                                           "'" + item.AMT + "','" + item.DISC + "','" + item.DISC_AMT + "','" + item.TAX + "','" + item.TAX_AMT + "','" + item.NET_AMT + "','" + item.DEL_DATE + "'," +
                                                           "'" + item.PRIORITY + "','" + item.REQ_TYPE + "','" + branch + "'," +
                                                           "'" + period + "','" + username + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                                           "'" + Computer + "','" + Ip + "','" + username + "'," +
                                                           "'" + CommonService.GetDateTime("Pakistan Standard Time") + "','" + Computer + "','" + Ip + "'," +
                                                           "'" + Postal + "','" + Postal + "','" + menuID + "','T','" + item.CHK + "')";
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
                                        detailQuery = $"UPDATE {detailTable} SET ITEM_CODE = '" + item.ITEM_CODE + @"',
                                                        QTY = '" + item.QTY + @"',
                                                        UNIT = '" + item.UNIT + @"',
                                                        QTY2 = '" + item.QTY2 + @"',
                                                        BAL_QTY = '" + item.BAL_QTY + @"',
                                                        DT_DESC = '" + item.DT_DESC + @"',
                                                        COLOR = '" + item.COLOR + @"',
                                                        DEL_DATE = '" + item.DEL_DATE + @"',
                                                        SIZE = '" + item.SIZE + @"',
                                                        GRADE = '" + item.GRADE + @"',
                                                        RATE = '" + item.RATE + @"',
                                                        AMT = '" + item.AMT + @"',
                                                        DISC = '"+ item.DISC + @"',
                                                        DISC_AMT = '"+ item.DISC_AMT + @"',
                                                        TAX = '"+ item.TAX + @"',
                                                        TAX_AMT = '"+ item.TAX_AMT + @"',
                                                        NET_AMT = '"+ item.NET_AMT + @"',
                                                        PRIOIRTY = '" + item.PRIORITY + @"',
                                                        REQ_TYPE = '" + item.REQ_TYPE + @"',
                                                        CHK = '" + item.CHK + @"',
                                                        EDIT_USER_ID = '" + username + @"',
                                                        EDIT_DATE = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
                                                        EDIT_COMPUTER_NAME = '" + Computer + @"',
                                                        EDIT_IP_ADDRESS = '" + Ip + @"',
                                                        EDIT_POSTALCODE = '" + Postal + @"',
                                                        DLT = 'T'
                                                        WHERE TRAN_ID = '" + modelRecord.Master.TRAN_ID + "' AND DT_CODE = '" + item.DT_CODE + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
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
                                response.data = new {
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
                response.msgType = 2;
                response.msg = _catchMessage;
            }
            return response;
        }

		public MyHttpResponseMessage Delete(int code, Common common)
		{
			MyHttpResponseMessage response = new MyHttpResponseMessage();
			response.msgType = 2;
			response.msg = "Data not found in our records";
			try
			{
				var Menu = _menuRepository.GetMenu(common.MenuID);
				string? table = string.Empty;
				string? tableDetail = string.Empty;
				if (Menu.data != null)
				{
					var menu = (Menu)Menu.data;
					table = menu.TABLE1;
                    tableDetail = menu.TABLE2;
				}

				if(!String.IsNullOrWhiteSpace(table))
				{
					if (code == 0)
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

                            string checkQuery = $@"SELECT D.DT_CODE
                                                        FROM TBL_PO_MASTER M
                                                        LEFT OUTER JOIN TBL_PO_DETAIL D ON M.TRAN_ID = D.TRAN_ID
                                                        WHERE M.TRAN_ID = 37 
                                                          AND M.BCODE = 1 
                                                          AND M.PERIOD_ID = 1
                                                          AND D.DT_CODE IN (
                                                                SELECT G.PICK_ID 
                                                                FROM TBL_GRN_DETAIL G
                                                                JOIN TBL_GRN_MASTER GM ON G.TRAN_ID = GM.TRAN_ID
                                                                WHERE GM.DLT <> 'F')";

                            SqlCommand checkCmd = new SqlCommand(checkQuery, connection);

                            int relatedCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (relatedCount > 0)
                            {
                                response.msgType = 2;
                                response.msg = "You cannot delete this entry, record is used in next form.";
                                return response;
                            }

                            string query = $"UPDATE {table} SET DLT = 'F' WHERE TRAN_ID = '{code}' AND BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                            SqlCommand command = new SqlCommand(query, connection);
							command.ExecuteNonQuery();
							response.msgType = 1;
							response.msg = "Record Deleted Successfully";
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

                PurchaseOrder purchaseOrder = new PurchaseOrder();
                List<PurchaseOrderDetail> PurchaseOrderDetailList = new List<PurchaseOrderDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        purchaseOrder = new PurchaseOrder
                        {
                            V_DATE = Convert.ToDateTime(reader["V_DATE"]),
                            PARTY_CODE = Convert.ToString(reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                            DEP = Convert.ToInt32(reader["DEP"]),
                            ORDER_TYPE = Convert.ToString(reader["ORDER_TYPE"]),
                            TERMS = Convert.ToInt32(reader["TERMS"]),
                            SCODE = Convert.ToString(reader["SCODE"]),
                            SACODE = Convert.ToInt32(reader["SACODE"]),
                            COMM_AMT = Convert.ToString(reader["COMM_AMT"]),
                            COMM = Convert.ToDouble(reader["COMM"]),
                            COMM_TYPE = Convert.ToString(reader["COMM_TYPE"]),
                            REF = Convert.ToString(reader["REF"]),
                            REF_DATE = Convert.ToDateTime(reader["REF_DATE"]),
                            CURR_CODE = Convert.ToInt32(reader["CURR_CODE"]),
                            CRATE = Convert.ToDouble(reader["CRATE"]),
                            WAREHOUSE = Convert.ToInt32(reader["WAREHOUSE"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            DOC = Convert.ToString(reader["DOC"]),
                            TRANSPORT_TYPE = Convert.ToString(reader["TRANSPORT_TYPE"]),
                            
                            
                        };
                    }

                    reader.Close();

                    SqlCommand detail_Command = new SqlCommand(detailQuery, connection);
                    SqlDataReader detail_Reader = detail_Command.ExecuteReader();
                    while (detail_Reader.Read())
                    {
                        var row = new PurchaseOrderDetail
                        {
                            ITEM_CODE = Convert.ToInt32(detail_Reader["ITEM_CODE"]),
                            QTY = Convert.ToDouble(detail_Reader["QTY"]),
                            UNIT = Convert.ToInt32(detail_Reader["UNIT"]),
                            QTY2 = Convert.ToDouble(detail_Reader["QTY2"]),
                            BAL_QTY = Convert.ToDouble(detail_Reader["BAL_QTY"]),
                            RATE = Convert.ToDouble(detail_Reader["RATE"]),
                            AMT = Convert.ToDouble(detail_Reader["AMT"]),
                            DISC = Convert.ToDouble(detail_Reader["DISC"]),
                            DISC_AMT = Convert.ToDouble(detail_Reader["DISC_AMT"]),
                            TAX = Convert.ToDouble(detail_Reader["TAX"]),
                            TAX_AMT = Convert.ToDouble(detail_Reader["TAX_AMT"]),
                            NET_AMT = Convert.ToDouble(detail_Reader["NET_AMT"]),
                            DT_DESC = Convert.ToString(detail_Reader["DT_DESC"]),
                            COLOR = Convert.ToInt32(detail_Reader["COLOR"]),
                            SIZE = Convert.ToInt32(detail_Reader["SIZE"]),
                            GRADE = Convert.ToInt32(detail_Reader["GRADE"]),
                            REQ_TYPE = Convert.ToString(detail_Reader["REQ_TYPE"]),
                            PRIORITY = Convert.ToString(detail_Reader["PRIOIRTY"]),
                            POD = Convert.ToString(detail_Reader["POD"]),
                            ORDER_NO = Convert.ToString(detail_Reader["ORDER_NO"]),
                            DEL_DATE = Convert.ToDateTime(detail_Reader["DEL_DATE"]),
                            SHIP_DATE = Convert.ToDateTime(detail_Reader["SHIP_DATE"]),
                            PARTY_CODE = Convert.ToInt32(detail_Reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(detail_Reader["ACT_CODE"]),
                            CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"])
                        };
                        PurchaseOrderDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customRequisition = new CustomPurchaseOrder
                {
                    Master = purchaseOrder,
                    Detail = PurchaseOrderDetailList
                };

                response = this.Save(customRequisition, common);

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

        public MyHttpResponseMessage DeletePurchaseOrderDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE2;
                }
                if (!String.IsNullOrWhiteSpace(table))
                {
                    var branch = common.Branch;
                    var period = common.Period;
                    if (code == 0)
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
                            //getting tran_id
                            string tranIdQuery = $@"SELECT isnull(COUNT(*),0) FROM TBL_GRN_DETAIL WHERE PICK_ID = {code} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                            SqlCommand checkCmd = new SqlCommand(tranIdQuery, connection);
                            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count > 0)
                            {
                                response.msgType = 2;
                                response.msg = "You cannot delete this entry, record is used in next form.";
                                return response;
                            }


                            string query = $"UPDATE {table} SET DLT = 'F'" +
                                $" WHERE DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.ExecuteNonQuery();
                            response.msgType = 1;
                            response.msg = "Record Deleted Successfully";
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

        //public MyHttpResponseMessage GetDataForReport(PurchaseOrderRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    PurchaseOrderRDLCReport masterData = new PurchaseOrderRDLCReport();
        //    CustomPurchaseOrderForPrintReport reportData = new CustomPurchaseOrderForPrintReport();
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
        //        if (menuDetails.MD_ID == 32)
        //        {
        //            query = @$"SELECT 
        //                        M.V_DATE,m.VOUCHER_NO,PT.PARTY_NAME,M.ORDER_TYPE,CONVERT(NVARCHAR(20),M.TERMS)+' Days'  as Terms,
        //                        M.REF,D.DEL_DATE,M.REMARKS,
        //                        IM.ITEM_NAME,D.BAL_QTY,U.GROUP_NAME AS UNIT,D.RATE,D.AMT,D.DISC,D.DISC_AMT,D.TAX,D.TAX_AMT,D.NET_AMT, G.GROUP_NAME AS GRADE
        //                        FROM {table} M
        //                        LEFT OUTER JOIN {detailTable} D
        //                        ON D.TRAN_ID = M.TRAN_ID AND D.BCODE = M.BCODE AND D.PERIOD_ID = M.PERIOD_ID
        //                        LEFT OUTER JOIN TBL_ITEMSMASTER IM
        //                        ON IM.ITEM_CODE = D.ITEM_CODE
        //                        LEFT OUTER JOIN TBL_UNIT U
        //                        ON U.GROUP_CODE = D.UNIT
        //                        LEFT OUTER JOIN TBL_PARTY_TYPES PT
        //                        ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE
        //                        LEFT OUTER JOIN TBL_CURRENCY CR
        //                        ON CR.CODE = M.CURR_CODE
        //                        LEFT OUTER JOIN TBL_GRADE G
        //                        ON G.GROUP_CODE = D.GRADE
        //                        WHERE M.TRAN_ID = {modelRecord.TRAN_ID} AND M.BCODE = {common.Branch} AND M.PERIOD_ID = {common.Period}
        //                        AND M.ASTATUS = 'Y' AND M.DLT = 'T' AND D.DLT = 'T'";
        //        }

        //        masterData.COMPANY_NAME = currentCompany.C_NAME;
        //        masterData.COMPANY_ADDRESS = currentCompany.C_ADDRESS;
        //        masterData.COMPANY_PHONE = currentCompany.C_TEL;
        //        masterData.COMPANY_LOGO = currentCompany.C_LOGO;
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
        //                DataRow dataRow = dataTable.NewRow();
        //                dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
        //                dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
        //                dataRow["Qty"] = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAL_QTY"]);
        //                dataRow["Rate"] = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RATE"]);
        //                dataRow["Amt"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
        //                dataRow["NetAmt"] = reader["NET_AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["NET_AMT"]);
        //                dataRow["Tax"] = reader["TAX"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TAX"]);
        //                dataRow["TaxAmt"] = reader["TAX_AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TAX_AMT"]);
        //                dataRow["Disc"] = reader["DISC"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DISC"]);
        //                dataRow["DiscAmt"] = reader["DISC_AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["DISC_AMT"]);

        //                dataRow["Voucher"] = Convert.ToString(reader["VOUCHER_NO"]);
        //                dataRow["Date"] = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
        //                dataRow["Party"] = Convert.ToString(reader["PARTY_NAME"]);
        //                dataRow["OrderType"] = Convert.ToString(reader["ORDER_TYPE"]);
        //                dataRow["Terms"] = Convert.ToString(reader["Terms"]);
        //                dataRow["Grade"] = Convert.ToString(reader["GRADE"]);
        //                dataRow["Reference"] = Convert.ToString(reader["REF"]);
        //                dataRow["Remarks"] = Convert.ToString(reader["REMARKS"]);
        //                dataRow["DeliveryDate"] = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("dd-MM-yyyy");
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

        public MyHttpResponseMessage GetDataForReport(PurchaseOrderRDLCReport modelRecord, DataTable dataTable, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            PurchaseOrderRDLCReport masterData = new PurchaseOrderRDLCReport();
            CustomPurchaseOrderForPrintReport reportData = new CustomPurchaseOrderForPrintReport();
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

                if (menuDetails.REPORT_NAME == "PurchaseOrder")
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
                            masterData.PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]);
                            masterData.ORDER_TYPE = reader["ORDER_TYPE"] == DBNull.Value ? "" : Convert.ToString(reader["ORDER_TYPE"]);
                            masterData.TERMS = reader["TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["TERMS"]);
                            masterData.REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]);
                            masterData.COMMENT = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]);
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
                            dataRow["Item"] = Convert.ToString(reader["ITEM"]);
                            dataRow["Qty"] = Convert.ToString(reader["BAL_QTY"]);
                            dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
                            dataRow["Rate"] = Convert.ToString(reader["RATE"]);
                            dataRow["Amt"] = Convert.ToString(reader["AMT"]);
                            dataRow["Disc"] = Convert.ToString(reader["DISC"]);
                            dataRow["DiscAmt"] = Convert.ToString(reader["DISC_AMT"]);
                            dataRow["Tax"] = Convert.ToString(reader["TAX"]);
                            dataRow["TaxAmt"] = Convert.ToString(reader["TAX_AMT"]);
                            dataRow["NetAmt"] = Convert.ToString(reader["NET_AMT"]);
                            dataRow["Grade"] = Convert.ToString(reader["GRADE"]);
                            dataRow["DeliveryDate"] = Convert.ToDateTime(reader["DEL_DATE"]);
                            //dataRow["Desc"] = Convert.ToString(reader["DT_DESC"]);
                            //dataRow["Qty"] = Convert.ToString(reader["QTY"]);
                            //dataRow["ChqDate"] = reader["CHQ_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["CHQ_DATE"]);
                            //dataRow["Debit"] = reader["DEBIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DEBIT"]);
                            //dataRow["Credit"] = reader["CREDIT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CREDIT"]);
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