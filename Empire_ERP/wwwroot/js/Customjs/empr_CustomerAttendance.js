var empr_CustomerAttendance = {
    InitEvents: function () {
        $(document).ready(function () {
            empr_CustomerAttendance.GetDataAttendance();

            $('body').on('click', '#BtnDownload', function () {

                $("#Loader").show();
                empr_CustomerAttendance.SyncAndGetData();

                
            });

        });
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
            { dataField: 'userid', caption: 'Identity', width: '110', cssClass: 'text-center', },
            { dataField: 'partY_NAME', caption: 'Employee Name', width: '700', cssClass: 'text-center', },
            { dataField: 'date', caption: 'Date', dataType: 'date', format: 'dd-MM-yyyy', cssClass: 'text-center', },
            { dataField: 'time', caption: 'Time', cssClass: 'text-center', },
        ];
        empr_helper.DxGridBindingForReportsWithSetting_Aging('#gridContainer', col, dataSrc, "PartyOpening");
    },
    
}