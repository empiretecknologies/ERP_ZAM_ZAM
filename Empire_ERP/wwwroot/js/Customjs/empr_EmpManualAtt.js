var empr_EmpManualAtt = {
    employeeData: [],
    initEvents: function () {

        $(document).ready(function () {

            empr_EmpManualAtt.InitShiftStatusDDL();
            empr_EmpManualAtt.InitEmployeeDDL();
            empr_EmpManualAtt.SetDefaultTime();
            empr_EmpManualAtt.InitQuickSearch();

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_EmpManualAtt.validateForm()) {
                            empr_EmpManualAtt.saveAttempt();
                        }
                    }
                } else {
                    if (empr_EmpManualAtt.validateForm()) {
                        empr_EmpManualAtt.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#QuickSearch', function () {
                empr_EmpManualAtt.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmpManualAtt.GetEmpManualAttByID(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
                empr_EmpManualAtt.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_EmpManualAtt.DeleteRecord();
            });

            if (Permissions != "Admin") {
                !Permissions.r_VIEW && $('#gridContainer').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },
    SetDefaultTime: function () {
        var now = new Date();
        var hours = now.getHours();
        var minutes = now.getMinutes();
        hours = (hours < 10) ? '0' + hours : hours;
        minutes = (minutes < 10) ? '0' + minutes : minutes;
        $("#PUNCH_TIME").val(hours + ':' + minutes);
    },
    InitShiftStatusDDL: function () {
        $('#SHIFT_STATUS').dxSelectBox({
            dataSource: [
                { value: 'Check In', text: 'Check In' },
                { value: 'Check Out', text: 'Check Out' }
            ],
            valueExpr: 'value',
            displayExpr: 'text',
            searchEnabled: true,
            width: '100%',
            placeholder: 'Select',
            showClearButton: true,
            dropDownOptions: {
                height: 'auto',
            },
            pagingEnabled: true,
            searchTimeout: 500,
            value: 'Check In'
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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/EmpManualAtt/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_EmpManualAtt.resetForm();
                    empr_EmpManualAtt.InitQuickSearch();
                    $('#optmodal').modal('hide');
                    $('#BtnDelete').hide();
                    $('#BtnNew').hide();
                }
            }, false, true);
        });

    },
    resetForm: function () {
        $("#Code").val('');
        $("#MACHINE_ID").val('');
        $("#PUNCH_DATE").val(empr_helper.GetCurrentDate());
        empr_EmpManualAtt.SetDefaultTime();
        $('#ASTATUS').dxSelectBox('instance').option('value', "Y");
        var shiftBox = $('#SHIFT_STATUS').dxSelectBox('instance');
        if (shiftBox) {
            shiftBox.option('value', "Check In");
        }
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
        var data = empr_EmpManualAtt.GetDataToSave();

        if (data.ASTATUS == '' || data.ASTATUS == null) {
            valid = false;
            empr_helper.notify("Please select status.", 2);
        }

        if (data.EMPLOYEE_ID == '' || data.EMPLOYEE_ID == null) {
            valid = false;
            empr_helper.notify("Please select employee.", 2);
        }

        if (data.MACHINE_ID == '' || data.MACHINE_ID == null) {
            valid = false;
            empr_helper.notify("Selected employee does not have a Machine ID.", 2);
        }

        if ($("#PUNCH_DATE").val() == '' || $("#PUNCH_DATE").val() == null) {
            valid = false;
            empr_helper.notify("Please select date.", 2);
        }

        if ($("#PUNCH_TIME").val() == '' || $("#PUNCH_TIME").val() == null) {
            valid = false;
            empr_helper.notify("Please select time.", 2);
        }

        if (data.SHIFT_STATUS == '' || data.SHIFT_STATUS == null) {
            valid = false;
            empr_helper.notify("Please select shift status.", 2);
        }

        return valid;
    },
    GetDataToSave: function () {

        var ID = $("#Code").val();
        var EMPLOYEE_ID = null;
        var employeeBox = $('#EMPLOYEE_ID').dxSelectBox('instance');
        if (employeeBox) {
            EMPLOYEE_ID = employeeBox.option('value');
        }
        var MACHINE_ID = $("#MACHINE_ID").val();
        var punchDate = $("#PUNCH_DATE").val();
        var punchTime = $("#PUNCH_TIME").val();
        if (punchTime && punchTime.length === 5) {
            punchTime = punchTime + ':00';
        }
        var punch_time = null;
        if (punchDate && punchTime) {
            punch_time = punchDate + ' ' + punchTime;
        }
        var SHIFT_STATUS = $("#SHIFT_STATUS").dxSelectBox('instance').option('value');
        var ASTATUS = $("#ASTATUS").dxSelectBox('instance').option('value');
        var modelRecord = {
            TRAN_ID: ID,
            EMPLOYEE_ID: EMPLOYEE_ID,
            MACHINE_ID: MACHINE_ID == '' ? null : MACHINE_ID,
            punch_time: punch_time,
            SHIFT_STATUS: SHIFT_STATUS,
            ASTATUS: ASTATUS
        }
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_EmpManualAtt.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/EmpManualAtt/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_EmpManualAtt.resetForm();
                empr_EmpManualAtt.InitQuickSearch();
                $('#optmodal').modal('hide');
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
            }
        }, false, true);
    },
    InitQuickSearch: function () {
        empr_EmpManualAtt.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/EmpManualAtt/QuickSearch', function (data) {
            empr_EmpManualAtt.CreateGrid(data.data);
        }, false, true);
    },
    GetEmpManualAttByID: function (id) {
        ajaxHelper.ajaxGetJson('/EmpManualAtt/GetEmpManualAttById?id=' + id, function (data) {
            empr_EmpManualAtt.resetForm();
            if (data.msgType == 1) {

                var record = data.data;

                $("#Code").val(record.traN_ID);
                $('#ASTATUS').dxSelectBox('instance').option('value', record.astatus);
                $("#MACHINE_ID").val(record.machinE_ID);
                $('#EMPLOYEE_ID').dxSelectBox('instance').option('value', record.employeE_ID);
                if (record.punch_time) {
                    $("#PUNCH_DATE").val(record.punch_time.substring(0, 10));
                    var timePart = record.punch_time.substring(11, 16);
                    $("#PUNCH_TIME").val(timePart);
                } else {
                    $("#PUNCH_DATE").val('');
                    $("#PUNCH_TIME").val('');
                }
                $('#SHIFT_STATUS').dxSelectBox('instance').option('value', record.shifT_STATUS);

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
            url: 'EmpManualAtt/GetEmployees',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    empr_EmpManualAtt.employeeData = data.data;
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
                        searchTimeout: 500,
                        onValueChanged: function (e) {
                            if (e.value == null) {
                                $("#MACHINE_ID").val('');
                                return;
                            }
                            var selected = empr_EmpManualAtt.employeeData.find(function (item) {
                                return item.key == e.value;
                            });
                            if (selected && selected.machineId != null) {
                                $("#MACHINE_ID").val(selected.machineId);
                            } else {
                                $("#MACHINE_ID").val('');
                            }
                        }
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
                var html = '<div class="btn-group btn-group-sm">';
                html += `<a href="javascript:;"  class="grid-action-icon elm_edit" style="padding-left: 6px;" reportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>`;
                html += '</div>';
                $(html).appendTo(container);
            }
        },
            { dataField: 'traN_ID', caption: 'Code' },
            { dataField: 'employeE_NAME', caption: 'Employee' },
            { dataField: 'machinE_ID', caption: 'Machine ID' },
            { dataField: 'punch_time', caption: 'Punch Time', dataType: 'datetime', format: 'dd-MM-yyyy HH:mm' },
            { dataField: 'shifT_STATUS', caption: 'Shift Status' },
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
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "EmpManualAtt");
    },
}
