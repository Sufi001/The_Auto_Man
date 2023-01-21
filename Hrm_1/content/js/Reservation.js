//----------------------------------------------------Reservation Section Start-----------------------------------------------------
var roomSatus = [];
var oldDate = "";
var SelectedRoom_Code = "";
function SaveData() {
    $('#btn_save').attr('disabled', 'disabled');
    var validationException = false;
    var main = {};
    var detailList = [];
    reservationItems = [];
    reservationAmenities = [];
    var vm = {
        main,
        detailList: [],
        reservationItems: [],
        reservationAmenities: []
    };
    vm.main.RES_ID = $("#RES_ID").val();
    vm.main.DOC_DATE = $("#DOC_DATE").val();
    vm.main.SUPL_CODE = $("#SUPL_CODE").val();
    vm.main.FIRST_NAME = $("#FIRST_NAME").val();
    vm.main.LAST_NAME = $("#LAST_NAME").val();
    vm.main.PHONE = $("#PHONE").val();
    vm.main.COUNTRY = $("#ddl_Country").val();
    vm.main.CITY = $("#ddl_City").val();
    vm.main.ADDRESS = $("#ADDRESS").val();
    vm.main.ID_TYPE = $("#ddl_ID_TYPE").val();
    vm.main.ID_NO = $("#ID_NO").val();
    vm.main.E_MAIL = $("#E_MAIL").val();
    vm.main.DOB = $("#DOB").val();
    vm.main.DISCOUNT = $("#DISCOUNT").val();
    vm.main.AMOUNT_PAID = $("#AMOUNT_PAID").val();
    vm.main.TOTAL_AMT = $("#TOTAL_AMT_MAIN").val();
    vm.main.BALANCE = $("#BALANCE").val();
    //vm.main.COMPANY = $("#txt_Payment").val();
    if (vm.main.DOC_DATE == null || vm.main.DOC_DATE == undefined || vm.main.DOC_DATE == "") {
        toastr.error("Select Document Date Type");
        validationException = true;
    }
    if (vm.main.FIRST_NAME == null || vm.main.FIRST_NAME == undefined || vm.main.FIRST_NAME == "") {
        toastr.error("Enter First Name");
        validationException = true;

    }
    if (vm.main.PHONE == null || vm.main.PHONE == undefined || vm.main.PHONE == "") {
        toastr.error("Enter Phone Number");
        validationException = true;

    }
    vm.main.TOTAL_AMT = $("#TOTAL_AMT_MAIN").val();
    if (vm.main.COUNTRY == null || vm.main.COUNTRY == undefined || vm.main.COUNTRY == "") {
        toastr.error("Select Country");
        validationException = true;

    }
    if (vm.main.CITY == null || vm.main.CITY == undefined || vm.main.CITY == "") {
        toastr.error("Select City");
        validationException = true;

    }
    if (vm.main.TOTAL_AMT == null || vm.main.TOTAL_AMT == undefined || vm.main.TOTAL_AMT == "") {
        toastr.error("Total Amount Can't be Null");
        validationException = true;

    }
    if (validationException) {
        $('#btn_save').removeAttr("disabled");
        return;
    };
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var detail = {};
        var td = $(tr[ind]).find('td');
        detail.ROOM_TYPE = td.eq(0).text();
        detail.ROOM_CATEGORY = td.eq(1).text();
        detail.ROOM_CODE = td.eq(2).text();
        //detail.ROOM_NAME = td.eq(3).text();
        detail.CHECKIN_DATETIME = td.eq(4).text();
        detail.ESTIMATED_CHECKOUT_DATETIME = td.eq(5).text();
        detail.CHECKOUT_DATETIME = td.eq(6).text();
        detail.ADULTS = td.eq(7).text();
        detail.CHILDREN = td.eq(8).text();
        detail.RATE_PER_DAY = td.eq(9).text();
        detail.SERVICES_CHARGES = td.eq(10).text();
        detail.OTHER_CHARGES = td.eq(11).text();
        detail.GST = td.eq(12).text();
        detail.TOTAL_AMT = td.eq(13).text();
        detail.STATUS = td.eq(14).text();
        vm.detailList.push(detail);
    }
    if (vm.detailList.length == 0) {
        toastr.error('Select min one item to save.');
        $('#btn_save').removeAttr("disabled");
        return;
    }
    tr = "";
    tr = $("#Inventory_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var item = {};
        var td = $(tr[ind]).find('td');
        item.RES_ID = td.eq(0).text();
        item.ROOM_CODE = td.eq(1).text();
        item.BARCODE = td.eq(2).text();
        item.COMMENTS = td.eq(5).text();
        item.QUANTITY = td.eq(6).text();
        item.AMOUNT = td.eq(7).text();
        item.STATUS = td.eq(9).text();
        vm.reservationItems.push(item);
    }
    tr = "";
    tr = $("#Amenity_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var amanity = {};
        var td = $(tr[ind]).find('td');
        amanity.RES_ID = td.eq(0).text();
        amanity.ROOM_CODE = td.eq(1).text();
        amanity.BARCODE = td.eq(2).text();
        amanity.COMMENTS = td.eq(5).text();
        amanity.DATETIME = td.eq(6).text();
        amanity.AMOUNT = td.eq(7).text();
        amanity.STATUS = td.eq(9).text();
        vm.reservationAmenities.push(amanity);
    }

    $.ajax({
        url: "/Reservation/Save",
        method: "post",
        data: vm
    }).done(function (obj) {
        if (obj.substring(0, 2) == "ex") {
            $('#btn_save').removeAttr("disabled");
            toastr.error("Data Not Save");
        }
        else if (obj == "Wrong") {
            $('#btn_save').removeAttr("disabled");
            toastr.error("Invalid Query");
        }
        else {
            toastr.success('Data saved successfully.');
            $("#RES_ID").val(obj)
            if ($("#STATUS").val() != 3) {
                $('#btn_save').removeAttr("disabled");
                $('#btn_cancel').removeAttr("disabled");
                $('#btn_authorize').removeAttr("disabled");
                $('#add_btn').removeAttr('disabled');
            }
        }
    }).fail(function (xhr) {
        alert('Request Status: \n ' +
            xhr.status +
            ' Status Text: ' +
            xhr.statusText +
            ' ' +
            xhr.responseText);
        $('#btn_save').removeAttr("disabled");
        toastr.error('Data not saved');
    });
}
function showTable()     {
    var counter = $("#tblbody tr").length + 1;
    if (counter == 0) {
        counter = counter + 1;
    }
    if ($("#tblbody tr").length > 0) {
        var lastrowid = ($('#tblbody tr:last').attr('id')).substr(4, 8);
        counter = parseInt(lastrowid) + 1;
    }
    else
        counter = 1;

    var resId = $("#RES_ID").val();
    var roomType = $("#ddl_ROOM_TYPE").val();
    var roomCategory = $("#ddl_ROOM_CATEGORY").val();
    var room = $("#ddl_ROOM").val();
    var roomName = $("#ddl_ROOM option:selected").text();
    var checkIn = $("#CHECKIN_DATETIME").val();
    var eCheckIn = $("#ESTIMATED_CHECKOUT_DATETIME").val();
    var checkOut = $("#CHECKOUT_DATETIME").val();

    checkIn = moment(checkIn).format('DD/MM/YYYY h:mm:ss A');
    eCheckIn = moment(eCheckIn).format('DD/MM/YYYY h:mm:ss A');
    checkOut = moment(checkOut).format('DD/MM/YYYY h:mm:ss A');

    var adults = parseInt($("#ADULTS").val());
    var children = parseInt($("#CHILDREN").val());
    var ratePerDay = parseInt($("#RATE_PER_DAY").val());
    var serviceCharges = parseInt($("#SERVICES_CHARGES").val());
    var otherCharges = parseInt($("#OTHER_CHARGES").val());
    var gst = parseInt($("#GST").val());
    var TotalAmount = parseInt($("#TOTAL_AMT_DETAIL").val());

    if (roomType == null || roomType == undefined || roomType == "") {
        toastr.error("Select Room Type");
        return;
    }
    if (roomCategory == null || roomCategory == undefined || roomCategory == "") {
        toastr.error("Select Room Category");
        return;
    }
    if (room == null || room == undefined || room == "") {
        toastr.error("Select Room");
        return;
    }
    if (checkIn == null || checkIn == undefined || checkIn == "" || checkIn == "Invalid date") {
        toastr.error("Provide CheckIn DateTime");
        return;
    }
    //if (eCheckIn == null || eCheckIn == undefined || eCheckIn == "" || eCheckIn == "Invalid date") {
    //    toastr.error("Provide Estimated CheckIn DateTime");
    //    return;
    //}
    if (checkOut == null || checkOut == undefined || checkOut == "" || checkOut == "Invalid date") {
        toastr.error("Provide CheckOut DateTime");
        return;
    }

    if (isNaN(adults)) {
        toastr.error("Adult Count");
        return;
    }
    if (isNaN(children)) {
        toastr.error("Children Count");
        return;
    }
    if (isNaN(ratePerDay)) {
        toastr.error("Rate Per Day");
        return;
    }

    if (isNaN(adults)) {
        adults = 0;
    }
    if (isNaN(children)) {
        children = 0;
    }
    if (isNaN(ratePerDay)) {
        ratePerDay = 0;
    }
    if (isNaN(serviceCharges)) {
        serviceCharges = 0;
    }
    if (isNaN(otherCharges)) {
        otherCharges = 0;
    }
    if (isNaN(gst)) {
        gst = 0;
    }
    if (isNaN(TotalAmount)) {
        TotalAmount = 0;
    }
    var index = checkDuplicate(room);
    if (index != -1) {
        toastr.error("Room Already Assigned")
    }
    else {
        $("#tblbody").append(
            "<tr id=trrr" + counter + ">" +
            "<td style='display:none;'>" + roomType + "</td>" +
            "<td style='display:none;'>" + roomCategory + "</td>" +
            "<td style='display:none;'>" + room + "</td>" +
            "<td>" + roomName + "</td>" +
            "<td>" + checkIn + "</td>" +
            "<td>" + eCheckIn + "</td>" +
            "<td>" + checkOut + "</td>" +
            "<td>" + adults + "</td>" +
            "<td>" + children + "</td>" +
            "<td>" + ratePerDay + "</td>" +
            "<td>" + serviceCharges + "</td>" +
            "<td>" + otherCharges + "</td>" +
            "<td>" + gst + "</td>" +
            "<td >" + TotalAmount + "</td>" +
            "<td style='display:none;'>" + "" + "</td>" +
            "</tr>");
        $("#tblbody tr").last().append(
            "<td><a onclick='RemoveRow(trrr" +
            counter +
            "," +
            counter +
            ")' class='fa fa-trash' style='color:red'></a>&nbsp&nbsp|&nbsp&nbsp<a onclick='EditRow(trrr" +
            counter +
            "," +
            counter +
            ")' class='fa fa-edit' style='color:blue'></a>&nbsp&nbsp|&nbsp&nbsp<a onclick='AuthorizeSingleReservation(trrr" +
            counter +
            ")' class='fa fa-check-circle' style='color:green'></a></td> "
            //+ 

            //"<td><a onclick='EditRow(trrr" +
            //counter +
            //"," +
            //counter +
            //")' class='fa fa-edit'></a></td>"
            );
        counter++;
        clearRow();
    }
    calculateTotal();
}
function calculateDetailTotal() {
    var ratePerDay = parseInt($("#RATE_PER_DAY").val());
    var serviceCharges = parseInt($("#SERVICES_CHARGES").val());
    var otherCharges = parseInt($("#OTHER_CHARGES").val());
    var gst = parseInt($("#GST").val());

    var TotalAmount = 0;

    if (!isNaN(ratePerDay)) {
        TotalAmount += ratePerDay;
    }
    if (!isNaN(serviceCharges)) {
        TotalAmount += serviceCharges;
    }
    if (!isNaN(otherCharges)) {
        TotalAmount += otherCharges;
    }
    if (!isNaN(gst)) {
        TotalAmount += gst;
    }

    $("#TOTAL_AMT_DETAIL").val(TotalAmount);
}
function checkDuplicate(roomcode) {
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var rcode = td.eq(2).text();
        if (rcode == roomcode) {
            return ind;
        }
    }
    return -1;

}
function clearRow() {
    $("#ddl_ROOM_TYPE").val("");
    $("#ddl_ROOM_CATEGORY").val("");
    $("#ddl_ROOM").val("");
    $("#CHECKIN_DATETIME").val("");
    $("#ESTIMATED_CHECKOUT_DATETIME").val("");
    $("#CHECKOUT_DATETIME").val("");
    $("#ADULTS").val("");
    $("#CHILDREN").val("");
    $("#RATE_PER_DAY").val("");
    $("#SERVICES_CHARGES").val("");
    $("#OTHER_CHARGES").val("");
    $("#GST").val("");
    $("#TOTAL_AMT_DETAIL").val("");
}
function calculateTotal() {
    var totalAmount = 0;
    var discount = $("#DISCOUNT").val();
    var paidAmount = $("#AMOUNT_PAID").val();
    var balance = 0;
    if (isNaN(discount)) {
        discount = 0;
    }
    if (isNaN(paidAmount)) {
        paidAmount = 0;
    }
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        totalAmount += parseInt(td.eq(13).text());
    }
    var tr = $("#Inventory_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        totalAmount += parseInt(td.eq(7).text());
    }
    //var tr = $("#Amenity_tblbody").find('tr');
    //for (var ind = 0; ind < tr.length; ind++) {
    //    var td = $(tr[ind]).find('td');
    //    totalAmount += parseInt(td.eq(7).text());
    //}

    balance = ((totalAmount - paidAmount) - discount)
    $("#TOTAL_AMT_MAIN").val(totalAmount);
    $("#BALANCE").val(balance);
}
function EditRow(id) {
    var d = $(id).find('td');
    $("#ddl_ROOM_TYPE").val(d.eq(0).text());
    $("#ddl_ROOM_CATEGORY").val(d.eq(1).text());
    SelectedRoom_Code = d.eq(2).text();
    var checkIn = d.eq(4).text();
    var eCheckIn = d.eq(5).text();
    var checkOut = d.eq(6).text();

    if (checkIn != null && checkIn != undefined && checkIn != "" && checkIn != "Invalid date") {
        checkIn = DateTimeChangeInEditMode(checkIn);
    }
    if (eCheckIn != null && eCheckIn != undefined && eCheckIn != "" && eCheckIn != "Invalid date") {
        eCheckIn = DateTimeChangeInEditMode(eCheckIn);
    }
    if (checkOut != null && checkOut != undefined && checkOut != "" && checkOut != "Invalid date") {
        checkOut = DateTimeChangeInEditMode(checkOut);
    }

    //$("#CHECKIN_DATETIME").val(d.eq(4).text());
    //$("#ESTIMATED_CHECKOUT_DATETIME").val(d.eq(5).text());
    //$("#CHECKOUT_DATETIME").val(d.eq(6).text());

    $("#CHECKIN_DATETIME").val(checkIn);
    $("#ESTIMATED_CHECKOUT_DATETIME").val(eCheckIn);
    $("#CHECKOUT_DATETIME").val(checkOut);
    LoadRooms();
    $("#ADULTS").val(d.eq(7).text());
    $("#CHILDREN").val(d.eq(8).text());
    $("#RATE_PER_DAY").val(d.eq(9).text());
    $("#SERVICES_CHARGES").val(d.eq(10).text());
    $("#OTHER_CHARGES").val(d.eq(11).text());
    $("#GST").val(d.eq(12).text());
    $("#TOTAL_AMT_DETAIL").val(d.eq(13).text());
    RemoveRow(id);
}
function Authorize() {
    disable4Button();
    var validationException = false;
    var main = {};
    var detailList = [];
    var vm = {
        main,
        detailList: []
    };
    vm.main.RES_ID = $("#RES_ID").val();
    vm.main.DOC_DATE = $("#DOC_DATE").val();
    vm.main.SUPL_CODE = $("#SUPL_CODE").val();
    vm.main.FIRST_NAME = $("#FIRST_NAME").val();
    vm.main.LAST_NAME = $("#LAST_NAME").val();
    vm.main.PHONE = $("#PHONE").val();
    vm.main.COUNTRY = $("#ddl_Country").val();
    vm.main.CITY = $("#ddl_City").val();
    vm.main.ADDRESS = $("#ADDRESS").val();
    vm.main.ID_TYPE = $("#ddl_ID_TYPE").val();
    vm.main.ID_NO = $("#ID_NO").val();
    vm.main.E_MAIL = $("#E_MAIL").val();
    vm.main.DOB = $("#DOB").val();
    vm.main.DISCOUNT = $("#DISCOUNT").val();
    vm.main.AMOUNT_PAID = $("#AMOUNT_PAID").val();
    vm.main.TOTAL_AMT = $("#TOTAL_AMT_MAIN").val();
    vm.main.BALANCE = $("#BALANCE").val();
    //vm.main.COMPANY = $("#txt_Payment").val();
    if (vm.main.DOC_DATE == null || vm.main.DOC_DATE == undefined || vm.main.DOC_DATE == "") {
        toastr.error("Select Document Date Type");
        validationException = true;
    }
    if (vm.main.FIRST_NAME == null || vm.main.FIRST_NAME == undefined || vm.main.FIRST_NAME == "") {
        toastr.error("Enter First Name");
        validationException = true;

    }
    if (vm.main.PHONE == null || vm.main.PHONE == undefined || vm.main.PHONE == "") {
        toastr.error("Enter Phone Number");
        validationException = true;

    }
    vm.main.TOTAL_AMT = $("#TOTAL_AMT_MAIN").val();
    if (vm.main.COUNTRY == null || vm.main.COUNTRY == undefined || vm.main.COUNTRY == "") {
        toastr.error("Select Country");
        validationException = true;

    }
    if (vm.main.CITY == null || vm.main.CITY == undefined || vm.main.CITY == "") {
        toastr.error("Select City");
        validationException = true;

    }
    if (vm.main.TOTAL_AMT == null || vm.main.TOTAL_AMT == undefined || vm.main.TOTAL_AMT == "") {
        toastr.error("Total Amount Can't be Null");
        validationException = true;

    }
    if (validationException) {
        Remove4ButtionAttr();
        return;
    };

    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var detail = {};
        var td = $(tr[ind]).find('td');
        detail.ROOM_TYPE = td.eq(0).text();
        detail.ROOM_CATEGORY = td.eq(1).text();
        detail.ROOM_CODE = td.eq(2).text();
        //detail.ROOM_NAME = td.eq(3).text();
        detail.CHECKIN_DATETIME = td.eq(4).text();
        detail.ESTIMATED_CHECKOUT_DATETIME = td.eq(5).text();
        detail.CHECKOUT_DATETIME = td.eq(6).text();
        detail.ADULTS = td.eq(7).text();
        detail.CHILDREN = td.eq(8).text();
        detail.RATE_PER_DAY = td.eq(9).text();
        detail.SERVICES_CHARGES = td.eq(10).text();
        detail.OTHER_CHARGES = td.eq(11).text();
        detail.GST = td.eq(12).text();
        detail.TOTAL_AMT = td.eq(13).text();
        detail.STATUS = td.eq(14).text();
        vm.detailList.push(detail);
    }
    if (vm.detailList.length == 0) {
        toastr.error('Select min one item to save.');
        Remove4ButtionAttr();
        return;
    }
    tr = "";
    tr = $("#Inventory_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var item = {};
        var td = $(tr[ind]).find('td');
        item.RES_ID = td.eq(0).text();
        item.ROOM_CODE = td.eq(1).text();
        item.BARCODE = td.eq(2).text();
        item.QUANTITY = td.eq(5).text();
        item.AMOUNT = td.eq(6).text();
        vm.reservationItems.push(item);
    }
    tr = "";
    tr = $("#Amenity_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var amanity = {};
        var td = $(tr[ind]).find('td');
        amanity.RES_ID = td.eq(0).text();
        amanity.ROOM_CODE = td.eq(1).text();
        amanity.BARCODE = td.eq(2).text();
        amanity.DATETIME = td.eq(5).text();
        amanity.AMOUNT = td.eq(6).text();
        vm.reservationAmenities.push(amanity);
    }
    $.ajax({
        //url: "/api/AuthorizeReservation/Authorize",
        url: "/Reservation/Authorize",
        method: "post",
        data: vm
    }).done(function (obj) {
        if (obj == "ex") {
            Remove4ButtionAttr();
            toastr.error("Data Could Not Authorize");
        }
        else if (obj == "Wrong") {
            Remove4ButtionAttr();
            toastr.error("Invalid Query");
            toastr.error("Or You Are Tring To Save An Authorized Document");
        }
        else {
            toastr.success('Data saved successfully.');
            $("#STATUS").val(obj)
            if ($("#STATUS").val() == 3) {
                disable4Button();
            }
        }
    }).fail(function (xhr) {
        alert('Request Status: \n ' +
            xhr.status +
            ' Status Text: ' +
            xhr.statusText +
            ' ' +
            xhr.responseText);
        Remove4ButtionAttr();
        toastr.error('Data not saved');
    });
    Events();
}
function Remove4ButtionAttr() {
    $('#btn_save').removeAttr("disabled");
    $('#btn_cancel').removeAttr("disabled");
    $('#btn_authorize').removeAttr("disabled");
    $('#add_btn').removeAttr('disabled');
}
function disable4Button() {
    $('#btn_authorize').attr('disabled', 'disabled');
    $("#add_btn").attr('disabled', 'disabled');
    $('#btn_save').attr('disabled', 'disabled');
    $('#btn_cancel').attr('disabled', 'disabled');
}
function CancelBooking() {
    var reservationId = $("#RES_ID").val();
    if (reservationId != "" && reservationId != null && reservationId > 0) {
        $.ajax({
            url: "/Reservation/CancelBooking?id=" + reservationId,
            method: "get",
        })
          .done(function (obj) {
              if (obj == "OK") {
                  toastr.success("Booking Successfully Canceled");
                  Remove4ButtionAttr();
              }
              else {
                  toastr.error("Booking Cancel Failed");
              }
          })
          .fail(function (xhr) {
              toastr.error("Call Fail");
          });
    }
}
function AuthorizeSingleReservation(id) {
    var d = $(id).find('td');
    var resId = $("#RES_ID").val();
    var roomType = d.eq(0).text();
    var roomCategory = d.eq(1).text();
    var roomCode = d.eq(2).text();
    var roomName = d.eq(3).text();
    var checkIn = d.eq(4).text();
    var eCheckIn = d.eq(5).text();
    var checkOut = d.eq(6).text();
    var adults = d.eq(7).text();
    var children = d.eq(8).text();
    var ratePerDay = d.eq(9).text();
    var serviceCharges = d.eq(10).text();
    var otherCharges = d.eq(11).text();
    var gst = d.eq(12).text();
    var TotalAmount = d.eq(13).text();
    if (resId == "" || resId == null || resId == undefined) {
        toastr.error("You Must Save This Document First");
        return;
    }
    var detail = {};
    detail.RES_ID = resId;
    detail.ROOM_TYPE = roomType;
    detail.ROOM_CODE = roomCode;
    detail.ROOM_NAME = roomName;
    detail.ROOM_CATEGORY = roomCategory;
    detail.CHECKIN_DATETIME = checkIn;
    detail.ESTIMATED_CHECKOUT_DATETIME = eCheckIn;
    detail.CHECKOUT_DATETIME = checkOut;
    detail.ADULTS = adults;
    detail.CHILDREN = children;
    detail.RATE_PER_DAY = ratePerDay;
    detail.SERVICES_CHARGES = serviceCharges;
    detail.OTHER_CHARGES = otherCharges;
    detail.GST = gst;
    detail.TOTAL_AMT = TotalAmount;
    $.ajax({
        url: "/Reservation/AuthrizeSingleReservation",
        method: "post",
        data: detail
    }).done(function (obj) {
        if (obj == "Ex") {
            toastr.error("Data Could Not Authorize");
        }
        else if (obj == "Wrong") {
            toastr.error("Invalid Query \n Or You Are Tring To Save\Update An Authorized Document");
        }
        else {
            d.eq(14).text("3")
            d.eq(15).html("<a onclick='AlertAuthorizeDocument()' class='fa fa-trash'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='AlertAuthorizeDocument()' class='fa fa-edit'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='AlertAuthorizeDocument()' class='fa fa-check-circle'></a>")
            toastr.success('Reservation Authorized.');
        }
    }).fail(function (xhr) {
        alert('Request Status: \n ' +
            xhr.status +
            ' Status Text: ' +
            xhr.statusText +
            ' ' +
            xhr.responseText);
        Remove4ButtionAttr();
        toastr.error('Data not saved');
    });
    Events();
}
function LoadRooms() {
    var resId = $("#RES_ID").val();
    $('#ddl_ROOM').empty();
    $('#ddl_ROOM').append(new Option('Select Room', ''));
    var roomType = $("#ddl_ROOM_TYPE").val();
    var roomCategory = $("#ddl_ROOM_CATEGORY").val();
    var checkIn = $("#CHECKIN_DATETIME").val();
    var checkOut = $("#CHECKOUT_DATETIME").val();
    if (roomType == "") {
        return;
    }
    else if (roomCategory == "") {
        return;
    }
    else if (checkIn == "") {
        return;
    }
    else if (checkOut == "") {
        return;
    }
    else {
        checkIn = moment(checkIn).format('MM/DD/YYYY h:mm:ss A');
        checkOut = moment(checkOut).format('MM/DD/YYYY h:mm:ss A');
        $.ajax({
            url: "/Reservation/GetRooms/?category=" + roomCategory + "&&type=" + roomType + "&&checkIn=" + checkIn + "&&checkOut=" + checkOut + "&&res_Id=" + resId,
            type: "GET",
            dataType: "JSON",
            success: function (_data) {
                var obj = JSON.parse(_data);
                roomSatus = [];
                $.each(obj, function (index, element) {
                     roomSatus.push(element);
                    $('#ddl_ROOM').append(new Option(obj[index].name, obj[index].code));
                });
                $("#ddl_ROOM").val(SelectedRoom_Code);
                SelectedRoom_Code = "";
            },
            error: function (xhr) {
                alert('Request Status: \n ' + xhr.status + ' Status Text: ' + xhr.statusText + ' ' + xhr.responseText);
            }
        });
    }
}
$('#ddl_ROOM').change(function () {
    var roomCode = $('#ddl_ROOM').val();
    $.each(roomSatus, function (index, data) {
        if (roomCode == data.code) {
            $('#RATE_PER_DAY').val(data.rate);
        }
        if (roomCode == data.code && data.alertMessage != null) {
            alert(data.alertMessage);
        }
    });
    calculateDetailTotal();
});
//----------------------------------------------------Reservation Section End-----------------------------------------------------
//----------------------------------------------------Items Section Start-----------------------------------------------------
function showItemTable(e) {
    var counter = $("#Inventory_tblbody tr").length + 1;
    if (counter == 0) {
        counter = counter + 1;
    }
    if ($("#Inventory_tblbody tr").length > 0) {
        var lastrowid = ($('#Inventory_tblbody tr:last').attr('id')).substr(5, 9);
        counter = parseInt(lastrowid) + 1;
    }
    else
        counter = 1
    var room = $("#ddl_AddItems").val();
    var roomName = $("#ddl_AddItems option:selected").text();
    var qty = $("#ItemsQty").val();
    var Comment = $("#iComment").val();
    if (qty == "" || qty == null) {
        toastr.error('Enter Quantity');
        return;
    }
    if (room == "" || room == null) {
        toastr.error('Select Room');
        return;
    }
    var isRoomExist = checkDuplicate(room);
    if (isRoomExist == -1) {
        toastr.error('Invalid Room Selection Room Is Not Exist In Reservation List');
        return;
    }
    var selectedTr = $(e).find('td');
    var barCode = selectedTr.eq(0).text();
    var Name = selectedTr.eq(1).text();
    var retail = selectedTr.eq(2).text();

    qty = parseInt(qty);
    retail = parseInt(retail);

    var total = qty * retail;

    var index = checkDuplicateItem(room, barCode);
    if (index != -1) {
        toastr.error("Already Added");

        //var tr = $("#Inventory_tblbody").find('tr');
        //var td = $(tr[index]).find('td');
        //var previousQty = parseInt(td.eq(5).text());
        //var currentQty = previousQty + qty;
        //td.eq(5).text(currentQty);
        //td.eq(6).text(currentQty * retail);
        //toastr.success("Item Updated");
    }
    else {
    $("#Inventory_tblbody").append(
        "<tr id=trrrr" + counter + ">" +
            "<td style='display:none;'>" + "" + "</td>" +
            "<td style='display:none;'>" + room + "</td>" +
            "<td style='display:none;'>" + barCode + "</td>" +
            "<td>" + roomName + "</td>" +
            "<td>" + Name + "</td>" +
            "<td>" + Comment + "</td>" +
            "<td>" + qty + "</td>" +
            "<td>" + total + "</td>" +
            "<td style='display:none;'>" + retail + "</td>" +
            "<td style='display:none;'></td>" +
        "</tr>"
        );
    $("#Inventory_tblbody tr").last().append
        (
            "<td align='center'><a onclick='RemoveRow(trrrr" + counter + ")'class='fa fa-trash' style='color:red'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditItemsRow(trrrr"+counter+")' class='fa fa-edit'  style='color:blue'></a>" +
            "</td>"
        );
    toastr.success("Item Added");
    counter++;
    }
    calculateTotal();
}
function checkDuplicateItem(roomcode, barcode) {
    var tr = $("#Inventory_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var rcode = td.eq(1).text();
        var bcode = td.eq(2).text();
        if (rcode == roomcode && bcode == barcode) {
            return ind;
        }
    }
    return -1;
}
function EditItemsRow(id) {
    var typeofId = typeof (id);
    var rowId = "";
    if (typeofId == "object") {
        rowId = id.getAttribute('id');
    }
    else {
        rowId = id; //Type will be string
    }

    var d = $(id).find('td');
    var RES_ID= d.eq(0).text();
    var ROOM_CODE= d.eq(1).text();
    var BARCODE= d.eq(2).text();
    var ROOM_NAME= d.eq(3).text();
    var DESCRIPTION = d.eq(4).text();
    var COMMENTS = d.eq(5).text();
    var QUANTITY = d.eq(6).text();
    var AMOUNT = d.eq(7).text();
    var UNIT_RETAIL = d.eq(8).text();
    var STATUS = d.eq(9).text();
   
    if (STATUS != "3") {
        d.eq(5).html('<input type="text" name="AddItemComment" id="AddItemComment" value="' + COMMENTS + '" />');
        d.eq(6).html('<input type="number" name="qtyAdd" id="qtyAdd" value="' + QUANTITY + '" />');
    }
    else {
        toastr.error("This Is An AUthorized Document");
        return;
    }
    if (typeofId == "object") {
        d.eq(10).html("<a onclick='UpdateItemsRow(" + rowId + ")' class='fa fa-check' style='color:green;'></a>");
    }
    else {
        d.eq(10).html('<a onclick="UpdateItemsRow(' + "'" + rowId + "'" + ')" class="fa fa-check" style="color:green;"></a>');
    }
}
function UpdateItemsRow(id) {
    var typeofId = typeof (id);
    var rowId = "";
    if (typeofId == "object") {
        rowId = id.getAttribute('id');
    }
    else {
        rowId = id; //Type will be string
    }

    var d = $(id).find('td');
    var comment = d.eq(5).find("input").val();
    var qty = parseInt(d.eq(6).find("input").val());
    if (qty <= 0) {
        toastr.error("Quantity Can't Be 0 Or Less Than 0");
        return;
    }
    var unitRetal = parseInt(d.eq(8).text());
    var total = qty * unitRetal;
    d.eq(5).text(comment);
    d.eq(6).text(qty);
    d.eq(7).text(total);

    if (typeofId == "object") {
        d.eq(10).html("<a onclick='RemoveRow(" + rowId + ")' class='fa fa-trash' style='color:red;'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditItemsRow(" + rowId + ")' class='fa fa-edit' style='color:blue;'></a>");
    }
    else {
        d.eq(10).html('<a onclick="RemoveRow(' + "'" + rowId + "'" + ')" class="fa fa-trash" style="color:red;"></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick="EditItemsRow(' + "'" + rowId + "'" + ')" class="fa fa-edit" style="color:blue;"></a>');
    }
    calculateTotal();
}
//----------------------------------------------------Items Section End-----------------------------------------------------
//----------------------------------------------------Amenity Section Start-----------------------------------------------------
function showAmenityTable(e) {
    var counter = $("#Amenity_tblbody tr").length + 1;
    if (counter == 0) {
        counter = counter + 1;
    }
    if ($("#Amenity_tblbody tr").length > 0) {
        var lastrowid = ($('#Amenity_tblbody tr:last').attr('id')).substr(5, 9);
        counter = parseInt(lastrowid) + 1;
    }
    else
        counter = 1
    var room = $("#ddl_AddAmanities").val();
    var roomName = $("#ddl_AddAmanities option:selected").text();
    var datetime = $("#aminityDateTime").val();
    var Comment = $("#aComment").val();
    if (datetime == "" || datetime == null) {
        toastr.error('Select Date Time');
        return;
    }
    if (room == "" || room == null) {
        toastr.error('Select Room');
        return;
    }
    var isRoomExist = checkDuplicate(room);
    if (isRoomExist == -1) {
        toastr.error('Invalid Room Selection Room Is Not Exist In Reservation List');
        return;
    }

    datetime = moment(datetime).format('DD/MM/YYYY h:mm:ss A');
    var selectedTr = $(e).find('td');
    var barCode = selectedTr.eq(0).text();
    var Name = selectedTr.eq(1).text();
    var retail = selectedTr.eq(2).text();
    retail = parseInt(retail);

    var index = checkDuplicateAmenity(room, barCode);
    if (index != -1) {
        toastr.error("Already Added");

        //var tr = $("#Amenity_tblbody").find('tr');
        //var td = $(tr[index]).find('td');
        //td.eq(5).text(datetime);
        //toastr.success("Successfully Updated");
    }
    else {
        $("#Amenity_tblbody").append(
            "<tr id=trami" + counter + ">" +
                "<td style='display:none;'>" + "" + "</td>" +
                "<td style='display:none;'>" + room + "</td>" +
                "<td style='display:none;'>" + barCode + "</td>" +
                "<td>" + roomName + "</td>" +
                "<td>" + Name + "</td>" +
                "<td>" + Comment + "</td>" +
                "<td>" + datetime + "</td>" +
                "<td style='display:none;'></td>" +
                "<td style='display:none;'>" + retail + "</td>" +
                "<td style='display:none;'></td>" +
            "</tr>"
            );
        $("#Amenity_tblbody tr").last().append
            (
                "<td align='center'><a onclick='RemoveRow(trami" + counter + ")'class='fa fa-trash' style='color:red'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditAmenityRow(trami" + counter + ")' class='fa fa-edit'  style='color:blue'></a>" +
                //"<a onclick='EditRow(trami" + counter + ")' class='fa fa-edit' style='color:blue'></a>" +
                "</td>"
            );
        counter++;
        toastr.success("Amenity Added");
    }
    calculateTotal();
}
function checkDuplicateAmenity(roomcode, barcode) {
    var tr = $("#Amenity_tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var rcode = td.eq(1).text();
        var bcode = td.eq(2).text();
        if (rcode == roomcode && bcode == barcode) {
            return ind;
        }
    }
    return -1;
}
function EditAmenityRow(id) {
    var typeofId = typeof (id);
    var rowId = "";
    if (typeofId == "object") {
        rowId = id.getAttribute('id');
    }
    else {
        rowId = id; //Type will be string
    }

    var d = $(id).find('td');
    var RES_ID = d.eq(0).text();
    var ROOM_CODE = d.eq(1).text();
    var BARCODE = d.eq(2).text();
    var ROOM_NAME = d.eq(3).text();
    var DESCRIPTION = d.eq(4).text();
    var COMMENTS = d.eq(5).text();
    var DATE = d.eq(6).text();
    oldDate = DATE;

    var AMOUNT = d.eq(7).text();
    var UNIT_RETAIL = d.eq(8).text();
    var STATUS = d.eq(9).text();
    
    if (STATUS != "3") {
        d.eq(5).html('<input type="text" name="AddAmenityComment" id="AddAmenityComment" value="' + COMMENTS + '" />');
        d.eq(6).html('<input type="datetime-local" name="DateAdd" id="DateAdd" />');
    }
    else {
        toastr.error("This Is An AUthorized Document");
        return;
    }
    if (typeofId == "object") {
        d.eq(10).html("<a onclick='UpdateAmenityRow(" + rowId + ")' class='fa fa-check' style='color:green;'></a>");
    }
    else {
        d.eq(10).html('<a onclick="UpdateAmenityRow(' + "'" + rowId + "'" + ')" class="fa fa-check" style="color:green;"></a>');
    }
}
function UpdateAmenityRow(id) {
    var typeofId = typeof (id);
    var rowId = "";
    if (typeofId == "object") {
        rowId = id.getAttribute('id');
    }
    else {
        rowId = id; //Type will be string
    }

    var d = $(id).find('td');
    var Comment = d.eq(5).find("input").val();
    var date = d.eq(6).find("input").val();
    d.eq(5).text(Comment);
    date = moment(date).format('DD/MM/YYYY h:mm:ss A');
    if (date == "" || date == "Invalid date") {
        d.eq(6).text(oldDate);
    }
    if (date != "Invalid date") {
        d.eq(6).text(date);
    }

    var unitRetal = parseInt(d.eq(8).text());
    //var total = qty * unitRetal;
    //d.eq(7).text(total);

    if (typeofId == "object") {
        d.eq(10).html("<a onclick='RemoveRow(" + rowId + ")' class='fa fa-trash' style='color:red;'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditAmenityRow(" + rowId + ")' class='fa fa-edit' style='color:blue;'></a>");
    }
    else {
        d.eq(10).html('<a onclick="RemoveRow(' + "'" + rowId + "'" + ')" class="fa fa-trash" style="color:red;"></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick="EditAmenityRow(' + "'" + rowId + "'" + ')" class="fa fa-edit" style="color:blue;"></a>');
    }
    calculateTotal();
}
//----------------------------------------------------Amenity Section End-----------------------------------------------------
function RemoveRow(id) {
    $(id).remove();
    calculateTotal();
}
function ClearForm() {
    $("#RES_ID").val("");
    $("#SUPL_CODE").val("");
    $("#STATUS").val("");
    $("#docDate").val("");
    $("#FIRST_NAME").val("");
    $("#LAST_NAME").val("");
    $("#PHONE").val("");
    $('#ddl_Country').val("");
    $('#ddl_City').val("");
    $('#ADDRESS').val("");
    $('#ddl_ID_TYPE').val("");
    $('#ID_NO').val("");
    $('#E_MAIL').val("");
    $('#DOB').val("");
    //$('#PER_DAY').val("");
    $('#DISCOUNT').val("");
    $('#AMOUNT_PAID').val("");
    $('#TOTAL_AMT_MAIN').val("");
    $('#BALANCE').val("");
    clearRow();
    $("#tblbody").empty();
    $("#Inventory_tblbody").empty();
    $("#Amenity_tblbody").empty();
    $('#btn_save').removeAttr("disabled");
}
function AlertAuthorizeDocument() {
    toastr.error("This Document Is Authorize You Can't Do This Operation On This");
}

//$('#btn_save').removeAttr("disabled");
//$('#btn_cancel').removeAttr("disabled");
//$('#btn_authorize').removeAttr("disabled");
//$('#add_btn').removeAttr('disabled');

//$('#btn_authorize').attr('disabled', 'disabled');
//$("#add_btn").attr('disabled', 'disabled');
//$('#btn_save').attr('disabled', 'disabled');
//$('#btn_cancel').attr('disabled', 'disabled');

function DateTimeChangeInEditMode(datetime) {
    
    var DateTimeArray = datetime.split(" ");
    var DateArray = DateTimeArray[0].split("/");

    var Day = parseInt(DateArray[0]);
    var Month = parseInt(DateArray[1]);
    var Year = parseInt(DateArray[2]);
    Day = Day < 10 ? '0' + Day : Day;
    Month = Month < 10 ? '0' + Month : Month;

    var time = DateTimeArray[1]; //Time
    var timeTemplate = DateTimeArray[2]; //AM or PM
    var timearr = time.split(":");
    var hours = parseInt(timearr[0]);
    var minutes = parseInt(timearr[1]);
    if (timeTemplate == "PM" || timeTemplate == "pm") {
        if (hours == 12) {
            hours = hours;
        }
        else {
            hours = hours + 12;
        }
        if (hours == 24) {
            hours = 0;
        }
    }
    else {
        if (hours == 12) {
            hours = 0
        }
    }

    minutes = minutes < 10 ? '0' + minutes : minutes;
    hours = hours <= 9 ? '0' + hours : hours;
    //var strTime = hours + ':' + minutes;

    return strDateTime = Year + '-' + Month + '-' + Day + 'T' + hours + ':' + minutes;

    //return new Date(Year, Month, Day, hours, minutes).toISOString();
}
