var empr_CustomerAttendance = {
    InitEvents: function () {
        $(document).ready(function () {
            empr_CustomerAttendance.ApplyDuration('today');
            empr_CustomerAttendance.GetDataAttendance();

            $('body').on('change', '#DURATION', function () {
                empr_CustomerAttendance.ApplyDuration($(this).val());
            });

            $('body').on('click', '#BtnFilter', function () {
                $("#Loader").show();
                empr_CustomerAttendance.GetDataAttendance();
            });

            $('body').on('click', '#BtnDownload', function () {
                $("#Loader").show();
                empr_CustomerAttendance.SyncAndGetData();
            });
        });
    },
    formatDateInput: function (date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    },
    ApplyDuration: function (duration) {
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        var fromDate = new Date(today);
        var toDate = new Date(today);

        if (duration === 'yesterday') {
            fromDate.setDate(today.getDate() - 1);
            toDate.setDate(today.getDate() - 1);
        } else if (duration === 'week') {
            var day = today.getDay();
            var diffToMonday = day === 0 ? -6 : 1 - day;
            fromDate.setDate(today.getDate() + diffToMonday);
            toDate = new Date(fromDate);
            toDate.setDate(fromDate.getDate() + 6);
        } else if (duration === 'month') {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }

        $("#FROM_DATE").val(empr_CustomerAttendance.formatDateInput(fromDate));
        $("#TO_DATE").val(empr_CustomerAttendance.formatDateInput(toDate));
    },
    SyncAndGetData: function () {
        var FROM_DATE = $("#FROM_DATE").val();
        var TO_DATE = $("#TO_DATE").val();

        empr_helper.fromDate = FROM_DATE;
        empr_helper.toDate = TO_DATE;

        ajaxHelper.ajaxGetJson('/CustomerAttendance/SyncAndGetData?fromDate=' + FROM_DATE + '&toDate=' + TO_DATE, function (data) {
            if (data.msgType == 1) {
                empr_CustomerAttendance.CreateGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
            $("#Loader").hide();
        }, false, true);
    },
    GetDataAttendance: function () {
        var FROM_DATE = $("#FROM_DATE").val();
        var TO_DATE = $("#TO_DATE").val();

        empr_helper.fromDate = FROM_DATE;
        empr_helper.toDate = TO_DATE;

        ajaxHelper.ajaxGetJson('/CustomerAttendance/GetDataAttendance?fromDate=' + FROM_DATE + '&toDate=' + TO_DATE, function (data) {
            if (data.msgType == 1) {
                empr_CustomerAttendance.CreateGrid(data.data);
            }
            else {
                empr_helper.notify(data.msg, data.msgType);
            }
            $("#Loader").hide();
        }, false, true);
    },
    CreateGrid: function (dataSrc) {
        var col = [
            { dataField: 'userid', caption: 'Identity', width: '110', cssClass: 'text-center' },
            { dataField: 'partY_NAME', caption: 'Employee Name', width: '700', cssClass: 'text-center' },
            { dataField: 'date', caption: 'Date', dataType: 'date', format: 'dd-MM-yyyy', cssClass: 'text-center' },
            { dataField: 'time', caption: 'Time', cssClass: 'text-center' },
        ];

        $('#gridContainer').dxDataGrid({
            dataSource: dataSrc,
            columns: col,
            remoteOperations: false,
            height: 500,
            rowAlternationEnabled: true,
            columnAutoWidth: true,
            allowColumnResizing: true,
            showBorders: true,
            headerFilter: {
                visible: true,
                search: {
                    enabled: false
                }
            },
            paging: {
                pageSize: 300,
                enabled: true
            },
            pager: {
                visible: true,
                allowedPageSizes: [100, 200, 300, 'all'],
                showPageSizeSelector: true,
                showInfo: true,
                showNavigationButtons: true
            },
            searchPanel: {
                visible: true,
                highlightCaseSensitive: true,
                width: 240,
                placeholder: 'Search...'
            },
            filterRow: {
                visible: false
            },
            scrolling: {
                mode: 'both'
            },
            columnFixing: {
                enabled: true
            },
            export: {
                enabled: true,
                formats: ['excel', 'pdf'],
                allowExportSelectedData: false
            },
            onExporting: function (e) {
                var fileName = empr_helper.reportName || 'Customer Attendance';
                e.cancel = true;

                if (e.format === 'pdf') {
                    $("#Loader").show();

                    var grid = e.component;
                    var visibleCols = grid.getVisibleColumns().map(function (c) {
                        return {
                            dataField: c.dataField,
                            caption: c.caption || c.dataField
                        };
                    });

                    var groupColsCap = grid.getVisibleColumns()
                        .filter(function (c) { return c.groupIndex !== undefined && c.groupIndex >= 0; })
                        .sort(function (a, b) { return a.groupIndex - b.groupIndex; })
                        .map(function (c) { return c.caption || c.dataField; });

                    var groupCols = (grid.option('columns') || [])
                        .filter(function (c) { return c.groupIndex !== undefined && c.groupIndex >= 0; })
                        .sort(function (a, b) { return a.groupIndex - b.groupIndex; })
                        .map(function (c) { return c.dataField; });

                    var data = (grid.option('dataSource') || []).map(function (row) {
                        var filteredRow = {};
                        visibleCols.forEach(function (col) {
                            filteredRow[col.caption] = row[col.dataField];
                        });
                        return filteredRow;
                    });

                    var requestGrids = [{
                        GridTitle: fileName,
                        GridData: data,
                        GroupColumnsCap: groupColsCap,
                        GroupColumns: groupCols,
                        Totals: {},
                        IsLandscape: false
                    }];

                    var pdfWindow = window.open('', '_blank');
                    if (pdfWindow) {
                        pdfWindow.document.write("<p style='font-family:sans-serif; text-align:center; margin-top:20%;'>Generating Your PDF, please wait...</p>");
                    }

                    $.ajax({
                        url: '/Report/GeneratePDF',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({
                            CompanyName: empr_helper.companyName,
                            ReportName: fileName,
                            lastDate: null,
                            lastAmount: null,
                            From: empr_helper.formatDateTopdf(empr_helper.fromDate),
                            To: empr_helper.formatDateTopdf(empr_helper.toDate),
                            IsLandscape: false,
                            MenuId: empr_helper.menuId || null,
                            Grids: requestGrids
                        }),
                        xhrFields: { responseType: 'blob' },
                        timeout: 300000,
                        success: function (result) {
                            var url = URL.createObjectURL(result);
                            if (pdfWindow && !pdfWindow.closed) {
                                pdfWindow.location.href = url;
                            } else {
                                window.open(url, '_blank');
                            }
                            $("#Loader").hide();
                        },
                        error: function () {
                            if (pdfWindow) pdfWindow.close();
                            alert('PDF generation failed!');
                            $("#Loader").hide();
                        }
                    });
                } else {
                    var workbook = new ExcelJS.Workbook();
                    var worksheet = workbook.addWorksheet(fileName);

                    DevExpress.excelExporter.exportDataGrid({
                        component: e.component,
                        worksheet: worksheet,
                        autoFilterEnabled: true
                    }).then(function () {
                        workbook.xlsx.writeBuffer().then(function (buffer) {
                            saveAs(new Blob([buffer], { type: 'application/octet-stream' }), fileName + '.xlsx');
                        });
                    });
                }
            }
        });
    }
}
