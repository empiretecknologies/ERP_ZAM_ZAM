var empr_SalaryGenerate = {
    isGenerated: false,
    attendanceChart: null,
    hidePopoverTimer: null,
    initEvents: function () {

        $(document).ready(function () {

            empr_SalaryGenerate.CreateGrid([]);
            empr_SalaryGenerate.loadData();

            $('#BtnRefresh').click(function () {
                empr_SalaryGenerate.loadData();
            });

            $('#BtnGenerate').click(function () {
                if (Permissions != "Admin") {
                    if (!Permissions.r_ADD && !Permissions.r_EDIT) {
                        empr_helper.notify("You are not allowed to generate salary !", 2);
                        return;
                    }
                }
                if (empr_SalaryGenerate.isGenerated) {
                    empr_helper.notify("Salary already generated for this month.", 2);
                    return;
                }
                if (empr_SalaryGenerate.validateForm()) {
                    empr_SalaryGenerate.generateAttempt();
                }
            });

            $(document).on('mouseenter', '.sg-badge-present', function (e) {
                var $el = $(this);
                empr_SalaryGenerate.showAttendancePopover($el, {
                    empName: $el.data('empname') || 'Employee',
                    present: parseFloat($el.data('present')) || 0,
                    absent: parseFloat($el.data('absent')) || 0,
                    weeklyOff: parseFloat($el.data('weeklyoff')) || 0,
                    working: parseFloat($el.data('working')) || 0
                }, e);
            });

            $(document).on('mousemove', '.sg-badge-present', function (e) {
                empr_SalaryGenerate.positionAttendancePopover(e);
            });

            $(document).on('mouseleave', '.sg-badge-present', function () {
                empr_SalaryGenerate.hideAttendancePopover();
            });

            if (Permissions != "Admin") {
                (!Permissions.r_ADD && !Permissions.r_EDIT) && $('#BtnGenerate').hide();
                !Permissions.r_VIEW && $('#gridContainer').hide();
            }
        });
    },
    getSelectedPeriod: function () {
        var month = parseInt($('#SALARY_MONTH').val(), 10);
        var year = parseInt($('#SALARY_YEAR').val(), 10);
        if (!month || !year) {
            return null;
        }
        var fromDate = new Date(year, month - 1, 1);
        var toDate = new Date(year, month, 0);
        var pad = function (n) { return (n < 10 ? '0' : '') + n; };
        return {
            month: month,
            year: year,
            FROM_DATE: fromDate.getFullYear() + '-' + pad(fromDate.getMonth() + 1) + '-' + pad(fromDate.getDate()),
            TO_DATE: toDate.getFullYear() + '-' + pad(toDate.getMonth() + 1) + '-' + pad(toDate.getDate())
        };
    },
    validateForm: function () {
        var period = empr_SalaryGenerate.getSelectedPeriod();
        if (!period) {
            empr_helper.notify("Please select Month and Year.", 2);
            return false;
        }
        return true;
    },
    setGenerateButtonState: function () {
        if (empr_SalaryGenerate.isGenerated) {
            $('#BtnGenerate').prop('disabled', true).attr('title', 'Salary already generated for this month');
        } else {
            $('#BtnGenerate').prop('disabled', false).attr('title', 'Generate salary for selected month');
        }
    },
    bindResponse: function (data) {
        var rows = [];
        if (data.data) {
            if ($.isArray(data.data)) {
                rows = data.data;
            } else if (data.data.rows) {
                rows = data.data.rows;
                empr_SalaryGenerate.isGenerated = !!data.data.isGenerated;
            }
        }
        empr_SalaryGenerate.CreateGrid(rows || []);
        empr_SalaryGenerate.setGenerateButtonState();
    },
    loadData: function () {
        if (!empr_SalaryGenerate.validateForm()) {
            return;
        }

        var period = empr_SalaryGenerate.getSelectedPeriod();
        ajaxHelper.ajaxGetJson('/SalaryGenerate/Load?FROM_DATE=' + period.FROM_DATE + '&TO_DATE=' + period.TO_DATE, function (data) {
            if (data.msgType == 1) {
                empr_SalaryGenerate.bindResponse(data);
            } else {
                empr_SalaryGenerate.CreateGrid([]);
                empr_SalaryGenerate.isGenerated = false;
                empr_SalaryGenerate.setGenerateButtonState();
                empr_helper.notify(data.msg, data.msgType);
            }
        }, false, true);
    },
    showLoader: function () {
        $('#salaryGenLoader').addClass('active');
        $('#salaryGenPercent').text('0%');
        $('#salaryGenRing').css('--sg-progress', 0);
    },
    hideLoader: function () {
        $('#salaryGenLoader').removeClass('active');
    },
    runProgress: function (durationMs, onComplete) {
        var start = Date.now();
        var timer = setInterval(function () {
            var elapsed = Date.now() - start;
            var pct = Math.min(100, Math.round((elapsed / durationMs) * 100));
            $('#salaryGenPercent').text(pct + '%');
            $('#salaryGenRing').css('--sg-progress', pct);
            if (pct >= 100) {
                clearInterval(timer);
                if (typeof onComplete === 'function') {
                    onComplete();
                }
            }
        }, 50);
        return timer;
    },
    generateAttempt: function () {

        var period = empr_SalaryGenerate.getSelectedPeriod();
        var obj = {
            FROM_DATE: period.FROM_DATE,
            TO_DATE: period.TO_DATE
        };

        var apiDone = false;
        var progressDone = false;
        var apiResult = null;

        var finishIfReady = function () {
            if (!apiDone || !progressDone) {
                return;
            }
            empr_SalaryGenerate.hideLoader();
            if (apiResult) {
                if (apiResult.data) {
                    empr_SalaryGenerate.bindResponse(apiResult);
                }
                empr_helper.notify(apiResult.msg, apiResult.msgType);
            }
        };

        empr_SalaryGenerate.showLoader();
        empr_SalaryGenerate.runProgress(5000, function () {
            progressDone = true;
            finishIfReady();
        });

        ajaxHelper.ajaxPostJsonData(obj, "/SalaryGenerate/Generate", function (data) {
            apiDone = true;
            apiResult = data;
            finishIfReady();
        }, false, true);
    },
    showAttendancePopover: function ($el, stats, event) {
        if (empr_SalaryGenerate.hidePopoverTimer) {
            clearTimeout(empr_SalaryGenerate.hidePopoverTimer);
            empr_SalaryGenerate.hidePopoverTimer = null;
        }

        $('#sgAttendanceTitle').text((stats.empName || 'Employee') + ' - Attendance');
        $('#sgAttendancePopover').show();
        empr_SalaryGenerate.positionAttendancePopover(event);

        var chartData = [
            { type: 'Present', value: stats.present },
            { type: 'Absent', value: stats.absent },
            { type: 'Weekly Off', value: stats.weeklyOff }
        ];

        if (empr_SalaryGenerate.attendanceChart) {
            empr_SalaryGenerate.attendanceChart.option('dataSource', chartData);
            return;
        }

        empr_SalaryGenerate.attendanceChart = $('#sgAttendanceChart').dxPieChart({
            type: 'doughnut',
            palette: ['#2e7d32', '#c62828', '#ef6c00'],
            dataSource: chartData,
            series: [{
                argumentField: 'type',
                valueField: 'value',
                label: {
                    visible: true,
                    connector: { visible: true },
                    format: 'fixedPoint',
                    customizeText: function (arg) {
                        return arg.argumentText + ': ' + arg.valueText;
                    }
                }
            }],
            legend: {
                visible: true,
                horizontalAlignment: 'center',
                verticalAlignment: 'bottom',
                itemTextPosition: 'right'
            },
            tooltip: {
                enabled: true,
                customizeTooltip: function (arg) {
                    return {
                        text: arg.argumentText + ': ' + arg.valueText + ' days'
                    };
                }
            },
            size: {
                height: 180
            }
        }).dxPieChart('instance');
    },
    positionAttendancePopover: function (event) {
        var $pop = $('#sgAttendancePopover');
        var left = event.clientX + 16;
        var top = event.clientY + 16;
        var popW = $pop.outerWidth() || 260;
        var popH = $pop.outerHeight() || 220;

        if (left + popW > window.innerWidth - 12) {
            left = event.clientX - popW - 16;
        }
        if (top + popH > window.innerHeight - 12) {
            top = event.clientY - popH - 16;
        }

        $pop.css({ left: left + 'px', top: top + 'px' });
    },
    hideAttendancePopover: function () {
        empr_SalaryGenerate.hidePopoverTimer = setTimeout(function () {
            $('#sgAttendancePopover').hide();
        }, 120);
    },
    CreateGrid: function (dataSrc) {

        var col = [{
            dataField: "Action",
            caption: "Profile",
            width: 70,
            alignment: 'center',
            fixed: true,
            fixedPosition: "left",
            allowExporting: false,
            allowGrouping: false,
            cellTemplate: function (container, options) {
                var html = '<div class="btn-group btn-group-sm">';
                if (options.data.emP_PIC != null && options.data.emP_PIC != '' && options.data.emP_PIC != undefined) {
                    html += `<a href="javascript:;" class="grid-action-icon" title="View Pic" onclick="ShowImage('/images/upload/empregistration/${options.data.emP_PIC}')"><i class="fa fa-eye"></i></a>`;
                } else {
                    html += `<span class="text-muted"><i class="fa fa-user"></i></span>`;
                }
                html += '</div>';
                $(html).appendTo(container);
            }
        },
            { dataField: 'machinE_ID', caption: 'Machine ID', width: 100, alignment: 'center' },
            { dataField: 'emP_NAME', caption: 'Employee Name', minWidth: 160 },
            { dataField: 'designation', caption: 'Designation', minWidth: 130 },
            { dataField: 'department', caption: 'Department', minWidth: 130 },
            { dataField: 'payrolL_MONTH', caption: 'Payroll Month', width: 120, alignment: 'center' },
            {
                dataField: 'presenT_DAYS',
                caption: 'Present Days',
                width: 110,
                alignment: 'center',
                cellTemplate: function (container, options) {
                    var present = options.data.presenT_DAYS || 0;
                    var absent = options.data.absenT_DAYS || 0;
                    var weeklyOff = options.data.sundaY_DAYS || 0;
                    var working = options.data.workinG_DAYS || 0;
                    var empName = options.data.emP_NAME || '';
                    $('<span>')
                        .addClass('sg-badge sg-badge-present')
                        .attr({
                            'data-present': present,
                            'data-absent': absent,
                            'data-weeklyoff': weeklyOff,
                            'data-working': working,
                            'data-empname': empName,
                            title: 'Hover to view attendance chart'
                        })
                        .text(present)
                        .appendTo(container);
                }
            },
            {
                dataField: 'sundaY_DAYS',
                caption: 'Weekly Off Days',
                width: 120,
                alignment: 'center',
                cellTemplate: function (container, options) {
                    $('<span>').addClass('sg-badge sg-badge-off').text(options.data.sundaY_DAYS || 0).appendTo(container);
                }
            },
            {
                dataField: 'workinG_DAYS',
                caption: 'Total Working Days',
                width: 130,
                alignment: 'center',
                cellTemplate: function (container, options) {
                    $('<span>').addClass('sg-badge sg-badge-working').text(options.data.workinG_DAYS || 0).appendTo(container);
                }
            },
            {
                dataField: 'absenT_DAYS',
                caption: 'Absent Days',
                width: 110,
                alignment: 'center',
                cellTemplate: function (container, options) {
                    $('<span>').addClass('sg-badge sg-badge-absent').text(options.data.absenT_DAYS || 0).appendTo(container);
                }
            },
            { dataField: 'grosS_SALARY', caption: 'Gross Salary', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'absenT_DEDUCTION', caption: 'Absence Deduction Salary', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'totaL_ADVANCE', caption: 'Advance', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'loaN_INSTALLMENT', caption: 'Loan Installment', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'totaL_LATE_HOURS', caption: 'Total Late Hours', alignment: 'center' },
            { dataField: 'avG_IN_TIME', caption: 'Average In-Time', alignment: 'center' },
            {
                dataField: 'neT_SALARY',
                caption: 'Net Salary Payable',
                format: { type: 'fixedPoint', precision: 2 },
                alignment: 'right',
                cellTemplate: function (container, options) {
                    var val = options.value == null ? 0 : options.value;
                    $('<span>').addClass('sg-net').text(Number(val).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })).appendTo(container);
                }
            },
        ];

        empr_helper.dxGridbindingVouchers('#gridContainer', col, dataSrc, "SalaryGenerate", "none", {
            selection: { mode: 'none', showCheckBoxesMode: 'none' },
            groupPanel: { visible: false }
        });

        // Extra safety: hide grid refresh only on this form
        setTimeout(function () {
            $('#gridContainer .dx-datagrid-header-panel .dx-icon-refresh').closest('.dx-item').hide();
        }, 50);
    },
}
