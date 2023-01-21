var counter = $("#tablePurchaseDetail tr").length;
var getProductInfo = true;
$(document).ready(function ()
{
    if ($("#tablePurchaseDetail tr").length == 1)
    {
        $("#tablePurchaseDetail").hide();
    }
    else
    {
        calculateTotal();
    }
    if ($("#txt_docNo").val() != "" && $("#txt_status").val() != 3) {
        $("#btn_print1").removeAttr('disabled');
        $("#btn_authorize").removeAttr('disabled');
    }
    else {
        $('#btn_authorize').attr('disabled', 'disabled');
    }
    if ($("#txt_status").val() == 3) {
        $('#btn_authorize').attr('disabled', 'disabled');
        $("#add_btn").attr('disabled', 'disabled');
        $('#btn_save').attr('disabled', 'disabled');
        $("#btn_print1").removeAttr('disabled');
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
        vm.DocumentMain.RequestPage = $("#RequestPage").val();
        vm.DocumentMain.DocNo = $("#txt_docNo").val();
        vm.DocumentMain.DocDate = $("#txt_docDate").val();
        vm.DocumentMain.DocType = $("#txt_doctype").val();
        vm.DocumentMain.Location = $("#ddl_location").val();
        vm.DocumentMain.staffcode = $("#ddl_staff").val();
        vm.DocumentMain.SuplCode = $("#ddl_supplier").val(); 
        vm.DocumentMain.CustomerName = $("#ddl_supplier option:selected").text();
        vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
        vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();
        if (vm.DocumentMain.SuplCode == "") {
            toastr.error('Please Select One Supplier.');
            $('#btn_save').removeAttr("disabled");
            return;
        }
        var tr = $("#tblbody").find('tr');
        for (var ind = 0; ind < tr.length; ind++) {
            var purchaseDetail = {};
            var td = $(tr[ind]).find('td');

            purchaseDetail.uanno = td.eq(0).text();
            purchaseDetail.barcode = td.eq(1).text();
            purchaseDetail.CtnQty = td.eq(4).text();
            purchaseDetail.qty = td.eq(5).text();
            purchaseDetail.FreeQty = td.eq(6).text();
            purchaseDetail.retail = td.eq(7).text();
            purchaseDetail.cost = td.eq(8).text();
            purchaseDetail.discount = td.eq(12).text();
            purchaseDetail.amount = td.eq(13).text();
            purchaseDetail.DateTimeSlot = vm.DocumentMain.DocDate;
            vm.DocumentDetailList.push(purchaseDetail);
        }
        if (vm.DocumentDetailList.length == 0) {
            toastr.error('Select min one item to save.');
            $('#btn_save').removeAttr("disabled");
            return;
        }
        debugger;
        var requestPage = $("#RequestPage").val();
        var URL = "";
        if (requestPage == "OrderBooking") {
            URL = "/Sales/SaveDn";
        }
        else {
            URL = "/api/Sales";
        }
        $.ajax({
            url: URL,
            // url:"/salesave/Index",
            method: "post",
            data: vm
        }).done(function (obj) {
            
                toastr.success('Data saved successfully.');
                $('#btn_save').removeAttr("disabled");
                $("#txt_docNo").val(obj);
                $("#DocumentMain_DocNoDisplay").val(obj);
                $("#btn_print1").removeAttr('disabled');
                $("#btn_authorize").removeAttr('disabled');
                if (requestPage == "OrderBooking") {

                    setTimeout(function () { window.location.href = obj.Url; }, 1500);
                    
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
    });
});
function Authorize() {
    var DocumentMain = {};
    var PurchaseDetailList = {};
    var vm = {
        DocumentMain,
        DocumentDetailList: []
    };
    $('#btn_authorize').attr('disabled', 'disabled');
    $("#add_btn").attr('disabled', 'disabled');
    $('#btn_save').attr('disabled', 'disabled');
    vm.DocumentMain.RequestPage = $("#RequestPage").val();
    vm.DocumentMain.DocNo = $("#txt_docNo").val();
    vm.DocumentMain.DocDate = $("#txt_docDate").val();
    vm.DocumentMain.DocType = $("#txt_doctype").val();
    vm.DocumentMain.Location = $("#ddl_location").val();
    vm.DocumentMain.staffcode = $("#ddl_staff").val();
    vm.DocumentMain.SuplCode = $("#ddl_supplier").val();
    vm.DocumentMain.CustomerName = $("#ddl_supplier option:selected").text();
    vm.DocumentMain.TotalAmount = $("#txt_totalAmount").val();
    vm.DocumentMain.DiscointAmount = $("#txt_TotalDiscountAmount").val();
    if (vm.DocumentMain.SuplCode == "") {
        toastr.error('Please Select One Supplier.');
        $('#btn_save').removeAttr("disabled");
        return;
    }
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++)
    {
        var purchaseDetail = {};
        var td = $(tr[ind]).find('td');

        purchaseDetail.uanno = td.eq(0).text();
        purchaseDetail.barcode = td.eq(1).text();
        purchaseDetail.qty = td.eq(5).text();
        purchaseDetail.FreeQty = td.eq(6).text();
        purchaseDetail.retail = td.eq(7).text();
        purchaseDetail.cost = td.eq(8).text();
        purchaseDetail.discount = td.eq(12).text();
        purchaseDetail.amount = td.eq(13).text();
        purchaseDetail.Totalamount = $("#txt_totalAmount").val();
        purchaseDetail.TotalQty = $("#txt_TotalQty").val();
        purchaseDetail.DateTimeSlot = vm.DocumentMain.DocDate;
        vm.DocumentDetailList.push(purchaseDetail);
    }
    if (vm.DocumentDetailList.length == 0)
    {
        toastr.error('Select min one item to save.');
        $('#btn_save').removeAttr("disabled");
        $('#btn_authorize').removeAttr("disabled");
        $("#add_btn").removeAttr("disabled");
        return;
    }
    $.ajax({
        url: "/api/Authorize",
        method: "post",
        data: vm
    }).done(function (obj) {
            toastr.success('Data saved successfully.');
            $("#txt_docNo").val(obj);
            $("#btn_print1").removeAttr('disabled');
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
        });
}
function calculateAmount() {
    debugger;
    var retail = $('#txt_retail').val();
    var calculation = $('#txt_qty').val() * retail - $('#txt_discount').val();
    $('#txt_retail').val(retail);
    $('#txt_amount').val(calculation);
}
function getProduct() {
    if (getProductInfo) {
    var Barcode = $('#ddl_itemName').val();
    $.ajax({
        url: "/api/Product?id=" + Barcode + "&UanNo=" + "" + "&Name=",
        type: "GET",
        dataType: "JSON",
        success: function (_data) {
            var obj = JSON.parse(_data);
            $.each(obj, function (index, obj) {
                $("#txt_uanno").val(obj.Uanno);
                $("#txt_discount").val('0');
                $("#txt_retail").val(obj.Retail);
                $("#txt_cost").val(obj.Cost);
                $("#txt_qtyinHand").val(obj.QtyinHand);
                $("#txt_CtnPcs").val(obj.Ctnpcs);
                $("#txt_PackQty").val(obj.PackQty);
                $("#txt_PackRetail").val(obj.PackRetail);
                $("#txt_UnitRetail").val(obj.Retail);
                $("#txt_qty").val("1");
                $("#txt_qty").focus().select();
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
    });
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
function showTable() {
    $("#tablePurchaseDetail").show();
    var icode = $("#ddl_itemName").val();
    var _iname = $("option:selected", "#ddl_itemName").html();
    var discount = $("#txt_discount").val();
    var qty = $("#txt_qty").val();
    var cost = $("#txt_cost").val();
    var retail = $("#txt_retail").val();
    var amount = qty * retail - discount;
    var CtnPcs = $("#txt_CtnPcs").val();
    var uanno = $("#txt_uanno").val();
    var freeQty = $("#txt_FreeQty").val();
    var location = $("#ddl_location").val();
    var numberOfCarton = $("#txt_ctn").val();
    var ProductUnitRetail = $("#txt_UnitRetail").val();
    var ProductPackRetail = $("#txt_PackRetail").val();
    var ProductPackQty = $("#txt_PackQty").val();

    if (location == "") {
        toastr.error('Select Location to add Item.');
        return;
    }
    if (parseFloat(cost) > parseFloat(retail)) {
        toastr.error("Retail Should be greater than origional Price and the origional Price is " + cost + "");
        return;
    }
    else if (icode == "" || qty == "" || retail == "") {
        toastr.error('ItemName,price and Quantity is required.');
    }
    else if (isNaN(qty) || isNaN(cost) ||isNaN(discount)) {
        toastr.error('Quantity, price and discount can only contains numerics.');
    }
    //else if (!qty.match("^[0-9]+(.[0-9])*$") ||
    //!cost.match("^[0-9]+(.[0-9])*$") ||
    //!discount.match("^[0-9]+(.[0-9])*$")) {
    //    toastr.error('Quantity, price and discount can only contains numerics.');

    //}
    else {
        var index = checkDuplicate(icode);
        if (index != -1) {
            var tr = $("#tblbody").find('tr');
            var td = $(tr[index]).find('td');

            var previousQty = parseFloat(td.eq(5).text());
            var currentQty = parseFloat(qty) + previousQty;
            td.eq(5).text(currentQty);
            var previousDiscount = parseFloat(td.eq(9).text());
            var currentDiscount = parseFloat(discount) + previousDiscount;
            td.eq(9).text(currentDiscount);

            if (currentQty >= ProductPackQty) {
                retail = $("#txt_PackRetail").val();
            }
            else {
                retail = $("#txt_UnitRetail").val();
            }

            td.eq(7).text(retail);
            td.eq(10).text((parseFloat(td.eq(5).text()) * parseFloat(td.eq(7).text())) - parseFloat(td.eq(9).text()));
        }
        else {
            var Html = "<tr id=trrr" +counter + ">"+ 
                "<td>" + uanno + "</td>" +
                "<td style='display:none;'>" + icode + "</td>" +
                "<td>" + _iname +"</td>"+
                "<td>" + CtnPcs + "</td>" +
                "<td>" + numberOfCarton + "</td>" +
                "<td>" + qty + "</td>" +
                "<td>" + freeQty + "</td>" +
                "<td>" + retail + "</td>" +
                "<td style='display:none;'>" + cost + "</td>" +
                "<td style='display:none;'>" + ProductUnitRetail + "</td>" +
                "<td style='display:none;'>" + ProductPackRetail + "</td>" +
                "<td style='display:none;'>" + ProductPackQty + "</td>" +
                "<td>" + discount + "</td>" +
                "<td>" + amount + "</td>" +
                "</tr>"
            $("#tblbody").append(Html);
            $("#tblbody tr").last().append(
                "<td><a onclick='RemoveRow(trrr" + counter + "," + counter + ")' class='fa fa-trash' style='color:red'></a>&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditRow(trrr" +
                counter +
                "," +
                counter +
                ")' class='fa fa-edit' style='color:blue'></a></td>");
            counter++;
        }
        clearRow();
        calculateTotal();
    }
}
function calculateTotal() {
    var totalAmount = 0;
    var TotalQty = 0;
    var TotalDiscount = 0;
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        totalAmount = parseFloat(totalAmount) + parseFloat(td.eq(13).text());
        TotalQty = parseFloat(TotalQty) + parseFloat(td.eq(5).text());
        TotalDiscount = parseFloat(TotalDiscount) + parseFloat(td.eq(12).text());
    }
    $("#txt_totalAmount").val(totalAmount);
    $("#txt_TotalQty").val(TotalQty);
    $("#txt_TotalDiscountAmount").val(TotalDiscount);
    //alert(TotalQty);
}
function clearRow() {
    $("#ddl_itemName").val("");
    $("option:selected", "#ddl_itemName").html("Select Item");
    $("#txt_uanno").val("");
    $("#txt_CtnPcs").val("");
    $("#txt_ctn").val("");
    $("#txt_qty").val("");
    $("#txt_FreeQty").val("0");
    $("#txt_discount").val("0");
    $("#txt_cost").val("");
    $("#txt_retail").val("");
    $("#txt_amount").val("");
    $("#txt_qtyinHand").val("");
    $("#txt_PackQty").val("0");
    $("#txt_PackRetail").val("0");
    $("#txt_UnitRetail").val("0");
}
function ClearForm(flag) {
    $("#ddl_itemName").val("");
    $("#txt_FreeQty").val("0");
    $('#btn_save').removeAttr("disabled");
    $('#btn_authorize').removeAttr("disabled");
    $('#btn_print1').attr('disabled', 'disabled');
    $("#add_btn").removeAttr("disabled");
    $("#ddl_warehouse").val("");
    $("#txt_Balance").val("0");
    $("#txt_docNo").val("");
    $("#txt_totalAmount").val("");
    $("#txt_TotalQty").val("");
    $("#txt_TotalDiscountAmount").val("");
    $("#tblbody tr").remove();
    $("#DocumentMain_DocNoDisplay").val("");
    $("#DocumentMain_DocNoDisplay").attr("placeholder", "New");
    clearRow();
}
function EditRow(id) {
    debugger;
    var d = $(id).find('td');
    getProductInfo = false;
    //$("#ddl_itemName").val(d.eq(1).text());
    $("#ddl_itemName").val(d.eq(1).text()).trigger('change');
    $("#txt_uanno").val(d.eq(0).text());
    $("#txt_CtnPcs").val(d.eq(3).text());
    $("#txt_ctn").val(d.eq(4).text());
    $("#txt_qty").val(d.eq(5).text());
    $("#txt_FreeQty").val(d.eq(6).text());
    var retail = d.eq(7).text();
    $("#txt_retail").val(retail);
    $("#txt_cost").val(d.eq(8).text());
    $("#txt_UnitRetail").val(d.eq(9).text());
    $("#txt_PackRetail").val(d.eq(10).text());
    $("#txt_PackQty").val(d.eq(11).text());
    $("#txt_discount").val(d.eq(12).text());
    $("#txt_amount").val(d.eq(13).text());
    getProductInfo = true;
   // getProduct();
    RemoveRow(id);
}
function RemoveRow(id)
{
    $(id).remove();
    calculateTotal();
}
function SelectSalesItemRetail() {
    var packQty = parseFloat($("#txt_PackQty").val());
    var qty = parseFloat($("#txt_qty").val());
    if (qty >= packQty) {
        $("#txt_retail").val($("#txt_PackRetail").val());
    }
    else {
        $("#txt_retail").val($("#txt_UnitRetail").val());
    }
}
$('#txt_qty').keyup(function () {
    SelectSalesItemRetail();
});
$('#txt_ctn').keyup(function () {
    var numberOfCarton = parseFloat($('#txt_ctn').val());
    var ctnSize = parseFloat($('#txt_CtnPcs').val());
    if (!isNaN(numberOfCarton) && !isNaN(ctnSize)) {
        $('#txt_qty').val(numberOfCarton * ctnSize);
    }

    SelectSalesItemRetail();
});
$("#txt_uanno").on("keydown", function (event) {
    if (event.which == 13) {
        var Uan = $('#txt_uanno').val();
        if (Uan != "") {
            $.ajax({
                url: "/api/Product?id=" + "" + "&UanNo=" + Uan + "&Name=",
                type: "GET",
                dataType: "JSON",
                success: function (_data) {
                    var obj = JSON.parse(_data);
                    if (obj == null) {
                        alert("null");
                        return;
                    }
                    var obj = JSON.parse(_data);
                    $.each(obj, function (index, obj) {
                        $("#txt_uanno").val(obj.Uanno);
                        $("#txt_discount").val('0');
                        $("#txt_retail").val(obj.Retail);
                        $("#txt_cost").val(obj.Cost);
                        $("#txt_qtyinHand").val(obj.QtyinHand);
                        $("#txt_CtnPcs").val(obj.Ctnpcs);
                        $("#txt_qty").val("1");
                        $('#ddl_itemName').val(obj.Barcode);
                        $("#txt_PackQty").val(obj.PackQty);
                        $("#txt_PackRetail").val(obj.PackRetail);
                        $("#txt_UnitRetail").val(obj.Retail);
                        $("#txt_qty").focus().select();
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
            })
        }
    }
});
$('#add_btn').keydown(function (e) {
    var key = e.which;
    if (key == 13)  // the enter key code
    {
        $("#add_btn").click();
    }
});

