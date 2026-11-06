namespace Empire_ERP.Core.Entities
{
    public class SalaryDisbursement
    {
        public int? TRAN_ID { get; set; }
        public DateTime? V_DATE { get; set; }
        public string? VOUCHER_NO { get; set; }
        public int? BOOK_TYPE { get; set; }
        public string? REMARKS { get; set; }
        public int? BCODE { get; set; }
        public int? PERIOD_ID { get; set; }
        public string? ADD_USER_ID { get; set; }
        public DateTime? ADD_DATE { get; set; }
        public string? ADD_COMPUTER_NAME { get; set; }
        public string? ADD_IP_ADDRESS { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public DateTime? EDIT_DATE { get; set; }
        public string? EDIT_COMPUTER_NAME { get; set; }
        public string? EDIT_IP_ADDRESS { get; set; }
        public string? ADD_POSTALCODE { get; set; }
        public string? EDIT_POSTALCODE { get; set; }
        public string? ASTATUS { get; set; }
        public int? MENU_ID { get; set; }
        public string? DLT { get; set; }
        public int? DT_CODE { get; set; }
        public int? EMP_CODE { get; set; }
        public string? EMP_NAME { get; set; }
        public double? GROSS_SALARY { get; set; }
        public double? ADVANCE { get; set; }
        public double? LOAN { get; set; }
        public double? ABSENT { get; set; }
        public double? NET_SALARY { get; set; }
        public string? CHQ_NO { get; set; }
        public DateTime? CHQ_DATE { get; set; }
        public string? DT_DESC { get; set; }
        public DateTime? PAYROLL_MONTH { get; set; }
        public string? PAYROLL_MONTH_TEXT { get; set; }
    }
}
