var empr_PurchaseOrder = {
    totalCount: 0,
    rowsCount: 0,
    InitEvents: function () {
        $(document).ready(function () {
            empr_PurchaseOrder.InitQuickSearchGrid();
            empr_PurchaseOrder.InitReportTypeDDL();
            empr_PurchaseOrder.ResetForm();
            window.addEventListener('message', function (event) {
                if (event.origin !== window.location.origin) {
                    return;
                }
                var data = event.data;
                if (data && data.traN_ID) {
                    $('#Code').val(data.traN_ID);
                    empr_PurchaseOrder.GetPurchaseOrderByCode(data.traN_ID);
                }
            });

            $('body').on('click', '.elm_print', function () {
                empr_helper.selectedBill = $(this).attr("reportid");
                empr_PurchaseOrder.GeneratePrintReport();
            });

            $('body').on('click', '#BtnPrint, #BtnGenerateReport', function () {
                empr_PurchaseOrder.GeneratePrintReport();
            });

            $('body').on('click', '.elm_edit', function () {
                var id = $(this).attr("reportid");
                $('#Code').val(id);
                $('.vHide').show();
                empr_helper.selectedBill = id;
                $('.modal').modal('hide');
                empr_PurchaseOrder.GetPurchaseOrderByCode(id);
            });

            $('body').on('click', '.elm_copy', function () {
                var id = $(this).attr("reportid");
                var date = $(this).attr("reportdate");
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
                ajaxHelper.ajaxPostJsonData({ traN_ID: empr_helper.selectedBill, v_DATE: $('#updatedDate').val() }, "/PurchaseOrder/CopyRecord", function (data) {
                    empr_helper.notify(data.msg, data.msgType);
                    if (data.msgType == 1) {
                        $('.modal').modal('hide');
                        empr_PurchaseOrder.GetPurchaseOrderByCode(data.data.code);
                    }
                }, false, true);
            });

            $('body').on('click', '#BtnSave', function () {
                if (Permissions != "Admin") {
                    if (!$("#Code").val() && !Permissions.r_ADD) {
                        empr_helper.notify("You are not allowed to add new record !", 2);
                    }
                    else if (($("#Code").val() > 0) && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to edit records !", 2);
                    } else {
                        if (empr_PurchaseOrder.ValidateMainInfo()) {
                            empr_PurchaseOrder.Save();
                        }
                    }
                } else {
                    if (empr_PurchaseOrder.ValidateMainInfo()) {
                        empr_PurchaseOrder.Save();
                    }
                }
            });

            $('body').on('click', '#BtnDelete', function () {
                empr_PurchaseOrder.Delete();
            });

            $('body').on('click', '#BtnNew', function () {
                empr_PurchaseOrder.ResetForm();
            });

            $('body').on('click', '#BtnQuickSearch', function () {
                empr_PurchaseOrder.InitQuickSearchGrid();
            });

            if (Permissions != "Admin") {
                !Permissions.r_ADD && $('#BtnNew').hide();
                !Permissions.r_VIEW && $('#BtnQuickSearch').hide();
                !Permissions.r_PRINT && $('.btn-print').hide();
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnSave').hide();
            }
        });
    },
    CreateGrid: function (dataSrc) {
        console.log('CreateGrid');
        console.log(dataSrc);
        if (dataSrc.length > 0) {
            empr_PurchaseOrder.rowsCount = dataSrc.length - 1;
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
                            : `<a href="javascript:;" class="grid-action-icon Clone" onclick="empr_PurchaseOrder.CloneRow(${options.rowIndex})" title="Duplicate"><i class="fa fa-clone"></i></a>`;
                        const addAction = (!Permissions.r_ADD && !Permissions.r_EDIT)
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Add" style="margin-left: 8px" onclick="empr_PurchaseOrder.AddRow()" title="Add"><i class="fa fa-add"></i></a>`;
                        const deleteAction = !Permissions.r_DLT
                            ? ''
                            : `<a href="javascript:;" class="grid-action-icon Delete" style="margin-left: 8px" onclick="empr_PurchaseOrder.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>`;
                        const actions = `<div class="btn-group btn-group-sm">${copyAction}${addAction}${deleteAction}</div>`;
                        $(actions).appendTo(container);
                    } else {
                        $(`<div class="btn-group btn-group-sm">
                           <a href="javascript:;" class="grid-action-icon" onclick="empr_PurchaseOrder.CloneRow(`+ options.rowIndex +`)" title="Duplicate"><i class="fa fa-clone"></i></a>
                           <a href="javascript:;" class="grid-action-icon" style="margin-left: 8px" onclick="empr_PurchaseOrder.AddRow()" title="Add"><i class="fa fa-add"></i></a>
                           <a href="javascript:;" class="grid-action-icon" style="margin-left: 8px" onclick="empr_PurchaseOrder.DeleteRow(${options.rowIndex},${options.data.dT_CODE})" title="Delete"><i class="fa fa-trash"></i></a>
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
                dataField: 'iteM_CODE',
                caption: 'Item',
                width: 150,
                allowSorting: false,
                fixed: true,
                fixedPosition: "left",
                lookup: {
                    dataSource: Items,
                    displayExpr: 'value',
                    valueExpr: 'key'

                },
            },
            //{
            //    dataField: 'partY_DDL',
            //    caption: 'Party Name',
            //    width: 250,
            //    allowSorting: false,
            //    lookup: {
            //        dataSource: Parties,
            //        displayExpr: 'value',
            //        valueExpr: 'customizedKey',
            //        allowClearing: true
            //    },
            //    setCellValue: function (newData, value, currentRowData) {
            //        var selectedParty = Parties.filter(u => u.customizedKey == value);
            //        if (selectedParty.length > 0) {
            //            newData.partY_DDL = selectedParty[0].customizedKey;
            //            newData.partY_CODE = selectedParty[0].key;
            //            newData.acT_CODE = selectedParty[0].accountCode;
            //        } else {
            //            newData.partY_DDL = null;
            //            newData.partY_CODE = null;
            //            newData.acT_CODE = null;
            //        }
            //    }
            //},
            //{
            //    dataField: 'ordeR_NO',
            //    caption: 'Order No',
            //},
            //{
            //    dataField: 'qty',
            //    caption: 'Quantity',
            //    setCellValue: function (newData, value, currentRowData) {
            //        newData.qty = value;
            //        var qty = parseFloat(newData.qty) || 0;
            //        var qtY2 = parseFloat(currentRowData.qtY2) || 1;
            //        var rate = parseFloat(currentRowData.rate) || 0;
            //        var tax = parseFloat(currentRowData.taX_AMT) || 0;
            //        var disc = parseFloat(currentRowData.disC_AMT) || 0;
            //        if (isNaN(qty)) {
            //            empr_helper.notify("Please enter the correct quantity.", 2);
            //        }
            //        if (isNaN(qtY2)) {
            //            empr_helper.notify("Please enter the correct quantity2.", 2);
            //        }
            //        if (!isNaN(qty) && !isNaN(qtY2)) {
            //            if (currentRowData.chK1) {
            //                newData.baL_QTY = qty * qtY2;
            //                newData.amt = (newData.baL_QTY * rate).toFixed(2);
            //            }
            //            else {
            //                newData.baL_QTY = qty;
            //                newData.amt = (newData.baL_QTY * rate).toFixed(2);
            //            }
            //        }

            //        newData.neT_AMT = (parseFloat(newData.amt) + tax) - disc;
            //    }
            //},
            {
                dataField: 'qty',
                caption: 'Qty',
                width: 75,
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

                            // After newData.neT_AMT is set
                            setTimeout(function () {
                                empr_PurchaseBill.CalculateCommition(); // 👈 Custom function to get updated total
                            }, 0);

                            //setTimeout(function () {
                            //    empr_PurchaseBill.CalculateCommition();
                            //}, 0);
                        }
                    }
                }
            },
            {
                dataField: 'unit',
                caption: 'Unit',
                lookup: {
                    dataSource: Units,
                    displayExpr: 'value',
                    valueExpr: 'key'
                }
            },
            {
                dataField: 'qtY2',
                caption: 'Qunatity 2',
                visible: CompCond == 2,
                setCellValue: function (newData, value, currentRowData) {
                    newData.qtY2 = value;
                    var qty = parseFloat(currentRowData.qty) || 0;
                    var qtY2 = parseFloat(newData.qtY2) || 1;
                    var tax = parseFloat(currentRowData.taX_AMT) || 0;
                    var disc = parseFloat(currentRowData.disC_AMT) || 0;
                    var rate = parseFloat(currentRowData.rate) || 0;
                    if (isNaN(qty)) {
                        empr_helper.notify("Please enter the correct quantity.", 2);
                    }
                    if (isNaN(qtY2)) {
                        empr_helper.notify("Please enter the correct quantity2.", 2);
                    }
                    if (!isNaN(qty) && !isNaN(qtY2)) {
                        if (currentRowData.chK1) {
                            newData.baL_QTY = qty * qtY2;
                            newData.amt = (newData.baL_QTY * rate).toFixed(2);
                        }
                        else {
                            newData.baL_QTY = qty;
                            newData.amt = (newData.baL_QTY * rate).toFixed(2);
                        }
                    }
                    newData.neT_AMT = (parseFloat(newData.amt) + tax) - disc;
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
                    var $checkbox = $("<input type='checkbox'>")
                        .addClass("chkCell")
                        .prop('checked', isChecked)
                        .on('change', function () {
                            var item = options.data;
                            var qty = parseFloat(item.qty) || 0;
                            var tax = parseFloat(item.taX_AMT) || 0;
                            var disc = parseFloat(item.disC_AMT) || 0;
                            var qtY2 = parseFloat(item.qtY2) || 1;
                            var rate = parseFloat(item.rate) || 0;
                            if (isNaN(qty)) {
                                empr_helper.notify("Please enter the correct quantity.", 2);
                            }
                            if (isNaN(qtY2)) {
                                empr_helper.notify("Please enter the correct quantity2.", 2);
                            }
                            if (!isNaN(qty) && !isNaN(qtY2)) {
                                if (this.checked) {
                                    item.baL_QTY = qty * qtY2;
                                    item.chk = "1";
                                    item.amt = (item.baL_QTY * rate).toFixed(2);
                                }
                                else {
                                    item.baL_QTY = qty;
                                    item.chk = "0";
                                    item.amt = (item.baL_QTY * rate).toFixed(2);
                                    console.log(item.amt)
                                }

                                item.neT_AMT = (parseFloat(item.amt) + tax) - disc;
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
            //{
            //    dataField: 'rate',
            //    caption: 'Rate',
            //    setCellValue: function (newData, value, currentRowData) {
            //        newData.rate = value;
            //        var rate = parseFloat(newData.rate) || 0;
            //        var balancedQTY = parseFloat(currentRowData.baL_QTY) || 0;
            //        var tax = parseFloat(currentRowData.taX_AMT) || 0;
            //        var disc = parseFloat(currentRowData.disC_AMT) || 0;

            //        if (!isNaN(balancedQTY) && !isNaN(newData.rate)) {
            //            newData.amt = (balancedQTY * rate).toFixed(2);
            //        }

            //        newData.neT_AMT = (parseFloat(newData.amt) + tax) - disc;
            //    }
            //},
            {
                dataField: 'rate',
                caption: 'Rate',
                width: 80,
                setCellValue: function (newData, value, currentRowData) {
                    newData.rate = value;
                    var rate = parseFloat(newData.rate) || 0;
                    var qty = parseFloat(currentRowData.baL_QTY) || 0;
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
                allowEditing: false,
            },
            {
                dataField: 'disc',
                caption: 'Discount',
                setCellValue: function (newData, value, currentRowData) {
                    newData.disc = value;
                    var disc = parseFloat(newData.disc) || 0;
                    var tax = parseFloat(currentRowData.taX_AMT) || 0;
                    var amt = parseFloat(currentRowData.amt) || 0;

                    if (!isNaN(disc)) {
                        newData.disC_AMT = (amt * disc)/100;
                    }
                    newData.neT_AMT = (amt + tax) - parseFloat(newData.disC_AMT);
                }
            },
            {
                dataField: 'disC_AMT',
                caption: 'Discount Amt',
                allowEditing: false,
            },
            {
                dataField: 'tax',
                caption: 'Tax',
                setCellValue: function (newData, value, currentRowData) {
                    newData.tax = value;
                    var tax = parseFloat(newData.tax) || 0;
                    var disc = parseFloat(currentRowData.disC_AMT) || 0;
                    var amt = parseFloat(currentRowData.amt) || 0;

                    if (!isNaN(tax)) {
                        newData.taX_AMT = (amt * tax) / 100;
                    }
                    newData.neT_AMT = (amt + parseFloat(newData.taX_AMT)) - disc;
                }
            },
            {
                dataField: 'taX_AMT',
                caption: 'Tax Amt',
                allowEditing: false,
            },
            {
                dataField: 'neT_AMT',
                caption: 'Net Amt',
                allowEditing: false,
            },
            {
                dataField: 'dT_DESC',
                caption: 'Description',
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
            {
                dataField: 'grade',
                caption: 'Grade',
                lookup: {
                    dataSource: Grades,
                    displayExpr: 'value',
                    valueExpr: 'key'
                }
            },
            {
                dataField: 'priority',
                caption: 'Priority',
                lookup: {
                    dataSource: [
                        { key: 'N', value: 'Normal' },
                        { key: 'U', value: 'Urgent' },
                        { key: 'M', value: 'Most Urgent' },
                    ],
                    displayExpr: 'value',
                    valueExpr: 'key'
                }
            },
            {
                dataField: 'deL_DATE',
                caption: 'Delivery Date',
                dataType: 'date',
                format: 'yyyy-MM-dd',
                editorOptions: {
                    displayFormat: 'yyyy-MM-dd',
                }
            },
            //{
            //    dataField: 'shiP_DATE',
            //    caption: 'Shipment Date',
            //    dataType: 'date',
            //    format: 'yyyy-MM-dd',
            //    editorOptions: {
            //        displayFormat: 'yyyy-MM-dd',
            //    }
            //},
            //{
            //    dataField: 'pod',
            //    caption: 'Port of Discharge',
            //},
            {
                dataField: 'reQ_TYPE',
                caption: 'Request Type',
                lookup: {
                    dataSource: [
                        { key: 'E', value: 'External' },
                        { key: 'I', value: 'Internal' },
                    ],
                    displayExpr: 'value',
                    valueExpr: 'key'
                }
            },
        ];
        empr_helper.editableDxGridbindingForTransactions('#DetailContainer', col, dataSrc, "AccountOpening", "iteM_CODE");
        if (dataSrc.length == 0) {
            $('#DetailContainer').dxDataGrid('instance').addRow().done(function () {
                $('#DetailContainer').dxDataGrid('instance').saveEditData();
            });
        }

        setTimeout(function () {
            var nextElement = $('#DetailContainer').dxDataGrid('instance').getCellElement(0, 'iteM_CODE');
            $('#DetailContainer').dxDataGrid('instance').focus(nextElement);  
        }, 1500);
    },
    CloneRow: function (index) {
        debugger;
        const gridIns = $('#DetailContainer').dxDataGrid('instance');
        const dataSrc = gridIns.option("dataSource");

        if (dataSrc.length >= Limit && Limit != 0) {
            empr_helper.notify("You can only add  " + Limit + " records.", 2);
            return;
        }
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {

                empr_PurchaseOrder.rowsCount += 1;
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                var dataSource = gridInstance.option("dataSource");
                if (dataSource.length > 0) {
                    //let clonedRowData = dataSource[index];
                    let clonedRowData = $.extend(true, {}, dataSource[index]);
                    //gridInstance.addRow();
                    //$.each(clonedRowData, function (key, value) {
                    //    if (key == 'dT_CODE') {
                    //        gridInstance.cellValue(0, 'dT_CODE', '');
                    //    }
                    //    else {
                    //        gridInstance.cellValue(0, key, value);
                    //    }
                    //});
                    // After adding the row, insert it at the first position
                    //gridInstance.insertRow(clonedRowData, 0); // Assuming you want to insert at the first position
                    //dataSource.unshift(clonedRowData); // Add the cloned row data at the beginning of the dataSource array
                    if (clonedRowData.hasOwnProperty('dT_CODE')) {
                        delete clonedRowData.dT_CODE;
                    }
                    //delete clonedRowData.dT_CODE;
                    clonedRowData.__KEY__ = empr_PurchaseOrder.GenerateKey(36);
                    let newDataSource = [clonedRowData].concat(dataSource);
                    //delete newDataSource[0].dT_CODE;
                    gridInstance.option("dataSource", newDataSource); // Update the grid's dataSource
                    gridInstance.refresh(); // Refresh the grid
                }
            });
        }
        else {
            empr_PurchaseOrder.rowsCount += 1;
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            var dataSource = gridInstance.option("dataSource");
            if (dataSource.length > 0) {
                //let clonedRowData = dataSource[index];
                let clonedRowData = $.extend(true, {}, dataSource[index]);
                //gridInstance.addRow();
                //$.each(clonedRowData, function (key, value) {
                //    if (key == 'dT_CODE') {
                //        gridInstance.cellValue(0, 'dT_CODE', '');
                //    }
                //    else {
                //        gridInstance.cellValue(0, key, value);
                //    }
                //});
                // After adding the row, insert it at the first position
                //gridInstance.insertRow(clonedRowData, 0); // Assuming you want to insert at the first position
                //dataSource.unshift(clonedRowData); // Add the cloned row data at the beginning of the dataSource array
                if (clonedRowData.hasOwnProperty('dT_CODE')) {
                    delete clonedRowData.dT_CODE;
                }
                clonedRowData.__KEY__ = empr_PurchaseOrder.GenerateKey(36);
                //delete clonedRowData.dT_CODE;
                let newDataSource = [clonedRowData].concat(dataSource);
                //delete newDataSource[0].dT_CODE;
                gridInstance.option("dataSource", newDataSource); // Update the grid's dataSource
                gridInstance.refresh(); // Refresh the grid
            }
        }
    },
    AddRow: function () {
        debugger;
        const gridIns = $('#DetailContainer').dxDataGrid('instance');
        const dataSrc = gridIns.option("dataSource");

        if (dataSrc.length >= Limit && Limit != 0) {
            empr_helper.notify("You can only add  " + Limit + " records.", 2);
            return;
        }
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                empr_PurchaseOrder.rowsCount += 1;
                //const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                //gridInstance.addRow();
                const gridInstance = $('#DetailContainer').dxDataGrid('instance');
                const dataSource = gridInstance.option("dataSource");

                dataSource.unshift({ __KEY__ : empr_PurchaseOrder.GenerateKey(36) });
                gridInstance.option("dataSource", dataSource);
                gridInstance.refresh();
            });
        }
        else {
            //empr_PurchaseOrder.rowsCount += 1;
            //const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            //gridInstance.addRow();
            empr_PurchaseOrder.rowsCount += 1;
            //const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            //gridInstance.addRow();
            const gridInstance = $('#DetailContainer').dxDataGrid('instance');
            const dataSource = gridInstance.option("dataSource");

            dataSource.unshift({ __KEY__: empr_PurchaseOrder.GenerateKey(36) });
            gridInstance.option("dataSource", dataSource);
            gridInstance.refresh();
        }
    },
    DeleteRow: function (index, dtCode) {
        debugger;
        const gridInstance = $('#DetailContainer').dxDataGrid('instance');
        var dataSource = gridInstance.option("dataSource");
        if (dataSource.length > 0) {
            if (dataSource.length > 1) {
                var row = dataSource[index];
                if (dtCode == '' || dtCode == null || dtCode == undefined) {
                    gridInstance.deleteRow(index);
                    empr_PurchaseOrder.rowsCount -= 1;
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
                        ajaxHelper.ajaxPostJsonData({ code: dtCode }, "/PurchaseOrder/DeletePurchaseOrderDetailByCode", function (data) {
                            empr_helper.notify(data.msg, data.msgType);
                            if (data.msgType == 1) {
                                gridInstance.deleteRow(index);
                                empr_PurchaseOrder.rowsCount -= 1;
                                gridInstance.saveEditData();
                            }
                        }, false, true);
                    });
                    } else {
                        empr_helper.notify("You are not allowed to delete the last row.", 2);
                    }
                }
            }
            else {
                empr_helper.notify("You are not allowed to delete the last row.", 2);
            }
        }
    },
    dropDownBoxEditorTemplate: function(cellElement, cellInfo) {
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
    InitDepartmentDDL: function (_selectedValue) {
        $.ajax({
            url: 'PurchaseOrder/GetDepartments',
            method: 'GET',
            success: function (data) {

                var selectedvalue = 0;
                var selectedobj = [];
                if (_selectedValue != null) {
                    selectedobj = data.filter(x => x.key == _selectedValue);
                    if (selectedobj.length > 0) {
                        selectedvalue = _selectedValue;
                        $("#department_hidden").val(selectedvalue);
                        $("#displayExpr_department").val(selectedobj[0].value);
                    }
                }
                
                empr_PurchaseOrder.BindDxGridBoxDdl('#DEP', data, [{ dataField: 'key', caption: 'Code', width: '60px' }, { dataField: 'value', caption: 'Name' }, { dataField: 'name', caption: 'Control Name' }], 'hidden', selectedobj, selectedvalue, 'key', 'value', '#displayExpr_department', function (selectedvalue, hidden) {

                    if (selectedvalue.selectedRowsData.length > 0) {
                        var key = selectedvalue.selectedRowsData[0]['key'];
                        var value = selectedvalue.selectedRowsData[0]['value'];
                        var rate = selectedvalue.selectedRowsData[0]['rate'];
                        $('#department_hidden').val(key);
                        $('#displayExpr_department').val(value);
                    }
                    else {
                        $('#department_hidden').val('');
                        $('#displayExpr_department').val('');
                    }
                });
            },
            error: function (error) {
                console.error('Error fetching data:', error);
            }
        });
    },
    BindDxGridBoxDdl: function (divid, datasrc, col, hiddenid, selectedOjb, selectval, valueExpr, displayExpr, displayExprHidden, onchangeFun) {
        ati_dxHelper.DxGridBoxDropdown(divid, datasrc, col, hiddenid, selectedOjb, selectval, valueExpr, displayExpr, displayExprHidden, onchangeFun);
    },
    InitQuickSearchGrid: function () {
        empr_PurchaseOrder.GetPurchaseOrders();
    },
    GetPurchaseOrders: function () {
        ajaxHelper.ajaxGetJson('/PurchaseOrder/GetPurchaseOrders', function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrder.CreateQuickSearchGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
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
                    if (Permissions != "Admin" && !Permissions.r_PRINT) {
                        $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>
                               </div>`).appendTo(container);
                    } else {
                        $(`<div class="btn-group btn-group-sm">
                               <a href="javascript:;"  class="grid-action-icon elm_edit" reportid=${options.data.id} title="Edit"><i class="fa fa-edit"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_print" style="margin-left: 8px" reportid=${options.data.id} title="PRINT"><i class="fa fa-print"></i></a>
                               <a href="javascript:;"  class="grid-action-icon elm_copy" style="margin-left: 8px" reportdate=${options.data.v_DATE} reportid=${options.data.id} title="COPY"><i class="fa fa-copy"></i></a>
                               </div>`).appendTo(container);
                    }
                }
            },
            { dataField: 'id', caption: 'Code', },
            { dataField: 'v_DATE', caption: 'Voucher Date', dataType: 'date', format: 'dd-MM-yyy' },
            { dataField: 'voucheR_NO', caption: 'Voucher No', },
            { dataField: 'ref', caption: 'Reference No', },
            { dataField: 'remarks', caption: 'Remarks', },
            { dataField: 'pod', caption: 'Port of Discharge', },
            { dataField: 'transporT_TYPE', caption: 'Transport Type', },
            { dataField: 'adD_USER_ID', caption: 'Created By', visible: false, },
            { dataField: 'adD_DATE', caption: 'Created Date', dataType: 'date', visible: false, format: 'dd-MM-yyy' },
            { dataField: 'adD_COMPUTER_NAME', caption: 'Created Computer', visible: false, },
            { dataField: 'adD_IP_ADDRESS', caption: 'Created IP', visible: false, },
            { dataField: 'ediT_USER_ID', caption: 'Updated By', visible: false, },
            { dataField: 'ediT_DATE', caption: 'Updated Date', dataType: 'date', visible: false, format: 'dd-MM-yyy' },
            { dataField: 'ediT_COMPUTER_NAME', caption: 'Updated Computer', visible: false, },
            { dataField: 'ediT_IP_ADDRESS', caption: 'Updated IP', visible: false, },
        ];
        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "PurchaseOrderQS");
        setTimeout(function () {
            $('#gridContainer').dxDataGrid('instance').resize();
        }, 500);
    },
    GetPurchaseOrderByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PurchaseOrder/GetPurchaseOrderByCode?code=' + code, function (data) {
            if (data.master.msgType == 1) {
                var masterData = data.master.data;
                if (masterData.length == 1) {
                    var response = masterData[0];
                    $('#Code').val(response.id);
                    $('#ASTATUS').dxSelectBox('instance').option('value', response.astatus);
                    $('#REF').val(response.ref);
                    $('#POD').val(response.pod);
                    $('#TRANSPORT_TYPE').val(response.transporT_TYPE);
                    $('#REMARKS').val(response.remarks);
                    $('#V_DATE').val(response.v_DATE);
                    $('#VOUCHER_NO').val(response.voucheR_NO);
                    $('#REF_DATE').val(response.reF_DATE);
                    $('#CRATE').val(response.crate);
                    $('#DEL_DATE').val(response.deL_DATE);
                    $('#hdnDOC').val(response.doc);
                    empr_PurchaseOrder.InitDepartmentDDL(response.dep);
                    //$('#ASTATUS').dxSelectBox('instance').option('value', 'Y');
                    empr_PurchaseOrder.InitDropdownsWithValue(response.ordeR_TYPE, response.comM_AMT, response.comM_TYPE, response.curR_CODE, response.warehouse, response.partY_CODE, response.scode);
                    $('#COMM').val(response.comm);

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
                    empr_PurchaseOrder.CreateGrid(data.detail.data);
                    $('.card-body').addClass('customHighlightForModifiedCells');
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
    GetPurchaseOrderDetailByCode: function (code) {
        ajaxHelper.ajaxGetJson('/PurchaseOrder/GetPurchaseOrderDetailByCode?code=' + code, function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrder.CreateGrid(data.data);
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
        var PARTY_CODE = $('#PARTY_CODE').dxSelectBox('option', 'value');
        var DEP = $("#department_hidden").val();
        var ORDER_TYPE = $('#ORDER_TYPE').dxSelectBox('option', 'value');
        var TERMS = $("#TERMS").val();
        var SCODE = $('#SCODE').dxSelectBox('option', 'value');
        var COMM_AMT = $('#COMM_AMT').dxSelectBox('option', 'value');
        var COMM = $("#COMM").val();
        var COMM_TYPE = $('#COMM_TYPE').dxSelectBox('option', 'value');
        var REF = $("#REF").val();
        // var POD = $("#POD").val(); ammar
        var TRANSPORT_TYPE = $("#TRANSPORT_TYPE").val();
        var REF_DATE = $("#REF_DATE").val();
        var CURR_CODE = $('#CURR_CODE').dxSelectBox('option', 'value');
        var CRATE = $("#CRATE").val();
        var WAREHOUSE = $('#WAREHOUSE').dxSelectBox('option', 'value');
        // var DEL_DATE = $("#DEL_DATE").val(); ammar
        var REMARKS = $("#REMARKS").val();
        var ASTATUS = $('#ASTATUS').dxSelectBox('option', 'value');
        var DOC = $("#hdnDOC").val();
        var masterRecord = {
            TRAN_ID: ID,
            V_DATE: V_DATE,
            VOUCHER_NO: VOUCHER_NO,
            PARTY_CODE: PARTY_CODE,
            DEP: DEP,
            ORDER_TYPE: ORDER_TYPE,
            TERMS: TERMS,
            SCODE: SCODE,
            COMM_AMT: COMM_AMT,
            COMM: COMM,
            COMM_TYPE: COMM_TYPE,
            REF: REF,
            // POD: POD, ammar
            TRANSPORT_TYPE: TRANSPORT_TYPE,
            REF_DATE: REF_DATE,
            CURR_CODE: CURR_CODE,
            CRATE: CRATE,
            WAREHOUSE: WAREHOUSE,
            // DEL_DATE: DEL_DATE, ammar
            REMARKS: REMARKS,
            ASTATUS: ASTATUS,
            DOC: DOC
        }
        var detailRecords = [];
        if ($('#DetailContainer').dxDataGrid('instance').hasEditData()) {
            $('#DetailContainer').dxDataGrid('instance').saveEditData().done(function () {
                detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");
            });
        }
        else{
            detailRecords = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

            detailRecords.forEach(item => {
                item.deL_DATE = empr_PurchaseOrder.formatDateForServer(item.deL_DATE);
                item.shiP_DATE = empr_PurchaseOrder.formatDateForServer(item.shiP_DATE);
            });
        }
        if (empr_PurchaseOrder.rowsCount == detailRecords.length) {
            console.log(detailRecords);
            var modelRecord = {
                Master: masterRecord,
                Detail: detailRecords
            };
            return modelRecord;
        }
        else {
            var modelRecord = {
                Master: masterRecord,
                Detail: $('#DetailContainer').dxDataGrid('instance').option("dataSource")
            };
            return modelRecord;
        }
    },
    formatDateForServer: function (date) {
        if (!date) return null;
        const d = new Date(date);
        const year = d.getFullYear();
        const month = ('0' + (d.getMonth() + 1)).slice(-2);
        const day = ('0' + d.getDate()).slice(-2);
        return `${year}-${month}-${day}`;
    },
    ValidateMainInfo: function () {

        var valid = true;
        var data = empr_PurchaseOrder.GetDataToSave();

        if (data.Master.V_DATE == '') {
            empr_helper.notify("Transaction date is required.", 2);
            valid = false;
            return valid;
        }

        if (data.Master.PARTY_CODE == '' || data.Master.PARTY_CODE == null) {
            empr_helper.notify("Party Name is required.", 2);
            valid = false;
            return valid;
        }

        if (data.Master.ASTATUS == '' || data.Master.ASTATUS == null) {
            empr_helper.notify("Status is required.", 2);
            valid = false;
            return valid;
        }

        if (data.Master.DEP == '') {
            empr_helper.notify("Please select department.", 2);
            valid = false;
            return valid;
        }

        data.Detail = $('#DetailContainer').dxDataGrid('instance').option("dataSource");

        if (data.Detail.length == 0) {
            empr_helper.notify("Please add items.", 2);
            valid = false;
            return valid;
        }

        $.each(data.Detail, function (index, item) {
            // Check if Item_Code or Quantity is empty
            if (item.iteM_CODE == "" || item.iteM_CODE == null || item.iteM_CODE == undefined) {
                empr_helper.notify("Please select item at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty ItemCode.");
                // Do something here, such as marking the item or taking appropriate action
            }
            if (item.qty == "" || item.qty == null || item.qty == undefined) {
                empr_helper.notify("Please enter item quantity at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity.");
                // Do something here, such as marking the item or taking appropriate action
            }

            if (item.qty <= 0) {
                empr_helper.notify("Please enter correct item quantity at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity.");
                // Do something here, such as marking the item or taking appropriate action
            }

            if (item.qtY2 != "" && item.qtY2 != null && item.qtY2 != undefined && item.qtY2 < 0) {
                empr_helper.notify("Please enter correct item quantity2 at index " + index, 2);
                valid = false;
                return valid;
                console.log("Item at index " + index + " has empty Quantity2.");
                // Do something here, such as marking the item or taking appropriate action
            }

        });

        return valid;
    },
    Save: function () {
        debugger;
        var dataModel = empr_PurchaseOrder.GetDataToSave();
        console.log(dataModel);
        if (dataModel.Master.TRAN_ID == 0
            || dataModel.Master.TRAN_ID == null
            || dataModel.Master.TRAN_ID == undefined
            || dataModel.Master.TRAN_ID == "") {
            dataModel.Detail.reverse();
        }
        ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseOrder/Save", function (data) {
            empr_helper.notify(data.msg, data.msgType);
            if (data.msgType == 1) {
                if (dataModel.Master.TRAN_ID == 0
                    || dataModel.Master.TRAN_ID == null
                    || dataModel.Master.TRAN_ID == undefined) {
                    $('#Code').val(data.data.code);
                    empr_helper.selectedBill = data.data.code;
                    $('#VOUCHER_NO').val(data.data.voucherNo);
                    $('.vHide').show();
                }
                if (dataClear == 1) {
                    empr_PurchaseOrder.GetPurchaseOrderDetailByCode($('#Code').val());
                    $('#BtnDelete').show();
                }
                else {
                    empr_PurchaseOrder.ResetForm();
                }

                
            }
        }, false, true);
    },
    ResetForm: function () {

        //$('.Record input').not('#Code, #ASTATUS, .dx-texteditor-input, #BTYPE, #ACT_CODE, #PARTYTYPE_CODE, #SALES_CODE, #SALESACT_CODE, #DEFAULT_DC_TYPE').val('');
        $('.Record input').not('#ASTATUS, .dx-texteditor-input, #WAREHOUSE, #V_DATE, #hdnDOC').val('');
        $('#hdnDOC').val('');
        $('#REMARKS').val('');
        $('.vHide').hide();
        $('#BtnDelete').hide();
        //$('#ASTATUS').dxSelectBox('instance').option('value', 'Y');
        $('.card-body').removeClass('customHighlightForModifiedCells');

        empr_PurchaseOrder.CreateGrid([]);
        empr_PurchaseOrder.InitDepartmentDDL();
        empr_PurchaseOrder.InitDropdowns();
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
            ajaxHelper.ajaxPostJsonData({ code: $('#Code').val() }, "/PurchaseOrder/Delete", function (data) {
                empr_helper.notify(data.msg, data.msgType);
                if (data.msgType == 1) {
                    empr_PurchaseOrder.ResetForm();
                    $('#BtnDelete').hide();
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
    InitDropdowns: function () {
        debugger;
        empr_PurchaseOrder.InitOrderTypeDDL("LOC");
        empr_PurchaseOrder.InitCommissionAmtDDL("RS");
        empr_PurchaseOrder.InitCommissionTypeDDL("E");
        empr_PurchaseOrder.InitCurrencyDDL();
        empr_PurchaseOrder.InitWarehouseDDL();
        
        ajaxHelper.ajaxGetJson("/PurchaseOrder/GetParties", function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrder.InitPartyCodeDDL(data.data);
                empr_PurchaseOrder.InitSalesmanCodeDDL(data.data);
            }
            else {
                empr_helper.notify(data.data, data.msgType);
            }
        }, false, true);
    },
    InitDropdownsWithValue: function (OrderType, CommissionAmt, CommissionType, Currency, Warehouse, PartyCode, SalesmanCode) {
        empr_PurchaseOrder.InitOrderTypeDDL(OrderType);
        empr_PurchaseOrder.InitCommissionAmtDDL(CommissionAmt);
        empr_PurchaseOrder.InitCommissionTypeDDL(CommissionType);
        empr_PurchaseOrder.InitCurrencyDDL(parseInt(Currency));
        empr_PurchaseOrder.InitWarehouseDDL(parseInt(Warehouse));
        
        ajaxHelper.ajaxGetJson("/PurchaseOrder/GetParties", function (data) {
            if (data.msgType == 1) {
                empr_PurchaseOrder.InitPartyCodeDDL(data.data, PartyCode);
                empr_PurchaseOrder.InitSalesmanCodeDDL(data.data, SalesmanCode);
            }
            else {
                empr_helper.notify(data.data, data.msgType);
            }
        }, false, true);
    },
    InitPartyCodeDDL: function (dataSource, selectedValue) {
        $('#PARTY_CODE').dxSelectBox({
            dataSource: dataSource,
            displayExpr: 'value',
            valueExpr: 'customizedKey',
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
                $("#TERMS").val(dataSource.filter(t => t.customizedKey == e.value)[0].paymentTerms);
            },
        });
    },
    InitSalesmanCodeDDL: function (dataSource, selectedValue) {
        console.log(dataSource)
        $('#SCODE').dxSelectBox({
            dataSource: dataSource,
            displayExpr: 'value',
            valueExpr: 'customizedKey',
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
                //$("#COMM").val(dataSource.filter(t => t.customizedKey == e.value)[0].commission);
            },
        });
    },
    InitOrderTypeDDL: function (selectedValue) {
        var dataSource = [
            { key: 'IMP', value: 'Import' },
            { key: 'EXP', value: 'Export' },
            { key: 'LOC', value: 'Local' }
        ];

        $('#ORDER_TYPE').dxSelectBox({
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
        });
    },
    InitCommissionAmtDDL: function (selectedValue) {
        var dataSource = [
            { key: 'PR', value: 'Percent' },
            { key: 'RS', value: 'Value' }
        ];

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
        });
    },
    InitCommissionTypeDDL: function (selectedValue) {
        var dataSource = [
            { key: 'I', value: 'Include' },
            { key: 'E', value: 'Exclude' }
        ];

        $('#COMM_TYPE').dxSelectBox({
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
        });
    },
    InitCurrencyDDL: function (_selectedValue) {
        $.ajax({
            url: 'CashReceiptVoucher/GetCurrencies',
            method: 'GET',
            success: function (data) {
                console.log(data.data)
                if (data.msgType == 1) {
                    if (_selectedValue == undefined || _selectedValue == null) {
                        _selectedValue = data.data[0].key;
                        $('#CRATE').val(data.data[0].rate);
                    }
                    $('#CURR_CODE').dxSelectBox({
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
                                    $('#CRATE').val(item[0].rate);
                                }
                            }
                            else {
                                $('#CRATE').val('');
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
    InitWarehouseDDL: function (_selectedValue) {
        $.ajax({
            url: 'PurchaseOrder/GetWarehouses',
            method: 'GET',
            success: function (data) {
                if (data.msgType == 1) {
                    $('#WAREHOUSE').dxSelectBox({
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
    InitReportTypeDDL: function (selectedValue) {
        ajaxHelper.ajaxGetJson("/PurchaseOrder/GetReportTypes", function (data) {
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
    //GeneratePrintReport: function () {
    //    empr_PurchaseOrder.InitReportTypeDDL();
    //    let TRAN_ID = empr_helper.selectedBill;
    //    let MD_ID = $('#ReportType').dxSelectBox('option', 'value');
    //    if (TRAN_ID == 0 || TRAN_ID == null || TRAN_ID == undefined || TRAN_ID == "") {
    //        empr_helper.notify("Please open the bill in edit mode.", 2);
    //        return;
    //    }
    //    var dataModel = {
    //        TRAN_ID: TRAN_ID,
    //        MD_ID: MD_ID,
    //    }
    //    ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseOrder/GetPrintReport", function (data) {
    //        if (data.msgType == 1) {
    //            $('#ModalBody').empty();
    //            setTimeout(function () {
    //                $('#ModalBody').html("<center><object id='objReport' data='" + window.location.origin + data.data + "' width='1100' height='600'></object></center>");
    //                $('#ShowReportModal').show();
    //                $('#ShowReportModal').modal('show');
    //            }, 100);
    //        }
    //        else {
    //            empr_helper.notify(data.msg, data.msgType);
    //        }
    //    }, false, true);
    //},
    GeneratePrintReport: function () {
        debugger
        empr_PurchaseOrder.InitReportTypeDDL();
        let TRAN_ID = empr_helper.selectedBill;
        let MD_ID = 0;
        let reportName = "";
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
        }
        ajaxHelper.ajaxPostJsonData(dataModel, "/PurchaseOrder/GetPrintReport", function (data) {
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
}