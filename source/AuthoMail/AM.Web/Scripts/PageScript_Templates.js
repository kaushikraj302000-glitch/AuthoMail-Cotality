var DolfusTemplates = function () {

    function DropZoneInit() {

        Dropzone.options.uploadtemplatediv = {
            paramName: "template", // The name that will be used to transfer the file
            //maxFilesize: 2, // MB
            //timeout: 60000,
            //uploadMultiple:true,
            //parallelUploads:100,
            dictDefaultMessage: '',//<b>Click or Drag here to upload template(s).</b>',
            //acceptedFiles:  'file/doc, file/docx',//'doc, docx',//"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            //accept: function (file, done) {
            //    //console.log('file:', file);
            //    done();
            //},
            autoQueue: true,
            autoDiscover: false,
            uploadprogress: function (file, progress, bytesSent) {
                $("#progressBar").show();
                $("#progressBar").val(Math.round(progress));
            },
            //processing: function (response) {
            //    console.log('processing: ', response);
            //},

            //sending: function (response) {
            //    console.log('sending: ', response.name);
            //},
            //success: function (file,response) {
            //    console.log('success : ', file.name);
            //    console.log('success : ', response);
            //},
            complete: function (response) {
                $("#progressBar").hide();
            },
            totaluploadprogress: function (response) {
                //console.log('totaluploadprogress:', response);
                //alertMessage("green", 'All templates uploaded successfully.');
            },
            queuecomplete: function (response) {
                //console.log(response);
                TemplateList();
                alertMessage("green", 'Template(s) uploaded.');
            },
            error: function (err) {
                //console.log('error: ', err);
            },

            init: function () {
                //this.on("addedfile", function (file) {
                //    console.log('addedfile: ', file);
                //});
                this.on("success", function (file, response) {
                    //var table = $('#templatelisttable').DataTable();
                    //if (response.Success) {
                    //    var isFound = false;
                    //    table.rows(function (idx, data, node) {
                    //        if (data.FileName === file.name) {
                    //            table.row(idx).data({
                    //                "FileName": file.name,
                    //                "FileSize": parseInt(file.size / 1024) + 'KB',
                    //                "Status": true,
                    //                "FileName": file.name
                    //            }).draw();
                    //            isFound = true;
                    //        }

                    //    });
                    //    if (isFound==false) {
                    //        table.row.add({
                    //            "FileName": file.name,
                    //            "FileSize": parseInt(file.size / 1024) + 'KB',
                    //            "Status": true,
                    //            "FileName": file.name
                    //        }).draw();
                    //    }
                        
                        
                    //    //var messages = '<ul>';
                    //    //for (var i = 0; i < response.Object.length; i++) {
                    //    //    messages += '<li>' + response.Object[i] + '</li>';
                    //    //}
                    //    //$(file.previewElement).addClass("dz-error").find('.dz-error-message').html(messages);
                    //}
                    //else {
                    //    table.row.add({
                    //        "FileName": file.name,
                    //        "FileSize": parseInt(file.size / 1024) + 'KB',
                    //        "Status": response.ErrorMessage,
                    //        "FileName": file.name
                    //    }).draw();
                        
                    //}
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
            dom: 'Bfrtip',
            //"processing": true,
            //"serverSide": true,
            "ajax": {
                "url": baseUrl + "Admin/DelfusTemplatesList",
                "type": "GET"
            },
            destroy: true,
            buttons: [
                {
                    text: 'Uploaded Summary',
                    className: 'btn btn-info',
                    action: function (e, dt, node, config) {
                        window.open(baseUrl + 'Admin/SystemSettings');

                    }
                }
            ],
            paging: false,
            "columns": [
                
                { "data": "FileName" ,"className": 'dt-body-left'},
                { "data": "FileSize", "className": 'dt-body-left'},
                {
                    "data": "ModifiedOn",
                    "render": function (data, type, row, meta) {
                        return moment(data).format("MM/DD/YY HH:mm:ss");
                    }
                },
                //{
                //    "data": "Status",
                //    "className": 'dt-body-left',
                //    "render": function (data) {
                //        if (data == true) {
                //            return '<span class="label label-success"> Valid </span>';
                //        }
                //        else {
                //            return '<span class="label label-danger" title="'+data+'"> Invalid </span>';
                //        }
                //    }
                //},
                {
                    "data": "FileName",
                    "className": 'dt-body-left',
                    "render": function (data, type, row, meta) {
                        if (row.Status==true) {
                            var buttons = '<button title="Download Template" type="button" class="btn btn-success btn-sm downloadtemplate" data-id="' + data + '"><i class="fa fa-download"></i></button>';
                            buttons += ' <button type="button" title="History Of Template" class="btn btn-info btn-sm historytemplate" data-id="' + data + '"><i class="fa fa-history"></i></button>';
                            buttons += ' <button type="button" title="Delete Template" class="btn btn-danger btn-sm removetemplate" data-id="' + data + '"><i class="fa fa-trash"></i></button>';


                            return buttons;
                        } else {
                            return '--';
                        }
                       
                    }
                },
                //{ "data": "RemovedBy" },
                //{
                //    "data": "RemovedOn",
                //    "render": function (data, type, row, meta) {
                //        if (data == null) return null;
                //        return moment(data).format("MM/DD/YY HH:mm:ss");
                //    }
                //}
            ]
        });
        RemoveTemplate(table);
        DownloadTemplate();
        GetHistory();
    }
    function RemoveTemplate(table) {
        $('#templatelisttable tbody').on('click', '.removetemplate', function () {
            var data = table.row($(this).parents('tr')).data();
            var isConfirm = confirm('Are you sure to remove "' + data.FileName + '" template?');
            if (isConfirm) {
                var context = $(this);
                $(this).attr('disabled', 'disabled');
                var fileName = $(this).data('id');
                $.ajax({
                    type: "POST",
                    url: baseUrl + "admin/RemoveDelfusTemplate",
                    data: { fileName: fileName },
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
    function GetHistory() {
        $('#templatelisttable tbody').on('click', '.historytemplate', function () {
            var fileName = $(this).data('id');
            $("#historyLabel").text('History of ' + fileName);
            var url = baseUrl + "Admin/DelfusTemplatesHistory?fileName=" + fileName;
            console.log(fileName,url)
            var id = 'historytable';
            if ($.fn.DataTable.isDataTable('#' + id)) {
                $('#' + id).remove();
                $('#' + id + '_wrapper').remove();
                $('#historytablebody').empty();
            }
            var tableBody = $('#historybody').html();
            tableBody = $(tableBody).attr('id', id);
            $('#historytablebody').html(tableBody);
            var table = $('#historytable').DataTable({
                "language": {
                    "emptyTable": "No rows to display."
                },
                //"processing": true,
                //"serverSide": true,
                "ajax": {
                    "url": url,
                    "type": "GET"
                },
                destroy: true,
                searching: false,
                info: false,
                paging: false,
                "columns": [
                    { "data": "ModifiedBy" },
                    {
                        "data": "ModifiedOn",
                        "render": function (data, type, row, meta) {
                            return moment(data).format("MM/DD/YY HH:mm:ss");
                        }
                    },
                    {
                        "data": "Status",
                        "render": function (data) {
                            if (data == "Modified") {
                                return '<span class="label label-success"> Modified </span>';
                            }
                            else if (data == "Deleted") {
                                return '<span class="label label-danger"> Deleted </span>';
                            }
                            else if (data == "Added") {
                                return '<span class="label label-info"> Added </span>';
                            }
                            else {
                                return '--';
                            }
                        }
                    }
                ]
            });
            $('#' + id).width(496);
            $("#historyModal").modal('show');
        });
    }
    function DownloadTemplate() {
        $('#templatelisttable tbody').on('click', '.downloadtemplate', function () {
            var context = $(this);
            $(this).attr('disabled', 'disabled');
            $(this).find('i').removeClass('fa-download');
            $(this).find('i').addClass('fa-spining fa-spinner fa-spin');
            var fileName = $(this).data('id');
            $.ajax({
                type: "POST",
                url: baseUrl + "Admin/DownloadDelfusTemplate",
                data: { fileName: fileName },
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