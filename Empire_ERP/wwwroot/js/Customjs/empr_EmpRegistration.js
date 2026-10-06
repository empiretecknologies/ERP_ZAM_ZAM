var empr_EmpRegistration = {
    initEvents: function () {

        $(document).ready(function () {

            empr_EmpRegistration.InitDepartmentDDL();
            empr_EmpRegistration.InitDesignationDDL();

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_EmpRegistration.validateForm()) {
                            empr_EmpRegistration.saveAttempt();
                        }
                    }
                } else {
                    if (empr_EmpRegistration.validateForm()) {
                        empr_EmpRegistration.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#quicksearch', function () {
                empr_EmpRegistration.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_EmpRegistration.GetEmpRegistrationByID(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
                empr_EmpRegistration.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_EmpRegistration.DeleteRecord();
            });

            $('#EMP_PIC_FILE').change(function () {
                empr_EmpRegistration.SaveImage();
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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/EmpRegistration/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_EmpRegistration.resetForm();
                    $('#BtnDelete').hide();
                    $('#BtnNew').hide();
                }
            }, false, true);
        });

    },
    resetForm: function () {
        $("#Code").val('');
        $("#EMPLOYEE_CODE").val('');
        $("#FIRST_NAME").val('');
        $("#LAST_NAME").val('');
        $("#EMAIL").val('');
        $("#PHONE").val('');
        $("#MACHINE_ID").val('');
        $("#JOINING_DATE").val('');
        $("#EMP_PIC").val('');
        $("#EMP_PIC_FILE").val('');
        $('#ASTATUS').dxSelectBox('instance').option('value', "Y");
        var departmentBox = $('#DEPARTMENT_ID').data('dxSelectBox');
        if (departmentBox) {
            departmentBox.option('value', null);
        }
        var designationBox = $('#DESIGNATION_ID').data('dxSelectBox');
        if (designationBox) {
            designationBox.option('value', null);
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
        var data = empr_EmpRegistration.GetDataToSave();

        if (data.ASTATUS == '' || data.ASTATUS == null) {
            valid = false;
            empr_helper.notify("Please select status.", 2);
        }

        if (data.EMPLOYEE_CODE == '') {
            valid = false;
            empr_helper.notify("Please enter employee code.", 2);
        }

        if (data.FIRST_NAME == '') {
            valid = false;
            empr_helper.notify("Please enter first name.", 2);
        }

        if (data.LAST_NAME == '') {
            valid = false;
            empr_helper.notify("Please enter last name.", 2);
        }

        if (data.PHONE == '') {
            valid = false;
            empr_helper.notify("Please enter phone.", 2);
        }

        if (data.DEPARTMENT_ID == '' || data.DEPARTMENT_ID == null) {
            valid = false;
            empr_helper.notify("Please select department.", 2);
        }

        if (data.DESIGNATION_ID == '' || data.DESIGNATION_ID == null) {
            valid = false;
            empr_helper.notify("Please select designation.", 2);
        }

        if (data.MACHINE_ID == '' || data.MACHINE_ID == null) {
            valid = false;
            empr_helper.notify("Please enter machine ID.", 2);
        }

        if (data.JOINING_DATE == '' || data.JOINING_DATE == null) {
            valid = false;
            empr_helper.notify("Please select joining date.", 2);
        }

        if (data.EMP_PIC == '' || data.EMP_PIC == null) {
            valid = false;
            empr_helper.notify("Please upload picture.", 2);
        }

        return valid;
    },
    GetDataToSave: function () {

        var ID = $("#Code").val();
        var EMPLOYEE_CODE = $("#EMPLOYEE_CODE").val().trim();
        var EMP_PIC = $("#EMP_PIC").val();
        var FIRST_NAME = $("#FIRST_NAME").val().trim();
        var LAST_NAME = $("#LAST_NAME").val().trim();
        var EMAIL = $("#EMAIL").val().trim();
        var PHONE = $("#PHONE").val().trim();
        var DEPARTMENT_ID = $('#DEPARTMENT_ID').dxSelectBox('instance').option('value');
        var DESIGNATION_ID = $('#DESIGNATION_ID').dxSelectBox('instance').option('value');
        var MACHINE_ID = $("#MACHINE_ID").val().trim();
        var JOINING_DATE = $("#JOINING_DATE").val();
        var ASTATUS = $("#ASTATUS").dxSelectBox('instance').option('value');
        var modelRecord = {
            ID: ID,
            EMPLOYEE_CODE: EMPLOYEE_CODE,
            EMP_PIC: EMP_PIC,
            FIRST_NAME: FIRST_NAME,
            LAST_NAME: LAST_NAME,
            EMAIL: EMAIL,
            PHONE: PHONE,
            DEPARTMENT_ID: DEPARTMENT_ID,
            DESIGNATION_ID: DESIGNATION_ID,
            MACHINE_ID: MACHINE_ID == '' ? null : MACHINE_ID,
            JOINING_DATE: JOINING_DATE == '' ? null : JOINING_DATE,
            ASTATUS: ASTATUS
        }
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_EmpRegistration.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/EmpRegistration/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_EmpRegistration.resetForm();
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
            }
        }, false, true);
    },
    SaveImage: function () {
        var files = document.getElementById('EMP_PIC_FILE').files;
        if (!files || files.length == 0) {
            return;
        }
        var formData = new FormData();
        formData.append("model", files[0]);
        $('#BtnSave').prop('disabled', true);
        $.ajax({
            url: "/EmpRegistration/SaveImage",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                if (data.msgType == 1) {
                    $("#EMP_PIC").val(data.data);
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                }
                $('#BtnSave').prop('disabled', false);
            },
            error: function () {
                $('#BtnSave').prop('disabled', false);
                empr_helper.notify("Something went wrong while saving the file. please re-upload the file.", 2);
            }
        });
    },
    InitQuickSearch: function () {
        empr_EmpRegistration.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/EmpRegistration/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_EmpRegistration.CreateGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetEmpRegistrationByID: function (id) {
        ajaxHelper.ajaxGetJson('/EmpRegistration/GetEmpRegistrationById?id=' + id, function (data) {
            if (data.msgType == 1) {

                var record = data.data;

                $("#Code").val(record.id);
                $('#ASTATUS').dxSelectBox('instance').option('value', record.astatus);
                $("#EMPLOYEE_CODE").val(record.employeE_CODE);
                $("#FIRST_NAME").val(record.firsT_NAME);
                $("#LAST_NAME").val(record.lasT_NAME);
                $("#EMAIL").val(record.email);
                $("#PHONE").val(record.phone);
                $("#MACHINE_ID").val(record.machinE_ID);
                if (record.joininG_DATE) {
                    $("#JOINING_DATE").val(record.joininG_DATE.substring(0, 10));
                } else {
                    $("#JOINING_DATE").val('');
                }
                $("#EMP_PIC").val(record.emP_PIC);
                $("#EMP_PIC_FILE").val('');
                $('#DEPARTMENT_ID').dxSelectBox('instance').option('value', record.departmenT_ID);
                $('#DESIGNATION_ID').dxSelectBox('instance').option('value', record.designatioN_ID);

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
    InitDepartmentDDL: function (_selectedValue) {
        $.ajax({
            url: 'EmpRegistration/GetDepartments',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $('#DEPARTMENT_ID').dxSelectBox({
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
    InitDesignationDDL: function (_selectedValue) {
        $.ajax({
            url: 'EmpRegistration/GetDesignations',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $('#DESIGNATION_ID').dxSelectBox({
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
                var html = '<div class="btn-group btn-group-sm">';
                if (options.data.emP_PIC != null && options.data.emP_PIC != '' && options.data.emP_PIC != undefined) {
                    html += `<a href="javascript:;" class="grid-action-icon" title="View Pic" onclick="ShowImage('/images/upload/empregistration/${options.data.emP_PIC}')"><i class="fa fa-eye"></i></a>`;
                }
                html += `<a href="javascript:;"  class="grid-action-icon elm_edit" style="padding-left: 6px;" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>`;
                html += '</div>';
                $(html).appendTo(container);
            }
        },
            { dataField: 'employeE_CODE', caption: 'Employee Code' },
            { dataField: 'firsT_NAME', caption: 'First Name' },
            { dataField: 'lasT_NAME', caption: 'Last Name' },
            { dataField: 'email', caption: 'Email' },
            { dataField: 'phone', caption: 'Phone' },
            { dataField: 'departmenT_NAME', caption: 'Department' },
            { dataField: 'designatioN_NAME', caption: 'Designation' },
            { dataField: 'machinE_ID', caption: 'Machine ID' },
            { dataField: 'joininG_DATE', caption: 'Joining Date', dataType: 'date', format: 'dd-MM-yyyy' },
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
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "EmpRegistration");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
}
