var empr_SalaryDisbursement = {
    rowsCount: 0,
    employeeData: [],
    skipPayrollLoad: false,

    InitEvents: function () {
        $(document).ready(function () {
            empr_SalaryDisbursement.ResetForm();

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_SalaryDisbursement.InitQuickSearchGrid();
            });

            $('body').on('click', '#BtnSave', function (e) {
                e.preventDefault();
                $("#Loader").show().css('display', 'flex');
                if ($('#BtnSave').prop('disabled')) {
                    return false;
                }
                $('#BtnSave').prop('disabled', true).hide();
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                        $('#BtnSave').prop('disabled', false).show();
                        setTimeout(function () { $("#Loader").hide(); }, 500);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                        $('#BtnSave').prop('disabled', false).show();
                        setTimeout(function () { $("#Loader").hide(); }, 500);
                    } else {
                        empr_SalaryDisbursement.ValidateAndPrepareDataForSave();
                        setTimeout(function () { $("#Loader").hide(); }, 500);
                    }
                } else {
                    empr_SalaryDisbursement.ValidateAndPrepareDataForSave();
                    setTimeout(function () { $("#Loader").hide(); }, 500);
                }
                $('#BtnSave').prop('disabled', false).show();
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.modal').modal('hide');
                empr_helper.selectedBill = id;
                empr_SalaryDisbursement.GetSalaryDisbursementByCode(id);
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_SalaryDisbursement.Delete();
            });

            $('body').on('click', '#BtnNew', function () {
                empr_SalaryDisbursement.ResetForm();
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },

    ResetForm: function () {
        empr_SalaryDisbursement.employeeData = [];
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE').val('');
        $('#REMARKS').val('');
        $('#BtnDelete').hide();
        empr_SalaryDisbursement.InitBookTypeDDL();
        empr_SalaryDisbursement.InitPayrollMonthDDL();
        empr_SalaryDisbursement.CreateGrid([{ __KEY__: empr_SalaryDisbursement.GenerateKey(36) }]);
        $('#V_DATE').val(todayDate);
        $('#V_DATE').focus();
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

    CreateGrid: function (dataSrc) {
        if (dataSrc.length > 0) {
            empr_SalaryDisbursement.rowsCount = dataSrc.length - 1;
            dataSrc.forEach(item => {
                if (
                    item.chQ_DATE == '1900-01-01' || item.chQ_DATE == '01-01-1900' || item.chQ_DATE == '01-Jan-1900' ||
                    item.chQ_DATE == '1/1/1900 12:00:00 AM' || item.chQ_DATE == '01/01/1900 12:00:00 AM' || item.chQ_DATE == '1/1/1900'
                ) {
                    item.chQ_DATE = null;
                }
            });
        }
        var col = [
            {
                dataField: "Action",
                width: 130,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                allowEditing: false,
                cellTemplate: function (container, options) {
                    const printAction = `<a href="javascript:;" class="grid-action-icon Print" style="margin-left: 8px" onclick="empr_SalaryDisbursement.PrintRow(${options.data.dT_CODE})" title="Print"><i class="fa fa-print"></i></a>`;
                    if (Permissions != "Admin") {
                        const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT)
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_SalaryDisbursement.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        const deleteAction = !Permissions.r_DLT
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_SalaryDisbursement.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const printBtn = !Permissions.r_PRINT ? '' : printAction;
                        $(`<div class="btn-group btn-group-sm">${addAction}${deleteAction}${printBtn}</div>`).appendTo(container);
                    } else {
                        $(`<div class="btn-group btn-group-sm">
                       <a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_SalaryDisbursement.AddRow()" title="Add"><i class="fa fa-add"></i></a>
                       <a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_SalaryDisbursement.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>
                       ${printAction}
                       </div>`).appendTo(container);
                    }
                }
            },
            {
                dataField: 'dT_CODE',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'emP_CODE',
                caption: 'Employee',
                width: 250,
                allowSorting: false,
                lookup: {
                    dataSource: {
                        store: empr_SalaryDisbursement.employeeData,
                        paginate: true,
                        pageSize: 50
                    },
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    showClearButton: true
                },
                setCellValue: function (newData, value) {
                    newData.emP_CODE = value;
                    var selected = empr_SalaryDisbursement.employeeData.filter(u => u.key == value);
                    if (selected.length > 0) {
                        newData.grosS_SALARY = selected[0].grosS_SALARY;
                        newData.advance = selected[0].advance;
                        newData.loan = selected[0].loan;
                        newData.neT_SALARY = selected[0].neT_SALARY;
                        newData.emP_NAME = selected[0].value;
                    } else {
                        newData.grosS_SALARY = null;
                        newData.advance = null;
                        newData.loan = null;
                        newData.neT_SALARY = null;
                        newData.emP_NAME = null;
                    }
                }
            },
            {
                dataField: 'grosS_SALARY',
                caption: 'Gross Salary',
                dataType: 'number',
                format: { type: 'fixedPoint', precision: 2 },
                allowEditing: false
            },
            {
                dataField: 'advance',
                caption: 'Advance',
                dataType: 'number',
                format: { type: 'fixedPoint', precision: 2 }
            },
            {
                dataField: 'loan',
                caption: 'Loan',
                dataType: 'number',
                format: { type: 'fixedPoint', precision: 2 }
            },
            {
                dataField: 'neT_SALARY',
                caption: 'Net Salary',
                dataType: 'number',
                format: { type: 'fixedPoint', precision: 2 },
                allowEditing: false
            },
            {
                dataField: 'chQ_NO',
                caption: 'Ref #',
            },
            {
                dataField: 'chQ_DATE',
                caption: 'Ref Date',
                dataType: 'date',
                format: 'dd-MM-yyyy',
                setCellValue: function (newData, value) {
                    if (value) {
                        var date = new Date(value);
                        var year = date.getFullYear();
                        if (year < 100) {
                            year += 2000;
                        }
                        newData.chQ_DATE = new Date(year, date.getMonth(), date.getDate());
                    } else {
                        newData.chQ_DATE = value;
                    }
                },
                cellTemplate: function (container, options) {
                    var $dateCell = $('<div>').appendTo(container);
                    var dateValue = options.value;
                    if (dateValue) {
                        var date = new Date(dateValue);
                        var day = ("0" + date.getDate()).slice(-2);
                        var month = ("0" + (date.getMonth() + 1)).slice(-2);
                        var year = date.getFullYear();
                        $dateCell.text(day + '-' + month + '-' + year);
                    }
                },
            },
        ];
        empr_helper.editableDxGridbindingForTransactionsVouchers('#DetailContainer', col, dataSrc, "SalaryDisbursement", "emP_CODE");
        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }
    },

    AddRow: function () {
        const gridIns = $('#DetailContainer').dxDataGrid('instance');
        const dataSrc = gridIns.option("dataSource");

        if (dataSrc.length >= Limit && Limit != 0) {
            empr_helper.notify("You can only add  " + Limit + " records.", 2);
            return;
        }
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                empr_SalaryDisbursement.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");
                dataSource.unshift({ __KEY__: empr_SalaryDisbursement.GenerateKey(36) });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
            });
        }
        else {
            empr_SalaryDisbursement.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            const dataSource = gridInstance.option("dataSource");
            dataSource.unshift({ __KEY__: empr_SalaryDisbursement.GenerateKey(36) });
            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();
        }
    },

    DeleteRow: function (index, dtCode) {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");
        if (dataSource.length > 0) {
            if (dataSource.length > 1) {
                if (dtCode == '' || dtCode == null || dtCode == undefined) {
                    gridInstance.deleteRow(index);
                    empr_SalaryDisbursement.rowsCount -= 1;
                    gridInstance.saveEditData();
                }
                else {
                    var availableRows = dataSource.filter(x => x.dT_CODE > 0);
                    if (availableRows.length > 1) {
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
                            ajaxHelper.ajaxPostJsonData({ tranID: $('#Code').val(), code: dtCode }, "/SalaryDisbursement/DeleteSalaryDisbursementDetailByCode", function (data) {
                                empr_helper.notify(data.msg, data.msgType);
                                if (data.msgType == 1) {
                                    gridInstance.deleteRow(index);
                                    empr_SalaryDisbursement.rowsCount -= 1;
                                    gridInstance.saveEditData();
                                }
                            }, false, true);
                        });
                    } else {
                        empr_helper.notify("You are not allowed to delete the last row.", 2);
                    }
                }
            } else {
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            }
        }
    },

    GenerateKey: function (keyLength) {
        var key = "";
        var characters = "abcdef0123456789";
        for (var i = 0; i < keyLength; i++) {
            if (i === 8 || i === 13 || i === 18 || i === 23) {
                key += "-";
            } else {
                key += characters.charAt(Math.floor(Math.random() * characters.length));
            }
        }
        return key;
    },

    BindSelectBox: function (element, dataSource, selectedValue, onValueChanged) {
        var options = {
            dataSource: dataSource || [],
            displayExpr: 'value',
            valueExpr: 'key',
            value: selectedValue || null,
            searchEnabled: true,
            width: '100%',
            placeholder: 'Search',
            showClearButton: true,
            dropDownOptions: { height: 'auto' },
            pagingEnabled: true,
            searchTimeout: 500
        };
        if (typeof onValueChanged === 'function') {
            options.onValueChanged = onValueChanged;
        }
        $(element).dxSelectBox(options);
    },

    InitBookTypeDDL: function (_selectedValue) {
        $.ajax({
            url: '/SalaryDisbursement/GetBookTypes',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    empr_SalaryDisbursement.BindSelectBox('#BookType', data.data, _selectedValue);
                }
                else {
                    empr_SalaryDisbursement.BindSelectBox('#BookType', [], _selectedValue);
                    if (data.msg || data.data) {
                        empr_helper.notify(data.msg || data.data, data.msgType);
                    }
                }
            },
            error: function () {
                empr_SalaryDisbursement.BindSelectBox('#BookType', [], _selectedValue);
                empr_helper.notify("Unable to load Book Type.", 2);
            }
        });
    },

    InitPayrollMonthDDL: function (_selectedValue) {
        $.ajax({
            url: '/SalaryDisbursement/GetPayrollMonths',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    empr_SalaryDisbursement.BindSelectBox('#PAYROLL_MONTH', data.data, _selectedValue, function (e) {
                        if (empr_SalaryDisbursement.skipPayrollLoad) {
                            empr_SalaryDisbursement.skipPayrollLoad = false;
                            return;
                        }
                        if (e.value) {
                            empr_SalaryDisbursement.LoadEmployeesByPayrollMonth(e.value, true);
                        } else {
                            empr_SalaryDisbursement.employeeData = [];
                            empr_SalaryDisbursement.CreateGrid([{ __KEY__: empr_SalaryDisbursement.GenerateKey(36) }]);
                        }
                    });
                }
                else {
                    empr_SalaryDisbursement.BindSelectBox('#PAYROLL_MONTH', [], _selectedValue);
                    if (data.msg || data.data) {
                        empr_helper.notify(data.msg || data.data, data.msgType);
                    }
                }
            },
            error: function () {
                empr_SalaryDisbursement.BindSelectBox('#PAYROLL_MONTH', [], _selectedValue);
                empr_helper.notify("Unable to load Payroll Month.", 2);
            }
        });
    },

    LoadEmployeesByPayrollMonth: function (payrollMonth, resetGrid) {
        ajaxHelper.ajaxGetJson('/SalaryDisbursement/GetEmployeesByPayrollMonth?payrollMonth=' + encodeURIComponent(payrollMonth || ''), function (data) {
            if (data.msgType == 1) {
                empr_SalaryDisbursement.employeeData = data.data || [];
                if (resetGrid) {
                    empr_SalaryDisbursement.CreateGrid([{ __KEY__: empr_SalaryDisbursement.GenerateKey(36) }]);
                } else {
                    var grid = $('#DetailContainer').dxDataGrid('instance');
                    if (grid) {
                        grid.columnOption('emP_CODE', 'lookup.dataSource', {
                            store: empr_SalaryDisbursement.employeeData,
                            paginate: true,
                            pageSize: 50
                        });
                        grid.refresh();
                    }
                }
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    InitQuickSearchGrid: function () {
        empr_SalaryDisbursement.GetSalaryDisbursements();
    },

    GetSalaryDisbursements: function () {
        ajaxHelper.ajaxGetJson('/SalaryDisbursement/GetSalaryDisbursements', function (data) {
            if (data.msgType == 1) {
                empr_SalaryDisbursement.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    CreateQuickSearchGrid: function (dataSrc) {
        var columns = [{
            dataField: "Action",
            width: 100,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            cellTemplate: function (container, options) {
                $(`<div class="btn-group btn-group-sm">
                   <a href="javascript:;" class="grid-action-icon elm_edit" reportid=${options.data.traN_ID} title="Edit"><i class="fa fa-edit"></i></a>
                   </div>`).appendTo(container);
            }
        },
        { dataField: 'traN_ID', caption: 'Code' },
        { dataField: 'voucheR_NO', caption: 'Voucher #' },
        { dataField: 'v_DATE', caption: 'Date', dataType: 'date', format: 'dd-MM-yyyy' },
        { dataField: 'payrolL_MONTH', caption: 'Payroll Month' },
        { dataField: 'booK_TYPE', caption: 'Book Type' },
        { dataField: 'emP_NAME', caption: 'Employee' },
        { dataField: 'grosS_SALARY', caption: 'Gross Salary' },
        { dataField: 'advance', caption: 'Advance' },
        { dataField: 'loan', caption: 'Loan' },
        { dataField: 'neT_SALARY', caption: 'Net Salary' },
        { dataField: 'chQ_NO', caption: 'Ref #' },
        { dataField: 'chQ_DATE', caption: 'Ref Date', dataType: 'date', format: 'dd-MM-yyyy' },
        { dataField: 'astatus', caption: 'Status' },
        { dataField: 'remarks', caption: 'Remarks', visible: false },
        { dataField: 'adD_USER_ID', caption: 'Created By', visible: false },
        { dataField: 'adD_DATE', caption: 'Created Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
        { dataField: 'adD_COMPUTER_NAME', caption: 'Created Computer', visible: false },
        { dataField: 'adD_IP_ADDRESS', caption: 'Created IP', visible: false },
        { dataField: 'adD_POSTALCODE', caption: 'Created Postal Code', visible: false },
        { dataField: 'ediT_USER_ID', caption: 'Updated By', visible: false },
        { dataField: 'ediT_COMPUTER_NAME', caption: 'Updated Computer', visible: false },
        { dataField: 'ediT_IP_ADDRESS', caption: 'Updated IP', visible: false },
        { dataField: 'ediT_DATE', caption: 'Updated Date', visible: false, dataType: 'date', format: 'dd-MM-yyy' },
        { dataField: 'ediT_POSTALCODE', caption: 'Updated Postal Code', visible: false },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', columns, dataSrc, "SalaryDisbursementQS", "multiple");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },

    ValidateAndPrepareDataForSave: function () {
        var proceed = function () {
            var IsValid = true;
            var V_DATE = $("#V_DATE").val();
            var BOOK_TYPE = $('#BookType').dxSelectBox('option', 'value');
            var PAYROLL_MONTH = $('#PAYROLL_MONTH').dxSelectBox('option', 'value');

            if (V_DATE == '' || V_DATE == null || V_DATE == undefined) {
                empr_helper.notify("Transaction date is required.", 2);
                return;
            }

            if (BOOK_TYPE == '' || BOOK_TYPE == null || BOOK_TYPE == undefined) {
                empr_helper.notify("Please select book type.", 2);
                return;
            }

            if (PAYROLL_MONTH == '' || PAYROLL_MONTH == null || PAYROLL_MONTH == undefined) {
                empr_helper.notify("Please select payroll month.", 2);
                return;
            }

            var detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
            if (Array.isArray(detailRecords) && detailRecords.some(item => item.key !== undefined)) {
                detailRecords = detailRecords.flatMap(group => group.items || []);
            }

            $.each(detailRecords, function (index, item) {
                if (item.emP_CODE == "" || item.emP_CODE == null || item.emP_CODE == undefined) {
                    empr_helper.notify("Please select employee at index " + index, 2);
                    IsValid = false;
                    return false;
                }

                if (!(item.chQ_DATE == "" || item.chQ_DATE == null || item.chQ_DATE == undefined)) {
                    item.chQ_DATE = empr_helper.PrepareDate(item.chQ_DATE);
                }
            });

            if (!IsValid) return;

            IsValid = empr_helper.validateDateRange($("#V_DATE").val(), minDate, maxDate);
            if (!IsValid) return;

            if ($("#Code").val() == 0
                || $("#Code").val() == null
                || $("#Code").val() == undefined
                || $("#Code").val() == "") {
                detailRecords.reverse();
            }

            var payrollDate = empr_SalaryDisbursement.ParsePayrollMonth(PAYROLL_MONTH);

            var codeVal = $("#Code").val();
            var saveList = detailRecords.map(function (obj) {
                var chqDate = obj.chQ_DATE;
                if (chqDate != '' && chqDate != null && chqDate != undefined) {
                    chqDate = empr_helper.PrepareDate(chqDate);
                } else {
                    chqDate = V_DATE;
                }
                return {
                    TRAN_ID: (codeVal == '' || codeVal == null || codeVal == undefined) ? 0 : codeVal,
                    ASTATUS: $('#ASTATUS').dxSelectBox('option', 'value'),
                    V_DATE: V_DATE,
                    VOUCHER_NO: $("#VOUCHER_NO").val(),
                    BOOK_TYPE: BOOK_TYPE,
                    REMARKS: $("#REMARKS").val(),
                    PAYROLL_MONTH: payrollDate,
                    EMP_CODE: obj.emP_CODE,
                    GROSS_SALARY: obj.grosS_SALARY,
                    ADVANCE: obj.advance,
                    LOAN: obj.loan,
                    NET_SALARY: obj.neT_SALARY,
                    CHQ_NO: obj.chQ_NO,
                    CHQ_DATE: chqDate,
                    DT_CODE: obj.dT_CODE || 0,
                    DT_DESC: obj.dT_DESC || ''
                };
            });

            empr_SalaryDisbursement.SaveInfo(saveList);
        };

        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                proceed();
            });
        } else {
            proceed();
        }
    },

    ParsePayrollMonth: function (payrollMonthText) {
        if (!payrollMonthText) return null;
        var parts = payrollMonthText.split(' ');
        if (parts.length < 2) return null;
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var monthIndex = monthNames.indexOf(parts[0]);
        var year = parseInt(parts[1], 10);
        if (monthIndex < 0 || isNaN(year)) return null;
        var month = (monthIndex + 1).toString().padStart(2, '0');
        return year + '-' + month + '-01';
    },

    SaveInfo: function (detailRecords) {
        ajaxHelper.ajaxPostJsonData({ modelRecord: detailRecords }, "/SalaryDisbursement/Save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                $('#BtnSave').prop('disabled', false).show();
                if ($("#Code").val() == 0
                    || $("#Code").val() == null
                    || $("#Code").val() == undefined
                    || $("#Code").val() == "") {
                    $('#Code').val(data.data.code);
                    empr_helper.selectedBill = data.data.code;
                    $('#VOUCHER_NO').val(data.data.voucherNo);
                }
                if (dataClear == 1) {
                    empr_SalaryDisbursement.GetSalaryDisbursementDetailsByCode($("#Code").val());
                    if (Permissions != "Admin") {
                        if (Permissions.r_DLT) {
                            $('#BtnDelete').show();
                        }
                    } else {
                        $('#BtnDelete').show();
                    }
                }
                else {
                    empr_SalaryDisbursement.ResetForm();
                }
            }
        }, false, true);
    },

    GetSalaryDisbursementByCode: function (code) {
        $("#Loader").show();
        $("#Loader").css('display', 'flex');
        ajaxHelper.ajaxGetJson('/SalaryDisbursement/GetSalaryDisbursementByCode?code=' + code, function (data) {
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                if (masterData.length == 1) {
                    var response = masterData[0];
                    $('#Code').val(response.traN_ID);
                    $('#V_DATE').val(response.v_DATE);
                    $('#VOUCHER_NO').val(response.voucheR_NO);
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                    empr_SalaryDisbursement.InitBookTypeDDL(response.booK_TYPE);
                    empr_SalaryDisbursement.skipPayrollLoad = true;
                    empr_SalaryDisbursement.InitPayrollMonthDDL(response.payrolL_MONTH);
                    $('#REMARKS').val(response.remarks);
                    if (response.payrolL_MONTH) {
                        empr_SalaryDisbursement.LoadEmployeesByPayrollMonth(response.payrolL_MONTH, false);
                    }
                    if (Permissions != "Admin") {
                        if (Permissions.r_DLT) {
                            $('#BtnDelete').show();
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
                    }
                }
                if (data.detail.msgType == 1) {
                    setTimeout(function () {
                        empr_SalaryDisbursement.CreateGrid(data.detail.data);
                        $("#Loader").hide();
                    }, 400);
                }
                else {
                    empr_helper.notify(data.detail.msg, data.detail.msgType);
                    $("#Loader").hide();
                }
            }
            else {
                empr_helper.notify(data.master.msg, data.master.msgType);
                $("#Loader").hide();
            }
        }, false, true);
    },

    GetSalaryDisbursementDetailsByCode: function (code) {
        ajaxHelper.ajaxGetJson('/SalaryDisbursement/GetSalaryDisbursementDetailsByCode?code=' + code, function (data) {
            if (data.msgType == 1) {
                empr_SalaryDisbursement.CreateGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    Delete: function () {
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/SalaryDisbursement/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_SalaryDisbursement.ResetForm();
                }
            }, false, true);
        });
    },

    PrintRow: function (dtCode) {
        var tranId = $("#Code").val();
        if (!tranId || tranId == 0) {
            empr_helper.notify("Please save the record before printing.", 2);
            return;
        }
        if (!dtCode || dtCode == 0 || dtCode == 'undefined' || dtCode == 'null') {
            empr_helper.notify("Please save the detail row before printing.", 2);
            return;
        }

        ajaxHelper.ajaxPostJsonData({ tranId: tranId, dtCode: dtCode }, "/SalaryDisbursement/GetPrintReport", function (data) {
            if (data.msgType == 1 && data.data) {
                window.open(window.location.origin + data.data + "?t=" + new Date().getTime(), "_blank");
            } else {
                empr_helper.notify(data.msg || "Unable to generate PDF.", data.msgType || 2);
            }
        }, false, true);
    },
}
