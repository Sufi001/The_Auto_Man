var counter = $("#tableWastDetail tr").length;
var InEditMode = false;
$(document).ready(function () {
   
    if ($("#tableWastDetail tr").length == 0) {

        $("#tableWastDetail").hide();
    } else {
        $("#tableWastDetail").show();
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
        var DocumentMain = {};
        var PurchaseDetailList = {};
        var vm = {
            DocumentMain,
            DocumentDetailList: []
        };
        $('#btn_submit').attr('disabled', 'disabled');
        vm.DocumentMain.DocNo = $("#txt_docNo").val();
        vm.DocumentMain.DocDate = $("#txt_docDate").val();
        vm.DocumentMain.DocType = $("#ddl_doctype").val();
        vm.DocumentMain.Location = $("#ddl_location").val();
        vm.DocumentMain.BranchId = $("#ddl_Branch").val();
        if (vm.DocumentMain.DocType == "" || vm.DocumentMain.DocType == null)
        {
            toastr.error('Please Document Type');
            $('#btn_submit').removeAttr("disabled");
            return;
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
            purchaseDetail.amount = td.eq(6).text();
            purchaseDetail.Totalamount = $("#txt_totalAmount").val();
            purchaseDetail.TotalQty = $("#txt_TotalQty").val();
            vm.DocumentDetailList.push(purchaseDetail);
        }
        if (vm.DocumentDetailList.length == 0) {
            toastr.error('Select min one item to save.');
            $('#btn_submit').removeAttr("disabled");
            return;
        }
        $.ajax({
            url: "/StockAdjustment/Save",
            method: "post",
            data: vm
        }).done(function (obj) {
                if (obj == "Invalid") {
                    toastr.error('Data not saved');
                }
                else {
                    toastr.success('Data saved successfully.');
                    $("#txt_docNo").val(obj);
                    $("#btn_print1").removeAttr("disabled");
                    $('#btn_authorize').removeAttr('disabled');
                }
                $('#btn_submit').removeAttr("disabled");

            }).fail(function (xhr) {
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
    vm.DocumentMain.DocType = $("#ddl_doctype").val();
    vm.DocumentMain.Location = $("#ddl_location").val();
    vm.DocumentMain.BranchId = $("#ddl_Branch").val();
    if (vm.DocumentMain.DocType == "" || vm.DocumentMain.DocType == null) {
        toastr.error('Please Document Type');
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
        purchaseDetail.qty = td.eq(3).text();
        purchaseDetail.retail = td.eq(4).text();
        purchaseDetail.cost = td.eq(5).text();
        purchaseDetail.amount = td.eq(6).text();
        purchaseDetail.Totalamount = $("#txt_totalAmount").val();
        purchaseDetail.TotalQty = $("#txt_TotalQty").val();
        vm.DocumentDetailList.push(purchaseDetail);
    }
    if (vm.DocumentDetailList.length == 0) {
        toastr.error('Select min one item to save.');
        $('#btn_authorize').removeAttr("disabled");
        $("#add_btn").removeAttr("disabled");
        return;
    }
    $.ajax({
        url: "/StockAdjustment/Authorize",
        method: "post",
        data: vm
    }).done(function (obj) {
            if (obj == "Invalid") {
                toastr.error('Invalid Query');
            }
            else {
                toastr.success('Document Authorized');
                $("#txt_docNo").val(obj);
                $("#btn_print1").removeAttr("disabled");
                $('#btn_authorize').attr('disabled', 'disabled');
                $("#add_btn").attr('disabled', 'disabled');
                $('#btn_submit').attr('disabled', 'disabled');
            }
        }).fail(function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
            $('#btn_authorize').removeAttr("disabled");
            $('#btn_submit').removeAttr("disabled");
            toastr.error('Invalid Query');
        });
}

function calculateAmount() {
    var IQty = parseFloat($('#iQty').val());
    var iCost = parseFloat($('#iCost').val());
    var amount = (IQty * iCost);
    $('#iAmount').val(amount);
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
    if (qty == "" || qty == null) {
        toastr.error('Enter Quantity');
        return;
    }
    var selectedTr = $(e).find('td');
    $("#tableWastDetail").show();
    var uanno = selectedTr.eq(0).text();
    var icode = selectedTr.eq(1).text();
    var _iname = selectedTr.eq(2).text();
    //var discount = '';
    //var qty ='';
    var cost = selectedTr.eq(4).text();
    var retail = selectedTr.eq(5).text();
    var CtnPcs = selectedTr.eq(6).text();

    var location = $("#ddl_location").val();
    var greateramount = parseFloat(cost) - parseFloat(retail)
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
            var Quantity = parseFloat(td.eq(3).text());
            Quantity = Quantity + qty;
            td.eq(3).text(Quantity)
            td.eq(6).text((Quantity * cost));
        }
        else {
            $("#tblbody").append(
                "<tr id=trrr" + counter + ">" +
                    "<td>" + uanno + "</td>" +
                    "<td style='display:none;'>" + icode + "</td>" +
                    "<td>" + _iname + "</td>" +
                    "<td>" + qty + "</td>" +
                    "<td style='display:none;'>" + retail + "</td>" +
                    "<td>" + cost + "</td>" +
                    "<td>" + qty * cost + "</td>" +
                "</tr>"
                );
            $("#tblbody tr").last().append
                (
                    "<td align='center'><a onclick='RemoveRow(trrr" + counter + ")'class='fa fa-trash' style='color:red'></a>&nbsp;&nbsp;|&nbsp;&nbsp;" +
                    "<a onclick='EditRow(trrr" + counter + ")' class='fa fa-edit' style='color:blue'></a>" +
                    "</td>"
                );
            counter++;
        }
        calculateTotal();
    }
}

function calculateTotal() {
   // var totalAmount = 0;
    var totalAmount = 0;
    var TotalQty = 0;
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        totalAmount = parseFloat(totalAmount) + parseFloat(td.eq(6).text());
        TotalQty = parseFloat(TotalQty) + parseFloat(td.eq(3).text());
    }
    $("#txt_TotalQty").val(TotalQty);
    $("#txt_totalAmount").val(totalAmount);
}

function EditRow(id) {
    if (!InEditMode) {
        $('#itemList').attr('disabled', 'disabled');
        var typeofId = typeof (id);
        var rowId = "";
        if (typeofId == "object") {
            rowId = id.getAttribute('id');
        }
        else {
            rowId = id; //Type will be string
        }

        var d = $(id).find('td');
        var qty = d.eq(3).text();
        d.eq(3).html('<input type="number" id="iQty" style="width:100%" min="1" value="' + qty + '">');
       
        var retail = d.eq(4).text();
        var cost = d.eq(5).text();
        d.eq(5).html('<input type="number" id="iCost" style="width:100%" min="0" value="' + cost + '">');
       
        var amount = d.eq(6).text();
        d.eq(6).html('<input type="number" id="iAmount" style="width:100%" min="0" value="' + amount + '" readonly>');

        if (typeofId == "object") {
            d.eq(7).html("<a onclick='UpdateRow(" + rowId + ")' class='fa fa-check' style='color:green;'></a>");
        }
        else {
            d.eq(7).html('<a onclick="UpdateRow(' + "'" + rowId + "'" + ')" class="fa fa-check" style="color:green;"></a>');
        }
        $("#iQty").focus().select();
        InEditMode = true;
        $('#iQty,#iRetail,#iCost,#iAmount').keydown(function (e) {
            var key = e.which;
            if (key == 13)  // the enter key code
            {

                UpdateRow(id);
            }
        });
    }
    else {
        toastr.error('Please Update The First Item Before Editing Another');
    }
}

function UpdateRow(id) {
    var typeofId = typeof (id);
    var rowId = "";
    if (typeofId == "object") {
        rowId = id.getAttribute('id');
    }
    else {
        rowId = id; //Type will be string
    }
    var d = $(id).find('td');
  
    var qty = parseFloat(d.eq(3).find("input").val());
    var retail = parseFloat(d.eq(4).text());
    var cost = parseFloat(d.eq(5).find("input").val());
    var amount = parseFloat(d.eq(6).find("input").val());

    calculateAmount();

    if (cost > retail) {
        toastr.error('Retail is: ' + retail);
        toastr.error('Cost Cannot Be Grater Than Retail');
        return;
    }
    if (amount <= 0) {
        toastr.error('Amount Cant be Negitive or Zero');
        return;
    }


    d.eq(3).text(qty);

    d.eq(5).text(cost);

    d.eq(6).text(qty * cost);

    if (typeofId == "object") {
        d.eq(7).html("<a onclick='RemoveRow(" + rowId + ")' class='fa fa-trash' style='color:red;'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditRow(" + rowId + ")' class='fa fa-edit' style='color:blue;'></a>");
    }
    else {
        d.eq(7).html('<a onclick="RemoveRow(' + "'" + rowId + "'" + ')" class="fa fa-trash" style="color:red;"></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick="EditRow(' + "'" + rowId + "'" + ')" class="fa fa-edit" style="color:blue;"></a>');
    }
    calculateTotal();
    InEditMode = false;
    $('#itemList').removeAttr('disabled');
}

function clearRow() {
    $("#txt_uanno").val("");
    $("#txt_qty").val("0");
    $("#txt_cost").val("");
    $("#txt_retail").val("");
    $("#txt_amount").val("");
}

function ClearForm(flag) {
    
    $("#ddl_itemName").val("");
    $("#ddl_Branch").val("");
    $("#txt_docNo").val("");
    $("#tableWastDetail").hide();
    $("#txt_totalAmount").val(""); 
    $("#ddl_doctype").val("");
    $("#txt_status").val("");
    $("#txt_TotalQty").val("");
    $("#txt_qty").val("0");
    $("#tblbody tr").remove();
    $("#btn_print1").attr('disabled', 'disabled');
    $("#ddl_doctype").removeAttr("disabled");
    $("#ddl_doctype").val("W");
    $('#btn_authorize').removeAttr("disabled");
    $('#btn_submit').removeAttr("disabled");
    $('#add_btn').removeAttr("disabled");
    $('#btn_save').removeAttr("disabled");
    clearRow();
}

function RemoveRow(id) {
    InEditMode = false;
    $(id).remove();
    calculateTotal();
}

$("#txt_uanno").on("keydown", function (event) {
    if (event.which == 13) {
        var Uan = $('#txt_uanno').val();
        if (Uan != "") {
            $.ajax({
                url: "/api/Product?id=" + "" + "&UanNo=" + Uan + "&Name=",
                type: "GET",
                dataType: "JSON",
                success: function (_data) {
                    debugger;
                    var obj = JSON.parse(_data);
                    if (obj == null) {
                        alert("null");
                        return;
                    }
                    CreateProductTable(obj);
                },
                error: function (xhr) {
                    alert('Request Status: \n ' +
                        xhr.status +
                        ' Status Text: ' +
                        xhr.statusText +
                        ' ' +
                        xhr.responseText);
                }
            })
        }
        else {
            GetSupplierProducts();
        }
    }
});
$("#myInput").on("keydown", function (event) {
    if (event.which == 13) {
        var ProductName = $('#myInput').val();
        if (ProductName != "") {
            $.ajax({
                url: "/api/Product?id=" + "" + "&UanNo=" + "" + "&Name=" + ProductName,
                type: "GET",
                dataType: "JSON",
                success: function (_data) {
                    var obj = JSON.parse(_data);
                    if (obj == null) {
                        alert("null");
                        return;
                    }
                    CreateProductTable(obj);
                },
                error: function (xhr) {
                    alert('Request Status: \n ' +
                        xhr.status +
                        ' Status Text: ' +
                        xhr.statusText +
                        ' ' +
                        xhr.responseText);
                }
            })
        }
        else {
            GetSupplierProducts();
        }
    }
});
$("#myInput").on("keyup", function () {
    var value = $(this).val().toLowerCase();
    $("#ItemBody tr").filter(function () {
        $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
    });
});