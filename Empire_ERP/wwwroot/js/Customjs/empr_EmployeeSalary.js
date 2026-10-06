var empr_EmployeeSalary = {
    employees: [],
    initEvents: function () {

        $(document).ready(function () {

            empr_EmployeeSalary.CreateSalaryGrid([]);

            $('#BtnSave').click(function () {
                if (Permissions != "Admin" && !Permissions.r_ADD) {
                    empr_helper.notify("You are not allowed to add new record !", 2);
                } else {
                    if (empr_EmployeeSalary.validateForm()) {
                        empr_EmployeeSalary.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#quicksearch', function () {
                empr_EmployeeSalary.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmployeeSalary.SelectEmployee(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                empr_EmployeeSalary.resetForm();
            });

            $('.salary-amount').on('input change', function () {
                empr_EmployeeSalary.CalculateTotals();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#quicksearch').hide();
                !Permissions.r_ADD && $('#BtnSave').hide();
            }
        });
    },
    AmountValue: function (selector) {
        return parseFloat($(selector).val()) || 0;
    },
    CalculateTotals: function () {
        var gross = empr_EmployeeSalary.AmountValue('#BASIC_SALARY') + empr_EmployeeSalary.AmountValue('#HOUSE_ALLOWANCE') + empr_EmployeeSalary.AmountValue('#MEDICAL_ALLOWANCE')
            + empr_EmployeeSalary.AmountValue('#CONVEYANCE_ALLOWANCE') + empr_EmployeeSalary.AmountValue('#OVERTIME') + empr_EmployeeSalary.AmountValue('#BONUS');
        var deduction = empr_EmployeeSalary.AmountValue('#INCOME_TAX') + empr_EmployeeSalary.AmountValue('#LOAN_DEDUCTION') + empr_EmployeeSalary.AmountValue('#ADVANCE_DEDUCTION');
        $('#GROSS_SALARY').val(gross);
        $('#TOTAL_DEDUCTION').val(deduction);
        $('#NET_SALARY').val(gross - deduction);
    },
    resetForm: function () {
        $('.salary-amount').val('');
        $("#EFFECTIVE_FROM").val(empr_helper.GetCurrentDate());
        $("#EFFECTIVE_TO").val('');
        empr_EmployeeSalary.CalculateTotals();

        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
        } else {
            $('#BtnSave').show();
        }
    },
    SelectEmployee: function (id) {
        var employee = empr_EmployeeSalary.employees.filter(e => e.id == id);
        if (employee.length > 0) {
            $("#EMPLOYEE_ID").val(employee[0].id);
            $("#EMPLOYEE_NAME").val(((employee[0].firsT_NAME || '') + ' ' + (employee[0].lasT_NAME || '')).trim());
        }
        $('.modal').modal('hide');
        empr_EmployeeSalary.resetForm();
        empr_EmployeeSalary.GetEmployeeSalaryByEmployeeId(id);

        if (Permissions != "Admin") {
            if (Permissions.r_ADD) {
                $('#BtnNew').show();
            }
        } else {
            $('#BtnNew').show();
        }
    },
    GetEmployeeSalaryByEmployeeId: function (employeeId) {
        ajaxHelper.ajaxGetJson('/EmployeeSalary/GetEmployeeSalaryByEmployeeId?employeeId=' + employeeId, function (data) {
            if (data.msgType == 1) {
                empr_EmployeeSalary.CreateSalaryGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    validateForm: function () {

        var valid = true;
        var data = empr_EmployeeSalary.GetDataToSave();
        var record = data.Detail[0];

        if (data.EMPLOYEE_ID == '' || data.EMPLOYEE_ID == null) {
            valid = false;
            empr_helper.notify("Please select employee.", 2);
        }

        if ($("#BASIC_SALARY").val().trim() == '') {
            valid = false;
            empr_helper.notify("Please enter basic salary.", 2);
        }

        if (record.EFFECTIVE_FROM == '' || record.EFFECTIVE_FROM == null) {
            valid = false;
            empr_helper.notify("Please select effective from date.", 2);
        }

        return valid;
    },
    GetDataToSave: function () {

        empr_EmployeeSalary.CalculateTotals();

        var EFFECTIVE_FROM = $("#EFFECTIVE_FROM").val();
        var EFFECTIVE_TO = $("#EFFECTIVE_TO").val();
        var record = {
            ID: 0,
            BASIC_SALARY: empr_EmployeeSalary.AmountValue('#BASIC_SALARY'),
            HOUSE_ALLOWANCE: empr_EmployeeSalary.AmountValue('#HOUSE_ALLOWANCE'),
            MEDICAL_ALLOWANCE: empr_EmployeeSalary.AmountValue('#MEDICAL_ALLOWANCE'),
            CONVEYANCE_ALLOWANCE: empr_EmployeeSalary.AmountValue('#CONVEYANCE_ALLOWANCE'),
            OVERTIME: empr_EmployeeSalary.AmountValue('#OVERTIME'),
            BONUS: empr_EmployeeSalary.AmountValue('#BONUS'),
            INCOME_TAX: empr_EmployeeSalary.AmountValue('#INCOME_TAX'),
            LOAN_DEDUCTION: empr_EmployeeSalary.AmountValue('#LOAN_DEDUCTION'),
            ADVANCE_DEDUCTION: empr_EmployeeSalary.AmountValue('#ADVANCE_DEDUCTION'),
            EFFECTIVE_FROM: EFFECTIVE_FROM == '' ? null : EFFECTIVE_FROM,
            EFFECTIVE_TO: EFFECTIVE_TO == '' ? null : EFFECTIVE_TO
        }
        var modelRecord = {
            EMPLOYEE_ID: $("#EMPLOYEE_ID").val(),
            Detail: [record]
        }
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_EmployeeSalary.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/EmployeeSalary/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_EmployeeSalary.resetForm();
                empr_EmployeeSalary.GetEmployeeSalaryByEmployeeId($("#EMPLOYEE_ID").val());
            }
        }, false, true);
    },
    InitQuickSearch: function () {
        empr_EmployeeSalary.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/EmployeeSalary/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_EmployeeSalary.employees = data.data;
                empr_EmployeeSalary.CreateQuickSearchGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    CreateSalaryGrid: function (dataSrc) {
        var col = [
            { dataField: 'basiC_SALARY', caption: 'Basic Salary' },
            { dataField: 'housE_ALLOWANCE', caption: 'House Allowance' },
            { dataField: 'medicaL_ALLOWANCE', caption: 'Medical Allowance' },
            { dataField: 'conveyancE_ALLOWANCE', caption: 'Conveyance Allowance' },
            { dataField: 'overtime', caption: 'Overtime' },
            { dataField: 'bonus', caption: 'Bonus' },
            { dataField: 'incomE_TAX', caption: 'Income Tax' },
            { dataField: 'loaN_DEDUCTION', caption: 'Loan Deduction', visible: false },
            { dataField: 'advancE_DEDUCTION', caption: 'Advance Deduction', visible: false },
            { dataField: 'grosS_SALARY', caption: 'Gross Salary' },
            { dataField: 'totaL_DEDUCTION', caption: 'Total Deduction' },
            { dataField: 'neT_SALARY', caption: 'Net Salary' },
            { dataField: 'effectivE_FROM', caption: 'Effective From', dataType: 'date', format: 'dd-MM-yyyy' },
            { dataField: 'effectivE_TO', caption: 'Effective To', dataType: 'date', format: 'dd-MM-yyyy' },
        ];
        empr_helper.dxGridbindingVouchers('#SalaryGridContainer', col, dataSrc, "EmployeeSalaryDetail");
    },
    CreateQuickSearchGrid: function (dataSrc) {
        var col = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                $(`<div class="btn-group btn-group-sm">
                   <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.id} title="Select"><i class="fa fa-edit"></i></a>
                   </div>`).appendTo(container);
            }
        },
            { dataField: 'employeE_CODE', caption: 'Employee Code' },
            { dataField: 'firsT_NAME', caption: 'First Name' },
            { dataField: 'lasT_NAME', caption: 'Last Name' },
            { dataField: 'phone', caption: 'Phone' },
            { dataField: 'departmenT_NAME', caption: 'Department' },
            { dataField: 'designatioN_NAME', caption: 'Designation' },
            { dataField: 'joininG_DATE', caption: 'Joining Date', dataType: 'date', format: 'dd-MM-yyyy' },
            { dataField: 'astatus', caption: 'Status' },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "EmployeeSalaryQS");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
}
