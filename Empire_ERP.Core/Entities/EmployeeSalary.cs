namespace Empire_ERP.Core.Entities
{
    public class EmployeeSalary
    {
        public int? ID { get; set; }
        public int? EMPLOYEE_ID { get; set; }
        public double? BASIC_SALARY { get; set; }
        public double? HOUSE_ALLOWANCE { get; set; }
        public double? MEDICAL_ALLOWANCE { get; set; }
        public double? CONVEYANCE_ALLOWANCE { get; set; }
        public double? OVERTIME { get; set; }
        public double? BONUS { get; set; }
        public double? INCOME_TAX { get; set; }
        public double? LOAN_DEDUCTION { get; set; }
        public double? ADVANCE_DEDUCTION { get; set; }
        public double? GROSS_SALARY { get; set; }
        public double? TOTAL_DEDUCTION { get; set; }
        public double? NET_SALARY { get; set; }
        public DateTime? EFFECTIVE_FROM { get; set; }
        public DateTime? EFFECTIVE_TO { get; set; }
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

    public class CustomEmployeeSalary
    {
        public int? EMPLOYEE_ID { get; set; }
        public List<EmployeeSalary>? Detail { get; set; }
    }
}
