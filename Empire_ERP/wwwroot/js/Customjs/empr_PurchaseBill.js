$(document).keydown(function (e) {
    //Check Ctrl key is pressed and S key is pressed Saveempr_PurchaseBill
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        if (empr_PurchaseBill.ValidateMainInfo()) {
            empr_PurchaseBill.Save();
        }
        return false;
    }
    //Check Ctrl key is pressed and D key is pressed Delete
    if ((e.ctrlKey || e.metaKey) && e.key === 'd') {
        e.preventDefault();
        if ($("#Code").val() != '') {
            empr_PurchaseBill.Delete();
        } else {
            empr_helper.notify("Please select any record for delete..", 2);
        }
        return false;
    }
    //Check Alt key is pressed and R key is pressed Refresh
    if ((e.altKey || e.metaKey) && e.key === 'a') {
        e.preventDefault();
        $("#SCODE").dxSelectBox("instance")?.option("value", "");
        empr_PurchaseBill.ResetForm();
        return false;
    }
    //Check Ctrl key is pressed and f key is pressed Show Modal
    if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault();
        $('.card .modal').modal('show');
        return false;
    }
    //var saveDone = false;  

    if (e.key === 'F3') {
        ////debugger;
        e.preventDefault();
        $("#BARCODE").focus();
    }

    if (e.key === 'Enter') {
        e.preventDefault();
        return false;
    }
});
var empr_PurchaseBill = {
    totalCount: 0,
    formName: typeForm,
    rowsCount: 0,
    pickIds: [],
    CurrentStock: [],
    originalValues: {},
    ItemsOnParty: [],
    firstClick: 0,
    CommissionTranId: 0,
    isProcessingRate: false,
    isSalesman: false,
    vDate: '',
    vdate: '',
    // BtnSodaPick
    InitEvents: function () {
        $(document).ready(function () {

            console.log("Items", Items);
            empr_PurchaseBill.InitQuickSearchGrid();
            empr_PurchaseBill.GetCurrentStock();
            empr_PurchaseBill.ResetForm();
            empr_PurchaseBill.InitPartyType();
            empr_PurchaseBill.InitCashAccount();
            empr_PurchaseBill.InitBankAccount();
            empr_PurchaseBill.InitItemIds();
            empr_PurchaseBill.InitReportTypeDDL();


            var Id = 0;
            empr_PurchaseBill.InitSalesman(Id);
            window.addEventListener('message', function (event) {
                ////debugger;
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    empr_helper.selectedBill = data.traN_ID;
                    $('#Code').val(data.traN_ID);
                    console.log("tranid is ", data.traN_ID)
                    empr_PurchaseBill.GetPurchaseBillByCode(data.traN_ID);
                }
            });
            //window.addEventListener("message", function (event) {
            //    if (event.origin !== window.location.origin) return;

            //    var data = event.data;

            //    // Page Ready Check
            //    if (data === "REQUEST_ID") {
            //        // Sender se ID mangwana
            //        window.opener.postMessage("SEND_ID", window.location.origin);
            //        return;
            //    }


            //    if (data && data.traN_ID) {
            //        $('#Code').val(data.traN_ID);
            //        empr_PurchaseBill.GetPurchaseBillByCode(data.traN_ID);
            //    }
            //});
            $('body').on('click', '#GridSearch', function () {
                if (empr_PurchaseBill.ValidateBarcode()) {
                    $('#BarcodePickModal').modal('show');
                    empr_PurchaseBill.InitBarcodePickGrid();
                    //$('#BarcodePickModal').modal('show');


                }
            });

            $('body').on('change', '#DISC_RATE, #DISC', function () {
                var changedId = $(this).attr('id'); 

                empr_PurchaseBill.SetDiscount(changedId);
            });

            $('body').on('click', '.elm_print', function () {
                ////debugger;
                empr_helper.selectedBill = $(this).attr("reportid");
                //var date = $(this).attr("reportdate");
                empr_PurchaseBill.vdate = $(this).attr("reportdate");
                empr_PurchaseBill.partyCode = $(this).attr("partyCode");
                empr_PurchaseBill.actCode = $(this).attr("actCode");
                empr_PurchaseBill.GeneratePrintReport();
            });

            //$('body').on('click', '#pickItems', function () {
            //    ////debugger;
            //   if (empr_PurchaseBill.ValidateBarcode()) {
            //        $('.bs-example-modal-lg').on('shown.bs.modal', function () {
            //            ////debugger;
            //            $(this).on('keydown', function (e) {
            //                if (e.key === "Enter") {
            //                    e.preventDefault();
            //                    $('#BtnGetItems').click();
            //                }
            //            });
            //        });

            //    }
            //});

            $('body').on('click', '#pickItems', function () {
                ////debugger;
                if (empr_PurchaseBill.ValidateBarcode()) {
                    $('#pickItemModel').modal('show');
                }
            });
            $('#pickItemModel').on('shown.bs.modal', function () {
                ////debugger;

                $('#IQty').focus();
                $(this).off('keydown').on('keydown', function (e) {
                    if (e.key === "Enter") {
                        if (!$(e.target).is('textarea')) {
                            e.preventDefault();
                            $('#BtnGetItems').click();
                        }
                    }
                });
            });

            $('.chkCell').prop('checked', true);

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.modal').modal('hide');
                empr_helper.selectedBill = id;
                empr_PurchaseBill.GetPurchaseBillByCode(id);
            });

            $('body').on('click', '.elm_copy', function () {
                var id = $(this).attr("reportid");
                var date = $(this).attr("reportdate");
                ////debugger;
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
                    $('#updatedDate').val(date);
                    empr_helper.selectedBill = id;
                    $('#CopyViewModal').modal('show');
                });
            });

            $('body').on('click', '#saveCopiedRecord', function () {
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/PurchaseBill/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_PurchaseBill.GetPurchaseBillByCode(data.data.code);
                    }
                }, false, true);
            });

            $('body').on('click', '#BtnSave', function (e) {
                e.preventDefault();

                if ($('#BtnSave').prop('disabled')) {
                    return false;
                }

                $('#BtnSave').prop('disabled', true).hide();

                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                        $('#BtnSave').prop('disabled', false).show();
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                        $('#BtnSave').prop('disabled', false).show();
                    } else {
                        processSave();
                    }
                } else {
                    processSave();
                }
            });

            function processSave() {
                $("#Loader").show();
                $("#Loader").css('display', 'flex');

                setTimeout(function () {
                    if (empr_PurchaseBill.ValidateMainInfo()) {
                        empr_PurchaseBill.Save();
                    } else {
                        $("#Loader").hide();
                        $('#BtnSave').prop('disabled', false).show();
                    }

                    setTimeout(function () {
                        $("#Loader").hide();
                    }, 500);

                }, 200);
            }

            $('body').on('click', '#BtnDelete', function () {
                empr_PurchaseBill.Delete();
            });

            $('body').on('click', '#BtnNew', function () {
                ////debugger;
                $("#SCODE").dxSelectBox("instance")?.option("value", "");
                empr_PurchaseBill.ResetForm();
                $('#REF').focus();
            });

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_PurchaseBill.InitQuickSearchGrid();
            });

            $('body').on('click', '#BtnPrint,#BtnGenerateReport', function () {
                empr_PurchaseBill.vdate = $("#V_DATE").val();
                empr_PurchaseBill.GeneratePrintReport();
            });

            $('body').on('click', '#BtnGetItems', function () {

                var itemId = $("#itemIdHidden").val();
                var itemQty = $("#IQty").val();
                var itemDisc = $("#IDISC").val();
                if (itemId != '' && itemQty != '') {
                    if (Permissions != "Admin") {
                        if (!$("#Code").val() && !Permissions.r_ADD) {
                            empr_helper.notify("You are not allowed to add new record !", 2);
                        }
                        else {
                            empr_PurchaseBill.GetPurchaseBillDetailByItem(itemId, itemQty, itemDisc);
                        }
                    } else {
                        empr_PurchaseBill.GetPurchaseBillDetailByItem(itemId, itemQty, itemDisc);
                    }
                } else {
                    empr_helper.notify("Please fill all fields.", 2);
                }
                $('#IDISC').val('');
            });

            $('body').on('click', '#BtnSodaPick', function () {
                empr_PurchaseBill.InitSodaPickGrid();
            });

            $('#COMM').on('input', function () {
                empr_PurchaseBill.CalculateCommition();
            });

            $('#COMM_VAL').on('input', function () {
                empr_PurchaseBill.CalculateCommition();
            });

            $('body').on('click', '#BtnAddBarcodes', function () {
                var selectedBarcodes = $('#BarcodePickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                if (selectedBarcodes.length > 0) {
                    empr_PurchaseBill.AddBarcodeToGrid();
                }
                else {
                    empr_helper.notify("Please select the items first.", 2);
                }
            });

            $('body').on('click', '#BtnAddSodaToDelivery', function () {
                var selectedSodas = $('#SodaPickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                if (selectedSodas.length > 0) {
                    console.log(selectedSodas)
                    var id = selectedSodas[0].id;
                    var disc = selectedSodas[0].disc;
                    //$('#DISC').val(disc);
                    $('.modal').modal('hide');
                    empr_PurchaseBill.GetPurchaseBillPickDetailByCode(id);
                }
                else {
                    empr_helper.notify("Please select the bill first.", 2);
                }
            });

            $('body').on('click', '#BtnAddSodaDetailToDelivery', function () {
                var selectedSodas = $('#SodaPickDetailGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                if (selectedSodas.length > 0) {
                    console.log('picked');
                    empr_PurchaseBill.AddSodaToDelivery();
                }
                else {
                    empr_helper.notify("Please select the items first.", 2);
                }
            });

            $('.bs-example-modal-lg').on('hidden.bs.modal', function () {
                $("#itemIdHidden").val("");
                $("#IQty").val("");
                $('#PICK_ITEM').dxSelectBox('instance').option('value', '');
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                !Permissions.r_PRINT && $('.btn-print').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }


            $("#BARCODE").on("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    e.stopImmediatePropagation();

                    if (empr_PurchaseBill.ValidateBarcode()) {
                        empr_PurchaseBill.processBarcode();
                    }
                }
            });

            $("#barcodeBtn").on("click", function () {

                empr_PurchaseBill.processBarcode();
            });
        });
    },

    SetDiscount: function (changedId) {
        debugger;
        var DISC_RATE = parseFloat($('#DISC_RATE').val()) || 0;
        var DISC = parseFloat($('#DISC').val()) || 0;
        var gridAmtSum = 0;

        var grid = $('#DetailContainer').dxDataGrid('instance');
        grid.getVisibleRows().forEach(function (row) {
            gridAmtSum += parseFloat(grid.cellValue(row.rowIndex, 'amt')) || 0;
        });


        if (changedId === 'DISC_RATE') {
            var calculatedDisc = (gridAmtSum * DISC_RATE) / 100;
            $('#DISC').val(calculatedDisc.toFixed(2));
            var netAmt = gridAmtSum - calculatedDisc;
            $('#NetAmount').val(netAmt.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        } else if (changedId === 'DISC') {
            var calculatedRate = (DISC / gridAmtSum) * 100;
            $('#DISC_RATE').val(calculatedRate.toFixed(2));
            var netAmt = gridAmtSum - DISC;
            $('#NetAmount').val(netAmt.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        } else if (changedId === 'GRID') {
            var calculatedDisc = (gridAmtSum * DISC_RATE) / 100;
            $('#DISC').val(calculatedDisc.toFixed(2));
            var netAmt = gridAmtSum - calculatedDisc;
            $('#NetAmount').val(netAmt.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        }
    },

    processBarcode: async function () { 
        debugger;
        var inputVal = $("#BARCODE").val().trim();
        if (inputVal === "") return;
        var matches = empr_PurchaseBill.ItemsOnParty.filter(u => u.barcode == inputVal || u.barcode.endsWith(inputVal));

        if (matches.length > 0) {
            let selectedItem = matches[0];
            debugger;
            var stockRecord = empr_PurchaseBill.CurrentStock.find(s => s.itemId == selectedItem.key);

            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            let dataSource = gridInstance.option("dataSource");

            var existingItem = dataSource.find(x => x.iteM_CODE === selectedItem.key);

            if (existingItem) {
                existingItem.qty = (parseFloat(existingItem.qty) || 0) + 1;
                existingItem.tax = (parseFloat(existingItem.tax) || 0);
                //existingItem.stock = (parseFloat(stockRecord.balance) || 0);
                existingItem.stock = stockRecord ? (parseFloat(stockRecord.balance) || 0) : 0;
                existingItem.taX_AMT = (parseFloat(existingItem.taX_AMT) || 0);
                existingItem.amt = (parseFloat(existingItem.qty) || 0) * (parseFloat(existingItem.rate) || 0);
                existingItem.neT_AMT = (parseFloat(existingItem.qty) || 0) * (parseFloat(existingItem.rate) || 0);
            } else {
                let rowToFill;
                if (dataSource.length > 0 && (!dataSource[0].iteM_CODE || dataSource[0].iteM_CODE === 0)) {
                    rowToFill = dataSource[0];
                } else {
                    empr_PurchaseBill.rowsCount += 1;
                    rowToFill = { __KEY__: empr_PurchaseBill.GenerateKey(36), dT_CODE: 0 };
                    dataSource.unshift(rowToFill);
                }
                await this.populateRowData(rowToFill, selectedItem.key);

                rowToFill.qty = 1;
                //rowToFill.stock = (parseFloat(stockRecord.balance) || 0);
                rowToFill.stock = stockRecord ? (parseFloat(stockRecord.balance) || 0) : 0;
                rowToFill.amt = rowToFill.qty * rowToFill.rate;
                rowToFill.neT_AMT = rowToFill.qty * rowToFill.rate;
                rowToFill.baL_QTY = 1;
            }
            const rowIndex = dataSource.findIndex(item => item.iteM_CODE === selectedItem.key);
            if (rowIndex !== -1) {
                //setTimeout(() => {
                //    gridInstance.editCell(rowIndex, 4);
                //}, 150);
            }

            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();

            setTimeout(() => { $("#BARCODE").val(""); }, 150);
            //setTimeout(() => { $("#BARCODE").focus(); }, 100);
        }
    },
    //processBarcode: function () {
    //    var inputVal = $("#BARCODE").val().trim();
    //    if (inputVal === "") return;
    //    var matches = Items.filter(u =>
    //        u.value == inputVal || u.value.endsWith(inputVal)
    //    );
    //    if (matches.length > 0) {
    //        let selectedItem;
    //        if (/^\d+$/.test(matches[0].value)) {
    //            if (matches.length > 1) matches.sort((a, b) => parseInt(a.value) - parseInt(b.value));
    //            selectedItem = matches[0];
    //        } else {
    //            if (matches.length === 1) {
    //                selectedItem = matches[0];
    //            } else {
    //                empr_helper.notify("Multiple matches, please type full barcode", 2);
    //                return;
    //            }
    //        }
    //        var grid = $("#DetailContainer").dxDataGrid("instance");
    //        var rowCount = grid.totalCount();
    //        var firstRowData = rowCount > 0 ? grid.getVisibleRows()[0].data : null;
    //        if (rowCount === 0 || !firstRowData.iteM_CODE) {
    //            grid.cellValue(0, "iteM_CODE", selectedItem.key);
    //            grid.cellValue(0, "qty", 1);
    //            grid.saveEditData(); 
    //        } else {
    //            grid.addRow().done(function () {
    //                grid.cellValue(0, "iteM_CODE", selectedItem.key);
    //                grid.cellValue(0, "qty", 1);
    //                grid.saveEditData();
    //            });
    //        }
    //        setTimeout(() => { $("#BARCODE").val("").focus(); }, 100);
    //    } else {
    //        empr_helper.notify("Item not found", 2);
    //        setTimeout(() => { $("#BARCODE").focus(); }, 100);
    //    }
    //},
    //processBarcode: function () {
    //    var inputVal = $("#BARCODE").val().trim();
    //    if (inputVal === "") return;
    //    ////debugger;
    //    var matches = Items.filter(u =>
    //        u.value == inputVal || u.value.endsWith(inputVal)
    //    );

    //    if (matches.length > 0) {
    //        let selectedItem;

    //        // Pure numeric case
    //        if (/^\d+$/.test(matches[0].value)) {
    //            if (matches.length > 1) {
    //                matches.sort((a, b) => parseInt(a.value) - parseInt(b.value));
    //            }
    //            selectedItem = matches[0];
    //        } else {
    //            // Prefix case
    //            if (matches.length === 1) {
    //                selectedItem = matches[0];
    //            } else {
    //                empr_helper.notify("Multiple matches, please type full barcode", 2);
    //                setTimeout(() => { $("#BARCODE").focus(); }, 100);
    //                return;
    //            }
    //        }

    //        // Row add & values set
    //        empr_PurchaseBill.AddRow();
    //        var grid = $("#DetailContainer").dxDataGrid("instance");
    //        var rowIndex = 0;

    //        grid.cellValue(rowIndex, "iteM_CODE", selectedItem.key);
    //        grid.cellValue(rowIndex, "qty", 1);

    //        setTimeout(() => { $("#BARCODE").val("").focus(); }, 100);

    //    } else {
    //        empr_helper.notify("Item not found", 2);
    //        setTimeout(() => { $("#BARCODE").focus(); }, 100);
    //    }
    //},
    InitSodaPickGrid: function () {
        var PARTY_CODE = $("#partyhidden").val();
        var ACT_CODE = $("#acthidden").val();
        console.log(PARTY_CODE);
        console.log(empr_PurchaseBill.formName);
        //if (PARTY_CODE == "" || PARTY_CODE == null || PARTY_CODE == undefined || PARTY_CODE == 0 && empr_PurchaseBill.formName != 'MP') {
        //    empr_helper.notify("Please select party first.", 2);
        //}
        //else {
        //    empr_PurchaseBill.GetPickDataByParty(PARTY_CODE, ACT_CODE);
        //}
        ////debugger;
        if ((PARTY_CODE == "" || PARTY_CODE == null || PARTY_CODE == undefined || PARTY_CODE == 0) && empr_PurchaseBill.formName != 'MPO') {
            empr_helper.notify("Please select party first.", 2);
        } else {
            empr_PurchaseBill.GetPickDataByParty(PARTY_CODE, ACT_CODE);
        }

    },
    GetPickDataByParty: function (PARTY_CODE, ACT_CODE) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPickDataByParty?partyCode=' + PARTY_CODE + '&actCode=' + ACT_CODE, function (data) {
            console.log(data)
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    if ($('#SodaPickGridContainer').data('dxDataGrid') != undefined) {
                        $('#SodaPickGridContainer').data('dxDataGrid').dispose();
                    }
                    if (empr_PurchaseBill.formName.includes('O')) {
                        empr_PurchaseBill.CreatePickDetailGrid(data.data);
                        $('#SodaPickDetailModal').modal('show');
                    }
                    else {
                        empr_PurchaseBill.CreatePickGrid(data.data);
                        $('#SodaPickModal').modal('show');
                    }


                } else {
                    empr_helper.notify("No soda found.", 2);
                }
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    // modal open
    InitBarcodePickGrid: function () {
        if (empr_PurchaseBill.pickIds.length > 0) {
            empr_helper.notify("Cannot open on return data.", 2);
        } else {
            ajaxHelper.ajaxGetJson('/PurchaseBill/GetBarcodeList', function (data) {
                if (data.msgType == 1) {
                    if (data.data.length > 0) {
                        //if ($('#BarcodePickGridContainer').data('dxDataGrid') != undefined) {
                        //    $('#BarcodePickGridContainer').data('dxDataGrid').dispose();
                        //}

                        var updatedDetailData = empr_PurchaseBill.ItemsOnParty.map(item => {
                            var stockRecord = empr_PurchaseBill.CurrentStock.find(s => s.itemId == item.iteM_CODE);

                            return {
                                ...item,
                                stock: stockRecord ? stockRecord.balance : 0
                            };
                        });


                        empr_PurchaseBill.CreateBarcodeGrid(updatedDetailData);
                        $('#BarcodePickModal').modal('show');
                        //$('#BarcodePickModal').css('display', 'block');
                    } else {
                        empr_helper.notify("No barcodes found.", 2);
                    }
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                }
            }, false, true);
        }
    },
    CreateBarcodeGrid: function (dataSrc) {
        var col = [
            { dataField: 'barcodE_CODE_CODE', caption: 'Barcode Code', visible: false, },
            { dataField: 'iteM_CODE', caption: 'Item Code', visible: false, },
            { dataField: 'amt', caption: 'Amount', visible: false, allowEditing: false },
            { dataField: 'neT_AMT', caption: 'Net Amount', visible: false, allowEditing: false },
            { dataField: 'color', caption: 'Color', visible: false, allowEditing: false },
            { dataField: 'size', caption: 'Size', visible: false, allowEditing: false },
            { dataField: 'value', caption: 'Item', visible: true },
            { dataField: 'barcode', caption: 'Barcode', allowEditing: false },
            { dataField: 'sizE_NAME', caption: 'Size', allowEditing: false, width: 120 },
            { dataField: 'coloR_NAME', caption: 'Color', allowEditing: false, width: 120 },
            { dataField: 'rate', caption: 'Rate', allowEditing: false, width: 120 },
        ];
        empr_helper.dxGridbindingVouchers('#BarcodePickGridContainer', col, dataSrc, "PurchaseBillBarcodes");
        //setTimeout(function () {
        //    $('#BarcodePickGridContainer').dxDataGrid('instance').resize();
        //}, 500);
    },
    // modal close
    //AddBarcodeToGrid: function () {
    //    const gridInstance = $('#DetailContainer').dxDataGrid('instance');
    //    var dataSource = gridInstance.option("dataSource");
    //    $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
    //        dataSource = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
    //    });
    //    var IsDataAvailableInGrid = false;
    //    $.each(dataSource, function (index, item) {
    //        if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
    //            IsDataAvailableInGrid = true;
    //        }
    //    });

    //    if (IsDataAvailableInGrid) {
    //        var selectedBarcodes = $('#BarcodePickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //        selectedBarcodes = empr_PurchaseBill.SetData(selectedBarcodes);
    //        console.log(selectedBarcodes);
    //        dataSource.unshift(...selectedBarcodes);
    //        gridInstance.option("dataSource", dataSource);
    //        gridInstance.refresh();
    //        empr_helper.MoveFocusToGridWithouTab('#DetailContainer', 0, 'iteM_CODE')
    //    }
    //    else {
    //        var selectedBarcodes = $('#BarcodePickGridContainer').dxDataGrid('instance').getSelectedRowKeys();
    //        selectedBarcodes = empr_PurchaseBill.SetData(selectedBarcodes);
    //        console.log(selectedBarcodes);
    //        $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedBarcodes);
    //    }

    //    $('.modal').hide();
    //    $('#V_DATE').focus();
    //},
    AddBarcodeToGrid: function () {
        ////debugger;
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");

        gridInstance.saveEditData().done(function () {

            dataSource = gridInstance.option("dataSource");
            var IsDataAvailableInGrid = false;

            $.each(dataSource, function (index, item) {
                if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
                    IsDataAvailableInGrid = true;
                    return false;
                }
            });
            //$('#BarcodePickModal').modal('show');
            var barcodeGrid = $('#BarcodePickGridContainer').dxDataGrid('instance');
            if (!barcodeGrid) {
                return;
            }

            var selectedBarcodes = barcodeGrid.getSelectedRowKeys();
            selectedBarcodes = empr_PurchaseBill.SetData(selectedBarcodes);

            if (IsDataAvailableInGrid) {
                dataSource.unshift(...selectedBarcodes);
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
                //empr_helper.MoveFocusToGridWithouTab('#StockAdjustmentDetailContainer', 0, 'iteM_CODE');
            } else {
                gridInstance.option('dataSource', selectedBarcodes);
            }

            try {
                barcodeGrid.dispose();
            } catch (ex) {
            }

            $('#BarcodePickModal').modal('hide');

            //$('#V_DATE').focus();
        });
    },
    SetData: function (dataSource) {
        $.each(dataSource, function (index, item) {
            if (Type != "I") {
                item.iteM_CODE = parseInt(item.barcodE_CODE);
            }
            item.dT_CODE = 0;
            item.__KEY__ = empr_PurchaseBill.GenerateKey(36);
        });

        return dataSource;
    },
    CreatePickGrid: function (dataSrc) {
        var col = [
            //{
            //    dataField: "Action",
            //    width: 100,
            //    alignment: 'center',
            //    fixed: true,
            //    fixedPosition: "left",
            //    allowExporting: false,
            //    cellTemplate: function (container, options) {


            //        $(`<div class="btn-group btn-group-sm">
            //               <a href="javascript:;"  class="grid-action-icon elm_edit_pick" reportid=${options.data.id} disc=${options.data.disc} title="Edit"><i class="fa fa-edit"></i></a>
            //               </div>`).appendTo(container);
            //    }
            //},
            { dataField: 'id', caption: 'Code', visible: true, },
            { dataField: 'lB_DATE', caption: 'Date', dataType: 'date', allowEditing: false, format: 'dd-MM-yyy' },
            { dataField: 'voucheR_NO', caption: 'Voucher No', allowEditing: false, },
            { dataField: 'partY_NAME', caption: 'Seller', allowEditing: false, },
            { dataField: 'ref', caption: 'Ref', allowEditing: false, },
            { dataField: 'qty', caption: 'Quantity', allowEditing: false, },
            { dataField: 'amt', caption: 'Amount', allowEditing: false, },
            { dataField: 'disc', caption: 'Discount', allowEditing: false, },
        ];
        empr_helper.dxGridbindingVouchers('#SodaPickGridContainer', col, dataSrc, "PurchaseBillPick", "single");
        setTimeout(function () {
            $('#SodaPickGridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    CreatePickDetailGrid: function (dataSrc) {
        console.log("PICK GRID", dataSrc);
        var col = [];
        var updatedData = [];

        if (empr_PurchaseBill.formName == 'O') {
            existingdata = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
            updatedData = dataSrc
                .map(item => {
                    const matchingRows = existingdata.filter(row => row.picK_ID_D === item.picK_ID_D);
                    const totalQty = matchingRows.reduce((sum, row) => parseInt(sum) + parseInt(row.qty), 0);
                    //console.log(totalQty);
                    item.qty -= totalQty;
                    //console.log(item.qty);

                    return item.qty > 0 ? item : null;
                })
                .filter(item => item !== null);
            empr_PurchaseBill.originalValues = {};
            updatedData.forEach(row => {
                empr_PurchaseBill.originalValues[row.picK_ID_D] = row.qty;
            });

            col = [
                /*{ dataField: 'id', caption: 'Transaction#', visible: true, width: 80,},*/
                { dataField: 'lB_DATE', caption: 'Date', visible: true, },
                { dataField: 'voucheR_NO', caption: 'Voucher No', visible: true, },
                { dataField: 'partY_NAME', caption: 'Party Name', visible: true, width: 150, },
                /*{ dataField: 'acT_CODE', caption: 'ACT Code', visible: false, },*/
                { dataField: 'ref', caption: 'Ref', visible: true, },
                { dataField: 'currency', caption: 'Currency', visible: true, },
                { dataField: 'crate', caption: 'C.Rate', visible: true, },
                { dataField: 'comM_AMT', caption: 'Com Type', visible: true, },
                { dataField: 'comm', caption: 'Com', visible: true, },
                { dataField: 'iteM_NAME', caption: 'Item', visible: true, width: 200, },
                {
                    dataField: 'qty',
                    caption: 'Quantity',
                    allowSorting: false,
                    allowFiltering: false,
                    dataType: 'number',
                    width: 90,
                    setCellValue: function (newData, value, currentRowData) {
                        const originalQty = empr_PurchaseBill.originalValues[currentRowData.picK_ID_D];
                        if (value <= originalQty) {
                            newData.qty = value;
                        } else {
                            newData.qty = originalQty;
                        }
                        var rate = parseFloat(currentRowData.rate) || 0;
                        var qty = parseFloat(newData.qty) || 0;
                        var qtY2 = parseFloat(currentRowData.qtY2) || 0;
                        if (isNaN(qty)) {
                            empr_helper.notify("Please enter the correct quantity.", 2);
                        }
                        if (isNaN(qtY2)) {
                            empr_helper.notify("Please enter the correct quantity2.", 2);
                        }
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (currentRowData.chK1) {
                                newData.baL_QTY = qty + qtY2;
                            }
                            else {
                                newData.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                newData.amt = (newData.baL_QTY * rate).toFixed(2);

                                var Amount = newData.amt;
                                var Dis = parseFloat(currentRowData.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(currentRowData.adv) || 0;
                                var Tax = parseFloat(currentRowData.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                newData.disC_AMT = disSum.toFixed(2);
                                newData.taX_AMT = TaxSum.toFixed(2);
                                newData.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    newData.neT_AMT = 0;
                                }
                            }
                        }
                    },
                    calculateCellValue: function (data) {
                        const originalQty = empr_PurchaseBill.originalValues[data.picK_ID_D];
                        if (data.qty > originalQty) {
                            data.qty = originalQty;
                        }
                        var rate = parseFloat(data.rate) || 0;
                        var qty = parseFloat(data.qty) || 0;
                        var qtY2 = parseFloat(data.qtY2) || 0;
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (data.chK1) {
                                data.baL_QTY = qty + qtY2;
                            }
                            else {
                                data.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                data.amt = (data.baL_QTY * rate).toFixed(2);

                                var Amount = data.amt;
                                var Dis = parseFloat(data.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(data.adv) || 0;
                                var Tax = parseFloat(data.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                data.disC_AMT = disSum.toFixed(2);
                                data.taX_AMT = TaxSum.toFixed(2);
                                data.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    data.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    data.neT_AMT = 0;
                                }
                            }
                        }
                        return data.qty;
                    }
                },
                { dataField: 'unit', caption: 'Unit', visible: true, },
                { dataField: 'rate', caption: 'Rate', dataType: 'number', width: 90, },
                { dataField: 'amt', caption: 'Amt', visible: true, },
                { dataField: 'disc', caption: 'Disc %', visible: true, },
                { dataField: 'disC_AMT', caption: 'Disc Amt', visible: true, },
                { dataField: 'tax', caption: 'Tax %', visible: true, },
                { dataField: 'taX_AMT', caption: 'Tax Amt', visible: true, },
                { dataField: 'neT_AMT', caption: 'Net Amt', visible: true, },
                { dataField: 'coloR_NAME', caption: 'Color', visible: true, },
                { dataField: 'sizE_NAME', caption: 'Size', visible: true, },
                { dataField: 'gradE_NAME', caption: 'Grade', visible: true, },
                { dataField: 'deL_DATE', caption: 'Del Date', visible: true, },
                { dataField: 'dT_CODE', caption: 'Net Amt', visible: false, },
                { dataField: 'partY_CODE', caption: 'Party Code', visible: false, },
                { dataField: 'partY_DDL', visible: false, },

            ];

            updatedData = dataSrc;
        }
        else if (empr_PurchaseBill.formName == 'MPO') {
            existingdata = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
            updatedData = dataSrc
                .map(item => {
                    const matchingRows = existingdata.filter(row => row.picK_ID_D === item.picK_ID_D);
                    const totalQty = matchingRows.reduce((sum, row) => parseInt(sum) + parseInt(row.qty), 0);
                    //console.log(totalQty);
                    item.qty -= totalQty;
                    //console.log(item.qty);

                    return item.qty > 0 ? item : null;
                })
                .filter(item => item !== null);
            empr_PurchaseBill.originalValues = {};
            updatedData.forEach(row => {
                empr_PurchaseBill.originalValues[row.picK_ID_D] = row.qty;
            });

            col = [
                /*{ dataField: 'id', caption: 'Transaction#', visible: true, width: 80,},*/
                { dataField: 'lB_DATE', caption: 'Date', visible: true, },
                { dataField: 'voucheR_NO', caption: 'Voucher No', visible: true, },
                { dataField: 'partY_NAME', caption: 'Party Name', visible: true, width: 150, },
                /*{ dataField: 'acT_CODE', caption: 'ACT Code', visible: false, },*/
                { dataField: 'ref', caption: 'Ref', visible: true, },
                //{ dataField: 'currency', caption: 'Currency', visible: true, },
                //{ dataField: 'crate', caption: 'C.Rate', visible: true, },
                //{ dataField: 'comM_AMT', caption: 'Com Type', visible: true, },
                //{ dataField: 'comm', caption: 'Com', visible: true, },
                { dataField: 'iteM_NAME', caption: 'Item', visible: true, width: 200, },
                {
                    dataField: 'qty',
                    caption: 'Quantity',
                    allowSorting: false,
                    allowFiltering: false,
                    dataType: 'number',
                    width: 90,
                    setCellValue: function (newData, value, currentRowData) {
                        const originalQty = empr_PurchaseBill.originalValues[currentRowData.picK_ID_D];
                        if (value <= originalQty) {
                            newData.qty = value;
                        } else {
                            newData.qty = originalQty;
                        }
                        var rate = parseFloat(currentRowData.rate) || 0;
                        var qty = parseFloat(newData.qty) || 0;
                        var qtY2 = parseFloat(currentRowData.qtY2) || 0;
                        if (isNaN(qty)) {
                            empr_helper.notify("Please enter the correct quantity.", 2);
                        }
                        if (isNaN(qtY2)) {
                            empr_helper.notify("Please enter the correct quantity2.", 2);
                        }
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (currentRowData.chK1) {
                                newData.baL_QTY = qty + qtY2;
                            }
                            else {
                                newData.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                newData.amt = (newData.baL_QTY * rate).toFixed(2);

                                var Amount = newData.amt;
                                var Dis = parseFloat(currentRowData.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(currentRowData.adv) || 0;
                                var Tax = parseFloat(currentRowData.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                newData.disC_AMT = disSum.toFixed(2);
                                newData.taX_AMT = TaxSum.toFixed(2);
                                newData.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    newData.neT_AMT = 0;
                                }
                            }
                        }
                    },
                    calculateCellValue: function (data) {
                        const originalQty = empr_PurchaseBill.originalValues[data.picK_ID_D];
                        if (data.qty > originalQty) {
                            data.qty = originalQty;
                        }
                        var rate = parseFloat(data.rate) || 0;
                        var qty = parseFloat(data.qty) || 0;
                        var qtY2 = parseFloat(data.qtY2) || 0;
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (data.chK1) {
                                data.baL_QTY = qty + qtY2;
                            }
                            else {
                                data.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                data.amt = (data.baL_QTY * rate).toFixed(2);

                                var Amount = data.amt;
                                var Dis = parseFloat(data.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(data.adv) || 0;
                                var Tax = parseFloat(data.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                data.disC_AMT = disSum.toFixed(2);
                                data.taX_AMT = TaxSum.toFixed(2);
                                data.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    data.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    data.neT_AMT = 0;
                                }
                            }
                        }
                        return data.qty;
                    }
                },
                { dataField: 'unit', caption: 'Unit', visible: true, },
                { dataField: 'rate', caption: 'Rate', dataType: 'number', width: 90, },
                { dataField: 'amt', caption: 'Amt', visible: true, },
                { dataField: 'disc', caption: 'Disc %', visible: true, },
                { dataField: 'disC_AMT', caption: 'Disc Amt', visible: true, },
                { dataField: 'tax', caption: 'Tax %', visible: true, },
                { dataField: 'taX_AMT', caption: 'Tax Amt', visible: true, },
                { dataField: 'neT_AMT', caption: 'Net Amt', visible: true, },
                { dataField: 'coloR_NAME', caption: 'Color', visible: true, },
                { dataField: 'sizE_NAME', caption: 'Size', visible: true, },
                //{ dataField: 'gradE_NAME', caption: 'Grade', visible: true, },
                //{ dataField: 'deL_DATE', caption: 'Del Date', visible: true, },
                { dataField: 'dT_CODE', caption: 'Net Amt', visible: false, },
                { dataField: 'partY_CODE', caption: 'Party Code', visible: false, },
                { dataField: 'partY_DDL', visible: false, },
                { dataField: 'iteM_CODE', visible: false, },
                { dataField: 'comm', visible: false, },
                { dataField: 'comM_AMT', visible: false, },
                { dataField: 'comM_VAL', visible: false, },
            ];

            updatedData = dataSrc;
        }
        else {
            existingdata = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
            updatedData = dataSrc
                .map(item => {
                    const matchingRows = existingdata.filter(row => row.picK_ID_D === item.picK_ID_D);
                    const totalQty = matchingRows.reduce((sum, row) => parseInt(sum) + parseInt(row.qty), 0);
                    console.log(totalQty);
                    item.qty -= totalQty;
                    console.log(item.qty);

                    return item.qty > 0 ? item : null;
                })
                .filter(item => item !== null);
            empr_PurchaseBill.originalValues = {};
            updatedData.forEach(row => {
                empr_PurchaseBill.originalValues[row.picK_ID_D] = row.qty;
            });

            var col = [
                {
                    caption: 'Item', allowEditing: false, width: 140,
                    cellTemplate: function (container, options) {
                        const selectedIds = Items.filter(x => x.key === options.data.iteM_CODE);
                        const customText = selectedIds.length > 0 ? selectedIds[0].itemID : '';
                        container.text(customText);
                    }
                },
                {
                    dataField: 'size', caption: 'Size', allowEditing: false, width: 140,
                    cellTemplate: function (container, options) {
                        const selectedSize = Sizes.filter(x => x.key === options.data.size);
                        const customText = selectedSize.length > 0 ? selectedSize[0].value : '';
                        container.text(customText);
                    }
                },
                {
                    dataField: 'color', caption: 'Color', allowEditing: false, width: 170,
                    cellTemplate: function (container, options) {
                        const selectedColor = Colors.filter(x => x.key === options.data.color);
                        const customText = selectedColor.length > 0 ? selectedColor[0].value : '';
                        container.text(customText);
                    }
                },
                {
                    dataField: 'qty',
                    caption: 'Quantity',
                    allowSorting: false,
                    allowFiltering: false,
                    dataType: 'number',
                    width: 90,
                    setCellValue: function (newData, value, currentRowData) {
                        const originalQty = empr_PurchaseBill.originalValues[currentRowData.picK_ID_D];
                        if (value <= originalQty) {
                            newData.qty = value;
                        } else {
                            newData.qty = originalQty;
                        }
                        var rate = parseFloat(currentRowData.rate) || 0;
                        var qty = parseFloat(newData.qty) || 0;
                        var qtY2 = parseFloat(currentRowData.qtY2) || 0;
                        if (isNaN(qty)) {
                            empr_helper.notify("Please enter the correct quantity.", 2);
                        }
                        if (isNaN(qtY2)) {
                            empr_helper.notify("Please enter the correct quantity2.", 2);
                        }
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (currentRowData.chK1) {
                                newData.baL_QTY = qty + qtY2;
                            }
                            else {
                                newData.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                newData.amt = (newData.baL_QTY * rate).toFixed(2);

                                var Amount = newData.amt;
                                var Dis = parseFloat(currentRowData.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(currentRowData.adv) || 0;
                                var Tax = parseFloat(currentRowData.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                newData.disC_AMT = disSum.toFixed(2);
                                newData.taX_AMT = TaxSum.toFixed(2);
                                newData.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    newData.neT_AMT = 0;
                                }
                            }
                        }
                    },
                    calculateCellValue: function (data) {
                        const originalQty = empr_PurchaseBill.originalValues[data.picK_ID_D];
                        if (data.qty > originalQty) {
                            data.qty = originalQty;
                        }
                        var rate = parseFloat(data.rate) || 0;
                        var qty = parseFloat(data.qty) || 0;
                        var qtY2 = parseFloat(data.qtY2) || 0;
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (data.chK1) {
                                data.baL_QTY = qty + qtY2;
                            }
                            else {
                                data.baL_QTY = qty;
                            }
                            if (!isNaN(rate)) {
                                data.amt = (data.baL_QTY * rate).toFixed(2);

                                var Amount = data.amt;
                                var Dis = parseFloat(data.disc) || 0;
                                var disSum = Amount * Dis / 100 || 0;
                                var Adv = parseFloat(data.adv) || 0;
                                var Tax = parseFloat(data.tax) || 0;
                                var DiscountAmount = disSum;
                                var TaxAmount = Amount - DiscountAmount || 0;

                                var TaxSum = TaxAmount * Tax / 100 || 0;
                                var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                                data.disC_AMT = disSum.toFixed(2);
                                data.taX_AMT = TaxSum.toFixed(2);
                                data.adV_AMT = AdvSum.toFixed(2);

                                var NetAmount = Amount - DiscountAmount || 0;
                                if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                    data.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                                } else {
                                    data.neT_AMT = 0;
                                }
                            }
                        }
                        return data.qty;
                    }
                },
                { dataField: 'rate', caption: 'Rate', allowEditing: false, dataType: 'number', width: 90, },
                {
                    caption: 'Barcode', allowEditing: false,
                    cellTemplate: function (container, options) {
                        const customText = Items.filter(x => x.key === options.data.iteM_CODE)[0].value;
                        container.text(customText);
                    }
                },
                { dataField: 'dT_CODE', visible: false, },
                { dataField: 'iteM_CODE', visible: false, },
                { dataField: 'unit', visible: false, },
                { dataField: 'qtY2', visible: false, },
                { dataField: 'baL_QTY', visible: false, },
                { dataField: 'rate', visible: false, },
                { dataField: 'amt', visible: false, },
                { dataField: 'disc', visible: false, },
                { dataField: 'disC_AMT', visible: false, },
                { dataField: 'tax', visible: false, },
                { dataField: 'taX_AMT', visible: false, },
                { dataField: 'adv', visible: false, },
                { dataField: 'adV_AMT', visible: false, },
                { dataField: 'neT_AMT', caption: 'Net Amt', visible: true, },
                { dataField: 'dT_DESC', visible: false, },
                { dataField: 'color', visible: false, },
                { dataField: 'size', visible: false, },
                { dataField: 'grade', visible: false, },
                { dataField: 'warehouse', visible: false, },
                { dataField: 'deL_DATE', visible: false, },
                { dataField: 'duE_DATE', visible: false, },
                { dataField: 'duE_DAYS', visible: false, },
                { dataField: 'veh', visible: false, },
                { dataField: 'chk', visible: false, },
                { dataField: 'chK1', visible: false, },
                { dataField: 'picK_ID', visible: false, },
                { dataField: 'picK_ID_D', visible: false, },
            ];
        }

        empr_helper.editableDxGridbinding('#SodaPickDetailGridContainer', col, updatedData, "SodaPickDetailGrid");
        setTimeout(function () {
            $('#SodaPickDetailGridContainer').dxDataGrid('instance').refresh();
            $('#SodaPickDetailGridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    CreateGrid: function (dataSrc) {
        console.log('CreateGrid', dataSrc);
        console.log("Items Loaded:", Items);

        dataSrc.forEach(item => {
            if (
                item.deL_DATE == '1900-01-01' || item.deL_DATE == '01-01-1900' || item.deL_DATE == '01-Jan-1900' || item.deL_DATE == '1/1/1900 12:00:00 AM' || item.deL_DATE == '01/01/1900 12:00:00 AM' || item.deL_DATE == '1/1/1900' ||
                item.deL_DATE == '2000-01-01' || item.deL_DATE == '01-01-2000' || item.deL_DATE == '01-Jan-2000' || item.deL_DATE == '1/1/2000 12:00:00 AM' || item.deL_DATE == '01/01/2000 12:00:00 AM' || item.deL_DATE == '1/1/2000' ||
                item.deL_DATE == '00-01-01' || item.deL_DATE == '01-01-00' || item.deL_DATE == '01-Jan-00' || item.deL_DATE == '1/1/00 12:00:00 AM' || item.deL_DATE == '01/01/00 12:00:00 AM' || item.deL_DATE == '1/1/00'
            ) {
                item.deL_DATE = undefined;
            }

            if (
                item.duE_DATE == '1900-01-01' || item.duE_DATE == '01-01-1900' || item.duE_DATE == '01-Jan-1900' || item.duE_DATE == '1/1/1900 12:00:00 AM' || item.duE_DATE == '01/01/1900 12:00:00 AM' || item.duE_DATE == '1/1/1900' ||
                item.duE_DATE == '2000-01-01' || item.duE_DATE == '01-01-2000' || item.duE_DATE == '01-Jan-2000' || item.duE_DATE == '1/1/2000 12:00:00 AM' || item.duE_DATE == '01/01/2000 12:00:00 AM' || item.duE_DATE == '1/1/2000' ||
                item.duE_DATE == '00-01-01' || item.duE_DATE == '01-01-00' || item.duE_DATE == '01-Jan-00' || item.duE_DATE == '1/1/00 12:00:00 AM' || item.duE_DATE == '01/01/00 12:00:00 AM' || item.duE_DATE == '1/1/00'
            ) {
                item.duE_DATE = undefined;
            }
            console.log(item.picK_ID)
            if (item.picK_ID_D > 0) {
                empr_PurchaseBill.pickIds.push(item.picK_ID_D);
                $('#pickItems').hide();
            }
        });
        if (dataSrc.length > 0) {
            empr_PurchaseBill.rowsCount = dataSrc.length - 1;
        }
        var Caption = "";
        if (Type == "I") {
            Caption = "Item";
        }
        else {
            Caption = "Bar Code";
        }

        var data_source;
        if (Items != null && Items.length > 0) {
            data_source = Items;
        }
        var col = [
            {
                dataField: "Action",
                width: 140,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                allowEditing: false,
                cellTemplate: function (container, options) {


                    const hasImage = options.data && options.data.doc;
                    const initialEyeIcon = !hasImage
                        ? ''
                        : `<a href="javascript:;" class="grid-action-icon ViewImage" style="margin-left: 8px" onclick="ShowImage('${options.data.doc}')" title="View Pic"><i class="fa fa-eye"></i></a>`;

                    // Class lagayi hai 'eye-container-marker' aur saath me runtime unique identifier data attribute: data-row="${options.rowIndex}"
                    const eyePlaceholder = `<span class="eye-container-marker" data-row="${options.rowIndex}">${initialEyeIcon}</span>`;

                    if (Permissions != "Admin") {
                        const copyAction = !Permissions.r_COPY ? '' : `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PurchaseBill.CloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT) ? '' : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PurchaseBill.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        //const deleteAction = !Permissions.r_DLT ? '' : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseBill.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const deleteAction = `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseBill.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const searchAction = (!Permissions.r_ADD && !Permissions.r_EDIT) ? '' : `<a href="javascript:;" class="grid-action-icon Search" style="margin-left: 8px" id="GridSearch" title="Search"><i class="fa fa-search"></i></a>`;

                        const actions = `<div class="btn-group btn-group-sm">${copyAction}${addAction}${deleteAction}${searchAction}${eyePlaceholder}</div>`;
                        $(actions).appendTo(container);
                    }
                    else {
                        $(`<div class="btn-group btn-group-sm">
               <a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PurchaseBill.CloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>
               <a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PurchaseBill.AddRow()" title="Add"><i class="fa fa-add"></i></a>
               <a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseBill.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>
               <a href="javascript:;" class="grid-action-icon Search" style="margin-left: 8px" id="GridSearch" title="Search"><i class="fa fa-search"></i></a>
               ${eyePlaceholder}
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
                dataField: 'partY_CODE',
                visible: false
            },
            {
                dataField: 'acT_CODE',
                visible: false
            },
            {
                dataField: 'iteM_CODE',
                caption: Caption,
                width: Caption == 'Item' ? 380 : 150,
                allowSorting: false,
                fixed: true,
                fixedPosition: "left",
                lookup: {
                    dataSource: data_source,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    showClearButton: true,
                    paging: { enabled: true, pageSize: 50 }
                },
                setCellValue: async function (newData, value, currentRowData) {
                    await empr_PurchaseBill.populateRowData(newData, value, currentRowData);

                    // Stock Logic
                    var stockRecord = empr_PurchaseBill.CurrentStock.find(s => s.itemId == value);
                    if (stockRecord) {
                        newData.stock = stockRecord.balance;
                    } else {
                        newData.stock = 0;
                    }
                    debugger;

                    var selectedItem = empr_PurchaseBill.ItemsOnParty.find(i => i.key == value);

                    setTimeout(function () {
                        var $eyeContainer = $('.dx-edit-row .eye-container-marker');

                        if (selectedItem && selectedItem.image) {
                            newData.doc = selectedItem.image; 

                            var eyeHtml = `<a href="javascript:;" class="grid-action-icon ViewImage" style="margin-left: 8px" onclick="ShowImage('${selectedItem.image}')" title="View Pic"><i class="fa fa-eye"></i></a>`;

                            $eyeContainer.html(eyeHtml);
                        } else {
                            newData.doc = null;
                            $eyeContainer.html('');
                        }
                    }, 100);

                    this.defaultSetCellValue(newData, value, currentRowData);
                },
            },
            {
                dataField: 'barcodE_ID',
                caption: 'Barcode Id',
                visible: false,
            },
            {
                dataField: 'stock',
                caption: "Balance",
                allowEditing: false,
                alignment: 'right',
            },
            {
                dataField: 'iteM_ID',
                caption: 'Item',
                disabled: true,
                allowEditing: false,
                visible: Type == "B",
                calculateCellValue: function (rowData) {
                    if (Type != "I") {
                        if (rowData.iteM_CODE) {
                            const selectedItem = empr_PurchaseBill.ItemsOnParty.find(u => u.key === parseInt(rowData.iteM_CODE));
                            if (selectedItem) {
                                return selectedItem.itemID;
                            }
                        }
                        return null;
                    } else {
                        return null;
                    }
                    if (Type != "B") {
                        if (rowData.iteM_CODE) {
                            const selectedItem = empr_PurchaseBill.ItemsOnParty.find(u => u.key === parseInt(rowData.iteM_CODE));
                            if (selectedItem) {
                                return selectedItem.code;
                            }
                        }
                    }
                }
            },
            {
                dataField: 'qty',
                caption: 'Qty',
                width: 150,
                alignment: 'right',
                setCellValue: function (newData, value, currentRowData) {
                    newData.qty = value;
                    var rate = parseFloat(currentRowData.rate) || 0;
                    var qty = parseFloat(newData.qty) || 0;
                    var qtY2 = parseFloat(currentRowData.qtY2) || 0;
                    if (isNaN(qty)) {
                        empr_helper.notify("Please enter the correct quantity.", 2);
                    }
                    if (isNaN(qtY2)) {
                        empr_helper.notify("Please enter the correct quantity2.", 2);
                    }
                    if (!isNaN(qty) && !isNaN(qtY2)) {
                        if (currentRowData.chK1) {
                            newData.baL_QTY = qty + qtY2;
                        }
                        else {
                            newData.baL_QTY = qty;
                        }
                        if (!isNaN(rate)) {
                            newData.amt = (newData.baL_QTY * rate).toFixed(2);

                            var Amount = newData.amt;
                            var Dis = parseFloat(currentRowData.disc) || 0;
                            var disSum = Amount * Dis / 100 || 0;
                            var Adv = parseFloat(currentRowData.adv) || 0;
                            var Tax = parseFloat(currentRowData.tax) || 0;
                            var DiscountAmount = disSum;
                            var TaxAmount = Amount - DiscountAmount || 0;

                            var TaxSum = TaxAmount * Tax / 100 || 0;
                            var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                            newData.disC_AMT = disSum.toFixed(2);
                            newData.taX_AMT = TaxSum.toFixed(2);
                            newData.adV_AMT = AdvSum.toFixed(2);

                            var NetAmount = Amount - DiscountAmount || 0;
                            if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                            } else {
                                newData.neT_AMT = 0;
                            }

                            setTimeout(function () {
                                empr_PurchaseBill.CalculateCommition();
                            }, 0);

                            //setTimeout(function () {
                            //    empr_PurchaseBill.CalculateCommition();
                            //}, 0);
                        }
                    }
                }
            },
            {
                dataField: 'picK_ID',
                caption: 'Pick ID',
                visible: false
            },
            {
                dataField: 'picK_ID_D',
                caption: 'Pick Detail ID',
                visible: false
            },
        ];
        if (Type === "I") {
            col.push(
                {
                    dataField: 'unit',
                    caption: 'Unit',
                    width: 150,
                    alignment: 'center',

                    calculateCellValue: function (data) {
                        return data.unit || Units[0].key;
                    },
                    lookup: {
                        dataSource: Units,
                        displayExpr: 'value',
                        valueExpr: 'key'
                    },

                },
                {
                    dataField: 'qtY2',
                    caption: 'Qty 2',
                    visible: CompCond == 2,
                    width: 75,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.qtY2 = value;
                        var qty = parseFloat(currentRowData.qty) || 0;
                        var qtY2 = parseFloat(newData.qtY2) || 0;
                        var rate = parseFloat(currentRowData.rate) || 0;
                        if (isNaN(qty)) {
                            empr_helper.notify("Please enter the correct quantity.", 2);
                        }
                        if (isNaN(qtY2)) {
                            empr_helper.notify("Please enter the correct quantity2.", 2);
                        }
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (currentRowData.chK1) {
                                newData.baL_QTY = qty + qtY2;
                            }
                            else {
                                newData.baL_QTY = qty;
                            }

                            if (!isNaN(newData.baL_QTY) && !isNaN(rate)) {
                                newData.amt = (newData.baL_QTY * rate).toFixed(2);
                            } else {
                                newData.amt = 0;
                            }

                            var Amount = newData.amt || 0;
                            var Discount = parseFloat(currentRowData.disc) || 0;
                            var Tax = parseFloat(currentRowData.tax) || 0;
                            var Adv = parseFloat(currentRowData.adv) || 0;
                            var DiscountAmount = Amount * Discount / 100 || 0;
                            var TaxAmount = Amount - DiscountAmount;
                            var TaxSum = TaxAmount * Tax / 100 || 0
                            var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0
                            var NetAmount = Amount - DiscountAmount || 0;
                            if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                            } else {
                                newData.neT_AMT = 0;
                            }
                        }
                    }
                },
                {
                    dataField: 'baL_QTY',
                    caption: 'Balance Quantity',
                    visible: CompCond == 2,
                    allowEditing: false,
                    cellTemplate: function (container, options) {
                        var $cell = $("<div>").addClass("custom-cell");
                        var isChecked = options.data.chK1;
                        var quantity = options.data.baL_QTY;
                        if (quantity == 0 || quantity == '' || quantity == null || quantity == undefined) {
                            quantity = '';
                        }
                        var $checkbox = $("<input type='checkbox'>")
                            .addClass("chkCell")
                            .prop('checked', isChecked)
                            .on('change', function () {
                                var item = options.data;
                                var qty = parseFloat(item.qty) || 0;
                                var qtY2 = parseFloat(item.qtY2) || 0;
                                if (isNaN(qty)) {
                                    empr_helper.notify("Please enter the correct quantity.", 2);
                                }
                                if (isNaN(qtY2)) {
                                    empr_helper.notify("Please enter the correct quantity2.", 2);
                                }
                                if (!isNaN(qty) && !isNaN(qtY2)) {
                                    if (this.checked) {
                                        item.baL_QTY = qty + qtY2;
                                        item.chk = "1";
                                    }
                                    else {
                                        item.baL_QTY = qty;
                                        item.chk = "0";
                                    }
                                    item.chK1 = this.checked;
                                    container.find('span').text(item.baL_QTY);
                                }


                                var gridInstance = $('#DetailContainer').dxDataGrid('instance');
                                var dataSource = gridInstance.option("dataSource");
                                dataSource[options.rowIndex] = item;
                                gridInstance.option("dataSource", dataSource);
                            });

                        var $span = $('<span>' + quantity + '</span>');
                        $cell.append($checkbox).append($span);
                        container.append($cell);
                    }
                },
                {
                    dataField: 'rate',
                    caption: 'Rate',
                    alignment: 'right',
                    width: 150,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.rate = value;
                        debugger;
                        var rate = parseFloat(newData.rate) || 0;
                        var qty = parseFloat(currentRowData.qty) || 0;
                        if (!isNaN(qty) && !isNaN(rate)) {
                            newData.amt = (qty * rate).toFixed(2);
                        } else {
                            newData.amt = 0;
                        }

                        var Amount = newData.amt || 0;
                        var Discount = parseFloat(currentRowData.disc) || 0;
                        var Tax = parseFloat(currentRowData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var DiscountAmount = Amount * Discount / 100 || 0;
                        var TaxAmount = Amount - DiscountAmount;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);
                        newData.disC_AMT = DiscountAmount.toFixed(2);
                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'amt',
                    caption: 'Amount',
                    alignment: 'right',
                    dataType: 'number',
                    format: { type: 'fixedPoint', precision: 0 },
                    width: 150,
                    allowEditing: false,
                },
                {
                    dataField: 'disc',
                    width: 75,
                    caption: 'Disc %',
                    visible: false,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.disc = parseFloat(value);
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var Discount = parseFloat(newData.disc) || 0;

                        if (!isNaN(Amount) && !isNaN(Discount)) {
                            newData.disC_AMT = (Amount * Discount / 100).toFixed(2);
                        } else {
                            newData.disC_AMT = 0;
                        }

                        var Tax = parseFloat(currentRowData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var DiscountAmount = parseFloat(newData.disC_AMT) || 0;
                        var TaxAmount = Amount - DiscountAmount;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);
                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'disC_AMT',
                    caption: 'Disc Amt',
                    width: 100,
                    visible: false,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.disC_AMT = value;
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var DiscountAmt = parseFloat(newData.disC_AMT) || 0;

                        if (!isNaN(Amount) && !isNaN(DiscountAmt)) {
                            newData.disc = ((DiscountAmt / Amount) * 100).toFixed(2);
                        } else {
                            newData.disc = 0;
                        }

                        var Tax = parseFloat(currentRowData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var DiscountAmount = parseFloat(newData.disC_AMT) || 0;
                        var TaxAmount = Amount - DiscountAmount;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);
                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'tax',
                    caption: 'Tax',
                    width: 75,
                    visible: false,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.tax = value;
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var DiscountAmount = parseFloat(currentRowData.disC_AMT) || 0;
                        var Tax = parseFloat(newData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var TaxAmount = Amount - DiscountAmount || 0;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                        if (!isNaN(TaxAmount) && !isNaN(Tax)) {
                            newData.taX_AMT = TaxSum.toFixed(2);
                            newData.adV_AMT = AdvSum.toFixed(2);
                        } else {
                            newData.taX_AMT = 0;
                        }

                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'taX_AMT',
                    caption: 'Tax Amount',
                    width: 100,
                    allowEditing: false,
                    visible: false,
                },
                {
                    dataField: 'neT_AMT',
                    width: 120,
                    caption: 'Net Amount',
                    allowEditing: false,
                    visible: false,
                },
                {
                    dataField: 'dT_DESC',
                    caption: 'Description',
                    width: 400,
                    wordWrapEnabled: true,
                },
                {
                    dataField: 'color',
                    caption: 'Color',
                    visible: false,
                    lookup: {
                        dataSource: Colors,
                        displayExpr: 'value',
                        valueExpr: 'key'
                    }
                },
                {
                    dataField: 'size',
                    caption: 'Size',
                    visible: false,
                    lookup: {
                        dataSource: Sizes,
                        displayExpr: 'value',
                        valueExpr: 'key'
                    }
                },
            );
        }
        else {
            col.push(
                {
                    dataField: 'qtY2',
                    caption: 'Qty 2',
                    visible: CompCond == 2,
                    width: 75,
                    setCellValue: function (newData, value, currentRowData) {
                        newData.qtY2 = value;
                        var qty = parseFloat(currentRowData.qty) || 0;
                        var qtY2 = parseFloat(newData.qtY2) || 0;
                        var rate = parseFloat(currentRowData.rate) || 0;
                        if (isNaN(qty)) {
                            empr_helper.notify("Please enter the correct quantity.", 2);
                        }
                        if (isNaN(qtY2)) {
                            empr_helper.notify("Please enter the correct quantity2.", 2);
                        }
                        if (!isNaN(qty) && !isNaN(qtY2)) {
                            if (currentRowData.chK1) {
                                newData.baL_QTY = qty + qtY2;
                            }
                            else {
                                newData.baL_QTY = qty;
                            }

                            if (!isNaN(newData.baL_QTY) && !isNaN(rate)) {
                                newData.amt = (newData.baL_QTY * rate).toFixed(2);
                            } else {
                                newData.amt = 0;
                            }

                            var Amount = newData.amt || 0;
                            var Discount = parseFloat(currentRowData.disc) || 0;
                            var Tax = parseFloat(currentRowData.tax) || 0;
                            var Adv = parseFloat(currentRowData.adv) || 0;
                            var DiscountAmount = Amount * Discount / 100 || 0;
                            var TaxAmount = Amount - DiscountAmount;
                            var TaxSum = TaxAmount * Tax / 100 || 0
                            var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0
                            var NetAmount = Amount - DiscountAmount || 0;
                            if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                                newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                            } else {
                                newData.neT_AMT = 0;
                            }
                        }
                    }
                },
                {
                    dataField: 'baL_QTY',
                    caption: 'Balance Quantity',
                    allowEditing: false,
                    visible: CompCond == 2,
                    cellTemplate: function (container, options) {
                        var $cell = $("<div>").addClass("custom-cell");
                        var isChecked = options.data.chK1;
                        var quantity = options.data.baL_QTY;
                        if (quantity == 0 || quantity == '' || quantity == null || quantity == undefined) {
                            quantity = '';
                        }
                        console.log("isChecked:", isChecked);
                        var $checkbox = $("<input type='checkbox'>")
                            .addClass("chkCell")
                            .prop('checked', isChecked)
                            .on('change', function () {
                                var item = options.data;
                                var qty = parseFloat(item.qty) || 0;
                                var qtY2 = parseFloat(item.qtY2) || 0;
                                if (isNaN(qty)) {
                                    empr_helper.notify("Please enter the correct quantity.", 2);
                                }
                                if (isNaN(qtY2)) {
                                    empr_helper.notify("Please enter the correct quantity2.", 2);
                                }
                                if (!isNaN(qty) && !isNaN(qtY2)) {
                                    if (this.checked) {
                                        item.baL_QTY = qty + qtY2;
                                        item.chk = "1";
                                    }
                                    else {
                                        item.baL_QTY = qty;
                                        item.chk = "0";
                                    }
                                    item.chK1 = this.checked;
                                    container.find('span').text(item.baL_QTY);
                                }


                                var gridInstance = $('#DetailContainer').dxDataGrid('instance');
                                var dataSource = gridInstance.option("dataSource");
                                dataSource[options.rowIndex] = item;
                                gridInstance.option("dataSource", dataSource);
                            });

                        var $span = $('<span>' + quantity + '</span>');
                        $cell.append($checkbox).append($span);
                        container.append($cell);
                    }
                },
                {
                    dataField: 'rate',
                    caption: 'Rate',
                    dataType: 'number',
                    width: 80,
                    setCellValue: function (newData, value, currentRowData) {
                        ////debugger;
                        newData.rate = value;

                        var row = Object.assign({}, currentRowData, newData);

                        var rate = parseFloat(row.rate) || 0;
                        var qty = parseFloat(row.baL_QTY) || 0;

                        var Amount = qty * rate;
                        newData.amt = Amount.toFixed(2);

                        var Discount = parseFloat(row.disc) || 0;
                        var Tax = parseFloat(row.tax) || 0;
                        var Adv = parseFloat(row.adv) || 0;

                        var DiscountAmount = Amount * Discount / 100;
                        var Taxable = Amount - DiscountAmount;
                        var TaxSum = Taxable * Tax / 100;
                        var AdvSum = (Taxable + TaxSum) * Adv / 100;

                        newData.disC_AMT = DiscountAmount.toFixed(2);
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);

                        newData.neT_AMT = (Taxable + TaxSum + AdvSum).toFixed(2);
                    }
                },
                {
                    dataField: 'amt',
                    caption: 'Amount',
                    width: 100,
                    allowEditing: false,
                    alignment: 'right',
                },
                {
                    dataField: 'disc',
                    width: 75,
                    alignment: 'right',
                    caption: 'Disc %',
                    setCellValue: function (newData, value, currentRowData) {
                        newData.disc = parseFloat(value);
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var Discount = parseFloat(newData.disc) || 0;
                        if (!isNaN(Amount) && !isNaN(Discount)) {
                            newData.disC_AMT = (Amount * Discount / 100).toFixed(2);
                        } else {
                            newData.disC_AMT = 0;
                        }

                        var Tax = parseFloat(currentRowData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var DiscountAmount = parseFloat(newData.disC_AMT) || 0;
                        var TaxAmount = Amount - DiscountAmount;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);
                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'disC_AMT',
                    width: 100,
                    caption: 'Disc Amt',
                    alignment: 'right',

                    setCellValue: function (newData, value, currentRowData) {
                        newData.disC_AMT = value;
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var DiscountAmt = parseFloat(newData.disC_AMT) || 0;

                        if (!isNaN(Amount) && !isNaN(DiscountAmt)) {
                            newData.disc = ((DiscountAmt / Amount) * 100).toFixed(2);
                        } else {
                            newData.disc = 0;
                        }

                        var Tax = parseFloat(currentRowData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var DiscountAmount = parseFloat(newData.disC_AMT) || 0;
                        var TaxAmount = Amount - DiscountAmount;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;
                        newData.taX_AMT = TaxSum.toFixed(2);
                        newData.adV_AMT = AdvSum.toFixed(2);
                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    },
                    calculateCellValue: function (rowData) {

                        if (rowData.disC_AMT) return rowData.disC_AMT;

                        var Amount = parseFloat(rowData.amt) || 0;
                        var Disc = parseFloat(rowData.disc) || 0;

                        return (Amount * Disc / 100).toFixed(2);
                    }
                },
                {
                    dataField: 'tax',
                    width: 75,
                    caption: 'Tax',
                    alignment: 'right',

                    setCellValue: function (newData, value, currentRowData) {
                        newData.tax = value;
                        var Amount = parseFloat(currentRowData.amt) || 0;
                        var DiscountAmount = parseFloat(currentRowData.disC_AMT) || 0;
                        var Tax = parseFloat(newData.tax) || 0;
                        var Adv = parseFloat(currentRowData.adv) || 0;
                        var TaxAmount = Amount - DiscountAmount || 0;
                        var TaxSum = TaxAmount * Tax / 100 || 0;
                        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0;

                        if (!isNaN(TaxAmount) && !isNaN(Tax)) {
                            newData.taX_AMT = TaxSum.toFixed(2);
                            newData.adV_AMT = AdvSum.toFixed(2);
                        } else {
                            newData.taX_AMT = 0;
                        }

                        var NetAmount = Amount - DiscountAmount || 0;
                        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                        } else {
                            newData.neT_AMT = 0;
                        }
                    }
                },
                {
                    dataField: 'taX_AMT',
                    caption: 'Tax Amount',
                    width: 100,
                    allowEditing: false,
                    alignment: 'right',


                },
                //{
                //    dataField: 'adv',
                //    width: 75,
                //    caption: 'Adv',
                //    setCellValue: function (newData, value, currentRowData) {
                //        newData.adv = value;
                //        var Amount = parseFloat(currentRowData.amt) || 0;
                //        var DiscountAmount = parseFloat(currentRowData.disC_AMT) || 0;
                //        var Adv = parseFloat(newData.adv) || 0;
                //        var TaxAmount = Amount - DiscountAmount || 0;
                //        var Tax = parseFloat(currentRowData.tax) || 0;
                //        var TaxSum = TaxAmount * Tax / 100 || 0
                //        var AdvSum = (TaxAmount + TaxSum) * Adv / 100 || 0

                //        if (!isNaN(TaxAmount) && !isNaN(Adv)) {
                //            newData.adV_AMT = AdvSum.toFixed(2);
                //        } else {
                //            newData.adV_AMT = 0;
                //        }

                //        var NetAmount = Amount - DiscountAmount || 0;
                //        if (!isNaN(NetAmount) && !isNaN(TaxSum) && !isNaN(AdvSum)) {
                //            newData.neT_AMT = (NetAmount + TaxSum + AdvSum).toFixed(2);
                //        } else {
                //            newData.neT_AMT = 0;
                //        }
                //    }
                //},
                //{
                //    dataField: 'adV_AMT',
                //    caption: 'Adv Amount',
                //    width: 100,
                //    allowEditing: false,
                //},
                {
                    dataField: 'neT_AMT',
                    width: 120,
                    caption: 'Net Amount',
                    allowEditing: false,
                    alignment: 'right',

                },
                {
                    dataField: 'dT_DESC',
                    caption: 'Description',
                    editorOptions: {
                        onKeyDown: function (e) {
                            if (e.event.key === 'Tab') {
                                setTimeout(function () {
                                    const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                                    gridInstance.resize();
                                }, 500);
                            }
                        },
                    },
                },
                {
                    dataField: 'color',
                    caption: 'Color',
                    lookup: {
                        dataSource: Colors,
                        displayExpr: 'value',
                        valueExpr: 'key'
                    }
                },
                {
                    dataField: 'size',
                    caption: 'Size',
                    lookup: {
                        dataSource: Sizes,
                        displayExpr: 'value',
                        valueExpr: 'key'
                    }
                },
                //{
                //    dataField: 'grade',
                //    caption: 'Grade',
                //    lookup: {
                //        dataSource: Grades,
                //        displayExpr: 'value',
                //        valueExpr: 'key'
                //    }
                //},
                //{
                //    dataField: 'warehouse',
                //    caption: 'Warehouse',
                //    lookup: {
                //        dataSource: Warehouses,
                //        displayExpr: 'value',
                //        valueExpr: 'key'
                //    }
                //},
                //{
                //    dataField: "warehouse",
                //    caption: "Warehouse",
                //    calculateCellValue: function (rowData) {
                //        let item = Warehouse.find(x => x.key === rowData.warehouse);
                //        return item ? item.value : "";
                //    },
                //    editorType: "dxDropDownBox",
                //    editorOptions: {
                //        dataSource: Warehouse,
                //        valueExpr: "key",
                //        displayExpr: "value",
                //        contentTemplate: function (e, cellInfo) {
                //            let $grid = $("<div>").dxDataGrid({
                //                dataSource: Warehouse,

                //                keyExpr: "key",
                //                columns: [
                //                    { dataField: "grcode", caption: "Group Code", width: 80 },
                //                    { dataField: "value", caption: "Name" },
                //                    { dataField: "name", caption: "Control " }
                //                ],
                //                selection: { mode: "single" },
                //                hoverStateEnabled: true,
                //                height: 200,
                //                searchPanel: {
                //                    visible: true,
                //                    width: 780,
                //                    placeholder: "Search..."
                //                },
                //                onSelectionChanged: function (selectedItems) {
                //                    let selectedKey = selectedItems.selectedRowKeys[0];
                //                    e.component.option("value", selectedKey);   // dropdown ke liye
                //                    cellInfo.setValue = selectedKey;             // parent grid ke liye
                //                    e.component.close();
                //                }
                //            });

                //            return $grid;
                //        }
                //    },
                //    width: 800,
                //},
                //{
                //    dataField: 'deL_DATE',
                //    caption: 'Delivery Date',
                //    dataType: 'date',
                //    format: 'dd-MM-yyyy',
                //},
                //{
                //    dataField: 'duE_DATE',
                //    caption: 'Due Date',
                //    dataType: 'date',
                //    format: 'dd-MM-yyyy',
                //},
                //{
                //    dataField: 'duE_DAYS',
                //    caption: 'Due Days',
                //},
                //{
                //    dataField: 'veh',
                //    caption: 'Vehicle #',
                //}
                //,
                //{
                //    dataField: 'iteM_ID',
                //    caption: 'Item Id',
                //    allowEditing: false,
                //}
            );
        }
        empr_helper.editableDxGridbindingForTransactionsVouchers('#DetailContainer', col, dataSrc, "PurchaseBill", "iteM_CODE");


        var detailGrid = $('#DetailContainer').dxDataGrid('instance');
        detailGrid.option('onCellValueChanged', function (e) {
            if (e.column.dataField === 'amt') {
                empr_PurchaseBill.SetDiscount('GRID');
            }
        });
        detailGrid.option('onContentReady', function () {
            empr_PurchaseBill.SetDiscount('GRID');
        });
        empr_PurchaseBill.SetDiscount('GRID');


        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }

        //setTimeout(function () {
        //    var nextElement = $('#DetailContainer').dxDataGrid('instance').getCellElement(0, 'qty');
        //    $('#DetailContainer').dxDataGrid('instance').focus(nextElement);
        //}, 1500);
    },
    populateRowData: async function (rowData, itemKey, currentRowData) {
        rowData.iteM_CODE = itemKey;
        if (itemKey) {
            debugger;
            //if (Type != "I") {
            //    try {
            //        const rate = await empr_PurchaseBill.GetLastRate(itemKey);
            //        rowData.rate = rate;
            //        rowData.amt = rate;
            //        rowData.neT_AMT = rate;
            //    } catch (e) {
            //        rowData.rate = 0;
            //    }
            //    var selectedItem = Items.find(u => u.key == itemKey);
            //    if (selectedItem) {
            //        rowData.iteM_ID = selectedItem.itemID;
            //        rowData.color = selectedItem.color;
            //        rowData.size = selectedItem.size;
            //    }
            //} else if (Type == "I") {
            //    var selectedItem = Items.find(u => String(u.key) === String(itemKey));
            //    if (selectedItem) {
            //        ////debugger;
            //        var rate = parseInt(selectedItem.rate, 10) || 0;
            //        var qty = 1;
            //        if (currentRowData) {
            //            qty = parseInt(currentRowData.qty, 10) || 0;
            //        }

            //        rowData.rate = selectedItem.rate;
            //        rowData.amt = rate * qty;
            //        rowData.hS_CODE = selectedItem.hscode;
            //    }
            //}

            var selectedItem = empr_PurchaseBill.ItemsOnParty.find(u => String(u.key) === String(itemKey));
            if (selectedItem) {
                ////debugger;
                var rate = parseInt(selectedItem.rate, 10) || 0;
                var qty = 1;
                if (currentRowData) {
                    qty = parseInt(currentRowData.qty, 10) || 0;
                }

                rowData.rate = selectedItem.rate;
                rowData.amt = rate * qty;
                rowData.hS_CODE = selectedItem.hscode;
            }
        } else {
            rowData.rate = 0;
        }
        return rowData;
    },
    CreateCommGrid: function (dataSrc) {
        console.log('CreateCommGrid', dataSrc);
        if (dataSrc.length > 0) {
            empr_PurchaseBill.rowsCount = dataSrc.length - 1;
        }
        var Caption = "";
        if (Type == "I") {
            Caption = "Item";
        }
        else {
            Caption = "Bar Code";
        }
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
                    if (Permissions != "Admin") {
                        const copyAction = !Permissions.r_COPY
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PurchaseBill.CommCloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT)
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PurchaseBill.CommAddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        const deleteAction = !Permissions.r_DLT
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseBill.CommDeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const actions = `<div class="btn-group btn-group-sm">${copyAction}${addAction}${deleteAction}</div>`;
                        $(actions).appendTo(container);
                    }
                    else {
                        $(`<div class="btn-group btn-group-sm">
                           <a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PurchaseBill.CommCloneRow(`+ options.rowIndex + `)" title="Duplicate"><i class="fa fa-clone"></i></a>
                           <a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PurchaseBill.CommAddRow()" title="Add"><i class="fa fa-add"></i></a>
                           <a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseBill.CommDeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>
                           </div>`).appendTo(container);
                    }
                }
            },
            {
                dataField: 'bilL_TRAN_ID',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'traN_ID',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'dT_CODE',
                caption: 'Code',
                visible: false,
            },
            {
                dataField: 'iteM_CODE',
                caption: "Items",
                alignment: 'center',
                lookup: {
                    dataSource: {
                        store: empr_PurchaseBill.ItemsOnParty,
                        paginate: true,
                        pageSize: 50
                    },
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    showClearButton: true,
                    paging: {
                        enabled: true,
                        pageSize: 50,
                    }
                },
            },
            {
                dataField: 'comM_UNIT',
                caption: 'Commission Unit',
                alignment: 'center',
                lookup: {
                    dataSource: {
                        store: empr_helper.commType,
                        paginate: true,
                        pageSize: 50
                    },
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    showClearButton: true,
                    paging: {
                        enabled: true,
                        pageSize: 50,
                    }
                },
            },
            {
                dataField: 'comM_VALUE',
                caption: 'Commission value',
                alignment: 'center',
            },
        ];
        empr_helper.editableDxGridbindingForTransactionsVouchers('#CommContainer', col, dataSrc, "PurchaseBill", "iteM_CODE");
        if (dataSrc.length == 0) {
            $('#CommContainer').dxDataGrid('instance').addRow().done(function () {
                $('#CommContainer').dxDataGrid('instance').saveEditData();
            });
        }

        setTimeout(function () {
            var nextElement = $('#CommContainer').dxDataGrid('instance').getCellElement(0, 'iteM_CODE');
            $('#CommContainer').dxDataGrid('instance').focus(nextElement);
        }, 500);
    },
    CloneRow: function (index) {
        if (empr_PurchaseBill.pickIds.length > 0) {
            empr_helper.notify("Cannot clone row on return data.", 2);
        } else {
            const gridIns = $('#DetailContainer').dxDataGrid('instance');
            const dataSrc = gridIns.option("dataSource");

            if (dataSrc.length >= Limit && Limit != 0) {
                empr_helper.notify("You can only add  " + Limit + " records.", 2);
                return;
            }
            if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
                $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {

                    empr_PurchaseBill.rowsCount += 1;
                    const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                    var dataSource = gridInstance.option("dataSource");
                    if (dataSource.length > 0) {
                        let clonedRowData = $.extend(true, {}, dataSource[index]);
                        if (clonedRowData.hasOwnProperty('dT_CODE')) {
                            delete clonedRowData.dT_CODE;
                        }
                        clonedRowData.__KEY__ = empr_PurchaseBill.GenerateKey(36);
                        clonedRowData.dT_CODE = 0;
                        let newDataSource = [clonedRowData].concat(dataSource);
                        gridInstance.option("dataSource", newDataSource);
                        gridInstance.refresh();
                    }
                });
            }
            else {
                empr_PurchaseBill.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    let clonedRowData = $.extend(true, {}, dataSource[index]);
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    clonedRowData.__KEY__ = empr_PurchaseBill.GenerateKey(36);
                    clonedRowData.dT_CODE = 0;
                    let newDataSource = [clonedRowData].concat(dataSource);
                    gridInstance.option("dataSource", newDataSource);
                    gridInstance.refresh();
                }
            }
        }
    },
    CommCloneRow: function (index) {
        if (empr_PurchaseBill.pickIds.length > 0) {
            empr_helper.notify("Cannot clone row on return data.", 2);
        } else {
            const gridIns = $('#CommContainer').dxDataGrid('instance');
            const dataSrc = gridIns.option("dataSource");

            if (dataSrc.length >= Limit && Limit != 0) {
                empr_helper.notify("You can only add  " + Limit + " records.", 2);
                return;
            }
            if ($('#CommContainer').dxDataGrid('instance').hasEditData()) {
                $('#CommContainer').dxDataGrid('instance').saveEditData().done(function () {

                    empr_PurchaseBill.rowsCount += 1;
                    const gridInstance = $('#CommContainer').dxDataGrid('instance');
                    var dataSource = gridInstance.option("dataSource");
                    if (dataSource.length > 0) {
                        let clonedRowData = $.extend(true, {}, dataSource[index]);
                        if (clonedRowData.hasOwnProperty('dT_CODE')) {
                            delete clonedRowData.dT_CODE;
                        }
                        clonedRowData.__KEY__ = empr_PurchaseBill.GenerateKey(36);
                        clonedRowData.dT_CODE = 0;
                        let newDataSource = [clonedRowData].concat(dataSource);
                        gridInstance.option("dataSource", newDataSource);
                        gridInstance.refresh();
                    }
                });
            }
            else {
                empr_PurchaseBill.rowsCount += 1;
                const gridInstance = $('#CommContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    let clonedRowData = $.extend(true, {}, dataSource[index]);
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    clonedRowData.__KEY__ = empr_PurchaseBill.GenerateKey(36);
                    clonedRowData.dT_CODE = 0;
                    let newDataSource = [clonedRowData].concat(dataSource);
                    gridInstance.option("dataSource", newDataSource);
                    gridInstance.refresh();
                }
            }
        }
    },
    AddRow: function () {
        if (empr_PurchaseBill.pickIds.length > 0) {
            empr_helper.notify("Cannot add row on return data.", 2);
        } else {
            const gridIns = $('#DetailContainer').dxDataGrid('instance');
            const dataSrc = gridIns.option("dataSource");

            if (dataSrc.length >= Limit && Limit != 0) {
                empr_helper.notify("You can only add  " + Limit + " records.", 2);
                return;
            }
            if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
                $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                    empr_PurchaseBill.rowsCount += 1;
                    const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                    const dataSource = gridInstance.option("dataSource");

                    dataSource.unshift({ __KEY__: empr_PurchaseBill.GenerateKey(36), dT_CODE: 0 });
                    gridInstance.option("dataSource", dataSource);
                    gridInstance.refresh();
                    empr_helper.MoveFocusToGridWithouTab('#DetailContainer', 0, 'iteM_CODE')
                });
            }
            else {
                empr_PurchaseBill.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");

                dataSource.unshift({ __KEY__: empr_PurchaseBill.GenerateKey(36), dT_CODE: 0 });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
                //empr_helper.MoveFocusToGridWithouTab('#DetailContainer', 0, 'iteM_CODE')
            }
        }
    },
    CommAddRow: function () {
        if (empr_PurchaseBill.pickIds.length > 0) {
            empr_helper.notify("Cannot add row on return data.", 2);
        } else {
            const gridIns = $('#CommContainer').dxDataGrid('instance');
            const dataSrc = gridIns.option("dataSource");

            if (dataSrc.length >= Limit && Limit != 0) {
                empr_helper.notify("You can only add  " + Limit + " records.", 2);
                return;
            }
            if ($('#CommContainer').dxDataGrid('instance').hasEditData()) {
                $('#CommContainer').dxDataGrid('instance').saveEditData().done(function () {
                    empr_PurchaseBill.rowsCount += 1;
                    const gridInstance = $('#CommContainer').dxDataGrid('instance');
                    const dataSource = gridInstance.option("dataSource");

                    dataSource.unshift({ __KEY__: empr_PurchaseBill.GenerateKey(36), dT_CODE: 0, traN_ID: empr_PurchaseBill.CommissionTranId });
                    gridInstance.option("dataSource", dataSource);
                    gridInstance.refresh();
                    empr_helper.MoveFocusToGridWithouTab('#CommContainer', 0, 'iteM_CODE')
                });
            }
            else {
                empr_PurchaseBill.rowsCount += 1;
                const gridInstance = $('#CommContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");

                dataSource.unshift({ __KEY__: empr_PurchaseBill.GenerateKey(36), dT_CODE: 0, traN_ID: empr_PurchaseBill.CommissionTranId });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
                empr_helper.MoveFocusToGridWithouTab('#CommContainer', 0, 'iteM_CODE')
            }
        }
    },
    DeleteRow: function (index, dtCode) {
        ;
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");

        gridInstance.cancelEditData();
        if (dataSource.length > 0) {
            if (dataSource.length > 1) {
                var row = dataSource[index];
                if (dtCode == '' || dtCode == null || dtCode == undefined) {
                    gridInstance.deleteRow(index);
                    empr_PurchaseBill.rowsCount -= 1;
                    gridInstance.saveEditData();
                }
                else {
                    debugger;
                    if (Permissions == "Admin" || Permissions.r_DLT) {
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
                                ajaxHelper.ajaxPostJsonData({ code: dtCode }, "/PurchaseBill/DeletePurchaseBillDetailByCode", function (data) {
                                    empr_helper.notify(data.msg, data.msgType);
                                    if (data.msgType == 1) {
                                        gridInstance.deleteRow(index);
                                        empr_PurchaseBill.rowsCount -= 1;
                                        gridInstance.saveEditData();
                                    }
                                }, false, true);
                            });
                        } else {
                            empr_helper.notify("You are not allowed to delete the last row.", 2);
                        }
                    }
                    else {
                        empr_helper.notify('You dont have right to delete the records', 2);
                    }
                }

            }
            else {
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            }
        }
    },
    CommDeleteRow: function (index, dtCode) {
        ////debugger;
        const gridInstance = $('#CommContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");

        if (dataSource.length > 0) {
            /*if (dataSource.length > 1) {*/
            var row = dataSource[index];
            if (dtCode == '' || dtCode == null || dtCode == undefined) {
                gridInstance.deleteRow(index);
                empr_PurchaseBill.rowsCount -= 1;
                gridInstance.saveEditData();
            }
            else {
                //var availableRows = dataSource.filter(x => x.dT_CODE > 0);
                //if (availableRows.length > 1) {
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
                    ajaxHelper.ajaxPostJsonData({ code: dtCode }, "/PurchaseBill/DeleteCommDetailByCode", function (data) {
                        empr_helper.notify(data.msg, data.msgType);
                        if (data.msgType == 1) {
                            gridInstance.deleteRow(index);
                            empr_PurchaseBill.rowsCount -= 1;
                            gridInstance.saveEditData();
                        }
                    }, false, true);
                });
                //} else {
                //    empr_helper.notify("You are not allowed to delete the last row.", 2);
                //}
            }
            //}
            //else {
            //    empr_helper.notify("You are not allowed to delete the last row.", 2);
            //}
        }
    },
    dropDownBoxEditorTemplate: function (cellElement, cellInfo) {
        return $('<div>').dxDropDownBox({
            dropDownOptions: { width: 500 },
            dataSource: [
                { key: 1, value: 'Value 1' },
                { key: 2, value: 'Value 2' },
                { key: 3, value: 'Value 3' }
            ],
            value: cellInfo.value,
            valueExpr: 'value',
            keyExpr: 'key',
            displayExpr: 'value',
            inputAttr: { 'aria-label': 'Owner' },
            contentTemplate(e) {
                return $('<div>').dxDataGrid({
                    dataSource: [
                        { key: 1, value: 'Value 1' },
                        { key: 2, value: 'Value 2' },
                        { key: 3, value: 'Value 3' }
                    ],
                    remoteOperations: true,
                    columns: [
                        { dataField: "key", caption: "Code" },
                        { dataField: "value", caption: "Name" },
                        { dataField: "name", caption: "Control Name" }
                    ],
                    hoverStateEnabled: true,
                    scrolling: { mode: 'virtual' },
                    height: 250,
                    selection: { mode: 'single' },
                    selectedRowKeys: [cellInfo.value],
                    keyExpr: 'key',
                    onSelectionChanged(selectionChangedArgs) {
                        e.component.option('value', selectionChangedArgs.selectedRowKeys[0]);
                        cellInfo.setValue(selectionChangedArgs.selectedRowKeys[0]);
                        if (selectionChangedArgs.selectedRowKeys.length > 0) {
                            e.component.close();
                        }
                    },
                });
            },
        });
    },
    BindDxGridBoxDdl: function (divid, datasrc, col, hiddenid, selectedOjb, selectval, valueExpr, displayExpr, displayExprHidden, onchangeFun) {
        ati_dxHelper.DxGridBoxDropdown(divid, datasrc, col, hiddenid, selectedOjb, selectval, valueExpr, displayExpr, displayExprHidden, onchangeFun);
    },
    InitQuickSearchGrid: function () {
        empr_PurchaseBill.GetPurchaseBill();
        //empr_PurchaseBill.CreateQuickSearchGrid();
    },
    GetPurchaseBill: function () {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPurchaseBill', function (data) {
            if (data.msgType == 1) {
                empr_PurchaseBill.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    CreateQuickSearchGrid: function (dataSrc) {
        console.log(dataSrc)
        var col = [];
        //debugger;
        if (dataSrc.length > 0 ? dataSrc[0].amt != undefined : false) {
            col = [{
                dataField: "Action",
                width: 100,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                cellTemplate: function (container, options) {
                    empr_PurchaseBill.vdate = options.data.v_DATE;
                    if (Permissions != "Admin" && !Permissions.r_PRINT) {
                        $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.code} title="Edit"><i class="fa fa-edit"></i></a>
                               </div>`).appendTo(container);
                    } else {
                        $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.code} title="Edit"><i class="fa fa-edit"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_print" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.code} partyCode=${options.data.partY_CODE} actCode=${options.data.acT_CODE} title="PRINT"><i class="fa fa-print"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.code} title="COPY"><i class="fa fa-copy"></i></a>
                               </div>`).appendTo(container);
                    }
                }
            },
            { dataField: 'id', caption: 'Code', width: 80, alignment: "center" },
            { dataField: 'v_DATE', caption: 'Voucher Date', dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'voucheR_NO', caption: 'Voucher No', },
            { dataField: 'partY_NAME', caption: 'Party Name', },
            { dataField: 'ref', caption: 'Reference No', },
            { dataField: 'remarks', caption: 'Description', },
            { dataField: 'amt', caption: 'Amount', },
            { dataField: 'astatus', caption: 'Status', },
            ];
        }
        else {
            col = [{
                dataField: "Action",
                width: 100,
                alignment: 'center',
                fixed: true,
                fixedPosition: "left",
                allowExporting: false,
                cellTemplate: function (container, options) {


                    $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.code} title="Edit"><i class="fa fa-edit"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_print" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.code} partyCode=${options.data.partY_CODE} actCode=${options.data.acT_CODE} title="PRINT"><i class="fa fa-print"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.code} title="COPY"><i class="fa fa-copy"></i></a>
                               </div>`).appendTo(container);
                }
            },
            { dataField: 'id', caption: 'Code', width: 80, alignment: "center" },
            { dataField: 'v_DATE', caption: 'Voucher Date', dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'voucheR_NO', caption: 'Voucher No', },
            { dataField: 'partY_NAME', caption: 'Party Name', },
            { dataField: 'ref', caption: 'Reference No', },
            { dataField: 'remarks', caption: 'Remarks', },
            { dataField: 'astatus', caption: 'Status', },
            ]
        }
        //empr_helper.dxGridbindingLazyLoading('#gridContainer', col, "/PurchaseBill/GetPurchaseBill", "id", "PartyOpening");
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "PurchaseBillQS");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    GetPurchaseBillByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPurchaseBillByCode?code=' + code, function (data) {
            console.log('editData', data);
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                if (masterData.length == 1) {
                    debugger;
                    $('#pickItems').show();
                    empr_PurchaseBill.pickIds = [];
                    var response = masterData[0];
                    empr_PurchaseBill.vDate = response.v_DATE;
                    var filteredData = $.grep(PartyType, function (item) {
                        return item.partyCode === response.partY_CODE && item.accountCode === response.acT_CODE.toString();
                    });
                    empr_PurchaseBill.partyCode = filteredData[0].partyCode;
                    empr_PurchaseBill.actCode = filteredData[0].accountCode;
                    $('#Code').val(response.id);
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                    $('#PARTY_CODE').dxSelectBox('instance').option('value', filteredData[0].key);
                    $('#partyhidden').val(filteredData[0].partyCode)
                    //$('#Currency').dxSelectBox('instance').option('value', response.curR_CODE);
                    $('#BACT').dxSelectBox('instance').option('value', response.bact);
                    $('#CACT').dxSelectBox('instance').option('value', response.cact);
                    $('#BAMT').val(response.bamt);
                    $('#CAMT').val(response.camt);
                    $('#Rate').val(response.crate);
                    $('#REF').val(response.ref);
                    $('#REMARKS').val(response.remarks);
                    //$('#HS_CODE').val(response.hS_CODE);
                    $('#COMM').val(response.comm);
                    $('#TERMS').val(response.terms);
                    $('#COMM_VAL').val(response.comM_VAL);
                    //$('#COMM_AMT').dxSelectBox('instance').option('value', response.comM_AMT);
                    empr_PurchaseBill.InitSalesman(0, parseInt(response.scode));
                    //$('#SCODE').dxSelectBox('instance').option('value', parseInt(response.scode));
                    $('#DISC').val(response.disc);
                    $('#DISC_RATE').val(response.disC_RATE);
                    $('#CARTAGE').val(response.cartage);
                    $('#V_DATE').val(response.v_DATE);
                    $('#VOUCHER_NO').val(response.voucheR_NO);
                    $('#hdnDOC').val(response.doc);
                    $('#D_CHARGES').val(response.d_CHARGES);
                    //$('#BtnDelete').show();
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
                ////debugger;
                if (data.commission.msgType == 1) {
                    if (masterData[0].scode != '0' && data.commission.data.length > 0) {
                        empr_PurchaseBill.CommissionTranId = data.commission.data[0].traN_ID;
                        $('.nav-item.commission').show();
                        empr_PurchaseBill.CreateCommGrid(data.commission.data);
                    }
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                }
                if (data.detail.msgType == 1) {
                    ////debugger;
                    var updatedDetailData = data.detail.data.map(item => {
                        var stockRecord = empr_PurchaseBill.CurrentStock.find(s => s.itemId == item.iteM_CODE);

                        return {
                            ...item,
                            stock: stockRecord ? stockRecord.balance : 0
                        };
                    });

                    empr_PurchaseBill.CreateGrid(updatedDetailData);
                }
                else {
                    empr_helper.notify(data.msg, data.msgType);
                }

            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetPurchaseBillDetailByItem: function (code, qty, disc) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPurchaseBillDetailByItem?code=' + code + '&qty=' + qty + '&disc=' + disc, function (data) {
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    empr_PurchaseBill.rowsCount += data.data.length;
                    const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                    var dataSource = gridInstance.option("dataSource");
                    $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                        dataSource = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
                    });
                    if ((dataSource[0].dT_CODE == undefined || dataSource[0].dT_CODE == 0) && (dataSource[0].iteM_CODE == "" || dataSource[0].iteM_CODE == null || dataSource[0].iteM_CODE == undefined)) {
                        empr_PurchaseBill.CreateGrid(data.data);
                    } else {
                        dataSource.unshift(...data.data);
                        gridInstance.option("dataSource", dataSource);
                        gridInstance.refresh();
                        empr_helper.MoveFocusToGridWithouTab('#DetailContainer', 0, 'iteM_CODE')
                    }
                }
                $('.bs-example-modal-lg').modal('hide');
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetPurchaseBillDetailByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPurchaseBillDetailByCode?code=' + code, function (data) {
            if (data.msgType == 1) {
                empr_PurchaseBill.CreateGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetPurchaseBillPickDetailByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetPurchaseBillPickDetailByCode?code=' + code, function (data) {
            if (data.msgType == 1) {
                if (Type == "I") {
                    empr_PurchaseBill.CreateGrid(data.data);
                } else {
                    if (data.data.length > 0) {
                        if ($('#SodaPickDetailGridContainer').data('dxDataGrid') != undefined) {
                            $('#SodaPickDetailGridContainer').data('dxDataGrid').dispose();
                        }
                        console.log(data.data)
                        empr_PurchaseBill.CreatePickDetailGrid(data.data);
                        $('#SodaPickDetailModal').modal('show');
                    } else {
                        empr_helper.notify("No data found.", 2);
                    }
                }
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    GetDataToSave: function () {
        var ID = $("#Code").val();
        var V_DATE = $("#V_DATE").val();
        var VOUCHER_NO = $("#VOUCHER_NO").val();
        var PARTY_CODE = $("#partyhidden").val();
        var ACT_CODE = $("#acthidden").val();
        var REF = $("#REF").val();
        var REMARKS = $("#REMARKS").val();
        //var BARCODEID = $("#barcodeId").val();
        var COMM = $("#COMM").val();
        var COMM_VAL = $("#COMM_VAL").val();
        ////debugger;
        var COMM_AMT = $("#COMM_AMT").dxSelectBox('option', 'value');
        var CACT = $("#CACT").dxSelectBox('option', 'value');
        var BACT = $("#BACT").dxSelectBox('option', 'value');
        var BAMT = $("#BAMT").val();
        var CAMT = $("#CAMT").val();

        var DISC = $("#DISC").val();
        var DISC_RATE = $("#DISC_RATE").val();
        var CARTAGE = $("#CARTAGE").val();
        var TERMS = $("#TERMS").val();

        var SCODE = $("#SCODE").dxSelectBox('option', 'value');
        var BTYPE = $("#BTYPE").dxSelectBox('option', 'value');
        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value');
        var DOC = $("#hdnDOC").val();
        var CRATE = $("#Rate").val();
        var D_CHARGES = $("#D_CHARGES").val();
        var CURR_CODE = $('#Currency').dxSelectBox('option', 'value');

        var masterRecord = {
            TRAN_ID: ID,
            V_DATE: V_DATE,
            VOUCHER_NO: VOUCHER_NO,
            PARTY_CODE: PARTY_CODE,
            ACT_CODE: ACT_CODE,
            COMM: COMM,
            COMM_VAL: COMM_VAL,
            COMM_AMT: COMM_AMT,
            DISC: DISC,
            DISC_RATE: DISC_RATE,
            CARTAGE: CARTAGE,
            SCODE: SCODE,
            BTYPE: BTYPE,
            REF: REF,
            REMARKS: REMARKS,
            ASTATUS: ASTATUS,
            //HS_CODE: HS_CODE,
            CURR_CODE: CURR_CODE,
            CRATE: CRATE,
            DOC: DOC,
            TERMS: TERMS,
            //BARCODE_ID: BARCODEID,
            D_CHARGES: D_CHARGES,
            CACT: CACT,
            BACT: BACT,
            CAMT: CAMT,
            BAMT: BAMT
        }
        ////debugger;
        var detailRecords = [];
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
            });
        }
        else {
            detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
        }
        var commRecords = [];
        if ($('#CommContainer').dxDataGrid('instance').hasEditData()) {
            $('#CommContainer').dxDataGrid('instance').saveEditData().done(function () {
                commRecords = $('#CommContainer').dxDataGrid('instance').option("dataSource");
            });
        }
        else {
            commRecords = $('#CommContainer').dxDataGrid('instance').option("dataSource");
        }
        if (empr_PurchaseBill.rowsCount == detailRecords.length) {
            var modelRecord = {
                Master: masterRecord,
                Detail: detailRecords,
                Commission: commRecords
            };
            return modelRecord;
        }
        else {

            var detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

            $.each(detailRecords, function (index, item) {
                if (!(item.duE_DATE == "" || item.duE_DATE == null || item.duE_DATE == undefined)) {
                    item.duE_DATE = empr_helper.PrepareDate(item.duE_DATE);
                }

                if (!(item.deL_DATE == "" || item.deL_DATE == null || item.deL_DATE == undefined)) {
                    item.deL_DATE = empr_helper.PrepareDate(item.deL_DATE);
                }
            });

            var modelRecord = {
                Master: masterRecord,
                Detail: detailRecords,
                Commission: $('#CommContainer').dxDataGrid('instance').option("dataSource"),
            };
            return modelRecord;
        }
    },

    ValidateBarcode: function () {

        var valid = true;
        var data = empr_PurchaseBill.GetDataToSave();
        if (data.Master.PARTY_CODE == "" && data.Master.ACT_CODE == "") {
            empr_helper.notify("Please Select Party First", 2);
            var valid = false;
            return
        }

        return valid
    },
    ValidateMainInfo: function () {
        ////debugger;
        var valid = true;
        var data = empr_PurchaseBill.GetDataToSave();

        debugger
        //if (
        //    (data.Master.CACT == '' || data.Master.CACT == null || data.Master.CACT == undefined) &&
        //    (data.Master.BACT == '' || data.Master.BACT == null || data.Master.BACT == undefined)
        //) {
        //    empr_helper.notify("Please select atleast one account.", 2);
        //    valid = false;
        //    return valid;
        //}
        if (data.Master.V_DATE == '') {
            empr_helper.notify("Transaction date is required.", 2);
            valid = false;
            return valid;
        }
        if (data.Master.PARTY_CODE == '') {
            empr_helper.notify("Please select Party Type.", 2);
            valid = false;
            return valid;
        }

        //if ((data.Master.CACT == '' || data.Master.CACT == null || data.Master.CACT == undefined) &&
        //    (data.Master.BACT == '' || data.Master.BACT == null || data.Master.BACT == undefined)) {
        //    empr_helper.notify("Please select atleast one account.", 2);
        //    valid = false;
        //    return valid;
        //}

        if (data.Master.CACT && !data.Master.CAMT) {
            empr_helper.notify("Please enter cash amt.", 2);
            valid = false;
            return valid;
        }
        if (data.Master.BACT && !data.Master.BAMT) {
            empr_helper.notify("Please enter bank amt.", 2);
            valid = false;
            return valid;
        }

        data.Detail = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

        ////debugger;
        if (data.Detail.length == 0) {
            empr_helper.notify("Please add items.", 2);
            valid = false;
            return valid;
        }

        $.each(data.Detail, function (index, item) {
            if (item.iteM_CODE == "" || item.iteM_CODE == null || item.iteM_CODE == undefined) {
                empr_helper.notify("Please select Item at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty ItemCode.");
            }
            if (item.qty == "" || item.qty == null || item.qty == undefined) {
                empr_helper.notify("Please enter item quantity at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity.");
            }

            if (item.qty <= 0) {
                empr_helper.notify("Please enter correct item quantity at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity.");
            }

            if (item.qtY2 != "" && item.qtY2 != null && item.qtY2 != undefined && item.qtY2 < 0) {
                empr_helper.notify("Please enter correct item quantity2 at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity2.");
            }

        });

        if (data.Detail.length > Limit && Limit != 0) {
            empr_helper.notify("You can only add  " + Limit + " records.", 2);
            valid = false;
            return valid;
        }

        return valid;
    },
    //Save: function () {
    //    ////debugger;
    //    var dataModel = empr_PurchaseBill.GetDataToSave();
    //    console.log('SaveAttempt', dataModel)
    //    if (dataModel.Master.TRAN_ID == 0
    //        || dataModel.Master.TRAN_ID == null
    //        || dataModel.Master.TRAN_ID == undefined
    //        || dataModel.Master.TRAN_ID == "") {
    //        dataModel.Detail.reverse();
    //    }
    //    ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseBill/Save", function (data) {
    //        empr_helper.notify(data.msg, data.msgType);
    //        if (data.msgType == 1) {
    //            if (dataModel.Master.TRAN_ID == 0
    //                || dataModel.Master.TRAN_ID == null
    //                || dataModel.Master.TRAN_ID == undefined) {
    //                $('#Code').val(data.data.code);
    //                empr_helper.selectedBill = data.data.code;
    //                $('#VOUCHER_NO').val(data.data.voucherNo);
    //            }
    //            if (dataClear == 1) {
    //                //empr_PurchaseBill.GetPurchaseBillDetailByCode(data.data.code);
    //                empr_PurchaseBill.GetPurchaseBillByCode(data.data.code);
    //                $('#BtnDelete').show();
    //            }
    //            else {
    //                $("#SCODE").dxSelectBox("instance")?.option("value", "");
    //                empr_PurchaseBill.ResetForm();
    //            }

    //        }
    //    }, false, true);
    //},

    Save: function () {
        var dataModel = empr_PurchaseBill.GetDataToSave();
        if (dataModel.Master.TRAN_ID == 0
            || dataModel.Master.TRAN_ID == null
            || dataModel.Master.TRAN_ID == undefined
            || dataModel.Master.TRAN_ID == "") {
            dataModel.Detail.reverse();
        }
        ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseBill/Save", function (data) {
            $('#BtnSave').prop('disabled', false).show();
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                if (dataModel.Master.TRAN_ID == 0
                    || dataModel.Master.TRAN_ID == null
                    || dataModel.Master.TRAN_ID == undefined) {
                    $('#Code').val(data.data.code);
                    empr_helper.selectedBill = data.data.code;
                    $('#VOUCHER_NO').val(data.data.voucherNo);
                }
                ////debugger;
                if (dataClear == 1) {
                    empr_PurchaseBill.GetCurrentStockAndEditData(data.data.code);

                    //empr_PurchaseBill.GetPurchaseBillByCode(data.data.code);
                    $('#BtnDelete').show();
                }
                else {
                    $("#SCODE").dxSelectBox("instance")?.option("value", "");
                    empr_PurchaseBill.ResetForm();
                }

            }
        }, false, true);
    },

    GetLastRate: function (bcode) {
        return new Promise((resolve, reject) => {
            var dataModel = empr_PurchaseBill.GetDataToSave();
            dataModel.Master.BARCODE_ID = bcode;
            dataModel.bcode = bcode;

            ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseBill/GetLastRate", function (data) {
                if (data.msgType == 1 && data.data && data.data.length > 0) {
                    resolve(data.data[0].rate);
                } else {
                    resolve(0);
                }
            }, function (err) {
                resolve(0); // Error ki surat mein 0 return karein taake flow na ruke
            });
        });
    },
    ResetForm: function () {
        $('.detailInfo a').tab('show');
        if ($("#CommContainer").data("dxDataGrid")) {
            $("#CommContainer").dxDataGrid("dispose");
            $("#CommContainer").removeData("dxDataGrid");
        }
        empr_PurchaseBill.CreateGrid([{ __KEY__: empr_PurchaseBill.GenerateKey(36), chK1: false, chk: "0", dT_CODE: 0 }]);
        empr_PurchaseBill.CreateCommGrid([{ __KEY__: empr_PurchaseBill.GenerateKey(96), chK1: false, chk: "0", dT_CODE: 0 }]);
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #V_DATE, #hdnDOC').val('');
        $('#hdnDOC').val('');
        $('#BtnDelete').hide();
        $('#REMARKS').val('');
        $('#D_CHARGES').val('');
        $('#DISC').val('');
        $('#CARTAGE').val('');
        //$('#HS_CODE').val('');
        //$('#ASTATUS').dxSelectBox('instance').option('value', 'Y');
        //$('#BTYPE').dxSelectBox('instance').option('value', 'CR');

        $('#Code').val();
        $('#BARCODE').val('');
        $('.nav-item.commission').hide();
        empr_PurchaseBill.pickIds = [];
        empr_PurchaseBill.firstClick = 0;
        empr_PurchaseBill.CommissionTranId = 0;
        empr_helper.selectedBill = 0;
        empr_PurchaseBill.isSalesman = false;
        empr_PurchaseBill.partyCode = 0;
        empr_PurchaseBill.actCode = 0;
        //empr_PurchaseBill.CreateGrid([]);
        empr_PurchaseBill.InitPartyType();
        empr_PurchaseBill.InitCurrencyDDL();
        empr_PurchaseBill.InitBankAccount();
        empr_PurchaseBill.InitCashAccount();
        empr_PurchaseBill.InitCommissionAmtDDL("PR");
        $('#PARTY_CODE').dxSelectBox('instance').option('value', '');
        //$('#CACT').dxSelectBox('instance').option('value', '');
        //$('#BACT').dxSelectBox('instance').option('value', '');

        $('#REF').focus();
        $('#pickItems').show();
        $('#V_DATE').val(todayDate);
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/PurchaseBill/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    $("#SCODE").dxSelectBox("instance")?.option("value", "");
                    empr_PurchaseBill.ResetForm();
                    $('#BtnDelete').hide();
                }
            }, false, true);
        });
    },
    UploadDoc: function () {
        $('#BtnSave').prop('disabled', true);
        var files = document.getElementById('DOC').files;
        var formData = new FormData();
        for (var i = 0; i !== files.length; i++) {
            formData.append("model", files[i]);
        }
        $.ajax(
            {
                url: "/Common/UploadVoucherDocs",
                data: formData,
                processData: false,
                contentType: false,
                type: "POST",
                success: function (data) {
                    if (data.msgType == '1') {
                        $("#hdnDOC").val(data.data);
                    }
                    else {
                        empr_helper.notify("Something went wrong while saving the file. please re-upload the file.", data.msgType);
                    }
                    $('#BtnSave').prop('disabled', false);

                }
            }
        );
    },
    OpenDoc: function () {
        var hdnUrl = $('#hdnDOC').val();
        if (hdnUrl == "" || hdnUrl == null) {
            empr_helper.notify("Please upload a file to view.", 2);
        }
        else {
            const fileURL = window.location.origin + hdnUrl;
            window.open(fileURL, '_blank');
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
    InitPartyType: function () {
        ////debugger;
        empr_PurchaseBill.bindDxDdl("PARTY_CODE", PartyType, null, "key", "value", "Select", function (d) {
            empr_PurchaseBill.OnPartyChange(d);
        });

    },

    OnPartyChange: function (d) {
        console.log(d)
        if (d.value == null || d.value == '') {
            $('#partyhidden').val('');
            $('#acthidden').val('');
            $('#TERMS').val('');
            $('#PARTY_BALANCE').val('').css('color', '');
            empr_PurchaseBill.InitSalesman(0);
        }
        else {
            var filteredData = $.grep(PartyType, function (item) {
                return item.key === d.value;
            });

            var filteredDataOld = $.grep(PartyTypeOld, function (item) {
                return item.key === d.value;
            });
            console.log("Party:" + filteredData);
            $('#partyhidden').val(filteredData[0].partyCode)
            $('#acthidden').val(filteredData[0].accountCode)
            //$('#TERMS').val(filteredData[0].paymentTerms);  001

            // item behalf on party
            empr_PurchaseBill.ItemsOnParty = [];
            empr_PurchaseBill.ItemsBehalfOnParty(filteredData[0].partyCode, filteredData[0].accountCode);



            var code = $('#Code').val();

            if (code) {

                ajaxHelper.ajaxGetJson('/PurchaseBill/GetPartyCurrentBalance?vDate=' + empr_PurchaseBill.vDate + '&partyCode=' + filteredData[0].partyCode + '&accountCode=' + filteredData[0].accountCode, function (data) {
                    
                    if (data.length > 0) {
                        var newPartyData = $.grep(data, function (item) {
                            return item.key === d.value;
                        });

                        let balance = parseFloat(newPartyData[0].balance || 0);

                        if (balance < 0) {
                            $('#PARTY_BALANCE').val(`(${Math.abs(balance)})`).css('color', 'red');
                        } else {
                            $('#PARTY_BALANCE').val(balance).css('color', 'black');
                        }
                    }
                }, false, true);
            }
            else {
                let balance = parseFloat(filteredDataOld[0].balance);
                let formattedBalance = Math.abs(balance).toLocaleString('en-US');
                if (balance < 0) {
                    $('#PARTY_BALANCE')
                        .val('(' + formattedBalance + ')')
                        .css('color', 'red');
                } else {
                    $('#PARTY_BALANCE')
                        .val(formattedBalance)
                        .css('color', '');
                }
            }



            if (filteredData[0].partyCode != "" && filteredData[0].partyCode != 0) {
                empr_PurchaseBill.InitSalesman(filteredDataOld[0].partyCode, parseInt(filteredDataOld[0].scode));
            }
        }
    },

    ItemsBehalfOnParty: function (PARTY_CODE, ACT_CODE) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/ItemsBehalfOnParty?partyCode=' + PARTY_CODE + '&actCode=' + ACT_CODE, function (data) {
            debugger;
            if (data.length > 0) {
                empr_PurchaseBill.ItemsOnParty = data;

                var grid = $('#DetailContainer').dxDataGrid('instance');

                grid.columnOption('iteM_CODE', 'lookup', {
                    dataSource: data,
                    displayExpr: 'value',
                    valueExpr: 'key',
                    searchEnabled: true,
                    allowClearing: true,
                    showClearButton: true,
                    paging: {
                        enabled: true,
                        pageSize: 50
                    }
                });

                grid.refresh();  
            } else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    InitItemIds: function (dataSource, selectedValue) {
        $('#PICK_ITEM').dxSelectBox({
            dataSource: {
                store: ItemIds,
                paginate: true,
                pageSize: 50
            },
            paging: {
                enabled: true,
                pageSize: 50,
            },
            displayExpr: 'itemId',
            valueExpr: 'key',
            value: selectedValue,
            searchEnabled: true,
            width: '100%',
            placeholder: 'Select',
            showClearButton: true,
            dropDownOptions: {
                height: 'auto',
            },
            pagingEnabled: true,
            searchTimeout: 500,
            onValueChanged: function (d) {
                if (d.value == null || d.value == '') {
                    $('#itemIdHidden').val('');
                }
                else {
                    $('#itemIdHidden').val(d.value)
                }
            },
        });
    },
    InitSalesman: function (Id, selectedValue = null) {
        ////debugger;
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetSalesmanByParty?id=' + Id, function (data) {
            //console.log("Salesman Data:", data);
            if (data.data.table.length > 0) {
                empr_PurchaseBill.bindDxDdl("SCODE", data.data.table, selectedValue, "partY_CODE", "partY_NAME", "Select", function (selected) {

                    if (selected && selected.selectedRowsData && selected.selectedRowsData.length > 0) {
                        var value = selected.selectedRowsData[0]['partY_CODE'];
                        var displayName = selected.selectedRowsData[0]['partY_NAME'];

                        $('#salesmanhidden').val(value);
                        $('#salesmanNameHidden').val(displayName);
                    } else {
                        $('#salesmanhidden').val('');
                        $('#salesmanNameHidden').val('');
                    }
                });
                if (selectedValue != 0 && selectedValue != null) {
                    $('#SCODE').dxSelectBox('instance').option('value', selectedValue);
                }
            }
            else {
                ajaxHelper.ajaxGetJson('/PurchaseBill/GetSalesmanByParty?id=' + 0, function (data) {
                    //console.log("Salesman Data:", data);
                    empr_PurchaseBill.bindDxDdl("SCODE", data.data.table, selectedValue, "partY_CODE", "partY_NAME", "Select", function (selected) {

                        if (selected && selected.selectedRowsData && selected.selectedRowsData.length > 0) {
                            var value = selected.selectedRowsData[0]['partY_CODE'];
                            var displayName = selected.selectedRowsData[0]['partY_NAME'];

                            $('#salesmanhidden').val(value);
                            $('#salesmanNameHidden').val(displayName);
                        } else {
                            $('#salesmanhidden').val('');
                            $('#salesmanNameHidden').val('');
                        }
                    });
                });
            }
        });
    },
    bindDxDdl: function (divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun) {

        ati_dxHelper.createDropdownSingle(divId, data, selectedvalues, keyExp, dataField, placeholder, onvalueChangeFun);

    },
    InitReportTypeDDL: function (selectedValue) {
        ajaxHelper.ajaxGetJson("/PurchaseBill/GetReportTypes", function (data) {
            if (data.msgType == 1) {
                if (data.data.length > 0) {
                    selectedValue = data.data[0].mD_ID;
                }
                $('#ReportType').dxSelectBox({
                    dataSource: data.data,
                    displayExpr: 'mD_NAME',
                    valueExpr: 'mD_ID',
                    value: selectedValue,
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
                    },
                });
            }
            else {
                empr_helper.notify(data.data, data.msgType);
            }
        }, false, true);
    },
    GeneratePrintReport: function () {
        //debugger
        empr_PurchaseBill.InitReportTypeDDL();
        let TRAN_ID = empr_helper.selectedBill;
        let V_DATE = empr_PurchaseBill.vdate;
        let PARTY_CODE = empr_PurchaseBill.partyCode;
        let ACT_CODE = empr_PurchaseBill.actCode;
        let MD_ID = 0;
        let reportName = "";
        //debugger;
        //let MD_ID = $('#ReportType').dxSelectBox('option', 'value');
        let selectedItem = $('#ReportType').dxSelectBox('option', 'selectedItem');
        if (selectedItem) {
            MD_ID = selectedItem.mD_ID;
            reportName = selectedItem.reporT_NAME;
        }
        if (TRAN_ID == 0 || TRAN_ID == null || TRAN_ID == undefined || TRAN_ID == "") {
            empr_helper.notify("Please open the bill in edit mode.", 2);
            return;
        }
        var dataModel = {
            TRAN_ID: TRAN_ID,
            MD_ID: MD_ID,
            REPORT_NAME: reportName,
            V_DATE: V_DATE,
            PARTY_CODE: PARTY_CODE,
            ACT_CODE: ACT_CODE,
        }
        ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseBill/GetPrintReport", function (data) {
            if (data.msgType == 1) {
                $('#ModalBody').empty();
                setTimeout(function () {
                    $('#ReportType').dxSelectBox('instance').option('value', MD_ID);
                    $('#ModalBody').html("<center><object id='objReport' data='" + window.location.origin + data.data + "' width='1100' height='600'></object></center>");
                    $('#ShowReportModal').show();
                    $('#ShowReportModal').modal('show');
                }, 100);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    AddSodaToDelivery: function () {
        if ($('#SodaPickDetailGridContainer').dxDataGrid('instance').hasEditData()) {
            console.log('method if');
            $('#SodaPickDetailGridContainer').dxDataGrid('instance').saveEditData().done(function () {
                var data = empr_PurchaseBill.GetDataToSave();
                var IsDataAvailableInGrid = false;
                $.each(data.Detail, function (index, item) {
                    if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
                        IsDataAvailableInGrid = true;
                    }
                });
                if (IsDataAvailableInGrid || empr_PurchaseBill.firstClick == 1) {
                    var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource');
                    var selectedSodas = $('#SodaPickDetailGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    var finalData = existingData.concat(selectedSodas);
                    empr_PurchaseBill.pickIds = finalData.map(x => x.picK_ID);
                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);
                }
                else {
                    var selectedSodas = $('#SodaPickDetailGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                    empr_PurchaseBill.pickIds = selectedSodas.map(x => x.picK_ID);
                    console.log(empr_PurchaseBill.pickIds);
                    $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
                    empr_PurchaseBill.firstClick = 1;
                }
                if (empr_PurchaseBill.pickIds.length > 0) {
                    $('#pickItems').hide();
                }
                $('.modal').hide();
            });
        }
        else {
            var data = empr_PurchaseBill.GetDataToSave();
            var IsDataAvailableInGrid = false;
            $.each(data.Detail, function (index, item) {
                if (item.iteM_CODE != "" && item.iteM_CODE != null && item.iteM_CODE != undefined) {
                    IsDataAvailableInGrid = true;
                }
            });
            if (IsDataAvailableInGrid || empr_PurchaseBill.firstClick == 1) {
                var existingData = $('#DetailContainer').dxDataGrid('instance').option('dataSource') || [];
                var selectedSodas = $('#SodaPickDetailGridContainer').dxDataGrid('instance').getSelectedRowKeys();
                var finalData = existingData.concat(selectedSodas);
                empr_PurchaseBill.pickIds = finalData.map(x => x.picK_ID_D);
                $('#DetailContainer').dxDataGrid('instance').option('dataSource', finalData);

            }
            else {
                var selectedSodas = $('#SodaPickDetailGridContainer').dxDataGrid('instance').getSelectedRowKeys();

                var filteredData = $.grep(PartyType, function (item) {
                    return item.partyCode === selectedSodas[0].spartY_CODE && item.accountCode === selectedSodas[0].sacT_CODE.toString();
                });
                if (filteredData.length > 0)
                    $('#PARTY_CODE').dxSelectBox('instance').option('value', filteredData[0].key);
                ////debugger;
                $('#COMM_AMT').dxSelectBox('instance').option('value', selectedSodas[0].comM_AMT);
                $("#COMM").val(selectedSodas[0].comm);
                $("#COMM_VAL").val(selectedSodas[0].comM_VAL);
                $('#Currency').dxSelectBox('instance').option('value', selectedSodas[0].crR_CODE);
                $("#Rate").val(selectedSodas[0].crate);
                empr_PurchaseBill.pickIds = selectedSodas.map(x => x.picK_ID_D);
                console.log('Pick data sent', selectedSodas);
                $('#DetailContainer').dxDataGrid('instance').option('dataSource', selectedSodas);
                empr_PurchaseBill.firstClick = 1;
            }
            if (empr_PurchaseBill.pickIds.length > 0) {
                $('#pickItems').hide();
            }
            $('.modal').hide();
        }
    },
    InitBankAccount: function (_selectedValue) {
        function toggleAmountReadonly(value) {
            if (value == null && value == '' || value == 0) {
                $('#BAMT').prop('readonly', true);
                $('#BAMT').val('');
            } else {
                $('#BAMT').prop('readonly', false);
                $('#BAMT').val('');

            }
        }
        $('#BACT').dxSelectBox({
            dataSource: BankAccounts,
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
            onInitialized: function (e) {
                toggleAmountReadonly(e.component.option("value"));
            },
            onValueChanged: function (e) {
                toggleAmountReadonly(e.value);
            }
            //onValueChanged: function (e) {

            //    if (e.value != '' && e.value != null) {
            //        var items = e.component._dataSource._items;
            //        var item = items.filter(i => i.key == e.value);
            //        if (item.length > 0) {
            //            $('#Rate').val(item[0].rate);
            //        }
            //    }
            //    else {
            //        $('#Rate').val('');
            //    }
            //},
        });
    },
    InitCashAccount: function (_selectedValue) {

        function toggleAmountReadonly(value) {
            debugger;
            if (value == null || value == '' || value == 0) {
                $('#CAMT').prop('readonly', true);
                $('#CAMT').val('');
            } else {
                $('#CAMT').prop('readonly', false);
                $('#CAMT').val('');
            }
        }
        $('#CACT').dxSelectBox({
            dataSource: CashAccounts,
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
            onInitialized: function (e) {
                toggleAmountReadonly(e.component.option("value"));
            },
            onValueChanged: function (e) {
                toggleAmountReadonly(e.value);
            }
            //onValueChanged: function (e) {

            //    if (e.value != '' && e.value != null) {
            //        var items = e.component._dataSource._items;
            //        var item = items.filter(i => i.key == e.value);
            //        if (item.length > 0) {
            //            $('#Rate').val(item[0].rate);
            //        }
            //    }
            //    else {
            //        $('#Rate').val('');
            //    }
            //},
        });
    },
    InitCurrencyDDL: function (_selectedValue) {
        $.ajax({
            url: 'CashReceiptVoucher/GetCurrencies',
            method: 'GET',
            success: function (data) {
                ////debugger;
                if (data.msgType == 1) {
                    console.log('GetCurrencies', data.data);
                    if (_selectedValue == undefined || _selectedValue == null) {
                        _selectedValue = data.data[0].key;
                        $('#Rate').val(data.data[0].rate);
                    }
                    $('#Currency').dxSelectBox({
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

                            if (e.value != '' && e.value != null) {
                                var items = e.component._dataSource._items;
                                var item = items.filter(i => i.key == e.value);
                                if (item.length > 0) {
                                    $('#Rate').val(item[0].rate);
                                }
                            }
                            else {
                                $('#Rate').val('');
                            }
                        },
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

    InitCommissionAmtDDL: function (selectedValue) {

        var dataSource = empr_helper.commType;

        $('#COMM_AMT').dxSelectBox({
            dataSource: dataSource,
            displayExpr: 'value',
            valueExpr: 'key',
            value: selectedValue,
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
                if (e.value === 'RS') {
                    $('#COMM').prop('disabled', true);
                    $('#COMM').val('');
                    $('#COMM_VAL').prop('disabled', false).removeAttr('readonly');
                } else {
                    $('#COMM').prop('disabled', false);
                    $('#COMM_VAL').prop('disabled', true).attr('readonly', true);
                    $('#COMM_VAL').val('');
                }
            },
        });
    },


    CalculateCommition: function () {
        var grid = $("#DetailContainer").dxDataGrid("instance");
        var visibleRows = grid.getVisibleRows(); // Get latest UI data

        var amt = visibleRows.reduce(function (sum, row) {
            return sum + (parseFloat(row.data.neT_AMT) || 0);
        }, 0);

        console.log("Total net_amt:", amt);

        var commissionType = $("#COMM_AMT").dxSelectBox("option", "value");

        let comm = parseFloat($('#COMM').val());
        let commVal = parseFloat($('#COMM_VAL').val());

        if (isNaN(amt) || amt <= 0) return;

        if (commissionType == 'PR') {
            let calcCommVal = (amt * comm) / 100;
            $('#COMM_VAL').val(calcCommVal.toFixed(2));
        }
        else if (commissionType == 'RS') {
            let calcComm = (commVal * 100) / amt;
            $('#COMM').val(calcComm.toFixed(2));
        }

        //if (!isNaN(comm)) {
        //    let calcCommVal = (amt * comm) / 100;
        //    $('#COMM_VAL').val(calcCommVal.toFixed(2));
        //}

        //if (!isNaN(commVal)) {
        //    let calcComm = (commVal * 100) / amt;
        //    $('#COMM').val(calcComm.toFixed(2));
        //}

    },

    calculateNetAmtTotal: function () {
        var grid = $("#DetailContainer").dxDataGrid("instance");
        var visibleRows = grid.getVisibleRows(); // Get latest UI data

        var totalNetAmt = visibleRows.reduce(function (sum, row) {
            return sum + (parseFloat(row.data.neT_AMT) || 0);
        }, 0);

        console.log("Total net_amt:", totalNetAmt);

        // Store in global variable if needed
        //window.totalNetAmt = totalNetAmt;

        //// Optional: Show in DOM
        //$("#netAmtTotal").text(totalNetAmt.toFixed(2));
    },

    GetCurrentStock: function () {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetCurrentStock', function (data) {
            ////debugger;
            if (data.length > 0) {
                empr_PurchaseBill.CurrentStock = data;
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

    GetCurrentStockAndEditData: function (tranId) {
        ajaxHelper.ajaxGetJson('/PurchaseBill/GetCurrentStock', function (data) {
            ////debugger;
            if (data.length > 0) {
                empr_PurchaseBill.CurrentStock = data;
                empr_PurchaseBill.GetPurchaseBillByCode(tranId);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },

}