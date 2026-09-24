var empr_CustomerPricing = {
    totalCount: 0,
    rowsCount: 0,
    DC_TYPE: '',
    act_code: '',
    _itemRateMap: null,

    InitEvents: function () {
        $(document).ready(function () {
            empr_CustomerPricing.ResetForm();
            empr_CustomerPricing.InitPartyDDL();
            empr_CustomerPricing.InitQuickSearchGrid();

            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_CustomerPricing.GetCommisionMapByCode(data.traN_ID);
                }
            });

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_CustomerPricing.InitQuickSearchGrid();
            });

            $('body').on('click', '#BtnSave', function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        empr_CustomerPricing.ValidateAndPrepareDataForSave();
                    }
                } else {
                    empr_CustomerPricing.ValidateAndPrepareDataForSave();
                }
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.modal').modal('hide');
                empr_helper.selectedBill = id;
                empr_CustomerPricing.GetCommisionMapByCode(id);
            });

            $('body').on('click', '.elm_copy', function () {
                var id = $(this).attr("reportid");
                swal({
                    title: 'Are you sure you want to Copy this record?',
                    text: "",
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#0CC27E',
                    cancelButtonColor: '#FF586B',
                    confirmButtonText: 'Yes',
                    cancelButtonText: 'No',
                    confirmButtonClass: 'btn btn-success mr-5',
                    cancelButtonClass: 'btn btn-danger',
                    buttonsStyling: false
                }).then(function () {
                    empr_helper.selectedBill = id;
                    empr_CustomerPricing.InitPartyDDL(null, 'PartyModel');
                    $('#CustomerPricingCopy').modal('show');
                });
            });

            $('body').on('click', '#CustomerPricingCopy #saveCopiedRecord', function () {
                var partyInstance = $('#PartyModel').dxSelectBox('instance');
                if (!partyInstance) {
                    empr_helper.notify("Please select a Party.", 2);
                    return;
                }
                var selectedObj = partyInstance.option('selectedItem');
                if (!selectedObj) {
                    empr_helper.notify("Please select a Party.", 2);
                    return;
                }
                ajaxHelper.ajaxPostJsonData({
                    grouP_CODE: empr_helper.selectedBill,
                    party: selectedObj.key,
                    acT_CODE: selectedObj.accountCode
                }, "/CustomerPricing/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_CustomerPricing.GetCommisionMapByCode(data.data.code);
                    }
                }, false, true);
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_CustomerPricing.Delete();
            });

            $('body').on('click', '#BtnNew', function () {
                empr_CustomerPricing.ResetForm();
                $('#PARTY_CODE').dxSelectBox('instance').option('value', null);
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                !Permissions.r_PRINT && $('.btn-print').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },

    GetItemRateMap: function () {
        if (!empr_CustomerPricing._itemRateMap) {
            empr_CustomerPricing._itemRateMap = {};
            if (Array.isArray(ItemMaster)) {
                for (var i = 0; i < ItemMaster.length; i++) {
                    empr_CustomerPricing._itemRateMap[String(ItemMaster[i].key)] = ItemMaster[i].rate;
                }
            }
        }
        return empr_CustomerPricing._itemRateMap;
    },

    InitPartyDDL: function (selectedValue, targetId) {
        var elementId = targetId || 'PARTY_CODE';
        $('#' + elementId).dxSelectBox({
            dataSource: {
                store: PartyType,
                paginate: true,
            },
            paging: {
                enabled: true,
                pageSize: 50,
            },
            displayExpr: 'value',
            valueExpr: 'customizedKey',
            value: selectedValue || null,
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
                if (elementId !== 'PARTY_CODE') {
                    if (e.value) {
                        var selectedObj = e.component.option('selectedItem');
                        if (selectedObj) {
                            empr_CustomerPricing.act_code = selectedObj.accountCode || '';
                        }
                    } else {
                        empr_CustomerPricing.act_code = '';
                    }
                    return;
                }

                if (e.value) {
                    var selectedObj = e.component.option('selectedItem');
                    if (selectedObj) {
                        $('#HIDDEN_PARTY_CODE').val(selectedObj.key || '');
                        $('#HIDDEN_ACCOUNT_CODE').val(selectedObj.accountCode || '');
                        empr_CustomerPricing.act_code = selectedObj.accountCode || '';
                    }
                } else {
                    $('#HIDDEN_PARTY_CODE').val('');
                    $('#HIDDEN_ACCOUNT_CODE').val('');
                    empr_CustomerPricing.act_code = '';
                }
            }
        });
    },

    ResetForm: function () {
        empr_CustomerPricing.CreateGrid([{ __KEY__: empr_CustomerPricing.GenerateKey(36), dC_TYPE: empr_CustomerPricing.DC_TYPE }]);
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE, #hdnDOC').val('');

        $('#BtnDelete').hide();

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

    ApplyGridPerformanceOptions: function () {
        var grid = $('#DetailContainer').dxDataGrid('instance');
        if (!grid) return;

        grid.option({
            height: 420,
            scrolling: {
                mode: 'virtual',
                rowRenderingMode: 'virtual',
                columnRenderingMode: 'virtual',
                useNative: false
            },
            paging: { enabled: false },
            groupPanel: { visible: false },
            headerFilter: { visible: false },
            searchPanel: { visible: true, width: 240 },
            repaintChangesOnly: true,
            renderAsync: true,
            rowAlternationEnabled: false,
            columnAutoWidth: false
        });
    },

    CreateGrid: function (dataSrc) {
        dataSrc = dataSrc || [];
        if (dataSrc.length > 0) {
            empr_CustomerPricing.rowsCount = dataSrc.length - 1;
        }

        var rateMap = empr_CustomerPricing.GetItemRateMap();

        var col = [
            {
                dataField: "Action",
                width: 120,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                allowEditing: false,
                cellTemplate: function (container, options) {
                    var copyAction = '';
                    var addAction = '';
                    var deleteAction = '';

                    if (Permissions != "Admin") {
                        if (Permissions.r_COPY) {
                            copyAction = `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_CustomerPricing.CloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        }
                        if (Permissions.r_ADD || Permissions.r_EDIT) {
                            addAction = `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_CustomerPricing.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        }
                        if (Permissions.r_DLT) {
                            deleteAction = `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_CustomerPricing.DeleteRow(${options.rowIndex},${options.data.dT_CODE || 0})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        }
                    } else {
                        copyAction = `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_CustomerPricing.CloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        addAction = `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_CustomerPricing.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        deleteAction = `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_CustomerPricing.DeleteRow(${options.rowIndex},${options.data.dT_CODE || 0})" title="Delete"><i class="fa fa-trash"></i></a>`;
                    }

                    $(`<div class="btn-group btn-group-sm">${copyAction}${addAction}${deleteAction}</div>`).appendTo(container);
                }
            },
            {
                dataField: 'grouP_CODE',
                caption: 'Code',
                width: 100,
                visible: false,
            },
            {
                dataField: 'dT_CODE',
                caption: 'dtcode',
                visible: false,
            },
            {
                dataField: 'iteM_CODE',
                caption: 'Items',
                lookup: {
                    dataSource: {
                        store: ItemMaster,
                        paginate: true,
                        pageSize: 50
                    },
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    showClearButton: true
                },
                setCellValue: function (newData, value) {
                    newData.iteM_CODE = value;
                    newData.rate = rateMap[String(value)] || 0;
                }
            },
            {
                dataField: 'rate',
                caption: 'Rate',
                width: 120,
            },
        ];

        empr_helper.editableDxGridbindingForTransactionsVouchers('#DetailContainer', col, dataSrc, "CustomerPricing", "custoM_ACT_CODE");
        empr_CustomerPricing.ApplyGridPerformanceOptions();

        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }
    },

    CloneRow: function (index) {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        let dataSource = gridInstance.option("dataSource") || [];

        if (dataSource.length >= Limit && Limit != 0) {
            empr_helper.notify("You can only add " + Limit + " records.", 2);
            return;
        }

        function cloneRowAtIndex(idx) {
            let ds = gridInstance.option("dataSource") || [];
            if (!ds[idx]) return;

            let clonedRowData = $.extend(true, {}, ds[idx]);
            if (clonedRowData.hasOwnProperty('dT_CODE')) delete clonedRowData.dT_CODE;
            clonedRowData.__KEY__ = empr_CustomerPricing.GenerateKey(36);

            ds.unshift(clonedRowData);
            gridInstance.option("dataSource", ds);
            empr_CustomerPricing.rowsCount += 1;
        }

        if (gridInstance.hasEditData()) {
            gridInstance.saveEditData().done(function () {
                cloneRowAtIndex(index);
            });
        } else {
            cloneRowAtIndex(index);
        }
    },

    AddRow: function () {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        const dataSrc = gridInstance.option("dataSource") || [];

        if (dataSrc.length >= Limit && Limit != 0) {
            empr_helper.notify("You can only add  " + Limit + " records.", 2);
            return;
        }

        function insertBlank() {
            const ds = gridInstance.option("dataSource") || [];
            ds.unshift({
                __KEY__: empr_CustomerPricing.GenerateKey(36),
                dC_TYPE: empr_CustomerPricing.DC_TYPE,
                grouP_CODE: $('#Code').val() || 0
            });
            gridInstance.option("dataSource", ds);
            empr_CustomerPricing.rowsCount += 1;
        }

        if (gridInstance.hasEditData()) {
            gridInstance.saveEditData().done(insertBlank);
        } else {
            insertBlank();
        }
    },

    DeleteRow: function (index, dtCode) {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource") || [];

        if (dataSource.length === 0) return;

        if (dataSource.length === 1) {
            if (dtCode) {
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            } else {
                empr_CustomerPricing.CreateGrid([{ __KEY__: empr_CustomerPricing.GenerateKey(36), dC_TYPE: empr_CustomerPricing.DC_TYPE }]);
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            }
            return;
        }

        if (!dtCode) {
            dataSource.splice(index, 1);
            gridInstance.option("dataSource", dataSource);
            empr_CustomerPricing.rowsCount -= 1;
            return;
        }

        var availableRows = dataSource.filter(function (x) { return x.dT_CODE > 0; });
        if (availableRows.length <= 1) {
            empr_helper.notify("You are not allowed to delete the last row.", 2);
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
            ajaxHelper.ajaxPostJsonData({ gcode: $('#Code').val(), code: dtCode }, "/CustomerPricing/DeleteCommisionMapDetailByCode", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    var ds = gridInstance.option("dataSource") || [];
                    ds.splice(index, 1);
                    gridInstance.option("dataSource", ds);
                    empr_CustomerPricing.rowsCount -= 1;
                }
            }, false, true);
        });
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

    InitQuickSearchGrid: function () {
        empr_CustomerPricing.GetCommisionMap();
    },

    GetCommisionMap: function () {
        ajaxHelper.ajaxGetJson('/CustomerPricing/GetCommisionMap', function (data) {
            if (data.msgType == 1) {
                empr_CustomerPricing.CreateQuickSearchGrid(data.data);
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
                if (Permissions != "Admin" && !Permissions.r_PRINT) {
                    $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.grouP_CODE} title="Edit"><i class="fa fa-edit"></i></a>
                               </div>`).appendTo(container);
                } else {
                    $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.grouP_CODE} title="Edit"><i class="fa fa-edit"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" party=${options.data.partY_CODE} reportid=${options.data.grouP_CODE} title="COPY"><i class="fa fa-copy"></i></a>
                               </div>`).appendTo(container);
                }
            }
        },
        { dataField: 'grouP_CODE', caption: 'Code' },
        { dataField: 'dT_CODE', caption: 'DetailCode', visible: false },
        { dataField: 'partY_NAME', caption: 'Party Name' },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', columns, dataSrc, "CashReceiptVoucherQS", "single");
        setTimeout(function () {
            var qsGrid = $('#gridContainer').dxDataGrid('instance');
            if (qsGrid) qsGrid.resize();
        }, 500);
    },

    FlattenDetailRecords: function (detailRecords) {
        if (Array.isArray(detailRecords) && detailRecords.some(function (item) { return item.key !== undefined; })) {
            return detailRecords.flatMap(function (group) { return group.items || []; });
        }
        return detailRecords;
    },

    PrepareSavePayload: function (detailRecords) {
        var party = $('#HIDDEN_PARTY_CODE').val();
        var actCode = $('#HIDDEN_ACCOUNT_CODE').val();
        var astatus = $('#ASTATUS').dxSelectBox('option', 'value');
        var groupCode = $("#Code").val() || 0;
        var seenItems = {};
        var duplicateItems = [];
        var payload = [];

        if (!party) {
            empr_helper.notify("Party is required for all records.", 2);
            return null;
        }

        for (var i = 0; i < detailRecords.length; i++) {
            var obj = detailRecords[i];
            var itemCode = obj.iteM_CODE || obj.ITEM_KEY || obj.ITEM_CODE;
            var rate = obj.rate || obj.RATE;

            if (!itemCode) {
                empr_helper.notify("Item is required for all records.", 2);
                return null;
            }

            if (!rate || Number(rate) == 0) {
                empr_helper.notify("RATE  must be greater than 0.", 2);
                return null;
            }

            var itemKey = String(itemCode);
            if (seenItems[itemKey]) {
                duplicateItems.push(itemKey);
            } else {
                seenItems[itemKey] = true;
            }

            payload.push({
                GROUP_CODE: groupCode,
                DT_CODE: obj.dT_CODE || obj.DT_CODE || 0,
                ITEM_CODE: itemCode,
                PARTY: party,
                ACT_CODE: actCode,
                RATE: rate,
                ASTATUS: astatus
            });
        }

        if (duplicateItems.length > 0) {
            var uniqueDups = duplicateItems.filter(function (v, idx, arr) { return arr.indexOf(v) === idx; });
            var shown = uniqueDups.slice(0, 15).join(', ');
            var more = uniqueDups.length > 15 ? ' (+' + (uniqueDups.length - 15) + ' more)' : '';
            empr_helper.notify('Duplicate item(s) in the form: ' + shown + more + '. Total: ' + uniqueDups.length + '.', 2);
            return null;
        }

        if (!groupCode || groupCode == 0) {
            payload.reverse();
        }

        return payload;
    },

    ValidateAndPrepareDataForSave: function () {
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');

        function continueSave() {
            let detailRecords = empr_CustomerPricing.FlattenDetailRecords(gridInstance.option("dataSource"));

            if (!Array.isArray(detailRecords) || detailRecords.length === 0) {
                empr_helper.notify("No detail records to save.", 2);
                return;
            }

            var payload = empr_CustomerPricing.PrepareSavePayload(detailRecords);
            if (payload) {
                empr_CustomerPricing.SaveInfo(payload);
            }
        }

        if (gridInstance.hasEditData()) {
            gridInstance.saveEditData().done(continueSave);
        } else {
            continueSave();
        }
    },

    SaveInfo: function (detailRecords) {
        ajaxHelper.ajaxPostJsonData({ modelRecord: detailRecords }, "/CustomerPricing/Save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgError != null) {
                empr_helper.notify(data.msgError, 2);
            }
            if (data.msgType == 1) {
                if ($("#Code").val() == 0
                    || $("#Code").val() == null
                    || $("#Code").val() == undefined
                    || $("#Code").val() == "") {
                    $('#Code').val(data.data.code);
                    empr_helper.selectedBill = data.data.code;
                }
                if (dataClear == 1) {
                    empr_CustomerPricing.GetCommisionMapByCode($('#Code').val());
                    if (Permissions != "Admin") {
                        if (Permissions.r_DLT) {
                            $('#BtnDelete').show();
                        }
                    } else {
                        $('#BtnDelete').show();
                    }
                }
                else {
                    empr_CustomerPricing.ResetForm();
                    $('#PARTY_CODE').dxSelectBox('instance').option('value', null);
                }
            }
        }, false, true);
    },

    GetCommisionMapByCode: function (code) {
        $("#Loader").show();
        $("#Loader").css('display', 'flex');
        ajaxHelper.ajaxGetJson('/CustomerPricing/GetCommisionMapByCode?code=' + code, function (data) {
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                if (masterData.length == 1) {
                    var response = masterData[0];
                    $('#Code').val(response.grouP_CODE);
                    $('#ACT_CODE').val(response.acT_CODE);
                    $('#HIDDEN_PARTY_CODE').val(response.party);
                    $('#HIDDEN_ACCOUNT_CODE').val(response.acT_CODE);
                    empr_CustomerPricing.act_code = response.acT_CODE;

                    $('#PARTY_CODE').dxSelectBox('instance').option('value', response.partY_KEY);
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);

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
                    empr_CustomerPricing.CreateGrid(data.detail.data);
                    $('.card-body').addClass('customHighlightForModifiedCells');
                    $("#Loader").hide();
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                    $("#Loader").hide();
                }
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
                $("#Loader").hide();
            }
        }, false, true);
    },

    GetCommisionMapDetailsByCode: function (code) {
        ajaxHelper.ajaxGetJson('/CustomerPricing/GetCommisionMapDetailsByCode?code=' + code, function (data) {
            if (data.msgType == 1) {
                empr_CustomerPricing.CreateGrid(data.data);
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/CustomerPricing/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_CustomerPricing.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },
}
