var DolfusTemplates = function () {


    function UploadTemplates() {
        $('.upload-container').each(function () {
            var $upload = $(this).find('.upload-components'),
                droppedFiles = false,
                self = $(this);

            // Hook up the "Upload File" button to the hidden file input.
            $upload.find('.btn').on('click', function () {
                //if ($('#Client').val() == null || $('#Client').val() == "" || $('#Client').val() == undefined) {
                //    toastr.error("Please select client.");
                //    return false;
                //}
                self.find('.upload-input').click();
                $(self).unbind('change');
                $(self).bind('change', function (e) {

                    droppedFiles = e.target.files || e.dataTransfer.files;
                    var files = null;
                    for (var i = 0; i < droppedFiles.length; i++) {
                        var fileExtension = droppedFiles[i].name.split('.').pop().toLowerCase();
                        if (fileExtension == "docx") {
                            $(this).addClass('uploading');
                            $(this).find('.filename').html(droppedFiles[0].name);
                            uploadFile($(this), droppedFiles[i]);
                        }
                        else {
                            toastr.error("Acceptable file type is .docx");
                            $('#fileuploadresult').html("Please upload file...");
                        }
                    }

                });
            });


            // This counter takes care of child events so they don't trigger dragleave/dragenter events and a lot of flickering.
            var counter = 0;

            /**
             * prop e.dataTransfer
             */
            $(this).on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
                e.preventDefault();
                e.stopPropagation();
            }).on('dragover', function () {
                $(this).addClass('dragover');
            }).on('dragenter', function () {
                counter++;
                $(this).addClass('dragover');
            }).on('dragleave', function () {
                counter--;
                if (counter === 0) {
                    $(this).removeClass('dragover');
                }
            }).on('dragend drop', function () {
                $(this).removeClass('dragover');
            }).on('drop', function (e) {
                droppedFiles = e.originalEvent.files || e.originalEvent.dataTransfer.files;
                if (droppedFiles.length == 1) {
                    var fileExtension = droppedFiles[0].name.split('.').pop().toLowerCase();
                    if (fileExtension == "docx") {
                        $(this).addClass('uploading');
                        $(this).find('.filename').html(droppedFiles[0].name);
                        uploadFile($(this), droppedFiles[0]);
                    }
                    else {
                        toastr.error("Acceptable file type is .docx");
                    }
                } else {
                    toastr.error("You can not upload more than one file.");
                }

            });
        });
    }
    function uploadFile(obj, file) {
        var formData = new FormData($('#uploadfileform')[0]);
        formData.append("file", file);
        $.ajax({
            xhr: function () {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total;
                        percentComplete = parseInt(percentComplete * 100);
                        //console.log(percentComplete);
                        obj.find('.progress-bar').css('width', percentComplete + '%');
                        if (percentComplete === 100) {
                            $('#fileuploadresult').html("Please wait while file is processed.");
                        }
                    }
                }, false);

                return xhr;
            },
            url: baseUrl + "Admin/SaveTemplates",
            type: "POST",
            processData: false,
            contentType: false,
            data: formData,
            // dataType: "json",
            success: function (response) {
                console.log(response);
                if (response.success) {
                    //toastr.success(response.Object);
                    //swal("", "File processed successfully.", "success");
                    $('#exceluploadmodalresulttitle').html('File processed successfully.');

                    var err = "<ul style='color:green;'>";
                    $.each(response.object, function (i, v) {
                        if (i != 0)
                            err = err + "<li>" + v + "</li>";
                    });
                    err = err + "</ul>";
                    $('#exceluploadmodalresultcontent').html(err);
                    $('#exceluploadmodalresult').modal("show");
                    $(".filename").empty();
                    $(".upload-container").removeClass("uploading");
                    $(".upload-input").val("");
                    $('#fileuploadresult').html("File processed successfully.");
                    //setTimeout(function () { window.location.reload(); }, 1000);
                    ApplyTable();
                }
                else {
                    $('#exceluploadmodalresulttitle').html('Please review following error(s).');
                    var err = "<ul style='color:red;'>";
                    $.each(response.object, function (i, v) {
                        err = err + "<li>" + v + "</li>";
                    });
                    err = err + "</ul>";
                    $('#exceluploadmodalresultcontent').html(err);
                    $('#exceluploadmodalresult').modal("show");
                    $('#fileuploadresult').html("Please upload file...");
                    $(".filename").empty();
                    $(".upload-container").removeClass("uploading");
                    $(".upload-input").val("");
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log(thrownError);
                $(".filename").empty();
                $(".upload-container").removeClass("uploading");
                //$("#uploadfilesrecord").html(response.Object);
                $(".upload-input").val("");
                //progress();
                return false;
            }
        });

    }

    function DropZoneInit() {
        url = baseUrl + "Admin/SaveDelfusTemplates";
        $("#uploadtemplatediv").dropzone(
            {
                url: url,
                clickable: true
            });
    }

    function TemplateList() {
        //$('#templatelistdiv').html('<div class="alert alert-info text-center">Please wait..</div>');
        $('#templatelisttable').DataTable({
            "language": {
                "emptyTable": "No rows to display."
            },
            //"processing": true,
            //"serverSide": true,
            "ajax": {
                "url": baseUrl + "Admin/DelfusTemplatesList",
                "type": "GET"
            },
            "columnDefs": [
                {
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        return '<button class="btn btn-primary btn-sm downloadtemplate" data-id="' + data + '"><i class="fa fa-download"></button>< button class="btn btn-primary btn-sm removetemplate" data-id="' + data + '" > <i class="fa fa-trash"></button>';
                    }
                },
                { "data": "FileName" },
                { "data": "UploadedBy" },
                {
                    "data": "UploadedOn",
                    "render": function (data, type, row, meta) {
                        return moment(data).format("MM/DD/YY HH:mm:ss");
                    }
                },
                { "data": "IsRemoved" },
                { "data": "RemovedBy" },
                {
                    "data": "RemovedOn",
                    "render": function (data, type, row, meta) {
                        return moment(data).format("MM/DD/YY HH:mm:ss");
                    }
                }
            ]
        });
        //$.ajax({
        //    type: "POST",
        //    url: baseUrl + "Admin/DelfusTemplatesList",
        //    dataType: "json",
        //    success: function (response) {
        //        console.log(response);
        //        if (response.length == 0) {
        //            $('#templatelistdiv').html('<div class="alert alert-warning text-center">No rows found.</div>');
        //        }
        //        else {

        //        }
        //    },
        //    error: function (err) {
        //        $('#templatelistdiv').html('<div class="alert alert-info text-center">Oop! An error is occured.</div>');
        //        console.log(err);
        //    }
        //});
    }
    return {
        init: function () {
            TemplateList();
            DropZoneInit();
            //UploadTemplates()
        }
    };
}();
jQuery(document).ready(function () {
    DolfusTemplates.init();
});