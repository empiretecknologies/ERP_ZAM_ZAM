namespace Empire_ERP.Core.Entities
{
    public class SalaryHeads
    {
        public int? ID { get; set; }
        public int? BASIC_SALARY_COA_ID { get; set; }
        public int? HOUSE_ALLOWANCE_COA_ID { get; set; }
        public int? MEDICAL_ALLOWANCE_COA_ID { get; set; }
        public int? CONVEYANCE_ALLOWANCE_COA_ID { get; set; }
        public int? OVERTIME_COA_ID { get; set; }
        public int? BONUS_COA_ID { get; set; }
        public int? INCOME_TAX_COA_ID { get; set; }
        public int? LOAN_DEDUCTION_COA_ID { get; set; }
        public int? ADVANCE_DEDUCTION_COA_ID { get; set; }
        public string? BASIC_SALARY_NAME { get; set; }
        public string? HOUSE_ALLOWANCE_NAME { get; set; }
        public string? MEDICAL_ALLOWANCE_NAME { get; set; }
        public string? CONVEYANCE_ALLOWANCE_NAME { get; set; }
        public string? OVERTIME_NAME { get; set; }
        public string? BONUS_NAME { get; set; }
        public string? INCOME_TAX_NAME { get; set; }
        public string? LOAN_DEDUCTION_NAME { get; set; }
        public string? ADVANCE_DEDUCTION_NAME { get; set; }
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
