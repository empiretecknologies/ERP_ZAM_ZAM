namespace Empire_ERP.Core.Entities
{
    public class EmployeeLoan
    {
        public int? ID { get; set; }
        public int? EMPLOYEE_ID { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public DateTime? LOAN_DATE { get; set; }
        public double? LOAN_AMOUNT { get; set; }
        public double? INSTALLMENT_AMOUNT { get; set; }
        public int? TOTAL_INSTALLMENTS { get; set; }
        public DateTime? START_DEDUCTION_DATE { get; set; }
        public string? REMARKS { get; set; }
        public int? BOOK_TYPE { get; set; }
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
    }
}
