var DolfusTemplates = function () {


    function DropZoneInit() {
        var uploadedProgress = 0;
        var totalError = 0;
        var dropzoneObj;
        var uploadedFile = 0;
        var dropedFile = 0;
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
                //$("#progressBar").show();
            },
            //processing: function (response) {
            //    console.log('processing: ', response);
            //    $("#progressBar").show();
            //},

            sending: function (response) {
                //console.log('sending: ', response.name);
                $("#progressBar").show();
            },
            //success: function (file,response) {
            //    console.log('success : ', file.name);
            //    console.log('success : ', response);
            //},
            //complete: function (response) {

            //},
            totaluploadprogress: function (response) {
                //uploadedProgress = response >= uploadedProgress ? response : uploadedProgress;
                uploadedProgress = parseInt(uploadedFile * 100) / dropzoneObj.files.length ;
               // console.log(parseInt(uploadedFile / dropzoneObj.files.length))
                $("#progressBar").val(Math.round(uploadedProgress));
                //console.log('totaluploadprogress:', response, uploadedFile, uploadedProgress, dropzoneObj);
                //alertMessage("green", 'All templates uploaded successfully.');
            },
            queuecomplete: function (response) {
                //console.log(response);
                
                
                $("#progressBar").hide();
                //console.log(dropzoneObj.files.length, totalError)
                if (totalError > 0) {
                    if (dropedFile == totalError) {
                        alertMessage("red", 'Upload failed with ' + totalError + ' error(s). You are being redirected to “Upload summary” page for more details...');

                    }
                    else {
                        alertMessage("yellow", 'Upload completed with ' + totalError + ' error(s). You are being redirected to “Upload summary” page for more details...');
                        setTimeout(function () {
                            TemplateList();
                        }, 1000);
                    }
                    setTimeout(function () {
                        window.open(baseUrl + 'DelfusTemplate/TemplateSummary');
                    }, 4000);
                    totalError = 0;
                    uploadedFile = 0;
                } else {
                alertMessage("green", 'Upload completed: Please check upload summary for more details.');
                    TemplateList();
                }
                dropedFile = 0;
                //window.open(baseUrl + 'DelfusTemplate/TemplateSummary');
            },
            error: function (err) {
                //console.log('error: ', err);
            },

            init: function () {
                dropzoneObj = this;
                //this.on("addedfile", function (file) {
                //    console.log('addedfile: ', file);
                //});
                this.on("success", function (file, response) {
                    var table = $('#templatelisttable').DataTable();
                    if (response.Success) {
                        uploadedFile += 1;
                        //var isFound = false;
                        //table.rows(function (idx, data, node) {
                        //    if (data.FileName === file.name) {
                        //        table.row(idx).data({
                        //            "FileName": file.name,
                        //            "FileSize": parseInt(file.size / 1024) + 'KB',
                        //            "Status": true,
                        //            "FileName": file.name
                        //        }).draw(false);
                        //        isFound = true;
                        //    }

                        //});
                        //if (isFound == false) {
                        //    table.row.add({
                        //        "FileName": file.name,
                        //        "FileSize": parseInt(file.size / 1024) + 'KB',
                        //        "Status": true,
                        //        "FileName": file.name
                        //    }).draw(false);
                        //}

                    } else {
                        totalError += 1;
                    }
                    dropedFile += 1;
                });
                this.on("error", function (file, errorMessage) {
                    //console.log('err: ', errorMessage);
                    totalError += 1;
                    dropedFile += 1;
                });
            }
        };
        
    }
    function calcDataTableHeight(decreaseTableHeight) {
        return ($(window).innerHeight() - 240) - decreaseTableHeight;
    };
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
            dom: 'Blifrtp',//Blifrtp
            order: [[2, "desc"]],
            //"processing": true,
            //"serverSide": true,
            "ajax": {
                "url": baseUrl + "DelfusTemplate/DelfusTemplatesList",
                "type": "GET"
            },
            destroy: true,
            buttons: [
                {
                    text: 'View Template Upload Summary',
                    className: 'btn btn-info pull-left',
                    action: function (e, dt, node, config) {
                        window.open(baseUrl + 'DelfusTemplate/TemplateSummary');

                    }
                }
            ],
            paging: false,
            "columns": [

                { "data": "FileName", "className": 'dt-body-left' },
                { "data": "FileSize", "className": 'dt-body-left', 'searchable': false, },
                {
                    "data": "ModifiedOn",
                    "className": 'dt-body-left',
                    "render": function (data, type, row, meta) {
                        $("#totalcounts").text(meta.row+1);
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
                    "className": 'dt-body-center',
                    orderable: false,
                    "render": function (data, type, row, meta) {
                        if (row.Status == true) {
                            var buttons = '<button title="Download Template" type="button" class="btn btn-success btn-sm downloadtemplate" data-id="' + data + '"><i class="fa fa-download"></i></button>';
                            buttons += ' <button type="button" title="View Template History" class="btn btn-info btn-sm historytemplate" data-id="' + data + '"><i class="fa fa-history"></i></button>';
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
        var info = table.page.info();
        RemoveTemplate(table);
        DownloadTemplate();
        GetHistory();
    }
    function RemoveTemplate(table) {
        $('#templatelisttable tbody').on('click', '.removetemplate', function () {
            var data = table.row($(this).parents('tr')).data();
            var isConfirm = confirm('Are you sure you want to delete "' + data.FileName + '" template?');
            if (isConfirm) {
                var context = $(this);
                $(this).attr('disabled', 'disabled');
                var fileName = $(this).data('id');
                $.ajax({
                    type: "POST",
                    url: baseUrl + "DelfusTemplate/RemoveDelfusTemplate",
                    data: { fileName: fileName },
                    dataType: "json",
                    success: function (response) {
                        if (response.Success) {
                            alertMessage("green", 'Template has been deleted.');
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
            var url = baseUrl + "DelfusTemplate/DelfusTemplatesHistory?fileName=" + fileName;
            //console.log(fileName, url)
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
                scrollY: calcDataTableHeight(80),
                "scrollCollapse": true,
                paging: false,
                "columns": [
                    {
                        "data": "Status", "className": 'dt-body-center',
                        "render": function (data) {
                            if (data == "Modified") {
                                return '<span class="label label-success"> Modified </span>';
                            }
                            else if (data == "Deleted" || data == "Invalid") {
                                return '<span class="label label-danger"> ' + data + ' </span>';
                            }
                            else if (data == "Added") {
                                return '<span class="label label-info"> Added </span>';
                            }
                            else {
                                return '--';
                            }
                        }
                    },
                    { "data": "ModifiedBy" ,"className": 'dt-body-right'},
                    {
                        "data": "ModifiedOn", "className": 'dt-body-right',
                        "render": function (data, type, row, meta) {
                            return moment(data).format("MM/DD/YY HH:mm:ss");
                        }
                    },
                    
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
                url: baseUrl + "DelfusTemplate/DownloadDelfusTemplate",
                data: { fileName: fileName },
                dataType: "json",
                success: function (response) {
                    //console.log(response);
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
                    //console.log(err);
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