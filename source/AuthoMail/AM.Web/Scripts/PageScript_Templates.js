var DolfusTemplates = function () {

    function DropZoneInit() {

        Dropzone.options.uploadtemplatediv = {
            paramName: "template", // The name that will be used to transfer the file
            //maxFilesize: 2, // MB
            timeout: 60000,
            dictDefaultMessage: '<b>Click or Drag here to upload template(s).</b>',
            acceptedFiles: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            //accept: function (file, done) {
            //    //console.log('file:', file);
            //    done();
            //},
            autoQueue: true,
            autoDiscover: false,
            //uploadprogress: function (file, progress, bytesSent) {
            //    console.log('uploadprogress: ', file);
            //    console.log('uploadprogress: ', progress);
            //    console.log('uploadprogress: ', bytesSent);
            //},
            //processing: function (response) {
            //    console.log('processing: ', response);
            //},

            //sending: function (response) {
            //    console.log('sending: ', response.name);
            //},
            ////success: function (file,response) {
            ////    console.log('success : ', file.name);
            ////    console.log('success : ', response.Object);
            ////},
            //complete: function (response) {
            //    console.log('complete: ', response.name);
            //},
            totaluploadprogress: function (response) {
                console.log('totaluploadprogress:', response);
                //alertMessage("green", 'All templates uploaded successfully.');
            },
            queuecomplete: function (response) {
                alertMessage("green", 'Template uploading completed.');
                TemplateList();
            },
            //error: function (err) {
            //    console.log('error: ', err);
            //},

            init: function () {
                //this.on("addedfile", function (file) {
                //    console.log('addedfile: ', file);
                //});
                this.on("success", function (file, response) {
                    if (!response.Success) {
                        var messages = '<ul>';
                        for (var i = 0; i < response.Object.length; i++) {
                            messages += '<li>' + response.Object[i] + '</li>';
                        }
                        $(file.previewElement).addClass("dz-error").find('.dz-error-message').html(messages);
                    }
                });
                //this.on("error", function (file, errorMessage) {
                //    console.log('err: ', file);
                //    console.log('err: ', errorMessage);
                //    if (file.accepted) {
                //        var mypreview = document.getElementsByClassName('dz-error');
                //        mypreview = mypreview[mypreview.length - 1];
                //        mypreview.classList.toggle('dz-error');
                //        mypreview.classList.toggle('dz-success');
                //    }
                //});
            }
        };
    }

    function TemplateList() {
        var id = 'templatelisttable';
        if ($.fn.DataTable.isDataTable('#' + id)) {
            $('#' + id).remove();
            $('#' + id + '_wrapper').remove();
            $('#templatestablebody').empty();
        }
        var tableBody = $('#templatebody').html();
        tableBody = $(tableBody).attr('id', id);
        $('#templatestablebody').html(tableBody);
        var table = $('#templatelisttable').DataTable({
            "language": {
                "emptyTable": "No rows to display."
            },
            //"processing": true,
            //"serverSide": true,
            "ajax": {
                "url": baseUrl + "Admin/DelfusTemplatesList",
                "type": "GET"
            },
            destroy: true,
            paging: false,
            "columns": [
                {
                    "data": "Id",
                    "render": function (data, type, row, meta) {
                        var buttons = '<button class="btn btn-success btn-sm downloadtemplate" data-id="' + data + '"><i class="fa fa-download"></i></button>';
                        if (!row.IsRemoved) {
                            buttons += ' <button class="btn btn-danger btn-sm removetemplate" data-id="' + data + '"><i class="fa fa-trash"></i></button>';
                        }
                        return buttons;
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
                {
                    "data": "IsRemoved",
                    "render": function (data) {
                        if (data == true) {
                            return '<span class="label label-danger"> Yes </span>';
                        }
                        else {
                            return '<span class="label label-success"> No </span>';
                        }
                    }
                },
                { "data": "RemovedBy" },
                {
                    "data": "RemovedOn",
                    "render": function (data, type, row, meta) {
                        if (data == null) return null;
                        return moment(data).format("MM/DD/YY HH:mm:ss");
                    }
                }
            ]
        });
        RemoveTemplate(table);
        DownloadTemplate();
    }
    function RemoveTemplate(table) {
        $('#templatelisttable tbody').on('click', '.removetemplate', function () {
            var data = table.row($(this).parents('tr')).data();
            var isConfirm = confirm('Are you sure to remove "' + data.FileName + '" template?');
            if (isConfirm) {
                var context = $(this);
                $(this).attr('disabled', 'disabled');
                var id = $(this).data('id');
                $.ajax({
                    type: "POST",
                    url: baseUrl + "admin/RemoveDelfusTemplate",
                    data: { id: id },
                    dataType: "json",
                    success: function (response) {
                        if (response.Success) {
                            alertMessage("green", 'Template has been removed.');
                        }
                        else {
                            alertMessage("red", response.ErrorMessage);
                        }
                        $(context).removeAttr('disabled');
                        TemplateList();
                    },
                    error: function (err) {
                        console.log(err);
                        $(context).removeAttr('disabled');
                    }
                });
            }
        });
    }
    function DownloadTemplate() {
        $('#templatelisttable tbody').on('click', '.downloadtemplate', function () {
            var context = $(this);
            $(this).attr('disabled', 'disabled');
            $(this).find('i').removeClass('fa-download');
            $(this).find('i').addClass('fa-spining fa-spinner fa-spin');
            var id = $(this).data('id');
            $.ajax({
                type: "POST",
                url: baseUrl + "Admin/DownloadDelfusTemplate",
                data: { id: id },
                dataType: "json",
                success: function (response) {
                    console.log(response);
                    if (response.Success) {
                        var stream = response.Object.stream;
                        var sampleArr = base64ToArrayBuffer(stream);
                        saveByteArray(response.Object.fileName, sampleArr);
                    }
                    else {
                        alertMessage("red", response.ErrorMessage);
                    }
                    $(context).removeAttr('disabled');
                    $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                    $(context).find('i').addClass('fa-download');
                },
                error: function (err) {
                    console.log(err);
                    $(context).removeAttr('disabled');
                    $(context).find('i').removeClass('fa-spining fa-spinner fa-spin');
                    $(context).find('i').addClass('fa-download');
                }
            });
        });
    }
    function base64ToArrayBuffer(base64) {
        var binaryString = window.atob(base64);
        var binaryLen = binaryString.length;
        var bytes = new Uint8Array(binaryLen);
        for (var i = 0; i < binaryLen; i++) {
            var ascii = binaryString.charCodeAt(i);
            bytes[i] = ascii;
        }
        return bytes;
    }

    //save Byte[] array and download
    function saveByteArray(fileName, byte) {
        var blob = new Blob([byte]);
        var link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName + '.docx';
        link.click();
    }
    return {
        init: function () {
            DropZoneInit();
            TemplateList();
            //UploadTemplates()
        }
    };
}();
jQuery(document).ready(function () {
    DolfusTemplates.init();
});