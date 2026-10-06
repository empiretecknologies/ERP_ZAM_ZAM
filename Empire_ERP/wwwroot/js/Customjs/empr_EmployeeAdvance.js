var empr_EmployeeAdvance = {
    initEvents: function () {

        $(document).ready(function () {

            empr_EmployeeAdvance.InitEmployeeDDL();
            empr_EmployeeAdvance.InitBookTypeDDL();

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_EmployeeAdvance.validateForm()) {
                            empr_EmployeeAdvance.saveAttempt();
                        }
                    }
                } else {
                    if (empr_EmployeeAdvance.validateForm()) {
                        empr_EmployeeAdvance.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#quicksearch', function () {
                empr_EmployeeAdvance.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmployeeAdvance.GetEmployeeAdvanceByID(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
                empr_EmployeeAdvance.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_EmployeeAdvance.DeleteRecord();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#quicksearch').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/EmployeeAdvance/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_EmployeeAdvance.resetForm();
                    $('#BtnDelete').hide();
                    $('#BtnNew').hide();
                }
            }, false, true);
        });

    },
    resetForm: function () {
        $("#Code").val('');
        $("#ADVANCE_DATE").val(empr_helper.GetCurrentDate());
        $("#ADVANCE_AMOUNT").val('');
        $("#REMARKS").val('');
        $('#ASTATUS').dxSelectBox('instance').option('value', "Y");
        var employeeBox = $('#EMPLOYEE_ID').data('dxSelectBox');
        if (employeeBox) {
            employeeBox.option('value', null);
        }
        var bookTypeBox = $('#BookType').data('dxSelectBox');
        if (bookTypeBox) {
            bookTypeBox.option('value', null);
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
        var data = empr_EmployeeAdvance.GetDataToSave();

        if (data.ASTATUS == '' || data.ASTATUS == null) {
            valid = false;
            empr_helper.notify("Please select status.", 2);
        }

        if (data.EMPLOYEE_ID == '' || data.EMPLOYEE_ID == null) {
            valid = false;
            empr_helper.notify("Please select employee.", 2);
        }

        if (data.ADVANCE_DATE == '' || data.ADVANCE_DATE == null) {
            valid = false;
            empr_helper.notify("Please select advance date.", 2);
        }

        if (data.ADVANCE_AMOUNT == '' || data.ADVANCE_AMOUNT == null || parseFloat(data.ADVANCE_AMOUNT) <= 0) {
            valid = false;
            empr_helper.notify("Please enter advance amount.", 2);
        }

        if (data.BOOK_TYPE == '' || data.BOOK_TYPE == null || data.BOOK_TYPE == undefined) {
            valid = false;
            empr_helper.notify("Please select book type.", 2);
        }

        return valid;
    },
    GetDataToSave: function () {

        var ID = $("#Code").val();
        var EMPLOYEE_ID = $('#EMPLOYEE_ID').dxSelectBox('instance').option('value');
        var ADVANCE_DATE = $("#ADVANCE_DATE").val();
        var ADVANCE_AMOUNT = $("#ADVANCE_AMOUNT").val().trim();
        var REMARKS = $("#REMARKS").val().trim();
        var BOOK_TYPE = $('#BookType').dxSelectBox('option', 'value');
        var ASTATUS = $("#ASTATUS").dxSelectBox('instance').option('value');
        var modelRecord = {
            ID: ID,
            EMPLOYEE_ID: EMPLOYEE_ID,
            ADVANCE_DATE: ADVANCE_DATE == '' ? null : ADVANCE_DATE,
            ADVANCE_AMOUNT: ADVANCE_AMOUNT == '' ? null : ADVANCE_AMOUNT,
            REMARKS: REMARKS,
            BOOK_TYPE: BOOK_TYPE,
            ASTATUS: ASTATUS
        }
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_EmployeeAdvance.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/EmployeeAdvance/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_EmployeeAdvance.resetForm();
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
            }
        }, false, true);
    },
    InitQuickSearch: function () {
        empr_EmployeeAdvance.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/EmployeeAdvance/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_EmployeeAdvance.CreateGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetEmployeeAdvanceByID: function (id) {
        ajaxHelper.ajaxGetJson('/EmployeeAdvance/GetEmployeeAdvanceById?id=' + id, function (data) {
            if (data.msgType == 1) {

                var record = data.data;

                $("#Code").val(record.id);
                $('#ASTATUS').dxSelectBox('instance').option('value', record.astatus);
                $('#EMPLOYEE_ID').dxSelectBox('instance').option('value', record.employeE_ID);
                if (record.advancE_DATE) {
                    $("#ADVANCE_DATE").val(record.advancE_DATE.substring(0, 10));
                } else {
                    $("#ADVANCE_DATE").val('');
                }
                $("#ADVANCE_AMOUNT").val(record.advancE_AMOUNT);
                $("#REMARKS").val(record.remarks);
                $('#BookType').dxSelectBox('instance').option('value', record.booK_TYPE);

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
            url: 'EmployeeAdvance/GetEmployees',
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
    InitBookTypeDDL: function (_selectedValue) {
        $.ajax({
            url: 'EmployeeAdvance/GetBookTypes',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $('#BookType').dxSelectBox({
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
                        searchTimeout: 500,
                    });
                }
                else {
                    empr_helper.notify(data.data, data.msgType);
                }
            },
            error: function (error) {
                //console.error('Error fetching data:', error);
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
            { dataField: 'advancE_DATE', caption: 'Advance Date', dataType: 'date', format: 'dd-MM-yyyy' },
            { dataField: 'advancE_AMOUNT', caption: 'Advance Amount' },
            { dataField: 'booK_TYPE_NAME', caption: 'Book Type' },
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
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "EmployeeAdvance");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
}
