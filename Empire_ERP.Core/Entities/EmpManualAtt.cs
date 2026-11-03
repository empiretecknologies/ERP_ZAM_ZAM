namespace Empire_ERP.Core.Entities
{
    public class EmpManualAtt
    {
        public int? TRAN_ID { get; set; }
        public int? MACHINE_ID { get; set; }
        public int? EMPLOYEE_ID { get; set; }
        public string? EMPLOYEE_NAME { get; set; }
        public DateTime? punch_time { get; set; }
        public string? SHIFT_STATUS { get; set; }
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
