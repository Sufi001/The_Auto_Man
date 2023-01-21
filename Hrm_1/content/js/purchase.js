var counter = $("#tablePurchaseDetail tr").length;
var InEditMode = false;
$(document).ready(function () {

    if ($("#tablePurchaseDetail tr").length == 0) {
        $("#tablePurchaseDetail").hide();
    } else {
        $("#tablePurchaseDetail").show();
        calculateTotal();
    }
    if ($("#txt_docNo").val() != "" && $("#txt_status").val() != 3) {
        $("#btn_print1").removeAttr("disabled");
    }
    else {
        $('#btn_authorize').attr('disabled', 'disabled');
    }
    if ($("#txt_status").val() == 3) {
        $('#btn_authorize').attr('disabled', 'disabled');
        $("#add_btn").attr('disabled', 'disabled');
        $('#btn_submit').attr('disabled', 'disabled');
        $("#btn_print1").removeAttr("disabled");
    }
    $('#pform').submit(function (e) {
        e.preventDefault();
        if (InEditMode) {
            toastr.error('Complete Your Editing First');
            return;
        }
        $('#btn_submit').attr('disabled', 'disabled');
        var DocumentMain = {};
        var PurchaseDetailList = {};
        var vm = {
            DocumentMain,
            DocumentDetailList: []
        };
        vm.DocumentMain.DocNo = $("#txt_docNo").val();
        vm.DocumentMain.DocDate = $("#txt_docDate").val();
        vm.DocumentMain.DocType = $("#txt_doctype").val();
        vm.DocumentMain.Location = $("#ddl_location").val();
        vm.DocumentMain.SuplCode = $("#ddl_supplier").val();
        vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
        vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();
        if (vm.DocumentMain.SuplCode == "") {
            toastr.error('Please Select Supplier.');
            $('#btn_submit').removeAttr("disabled");
            return;
        }
        var tr = $("#tblbody").find('tr');
        for (var ind = 0; ind < tr.length; ind++) {
            var purchaseDetail = {};
            var td = $(tr[ind]).find('td');

            purchaseDetail.uanno = td.eq(0).text();
            purchaseDetail.barcode = td.eq(1).text();
            purchaseDetail.qty = td.eq(5).text();
            purchaseDetail.FreeQty = td.eq(6).text();
            purchaseDetail.retail = td.eq(7).text();
            purchaseDetail.cost = td.eq(8).text();
            purchaseDetail.discount = td.eq(9).text();
            purchaseDetail.amount = td.eq(10).text();
            vm.DocumentDetailList.push(purchaseDetail);
        }
        if (vm.DocumentDetailList.length == 0) {
            toastr.error('Select min one item to save.');
            $('#btn_submit').removeAttr("disabled");
            return;
        }
        $.ajax({
            url: "/api/Purchase",
            method: "post",
            data: vm
        })
            .done(function (obj) {
                $('#btn_submit').removeAttr("disabled");
                // $("#btn_print1").attr('disabled', 'disabled');
                toastr.success('Data saved successfully.');
                $("#txt_docNo").val(obj);
                $("#new_DocNo").val("");
                $("#btn_print1").removeAttr("disabled");
                $("#btn_authorize").removeAttr("disabled");
            })
            .fail(function (xhr) {
                alert('Request Status: \n ' +
                    xhr.status +
                    ' Status Text: ' +
                    xhr.statusText +
                    ' ' +
                    xhr.responseText);
                $('#btn_submit').removeAttr("disabled");
                toastr.error('Data not saved');
            });
    });

});

function Authorize() {
    if (InEditMode) {
        toastr.error('Complete Your Editing First');
        return;
    }
    var DocumentMain = {};
    var PurchaseDetailList = {};
    var vm = {
        DocumentMain,
        DocumentDetailList: []
    };
    vm.DocumentMain.DocNo = $("#txt_docNo").val();
    vm.DocumentMain.DocDate = $("#txt_docDate").val();
    vm.DocumentMain.DocType = $("#txt_doctype").val();
    vm.DocumentMain.Location = $("#ddl_location").val();
    vm.DocumentMain.SuplCode = $("#ddl_supplier").val();
    vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
    vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();

    if (vm.DocumentMain.SuplCode == "") {
        toastr.error('Please Select Supplier.');
        $('#btn_submit').removeAttr("disabled");
        return;
    }
    $('#btn_authorize').attr('disabled', 'disabled');
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var purchaseDetail = {};
        var td = $(tr[ind]).find('td');
        purchaseDetail.uanno = td.eq(0).text();
        purchaseDetail.barcode = td.eq(1).text();
        purchaseDetail.qty = td.eq(5).text();
        purchaseDetail.FreeQty = td.eq(6).text();
        purchaseDetail.retail = td.eq(7).text();
        purchaseDetail.cost = td.eq(8).text();
        purchaseDetail.discount = td.eq(9).text();
        purchaseDetail.amount = td.eq(10).text();
        purchaseDetail.Totalamount = $("#txt_totalAmount").val();
        purchaseDetail.TotalQty = $("#txt_TotalQty").val();
        vm.DocumentDetailList.push(purchaseDetail);
    }
    if (vm.DocumentDetailList.length == 0) {
        $('#btn_authorize').removeAttr("disabled");
        toastr.error('Select min one item to save.');
        return;
    }
    $.ajax({
        url: "/api/AuthorizePurchase",
        method: "post",
        data: vm
    }).done(function (obj) {
            toastr.success('Data saved successfully.');
            // $('#btn_authorize').attr('disabled', 'disabled');
            $('#btn_authorize').attr('disabled', 'disabled');
            $("#add_btn").attr('disabled', 'disabled');
            $('#btn_save').attr('disabled', 'disabled');
            $("#txt_docNo").val(obj);
            $("#btn_print1").removeAttr("disabled");
        })
        .fail(function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
            $('#btn_authorize').removeAttr("disabled");
            $('#btn_submit').removeAttr("disabled");
            $("#add_btn").removeAttr("disabled");
            toastr.error('Data not saved');
        });
}

function calculateAmount(ctnSize) {
    var IQty = parseFloat($('#iQty').val());
    var iCost = parseFloat($('#iCost').val());
    var iDiscount = parseFloat($('#iDiscount').val());
    var amount = (IQty * iCost) - iDiscount;
    $('#iAmount').val(amount);

    if (ctnSize != "" && ctnSize != null && ctnSize != undefined && !isNaN(ctnSize)) {
        var result = (IQty % ctnSize) == 0 ? (IQty / ctnSize) : 0;
        $('#iQtyFromCarton').val(result)
    }
}

function checkDuplicate(barcode) {
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var bcode = td.eq(1).text();
        if (bcode == barcode) {
            return ind;
        }
    }
    return -1;
}

function showTable(e) {
    var qty = parseFloat($("#QTYYYY").val());
    if (qty ==  "" || qty == null ) {
        toastr.error('Enter Quantity');
        return;
    }

    var selected = $(e).hasClass("selected");
    if (!selected)
        $(e).addClass("selected");


    var selectedTr = $(e).find('td');
    $("#tablePurchaseDetail").show();
    var uanno = selectedTr.eq(0).text();
    var icode = selectedTr.eq(1).text();
    var _iname = selectedTr.eq(2).text();
    //var discount = '';
    var cost = selectedTr.eq(4).text();
    var retail = selectedTr.eq(5).text();
    var CtnPcs = selectedTr.eq(6).text();
    var location = $("#ddl_location").val();
    
    var greateramount = parseFloat(cost) - parseFloat(retail)
    var location = $("#ddl_location").text();
    if (location == "") {
        toastr.error('Select Location to add Item.');
    }
    else if (parseFloat(cost) > parseFloat(retail)) {
        toastr.error('Cost Greater Than Retail  ' + greateramount + ' Rupees');
        return;
    }
    else {
        var index = checkDuplicate(icode);
        if (index != -1) {
            var tr = $("#tblbody").find('tr');
            var td = $(tr[index]).find('td');
            var Quantity = parseFloat(td.eq(5).text());
            var Discount = parseFloat(td.eq(9).text());
            Quantity = Quantity + qty;
            td.eq(5).text(Quantity)
            td.eq(10).text((Quantity * cost) - Discount);
        }
        else {
            $("#tblbody").append(
                "<tr id=trrr" + counter + ">" +
                    "<td>" + uanno + "</td>" +
                    "<td style='display:none;'>" + icode + "</td>" +
                    "<td>" + _iname + "</td>" +
                    "<td  style='background-color: #e9ecef;'>" + CtnPcs + "</td>" +
                    "<td class='changeable' contenteditable='true'>" + 0 + "</td>" +
                    "<td class='changeable' contenteditable='true'>" + qty + "</td>" +
                    "<td class='changeable' contenteditable='true'>" + 0 + "</td>" +
                    "<td class='changeable' contenteditable='true' style=''>" + retail + "</td>" +
                    "<td class='changeable' contenteditable='true'>" + cost + "</td>" +
                    "<td class='changeable' contenteditable='true'>" + 0 + "</td>" +
                    "<td>" + cost * qty + "</td>" +
                "</tr>"
                );
            $("#tblbody tr").last().append
                (
                    "<td align='center'>" +
                    "<a onclick='RemoveRow(trrr" + counter + ")'class='fa fa-trash' style='color:red'></a>" +
                    //" &nbsp;&nbsp;|&nbsp;&nbsp; <a onclick='EditRow(trrr" + counter + ")' class='fa fa-edit' style='color:blue'></a>" +
                    "</td>"
                );
            counter++;
        }
        calculateTotal();
    }
}

function clearRow() {
    $("#ddl_itemName").val("");
    $("option:selected", "#ddl_itemName").html("Select Item");
    $("#txt_uanno").val(""); 
    $("#txt_FreeQty").val(0);
    $("#txt_qty").val("");
    $("#txt_qty").val("");
    $("#txt_discount").val("0");
    $("#txt_cost").val("");
    $("#txt_retail").val("");
    $("#txt_amount").val(""); 
    $("#txt_qtyinHand").val("");
}

function ClearForm(flag) {
    $("#txt_Balance").val("");
    $("#ddl_itemName").val("");
    $("#ddl_supplier").val("");
    if (flag) {
        //  $("#ddl_location").val("");
    }
    // $("#txt_docDate").val("");
    $("#txt_docNo").val("");
    $("#tablePurchaseDetail").hide();
    $("#txt_totalAmount").val("");
    $("#txt_TotalQty").val("");
    $("#txt_TotalDiscountAmount").val("");
    $("#tblbody tr").remove();
    $("#btn_print1").attr('disabled', 'disabled');
    $('#btn_authorize').removeAttr("disabled");
    $('#btn_save').removeAttr("disabled");
    $('#btn_submit').removeAttr("disabled");
    $('#add_btn').removeAttr("disabled");
    clearRow();
    // location.reload();
    //  <input type="text" id="new_DocNo" style="display:none" />
    var docno = $("#new_DocNo").val();
    if (docno == "") {
        $.ajax({
            url: "/Purchase/GenerateBarcode",
            method: "get",
            //  data:doctype
        })
          .done(function (obj) {
              var _obj = JSON.parse(obj);
              $("#new_DocNo").val(_obj);
              $("#txt_docNo").val("");
              $("#txt_docDisplayNo").text(_obj);
              $("#DocumentMain_DocNoDisplay").val(_obj);
              $("#btn_new").removeAttr('disabled');
          })
          .fail(function (xhr) {
              $("#btn_new").removeAttr('disabled');
          });
    }
    else {
        $("#txt_docDisplayNo").text(docno);
        $("#txt_docNo").val(docno);
    }
}

//function EditRow(id) {
//    if (!InEditMode) {
//        $('#itemList').attr('disabled','disabled');
//        var typeofId = typeof (id);
//        var rowId = "";
//        if (typeofId == "object") {
//            rowId = id.getAttribute('id');
//        }
//        else {
//            rowId = id; //Type will be string
//        }
//        var d = $(id).find('td');
//        var ctnSize = parseFloat(d.eq(3).text());
//        var numberOfCarton = parseFloat(d.eq(4).text());
//        d.eq(4).html('<input type="number" id="iQtyFromCarton" onkeyup="calculateQtyFromCarton(' + ctnSize + ')" style="width:100%" min="1" value="' + numberOfCarton + '">');
//        var qty = d.eq(5).text();
//        d.eq(5).html('<input type="number" id="iQty" onkeyup="calculateAmount(' + ctnSize + ')" style="width:100%" min="1" value="' + qty + '">');
//        var freeQty = d.eq(6).text();
//        d.eq(6).html('<input type="number" id="ifreeQty" onkeyup="calculateAmount()" style="width:100%" min="1" value="' + freeQty + '">');
//        var retail = d.eq(7).text();
//        d.eq(7).html('<input type="number" id="iRetail" onkeyup="calculateAmount()" style="width:100%" min="1" value="' + retail + '">');
//        var cost = d.eq(8).text();
//        d.eq(8).html('<input type="number" id="iCost" onkeyup="calculateAmount()" style="width:100%" min="1" value="' + cost + '">');
//        var discount = d.eq(9).text();
//        d.eq(9).html('<input type="number" id="iDiscount" onkeyup="calculateAmount()" style="width:100%" min="1" value="' + discount + '">');
//        var amount = d.eq(10).text();
//        d.eq(10).html('<input type="number" id="iAmount" onkeyup="calculateAmount()" style="width:100%" min="1" value="' + amount + '" readonly>');
//        if (typeofId == "object") {
//            d.eq(11).html("<a onclick='UpdateRow(" + rowId + ")' class='fa fa-check' style='color:green;'></a>");
//        }
//        else {
//            d.eq(11).html('<a onclick="UpdateRow(' + "'" + rowId + "'" + ')" class="fa fa-check" style="color:green;"></a>');
//        }
//        $("#iQty").focus().select();
//        InEditMode = true;
//        $('#iQtyFromCarton,#ifreeQty,#iQty,#iRetail,#iCost,#iDiscount,#iAmount').keydown(function (e) {
//            var key = e.which;
//            if (key == 13)  // the enter key code
//            {
//                UpdateRow(id);
//            }
//        });
//    }
//    else {
//        toastr.error('Please Update The First Item Before Editing Another');
//    }
//}

//function UpdateRow(id) {
//    debugger;
//    var typeofId = typeof (id);
//    var rowId = "";
//    if (typeofId == "object") {
//        rowId = id.getAttribute('id');
//    }
//    else {
//        rowId = id; //Type will be string
//    }
//    var d = $(id).find('td');
//    var numberOfCarton = parseFloat(d.eq(4).find("input").val());
//    var qty = parseFloat(d.eq(5).find("input").val());
//    var freeQty = parseFloat(d.eq(6).find("input").val());
//    var retail = parseFloat(d.eq(7).find("input").val());
//    var cost = parseFloat(d.eq(8).find("input").val());
//    var discount = parseFloat(d.eq(9).find("input").val());
//    var amount = parseFloat(d.eq(10).find("input").val());
//    if (cost > retail) {
//        toastr.error('Retail is: ' + retail);
//        toastr.error('Cost Cannot Be Grater Than Retail');
//        return;
//    }
//    if (amount <= 0) {
//        toastr.error('Amount Cant be Negitive or Zero');
//        return;
//    }
//    d.eq(4).text(numberOfCarton);
//    d.eq(5).text(qty);
//    d.eq(6).text(freeQty);
//    d.eq(7).text(retail);
//    d.eq(8).text(cost);
//    d.eq(9).text(discount);
//    d.eq(10).text(amount);
//    if (typeofId == "object") {
//        d.eq(11).html("<a onclick='RemoveRow(" + rowId + ")' class='fa fa-trash' style='color:red;'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditRow(" + rowId + ")' class='fa fa-edit' style='color:blue;'></a>");
//    }
//    else {
//        d.eq(11).html('<a onclick="RemoveRow(' + "'" + rowId + "'" + ')" class="fa fa-trash" style="color:red;"></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick="EditRow(' + "'" + rowId + "'" + ')" class="fa fa-edit" style="color:blue;"></a>');
//    }
//    calculateTotal();
//    InEditMode = false;
//    $('#itemList').removeAttr('disabled');
//}

function RemoveRow(id) {
    InEditMode = false;
    $(id).remove();
    calculateTotal();
}

function calculateQtyFromCarton(ctnPcs) {

    if (ctnPcs == "" || ctnPcs == 0) {
        return;
    }
    var ctnSize = parseFloat(ctnPcs);
    var numberOfCarton = parseFloat($("#iQtyFromCarton").val());
    var result = ctnSize * numberOfCarton;
    $("#iQty").val(result);
    
    calculateAmount();
}


$("body").delegate(".changeable", "blur keyup", function () {
    calculateTotal();
});

function calculateTotal() {
    var totalAmount = 0;
    var TotalQty = 0;
    var TotalDiscount = 0;
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');

        var productName = td.eq(2).text();
        var rowQty = parseFloat(td.eq(5).text());
        if (rowQty <= 0) {
            toastr.error("Qty should be grater than 0")
            return;
        }
        var rowCost = parseFloat(td.eq(8).text());
        var rowDiscount = parseFloat(td.eq(9).text());
        var rowTotal = (rowQty * rowCost) - rowDiscount;
        if (rowDiscount > (rowQty * rowCost)) {
            toastr.error(productName + ":" + "</br>Discount cannot be grater than total amount");
            return;
        }
        td.eq(10).text(rowTotal);
        TotalQty = parseFloat(TotalQty) + rowQty;
        TotalDiscount = parseFloat(TotalDiscount) + rowDiscount;
        totalAmount = parseFloat(totalAmount) + rowTotal;
    }

    $("#txt_totalAmount").val(totalAmount);
    $("#txt_TotalQty").val(TotalQty);
    $("#txt_TotalDiscountAmount").val(TotalDiscount);

}
