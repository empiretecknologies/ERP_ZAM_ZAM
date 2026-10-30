var empr_EmployeeSalary = {
    employees: [],
    initEvents: function () {

        $(document).ready(function () {

            empr_EmployeeSalary.CreateSalaryGrid([]);

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    var salaryId = $("#SALARY_ID").val();
                    if ((!salaryId || salaryId == 0) && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if ((salaryId > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_EmployeeSalary.validateForm()) {
                            empr_EmployeeSalary.saveAttempt();
                        }
                    }
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

            $('body').on('click', '.elm_salary_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmployeeSalary.LoadSalaryForEdit(reportid);
            });

            $('body').on('click', '.elm_salary_delete', function () {
                var reportid = $(this).attr("reportid");
                $("#SALARY_ID").val(reportid);
                empr_EmployeeSalary.DeleteRecord();
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                empr_EmployeeSalary.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_EmployeeSalary.DeleteRecord();
            });

            $('.salary-amount').on('input change', function () {
                empr_EmployeeSalary.CalculateTotals();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#quicksearch').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },
    AmountValue: function (selector) {
        return parseFloat($(selector).val()) || 0;
    },
    FormatDateInput: function (value) {
        if (!value) {
            return '';
        }
        if (typeof value === 'string') {
            return value.substring(0, 10);
        }
        try {
            var date = new Date(value);
            if (!isNaN(date.getTime())) {
                return date.toISOString().substring(0, 10);
            }
        } catch (e) {
        }
        return '';
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
        $("#SALARY_ID").val('');
        $('.salary-amount').val('');
        $("#EFFECTIVE_FROM").val(empr_helper.GetCurrentDate());
        $("#EFFECTIVE_TO").val('');
        $('#BtnDelete').hide();
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
    DeleteRecord: function () {
        var salaryId = $("#SALARY_ID").val();
        if (!salaryId || salaryId == 0) {
            empr_helper.notify("Please select any record for delete..", 2);
            return;
        }

        if (Permissions != "Admin" && !Permissions.r_DLT) {
            empr_helper.notify("You are not allowed to delete records !", 2);
            return;
        }

        swal({
            title: 'Are you sure you want to remove this record?',
            text: "You won't be able to revert this!",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#0CC27E',
            cancelButtonColor: '#FF586B',
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel!',
            confirmButtonClass: 'btn btn-success mr-5',
            cancelButtonClass: 'btn btn-danger',
            buttonsStyling: false
        }).then(function () {
            ajaxHelper.ajaxPostJsonData({ id: salaryId }, "/EmployeeSalary/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_EmployeeSalary.resetForm();
                    $('#BtnDelete').hide();
                    var employeeId = $("#EMPLOYEE_ID").val();
                    if (employeeId) {
                        empr_EmployeeSalary.GetEmployeeSalaryByEmployeeId(employeeId);
                    }
                }
            }, false, true);
        });
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
    LoadSalaryForEdit: function (salaryId) {
        var grid = $('#SalaryGridContainer').dxDataGrid('instance');
        if (!grid) {
            return;
        }
        var dataSource = grid.option('dataSource') || [];
        var record = dataSource.find(function (item) {
            return item.id == salaryId;
        });
        if (!record) {
            empr_helper.notify("Salary record not found.", 2);
            return;
        }

        $("#SALARY_ID").val(record.id);
        $("#BASIC_SALARY").val(record.basiC_SALARY ?? '');
        $("#HOUSE_ALLOWANCE").val(record.housE_ALLOWANCE ?? '');
        $("#MEDICAL_ALLOWANCE").val(record.medicaL_ALLOWANCE ?? '');
        $("#CONVEYANCE_ALLOWANCE").val(record.conveyancE_ALLOWANCE ?? '');
        $("#OVERTIME").val(record.overtime ?? '');
        $("#BONUS").val(record.bonus ?? '');
        $("#INCOME_TAX").val(record.incomE_TAX ?? '');
        $("#LOAN_DEDUCTION").val(record.loaN_DEDUCTION ?? '');
        $("#ADVANCE_DEDUCTION").val(record.advancE_DEDUCTION ?? '');
        $("#GROSS_SALARY").val(record.grosS_SALARY ?? '');
        $("#TOTAL_DEDUCTION").val(record.totaL_DEDUCTION ?? '');
        $("#NET_SALARY").val(record.neT_SALARY ?? '');
        $("#EFFECTIVE_FROM").val(empr_EmployeeSalary.FormatDateInput(record.effectivE_FROM));
        $("#EFFECTIVE_TO").val(empr_EmployeeSalary.FormatDateInput(record.effectivE_TO));
        empr_EmployeeSalary.CalculateTotals();

        if (Permissions != "Admin") {
            if (Permissions.r_EDIT) {
                $('#BtnSave').show();
            } else {
                $('#BtnSave').hide();
            }
            if (Permissions.r_DLT) {
                $('#BtnDelete').show();
            } else {
                $('#BtnDelete').hide();
            }
        } else {
            $('#BtnSave').show();
            $('#BtnDelete').show();
        }
        $('#BtnNew').show();
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
        var salaryId = parseInt($("#SALARY_ID").val(), 10) || 0;
        var record = {
            ID: salaryId,
            BASIC_SALARY: empr_EmployeeSalary.AmountValue('#BASIC_SALARY'),
            HOUSE_ALLOWANCE: empr_EmployeeSalary.AmountValue('#HOUSE_ALLOWANCE'),
            MEDICAL_ALLOWANCE: empr_EmployeeSalary.AmountValue('#MEDICAL_ALLOWANCE'),
            CONVEYANCE_ALLOWANCE: empr_EmployeeSalary.AmountValue('#CONVEYANCE_ALLOWANCE'),
            OVERTIME: empr_EmployeeSalary.AmountValue('#OVERTIME'),
            BONUS: empr_EmployeeSalary.AmountValue('#BONUS'),
            INCOME_TAX: empr_EmployeeSalary.AmountValue('#INCOME_TAX'),
            LOAN_DEDUCTION: empr_EmployeeSalary.AmountValue('#LOAN_DEDUCTION'),
            ADVANCE_DEDUCTION: empr_EmployeeSalary.AmountValue('#ADVANCE_DEDUCTION'),
            GROSS_SALARY: empr_EmployeeSalary.AmountValue('#GROSS_SALARY'),
            TOTAL_DEDUCTION: empr_EmployeeSalary.AmountValue('#TOTAL_DEDUCTION'),
            NET_SALARY: empr_EmployeeSalary.AmountValue('#NET_SALARY'),
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
            {
                dataField: "Action",
                width: 100,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                cellTemplate: function (container, options) {
                    var html = '<div class="btn-group btn-group-sm">';
                    if (Permissions != "Admin") {
                        if (Permissions.r_EDIT) {
                            html += `<a href="javascript:;" class="grid-action-icon elm_salary_edit" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>`;
                        }
                        if (Permissions.r_DLT) {
                            html += `<a href="javascript:;" class="grid-action-icon elm_salary_delete" style="margin-left: 8px" reportid=${options.data.id} title="Delete"><i class="fa fa-trash"></i></a>`;
                        }
                    } else {
                        html += `<a href="javascript:;" class="grid-action-icon elm_salary_edit" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>`;
                        html += `<a href="javascript:;" class="grid-action-icon elm_salary_delete" style="margin-left: 8px" reportid=${options.data.id} title="Delete"><i class="fa fa-trash"></i></a>`;
                    }
                    html += '</div>';
                    $(html).appendTo(container);
                }
            },
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
