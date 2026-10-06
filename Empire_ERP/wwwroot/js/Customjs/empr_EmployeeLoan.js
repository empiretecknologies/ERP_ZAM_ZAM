var empr_EmployeeLoan = {
    initEvents: function () {

        $(document).ready(function () {

            empr_EmployeeLoan.InitEmployeeDDL();

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_EmployeeLoan.validateForm()) {
                            empr_EmployeeLoan.saveAttempt();
                        }
                    }
                } else {
                    if (empr_EmployeeLoan.validateForm()) {
                        empr_EmployeeLoan.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#quicksearch', function () {
                empr_EmployeeLoan.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmployeeLoan.GetEmployeeLoanByID(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
                empr_EmployeeLoan.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_EmployeeLoan.DeleteRecord();
            });

            $('#LOAN_AMOUNT, #TOTAL_INSTALLMENTS').on('input change', function () {
                empr_EmployeeLoan.CalculateInstallmentAmount();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#quicksearch').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },
    CalculateInstallmentAmount: function () {
        var loanAmount = parseFloat($("#LOAN_AMOUNT").val());
        var totalInstallments = parseInt($("#TOTAL_INSTALLMENTS").val());
        if (!isNaN(loanAmount) && !isNaN(totalInstallments) && totalInstallments > 0) {
            $("#INSTALLMENT_AMOUNT").val(Math.round((loanAmount / totalInstallments) * 100) / 100);
        } else {
            $("#INSTALLMENT_AMOUNT").val('');
        }
    },
    DeleteRecord: function () {

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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/EmployeeLoan/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_EmployeeLoan.resetForm();
                    $('#BtnDelete').hide();
                    $('#BtnNew').hide();
                }
            }, false, true);
        });

    },
    resetForm: function () {
        $("#Code").val('');
        $("#LOAN_DATE").val(empr_helper.GetCurrentDate());
        $("#LOAN_AMOUNT").val('');
        $("#TOTAL_INSTALLMENTS").val('');
        $("#INSTALLMENT_AMOUNT").val('');
        $("#START_DEDUCTION_DATE").val('');
        $("#REMARKS").val('');
        $('#ASTATUS').dxSelectBox('instance').option('value', "Y");
        var employeeBox = $('#EMPLOYEE_ID').data('dxSelectBox');
        if (employeeBox) {
            employeeBox.option('value', null);
        }

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
    validateForm: function () {

        var valid = true;
        var data = empr_EmployeeLoan.GetDataToSave();

        if (data.ASTATUS == '' || data.ASTATUS == null) {
            valid = false;
            empr_helper.notify("Please select status.", 2);
        }

        if (data.EMPLOYEE_ID == '' || data.EMPLOYEE_ID == null) {
            valid = false;
            empr_helper.notify("Please select employee.", 2);
        }

        if (data.LOAN_DATE == '' || data.LOAN_DATE == null) {
            valid = false;
            empr_helper.notify("Please select loan date.", 2);
        }

        if (data.LOAN_AMOUNT == '' || data.LOAN_AMOUNT == null || parseFloat(data.LOAN_AMOUNT) <= 0) {
            valid = false;
            empr_helper.notify("Please enter loan amount.", 2);
        }

        if (data.TOTAL_INSTALLMENTS == '' || data.TOTAL_INSTALLMENTS == null || !/^\d+$/.test(data.TOTAL_INSTALLMENTS) || parseInt(data.TOTAL_INSTALLMENTS) <= 0) {
            valid = false;
            empr_helper.notify("Please enter total installments.", 2);
        }

        if (data.START_DEDUCTION_DATE == '' || data.START_DEDUCTION_DATE == null) {
            valid = false;
            empr_helper.notify("Please select start deduction date.", 2);
        }

        return valid;
    },
    GetDataToSave: function () {

        empr_EmployeeLoan.CalculateInstallmentAmount();

        var ID = $("#Code").val();
        var EMPLOYEE_ID = $('#EMPLOYEE_ID').dxSelectBox('instance').option('value');
        var LOAN_DATE = $("#LOAN_DATE").val();
        var LOAN_AMOUNT = $("#LOAN_AMOUNT").val().trim();
        var TOTAL_INSTALLMENTS = $("#TOTAL_INSTALLMENTS").val().trim();
        var INSTALLMENT_AMOUNT = $("#INSTALLMENT_AMOUNT").val();
        var START_DEDUCTION_DATE = $("#START_DEDUCTION_DATE").val();
        var REMARKS = $("#REMARKS").val().trim();
        var ASTATUS = $("#ASTATUS").dxSelectBox('instance').option('value');
        var modelRecord = {
            ID: ID,
            EMPLOYEE_ID: EMPLOYEE_ID,
            LOAN_DATE: LOAN_DATE == '' ? null : LOAN_DATE,
            LOAN_AMOUNT: LOAN_AMOUNT == '' ? null : LOAN_AMOUNT,
            TOTAL_INSTALLMENTS: TOTAL_INSTALLMENTS == '' ? null : TOTAL_INSTALLMENTS,
            INSTALLMENT_AMOUNT: INSTALLMENT_AMOUNT == '' ? null : INSTALLMENT_AMOUNT,
            START_DEDUCTION_DATE: START_DEDUCTION_DATE == '' ? null : START_DEDUCTION_DATE,
            REMARKS: REMARKS,
            ASTATUS: ASTATUS
        }
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_EmployeeLoan.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/EmployeeLoan/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_EmployeeLoan.resetForm();
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
            }
        }, false, true);
    },
    InitQuickSearch: function () {
        empr_EmployeeLoan.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/EmployeeLoan/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_EmployeeLoan.CreateGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetEmployeeLoanByID: function (id) {
        ajaxHelper.ajaxGetJson('/EmployeeLoan/GetEmployeeLoanById?id=' + id, function (data) {
            if (data.msgType == 1) {

                var record = data.data;

                $("#Code").val(record.id);
                $('#ASTATUS').dxSelectBox('instance').option('value', record.astatus);
                $('#EMPLOYEE_ID').dxSelectBox('instance').option('value', record.employeE_ID);
                if (record.loaN_DATE) {
                    $("#LOAN_DATE").val(record.loaN_DATE.substring(0, 10));
                } else {
                    $("#LOAN_DATE").val('');
                }
                $("#LOAN_AMOUNT").val(record.loaN_AMOUNT);
                $("#TOTAL_INSTALLMENTS").val(record.totaL_INSTALLMENTS);
                $("#INSTALLMENT_AMOUNT").val(record.installmenT_AMOUNT);
                if (record.starT_DEDUCTION_DATE) {
                    $("#START_DEDUCTION_DATE").val(record.starT_DEDUCTION_DATE.substring(0, 10));
                } else {
                    $("#START_DEDUCTION_DATE").val('');
                }
                $("#REMARKS").val(record.remarks);

                $('.modal').modal('hide');

                if (Permissions != "Admin") {
                    if (Permissions.r_DLT) {
                        $('#BtnDelete').show();
                    }
                    if (Permissions.r_ADD) {
                        $('#BtnNew').show();
                    }
                    if (Permissions.r_EDIT) {
                        $('#BtnSave').show();
                    }
                    else {
                        $('#BtnSave').hide();
                    }
                } else {
                    $('#BtnSave').show();
                    $('#BtnDelete').show();
                    $('#BtnNew').show();
                }
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    InitEmployeeDDL: function (_selectedValue) {
        $.ajax({
            url: 'EmployeeLoan/GetEmployees',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $('#EMPLOYEE_ID').dxSelectBox({
                        dataSource: data.data,
                        displayExpr: 'value',
                        valueExpr: 'key',
                        value: _selectedValue,
                        searchEnabled: true,
                        width: '100%',
                        placeholder: 'Search',
                        showClearButton: true,
                        dropDownOptions: {
                            height: 'auto',
                        },
                        pagingEnabled: true,
                        searchTimeout: 500
                    });
                }
                else {
                    empr_helper.notify(data.data, data.msgType);
                }
            },
            error: function (error) {
                console.error('Error fetching data:', error);
            }
        });
    },
    CreateGrid: function (dataSrc) {
        var col = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                $(`<div class="btn-group btn-group-sm">
                   <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>
                   </div>`).appendTo(container);
            }
        },
            { dataField: 'id', caption: 'Code' },
            { dataField: 'employeE_NAME', caption: 'Employee' },
            { dataField: 'loaN_DATE', caption: 'Loan Date', dataType: 'date', format: 'dd-MM-yyyy' },
            { dataField: 'loaN_AMOUNT', caption: 'Loan Amount' },
            { dataField: 'totaL_INSTALLMENTS', caption: 'Total Installments' },
            { dataField: 'installmenT_AMOUNT', caption: 'Installment Amount' },
            { dataField: 'starT_DEDUCTION_DATE', caption: 'Start Deduction Date', dataType: 'date', format: 'dd-MM-yyyy' },
            { dataField: 'remarks', caption: 'Remarks' },
            { dataField: 'astatus', caption: 'Status' },
            { dataField: 'adD_USER_ID', caption: 'Created By', visible: false },
            { dataField: 'adD_DATE', caption: 'Created Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'adD_COMPUTER_NAME', caption: 'Created Computer', visible: false },
            { dataField: 'adD_IP_ADDRESS', caption: 'Created IP', visible: false },
            { dataField: 'adD_POSTALCODE', caption: 'Created PostalCode', visible: false },
            { dataField: 'ediT_USER_ID', caption: 'Updated By', visible: false },
            { dataField: 'ediT_COMPUTER_NAME', caption: 'Updated Computer', visible: false },
            { dataField: 'ediT_IP_ADDRESS', caption: 'Updated IP', visible: false },
            { dataField: 'ediT_DATE', caption: 'Updated Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'ediT_POSTALCODE', caption: 'Updated PostalCode', visible: false },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "EmployeeLoan");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
}
