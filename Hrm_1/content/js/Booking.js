var CurrentDocDate = new Date($("#txt_docDate").val());
var CurrentDate = new Date();
CurrentDate.setHours(0, 0, 0, 0);
CurrentDocDate.setHours(0, 0, 0, 0);
$(document).ready(function () {
    //var CurrentDocDate = new Date($("#txt_docDate").val());
    //var CurrentDate = new Date();
    //CurrentDate.setHours(0, 0, 0, 0);
    //CurrentDocDate.setHours(0, 0, 0, 0);
    
    if ($("#tablePurchaseDetail tr").length == 1) {
        $("#tablePurchaseDetail").hide();
    }
    else
    {
        calculateTotal();
    }
    if ($("#txt_docNo").val() == "") {
        $('#btn_getReport').attr('disabled', 'disabled');
    }
    if ($("#txt_status").val() == 3) {
        $('#btn_authorize').attr('disabled', 'disabled');
        $("#add_btn").attr('disabled', 'disabled');
        $('#btn_save').attr('disabled', 'disabled');
        $("#btn_cancel").attr('disabled', 'disabled');
    }
    $('#pform').submit(function (e) {
        e.preventDefault();
        $('#btn_save').attr('disabled', 'disabled');
        var DocumentMain = {};
        var PurchaseDetailList = {};
        var vm = {
            DocumentMain,
            DocumentDetailList: []
        };
        var Totalamount = parseInt($("#txt_totalAmount").val());
        vm.DocumentMain.RequestPage = $("#RequestPage").val();
        vm.DocumentMain.DocNo = $("#txt_docNo").val();
        vm.DocumentMain.DocDate = $("#txt_docDate").val();
        var date = new Date(vm.DocumentMain.DocDate);
     
        if (vm.DocumentMain.DocNo != "" || vm.DocumentMain.DocNo != null) {
            if (new Date(date) < CurrentDocDate && new Date(date) < CurrentDate) {
                toastr.error("You Can't Select Back Date </br> Or Please Select Document or Future Date");
                $('#btn_save').removeAttr("disabled");
                return;
            }
        }
        else
        {
            if (date < CurrentDate) {
                toastr.error("You Can't Select Back Date </br> Please Select Current Date or Future Date");
                $('#btn_save').removeAttr("disabled");
                return;
            }
        }
        vm.DocumentMain.DocType = $("#txt_doctype").val();
        vm.DocumentMain.Location = $("#ddl_location").val();
        //if (vm.DocumentMain.Location != "" || vm.DocumentMain.Location != null) {
        //        toastr.error("Select Location");
        //        return;
        //}
        vm.DocumentMain.SuplCode = $("#ddl_supplier").val();
        vm.DocumentMain.Time = $("#txt_time").val();
        vm.DocumentMain.Advance = $("#txt_AdvanceAmount").val();
        vm.DocumentMain.Phone = $("#txt_phone").val();
        vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
        vm.DocumentMain.staffcode = $("#ddl_staffName").val();
        vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();
        vm.DocumentMain.ReturnAmount = $("#txt_Return").val();
        vm.DocumentMain.Payment = $("#txt_Payment").val();
        var tr = $("#tblbody").find('tr');
        for (var ind = 0; ind < tr.length; ind++) {
            var purchaseDetail = {};
            var td = $(tr[ind]).find('td');
            purchaseDetail.uanno = td.eq(0).text();
            purchaseDetail.barcode = td.eq(1).text();
            purchaseDetail.qty = td.eq(3).text();
            purchaseDetail.retail = td.eq(4).text();
            purchaseDetail.cost = td.eq(5).text();
            purchaseDetail.discount = td.eq(6).text();
            purchaseDetail.amount = td.eq(7).text();
            purchaseDetail.staffcode = td.eq(8).text();
            purchaseDetail.warehouse = "01"; //td.eq(8).text();
            purchaseDetail.colour = "01";//td.eq(10).text();
            purchaseDetail.DateTimeSlot = vm.DocumentMain.DocDate +" "+  td.eq(14).text();
            vm.DocumentDetailList.push(purchaseDetail);
        }
        if (vm.DocumentDetailList.length == 0) {
            toastr.error('Select min one item to save.');
            $('#btn_save').removeAttr("disabled");
            return;
        }
        if (Totalamount < parseInt(vm.DocumentMain.Advance)) {
            toastr.error('Please Enter Correct Advance Amount.');
            $('#btn_save').removeAttr("disabled");
            return;
        }
        if (Totalamount <= parseInt(vm.DocumentMain.ReturnAmount)) {
            toastr.error('Please Enter Correct return Amount.');
            $('#btn_save').removeAttr("disabled");
            return;
        }
        $.ajax({
            url: "/api/Sales",
            method: "post",
            data: vm
        })
            .done(function (obj) {
                if (obj == "NotSaved") {
                    toastr.error('Advance cannot be greater than Total Amount');
                    $('#btn_save').removeAttr("disabled");
                }
                else {
                    toastr.success('Data saved successfully.');
                    $("#btn_print1").attr('disabled', 'disabled');
                    $('#btn_save').removeAttr("disabled");
                    $("#txt_docNo").val(obj);
                    $("#new_DocNo").val("");
                    $('#btn_cancel').removeAttr("disabled");
                    //fetchevents();
                    $("#btn_print1").removeAttr('disabled');
                }
                //  doSubmit();
            })
            .fail(function (xhr) {
                alert('Request Status: \n ' +
                    xhr.status +
                    ' Status Text: ' +
                    xhr.statusText +
                    ' ' +
                    xhr.responseText);
                $('#btn_save').removeAttr("disabled");
                toastr.error('Data not saved');

            });
    });
});
function AddTextBox() {
    var v = jQuery("#ddl_supplier option:selected").text();
    var cust_id = $("#ddl_supplier").val();
    var custpresentid = $("#txt_supplcode").val();
    $("#txt_customerCategory").val("");
    $("#txt_customerCategoryId").val("");
    $("#txt_customerDiscount").val("");
    $("#txt_phone").val("");
    if (v == 'Add New') {
        $("#div_text").show();
        $("#ddl_supplier").hide();
        $("#btn_save").attr('disabled', 'disabled');
    }
    else if (cust_id != "") {
        var tablelength = $("#tblbody tr").length;
        if (custpresentid != "" && tablelength > 0) {
            var r = confirm("Are you sure clear this document!");
            if (r == true) {
                ClearForm();
                $("#tblbody tr").remove();
                $("#ddl_supplier").val(cust_id);
                $.ajax({
                    url: "GetPhoneNumberanddiscountcategory/",
                    method: "post",
                    data: { SUPL_CODE: cust_id }
                })
                .done(function (_obj) {
                    if (_obj == "null") { }
                    else {
                        var obj = JSON.parse(_obj);
                        $("#txt_phone").val(obj);
                        //$("#txt_phone").val(obj.MOBILE);
                        //$("#txt_customerCategory").val(obj.DiscountCategoryName);
                        //$("#txt_customerCategoryId").val(obj.DiscountCategoryId);
                        //$("#txt_customerDiscount").val(obj.Discount);
                        $("#div_text").hide();
                        $("#ddl_supplier").show();
                        $('#btn_save').removeAttr('disabled');
                    }
                    $("#txt_supplcode").val(cust_id);
                }).fail(function () {
                    alert("call fail");
                })
            }
            else {
                $("#ddl_supplier").val(custpresentid);
            }
        }
        else {
            $.ajax({
                url: "GetPhoneNumberanddiscountcategory/",
                method: "post",
                data: { SUPL_CODE: cust_id }
            }).done(function (_obj) {
                    if (_obj == "null") { }
                    else {
                        var obj = JSON.parse(_obj);
                        $("#txt_phone").val(obj);
                        //$("#txt_phone").val(obj.MOBILE);
                        //$("#txt_customerCategory").val(obj.DiscountCategoryName);
                        //$("#txt_customerCategoryId").val(obj.DiscountCategoryId);
                        //$("#txt_customerDiscount").val(obj.Discount);
                        $("#div_text").hide();
                        $("#ddl_supplier").show();
                        $('#btn_save').removeAttr('disabled');
                    }
                    $("#txt_supplcode").val(cust_id);
            }).fail(function () {
                    alert("call fail");
            })
        }
    }
    else {
        $("#div_text").hide();
        $("#ddl_Customer").show();
        $('#btn_Save').removeAttr('disabled');
    }
}
function AddNewCustomer() {
    $('#ddl_supplier').append(new Option('Add New', ''));
    $('#ddl_supplier option:last-child').css("color", "blue");
}
function SaveNewCustomer() {
    var uppervalue = toUpper($("#txt_CustomerName").val());
    $("#txt_CustomerName").val(uppervalue);
    var Name = $("#txt_CustomerName").val();
    var Phone = $("#txt_phone").val();
    if (Name == "" || Phone == "" || Phone.length < 11) {
        alert("Please Input Correct Name and Phone");
        return;
    }
    else {
        $.ajax({
            url: "SaveNewCustomer/",
            method: "post",
            data: { Name: Name, Phone: Phone }
        })
        .done(function (_obj) {


            if (_obj != "") {
                var obj = JSON.parse(_obj);
                var lastind;
                $('#ddl_supplier').empty();
                $('#ddl_supplier').append(new Option('Select Customer', ''));
                $.each(obj, function (index, element) {
                    $('#ddl_supplier').append(new Option(obj[index].SUPL_NAME, obj[index].SUPL_CODE));
                    lastind = obj[index].SUPL_CODE;
                });

                $('#ddl_supplier').val(lastind);
                toastr.success('Customer saved successfully.');
                //  HideMe();
                //  $('#btn_Save').removeAttr('disabled');
                $("#div_text").hide();
                $("#ddl_supplier").show();
                $('#btn_save').removeAttr('disabled');
                AddNewCustomer();
            }
            toastr.error('Customer is already exist');
        })
        .fail(function () {
            toastr.error('Data not saved.');
        });
    }
}
function cancelcustomer() {
    $("#div_text").hide();
    $("#ddl_supplier").show();
    $('#btn_Save').removeAttr('disabled');
    $("#btn_save").removeAttr('disabled');
}
function GetQty() {
    var b = $("#ddl_itemName").val();
    var c = $("#ddl_colour").val();
    var id;
    if (b != null && c != null) {
        id = b + "," + c;
        $.ajax({
            url: "/api/ProdBalance?id=" + id,
            type: "GET",
            dataType: "JSON",
            success: function (_data) {
                $("#txt_qtyinHand").val(_data);

                $("#txt_qty").focus();

            },
            error: function (xhr) {
                alert('Request Status: \n ' +
                    xhr.status +
                    ' Status Text: ' +
                    xhr.statusText +
                    ' ' +
                    xhr.responseText);
            }
        }
        );
    }
}
function Authorize() {
    $('#btn_authorize').attr('disabled', 'disabled');
    $("#add_btn").attr('disabled', 'disabled');
    $('#btn_save').attr('disabled', 'disabled');

    var DocumentMain = {};
    var PurchaseDetailList = {};
    var vm = {
        DocumentMain,
        DocumentDetailList: []
    };
    var Totalamount = parseInt($("#txt_totalAmount").val());
    vm.DocumentMain.RequestPage = $("#RequestPage").val();
    vm.DocumentMain.DocNo = $("#txt_docNo").val();
    vm.DocumentMain.DocDate = $("#txt_docDate").val();
    vm.DocumentMain.DocType = $("#txt_doctype").val();
    vm.DocumentMain.Location = "01";////$("#ddl_location").val();
    vm.DocumentMain.SuplCode = $("#ddl_supplier").val();
    vm.DocumentMain.Advance = $("#txt_AdvanceAmount").val();
    vm.DocumentMain.Time = $("#txt_time").val();
    vm.DocumentMain.Phone = $("#txt_phone").val();
    vm.DocumentMain.ReturnAmount = $("#txt_Return").val();
    vm.DocumentMain.Payment = $("#txt_Payment").val();
    vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
    vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();
    vm.DocumentMain.staffcode = $("#ddl_staffName").val();
    var date = new Date(vm.DocumentMain.DocDate);
    if (vm.DocumentMain.DocNo != "" || vm.DocumentMain.DocNo != null) {
        if (new Date(date) < CurrentDocDate && new Date(date) < CurrentDate) {
            toastr.error("You Can't Select Back Date </br> Or Please Select Document or Future Date");
            return;
        }
    }
    else {
        if (date < CurrentDate) {
            toastr.error("You Can't Select Back Date </br> Please Select Current Date or Future Date");
            return;
        }
    }
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var purchaseDetail = {};
        var td = $(tr[ind]).find('td');
        purchaseDetail.uanno = td.eq(0).text();
        purchaseDetail.barcode = td.eq(1).text();
        purchaseDetail.qty = td.eq(3).text();
        purchaseDetail.retail = td.eq(4).text();
        purchaseDetail.cost = td.eq(5).text();
        purchaseDetail.discount = td.eq(6).text();
        purchaseDetail.amount = td.eq(7).text();
        purchaseDetail.staffcode = td.eq(8).text();
        purchaseDetail.warehouse = "01";//td.eq(8).text();
        purchaseDetail.colour = td.eq(10).text();
        purchaseDetail.Totalamount = $("#txt_totalAmount").val();
        purchaseDetail.TotalQty = $("#txt_TotalQty").val();
        purchaseDetail.DateTimeSlot = vm.DocumentMain.DocDate + " " + td.eq(14).text();
        vm.DocumentDetailList.push(purchaseDetail);
    }
    if (vm.DocumentDetailList.length == 0) {
        toastr.error('Select min one item to save.');
        $('#btn_save').removeAttr("disabled");
        $('#btn_authorize').removeAttr("disabled");
        $("#add_btn").removeAttr("disabled");
        return;
    }
    if (Totalamount < parseInt(vm.DocumentMain.Advance)) {
        toastr.error('Please Enter Correct Advance Amount.');
        $('#btn_save').removeAttr("disabled");
        return;
    }
    if (Totalamount <= parseInt(vm.DocumentMain.ReturnAmount)) {
        toastr.error('Please Enter Correct return Amount.');
        $('#btn_save').removeAttr("disabled");
        return;
    }
    $.ajax({
        url: "/api/Authorize",
        method: "post",
        data: vm
    }).done(function (obj) {
            if (obj == "NotSaved") {
                toastr.error('Advance cannot be greater than Total Amount');
                $('#btn_save').removeAttr("disabled");
                $('#btn_authorize').removeAttr("disabled");
            }
            else {
                toastr.success('Data saved successfully.');
                //fetchevents();
                $("#btn_print1").removeAttr('disabled');
                toastr.success('Data saved successfully.');
                $('#btn_authorize').attr('disabled', 'disabled');
                $("#txt_docNo").val(obj);
                $("#btn_cancel").attr('disabled', 'disabled');
            }
        }).fail(function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
            toastr.error('Data not saved');
            $('#btn_save').removeAttr("disabled");
            $('#btn_authorize').removeAttr("disabled");
            $("#add_btn").removeAttr("disabled");
            $("#btn_print1").removeAttr('disabled');

        });
    //fetchevents();
}
function calculateAmount() {
    $('#txt_amount').val($('#txt_qty').val() * $('#txt_retail').val() - $('#txt_discount').val());
    $("#txt_discount").val(getDiscount($('#txt_qty').val(), $('#txt_retail').val()));
    //   console.log("Amount Calculated.");
}
function getProduct() {
    var barcode = $('#ddl_itemName').val();
    $.ajax({
        url: "/api/Product?id=" + barcode + "&UanNo=" + "" + "&Name=",
        type: "GET",
        dataType: "JSON",
        success: function (_data) {
            var obj = JSON.parse(_data);
            $.each(obj, function (index, obj) {
                $("#txt_uanno").val(obj.Uanno);
                $("#txt_retail").val(obj.Retail);
                $("#txt_cost").val(obj.Cost);
                $("#txt_qtyinHand").val(obj.QtyinHand);
                $("#txt_discount").val(0);
                $("#ddl_staffName").focus();
            });
            
        },
        error: function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
        }
    }
    );
}
function getDiscount(qty, retail) {
    var customerdiscount = parseInt($("#txt_customerDiscount").val());
    var qtyy = parseInt(qty);
    var retaill = parseInt(retail)
    if (customerdiscount > 0 && qtyy > 0 && retaill > 0) {
        var totalamount = qtyy * retaill;
        var discount = (totalamount * customerdiscount) / 100;
        return Math.round(discount);
    }
    else {
        return 0;
    }
}
function getTableRows() {
    return $("#tablePurchaseDetail tr").length;
}
function FillWarehouse1() {

    $("#txt_warehouse").val($("#ddl_location").val());
}
function FillWarehouse() {
    if (getTableRows() > 1) {
        if (confirm("Are you sure you want to cancel document.")) {
            ClearForm(false);
        } else {

            $("#ddl_location").val($("#txt_warehouse").val());
            return;
        }
    }

    var stateId = $('#ddl_location').val();

    $.ajax({
        url: "/api/Warehouse?id=" + stateId,
        dataType: "JSON",
        success: function (data) {
            var obj = JSON.parse(data);
            $('#ddl_warehouse').empty();
            $('#ddl_warehouse').append(new Option('Select Warehouse', ''));
            $.each(obj,
                function (index, element) {
                    $('#ddl_warehouse').append(new Option(obj[index].Name, obj[index].id));

                });
        },
        error: function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
        }
    }
    );
}
function checkDuplicate(barcode, Warehouse, Color) {
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var bcode = td.eq(1).text();
        //var whouse = td.eq(8).text();
        //var color = td.eq(10).text();
        var whouse = "01";
        var color = "01";
        if (bcode == barcode && whouse == Warehouse && color == Color) {
            return ind;
        }
    }
    return -1;

}
function showTable() {

    var customer = $("#ddl_supplier").val();
    if (customer != "") {
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
        //  alert(counter)
        $("#tablePurchaseDetail").show();
        var icode = $("#ddl_itemName").val();
        var _iname = $("option:selected", "#ddl_itemName").html();
        //var discount = Math.round(parseInt($("#txt_discount").val()),0);
        var discount = $("#txt_discount").val();
        var qty = $("#txt_qty").val();
        var cost = $("#txt_cost").val();
        var retail = ($("#txt_retail").val());
        var amount = (parseInt(qty) * parseInt(retail)) - parseInt(discount);
        var uanno = $("#txt_uanno").val(); 
        var timeSlot = $("#txt_time").val();
        timeSlot = formatAMPM(timeSlot);
        var warehouse = "01";//$("#ddl_warehouse").val();
        var colour = "01";//$("#ddl_colour").val();
        var warehouseName = $("option:selected", "#ddl_warehouse").html();
        var colourName = $("option:selected", "#ddl_colour").html();
        var StaffMember = $("option:selected", "#ddl_staffName").html();
        var Staffcode = $("#ddl_staffName").val();
        colourName = (colourName === "Select Color") ? "" : colourName;
        var location = "01";//$("#ddl_location").val();
        if (parseFloat(cost) >= parseFloat(retail)) {
            toastr.error("Retail cannot be less than origional Price and the origional Price is" + cost + "");
            return;
        }
        else if (location == "") {
            toastr.error('Select Location to add Item.');
        }
            //else if (Staffcode == "") {
            //    toastr.error('Select Staff Member to add Item.');
            //}
        else if (colour == "" && $("#ddl_colour option").length > 1) {
            toastr.error('Please select color.');
        }
        else if (icode == "" || qty == "" || retail == "" || warehouse == "") {
            toastr.error('ItemName,price and Quantity is required.');
        }
        else if (!qty.match("^[0-9]+(.[0-9])*$") ||
            !retail.match("^[0-9]+(.[0-9])*$") ||
            !discount.match("^[0-9]+(.[0-9])*$")) {
            toastr.error('Quantity, price and discount can only contains numerics.');

        }
        else {
            var index = checkDuplicate(icode, warehouse, colour);
            if (index != -1) {
                var tr = $("#tblbody").find('tr');
                var td = $(tr[index]).find('td');
                //td.eq(3).text(parseInt(qty) + parseInt(td.eq(3).text()));
                //td.eq(7).text(td.eq(7).text() + ((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text())));

                td.eq(3).text(parseInt(qty) + parseInt(td.eq(3).text()));
                td.eq(6).text(parseInt(discount) + parseInt(td.eq(6).text()));
                td.eq(4).text(retail);
                // td.eq(7).text(parseInt(td.eq(7).text()) + ((parseInt(td.eq(3).text()) * parseInt(td.eq(5).text())) - parseInt(td.eq(6).text())));
                td.eq(7).text((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text()));
                td.eq(9).text(StaffMember);
            }
            else {
                $("#tblbody").append(
                    "<tr id=trrr" + counter + ">" +
                    "<td style='display:none;'>" + uanno + "</td>" +
                    "<td style='display:none;'>" + icode + "</td>" +
                    "<td>" + _iname + "</td>" +
                    "<td>" + qty + "</td>" +
                    "<td>" + retail + "</td>" +
                    "<td style='display:none;'>" + cost + "</td>" +
                    "<td>" + discount + "</td>" +
                    "<td>" + amount + "</td>" +
                    "<td style='display:none'>" + Staffcode + "</td>" +
                    "<td>" + StaffMember + "</td>" +
                    "<td style='display:none;'>" + warehouse + "</td>" +
                    "<td style='display:none;'>" + warehouseName + "</td>" +
                    "<td style='display:none;'>" + colour + "</td>" +
                    "<td style='display:none;'>" + colourName + "</td>" +
                    "<td >" + timeSlot + "</td>" +
                    "</tr>");
                $("#tblbody tr").last().append(
                    "<td><a onclick='RemoveRow(trrr" +
                    counter +
                    "," +
                    counter +
                    ")' class='fa fa-trash'></a></td><td><a onclick='EditRow(trrr" +
                    counter +
                    "," +
                    counter +
                    ")' class='fa fa-edit'></a></td>");
                counter++;
            }
            clearRow();
            calculateTotal();
        }
    }
    else {
        toastr.error('Select Customer to add Item.');
    }
}
function calculateTotal() {
    var totalAmount = 0;
    var TotalQty = 0;
    var TotalDiscount = 0;
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        totalAmount = parseInt(totalAmount) + parseInt(td.eq(7).text());
        TotalQty = parseInt(TotalQty) + parseFloat(td.eq(3).text());
        TotalDiscount = parseInt(TotalDiscount) + parseFloat(td.eq(6).text());
    }
    $("#txt_totalAmount").val(totalAmount);
    $("#txt_TotalQty").val(TotalQty);
    $("#txt_TotalDiscountAmount").val(TotalDiscount);
}
function clearRow() {
    $("#ddl_itemName").val("");
    $("option:selected", "#ddl_itemName").html("Select Item");
    $("#ddl_colour").val("");
    $("#ddl_warehouse").val("");
    $("#txt_uanno").val("");
    $("#txt_qty").val("1");
    $("#txt_discount").val("0");
    $("#txt_cost").val("");
    $("#txt_retail").val("");
    $("#txt_amount").val("");
    $("#txt_qtyinHand").val("");
    $("#txt_totalAmount").val("");
    $("#txt_TotalDiscountAmount").val("");
}
function ClearForm(flag) {
    $("#ddl_itemName").val("");
    $("#ddl_supplier").val("");
    $("#txt_AdvanceAmount").val("");
    $("#txt_Payment").val("");
    $("#btn_print1").attr('disabled', 'disabled');
    $('#btn_save').removeAttr("disabled");
    $('#btn_authorize').removeAttr("disabled");
    $("#add_btn").removeAttr("disabled");
    $("#ddl_staffName").val("");
    $("#txt_Return").val("");
    $('#btn_cancel').attr('disabled', 'disabled');
    if (flag) { $("#ddl_location").val(""); }
    $("#txt_customerCategory").val("");
    $("#txt_customerCategoryId").val("");
    $("#txt_customerDiscount").val("");
    $("#ddl_warehouse").val("");
    $("#txt_docNo").val("");
    $("#tablePurchaseDetail").hide();
    $("#txt_totalAmount").val("");
    $("#tblbody tr").remove();
    $("#txt_phone").val("");
    $("#txt_supplcode").val("");
    clearRow();
    var doc_type = $("#txt_doctype").val();
    var docno = $("#new_DocNo").val();
    if (docno == "") {
        $.ajax({
            url: "/Sales/GenerateBarcode?DocType=" + doc_type,
            method: "get",
            //  data:doctype
        })
          .done(function (obj) {
              var _obj = JSON.parse(obj);
              $("#new_DocNo").val(_obj);
              $("#txt_docNo").val(docno);
              $("#txt_docDisplayNo").val(_obj);
              $("#DocumentMain_DocNoDisplay").val(_obj);

              $("#btn_new").removeAttr('disabled');
          })
          .fail(function (xhr) {
              $("#btn_new").removeAttr('disabled');
          });
    }
    else {
        //  $("#txt_docDisplayNo").val(docno);
        $("#DocumentMain_DocNoDisplay").val(docno);
        //    $("#txt_docNo").val(docno);
    }
}
function inactiveBooking() {
    var docno = $("#DocumentMain_DocNoDisplay").val();
    if (docno != "") {
        $.ajax({
            url: "/Sales/bookingStatus?DocNo=" + docno,
            method: "get",
            //  data:doctype
        })
          .done(function (obj) {
              var _obj = JSON.parse(obj);
              fetchevents();
              RemoveModel();

              $("#btn_cancel").attr('disabled', 'disabled');
              $('#btn_authorize').attr('disabled', 'disabled');
              $("#add_btn").attr('disabled', 'disabled');


          })
          .fail(function (xhr) {
              $("#btn_cancel").removeAttr('disabled');
          });
    }
}
function EditRow(id) {
    var d = $(id).find('td');
    $("option:selected", "#ddl_itemName").html(d.eq(2).text());
    $("#ddl_itemName").val(d.eq(1).text());
    $("#txt_uanno").val(d.eq(0).text());
    $("#txt_qty").val(d.eq(3).text());
    $("#txt_retail").val(d.eq(4).text());
    $("#txt_cost").val(d.eq(5).text());
    $("#txt_discount").val(d.eq(6).text());
    $("#txt_amount").val(d.eq(7).text());
    //  $("option:selected,#ddl_staffName").html(d.eq(8).text());
    $("#ddl_warehouse").val(d.eq(8).text());
    var staffname = d.eq(8).text();
    var time = d.eq(14).text();
    time  = changeTimeInEditMode(time);
    $("#txt_time").val(time);
    //    alert(staffname);
    if (staffname == "null") {
        $("#ddl_staffName").val('');
    }
    else {
        $("#ddl_staffName").val(d.eq(8).text());
    }

    $("option:selected", "#ddl_warehouse").html(d.eq(9).text());
    //  getProduct();
    setTimeout(function () {
        $("#ddl_colour").val(d.eq(10).text());
        $("option:selected", "#ddl_colour").html(d.eq(11).text());
    }, 1000);
   
    RemoveRow(id);
}
function RemoveRow(id) {
    $(id).remove();
    calculateTotal();
}
function RemoveModel() {
    //  $('#myReportModal').modal('hide');
    $('#myModal').modal('hide');
    $('#OnlineBookingModel').modal('hide');
    $('#btn_print1').attr('disabled', 'disabled');
}
function RemoveReportModel() {
    $('#myReportModal').modal('hide');
    //   $('#btn_print1').attr('disabled', 'disabled');
}
function formatAMPM(time) {
    var timearr = time.split(":");
    var hours = parseInt(timearr[0]);
    var minutes =parseInt(timearr[1]);
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    hours = hours <= 9 ? '0' + hours : hours;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
}
function changeTimeInEditMode(time) {
    var TimeAndformat = time.split(" ");
    var time = TimeAndformat[0]; //Time
    var timeTemplate = TimeAndformat[1]; //AM or PM
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
    else{
        if (hours == 12) {
            hours = 0
        }
    }
    
    minutes = minutes < 10 ? '0' + minutes : minutes;
    hours = hours <= 9 ? '0' + hours : hours;
    var strTime = hours + ':' + minutes;
    return strTime;
}