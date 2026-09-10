using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities.Zlib;
using System.Data;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class PurchaseBillRepository : IPurchaseBillRepository
    {
        public IMenuRepository _menuRepository { get; set; }
        public ICommonRepository _commonRepository { get; set; }
        public ICommonService _commonService { get; set; }
        public IBranchRepository _branchRepository { get; set; }
        public IPeriodRepository _periodRepository { get; set; }

        public PurchaseBillRepository(IMenuRepository menuRepository, IBranchRepository branchRepository, ICommonRepository commonRepository, ICommonService commonService, IPeriodRepository periodRepository)
        {
            _menuRepository = menuRepository;
            _branchRepository = branchRepository;
            _commonRepository = commonRepository;
            _commonService = commonService;
            _periodRepository = periodRepository;
        }
        public MyHttpResponseMessage GetLastRateByBarcode(CustomPurchaseBill model, string? dctype, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                string? detailTable = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                }
                var BARCODE = model.Detail.First().BARCODE_ID;

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();

                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {

                        string query = $@"DECLARE @DC_TYPE VARCHAR(5);
                            SET @DC_TYPE = '{dctype}';
                            DECLARE @PARTY_CODE INT = {model.Master.PARTY_CODE};
                            DECLARE @ACT_CODE INT = {model.Master.ACT_CODE};
                            DECLARE @BARCODE INT = {model.Master.BARCODE_ID};

                            SELECT
                                A.CODE,
                                A.BARCODE,
                                A.BLABEL,
                                B.ITEM_NAME AS ITEM_ID,
                                A.COLOR,
                                A.SIZE,
                                CASE
                                    WHEN @DC_TYPE IN('PB','PR') 
                                        THEN ISNULL(NULLIF(LAST_RATE.RATE,0), A.PRATE)

                                    WHEN @DC_TYPE IN('SB', 'SR')
                                        THEN ISNULL(NULLIF(LAST_RATE.RATE,0), A.SRATE)

                                    ELSE 0
                                END AS SRATE,

                                LAST_RATE.RATE AS LAST_RATE,

                                M.PARTY_CODE,
                                M.ACT_CODE

                            FROM TBL_BARCODE A

                            INNER JOIN TBL_ITEMSMASTER B
                                ON A.ITEM_CODE = B.ITEM_CODE



                            OUTER APPLY(
                                SELECT TOP 1
                                    D.RATE
                                FROM TBL_SB_DETAIL D
                                INNER JOIN TBL_SB_MASTER M2
                                    ON M2.TRAN_ID = D.TRAN_ID
                                    AND M2.BCODE = D.BCODE
                                    AND M2.PERIOD_ID = D.PERIOD_ID
                                WHERE
                                    D.ITEM_CODE = A.BARCODE
                                    AND M2.PARTY_CODE = @PARTY_CODE
                                    AND M2.ACT_CODE = @ACT_CODE
                                ORDER BY M2.TRAN_ID DESC
                            ) LAST_RATE



                            OUTER APPLY(
                                SELECT TOP 1
                                    M3.PARTY_CODE,
                                    M3.ACT_CODE
                                FROM TBL_SB_MASTER M3
                                WHERE
                                    M3.PARTY_CODE = @PARTY_CODE
                                    AND M3.ACT_CODE = @ACT_CODE
                                    AND EXISTS(
                                        SELECT 1
                                        FROM TBL_SB_DETAIL D3
                                        WHERE D3.TRAN_ID = M3.TRAN_ID
                                        AND D3.ITEM_CODE = A.BARCODE
                                    )
                                ORDER BY M3.TRAN_ID DESC
                            ) M

                            WHERE
                                A.DLT = 'T'
                                AND A.ASTATUS = 'Y'
                                AND B.DLT = 'T'
                                AND B.ASTATUS = 'Y'
                                AND A.CODE = @BARCODE; ";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                rate = Convert.ToString(reader["SRATE"]),
                            };
                            jsonDataResult.Add(row);
                        }
                        reader.Close();
                    }
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

        public MyHttpResponseMessage GetAvailableStock(string period, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            List<object> stockList = new List<object>(); // List jisme data store hoga

            try
            {
                var periodInfo = _periodRepository.GetPeriodById(Convert.ToInt32(period));

                // Dates nikalne ka logic
                string StartDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
                string EndDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");

                // Query (ensure karein ki STKPROC se ID/Barcode bhi aye)
                string query = $@"EXEC STKPROC 37,'{StartDate}','{EndDate}','{common.Branch}','{common.Period}','',''";
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        stockList.Add(new
                        {
                            BarcodeID = reader["CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CODE"]),
                            Balance = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BALANCE"])
                        });
                    }
                }

                response.data = stockList;
                response.msg = "Success";
                response.msgType = 1;
            }
            catch (Exception ex)
            {
                response.msg = ex.Message;
                response.msgType = 2;
            }

            return response;
        }
        public MyHttpResponseMessage QuickSearch(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                string? detailTable = string.Empty;
                string? search = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    search = menu.SEARCH;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();

                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        if (search == "M")
                        {
                            string query = "SELECT M.TRAN_ID AS TRAN_ID, M.TRAN_ID AS CODE,M.V_DATE,M.VOUCHER_NO,M.BTYPE,M.COMM," +
                                    "M.PARTY_CODE,M.ACT_CODE,M.REF,M.REMARKS,M.SCODE,M.SACODE,M.BCODE,M.PERIOD_ID," +
                                        "M.ADD_USER_ID,M.ADD_DATE,M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS," +
                                        "M.EDIT_USER_ID,M.EDIT_DATE,M.EDIT_COMPUTER_NAME,M.EDIT_IP_ADDRESS,PT.PARTY_NAME," +
                                        "M.ADD_POSTALCODE,M.EDIT_POSTALCODE, CASE WHEN M.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS " +
                                        $"FROM {table} M " +
                                        $"LEFT OUTER JOIN TBL_PARTY_TYPES PT " +
                                        $"ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE " +
                                        $"WHERE  M.DLT = 'T' AND M.BCODE = '" + common.Branch + "' AND M.PERIOD_ID = '" + common.Period + "' ORDER BY M.TRAN_ID DESC";

                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToInt32(reader["TRAN_ID"]),
                                    CODE = Convert.ToInt32(reader["CODE"]),
                                    ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                    V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                    VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    REF = Convert.ToString(reader["REF"]),
                                    BTYPE = Convert.ToString(reader["BTYPE"]),
                                    REMARKS = Convert.ToString(reader["REMARKS"]),
                                    SCODE = reader["SCODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SCODE"]),
                                    SACODE = reader["SACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACODE"]),
                                    ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                                    ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]).ToString("yyyy-MM-dd"),
                                    ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                                    ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                                    EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                                    EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("yyyy-MM-dd"),
                                    EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                                    EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
                                    PARTY_NAME = reader["PARTY_NAME"],
                                    ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                                    EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
                                    COMM = Convert.ToInt32(reader["COMM"]),
                                };
                                jsonDataResult.Add(row);
                            }
                            reader.Close();


                        }
                        else
                        {
                            var query = $@"SELECT M.TRAN_ID AS TRAN_ID, M.TRAN_ID AS CODE,M.V_DATE,M.VOUCHER_NO,M.BTYPE,M.COMM,M.PARTY_CODE,M.ACT_CODE,M.REF,D.DT_DESC,
                                            ROUND(SUM(D.NET_AMT),0) AS AMT,
                                            M.SCODE,M.SACODE,M.BCODE,M.PERIOD_ID,
                                            M.ADD_USER_ID,M.ADD_DATE,M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS,M.EDIT_USER_ID,M.EDIT_DATE,
                                            M.EDIT_COMPUTER_NAME,M.EDIT_IP_ADDRESS,PT.PARTY_NAME,M.ADD_POSTALCODE,M.EDIT_POSTALCODE, 
                                            CASE WHEN M.ASTATUS = 'Y' THEN 'Active' ELSE 'In-Active' END AS ASTATUS 
                                            FROM {table} M 
                                            LEFT OUTER JOIN TBL_PARTY_TYPES PT ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE 
                                            LEFT OUTER JOIN {detailTable} D
                                            ON D.TRAN_ID = M.TRAN_ID AND D.PERIOD_ID = M.PERIOD_ID AND D.BCODE = M.BCODE
                                            WHERE  M.DLT = 'T' AND M.BCODE = {common.Branch} AND M.PERIOD_ID = {common.Period} AND  D.DLT = 'T'
                                            GROUP BY 
                                            M.TRAN_ID  , M.TRAN_ID  ,M.V_DATE,M.VOUCHER_NO,M.BTYPE,M.COMM,M.PARTY_CODE,M.ACT_CODE,M.REF,D.DT_DESC,
                                            M.SCODE,M.SACODE,M.BCODE,M.PERIOD_ID,
                                            M.ADD_USER_ID,M.ADD_DATE,M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS,M.EDIT_USER_ID,M.EDIT_DATE,
                                            M.EDIT_COMPUTER_NAME,M.EDIT_IP_ADDRESS,PT.PARTY_NAME,M.ADD_POSTALCODE,M.EDIT_POSTALCODE, M.ASTATUS 
                                            ORDER BY M.TRAN_ID DESC";
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToInt32(reader["TRAN_ID"]),
                                    CODE = Convert.ToInt32(reader["CODE"]),
                                    ASTATUS = Convert.ToString(reader["ASTATUS"]),
                                    V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                    VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    REF = Convert.ToString(reader["REF"]),
                                    REMARKS = Convert.ToString(reader["DT_DESC"]),
                                    AMT = Convert.ToString(reader["AMT"]),
                                    BTYPE = Convert.ToString(reader["BTYPE"]),
                                    SCODE = reader["SCODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SCODE"]),
                                    SACODE = reader["SACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACODE"]),
                                    ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                                    ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]).ToString("yyyy-MM-dd"),
                                    ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                                    ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                                    EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                                    EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("yyyy-MM-dd"),
                                    EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                                    EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
                                    PARTY_NAME = reader["PARTY_NAME"],
                                    ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                                    EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
                                    COMM = Convert.ToInt32(reader["COMM"]),
                                };
                                jsonDataResult.Add(row);
                            }
                            reader.Close();
                        }
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

        //public MyHttpResponseMessage QuickSearch(Common common, int skip, int take, string filter = null, string group = null, string sort = null)
        //{
        //    MyHttpResponseMessage response = new MyHttpResponseMessage();
        //    try
        //    {
        //        string sortBy;
        //        var Menu = _menuRepository.GetMenu(common.MenuID);
        //        string? table = string.Empty;
        //        if (Menu.data != null)
        //        {
        //            var menu = (Menu)Menu.data;
        //            table = menu.TABLE1;
        //        }

        //        if (sort is not null)
        //        {
        //            var data = JsonSerializer.Deserialize<dynamic>(sort);
        //            var selector = data[0].GetProperty("selector").ToString();
        //            var order = data[0].GetProperty("desc").ToString() == "False" ? "ASC" : "DESC";
        //            sortBy = $"{selector} {order}";
        //            sortBy = sortBy
        //                .Replace("id", "TRAN_ID");
        //        }
        //        else
        //        {
        //            sortBy = "TRAN_ID DESC";
        //        }

        //        if (!String.IsNullOrWhiteSpace(table))
        //        {
        //            List<object> jsonDataResult = new List<object>();
        //            int totalCount = 0;

        //            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
        //            {
        //                string filterCondition = string.Empty;
        //                if (!string.IsNullOrEmpty(filter))
        //                {
        //                    JsonNode jsonNode = JsonNode.Parse(filter);
        //                    JsonArray jsonArray = jsonNode.AsArray();
        //                    if (jsonArray.Count > 8)
        //                    {
        //                        jsonArray.RemoveAt(1);
        //                        jsonArray.RemoveAt(0);
        //                    }
        //                    filterCondition = _commonRepository.BuildFilterCondition(jsonArray);
        //                    filterCondition = filterCondition
        //                        .Replace("id", "TRAN_ID");
        //                }

        //                string query = $"SELECT COUNT(*) FROM {table} WHERE DLT = 'T' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + $"' {filterCondition}";

        //                SqlCommand countCommand = new SqlCommand(query, connection);
        //                connection.Open();
        //                totalCount = (int)countCommand.ExecuteScalar();
        //                connection.Close();

        //                if (string.IsNullOrEmpty(group))
        //                {
        //                    query = "SELECT TRAN_ID,V_DATE,VOUCHER_NO,BTYPE,COMM," +
        //                               "PARTY_CODE,ACT_CODE,REF,REMARKS,SCODE,SACODE,BCODE,PERIOD_ID," +
        //                               "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS," +
        //                               "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS," +
        //                               "ADD_POSTALCODE,EDIT_POSTALCODE,ASTATUS " +
        //                               $"FROM {table} WHERE DLT = 'T' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + $"' {filterCondition} ORDER BY {sortBy}" +
        //                               $" OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
        //                }
        //                else
        //                {
        //                    query = "SELECT TRAN_ID,V_DATE,VOUCHER_NO,BTYPE,COMM," +
        //                               "PARTY_CODE,ACT_CODE,REF,REMARKS,SCODE,SACODE,BCODE,PERIOD_ID," +
        //                               "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS," +
        //                               "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS," +
        //                               "ADD_POSTALCODE,EDIT_POSTALCODE,ASTATUS " +
        //                               $"FROM {table} WHERE DLT = 'T' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + $"' {filterCondition} ORDER BY {sortBy}";
        //                }
        //                SqlCommand command = new SqlCommand(query, connection);
        //                connection.Open();
        //                SqlDataReader reader = command.ExecuteReader();
        //                while (reader.Read())
        //                {
        //                    var row = new
        //                    {
        //                        ID = Convert.ToString(reader["TRAN_ID"]),
        //                        ASTATUS = Convert.ToString(reader["ASTATUS"]),
        //                        V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
        //                        VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
        //                        PARTY_CODE = Convert.ToInt32(reader["PARTY_CODE"]),
        //                        ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
        //                        REF = Convert.ToString(reader["REF"]),
        //                        BTYPE = Convert.ToString(reader["BTYPE"]),
        //                        REMARKS = Convert.ToString(reader["REMARKS"]),
        //                        SCODE = Convert.ToInt32(reader["SCODE"]),
        //                        SACODE = Convert.ToInt32(reader["SACODE"]),
        //                        ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
        //                        ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]).ToString("yyyy-MM-dd"),
        //                        ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
        //                        ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
        //                        EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
        //                        EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]).ToString("yyyy-MM-dd"),
        //                        EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
        //                        EDIT_IP_ADDRESS = Convert.ToString(reader["EDIT_IP_ADDRESS"]),
        //                        ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
        //                        EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
        //                        COMM = Convert.ToInt32(reader["COMM"]),
        //                    };
        //                    jsonDataResult.Add(row);
        //                }
        //                reader.Close();
        //            }

        //            if (!string.IsNullOrEmpty(group))
        //            {

        //                JsonNode groupNode = JsonNode.Parse(group);
        //                JsonArray groupArray = groupNode.AsArray();
        //                var groupSelectors = groupArray.Select(g => g["selector"].ToString());
        //                string groupByClause = string.Join("", groupSelectors.Select(selector => selector));

        //                var groupedData = new List<object>();
        //                var grouped = jsonDataResult.GroupBy(d => _commonRepository.GetPropertyValue(d, groupByClause.ToUpper())).Select(g => new
        //                {
        //                    key = g.Key,
        //                    items = g.ToList()
        //                });
        //                groupedData.AddRange(grouped);
        //                response.data = new
        //                {
        //                    data = groupedData,
        //                    totalCount = totalCount,
        //                };
        //            }
        //            else
        //            {
        //                response.data = new
        //                {
        //                    data = jsonDataResult,
        //                    totalCount = totalCount
        //                };
        //            }
        //            response.msg = "";
        //            response.msgType = 1;
        //        }
        //        else
        //        {
        //            response.data = "";
        //            response.msg = "Something went wrong! please try again later.";
        //            response.msgType = 2;
        //        }
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

        public MyHttpResponseMessage GetPurchaseBillByCode(int code, Common common)
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
                        string query = $"SELECT TRAN_ID,V_DATE,VOUCHER_NO,BTYPE, DOC,COMM,COMM_AMT,COMM_VAL,DISC,DISC_RATE,CARTAGE,TERMS,BACT_CODE,CACT_CODE,BAMT,CAMT," +
                                       "PARTY_CODE,ACT_CODE,CURR_CODE,CRATE,REF,REMARKS,SCODE,SACODE,BCODE,PERIOD_ID," +
                                       "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS," +
                                       "EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS," +
                                       "ADD_POSTALCODE,EDIT_POSTALCODE,ASTATUS " +
                                       $"FROM {table} WHERE DLT = 'T' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "' AND TRAN_ID = '" + code + "'";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                ID = reader["TRAN_ID"] == DBNull.Value ? "" : Convert.ToString(reader["TRAN_ID"]),
                                ASTATUS = reader["ASTATUS"] == DBNull.Value ? "" : Convert.ToString(reader["ASTATUS"]),
                                V_DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("yyyy-MM-dd"),
                                VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                                BTYPE = reader["BTYPE"] == DBNull.Value ? "" : Convert.ToString(reader["BTYPE"]),
                                DOC = reader["DOC"] == DBNull.Value ? "" : Convert.ToString(reader["DOC"]),
                                COMM = reader["COMM"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COMM"]),
                                TERMS = reader["TERMS"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TERMS"]),
                                COMM_AMT = reader["COMM_AMT"] == DBNull.Value ? "" : Convert.ToString(reader["COMM_AMT"]),
                                COMM_VAL = reader["COMM_VAL"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COMM_VAL"]),
                                DISC = reader["DISC"] == DBNull.Value ? "" : Convert.ToString(reader["DISC"]),
                                DISC_RATE = reader["DISC"] == DBNull.Value ? "" : Convert.ToString(reader["DISC_RATE"]),
                                CARTAGE = reader["CARTAGE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CARTAGE"]),
                                REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                SCODE = reader["SCODE"] == DBNull.Value ? "" : Convert.ToString(reader["SCODE"]),
                                SACODE = reader["SACODE"] == DBNull.Value ? "" : Convert.ToString(reader["SACODE"]),
                                CURR_CODE = reader["CURR_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CURR_CODE"]),
                                CRATE = reader["CRATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CRATE"]),
                                REMARKS = reader["REMARKS"] == DBNull.Value ? "" : Convert.ToString(reader["REMARKS"]),
                                PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                BACT = reader["BACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BACT_CODE"]),
                                CACT = reader["CACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CACT_CODE"]),
                                BAMT = reader["BAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAMT"]),
                                CAMT = reader["CAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CAMT"]),

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

        public MyHttpResponseMessage GetPickDataByParty(int partyCode, int actCode, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                string? detailTable = string.Empty;
                string? pickTable = string.Empty;
                string? pickDetailTable = string.Empty;
                string? pickType = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    pickTable = menu.PICK_TABLE_MASTER;
                    pickDetailTable = menu.PICK_TABLE_DETAIL;
                    pickType = menu.PICK_TYPE;
                }

                if (pickType == "BL")
                {
                    if (!String.IsNullOrWhiteSpace(table))
                    {
                        List<object> jsonDataResult = new List<object>();
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            string query = $@"SELECT A.TRAN_ID,
                                        A.V_DATE AS LB_DATE,
                                        A.VOUCHER_NO,AC.PARTY_NAME AS PARTY_NAME,A.ACT_CODE,A.REF,
                                        SUM(ISNULL(B.QTY, 0) - ISNULL(VC.QTY, 0)) AS QTY, SUM(B.NET_AMT) AS AMT, (ISNULL(A.DISC, 0)) AS DISC
                                        FROM {pickTable} A
                                        LEFT OUTER JOIN {pickDetailTable} B
                                        ON B.TRAN_ID = A.TRAN_ID AND B.BCODE = A.BCODE AND B.PERIOD_ID = A.PERIOD_ID
                                        LEFT OUTER JOIN {detailTable} VC
                                        ON VC.PICK_ID = B.DT_CODE AND VC.BCODE = A.BCODE AND VC.PERIOD_ID = A.PERIOD_ID
                                        LEFT OUTER JOIN TBL_PARTY_TYPES AC
                                        ON AC.PARTY_CODE = A.PARTY_CODE AND AC.ACT_CODE = A.ACT_CODE
                                         WHERE A.BCODE = {common.Branch} And A.PERIOD_ID = {common.Period} AND A.DLT = 'T' AND A.ASTATUS = 'Y' 
                                         And A.PARTY_CODE = {partyCode} And A.ACT_CODE = {actCode}
                                         Group By
                                         A.TRAN_ID,
                                         A.DISC,
                                         A.V_DATE,
                                         A.VOUCHER_NO,AC.PARTY_NAME,A.REF,A.ACT_CODE,
                                         a.MENU_ID
                                         Having Sum(IsNull(b.QTY, 0)) > Sum(IsNull(VC.QTY, 0)) ORDER BY A.TRAN_ID DESC";

                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToString(reader["TRAN_ID"]),
                                    LB_DATE = reader["LB_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["LB_DATE"]).ToString("yyyy-MM-dd"),
                                    VOUCHER_NO = Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]),
                                    ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                                    REF = Convert.ToString(reader["REF"]),
                                    QTY = Convert.ToString(reader["QTY"]),
                                    AMT = Convert.ToString(reader["AMT"]),
                                    DISC = Convert.ToString(reader["DISC"])
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
                else if (pickType == "O")
                {
                    if (!String.IsNullOrWhiteSpace(table))
                    {
                        List<object> jsonDataResult = new List<object>();
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            string query = $@"SELECT A.TRAN_ID,
                                            A.V_DATE AS LB_DATE,
                                            A.VOUCHER_NO,AC.PARTY_NAME AS PARTY_NAME,A.ACT_CODE,A.REF,
                                            (ISNULL(B.QTY, 0) - ISNULL(VC.QTY, 0)) AS QTY,U.GROUP_CODE AS UNIT,U.GROUP_NAME AS UNIT1,b.RATE, (B.AMT) AS AMT,
                                            B.DT_CODE,CR.DESCR AS CURRENCY,A.CRATE,IT.ITEM_NAME,BU.PARTY_NAME AS BUYER,BU.ACT_CODE AS BUYER_ACODE,
											B.DISC,B.DISC_AMT,B.TAX,B.TAX_AMT,CL.GROUP_NAME AS COLOR,SL.GROUP_NAME AS SIZE,G.GROUP_NAME AS GRADE,B.DEL_DATE,
											A.COMM_AMT,A.COMM,(B.NET_AMT) AS NET_AMT,CR.CODE AS CRR_CODE,CL.GROUP_CODE AS COLOR_ID , SL.GROUP_CODE AS SIZE_ID ,
                                            G.GROUP_CODE AS GRADE_ID,IT.ITEM_CODE , BU.PARTY_CODE
                                            FROM {pickTable} A
                                            LEFT OUTER JOIN {pickDetailTable} B
                                            ON B.TRAN_ID = A.TRAN_ID AND B.BCODE = A.BCODE AND B.PERIOD_ID = A.PERIOD_ID
                                            LEFT OUTER JOIN {detailTable} VC
                                            ON VC.PICK_ID = B.DT_CODE AND VC.BCODE = A.BCODE AND VC.PERIOD_ID = A.PERIOD_ID
                                            LEFT OUTER JOIN TBL_PARTY_TYPES AC
                                            ON AC.PARTY_CODE = A.PARTY_CODE AND AC.ACT_CODE = A.ACT_CODE
                                            LEFT OUTER JOIN TBL_CURRENCY CR
                                            ON CR.CODE = A.CURR_CODE
											LEFT OUTER JOIN TBL_ITEMSMASTER IT
                                            ON IT.ITEM_CODE = B.ITEM_CODE
											LEFT OUTER JOIN TBL_PARTY_TYPES BU
                                            ON BU.PARTY_CODE = B.PARTY_CODE AND BU.ACT_CODE = B.ACT_CODE
											LEFT OUTER JOIN TBL_COLOR CL
                                            ON CL.GROUP_CODE = B.COLOR
											LEFT OUTER JOIN TBL_SIZE SL
                                            ON SL.GROUP_CODE = B.SIZE
											LEFT OUTER JOIN TBL_GRADE G
                                            ON G.GROUP_CODE = B.GRADE
                                            LEFT OUTER JOIN TBL_UNIT U
                                            ON U.GROUP_CODE = B.UNIT 
                                            WHERE A.BCODE = {common.Branch} And A.PERIOD_ID = {common.Period} AND A.DLT = 'T' AND A.ASTATUS = 'Y' 
                                            AND A.PARTY_CODE = {partyCode} AND A.ACT_CODE = {actCode}";

                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToString(reader["TRAN_ID"]),
                                    LB_DATE = reader["LB_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["LB_DATE"]).ToString("dd-MM-yyyy"),
                                    VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]),
                                    MACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                                    UNIT = reader["UNIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UNIT"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                                    AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                                    PICK_ID_D = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                                    CURRENCY = reader["CURRENCY"] == DBNull.Value ? "" : Convert.ToString(reader["CURRENCY"]),
                                    CRATE = reader["CRATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CRATE"]),
                                    ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),
                                    BUYER = reader["BUYER"] == DBNull.Value ? "" : Convert.ToString(reader["BUYER"]),
                                    BUYER_ACODE = reader["BUYER_ACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BUYER_ACODE"]),
                                    DISC = reader["DISC"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DISC"]),
                                    DISC_AMT = reader["DISC_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DISC_AMT"]),
                                    TAX = reader["TAX"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TAX"]),
                                    TAX_AMT = reader["TAX_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TAX_AMT"]),
                                    COLOR_NAME = reader["COLOR"] == DBNull.Value ? "" : Convert.ToString(reader["COLOR"]),
                                    SIZE_NAME = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                                    GRADE_NAME = reader["GRADE"] == DBNull.Value ? "" : Convert.ToString(reader["GRADE"]),
                                    DEL_DATE = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("dd-MM-yyyy"),
                                    COMM_AMT = reader["COMM_AMT"] == DBNull.Value ? "" : Convert.ToString(reader["COMM_AMT"]),
                                    COMM = reader["COMM"] == DBNull.Value ? 0 : Convert.ToDouble(reader["COMM"]),
                                    NET_AMT = reader["NET_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["NET_AMT"]),
                                    CRR_CODE = reader["CRR_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CRR_CODE"]),
                                    COLOR = reader["COLOR_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR_ID"]),
                                    SIZE = reader["SIZE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE_ID"]),
                                    GRADE = reader["GRADE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GRADE_ID"]),
                                    ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    ACT_CODE = reader["BUYER_ACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BUYER_ACODE"]),
                                    PARTY_DDL =
                                    (reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToString(reader["PARTY_CODE"])) +
                                    (reader["BUYER_ACODE"] == DBNull.Value ? "" : Convert.ToString(reader["BUYER_ACODE"])),
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
                else if (pickType == "MPO")
                {
                    if (!String.IsNullOrWhiteSpace(table))
                    {
                        List<object> jsonDataResult = new List<object>();
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            string query = $@"SELECT A.TRAN_ID,A.V_DATE AS LB_DATE,A.VOUCHER_NO,A.PARTY_CODE,A.ACT_CODE,AC.PARTY_NAME AS PARTY_NAME,A.REF,(ISNULL(B.QTY, 0) - ISNULL(VC.QTY, 0)) AS QTY,
                                                U.GROUP_CODE AS UNIT,U.GROUP_NAME AS UNIT1,b.RATE, (B.AMT) AS AMT,
                                                B.DT_CODE,CL.GROUP_NAME AS COLOR,SL.GROUP_NAME AS SIZE, A.COMM, A.COMM_AMT, A.COMM_VAL,
                                                CL.GROUP_CODE AS COLOR_ID , SL.GROUP_CODE AS SIZE_ID , MPO.SPARTY_CODE, MPO.SACT_CODE, MPO.ITEM_CODE, I.ITEM_NAME
                                            FROM {pickTable} A
                                            LEFT OUTER JOIN {pickDetailTable} B ON B.TRAN_ID = A.TRAN_ID AND B.BCODE = A.BCODE AND B.PERIOD_ID = A.PERIOD_ID
                                            LEFT OUTER JOIN {detailTable} VC ON VC.PICK_ID = B.DT_CODE AND VC.BCODE = A.BCODE AND VC.PERIOD_ID = A.PERIOD_ID
                                            LEFT OUTER JOIN TBL_MPO_MASTER MPO ON MPO.TRAN_ID = B.PICK_ID AND MPO.BCODE = A.BCODE AND MPO.PERIOD_ID = A.PERIOD_ID
                                            LEFT OUTER JOIN TBL_PARTY_TYPES AC ON AC.PARTY_CODE = A.PARTY_CODE AND AC.ACT_CODE = A.ACT_CODE
                                            LEFT OUTER JOIN TBL_COLOR CL ON CL.GROUP_CODE = B.COLOR
                                            LEFT OUTER JOIN TBL_SIZE SL ON SL.GROUP_CODE = B.SIZE
                                            LEFT OUTER JOIN TBL_UNIT U ON U.GROUP_CODE = B.UNIT 
                                            LEFT OUTER JOIN TBL_ITEMSMASTER I ON I.ITEM_CODE = MPO.ITEM_CODE
                                            WHERE A.BCODE = {common.Branch} And A.PERIOD_ID = {common.Period} AND A.DLT = 'T' AND A.ASTATUS = 'Y'";

                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    ID = Convert.ToString(reader["TRAN_ID"]),
                                    LB_DATE = reader["LB_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["LB_DATE"]).ToString("dd-MM-yyyy"),
                                    VOUCHER_NO = reader["VOUCHER_NO"] == DBNull.Value ? "" : Convert.ToString(reader["VOUCHER_NO"]),
                                    PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                    SPARTY_CODE = reader["SPARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SPARTY_CODE"]),
                                    ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                    SACT_CODE = reader["SACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACT_CODE"]),
                                    PARTY_DDL = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                    PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]),
                                    REF = reader["REF"] == DBNull.Value ? "" : Convert.ToString(reader["REF"]),
                                    QTY = reader["QTY"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QTY"]),
                                    UNIT = reader["UNIT"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UNIT"]),
                                    RATE = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RATE"]),
                                    AMT = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AMT"]),
                                    COLOR_NAME = reader["COLOR"] == DBNull.Value ? "" : Convert.ToString(reader["COLOR"]),
                                    SIZE_NAME = reader["SIZE"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE"]),
                                    COLOR = reader["COLOR_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR_ID"]),
                                    SIZE = reader["SIZE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE_ID"]),
                                    PICK_ID_D = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                                    ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                    ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),
                                    COMM_AMT = reader["COMM_AMT"] == DBNull.Value ? "" : Convert.ToString(reader["COMM_AMT"]),
                                    COMM = reader["COMM"] == DBNull.Value ? 0 : Convert.ToDouble(reader["COMM"]),
                                    COMM_VAL = reader["COMM_VAL"] == DBNull.Value ? 0 : Convert.ToDouble(reader["COMM_VAL"]),
                                    //ITEM_NAME = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]),

                                    //CURRENCY = reader["CURRENCY"] == DBNull.Value ? "" : Convert.ToString(reader["CURRENCY"]),
                                    //CRATE = reader["CRATE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["CRATE"]),
                                    //BUYER = reader["BUYER"] == DBNull.Value ? "" : Convert.ToString(reader["BUYER"]),
                                    //BUYER_ACODE = reader["BUYER_ACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BUYER_ACODE"]),
                                    //DISC = reader["DISC"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DISC"]),
                                    //DISC_AMT = reader["DISC_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DISC_AMT"]),
                                    //TAX = reader["TAX"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TAX"]),
                                    //TAX_AMT = reader["TAX_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TAX_AMT"]),
                                    //GRADE_NAME = reader["GRADE"] == DBNull.Value ? "" : Convert.ToString(reader["GRADE"]),
                                    //DEL_DATE = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("dd-MM-yyyy"),
                                    //COMM_AMT = reader["COMM_AMT"] == DBNull.Value ? "" : Convert.ToString(reader["COMM_AMT"]),
                                    //COMM = reader["COMM"] == DBNull.Value ? 0 : Convert.ToDouble(reader["COMM"]),
                                    //NET_AMT = reader["NET_AMT"] == DBNull.Value ? 0 : Convert.ToDouble(reader["NET_AMT"]),
                                    //CRR_CODE = reader["CRR_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CRR_CODE"]),

                                    //GRADE = reader["GRADE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["GRADE_ID"]),
                                    //ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                    //ACT_CODE = reader["BUYER_ACODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BUYER_ACODE"]),
                                    //PARTY_DDL =
                                    //(reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToString(reader["PARTY_CODE"])) +
                                    //(reader["BUYER_ACODE"] == DBNull.Value ? "" : Convert.ToString(reader["BUYER_ACODE"])),
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
                else
                {
                    response.data = "";
                    response.msg = "Pick data is not mapped. Please contact the administrator.";
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

        public MyHttpResponseMessage GetBarcodeList()
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = $@"SELECT IM.ITEM_NAME AS ITEM_ID, IM.ITEM_CODE AS ITEM_CODE, '' AS COLOR_ID, '' AS SIZE_ID,
		                                IM.ITEM_CODE AS BARCODE_CODE,'' AS SIZE, '' AS COLOR, IM.BARCODE, IM.SALE_RATE AS RATE 
		                                FROM TBL_ITEMSMASTER IM                                  
		                                WHERE IM.DLT = 'T' AND IM.ASTATUS = 'Y' AND IM.ASTATUS = 'Y'
		                                ORDER BY ITEM_ID , SIZE";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            ITEM_ID = Convert.ToString(reader["ITEM_ID"]),
                            ITEM_CODE = Convert.ToString(reader["ITEM_CODE"]),
                            //COLOR = reader["COLOR_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR_ID"]),
                            //SIZE = reader["SIZE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE_ID"]),
                            COLOR = Convert.ToString(reader["COLOR_ID"]),
                            SIZE = Convert.ToString(reader["SIZE_ID"]),
                            BARCODE_CODE = Convert.ToString(reader["BARCODE_CODE"]),
                            SIZE_NAME = Convert.ToString(reader["SIZE"]),
                            COLOR_NAME = Convert.ToString(reader["COLOR"]),
                            BARCODE = Convert.ToString(reader["BARCODE"]),
                            RATE = Convert.ToInt32(reader["RATE"]),
                            AMT = Convert.ToInt32(reader["RATE"]),
                            NET_AMT = Convert.ToInt32(reader["RATE"]),
                            QTY = 1,
                            BAL_QTY = 1,

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

        public MyHttpResponseMessage GetPurchaseBillPickDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.PICK_TABLE_DETAIL;
                }

                if (!String.IsNullOrWhiteSpace(table))
                {
                    List<object> jsonDataResult = new List<object>();
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        string query = "SELECT TRAN_ID,DT_CODE,M.ITEM_CODE, QTY,UNIT,QTY2,BAL_QTY,M.COLOR,M.SIZE, " +
                         " RATE,AMT,DISC,DISC_AMT, TAX,TAX_AMT,ADV,ADV_AMT,NET_AMT, DT_DESC, " +
                         " CL.GROUP_NAME AS COLORNAME,SL.GROUP_NAME AS SIZENAME,M.GRADE, WAREHOUSE,DEL_DATE,DUE_DATE, DUE_DAYS,VEH,BCODE,PERIOD_ID, M.ADD_USER_ID,M.ADD_DATE," +
                         " M.ADD_COMPUTER_NAME,M.ADD_IP_ADDRESS, M.EDIT_USER_ID,M.EDIT_DATE,M.EDIT_COMPUTER_NAME, " +
                         " M.EDIT_IP_ADDRESS, M.ADD_POSTALCODE,M.EDIT_POSTALCODE,M.MENU_ID,M.DLT,CHK,PICK_ID,PICK_ID_D " +
                         $" FROM {table} M LEFT OUTER JOIN TBL_BARCODE BG ON BG.CODE = M.ITEM_CODE " +
                         " LEFT OUTER JOIN TBL_SIZE SL ON SL.GROUP_CODE = BG.SIZE LEFT OUTER JOIN TBL_COLOR CL ON CL.GROUP_CODE = BG.COLOR" +
                         " WHERE  M.DLT = 'T' AND TRAN_ID = '" + code + "' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "'" +
                         " ORDER BY DT_CODE DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = 0,
                                ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                                QTY = Convert.ToString(reader["QTY"]),
                                UNIT = Convert.ToInt32(reader["UNIT"]),
                                QTY2 = Convert.ToString(reader["QTY2"]),
                                BAL_QTY = Convert.ToString(reader["BAL_QTY"]),
                                RATE = Convert.ToString(reader["RATE"]),
                                AMT = Convert.ToString(reader["AMT"]),
                                DISC = Convert.ToString(reader["DISC"]),
                                DISC_AMT = Convert.ToString(reader["DISC_AMT"]),
                                TAX = Convert.ToString(reader["TAX"]),
                                TAX_AMT = Convert.ToString(reader["TAX_AMT"]),
                                ADV = Convert.ToString(reader["ADV"]),
                                ADV_AMT = Convert.ToString(reader["ADV_AMT"]),
                                NET_AMT = Convert.ToString(reader["NET_AMT"]),
                                DT_DESC = Convert.ToString(reader["DT_DESC"]),
                                COLOR = reader["COLOR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR"]),
                                SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                                GRADE = Convert.ToInt32(reader["GRADE"]),
                                WAREHOUSE = Convert.ToInt32(reader["WAREHOUSE"]),
                                DEL_DATE = Convert.ToString(reader["DEL_DATE"]),
                                DUE_DATE = Convert.ToString(reader["DUE_DATE"]),
                                DUE_DAYS = Convert.ToString(reader["DUE_DAYS"]),
                                VEH = Convert.ToString(reader["VEH"]),
                                CHK = Convert.ToString(reader["CHK"]),
                                CHK1 = Convert.ToString(reader["CHK"]) == "1" ? true : false,
                                PICK_ID = Convert.ToInt32(reader["TRAN_ID"]),
                                PICK_ID_D = Convert.ToInt32(reader["DT_CODE"])

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

        public MyHttpResponseMessage GetPurchaseBillDetailByCode(int code, Common common)
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
                        string quergy = "SELECT TRAN_ID,PARTY_CODE,ACT_CODE,DT_CODE,ITEM_CODE,HS_CODE," +
                            " QTY,UNIT,QTY2,BAL_QTY," +
                            " RATE,AMT,DISC,DISC_AMT," +
                            " TAX,TAX_AMT,ADV,ADV_AMT,NET_AMT," +
                            " DT_DESC,COLOR,SIZE,GRADE," +
                            " WAREHOUSE,DEL_DATE,DUE_DATE," +
                            " DUE_DAYS,VEH,BCODE,PERIOD_ID," +
                            " ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME,ADD_IP_ADDRESS," +
                            " EDIT_USER_ID,EDIT_DATE,EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS," +
                            " ADD_POSTALCODE,EDIT_POSTALCODE,MENU_ID,DLT,CHK,PICK_ID,PICK_ID_D" +
                            $" FROM {table} " +
                            " WHERE  DLT = 'T' AND TRAN_ID = '" + code + "' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "'" +
                            " ORDER BY DT_CODE DESC";

                        string query = $@"SELECT D.TRAN_ID, D.PARTY_CODE, D.ACT_CODE, DT_CODE, D.ITEM_CODE, IT.IPIC, D.HS_CODE, D.QTY, D.UNIT, D.QTY2, D.BAL_QTY, 
                                            D.RATE, D.AMT, D.DISC, D.DISC_AMT, D.TAX, D.TAX_AMT, D.ADV, D.ADV_AMT, D.NET_AMT, D.DT_DESC, D.COLOR, D.SIZE, D.GRADE, 
                                            D.WAREHOUSE, D.DEL_DATE, D.DUE_DATE, D.DUE_DAYS, D.VEH, D.BCODE, D.PERIOD_ID,
                                            D.MENU_ID, D.DLT, D.CHK, D.PICK_ID, D.PICK_ID_D 
                                            FROM {table} D
                                            LEFT OUTER JOIN TBL_ITEMSMASTER IT ON IT.ITEM_CODE =  D.ITEM_CODE
                                            WHERE D.DLT = 'T' AND D.TRAN_ID = '{code}' AND D.BCODE = '{common.Branch}' AND D.PERIOD_ID = '{common.Period}' ORDER BY D.DT_CODE DESC";

                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = Convert.ToString(reader["DT_CODE"]),
                                ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                                HS_CODE = Convert.ToString(reader["HS_CODE"]),
                                PARTY_CODE = reader["PARTY_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PARTY_CODE"]),
                                ACT_CODE = reader["ACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ACT_CODE"]),
                                PARTY_DDL = $"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                                QTY = Convert.ToString(reader["QTY"]),
                                UNIT = Convert.ToInt32(reader["UNIT"]),
                                QTY2 = Convert.ToString(reader["QTY2"]),
                                BAL_QTY = Convert.ToString(reader["BAL_QTY"]),
                                RATE = Convert.ToString(reader["RATE"]),
                                AMT = Convert.ToString(reader["AMT"]),
                                DISC = Convert.ToString(reader["DISC"]),
                                DISC_AMT = Convert.ToString(reader["DISC_AMT"]),
                                TAX = Convert.ToString(reader["TAX"]),
                                TAX_AMT = Convert.ToString(reader["TAX_AMT"]),
                                ADV = Convert.ToString(reader["ADV"]),
                                ADV_AMT = Convert.ToString(reader["ADV_AMT"]),
                                DOC = Convert.ToString(reader["IPIC"]),
                                NET_AMT = Convert.ToString(reader["NET_AMT"]),
                                DT_DESC = Convert.ToString(reader["DT_DESC"]),
                                COLOR = Convert.ToInt32(reader["COLOR"]),
                                SIZE = Convert.ToInt32(reader["SIZE"]),
                                GRADE = Convert.ToInt32(reader["GRADE"]),
                                WAREHOUSE = Convert.ToInt32(reader["WAREHOUSE"]),
                                DEL_DATE = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("yyyy-MM-dd"),
                                DUE_DATE = reader["DUE_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DUE_DATE"]).ToString("yyyy-MM-dd"),
                                DUE_DAYS = Convert.ToString(reader["DUE_DAYS"]),
                                VEH = Convert.ToString(reader["VEH"]),
                                CHK = Convert.ToString(reader["CHK"]),
                                CHK1 = Convert.ToString(reader["CHK"]) == "1" ? true : false,
                                PICK_ID = Convert.ToInt32(reader["PICK_ID"]),
                                PICK_ID_D = Convert.ToInt32(reader["PICK_ID_D"])

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
        public MyHttpResponseMessage GetPurchaseBillCommissionByCode(int code, Common common)
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
                        string query = "SELECT * FROM TBL_COMM_GEN" +
                            " WHERE  DLT = 'T' AND BILL_TRAN_ID = '" + code + "' AND BCODE = '" + common.Branch + "' AND PERIOD_ID = '" + common.Period + "'" +
                            " ORDER BY DT_CODE DESC";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                                TRAN_ID = reader["TRAN_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TRAN_ID"]),
                                BILL_TRAN_ID = reader["BILL_TRAN_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BILL_TRAN_ID"]),
                                COMM_UNIT = Convert.ToString(reader["COMM_UNIT"]),
                                COMM_VALUE = Convert.ToString(reader["COMM_VALUE"]),
                                ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
                                SACT_CODE = reader["SACT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SACT_CODE"]),
                                SALESMAN = reader["SALESMAN"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SALESMAN"]),
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

        public MyHttpResponseMessage GetPurchaseBillDetailByItem(int code, int qty, decimal? disc, Common common)
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
                        string query = "SELECT B.CODE, B.SRATE, B.COLOR, B.SIZE, IT.ITEM_ID, IT.SALE_RATE FROM TBL_BARCODE B " +
                            $"LEFT OUTER JOIN TBL_ITEMSMASTER IT ON B.ITEM_CODE = IT.ITEM_CODE " +
                            "WHERE  B.DLT = 'T' AND B.ITEM_CODE = " + code + "";
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var row = new
                            {
                                DT_CODE = 0,
                                ITEM_CODE = Convert.ToInt32(reader["CODE"]),
                                ITEM_ID = Convert.ToString(reader["ITEM_ID"]),
                                QTY = qty,
                                UNIT = 0,
                                QTY2 = qty.ToString(),
                                BAL_QTY = qty.ToString(),
                                RATE = Convert.ToString(reader["SRATE"]),
                                AMT = (Convert.ToInt32(reader["SRATE"]) * qty).ToString(),
                                DISC = disc,
                                DISC_AMT = "",
                                TAX = "",
                                TAX_AMT = "",
                                ADV = "",
                                ADV_AMT = "",
                                NET_AMT = (Convert.ToInt32(reader["SRATE"]) * qty).ToString(),
                                DT_DESC = "",
                                COLOR = reader["COLOR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["COLOR"]),
                                SIZE = reader["SIZE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SIZE"]),
                                GRADE = 0,
                                WAREHOUSE = 0,
                                DEL_DATE = "",
                                DUE_DATE = "",
                                DUE_DAYS = "",
                                VEH = "",
                                CHK = "",
                                CHK1 = false,
                                PICK_ID = 0
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

        private int GenerateNextId(Common common, SqlCommand command, string Commission = "")
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
                    if (Commission == "")
                    {
                        string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM {table} WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                        command.CommandText = maxIdQuery;
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        string maxIdQuery = $"SELECT ISNULL(MAX(TRAN_ID), 0) + 1 FROM TBL_COMM_GEN WHERE BCODE = '{common.Branch}' AND PERIOD_ID = '{common.Period}'";
                        command.CommandText = maxIdQuery;
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }

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

        private int GenerateNextDetailId(Common common, SqlCommand command, string Commission = "")
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
                    if (Commission == "")
                    {
                        string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) FROM {table}";
                        command.CommandText = maxIdQuery;
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        string maxIdQuery = $"SELECT ISNULL(MAX(DT_CODE), 0) + 1 FROM TBL_COMM_GEN";
                        command.CommandText = maxIdQuery;
                        object result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        public MyHttpResponseMessage Save(CustomPurchaseBill modelRecord, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Menu = _menuRepository.GetMenu(common.MenuID);
                string? table = string.Empty, detailTable = string.Empty, b_i = string.Empty, stk_status = string.Empty;
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                    detailTable = menu.TABLE2;
                    b_i = menu.B_I;
                    stk_status = menu.STK_STATUS;
                }

                if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(detailTable))
                {
                    var Ip = common.IPAddress;
                    var Computer = common.ComputerName;
                    var Postal = common.PostalCode;
                    var username = common.Username;
                    var branch = common.Branch;
                    var period = common.Period;
                    var periodInfo = _periodRepository.GetPeriodById(Convert.ToInt32(period));
                    string startDate = ((Period)periodInfo.data).START_D.Value.ToString("yyyy-MM-dd");
                    string endDate = ((Period)periodInfo.data).START_E.Value.ToString("yyyy-MM-dd");
                    var menuID = common.MenuID;
                    bool isStockSufficient = true;
                    string InSufficientItem = "";
                    double InSufficientItemQty = 0;
                    DataTable dt = new DataTable();
                    string connectionString = new SQLService().getconnstring();
                    Dictionary<int?, (double? Balance, string ItemName)> stockBalance = new Dictionary<int?, (double?, string)>();

                    if (stk_status == "Y")
                    {


                        string stkQuery = $@"EXEC STKPROC 71,'{startDate}','{endDate}','{common.Branch}',{common.Period},'',''";

                        using (SqlConnection stkconnection = new SqlConnection(connectionString))
                        {
                            SqlCommand command = new SqlCommand(stkQuery, stkconnection);
                            stkconnection.Open();

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int? itemCode = reader["ITEM_CODE"] as int?;
                                    double? balance = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BALANCE"]);
                                    string itemName = reader["ITEM_NAME"] == DBNull.Value ? string.Empty : reader["ITEM_NAME"].ToString();

                                    if (itemCode != null)
                                    {
                                        if (!stockBalance.ContainsKey(itemCode))
                                        {
                                            // Teesri value bhi sath add kardi
                                            stockBalance.Add(itemCode, (balance, itemName));
                                        }
                                        else
                                        {
                                            stockBalance[itemCode] = (balance, itemName);
                                        }
                                    }
                                }
                            }
                        }
                    }










                    //Dictionary<int?, double?> currentItems = new Dictionary<int?, double?>();
                    //Dictionary<int?, double?> previousItems = new Dictionary<int?, double?>();
                    //Dictionary<int?, double?> stockBalance = new Dictionary<int?, double?>();
                    //if (stk_status == "Y")
                    //{
                    //    if (b_i == "B")
                    //    {
                    //        foreach (var item in modelRecord.Detail.ToList())
                    //        {
                    //            if (!currentItems.ContainsKey(item.ITEM_CODE))
                    //            {
                    //                currentItems.Add(item.ITEM_CODE, item.BAL_QTY);
                    //            }
                    //            else
                    //            {
                    //                currentItems[item.ITEM_CODE] += item.BAL_QTY;
                    //            }
                    //        }

                    //        var itemCodes = string.Join(",", modelRecord.Detail.Select(x => x.ITEM_CODE).Distinct());
                    //        string query = $@"EXEC STKPROC 71,'{startDate}','{endDate}','{common.Branch}','{common.Period}','',''";
                    //        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    //        {
                    //            SqlCommand command = new SqlCommand(query, connection);
                    //            connection.Open();
                    //            using (SqlDataReader reader = command.ExecuteReader())
                    //            {
                    //                while (reader.Read())
                    //                {
                    //                    int? itemCode = reader["ITEM_ID"] as int?;
                    //                    double? balance = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BALANCE"]);

                    //                    if (itemCode != null)
                    //                        stockBalance.Add(itemCode, balance);
                    //                }
                    //            }
                    //        }

                    //        if (modelRecord.Master.TRAN_ID > 0)
                    //        {
                    //            previousItems = PreviousStockInBill(detailTable, modelRecord.Master.TRAN_ID, period, branch);

                    //            foreach (var item in previousItems)
                    //            {
                    //                if (currentItems.ContainsKey(item.Key))
                    //                {
                    //                    currentItems[item.Key] = (currentItems[item.Key] ?? 0) - (item.Value ?? 0);

                    //                    if (currentItems[item.Key] < 0)
                    //                    {
                    //                        currentItems[item.Key] = 0;
                    //                    }
                    //                }
                    //            }
                    //        }

                    //        foreach (var item in currentItems)
                    //        {
                    //            double availableStock = stockBalance.ContainsKey(item.Key) ? stockBalance[item.Key] ?? 0 : 0;
                    //            double currentQty = item.Value ?? 0;

                    //            if (currentQty > availableStock)
                    //            {
                    //                List<CustomKeyValuPair> barcodes = DropdownService.BarcodesKeyAndValue();
                    //                var SelectedItem = barcodes.Where(b => b.key == item.Key).FirstOrDefault();
                    //                InSufficientItem = SelectedItem.value;
                    //                InSufficientItemQty = availableStock;
                    //                isStockSufficient = false;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}

                    if (isStockSufficient)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            SqlTransaction transaction = connection.BeginTransaction();
                            SqlCommand command = connection.CreateCommand();
                            command.Transaction = transaction;
                            try
                            {
                                if (modelRecord.Master.SCODE != 0 && modelRecord.Master.SCODE != null)
                                {
                                    string maxIdQuery1 = "SELECT SACT_CODE FROM TBL_PARTY_TYPES WHERE PARTY_CODE = '" + modelRecord.Master.PARTY_CODE + "' AND SACT_CODE IS NOT NULL";
                                    using (SqlConnection connectionNew = new SqlConnection(new SQLService().getconnstring()))
                                    {
                                        SqlCommand commandNew1 = new SqlCommand(maxIdQuery1, connectionNew);
                                        connectionNew.Open();
                                        object result1 = commandNew1.ExecuteScalar();
                                        modelRecord.Master.SACODE = result1 == DBNull.Value ? 0 : Convert.ToInt32(result1);
                                    }
                                }
                                else
                                {
                                    modelRecord.Master.SACODE = 0;
                                }

                                string query = "", commQuery = "", detailQuery = "", voucherNo = string.Empty;
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
                                            "(TRAN_ID,V_DATE,VOUCHER_NO,PARTY_CODE,ACT_CODE,DOC,TERMS,BACT_CODE,CACT_CODE,BAMT,CAMT," +
                                            "REF,CURR_CODE,CRATE,REMARKS,SCODE,SACODE,BCODE,PERIOD_ID,COMM,COMM_VAL,COMM_AMT,DISC,DISC_RATE,CARTAGE,BTYPE," +
                                            "ADD_USER_ID,ADD_DATE,ADD_COMPUTER_NAME," +
                                            "ADD_IP_ADDRESS,EDIT_USER_ID,EDIT_DATE," +
                                            "EDIT_COMPUTER_NAME,EDIT_IP_ADDRESS,ADD_POSTALCODE," +
                                            "EDIT_POSTALCODE,ASTATUS,MENU_ID,DLT)" +
                                            "VALUES" +
                                            "('" + code + "','" + modelRecord.Master.V_DATE + "','" + voucherNo + "','" + modelRecord.Master.PARTY_CODE + "','" + modelRecord.Master.ACT_CODE + "','" + modelRecord.Master.DOC + "','" + modelRecord.Master.TERMS + "','" + modelRecord.Master.BACT + "','" + modelRecord.Master.CACT + "','" + modelRecord.Master.BAMT + "','" + modelRecord.Master.CAMT + "'," +
                                            "'" + modelRecord.Master.REF + "','" + modelRecord.Master.CURR_CODE + "','" + modelRecord.Master.CRATE + "','" + modelRecord.Master.REMARKS + "','" + modelRecord.Master.SCODE + "','" + modelRecord.Master.SACODE + "','" + branch + "','" + period + "','" + modelRecord.Master.COMM + "','" + modelRecord.Master.COMM_VAL + "','" + modelRecord.Master.COMM_AMT + "','" + modelRecord.Master.DISC + "','" + modelRecord.Master.DISC_RATE + "','" + modelRecord.Master.CARTAGE + "','" + modelRecord.Master.BTYPE + "'," +
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
                                                    TERMS = '" + modelRecord.Master.TERMS + @"',
                                                    ACT_CODE = '" + modelRecord.Master.ACT_CODE + @"',
                                                    CACT_CODE = '" + modelRecord.Master.CACT + @"',
                                                    BACT_CODE = '" + modelRecord.Master.BACT + @"',
                                                    BAMT = '" + modelRecord.Master.BAMT + @"',
                                                    CAMT = '" + modelRecord.Master.CAMT + @"',
                                                    REF = '" + modelRecord.Master.REF + @"',
                                                    CURR_CODE = '" + modelRecord.Master.CURR_CODE + @"',
                                                    CRATE = '" + modelRecord.Master.CRATE + @"',
                                                    DOC = '" + modelRecord.Master.DOC + @"',
                                                    REMARKS = '" + modelRecord.Master.REMARKS + @"',
                                                    SCODE = '" + modelRecord.Master.SCODE + @"',
                                                    SACODE = '" + modelRecord.Master.SACODE + @"',
                                                    COMM = '" + modelRecord.Master.COMM + @"',
                                                    COMM_AMT = '" + modelRecord.Master.COMM_AMT + @"',
                                                    COMM_VAL = '" + modelRecord.Master.COMM_VAL + @"',
                                                    DISC = '" + modelRecord.Master.DISC + @"',
                                                    DISC_RATE = '" + modelRecord.Master.DISC_RATE + @"',
                                                    CARTAGE = '" + modelRecord.Master.CARTAGE + @"',
                                                    BTYPE = '" + modelRecord.Master.BTYPE + @"',
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

                                StringBuilder insertQueryBuilder = new StringBuilder();
                                StringBuilder insertCommQueryBuilder = new StringBuilder();
                                StringBuilder updateQueryBuilder = new StringBuilder();
                                StringBuilder updateCommQueryBuilder = new StringBuilder();

                                bool allowInserts = true;
                                bool hasInserts = false;
                                bool hasUpdates = false;
                                int detailCode = GenerateNextDetailId(common, command);
                                foreach (var item in modelRecord.Detail.ToList())
                                {
                                    
                                    try
                                    {
                                        var amt = item.QTY * item.RATE;
                                        item.AMT = amt;
                                        item.NET_AMT = amt;

                                        if (item.RATE > 0 && (item.AMT == null || item.AMT == 0))
                                        {
                                            response.msg = "Something went wrong";
                                            response.msgType = 2;
                                            return response;
                                        }

                                        if (item.DT_CODE == null || item.DT_CODE == 0)
                                        {
                                            detailCode++;
                                            if (detailCode > 0)
                                            {
                                                if (!hasInserts)
                                                {
                                                    hasInserts = true;
                                                }

                                                if (stk_status == "Y")
                                                {
                                                    if (item.ITEM_CODE != null)
                                                    {
                                                        double procQty = 0;
                                                        string itemName = "";

                                                        if (stockBalance.ContainsKey(item.ITEM_CODE))
                                                        {
                                                            procQty = stockBalance[item.ITEM_CODE].Balance ?? 0;
                                                            itemName = stockBalance[item.ITEM_CODE].ItemName;
                                                        }

                                                        if ((item.QTY ?? 0) > procQty)
                                                        {
                                                            isDetailAdded = false;
                                                            response.msg = $@"Stock unavailable in {itemName}";
                                                            response.msgType = 2;
                                                            return response;
                                                        }

                                                        double totalSum = (item.QTY ?? 0) + procQty;
                                                    }
                                                }

                                                

                                                insertQueryBuilder.AppendLine(
                                                    $"INSERT INTO {detailTable} (TRAN_ID, DT_CODE, ITEM_CODE, PARTY_CODE, ACT_CODE, QTY, UNIT, QTY2, BAL_QTY, RATE, AMT, DISC, DISC_AMT, TAX, TAX_AMT, ADV, ADV_AMT, NET_AMT, DT_DESC, COLOR, SIZE, GRADE, WAREHOUSE, DEL_DATE, DUE_DATE, DUE_DAYS, VEH, BCODE, PERIOD_ID, ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, MENU_ID, DLT, CHK, PICK_ID, PICK_ID_D,HS_CODE) VALUES " +
                                                    $"('{modelRecord.Master.TRAN_ID}', '{detailCode}', '{item.ITEM_CODE}', '{item.PARTY_CODE}', '{item.ACT_CODE}', '{item.QTY}', " +
                                                    $"'{item.UNIT}', '{item.QTY2}', '{item.BAL_QTY}', '{item.RATE}', '{item.AMT}', '{item.DISC}', " +
                                                    $"'{item.DISC_AMT}', '{item.TAX}', '{item.TAX_AMT}', '{item.ADV}', '{item.ADV_AMT}', '{item.NET_AMT}', '{item.DT_DESC}', " +
                                                    $"'{item.COLOR}', '{item.SIZE}', '{item.GRADE}', '{item.WAREHOUSE}', '{item.DEL_DATE}', '{item.DUE_DATE}', " +
                                                    $"'{item.DUE_DAYS}', '{item.VEH}', '{branch}', '{period}', '{username}', " +
                                                    $"'{CommonService.GetDateTime("Pakistan Standard Time")}', '{Computer}', '{Ip}', '{username}', " +
                                                    $"'{CommonService.GetDateTime("Pakistan Standard Time")}', '{Computer}', '{Ip}', " +
                                                    $"'{Postal}', '{Postal}', '{menuID}', 'T', '{item.CHK}', '{item.PICK_ID}', '{item.PICK_ID_D}','{item.HS_CODE}');");
                                            }
                                            else
                                            {
                                                isDetailAdded = false;
                                            }
                                        }
                                        else
                                        {
                                            if (!hasUpdates)
                                            {
                                                hasUpdates = true;
                                            }


                                            double oldQty = 0;
                                            string previousQty = $@"SELECT QTY FROM {detailTable} WHERE TRAN_ID = {modelRecord.Master.TRAN_ID} AND DT_CODE = {item.DT_CODE} AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";

                                            using (SqlConnection connection1 = new SqlConnection(connectionString))
                                            {
                                                SqlCommand command1 = new SqlCommand(previousQty, connection1);
                                                connection1.Open();
                                                object result = command1.ExecuteScalar();
                                                oldQty = (result == null || result == DBNull.Value) ? 0 : Convert.ToDouble(result);
                                            }

                                            if (stk_status == "Y")
                                            {
                                                if (item.ITEM_CODE != null)
                                                {
                                                    double procQty = 0;
                                                    string itemName = "";

                                                    if (stockBalance.ContainsKey(item.ITEM_CODE))
                                                    {
                                                        procQty = stockBalance[item.ITEM_CODE].Balance ?? 0;
                                                        itemName = stockBalance[item.ITEM_CODE].ItemName;
                                                    }

                                                    double? finalStk = procQty + oldQty;

                                                    if ((item.QTY ?? 0) > finalStk)
                                                    {
                                                        isDetailAdded = false;
                                                        response.msg = $@"Stock unavailable in {itemName}";
                                                        response.msgType = 2;
                                                        return response;
                                                    }

                                                    double totalSum = (item.QTY ?? 0) + procQty;
                                                }
                                            }


                                            updateQueryBuilder.AppendLine(
                                                $"UPDATE {detailTable} SET " +
                                                $"ITEM_CODE = '{item.ITEM_CODE}', QTY = '{item.QTY}', PARTY_CODE = '{item.PARTY_CODE}', ACT_CODE = '{item.ACT_CODE}', UNIT = '{item.UNIT}', " +
                                                $"QTY2 = '{item.QTY2}', BAL_QTY = '{item.BAL_QTY}', RATE = '{item.RATE}', " +
                                                $"AMT = '{item.AMT}', DISC = '{item.DISC}', DISC_AMT = '{item.DISC_AMT}', " +
                                                $"TAX = '{item.TAX}', TAX_AMT = '{item.TAX_AMT}', ADV = '{item.ADV}', ADV_AMT = '{item.ADV_AMT}', NET_AMT = '{item.NET_AMT}', " +
                                                $"DT_DESC = '{item.DT_DESC}', COLOR = '{item.COLOR}', SIZE = '{item.SIZE}', " +
                                                $"GRADE = '{item.GRADE}', WAREHOUSE = '{item.WAREHOUSE}', DEL_DATE = '{item.DEL_DATE}', " +
                                                $"DUE_DATE = '{item.DUE_DATE}', DUE_DAYS = '{item.DUE_DAYS}', VEH = '{item.VEH}', " +
                                                $"CHK = '{item.CHK}', EDIT_USER_ID = '{username}', " +
                                                $"HS_CODE = '{item.HS_CODE}'," +
                                                $"EDIT_DATE = '{CommonService.GetDateTime("Pakistan Standard Time")}', " +
                                                $"EDIT_COMPUTER_NAME = '{Computer}', EDIT_IP_ADDRESS = '{Ip}', " +
                                                $"EDIT_POSTALCODE = '{Postal}', DLT = 'T' " +
                                                $"WHERE TRAN_ID = '{modelRecord.Master.TRAN_ID}' AND DT_CODE = '{item.DT_CODE}' " +
                                                $"AND BCODE = '{branch}' AND PERIOD_ID = '{period}';");
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        isDetailAdded = false;
                                    }
                                }

                                if (b_i == "I")
                                {
                                    if (modelRecord.Commission.Count > 0 && modelRecord.Commission.First().ITEM_CODE != null)
                                    {
                                        var BILL_TRAN_ID = code == 0 ? modelRecord.Master.TRAN_ID : code;
                                        if (modelRecord.Commission.Last().SALESMAN == modelRecord.Master.SCODE)
                                        {
                                            dt = GetComm(Convert.ToInt32(modelRecord.Master.SCODE));
                                            foreach (var item in modelRecord.Commission.ToList())
                                            {
                                                try
                                                {
                                                    if (item.DT_CODE == null || item.DT_CODE == 0)
                                                    {

                                                        int CommCode = GenerateNextDetailId(common, command, "Commission");
                                                        if (CommCode > 0)
                                                        {
                                                            if (!hasInserts)
                                                            {
                                                                hasInserts = true;
                                                            }
                                                            insertCommQueryBuilder.AppendLine($@"
                                                    INSERT INTO [dbo].[TBL_COMM_GEN]
                                                    ([BILL_TRAN_ID],[BILL_MENU_ID],[BCODE],[PERIOD_ID],[TRAN_ID],[DT_CODE],[SALESMAN],[SACT_CODE],[ITEM_CODE],[COMM_UNIT],[COMM_VALUE],
                                                     [ADD_USER_ID],[ADD_DATE],[ADD_COMPUTER_NAME],[ADD_IP_ADDRESS],[EDIT_USER_ID],[EDIT_DATE],[EDIT_COMPUTER_NAME],[EDIT_IP_ADDRESS],
                                                     [ADD_POSTALCODE],[EDIT_POSTALCODE],[MENU_ID],[DLT])
                                                    VALUES ('{BILL_TRAN_ID}','{common.MenuID}','{common.Branch}','{common.Period}','{item.TRAN_ID}','{CommCode}','{dt.Rows[0]["SALESMAN"]}','{dt.Rows[0]["SACT_CODE"]}',
                                                     '{item.ITEM_CODE}','{item.COMM_UNIT}','{item.COMM_VALUE}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{Postal}','{Postal}','{menuID}','T');");
                                                        }

                                                    }
                                                    else
                                                    {
                                                        updateCommQueryBuilder.AppendLine($@"
                                                UPDATE [dbo].[TBL_COMM_GEN] SET 
                                                 [BILL_MENU_ID]='{common.MenuID}', [BCODE]='{common.Branch}', [PERIOD_ID]='{common.Period}',
                                                 [SALESMAN]='{item.SALESMAN}', [SACT_CODE]='{item.SACT_CODE}', [ITEM_CODE]='{item.ITEM_CODE}',
                                                 [COMM_UNIT]='{item.COMM_UNIT}', [COMM_VALUE]='{item.COMM_VALUE}', [EDIT_USER_ID]='{username}',
                                                 [EDIT_DATE]='{CommonService.GetDateTime("Pakistan Standard Time")}', [EDIT_COMPUTER_NAME]='{Computer}',
                                                 [EDIT_IP_ADDRESS]='{Ip}', [EDIT_POSTALCODE]='{Postal}', [MENU_ID]='{menuID}'
                                                WHERE [TRAN_ID]='{item.TRAN_ID}' AND [DT_CODE] = '{item.DT_CODE}' AND [DLT]='T';");
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    isDetailAdded = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            query = @$"UPDATE TBL_COMM_GEN SET DLT = 'F' WHERE BILL_TRAN_ID = '" + BILL_TRAN_ID + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
                                            command.CommandText = query;
                                            command.ExecuteNonQuery();

                                            dt = GetComm(Convert.ToInt32(modelRecord.Master.SCODE));
                                            var Tran_Id = GenerateNextId(common, command, "Commission");
                                            int CommCode = GenerateNextDetailId(common, command, "Commission");
                                            foreach (DataRow row in dt.Rows)
                                            {
                                                try
                                                {
                                                    if (CommCode > 0)
                                                    {
                                                        if (!hasInserts)
                                                        {
                                                            hasInserts = true;
                                                        }
                                                        insertCommQueryBuilder.AppendLine($@"
                                                    INSERT INTO [dbo].[TBL_COMM_GEN]
                                                    ([BILL_TRAN_ID],[BILL_MENU_ID],[BCODE],[PERIOD_ID],[TRAN_ID],[DT_CODE],[SALESMAN],[SACT_CODE],[ITEM_CODE],[COMM_UNIT],[COMM_VALUE],
                                                     [ADD_USER_ID],[ADD_DATE],[ADD_COMPUTER_NAME],[ADD_IP_ADDRESS],[EDIT_USER_ID],[EDIT_DATE],[EDIT_COMPUTER_NAME],[EDIT_IP_ADDRESS],
                                                     [ADD_POSTALCODE],[EDIT_POSTALCODE],[MENU_ID],[DLT])
                                                    VALUES ('{BILL_TRAN_ID}','{common.MenuID}','{common.Branch}','{common.Period}','{Tran_Id}','{CommCode}','{row["SALESMAN"]}','{row["SACT_CODE"]}',
                                                     '{row["ITEM_CODE"]}','{row["COMM_UNIT"]}','{row["COMM_VALUE"]}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{Postal}','{Postal}','{menuID}','T');");
                                                    }
                                                    else
                                                    {
                                                        isDetailAdded = false;
                                                    }
                                                    CommCode++;
                                                }
                                                catch (Exception)
                                                {
                                                    isDetailAdded = false;
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var BILL_TRAN_ID = code == 0 ? modelRecord.Master.TRAN_ID : code;

                                        query = @$"UPDATE TBL_COMM_GEN SET DLT = 'F' WHERE BILL_TRAN_ID = '" + BILL_TRAN_ID + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();

                                        dt = GetComm(Convert.ToInt32(modelRecord.Master.SCODE));
                                        var Tran_Id = GenerateNextId(common, command, "Commission");
                                        int CommCode = GenerateNextDetailId(common, command, "Commission");

                                        foreach (DataRow row in dt.Rows)
                                        {
                                            try
                                            {
                                                if (CommCode > 0)
                                                {
                                                    if (!hasInserts)
                                                    {
                                                        hasInserts = true;
                                                    }
                                                    insertCommQueryBuilder.AppendLine($@"
                                                    INSERT INTO [dbo].[TBL_COMM_GEN]
                                                    ([BILL_TRAN_ID],[BILL_MENU_ID],[BCODE],[PERIOD_ID],[TRAN_ID],[DT_CODE],[SALESMAN],[SACT_CODE],[ITEM_CODE],[COMM_UNIT],[COMM_VALUE],
                                                     [ADD_USER_ID],[ADD_DATE],[ADD_COMPUTER_NAME],[ADD_IP_ADDRESS],[EDIT_USER_ID],[EDIT_DATE],[EDIT_COMPUTER_NAME],[EDIT_IP_ADDRESS],
                                                     [ADD_POSTALCODE],[EDIT_POSTALCODE],[MENU_ID],[DLT])
                                                    VALUES ('{BILL_TRAN_ID}','{common.MenuID}','{common.Branch}','{common.Period}','{Tran_Id}','{CommCode}','{row["SALESMAN"]}','{row["SACT_CODE"]}',
                                                     '{row["ITEM_CODE"]}','{row["COMM_UNIT"]}','{row["COMM_VALUE"]}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{username}','{CommonService.GetDateTime("Pakistan Standard Time")}',
                                                     '{Computer}','{Ip}','{Postal}','{Postal}','{menuID}','T');");
                                                }
                                                else
                                                {
                                                    isDetailAdded = false;
                                                }
                                                CommCode++;
                                            }
                                            catch (Exception)
                                            {
                                                isDetailAdded = false;
                                            }
                                        }
                                    }
                                }


                                if (hasInserts)
                                {
                                    command.CommandText = insertQueryBuilder.ToString() + ";" + insertCommQueryBuilder.ToString();
                                    command.ExecuteNonQuery();
                                }
                                if (hasUpdates)
                                {
                                    command.CommandText = updateQueryBuilder.ToString() + ";" + updateCommQueryBuilder.ToString();
                                    command.ExecuteNonQuery();
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
                    else
                    {
                        response.data = "";
                        response.msg = $"{InSufficientItem} has only {InSufficientItemQty} in stock.";
                        response.msgType = 2;
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

        public Dictionary<int?, double?> PreviousStockInBill(string table, int? TRAN_ID, string period, string branch)
        {
            List<CurrentItemsInBill> jsonDataResult = new List<CurrentItemsInBill>();
            string query;
            Dictionary<int?, double?> stock = new();
            using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
            {
                query = $"SELECT TRAN_ID,DT_CODE,ITEM_CODE, BAL_QTY FROM {table} " +
                    " WHERE DLT = 'T' AND TRAN_ID = '" + TRAN_ID + "' AND BCODE = '" + branch + "' AND PERIOD_ID = '" + period + "'";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var row = new CurrentItemsInBill
                    {
                        ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                        QTY = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BAL_QTY"])
                    };
                    jsonDataResult.Add(row);
                }
                reader.Close();
            }
            if (jsonDataResult is not null)
            {
                foreach (var item in jsonDataResult)
                {
                    if (!stock.ContainsKey(item.ITEM_CODE))
                    {
                        stock.Add(Convert.ToInt32(item.ITEM_CODE), Convert.ToDouble(item.QTY));
                    }
                    else
                    {
                        stock[item.ITEM_CODE] += Convert.ToDouble(item.QTY);
                    }
                }
            }
            return stock;
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
                if (Menu.data != null)
                {
                    var menu = (Menu)Menu.data;
                    table = menu.TABLE1;
                }

                if (!String.IsNullOrWhiteSpace(table))
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

                PurchaseBill purchaseBill = new PurchaseBill();
                List<PurchaseBillDetail> purchaseBillDetailList = new List<PurchaseBillDetail>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    string detailQuery = $@"SELECT * FROM {table2} WHERE TRAN_ID = {record.TRAN_ID} AND DLT = 'T' AND BCODE = {common.Branch} AND PERIOD_ID = {common.Period}";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        purchaseBill = new PurchaseBill
                        {
                            V_DATE = Convert.ToDateTime(reader["V_DATE"]),
                            PARTY_CODE = Convert.ToInt32(reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                            CACT = Convert.ToInt32(reader["CACT_CODE"]),
                            BACT = Convert.ToInt32(reader["BACT_CODE"]),
                            SCODE = Convert.ToInt32(reader["SCODE"]),
                            SACODE = Convert.ToInt32(reader["SACODE"]),
                            COMM = Convert.ToDouble(reader["COMM"]),
                            REF = Convert.ToString(reader["REF"]),
                            REMARKS = Convert.ToString(reader["REMARKS"]),
                            BTYPE = Convert.ToString(reader["BTYPE"]),
                            BCODE = Convert.ToInt32(reader["BCODE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
                            DISC = Convert.ToDouble(reader["DISC"]),
                            HS_CODE = Convert.ToString(reader["HS_CODE"]),
                            DOC = Convert.ToString(reader["DOC"]),
                            CURR_CODE = Convert.ToInt32(reader["CURR_CODE"]),
                            CRATE = Convert.ToDouble(reader["CRATE"]),
                            BAMT = Convert.ToDouble(reader["BAMT"]),
                            CAMT = Convert.ToDouble(reader["CAMT"]),
                        };
                    }

                    reader.Close();

                    SqlCommand detail_Command = new SqlCommand(detailQuery, connection);
                    SqlDataReader detail_Reader = detail_Command.ExecuteReader();
                    while (detail_Reader.Read())
                    {
                        var row = new PurchaseBillDetail
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
                            WAREHOUSE = Convert.ToInt32(detail_Reader["WAREHOUSE"]),
                            DEL_DATE = Convert.ToDateTime(detail_Reader["DEL_DATE"]),
                            DUE_DATE = Convert.ToDateTime(detail_Reader["DUE_DATE"]),
                            DUE_DAYS = Convert.ToInt32(detail_Reader["DUE_DAYS"]),
                            VEH = Convert.ToString(detail_Reader["VEH"]),
                            CHK = detail_Reader["CHK"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["CHK"]),
                            PICK_ID = Convert.ToInt32(detail_Reader["PICK_ID"]),
                            PICK_ID_D = detail_Reader["PICK_ID_D"] == DBNull.Value ? 0 : Convert.ToInt32(detail_Reader["PICK_ID_D"]),
                            ADV = Convert.ToDouble(detail_Reader["ADV"]),
                            ADV_AMT = Convert.ToDouble(detail_Reader["ADV_AMT"]),
                            PARTY_CODE = Convert.ToInt32(detail_Reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(detail_Reader["ACT_CODE"]),
                        };
                        purchaseBillDetailList.Add(row);
                    }

                    detail_Reader.Close();
                    connection.Close();
                }

                var customRequisition = new CustomPurchaseBill
                {
                    Master = purchaseBill,
                    Detail = purchaseBillDetailList
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

        public MyHttpResponseMessage DeletePurchaseBillDetailByCode(int code, Common common)
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
        public MyHttpResponseMessage DeleteCommDetailByCode(int code, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
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
                        string query = $"UPDATE TBL_COMM_GEN SET DLT = 'F'" +
                            $" WHERE DT_CODE = '{code}' AND BCODE = '{branch}' AND PERIOD_ID = '{period}'";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.ExecuteNonQuery();
                        response.msgType = 1;
                        response.msg = "Record Deleted Successfully";
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
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return response;
        }

        public MyHttpResponseMessage GetDataForReport(PurchaseBillRDLCReport modelRecord, DataTable dataTable, DataTable taxDataTable, DataTable inspectionServiceChargesDetails, DataTable reportDetailsDDJ, CustomMenuDetail menuDetails, Company currentCompany, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            PurchaseBillRDLCReport masterData = new PurchaseBillRDLCReport();
            CustomPurchaseBillForPrintReport reportData = new CustomPurchaseBillForPrintReport();
            var Menu = _menuRepository.GetMenu(common.MenuID);
            string? table = string.Empty, detailTable = string.Empty;
            string? prefix = string.Empty;
            if (Menu.data != null)
            {
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
                detailTable = menu.TABLE2;
                prefix = menu.PERFIX;
            }
            try
            {
                string topQuery = "", query = "";
                if (menuDetails.MD_ID == 17 || menuDetails.MD_ID == 68)
                {
                    query = $@"EXEC PPROC 143,'{modelRecord.S_DATE}','{modelRecord.V_DATE}','{modelRecord.ACT_CODE}','{modelRecord.PARTY_CODE}','{common.Branch}','{common.Period}','','','{modelRecord.TRAN_ID}','{table}','{detailTable}','','','',''";
                }
                else if (menuDetails.MD_ID == 54)
                {
                    topQuery = @$"SELECT  CM.C_NAME AS COMPANY_NAME,BR.B_ADDRESS,BR.B_TEL,BR.B_GST,BR.B_NTN,
                                 A.TRAN_ID ,A.V_DATE, A.VOUCHER_NO ,
                                +'0'+CH.ACT_GR_CODE+'-'+CONVERT(NVARCHAR(50),PT.PARTY_CODE)
                                AS ACT_GRCODE,
                                A.PARTY_CODE ,PT.PARTY_NAME, A.REF, C.DESCR2 AS CURR, C.SHORT_NAME AS CURR_SIG 
                                  FROM  {table}  A
                                 LEFT OUTER JOIN TBL_BRANCH BR
                                 ON BR.BCODE = A.BCODE
                                 LEFT OUTER JOIN TBL_COMPANY CM
                                 ON CM.CCODE = BR.CCODE
                                 LEFT OUTER JOIN TBL_PARTY_TYPES PT
                                 ON PT.PARTY_CODE = A.PARTY_CODE AND PT.ACT_CODE = A.ACT_CODE
                                 LEFT OUTER JOIN TBL_CHART CH
                                 ON CH.ACT_CODE = PT.ACT_CODE
								 LEFT OUTER JOIN TBL_CURRENCY C
								 ON C.CODE = A.CURR_CODE 
                                WHERE A.BCODE = '{common.Branch}' AND A.DLT = 'T' And A.PERIOD_ID = '{common.Period}' AND A.TRAN_ID =  '{modelRecord.TRAN_ID}' 
                                Group By
                                BR.B_NAME,BR.B_ADDRESS,BR.B_TEL,BR.B_GST,BR.B_NTN,
                                A.TRAN_ID  ,A.V_DATE, A.VOUCHER_NO ,
                                CH.ACT_GR_CODE,PT.PARTY_CODE,A.REF,
                                A.PARTY_CODE ,PT.PARTY_NAME,
                                A.BCODE,CM.C_NAME,C.DESCR2,C.SHORT_NAME";

                    query = @$"SELECT  MPOM.TRAN_ID,E.ITEM_NAME AS ITEM_NAME, MPOM.JOB_NO, B.DT_DESC, PT.PARTY_NAME AS BUYER_NAME, B.QTY AS QTY, B.AMT AS AMT, 
                                A.COMM AS COMMISSION, (B.AMT * A.COMM / 100) AS COMMISSION_AMOUNT 
                                FROM  {detailTable} B
								LEFT OUTER JOIN TBL_MPOD_DETAIL MPOD ON MPOD.DT_CODE = B.PICK_ID_D
                                LEFT OUTER JOIN TBL_MPOD_MASTER MPOM ON MPOM.TRAN_ID = MPOD.TRAN_ID
                                LEFT OUTER JOIN TBL_SB_MASTER A ON A.TRAN_ID = B.TRAN_ID 
                                LEFT OUTER JOIN TBL_ITEMSMASTER E ON E.ITEM_CODE = B.ITEM_CODE
                                LEFT OUTER JOIN TBL_PARTY_TYPES PT ON PT.PARTY_CODE = B.PARTY_CODE AND PT.ACT_CODE = B.ACT_CODE
                                WHERE B.BCODE = '{common.Branch}' AND B.DLT = 'T' And B.PERIOD_ID = '{common.Period}' AND B.TRAN_ID = '{modelRecord.TRAN_ID}' 
                                Group By MPOM.TRAN_ID,B.TRAN_ID , B.DT_DESC, B.QTY, PT.PARTY_NAME, B.AMT, E.ITEM_ID, E.ITEM_NAME, A.COMM, B.BCODE, MPOM.JOB_NO";
                }
                else if (menuDetails.MD_ID == 18)
                {
                    topQuery = @$"SELECT CM.C_NAME AS COMPANY_NAME,BR.B_ADDRESS,BR.B_TEL,BR.B_GST,BR.B_NTN,
                                 A.TRAN_ID ,A.V_DATE, A.VOUCHER_NO ,
                                +'0'+CH.ACT_GR_CODE+'-'+CONVERT(NVARCHAR(50),PT.PARTY_CODE)
                                AS ACT_GRCODE,
                                A.PARTY_CODE ,PT.PARTY_NAME , ISNULL(A.DISC, 0) AS DISC
                                  FROM {table} A   
                                 LEFT OUTER JOIN TBL_BRANCH BR
                                 ON BR.BCODE = A.BCODE
                                 LEFT OUTER JOIN TBL_COMPANY CM
                                 ON CM.CCODE = BR.CCODE
                                 LEFT OUTER JOIN TBL_PARTY_TYPES PT
                                 ON PT.PARTY_CODE = A.PARTY_CODE AND PT.ACT_CODE = A.ACT_CODE
                                 LEFT OUTER JOIN TBL_CHART CH
                                 ON CH.ACT_CODE = PT.ACT_CODE
 
                                WHERE A.BCODE = '{common.Branch}' And A.PERIOD_ID = '{common.Period}' AND A.TRAN_ID = '{modelRecord.TRAN_ID}' 
                                AND A.DLT = 'T' AND A.ASTATUS = 'Y'
                                Group By
                                BR.B_NAME, BR.B_ADDRESS,BR.B_TEL,BR.B_GST,BR.B_NTN,
                                A.TRAN_ID, A.V_DATE, A.VOUCHER_NO ,
                                CH.ACT_GR_CODE,PT.PARTY_CODE,
                                A.PARTY_CODE ,PT.PARTY_NAME,
                                A.BCODE,CM.C_NAME, A.DISC";

                    query = @$"SELECT  E.ITEM_NAME+' - '+E.REMARKS AS ITEM_NAME ,B.DT_DESC,
                            SUM(B.QTY) AS QTY 
                              FROM {detailTable} B 
                             LEFT OUTER JOIN TBL_BARCODE BG
                             ON BG.CODE = B.ITEM_cODE
                             LEFT OUTER JOIN TBL_ITEMSMASTER E
                             ON E.ITEM_CODE = B.ITEM_CODE
                             LEFT OUTER JOIN TBL_SIZE BGS
                             ON BGS.GROUP_CODE = BG.SIZE
                             LEFT OUTER JOIN TBL_COLOR BGC
                             ON BGC.GROUP_CODE = BG.COLOR
                             WHERE B.BCODE = '{common.Branch}' And B.PERIOD_ID = '{common.Period}' AND B.TRAN_ID = '{modelRecord.TRAN_ID}' 
                            AND B.DLT = 'T' 
                            Group By E.ITEM_ID,E.ITEM_NAME, E.REMARKS,B.DT_DESC";
                }
                else if (menuDetails.MD_ID == 21)
                {
                    query = @$"SELECT 
                            PT.PARTY_NAME AS BUYER_NAME, M.REMARKS, D.HS_CODE, M.VOUCHER_NO,PT.PADDRESS AS PARTY_ADDRESS,
                            M.V_DATE,PT.CELL , '' AS PURCHASE_ORDER,PT.NTN,PT.SERVICE_TAX AS STRN,
                            IT.ITEM_NAME, D.DT_DESC,U.GROUP_NAME AS UNIT,D.BAL_QTY,D.RATE,D.AMT,D.TAX_AMT,(D.AMT+D.TAX_AMT) AS NET_AMT, 
                            B.B_ADDRESS AS BRANCH_ADDRESS, B.B_TEL AS BRANCH_PHONE, B.EMAIL AS BRANCH_EMAIL, B.B_GST AS BRANCH_GST, B.B_NTN AS BRANCH_NTN ,M.REF , B.TERMS
                            , M.EDIT_USER_ID, D.DEL_DATE
                            FROM {table} M
                            LEFT OUTER JOIN {detailTable} D
                            ON D.TRAN_ID = M.TRAN_ID AND M.BCODE = D.BCODE AND M.PERIOD_ID = D.PERIOD_ID
                            LEFT OUTER JOIN TBL_PARTY_TYPES PT
                            ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE
                            LEFT OUTER JOIN TBL_ITEMSMASTER IT
                            ON IT.ITEM_CODE = D.ITEM_CODE
                            LEFT OUTER JOIN TBL_UNIT U
                            ON U.GROUP_CODE = D.UNIT
                            LEFT OUTER JOIN TBL_BRANCH B
                            ON B.BCODE = M.BCODE 
                            WHERE M.BCODE = '{common.Branch}' And M.PERIOD_ID = '{common.Period}' AND M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.DLT = 'T' AND D.DLT = 'T'";
                }
                else if (menuDetails.MD_ID == 22)
                {
                    query = @$"SELECT 
                            PT.PARTY_NAME AS BUYER_NAME,M.VOUCHER_NO,PT.PADDRESS AS PARTY_ADDRESS,
                            M.V_DATE,PT.CELL , '' AS PURCHASE_ORDER,PT.NTN,PT.SERVICE_TAX AS STRN,
                            D.DT_DESC AS ITEM_NAME,U.GROUP_NAME AS UNIT,D.BAL_QTY, 
                            B.B_ADDRESS AS BRANCH_ADDRESS, B.B_TEL AS BRANCH_PHONE, B.EMAIL AS BRANCH_EMAIL, B.B_GST AS BRANCH_GST, B.B_NTN AS BRANCH_NTN ,M.REF , B.TERMS
                            , M.EDIT_USER_ID
                            FROM {table} M
                            LEFT OUTER JOIN {detailTable} D
                            ON D.TRAN_ID = M.TRAN_ID AND M.BCODE = D.BCODE AND M.PERIOD_ID = D.PERIOD_ID
                            LEFT OUTER JOIN TBL_PARTY_TYPES PT
                            ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE
                            LEFT OUTER JOIN TBL_ITEMSMASTER IT
                            ON IT.ITEM_CODE = D.ITEM_CODE
                            LEFT OUTER JOIN TBL_UNIT U
                            ON U.GROUP_CODE = IT.IUNIT_CODE
                            LEFT OUTER JOIN TBL_BRANCH B
                            ON B.BCODE = M.BCODE 
                            WHERE M.BCODE = '{common.Branch}' And M.PERIOD_ID = '{common.Period}' AND M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.DLT = 'T' AND D.DLT = 'T'";
                }
                else if (menuDetails.MD_ID == 25)
                {
                    query = @$"SELECT M.V_DATE, M.VOUCHER_NO, M.REMARKS, IT.ITEM_CODE,
                            PT.PARTY_NAME AS BUYER_NAME, PT.PADDRESS AS PARTY_ADDRESS,
                            PT.CELL , '' AS PURCHASE_ORDER, PT.NTN, PT.SERVICE_TAX AS STRN,
                            IT.ITEM_NAME, D.DISC AS UNIT, D.BAL_QTY, D.RATE, D.AMT, D.TAX_AMT, (D.RATE - (D.RATE * D.DISC / 100)) * D.BAL_QTY AS NET_AMT, 
                            B.B_ADDRESS AS BRANCH_ADDRESS, B.B_TEL AS BRANCH_PHONE, B.EMAIL AS BRANCH_EMAIL, B.B_GST AS BRANCH_GST, B.B_NTN AS BRANCH_NTN , M.REF , B.TERMS,
                            M.EDIT_USER_ID
                            FROM {table} M
                            LEFT OUTER JOIN {detailTable} D
                            ON D.TRAN_ID = M.TRAN_ID AND M.BCODE = D.BCODE AND M.PERIOD_ID = D.PERIOD_ID
                            LEFT OUTER JOIN TBL_PARTY_TYPES PT
                            ON PT.PARTY_CODE = M.PARTY_CODE AND PT.ACT_CODE = M.ACT_CODE
                            LEFT OUTER JOIN TBL_ITEMSMASTER IT
                            ON IT.ITEM_CODE = D.ITEM_CODE
                            LEFT OUTER JOIN TBL_UNIT U
                            ON U.GROUP_CODE = IT.IUNIT_CODE
                            LEFT OUTER JOIN TBL_BRANCH B
                            ON B.BCODE = M.BCODE 
                            WHERE M.BCODE = '{common.Branch}' And M.PERIOD_ID = '{common.Period}' AND M.TRAN_ID = '{modelRecord.TRAN_ID}' AND M.DLT = 'T' AND D.DLT = 'T'";
                }
                else if (menuDetails.REPORT_NAME == "SaleInvoiceDDJ")
                {
                    query = $@"EXEC PROC_PRINT '{table}','{detailTable}','','','{common.Branch}','{common.Period}','{modelRecord.TRAN_ID}','','','{menuDetails.REPORT_NAME}'";
                }



                if (menuDetails.REPORT_NAME == "SaleInvoiceDDJ")
                {
                    using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
                        masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
                        masterData.COMPANY_LOGO = currentCompany.C_LOGO;

                        if (reader.Read())
                        {
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

                            masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                            masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                            masterData.USER = reader["USER_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["USER_NAME"]);
                            masterData.STATUS = Convert.ToString(reader["ASTATUS"]);
                            masterData.PARTY_NAME = reader["PARTY_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["PARTY_NAME"]);
                            //masterData.ORDER_TYPE = reader["ORDER_TYPE"] == DBNull.Value ? "" : Convert.ToString(reader["ORDER_TYPE"]);
                            //masterData.TERMS = reader["TERMS"] == DBNull.Value ? "" : Convert.ToString(reader["TERMS"]);
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
                            DataRow dataRow = reportDetailsDDJ.NewRow();
                            dataRow["Item"] = Convert.ToString(reader["ITEM"]);
                            //dataRow["Qty"] = Convert.ToString(reader["BAL_QTY"]);
                            dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
                            dataRow["Rate"] = Convert.ToString(reader["RATE"]);
                            dataRow["Amt"] = Convert.ToString(reader["AMT"]);
                            dataRow["Disc"] = Convert.ToString(reader["DISC"]);
                            dataRow["DiscAmt"] = Convert.ToString(reader["DISC_AMT"]);
                            dataRow["Tax"] = Convert.ToString(reader["TAX"]);
                            dataRow["TaxAmt"] = Convert.ToString(reader["TAX_AMT"]);
                            dataRow["Adv"] = Convert.ToString(reader["ADV"]);
                            dataRow["AdvAmt"] = Convert.ToString(reader["ADV_AMT"]);
                            dataRow["NetAmt"] = Convert.ToString(reader["NET_AMT"]);
                            dataRow["Grade"] = Convert.ToString(reader["GRADE"]);
                            dataRow["DeliveryDate"] = Convert.ToDateTime(reader["DEL_DATE"]);
                            reportDetailsDDJ.Rows.Add(dataRow);
                        }
                        reader.Close();
                    }
                }
                else
                {
                    masterData.COMPANY_NAME = currentCompany.C_NAME;
                    masterData.COMPANY_ADDRESS = currentCompany.C_ADDRESS;
                    masterData.COMPANY_PHONE = currentCompany.C_TEL;
                    masterData.COMPANY_LOGO = currentCompany.C_LOGO;
                    masterData.COMPANY_WATER = currentCompany.C_WATER;
                    masterData.HEADER_NAME = $"{menuDetails.MD_NAME}";
                    masterData.REPORT_NAME = $"{menuDetails.REPORT_NAME}";
                    masterData.SIG1 = $"{menuDetails.MENU_SIG1}";
                    masterData.SIG2 = $"{menuDetails.MENU_SIG2}";
                    masterData.SIG3 = $"{menuDetails.MENU_SIG3}";
                    masterData.SIG4 = $"{menuDetails.MENU_SIG4}";
                    masterData.MENU_TERMS = $"{menuDetails.MENU_TERMS}";
                    masterData.PREFIX = prefix;

                    if (menuDetails.MD_ID == 17)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                                masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                masterData.PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]);
                                masterData.ACT_GRCODE = Convert.ToString(reader["ACT_GRCODE"]);
                                masterData.BRANCH_ADDRESS = Convert.ToString(reader["B_ADDRESS"]);
                                masterData.BRANCH_PHONE = Convert.ToString(reader["B_TEL"]);
                                masterData.COMMENT = Convert.ToString(reader["REMARKS"]);
                                masterData.DISC = Convert.ToDecimal(reader["DISC"]);
                                masterData.CARTAGE = reader["CARTAGE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CARTAGE"]);
                                masterData.BAMT = reader["BAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAMT"]);
                                masterData.CAMT = reader["CAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CAMT"]);
                            }
                            reader.Close();
                        }
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            {
                                while (reader.Read())
                                {
                                    DataRow dataRow = dataTable.NewRow();
                                    dataRow["ItemName"] = reader["ITEM_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["ITEM_NAME"]);
                                    dataRow["Size"] = reader["SIZE_NAME"] == DBNull.Value ? "" : Convert.ToString(reader["SIZE_NAME"]);
                                    dataRow["Qty"] = reader["QTY"] == DBNull.Value ? "" : Convert.ToString(reader["QTY"]);
                                    dataRow["Rate"] = reader["RATE"] == DBNull.Value ? "" : Convert.ToString(reader["RATE"]);
                                    dataRow["Code"] = reader["CODE"] == DBNull.Value ? "" : Convert.ToString(reader["CODE"]);
                                    dataRow["Desc"] = reader["DT_DESC"] == DBNull.Value ? "" : Convert.ToString(reader["DT_DESC"]);
                                    dataRow["Amount"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
                                    dataRow["Balance"] = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BALANCE"]);

                                    dataTable.Rows.Add(dataRow);
                                }
                            }

                            reader.Close();
                        }
                    }
                    if (menuDetails.MD_ID == 68)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                                masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                masterData.PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]);
                                masterData.ACT_GRCODE = Convert.ToString(reader["ACT_GRCODE"]);
                                masterData.BRANCH_ADDRESS = Convert.ToString(reader["B_ADDRESS"]);
                                masterData.BRANCH_PHONE = Convert.ToString(reader["B_TEL"]);
                                masterData.COMMENT = Convert.ToString(reader["REMARKS"]);
                                masterData.DISC = Convert.ToDecimal(reader["DISC"]);
                                masterData.CARTAGE = reader["CARTAGE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CARTAGE"]);
                                masterData.BAMT = reader["BAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAMT"]);
                                masterData.CAMT = reader["CAMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CAMT"]);
                            }
                            reader.Close();
                        }
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            {
                                while (reader.Read())
                                {
                                    DataRow dataRow = dataTable.NewRow();
                                    dataRow["ItemName"] = Convert.ToString(reader["ITEM_NAME"]);
                                    dataRow["Image"] = Convert.ToString(reader["IPIC"]);
                                    dataRow["Size"] = Convert.ToString(reader["SIZE_NAME"]);
                                    dataRow["Qty"] = Convert.ToString(reader["QTY"]);
                                    dataRow["Rate"] = Convert.ToString(reader["RATE"]);
                                    dataRow["Code"] = Convert.ToString(reader["CODE"]);
                                    dataRow["Amount"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
                                    dataRow["Balance"] = reader["BALANCE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BALANCE"]);

                                    dataTable.Rows.Add(dataRow);
                                }
                            }

                            reader.Close();
                        }
                    }
                    if (modelRecord.MD_ID == 18)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(topQuery, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            if (reader.Read())
                            {
                                masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                                masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                masterData.PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]);
                                masterData.ACT_GRCODE = Convert.ToString(reader["ACT_GRCODE"]);
                                masterData.BRANCH_ADDRESS = Convert.ToString(reader["B_ADDRESS"]);
                                masterData.BRANCH_PHONE = Convert.ToString(reader["B_TEL"]);
                                masterData.DISC = Convert.ToDecimal(reader["DISC"]);
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
                                dataRow["ItemName"] = Convert.ToString(reader["ITEM_NAME"]);
                                dataRow["Desc"] = Convert.ToString(reader["DT_DESC"]);
                                dataRow["Size"] = "";
                                dataRow["Qty"] = Convert.ToString(reader["QTY"]);
                                dataRow["Rate"] = 0;
                                dataRow["Amount"] = 0;

                                dataTable.Rows.Add(dataRow);
                            }
                            reader.Close();
                        }

                    }
                    if (menuDetails.MD_ID == 54)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(topQuery, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                masterData.INVOICE_NUMBER = Convert.ToString(reader["VOUCHER_NO"]);
                                masterData.DATE = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                masterData.PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]);
                                masterData.REFERENCENO = Convert.ToString(reader["REF"]);
                                masterData.BRANCH_ADDRESS = Convert.ToString(reader["B_ADDRESS"]);
                                masterData.BRANCH_PHONE = Convert.ToString(reader["B_TEL"]);
                                masterData.CURR = Convert.ToString(reader["CURR"]);
                                masterData.CURR_SIG = Convert.ToString(reader["CURR_SIG"]);
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
                                DataRow dataRow = inspectionServiceChargesDetails.NewRow();
                                dataRow["Desc"] = Convert.ToString(reader["DT_DESC"]);
                                dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
                                dataRow["JobNo"] = Convert.ToString(reader["JOB_NO"]);
                                dataRow["Buyer"] = Convert.ToString(reader["BUYER_NAME"]);
                                dataRow["Qty"] = reader["QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["QTY"]);
                                dataRow["Amount"] = reader["AMT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["AMT"]);
                                dataRow["Comm"] = reader["COMMISSION"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COMMISSION"]);
                                dataRow["CommAmt"] = reader["COMMISSION_AMOUNT"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COMMISSION_AMOUNT"]);
                                inspectionServiceChargesDetails.Rows.Add(dataRow);
                            }

                            reader.Close();
                        }
                    }
                    else if (menuDetails.MD_ID == 21)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                DataRow dataRow = taxDataTable.NewRow();
                                masterData.EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["HsCode"] = Convert.ToString(reader["HS_CODE"]);
                                dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
                                dataRow["Desc"] = Convert.ToString(reader["DT_DESC"]);
                                dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
                                dataRow["Qty"] = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAL_QTY"]); ;
                                dataRow["Rate"] = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RATE"]); ;
                                dataRow["Amt"] = Convert.ToInt32(reader["AMT"]);
                                dataRow["TaxAmt"] = Convert.ToInt32(reader["TAX_AMT"]);
                                dataRow["NetAmt"] = Convert.ToInt32(reader["NET_AMT"]);
                                dataRow["EDITUSERID"] = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["MENUTERMS"] = masterData.MENU_TERMS;

                                dataRow["BuyerName"] = Convert.ToString(reader["BUYER_NAME"]);
                                dataRow["BuyerAddress"] = Convert.ToString(reader["PARTY_ADDRESS"]);
                                dataRow["BuyerPhone"] = Convert.ToString(reader["CELL"]);
                                dataRow["BuyerNtn"] = Convert.ToString(reader["NTN"]);
                                dataRow["BuyerStrn"] = Convert.ToString(reader["STRN"]);
                                dataRow["Voucher"] = Convert.ToString(reader["VOUCHER_NO"]);
                                dataRow["Date"] = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                //dataRow["DelDate"] = reader["DEL_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["DEL_DATE"]).ToString("dd-MM-yyyy");
                                var delDateValue = reader["DEL_DATE"];

                                if (delDateValue == DBNull.Value)
                                {
                                    dataRow["DelDate"] = "";
                                }
                                else
                                {
                                    DateTime dt = Convert.ToDateTime(delDateValue);

                                    if (dt == DateTime.MinValue || dt == new DateTime(1900, 1, 1))
                                    {
                                        dataRow["DelDate"] = "";
                                    }
                                    else
                                    {
                                        dataRow["DelDate"] = dt.ToString("dd-MM-yyyy");
                                    }
                                }
                                dataRow["PurchaseOrder"] = Convert.ToString(reader["REMARKS"]);

                                dataRow["BranchAddress"] = Convert.ToString(reader["BRANCH_ADDRESS"]);
                                dataRow["BranchPhone"] = Convert.ToString(reader["BRANCH_PHONE"]);
                                dataRow["BranchEmail"] = Convert.ToString(reader["BRANCH_EMAIL"]);
                                dataRow["BranchNtn"] = Convert.ToString(reader["bRANCH_NTN"]);
                                dataRow["BranchStrn"] = Convert.ToString(reader["BRANCH_GST"]);
                                dataRow["REFERENCENO"] = Convert.ToString(reader["REF"]);
                                dataRow["TERM"] = Convert.ToString(reader["TERMS"]);
                                masterData.REFERENCENO = Convert.ToString(reader["REF"]);
                                masterData.TERM = Convert.ToString(reader["TERMS"]);
                                taxDataTable.Rows.Add(dataRow);
                            }
                            reader.Close();
                        }
                    }
                    else if (menuDetails.MD_ID == 22)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                DataRow dataRow = taxDataTable.NewRow();
                                masterData.EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
                                dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
                                dataRow["Qty"] = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAL_QTY"]); ;

                                dataRow["EDITUSERID"] = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["MENUTERMS"] = masterData.MENU_TERMS;

                                dataRow["BuyerName"] = Convert.ToString(reader["BUYER_NAME"]);
                                dataRow["BuyerAddress"] = Convert.ToString(reader["PARTY_ADDRESS"]);
                                dataRow["BuyerPhone"] = Convert.ToString(reader["CELL"]);
                                dataRow["BuyerNtn"] = Convert.ToString(reader["NTN"]);
                                dataRow["BuyerStrn"] = Convert.ToString(reader["STRN"]);
                                dataRow["Voucher"] = Convert.ToString(reader["VOUCHER_NO"]);
                                dataRow["Date"] = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                dataRow["PurchaseOrder"] = Convert.ToString(reader["PURCHASE_ORDER"]);

                                dataRow["BranchAddress"] = Convert.ToString(reader["BRANCH_ADDRESS"]);
                                dataRow["BranchPhone"] = Convert.ToString(reader["BRANCH_PHONE"]);
                                dataRow["BranchEmail"] = Convert.ToString(reader["BRANCH_EMAIL"]);
                                dataRow["BranchNtn"] = Convert.ToString(reader["bRANCH_NTN"]);
                                dataRow["BranchStrn"] = Convert.ToString(reader["BRANCH_GST"]);
                                dataRow["REFERENCENO"] = Convert.ToString(reader["REF"]);
                                dataRow["TERM"] = Convert.ToString(reader["TERMS"]);
                                masterData.REFERENCENO = Convert.ToString(reader["REF"]);
                                masterData.TERM = Convert.ToString(reader["TERMS"]);
                                taxDataTable.Rows.Add(dataRow);
                            }
                            reader.Close();
                        }
                    }
                    else if (menuDetails.MD_ID == 25)
                    {
                        using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            connection.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                DataRow dataRow = taxDataTable.NewRow();
                                masterData.EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["HsCode"] = Convert.ToString(reader["ITEM_CODE"]);
                                dataRow["Item"] = Convert.ToString(reader["ITEM_NAME"]);
                                dataRow["Unit"] = Convert.ToString(reader["UNIT"]);
                                dataRow["Qty"] = reader["BAL_QTY"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BAL_QTY"]); ;
                                dataRow["Rate"] = reader["RATE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RATE"]); ;
                                dataRow["Amt"] = Convert.ToInt32(reader["AMT"]);
                                dataRow["TaxAmt"] = Convert.ToInt32(reader["TAX_AMT"]);
                                dataRow["NetAmt"] = Convert.ToInt32(reader["NET_AMT"]);
                                dataRow["EDITUSERID"] = Convert.ToString(reader["EDIT_USER_ID"]);
                                dataRow["MENUTERMS"] = masterData.MENU_TERMS;

                                dataRow["BuyerName"] = Convert.ToString(reader["BUYER_NAME"]);
                                dataRow["BuyerAddress"] = Convert.ToString(reader["PARTY_ADDRESS"]);
                                dataRow["BuyerPhone"] = Convert.ToString(reader["CELL"]);
                                dataRow["BuyerNtn"] = Convert.ToString(reader["NTN"]);
                                dataRow["BuyerStrn"] = Convert.ToString(reader["STRN"]);
                                dataRow["Voucher"] = Convert.ToString(reader["VOUCHER_NO"]);
                                dataRow["Date"] = reader["V_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["V_DATE"]).ToString("dd-MM-yyyy");
                                dataRow["PurchaseOrder"] = Convert.ToString(reader["REMARKS"]);

                                dataRow["BranchAddress"] = Convert.ToString(reader["BRANCH_ADDRESS"]);
                                dataRow["BranchPhone"] = Convert.ToString(reader["BRANCH_PHONE"]);
                                dataRow["BranchEmail"] = Convert.ToString(reader["BRANCH_EMAIL"]);
                                dataRow["BranchNtn"] = Convert.ToString(reader["bRANCH_NTN"]);
                                dataRow["BranchStrn"] = Convert.ToString(reader["BRANCH_GST"]);
                                dataRow["REFERENCENO"] = Convert.ToString(reader["REF"]);
                                dataRow["TERM"] = Convert.ToString(reader["TERMS"]);
                                masterData.REFERENCENO = Convert.ToString(reader["REF"]);
                                masterData.TERM = Convert.ToString(reader["TERMS"]);
                                taxDataTable.Rows.Add(dataRow);
                            }
                            reader.Close();
                        }
                    }
                }


                reportData.Master = masterData;
                if (menuDetails.MD_ID == 22)
                {
                    reportData.Detail = taxDataTable;
                }
                else if (menuDetails.MD_ID == 54)
                {
                    reportData.Detail = inspectionServiceChargesDetails;
                }
                else if (menuDetails.REPORT_NAME == "SaleInvoiceDDJ")
                {
                    reportData.Detail = reportDetailsDDJ;
                }
                else if (menuDetails.MD_ID == 18)
                {
                    reportData.Detail = dataTable;
                }
                else
                {
                    reportData.Detail = (menuDetails.MD_ID == 21 || menuDetails.MD_ID == 25) ? taxDataTable : dataTable;
                }
                //=FormatNumber(FormatNumber(Fields!Rate.Value * (1 - Fields!Unit.Value / 100), 2) * Fields!Qty.Value, 0)
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

        public DataTable GetComm(int Salesman)
        {
            DataTable dt = new DataTable();
            string getComm = $"Select * from TBL_SCOMM_LIST where SALESMAN = {Salesman} ANd DLT = 'T'";

            using (SqlConnection connectionNew = new SqlConnection(new SQLService().getconnstring()))
            {
                using (SqlCommand commandNew1 = new SqlCommand(getComm, connectionNew))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(commandNew1))
                    {
                        connectionNew.Open();
                        da.Fill(dt);
                    }
                }
            }
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    Console.WriteLine($"{col.ColumnName}: {row[col]}");
                }
            }
            return dt;
        }
    }
}