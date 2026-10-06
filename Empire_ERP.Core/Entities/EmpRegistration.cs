namespace Empire_ERP.Core.Entities
{
    public class EmpRegistration
    {
        public int? ID { get; set; }
        public string? EMPLOYEE_CODE { get; set; }
        public string? EMP_PIC { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? EMAIL { get; set; }
        public string? PHONE { get; set; }
        public int? DEPARTMENT_ID { get; set; }
        public int? DESIGNATION_ID { get; set; }
        public string? DEPARTMENT_NAME { get; set; }
        public string? DESIGNATION_NAME { get; set; }
        public int? MACHINE_ID { get; set; }
        public DateTime? JOINING_DATE { get; set; }
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
