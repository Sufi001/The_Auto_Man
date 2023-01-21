//setInterval(
//    function () { 



//$.ajax({
//    url: "/DB/Migrate",
//    method: "get",
//}).done(function (obj) {
//    toastr.success('Data saved successfully.');
//    // $('#btn_authorize').attr('disabled', 'disabled');
//    $('#btn_authorize').attr('disabled', 'disabled');
//    $("#add_btn").attr('disabled', 'disabled');
//    $('#btn_save').attr('disabled', 'disabled');
//    $("#txt_docNo").val(obj);
//    $("#btn_print1").removeAttr("disabled");
//}).fail(function (xhr) {
//    alert('Request Status: \n ' +
//        xhr.status +
//        ' Status Text: ' +
//        xhr.statusText +
//        ' ' +
//        xhr.responseText);
//    $('#btn_authorize').removeAttr("disabled");
//    $('#btn_submit').removeAttr("disabled");
//    $("#add_btn").removeAttr("disabled");
//    toastr.error('Data not saved');
//});

//alert("Hello");
//    }
//    , 10000);