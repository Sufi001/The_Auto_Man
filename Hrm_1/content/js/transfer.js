var counter = $("#tableTransferDetail tr").length;
var getProductInfo = true;

$(document).ready(function() {
    if ($("#transferDetail_Table tr").length == 1) {

        $("#tableTransferDetail").hide();
    }
    else
    {
        $("#tableTransferDetail").show();
        calculateTotal();
    }
    //styleWarehouse();
    HideAll();

    if ($("#txt_status").val() == 3)
    {
        $('#add_btn').attr('disabled', 'disabled');
        $("#btn_print1").removeAttr("disabled");
       // $('#btn_submit')
    }
    if ($("#txt_status").val() == 0)
    {
        $("#btn_authorize").removeAttr("disabled");
    }
    if ($("#txt_docno").val() !="") {
      
        $("#btn_print1").removeAttr("disabled");
        // $('#btn_submit')
      //  $("#tableTransferDetail").hide();
    }
    $('#tform').submit(function(e) {
        e.preventDefault();

        var TransferMain = {};
        var transferDetailList = {};
        var vm = {
            TransferMain,
            TransferDetailList: []
        };
        $('#btn_submit').attr('disabled', 'disabled');
        $('#btn_authorize').attr('disabled', 'disabled');
        vm.TransferMain.doc_no = $("#txt_docno").val();
        vm.TransferMain.doc_date = $("#txt_docDate").val();
        vm.TransferMain.doc_type = $("#txt_doctype").val();
        vm.TransferMain.userid = $("#txt_userid").val();
        vm.TransferMain.BranchIdFrom = $("#ddl_BranchIdFrom").val();
        vm.TransferMain.BranchIdTo = $("#ddl_BranchIdTo").val();
       // vm.TransferMain.location = $("#ddl_location").val();
        vm.TransferMain.location = $("#ddl_LocationType").val();
        vm.TransferMain.warehouse = $("#ddl_warehouse").val();

        if (vm.TransferMain.location == "" || vm.TransferMain.location == null) {
            toastr.error("Select Location");
            return;
        }
        //if (vm.TransferMain.warehouse == "" || vm.TransferMain.warehouse == null) {
        //    toastr.error("Select Warehouse");
        //    return;
        //}
        if (vm.TransferMain.doc_type == "I" && vm.TransferMain.BranchIdFrom == vm.TransferMain.BranchIdTo) {
            toastr.error("Can't transfer to same branch");
            return;
        }

        if (vm.TransferMain.doc_type == "I" && !vm.TransferMain.BranchIdFrom) {
            toastr.error("Select transfer from branch");
            return;
        }
        if (!vm.TransferMain.BranchIdTo) {
            toastr.error("Select branch to transfer");
            return;
        }
        var tr = $("#tblbody").find('tr');
        for (var ind = 0; ind < tr.length; ind++) {
            var _TransferDetail = {};
            var td = $(tr[ind]).find('td');
            _TransferDetail.uanno = td.eq(0).text();
            _TransferDetail.barcode = td.eq(1).text();
            _TransferDetail.qty = td.eq(3).text();
            _TransferDetail.usize = "1";//td.eq(4).text();
            _TransferDetail.cost = td.eq(5).text();
            _TransferDetail.retail = td.eq(6).text();
            _TransferDetail.DocNo = td.eq(8).text();
            vm.TransferDetailList.push(_TransferDetail);
        }
        if (vm.TransferDetailList.length == 0) {
            toastr.error('Select min one item to save.');
            $('#btn_submit').removeAttr("disabled");
            $("#btn_authorize").removeAttr("disabled");
            return;
        }
        $.ajax({
            url: "/api/Transfer",
            method: "post",
            data: vm
        })
            .done(function(obj) {

                toastr.success('Data saved successfully.');
                $("#txt_docno").val(obj);
                $("#btn_authorize").removeAttr("disabled");
                $('#btn_submit').removeAttr("disabled");
                $("#btn_print1").removeAttr("disabled");
            })
            .fail(function(xhr) {
                alert('Request Status: \n ' +
                    xhr.status +
                    ' Status Text: ' +
                    xhr.statusText +
                    ' ' +
                    xhr.responseText);
                $('#btn_submit').removeAttr("disabled");
                $("#btn_authorize").removeAttr("disabled");
                toastr.error('Data not saved');

            });
    });

});

function getProduct() {
    if (getProductInfo) {

        var barcode = $('#ddl_itemName').val();

        getMaxTransferQty(barcode, "");
    $.ajax({
        url: "/api/Product?id=" + barcode + "&UanNo=" + "" + "&Name=",
        dataType: "JSON",
        success: function (_data) {
            var obj = JSON.parse(_data);

            $.each(obj, function (index, obj) {
                $("#txt_uanno").val(obj.Uanno);
                $("#txt_retail").val(obj.Retail);
                $("#txt_cost").val(obj.Cost);
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
    }
    );
    }
}

function styleWarehouse() {

    $('#ddl_warehouse').append(new Option('Add New', ''));
    $('#ddl_warehouse option:last-child').css("color", "blue");
}

function AddWarehouse() {
    var v = jQuery("#ddl_warehouse option:selected").text();
    if (v == 'Add New') {
        $("#txt_warehousediv").show();
        $('#txt_warehousediv').attr('disabled', 'disabled');

    } else {
        $("#txt_warehousediv").hide();
        $('#btn_submit').removeAttr('disabled');
    }
}

function FillWarehouse() {
    HideAll();

    var stateId = $('#ddl_location').val();

    $.ajax({
        url: "/api/Warehouse?id=" + stateId,
        dataType: "JSON",

        success: function(_data) {
            var obj = JSON.parse(_data);
            $('#ddl_warehouse').empty();
            $('#ddl_warehouse').append(new Option('Select Warehouse', ''));
            $.each(obj,
                function(index, element) {
                    $('#ddl_warehouse').append(new Option(obj[index].Name, obj[index].id));

                });
            styleWarehouse();
        },
        error: function(xhr) {
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

function saveWarehouse() {
    var vm = {
    };
    vm.id = '';
    vm.Name = $("#txt_warehouse").val();
    vm.loc_id = $("#ddl_location").val();
    $.ajax({
        url: "/api/Warehouse",
        method: "post",
        data: vm
    })
        .done(function(obj) {
            var lastind;
            $('#ddl_warehouse').empty();
            $('#ddl_warehouse').append(new Option('Select Warehouse', ''));
            $.each(obj,
                function(index, element) {
                    $('#ddl_warehouse').append(new Option(obj[index].Name, obj[index].id));
                    lastind = obj[index].id;
                });
            toastr.success('Warehouse saved successfully.');
            $('#ddl_warehouse').val(lastind);
            styleWarehouse();
            HideAll();
            $('#btn_submit').removeAttr('disabled');
        })
        .fail(function(xhr) {
            toastr.error(xhr, 'Data not saved.');
        });

}

function showTable() {
    $("#tableTransferDetail").show();
    var icode = $("#ddl_itemName").val();
    var _iname = $("option:selected", "#ddl_itemName").html();
    var usize = $("#txt_usize").val();
    var qty = $("#txt_qty").val();
    var cost = $("#txt_cost").val();
    var retail = $("#txt_retail").val();
    var uanno = $("#txt_uanno").val();

    var maxtransferQty = parseInt($('#max_transfer_qty').val());

    if (!maxtransferQty || maxtransferQty < 0) {
        toastr.error("Not in stock");
        return;
    }
    else if (qty > maxtransferQty) {
        toastr.error("Max available quantity for transfer is: " + maxtransferQty);
        return;
    }
    if (icode == "" || qty == ""||cost==""||retail=="")
    {
        toastr.error('ItemName,Quantity,Cost and Retail is required.');
        return;
    }
    else if (!qty.match("^[0-9]+(.[0-9])*$"))
    {
        toastr.error('Quantity can only contains numerics.');
        return;
    }
    else if (parseInt(cost) > parseInt(retail))
    {
        toastr.error('Retail Should be greater than origional Price and the origional Price is  ' + cost)
        return;
    }
    else
    {
        var index = checkDuplicate(icode);
        if (index != -1) {
            var tr = $("#tblbody").find('tr');
            var td = $(tr[index]).find('td');
            //td.eq(3).text(parseInt(qty) + parseInt(td.eq(3).text()));
            //td.eq(7).text(td.eq(7).text() + ((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text())));
            var totalQty = parseInt(qty) + parseInt(td.eq(3).text())
            if (totalQty > maxtransferQty) {
                toastr.error("Max available quantity for transfer is: " + maxtransferQty);
                return;
            }
            else {

            td.eq(3).text(totalQty);
            td.eq(4).text(parseInt(usize) + parseInt(td.eq(4).text()));
            td.eq(5).text(parseInt(cost));
            td.eq(6).text(parseInt(retail) + parseInt(td.eq(6).text()));
          //  td.eq(4).text(retail);
            // td.eq(7).text(parseInt(td.eq(7).text()) + ((parseInt(td.eq(3).text()) * parseInt(td.eq(5).text())) - parseInt(td.eq(6).text())));
          //  td.eq(7).text((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text()));
            }

        }
        else {
            $("#tblbody").append("<tr id=trrr" + counter + ">" +
                "<td>" + uanno + "</td>" +
                "<td style='display:none;'>" + icode + "</td>" +
                "<td>" + _iname + "</td>" +
                "<td class='changeable' contenteditable='true'>" + qty + "</td>" +
                "<td class='changeable' contenteditable='true' style='display:none;'>" + usize + "</td>" +
                "<td class='changeable' contenteditable='true'>" + cost + "</td>" +
                "<td class='changeable' contenteditable='true'>" + retail + "</td>" +
                "<td style='display:none;'>" + maxtransferQty + "</td>" +
                "<td style='display:none;'>" + "" + "</td>" +
                "</tr>"
                );

            $("#tblbody tr").last().append(
                "<td align='center'><a onclick='RemoveRow(trrr" + counter + "," + counter + ")' class='fa fa-trash' style='color:red;'></a>"
                //+"&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditRow(trrr" + counter + "," + counter + ")' class='fa fa-edit' style='color:blue;'></a></td>"
                );
            counter++;
            clearRow();
            calculateTotal();
        }
        clearRow();
        calculateTotal();
        $('#max_transfer_qty').val("");
    }

}
//function calculateTotal() {
//    // var totalAmount = 0;
//    var totalAmount = 0; 
//    var calculatedamount = 0;
//    var Calculatedqty = 0;
//    var tr = $("#tblbody").find('tr');
//    for (var ind = 0; ind < tr.length; ind++)
//    {
//        var TotalQty = 0;
//        var FullTotalAmount = 0;
//        var td = $(tr[ind]).find('td');
//        totalAmount = parseInt(FullTotalAmount) + parseInt(td.eq(5).text());
//        TotalQty = parseInt(TotalQty) + parseFloat(td.eq(3).text());
//        FullTotalAmount = totalAmount * TotalQty;
//        calculatedamount = calculatedamount + FullTotalAmount;
//        Calculatedqty=Calculatedqty+TotalQty
//    }
//    $("#txt_TotalQty").val(Calculatedqty);
//    $("#txt_totalAmount").val(calculatedamount);
//}
function Authorize() {
    var TransferMain = {};
    var transferDetailList = {};
    var vm = {
        TransferMain,
        TransferDetailList: []
    };
    vm.TransferMain.doc_no = $("#txt_docno").val();
    vm.TransferMain.doc_date = $("#txt_docDate").val();
    vm.TransferMain.doc_type = $("#txt_doctype").val();
    vm.TransferMain.location = $("#ddl_LocationType").val();
    vm.TransferMain.warehouse = $("#ddl_warehouse").val();
    vm.TransferMain.BranchIdFrom = $("#ddl_BranchIdFrom").val();
    vm.TransferMain.BranchIdTo = $("#ddl_BranchIdTo").val();

    if (vm.TransferMain.location == "" || vm.TransferMain.location == null) {
        toastr.error("Select Location");
        return;
    }

    if (vm.TransferMain.doc_type == "I" && vm.TransferMain.BranchIdFrom == vm.TransferMain.BranchIdTo) {
        toastr.error("Can't transfer to same branch");
        return;
    }
    
    //if (vm.TransferMain.warehouse == "" || vm.TransferMain.warehouse == null) {
    //    toastr.error("Select Warehouse");
    //    return;
    //}
    if (vm.TransferMain.doc_type == "I" && !vm.TransferMain.BranchIdFrom) {
        toastr.error("Select transfer from branch");
        return;
    }
    if (!vm.TransferMain.BranchIdTo) {
        toastr.error("Select branch to transfer");
        return;
    }
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var _TransferDetail = {};
        var td = $(tr[ind]).find('td');
        _TransferDetail.uanno = td.eq(0).text();
        _TransferDetail.barcode = td.eq(1).text();
        _TransferDetail.qty = td.eq(3).text();
        _TransferDetail.usize = td.eq(4).text();
        _TransferDetail.cost = td.eq(5).text();
        _TransferDetail.retail = td.eq(6).text();
        _TransferDetail.Totalamount = $("#txt_totalAmount").val();
        _TransferDetail.TotalQty = $("#txt_TotalQty").val();
        vm.TransferDetailList.push(_TransferDetail);
    }
    if (vm.TransferDetailList.length == 0) {
        toastr.error('Select min one item to save.');
        return;
    }
    $.ajax({
        url: "/api/AuthorizeTransfer",
        method: "post",
        data: vm
    }).done(function (obj) {
            toastr.success('Data Authorized successfully.');
            $('#btn_save').attr('disabled', 'disabled');
            $('#btn_authorize').attr('disabled', 'disabled');
            $("#add_btn").attr('disabled', 'disabled');
            $("#txt_docNo").val(obj);
            $("#txt_status").val("3");

        }).fail(function (xhr) {
            alert('Request Status: \n ' +
                xhr.status +
                ' Status Text: ' +
                xhr.statusText +
                ' ' +
                xhr.responseText);
            toastr.error('Data not Authorized');
            $('#btn_save').removeAttr("disabled");
            $('#btn_authorize').removeAttr("disabled");
            $("#add_btn").removeAttr("disabled");

        });
}

function clearRow() {
    $("#ddl_itemName").val("");
    $("#txt_uanno").val("");
    $("#txt_qty").val("");
    $("#txt_usize").val("");
    $("#txt_cost").val("");
    $("#txt_retail").val("");
    $('#max_transfer_qty').val("");
}

function ClearForm() {
    $("#ddl_itemName").val("");
    $("#ddl_location").val("");
    $("#ddl_warehouse").val("");
    $("#txt_docno").val("");
    $("#tblbody tr").remove();
    $("#tableTransferDetail").hide();
    $('#btn_save').removeAttr("disabled");
    $("#add_btn").removeAttr("disabled");
    $('#btn_authorize').attr('disabled', 'disabled');
    $('#btn_print1').attr('disabled', 'disabled');

    debugger;
    var doc = $("#txt_doctype").val();
    if (doc == "I") {
        window.location.replace("/Transfer/TransferPage");
    }
    else {
        window.location.replace("/Transfer/TransferPageOut");
    }


    clearRow();
}

//function EditRow(id) {
//    getProductInfo = false;
//    var d = $(id).find('td');
//    $("option:selected", "#ddl_itemName").html(d.eq(2).text());
//    //$("#ddl_itemName").val(d.eq(1).text());
//    $("#ddl_itemName").val(d.eq(1).text()).trigger('change');
//    $("#txt_uanno").val(d.eq(0).text());
//    $("#txt_qty").val(d.eq(3).text());
//    $("#txt_usize").val(d.eq(4).text());
//    $("#txt_cost").val(d.eq(5).text());
//    $("#txt_retail").val(d.eq(6).text());

//    getMaxTransferQty(d.eq(1).text(), "");
//    getProductInfo = true;
//    RemoveRow(id);
//}

function RemoveRow(id) {
    $(id).remove();
}

function HideAll() {
    $("#txt_warehousediv").hide();

}

function checkDuplicate(code) {
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');
        var itemcode = td.eq(1).text();
       // var whouse = td.eq(8).text();
        //var color = td.eq(10).text();
        if (itemcode == code ) {
            return ind;
        }
    }
    return -1;

}

$("#txt_uanno").on("keydown", function (event) {
    if (event.which == 13) {
        var Uan = $('#txt_uanno').val();
        if (Uan != "") {
            getMaxTransferQty("", Uan);
            $.ajax({
                url: "/api/Product?id=" + "" + "&UanNo=" + Uan + "&Name=",
                type: "GET",
                success: function (_data) {
                    var obj = JSON.parse(_data);
                    if (obj == null) {
                        alert("null");
                        return;
                    }
                    $.each(obj, function (index, obj) {
                        $("#txt_uanno").val(obj.Uanno);
                        $("#txt_retail").val(obj.Retail);
                        $("#txt_cost").val(obj.Cost);
                        $("#txt_qty").val("1");
                        $("#txt_qty").focus().select();
                        $('#ddl_itemName').val(obj.Barcode);

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

function getMaxTransferQty(code, uan) {

    var branchId = $('#ddl_BranchIdFrom').val();
    var docType = $('#txt_doctype').val();

        $.ajax({
            url: "/Transfer/MaxTransferQty?type=" + docType + "&barcode=" + code + "&uan=" + uan + "&branchId=" + branchId,
            dataType: "JSON",
        }).done(function (obj) {
            $('#max_transfer_qty').val(obj);

        }).fail(function (obj) {
            toastr.error("Quantity service is not working");
        });
}

$("#ddl_BranchIdFrom").change(function (event) {
    $("#tblbody").empty();
    clearRow();
    
});

$("body").delegate(".changeable", "blur keyup", function () {
    calculateTotal();
});

function calculateTotal() {
    if ($("#txt_status").val() == 3) {
        return;
    }

    var totalAmount = 0;
    var TotalQty = 0;
    var tr = $("#tblbody").find('tr');
    for (var ind = 0; ind < tr.length; ind++) {
        var td = $(tr[ind]).find('td');


        var maxTakeQty = parseFloat(td.eq(7).text());

        if (isNaN(maxTakeQty) || !maxTakeQty) {
            toastr.error("Not in Stock");
            td.eq(3).text(0);
            return;
        }

        var productName = td.eq(2).text();
        var rowQty = parseFloat(td.eq(3).text());
        if (rowQty > maxTakeQty) {
            toastr.error("Max Qty available is:" + maxTakeQty);
            td.eq(3).text(maxTakeQty);
            return;
        }

        if (rowQty <= 0) {
            toastr.error("Qty should be grater than 0");
            td.eq(3).text("1");
            return;
        }
        var rowCost = parseFloat(td.eq(5).text());
        var rowTotal = (rowQty * rowCost);
       
        TotalQty = parseFloat(TotalQty) + rowQty;
        totalAmount = parseFloat(totalAmount) + rowTotal;
    }

    $("#txt_totalAmount").val(totalAmount);
    $("#txt_TotalQty").val(TotalQty);
}



$("#get-purchase-data").click(function () {
    var grn = $('#grn').val();

    if (!grn) {
        toastr.error("Enter purchase no");
        return;
    }

    $.ajax({
        url: "/transfer/GetGrn/" + grn,
        dataType: "JSON",
    }).done(function (arr) {
        if (arr) {
            $("#po-table-body").empty();
            $.each(arr, function (index, obj) {
                $("#po-table-body").append("<tr onclick=insertRow(this)>" +
                                                "<td>" + obj.Barcode + "</td>" +
                                                "<td>" + obj.Description + "</td>" +
                                                "<td>" + obj.Cost + "</td>" +
                                                "<td>" + obj.Retail + "</td>" +
                                                "<td>" + obj.Qty + "</td>" +
                                                "<td>" + obj.Amount + "</td>" +
                                                "<td>" + obj.QtyinHand + "</td>" +
                                                "<td style='display:none'>" + obj.DocNo + "</td>" +
                                                "</tr>"
                                                );
            });
        }
    }).fail(function (xhr) {
        alert('Request Status: \n ' +
               xhr.status +
               ' Status Text: ' +
               xhr.statusText +
               ' ' +
               xhr.responseText);

    });



});


function insertRow(e) {
    $("#tablePurchaseDetail").show();
    
    var selectedTr = $(e).find('td');
    var uanno = selectedTr.eq(0).text();
    var icode = selectedTr.eq(0).text();
    var _iname = selectedTr.eq(1).text();
    var cost = parseInt(selectedTr.eq(2).text());
    var retail = parseInt(selectedTr.eq(3).text());
    var usize = 0;
    var qty = parseInt(selectedTr.eq(4).text());
    var maxtransferQty = parseInt(selectedTr.eq(6).text());
    var DocNo = selectedTr.eq(7).text();

    if (qty > maxtransferQty) {
        qty = maxtransferQty;
    }

    if (!maxtransferQty || maxtransferQty <= 0) {
        toastr.error("Not in stock");
        return;
    }
    else if (qty > maxtransferQty) {
        toastr.error("Max available quantity for transfer is: " + maxtransferQty);
        return;
    }
    if (icode == "" || qty == "" || cost == "" || retail == "") {
        toastr.error('ItemName,Quantity,Cost and Retail is required.');
        return;
    }
    else if (parseInt(cost) > parseInt(retail)) {
        toastr.error('Retail Should be greater than origional Price and the origional Price is  ' + cost)
        return;
    }
    else {
        var index = checkDuplicate(icode);
        if (index != -1) {
            var tr = $("#tblbody").find('tr');
            var td = $(tr[index]).find('td');
            //td.eq(3).text(parseInt(qty) + parseInt(td.eq(3).text()));
            //td.eq(7).text(td.eq(7).text() + ((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text())));
            var totalQty = parseInt(qty) + parseInt(td.eq(3).text())
            if (totalQty > maxtransferQty) {
                toastr.error("Max available quantity for transfer is: " + maxtransferQty);
                return;
            }
            else {

                td.eq(3).text(totalQty);
                td.eq(4).text(parseInt(usize) + parseInt(td.eq(4).text()));
                td.eq(5).text(parseInt(cost));
                td.eq(6).text(parseInt(retail) + parseInt(td.eq(6).text()));
                //  td.eq(4).text(retail);
                // td.eq(7).text(parseInt(td.eq(7).text()) + ((parseInt(td.eq(3).text()) * parseInt(td.eq(5).text())) - parseInt(td.eq(6).text())));
                //  td.eq(7).text((parseInt(td.eq(3).text()) * parseInt(td.eq(4).text())) - parseInt(td.eq(6).text()));
            }

        }
        else {
            $("#tblbody").append("<tr id=trrr" + counter + ">" +
                "<td>" + uanno + "</td>" +
                "<td style='display:none;'>" + icode + "</td>" +
                "<td>" + _iname + "</td>" +
                "<td class='changeable' contenteditable='true'>" + qty + "</td>" +
                "<td class='changeable' contenteditable='true' style='display:none;'>" + usize + "</td>" +
                "<td class='changeable' contenteditable='true'>" + cost + "</td>" +
                "<td class='changeable' contenteditable='true'>" + retail + "</td>" +
                "<td style='display:none;'>" + maxtransferQty + "</td>" +
                "<td style='display:none;'>" + DocNo + "</td>" +
                "</tr>"
                );

            $("#tblbody tr").last().append(
                "<td align='center'><a onclick='RemoveRow(trrr" + counter + "," + counter + ")' class='fa fa-trash' style='color:red;'></a>"
                //+"&nbsp;&nbsp;|&nbsp;&nbsp;<a onclick='EditRow(trrr" + counter + "," + counter + ")' class='fa fa-edit' style='color:blue;'></a></td>"
                );
            counter++;
            calculateTotal();
        }
        calculateTotal();
    }
}