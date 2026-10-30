namespace Empire_ERP.Core.Entities
{
    public class SalaryGenerate
    {
        public int? ID { get; set; }
        public int? EMPLOYEE_ID { get; set; }
        public DateTime? FROM_DATE { get; set; }
        public DateTime? TO_DATE { get; set; }
        public string? EMP_PIC { get; set; }
        public string? MACHINE_ID { get; set; }
        public string? EMPLOYEE_CODE { get; set; }
        public string? EMP_NAME { get; set; }
        public string? DESIGNATION { get; set; }
        public string? DEPARTMENT { get; set; }
        public string? PAYROLL_MONTH { get; set; }
        public double? PRESENT_DAYS { get; set; }
        public double? SUNDAY_DAYS { get; set; }
        public double? WORKING_DAYS { get; set; }
        public double? ABSENT_DAYS { get; set; }
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
        public double? TOTAL_ADVANCE { get; set; }
        public double? LOAN_INSTALLMENT { get; set; }
        public string? TOTAL_LATE_HOURS { get; set; }
        public string? AVG_IN_TIME { get; set; }
        public double? NET_SALARY { get; set; }
        public bool? IS_GENERATED { get; set; }
    }

    public class SalaryGenerateRequest
    {
        public DateTime? FROM_DATE { get; set; }
        public DateTime? TO_DATE { get; set; }
    }
}
