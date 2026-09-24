using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class CustomerPricingpRepository : ICustomerPricingRepository
    {

        public IBranchRepository _branchRepository { get; set; }
        public ICommonRepository _commonRepository { get; set; }
        public IMenuRepository _menuRepository { get; set; }
        public CustomerPricingpRepository(IBranchRepository branchRepository, ICommonRepository commonRepository, IMenuRepository menuRepository)
        {
            _branchRepository = branchRepository;
            _commonRepository = commonRepository;
            _menuRepository = menuRepository;
        }


        public MyHttpResponseMessage QuickSearch(MyHttpResponseMessage Menu, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = string.Empty;
                int? pType;
                var menu = (Menu)Menu.data;
                table = menu.TABLE1;
                pType = menu.PTYPE;
                List<object> jsonDataResult = new List<object>();

                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {

                    string query = $@"
                                SELECT Distinct 
                                    S.GROUP_CODE,
                                    P.PARTY_NAME,
                                    p.PARTY_CODE
                                FROM 
                                    {table} AS S
                                INNER JOIN 
                                    TBL_PARTY_TYPES AS P 
                                    ON S.PARTY_CODE = P.PARTY_CODE
                                WHERE 
                                    P.PARTY_TYPE_CODE = 4
                                    AND S.DLT = 'T' 
                                    AND S.ASTATUS = 'Y'
                                ORDER BY 
                                    S.GROUP_CODE DESC";



                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            GROUP_CODE = Convert.ToString(reader["GROUP_CODE"]),
                            PARTY_NAME = Convert.ToString(reader["PARTY_NAME"]),
                            PARTY_CODE = Convert.ToInt32(reader["PARTY_CODE"]),
                        };
                        jsonDataResult.Add(row);
                    }

                    reader.Close();
                }

                response.data = jsonDataResult;
                response.msg = "";
                response.msgType = 1;
                return response;
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
                string maxIdQuery = $"SELECT ISNULL(MAX(GROUP_CODE), 0) + 1 FROM {table}";
                command.CommandText = maxIdQuery;
                object result = command.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {

            }
            return 0;
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
            catch (Exception ex)
            {

            }
            return 0;
        }

        public MyHttpResponseMessage Save(List<CommList> modelRecord, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                if (modelRecord == null || modelRecord.Count == 0)
                {
                    response.msg = "No detail records to save.";
                    response.msgType = 2;
                    return response;
                }

                string? table = menu.TABLE1;
                var ip = EscapeSql(common.IPAddress);
                var computerName = EscapeSql(common.ComputerName);
                var postalCode = EscapeSql(common.PostalCode);
                var userid = EscapeSql(common.Username);
                var menuID = common.MenuID;
                string connectionString = new SQLService().getconnstring();
                string now = CommonService.GetDateTime("Pakistan Standard Time");

                int? partyCode = modelRecord[0].PARTY;
                int? actCode = modelRecord[0].ACT_CODE;
                int groupCode = modelRecord[0].GROUP_CODE ?? 0;
                string astatus = EscapeSql(modelRecord[0].ASTATUS ?? "Y");
                bool isNew = groupCode <= 0;

                if (partyCode == null || partyCode <= 0 || actCode == null || actCode <= 0)
                {
                    response.msg = "Party is required.";
                    response.msgType = 2;
                    return response;
                }

                // Normalize rows + in-memory validation (no DB round-trips)
                var normalized = new List<(int DtCode, int ItemCode, string Rate)>();
                var seenItems = new HashSet<int>();
                var duplicateInForm = new List<int>();

                foreach (var item in modelRecord)
                {
                    if (item.PARTY == null || item.PARTY <= 0 || item.ACT_CODE == null || item.ACT_CODE <= 0)
                    {
                        response.msg = "Party is required for all records.";
                        response.msgType = 2;
                        return response;
                    }

                    if (item.PARTY != partyCode || item.ACT_CODE != actCode)
                    {
                        response.msg = "All detail rows must belong to the same Party.";
                        response.msgType = 2;
                        return response;
                    }

                    if (item.ITEM_CODE == null || item.ITEM_CODE <= 0)
                    {
                        response.msg = "Item is required for all records.";
                        response.msgType = 2;
                        return response;
                    }

                    if (string.IsNullOrWhiteSpace(item.RATE) || !decimal.TryParse(item.RATE, out decimal rateVal) || rateVal <= 0)
                    {
                        response.msg = "RATE must be greater than 0.";
                        response.msgType = 2;
                        return response;
                    }

                    int itemCode = item.ITEM_CODE.Value;
                    if (!seenItems.Add(itemCode))
                    {
                        duplicateInForm.Add(itemCode);
                    }

                    normalized.Add((item.DT_CODE ?? 0, itemCode, item.RATE));
                }

                if (duplicateInForm.Count > 0)
                {
                    response.msg = BuildDuplicateItemMessage(duplicateInForm.Distinct().ToList(), "in the form");
                    response.msgType = 2;
                    return response;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandTimeout = 120;

                    try
                    {
                        // 1) Bulk master duplicate check (PARTY_CODE + ACT_CODE) — single query
                        // Edit mode excludes the current GROUP_CODE so the record is not treated as its own duplicate.
                        string masterDupQuery = $@"
                            SELECT TOP 1 GROUP_CODE
                            FROM {table}
                            WHERE PARTY_CODE = {partyCode}
                              AND ACT_CODE = {actCode}
                              AND DLT = 'T'
                              AND ({(isNew ? "1=1" : $"GROUP_CODE <> {groupCode}")})";

                        command.CommandText = masterDupQuery;
                        object masterDup = command.ExecuteScalar();
                        if (masterDup != null && masterDup != DBNull.Value)
                        {
                            transaction.Rollback();
                            response.msg = $"Party already exists (Code: {Convert.ToInt32(masterDup)}). Please select another party.";
                            response.msgType = 2;
                            return response;
                        }

                        // 2) Bulk item duplicate check — single set-based query for all items
                        var conflictItems = GetExistingDuplicateItems(command, table, partyCode.Value, actCode.Value, groupCode, isNew, normalized);
                        if (conflictItems.Count > 0)
                        {
                            transaction.Rollback();
                            response.msg = BuildDuplicateItemMessage(conflictItems, "for this Party");
                            response.msgType = 2;
                            return response;
                        }

                        // 3) Allocate IDs once
                        if (isNew)
                        {
                            groupCode = GenerateNextId(common, command, menu);
                            if (groupCode <= 0)
                            {
                                transaction.Rollback();
                                response.msg = "Something went wrong! please try again later.";
                                response.msgType = 2;
                                return response;
                            }
                        }

                        var insertRows = normalized.Where(x => x.DtCode <= 0).ToList();
                        var updateRows = normalized.Where(x => x.DtCode > 0).ToList();

                        int nextDtCode = 0;
                        if (insertRows.Count > 0)
                        {
                            nextDtCode = GenerateNextDetailId(command, menu);
                            if (nextDtCode <= 0)
                            {
                                transaction.Rollback();
                                response.msg = "Something went wrong! please try again later.";
                                response.msgType = 2;
                                return response;
                            }
                        }

                        // 4) Bulk INSERT new detail rows (batched multi-value)
                        if (insertRows.Count > 0)
                        {
                            const int batchSize = 500;
                            for (int offset = 0; offset < insertRows.Count; offset += batchSize)
                            {
                                var batch = insertRows.Skip(offset).Take(batchSize).ToList();
                                var sb = new StringBuilder();
                                sb.Append($"INSERT INTO {table} ");
                                sb.Append("(GROUP_CODE, PARTY_CODE, ACT_CODE, ITEM_CODE, RATE, ");
                                sb.Append("ADD_USER_ID, ADD_DATE, ADD_COMPUTER_NAME, ADD_IP_ADDRESS, EDIT_USER_ID, EDIT_DATE, ");
                                sb.Append("EDIT_COMPUTER_NAME, EDIT_IP_ADDRESS, ADD_POSTALCODE, EDIT_POSTALCODE, ASTATUS, MENU_ID, DLT, DT_CODE) VALUES ");

                                var values = new List<string>(batch.Count);
                                foreach (var row in batch)
                                {
                                    values.Add($"({groupCode}, {partyCode}, {actCode}, {row.ItemCode}, {EscapeSql(row.Rate)}, " +
                                               $"'{userid}', '{now}', '{computerName}', '{ip}', " +
                                               $"'{userid}', '{now}', '{computerName}', '{ip}', " +
                                               $"'{postalCode}', '{postalCode}', '{astatus}', {menuID}, 'T', {nextDtCode})");
                                    nextDtCode++;
                                }

                                sb.Append(string.Join(",", values));
                                command.CommandText = sb.ToString();
                                command.ExecuteNonQuery();
                            }
                        }

                        // 5) Bulk UPDATE existing detail rows via temp table (set-based, one pass)
                        if (updateRows.Count > 0)
                        {
                            command.CommandText = @"
                                IF OBJECT_ID('tempdb..#CP_UpdateRows') IS NOT NULL DROP TABLE #CP_UpdateRows;
                                CREATE TABLE #CP_UpdateRows (
                                    DT_CODE INT NOT NULL,
                                    ITEM_CODE INT NOT NULL,
                                    RATE FLOAT NOT NULL
                                );";
                            command.ExecuteNonQuery();

                            const int batchSize = 500;
                            for (int offset = 0; offset < updateRows.Count; offset += batchSize)
                            {
                                var batch = updateRows.Skip(offset).Take(batchSize).ToList();
                                var sb = new StringBuilder();
                                sb.Append("INSERT INTO #CP_UpdateRows (DT_CODE, ITEM_CODE, RATE) VALUES ");
                                sb.Append(string.Join(",", batch.Select(r =>
                                    $"({r.DtCode}, {r.ItemCode}, {EscapeSql(r.Rate)})")));
                                command.CommandText = sb.ToString();
                                command.ExecuteNonQuery();
                            }

                            command.CommandText = $@"
                                UPDATE T SET
                                    PARTY_CODE = {partyCode},
                                    ACT_CODE = {actCode},
                                    ITEM_CODE = U.ITEM_CODE,
                                    RATE = U.RATE,
                                    EDIT_USER_ID = '{userid}',
                                    EDIT_DATE = '{now}',
                                    EDIT_COMPUTER_NAME = '{computerName}',
                                    EDIT_IP_ADDRESS = '{ip}',
                                    EDIT_POSTALCODE = '{postalCode}',
                                    ASTATUS = '{astatus}',
                                    MENU_ID = {menuID},
                                    DLT = 'T'
                                FROM {table} T
                                INNER JOIN #CP_UpdateRows U ON T.DT_CODE = U.DT_CODE
                                WHERE T.GROUP_CODE = {groupCode};

                                DROP TABLE #CP_UpdateRows;";
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        response.data = new { code = isNew ? groupCode : 0 };
                        response.msgType = 1;
                        response.msg = isNew ? "Record Added Successfully" : "Record Updated Successfully";
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
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

        /// <summary>
        /// Single set-based query: finds ITEM_CODEs already assigned to this PARTY+ACT
        /// among live rows, excluding the current detail rows being updated (edit mode).
        /// </summary>
        private List<int> GetExistingDuplicateItems(
            SqlCommand command,
            string table,
            int partyCode,
            int actCode,
            int groupCode,
            bool isNew,
            List<(int DtCode, int ItemCode, string Rate)> rows)
        {
            var conflicts = new List<int>();
            if (rows.Count == 0) return conflicts;

            // Load candidate keys into a temp table, then one JOIN against live data
            command.CommandText = @"
                IF OBJECT_ID('tempdb..#CP_CheckItems') IS NOT NULL DROP TABLE #CP_CheckItems;
                CREATE TABLE #CP_CheckItems (
                    DT_CODE INT NOT NULL,
                    ITEM_CODE INT NOT NULL
                );";
            command.ExecuteNonQuery();

            const int batchSize = 500;
            for (int offset = 0; offset < rows.Count; offset += batchSize)
            {
                var batch = rows.Skip(offset).Take(batchSize).ToList();
                var sb = new StringBuilder();
                sb.Append("INSERT INTO #CP_CheckItems (DT_CODE, ITEM_CODE) VALUES ");
                sb.Append(string.Join(",", batch.Select(r => $"({r.DtCode}, {r.ItemCode})")));
                command.CommandText = sb.ToString();
                command.ExecuteNonQuery();
            }

            // Exclude the same GROUP_CODE + DT_CODE so edit of an existing line is not a self-duplicate.
            string excludeSelf = isNew
                ? "1=1"
                : $@"NOT (T.GROUP_CODE = {groupCode} AND T.DT_CODE = C.DT_CODE AND C.DT_CODE > 0)";

            command.CommandText = $@"
                SELECT DISTINCT T.ITEM_CODE
                FROM {table} T
                INNER JOIN #CP_CheckItems C ON T.ITEM_CODE = C.ITEM_CODE
                WHERE T.PARTY_CODE = {partyCode}
                  AND T.ACT_CODE = {actCode}
                  AND T.DLT = 'T'
                  AND ({excludeSelf})";

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    conflicts.Add(Convert.ToInt32(reader["ITEM_CODE"]));
                }
            }

            command.CommandText = "IF OBJECT_ID('tempdb..#CP_CheckItems') IS NOT NULL DROP TABLE #CP_CheckItems;";
            command.ExecuteNonQuery();

            return conflicts;
        }

        private static string BuildDuplicateItemMessage(List<int> itemCodes, string context)
        {
            const int maxShow = 15;
            var shown = itemCodes.Take(maxShow).Select(x => x.ToString()).ToList();
            string list = string.Join(", ", shown);
            string more = itemCodes.Count > maxShow ? $" (+{itemCodes.Count - maxShow} more)" : "";
            return $"Duplicate item(s) {context}: {list}{more}. Total: {itemCodes.Count}.";
        }

        private static string EscapeSql(string? value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }


        public MyHttpResponseMessage GetCommisionMapByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT TOP 1 GROUP_CODE, PARTY_CODE, ACT_CODE, ITEM_CODE ,RATE, " +
                        "ASTATUS " +
                        "FROM " + table + " " +
                        "WHERE DLT = 'T' AND GROUP_CODE = '" + code + "' " +
                        "ORDER BY GROUP_CODE";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            GROUP_CODE = Convert.ToString(reader["GROUP_CODE"]),
                            PARTY_KEY = $@"{Convert.ToString(reader["PARTY_CODE"])}{Convert.ToString(reader["ACT_CODE"])}",
                            PARTY = Convert.ToInt32(reader["PARTY_CODE"]),
                            ACT_CODE = Convert.ToInt32(reader["ACT_CODE"]),
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            RATE = Convert.ToString(reader["RATE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"]),
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

        public MyHttpResponseMessage GetCommisionMapDetailsByCode(int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = @$"SELECT C.GROUP_CODE, C.ITEM_CODE, C.RATE,C.DT_CODE
                                       FROM {table} C
                                       WHERE C.DLT = 'T' AND C.GROUP_CODE = '{code}'
                                       ORDER BY C.DT_CODE";


                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new
                        {
                            GROUP_CODE = Convert.ToString(reader["GROUP_CODE"]),
                            DT_CODE = reader["DT_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DT_CODE"]),
                            RATE = Convert.ToString(reader["RATE"]),
                            ITEM_CODE = reader["ITEM_CODE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ITEM_CODE"]),
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
                    string query = $"UPDATE {table} SET DLT = 'F' WHERE GROUP_CODE = '{code}'";
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

        public MyHttpResponseMessage CopyRecord(CopyRecordSalesman record, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msgType = 2;
            response.msg = "Data not found in our records";
            try
            {
                string? table = menu.TABLE1;
                string connectionString = new SQLService().getconnstring();
                List<CommList> commList = new List<CommList>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT * FROM {table} WHERE GROUP_CODE = {record.GROUP_CODE}  AND DLT = 'T'";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new CommList
                        {
                            PARTY = record.PARTY,
                            ACT_CODE = record.ACT_CODE,
                            ITEM_CODE = Convert.ToInt32(reader["ITEM_CODE"]),
                            RATE = Convert.ToString(reader["RATE"]),
                            ASTATUS = Convert.ToString(reader["ASTATUS"])
                        };
                        commList.Add(row);
                    }
                    reader.Close();
                    connection.Close();
                }
                response = this.Save(commList, common, menu);
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


        public MyHttpResponseMessage DeleteCommisionMapDetailByCode(int gcode, int code, Common common, Menu menu)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                string? table = menu.TABLE1;
                string connectionString = new SQLService().getconnstring();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = $"UPDATE {table} SET DLT = 'F' " +
                                   $"WHERE GROUP_CODE = '{gcode}' AND DT_CODE = '{code}'";
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



    }
}
