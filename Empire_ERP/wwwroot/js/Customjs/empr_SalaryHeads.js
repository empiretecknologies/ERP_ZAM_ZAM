var empr_SalaryHeads = {
    // id = form control / model property, json = property name returned by the server, caption = label
    Heads: [
        { id: 'BASIC_SALARY_COA_ID', json: 'basiC_SALARY_COA_ID', caption: 'Basic Salary' },
        { id: 'HOUSE_ALLOWANCE_COA_ID', json: 'housE_ALLOWANCE_COA_ID', caption: 'House Allowance' },
        { id: 'MEDICAL_ALLOWANCE_COA_ID', json: 'medicaL_ALLOWANCE_COA_ID', caption: 'Medical Allowance' },
        { id: 'CONVEYANCE_ALLOWANCE_COA_ID', json: 'conveyancE_ALLOWANCE_COA_ID', caption: 'Conveyance Allowance' },
        { id: 'OVERTIME_COA_ID', json: 'overtimE_COA_ID', caption: 'Overtime' },
        { id: 'BONUS_COA_ID', json: 'bonuS_COA_ID', caption: 'Bonus' },
        { id: 'INCOME_TAX_COA_ID', json: 'incomE_TAX_COA_ID', caption: 'Income Tax' },
        { id: 'LOAN_DEDUCTION_COA_ID', json: 'loaN_DEDUCTION_COA_ID', caption: 'Loan Deduction' },
        { id: 'ADVANCE_DEDUCTION_COA_ID', json: 'advancE_DEDUCTION_COA_ID', caption: 'Advance Deduction' }
    ],
    initEvents: function () {

        $(document).ready(function () {

            empr_SalaryHeads.InitChartDDL();

            $('#BtnSave').click(function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_SalaryHeads.validateForm()) {
                            empr_SalaryHeads.saveAttempt();
                        }
                    }
                } else {
                    if (empr_SalaryHeads.validateForm()) {
                        empr_SalaryHeads.saveAttempt();
                    }
                }
            });

            $('body').on('click', '#quicksearch', function () {
                empr_SalaryHeads.InitQuickSearch();
            });

            $('body').on('click', '.elm_edit', function () {
                var reportid = $(this).attr("reportid");
                empr_SalaryHeads.GetSalaryHeadsByID(reportid);
            });

            $('body').on('click', '#BtnNew', function () {
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
                empr_SalaryHeads.resetForm();
            });

            $('#BtnDelete').click(function () {
                empr_SalaryHeads.DeleteRecord();
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
            ajaxHelper.ajaxPostJsonData({ id: $('#Code').val() }, "/SalaryHeads/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_SalaryHeads.resetForm();
                    $('#BtnDelete').hide();
                    $('#BtnNew').hide();
                }
            }, false, true);
        });

    },
    resetForm: function () {
        $("#Code").val('');
        $('#ASTATUS').dxSelectBox('instance').option('value', "Y");
        $.each(empr_SalaryHeads.Heads, function (i, head) {
            var box = $('#' + head.id).data('dxSelectBox');
            if (box) {
                box.option('value', null);
            }
        });

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
        var data = empr_SalaryHeads.GetDataToSave();

        $.each(empr_SalaryHeads.Heads, function (i, head) {
            if (data[head.id] == '' || data[head.id] == null) {
                valid = false;
                empr_helper.notify("Please select " + head.caption.toLowerCase() + ".", 2);
            }
        });

        return valid;
    },
    GetDataToSave: function () {

        var modelRecord = {
            ID: $("#Code").val(),
            ASTATUS: $("#ASTATUS").dxSelectBox('instance').option('value')
        };
        $.each(empr_SalaryHeads.Heads, function (i, head) {
            modelRecord[head.id] = $('#' + head.id).dxSelectBox('instance').option('value');
        });
        return modelRecord;
    },
    saveAttempt: function () {

        var obj = empr_SalaryHeads.GetDataToSave();
        ajaxHelper.ajaxPostJsonData(obj, "/SalaryHeads/save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                empr_SalaryHeads.resetForm();
                $('#BtnDelete').hide();
                $('#BtnNew').hide();
            }
        }, false, true);
    },
    InitQuickSearch: function () {
        empr_SalaryHeads.GetAll();
    },
    GetAll: function () {
        ajaxHelper.ajaxGetJson('/SalaryHeads/QuickSearch', function (data) {
            if (data.msgType == 1) {
                empr_SalaryHeads.CreateGrid(data.data);
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetSalaryHeadsByID: function (id) {
        ajaxHelper.ajaxGetJson('/SalaryHeads/GetSalaryHeadsById?id=' + id, function (data) {
            if (data.msgType == 1) {

                var record = data.data;

                $("#Code").val(record.id);
                $('#ASTATUS').dxSelectBox('instance').option('value', record.astatus);
                $.each(empr_SalaryHeads.Heads, function (i, head) {
                    $('#' + head.id).dxSelectBox('instance').option('value', record[head.json]);
                });

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
    InitChartDDL: function () {
        $.ajax({
            url: 'SalaryHeads/GetChartAccounts',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $.each(empr_SalaryHeads.Heads, function (i, head) {
                        $('#' + head.id).dxSelectBox({
                            dataSource: data.data,
                            displayExpr: 'value',
                            valueExpr: 'key',
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
                html += `<a href="javascript:;"  class="grid-action-icon elm_edit" style="padding-left: 6px;" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>`;
                html += '</div>';
                $(html).appendTo(container);
            }
        },
            { dataField: 'id', caption: 'Code' },
            { dataField: 'basiC_SALARY_NAME', caption: 'Basic Salary' },
            { dataField: 'housE_ALLOWANCE_NAME', caption: 'House Allowance' },
            { dataField: 'medicaL_ALLOWANCE_NAME', caption: 'Medical Allowance' },
            { dataField: 'conveyancE_ALLOWANCE_NAME', caption: 'Conveyance Allowance' },
            { dataField: 'overtimE_NAME', caption: 'Overtime' },
            { dataField: 'bonuS_NAME', caption: 'Bonus' },
            { dataField: 'incomE_TAX_NAME', caption: 'Income Tax' },
            { dataField: 'loaN_DEDUCTION_NAME', caption: 'Loan Deduction' },
            { dataField: 'advancE_DEDUCTION_NAME', caption: 'Advance Deduction' },
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
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "SalaryHeads");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
}
