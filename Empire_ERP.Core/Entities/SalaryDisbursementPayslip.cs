namespace Empire_ERP.Core.Entities
{
    public class SalaryDisbursementPayslip
    {
        public int? TRAN_ID { get; set; }
        public int? DT_CODE { get; set; }
        public string? VOUCHER_NO { get; set; }
        public DateTime? V_DATE { get; set; }
        public DateTime? PAYROLL_MONTH { get; set; }
        public string? PAYROLL_MONTH_TEXT { get; set; }
        public string? PAYROLL_PERIOD { get; set; }
        public string? PAYMENT_MODE { get; set; }
        public int? EMP_CODE { get; set; }
        public string? EMPLOYEE_CODE { get; set; }
        public string? EMP_NAME { get; set; }
        public string? EMP_PIC { get; set; }
        public string? DEPARTMENT { get; set; }
        public string? DESIGNATION { get; set; }
        public double? WORKING_DAYS { get; set; }
        public double? PRESENT_DAYS { get; set; }
        public double? SUNDAY_DAYS { get; set; }
        public double? ABSENT_DAYS { get; set; }
        public string? TOTAL_LATE_HOURS { get; set; }
        public string? AVG_IN_TIME { get; set; }
        public double? BASIC_SALARY { get; set; }
        public double? HOUSE_ALLOWANCE { get; set; }
        public double? MEDICAL_ALLOWANCE { get; set; }
        public double? CONVEYANCE_ALLOWANCE { get; set; }
        public double? OVERTIME { get; set; }
        public double? BONUS { get; set; }
        public double? GROSS_SALARY { get; set; }
        public double? PER_DAY_SALARY { get; set; }
        public double? ABSENT_DEDUCTION { get; set; }
        public double? INCOME_TAX { get; set; }
        public double? ADVANCE { get; set; }
        public double? LOAN { get; set; }
        public double? TOTAL_DEDUCTIONS { get; set; }
        public double? NET_SALARY { get; set; }
        public string? NET_SALARY_WORDS { get; set; }
        public string? COMPANY_NAME { get; set; }
        public string? COMPANY_ADDRESS { get; set; }
        public string? COMPANY_PHONE { get; set; }
        public string? COMPANY_LOGO { get; set; }
        public string? COMPANY_TAGLINE { get; set; }
    }
}
