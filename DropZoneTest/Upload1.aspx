<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Upload1.aspx.cs" Inherits="Upload1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script type="text/javascript" src="latestJs_1.11/jquery.min.js"></script>
    <script type="text/javascript" src="latestJs_1.11/moment.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js" integrity="sha384-JZR6Spejh4U02d8jOt6vLEHfe/JQGiRRSQQxSfFWpi1MquVdAyjUar5+76PVCmYl" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap-datetimepicker/4.17.47/js/bootstrap-datetimepicker.min.js" crossorigin="anonymous"></script>
    <script src="DropzoneJs_scripts/dropzone.js"></script>
    <link href="DropzoneJs_scripts/dropzone.css" rel="stylesheet" />
    <link rel="stylesheet" href="latestJs_1.11/bootstrap/css/bootstrap-datetimepicker-standalone.min.css" />
    <link rel="stylesheet" href="latestJs_1.11/bootstrap/css/bootstrap.min.css" />

    <style>
        td, th {
            padding: 2px;
        }

        div.dropzone {
            float: right;
            padding: 5px;
            margin: 5px;
            /*display: block;  reverse comment to always show drop zone   */
            display: none;
        }

        .dropzone.dz-clickable {
            /*padding: 10px; */
            padding-left: 20px;
            width: 200px;
            height: 214px;
            /* height: 150px; */
            align-items: normal;
            /*margin: 10px; /*
            /*margin-left: 12px;*/
        }

        .dropzone .dz-message {
            text-align: center;
            margin: 5px;
        }

        div.dz-default {
            width: 150px;
            height: 180px;
            float: none;
            padding: 5px;
            margin: 5px;
            background-color: #eee;
            border: none;
        }


        div.status {
            float: none;
            border: none;
            margin-left: 50px;
            width: 700px;
            padding: 10px;
        }
        /*
        div.container {
            float: none;
            border: none;
            margin-left: 50px;
            width: 600px;
            padding: 10px;
        }
            */
        div.form-group {
            float: none;
            border: none;
            margin-top: 25px;
        }


        div.input-group {
            width: 200px;
            float: none;
            border: none;
        }

        div.left-half {
            float: none;
            border: none;
            padding-left: 20px;
            width: 750px;
        }

        div.settings {
            border: 0px solid darkgrey;
            padding: 10px;
        }

        .procButton {
            display: none;
        }

        .stsSpan {
            color: green;
            font-size: xx-large;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <div style="text-align: right; width: 667px;">
            <b>
                <label style="font-size: x-large;">Certificate of Interest Generator &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</label></b>
            <img src="images/Logo_new_letterHead.JPG" style="height: 94px; width: 87px;" />

        </div>

        <br />

        <div class="left-half">
            <div id="dZUpload" class="dropzone">
                <div class="dz-default dz-message">
                    Drop PDF here. 
                </div>
            </div>

            <div class="form-group">
                <label for="datetimepicker6" class=" col-form-label">Financial Year Begin : </label>
                <div class='input-group date' id='datetimepicker6'>
                    <input type='text' class="form-control" id="dteFinStart" value="" />
                    <span class="input-group-addon">
                        <span class="glyphicon glyphicon-calendar"></span>
                    </span>
                </div>
            </div>
            <div class="form-group">
                <label for="datetimepicker7" class=" col-form-label">Financial Year End : </label>
                <div class='input-group date' id='datetimepicker7'>
                    <input type='text' class="form-control" id="dteFinEnd" value="" />
                    <span class="input-group-addon">
                        <span class="glyphicon glyphicon-calendar"></span>
                    </span>
                </div>
            </div>
            <div class="settings">
                <table border="0" style="border-collapse: collapse; padding: 2px;">
                    <tr>
                        <td>
                            <label style="font-size: x-small;">Region : </label>
                        </td>
                        <td style="width: 250px;">
                            <input type="text" id="txtRegion" onchange="checkDates();" style="font-size: x-small; width: inherit;" />
                            <td rowspan="3" width="100px" align="center">
                                <input type="button" id="btnSaveData" value="Save" onclick="btnSaveclick();" style="font-size: x-small;" />
                            </td>
                    </tr>
                    <tr>
                        <td>
                            <label style="font-size: x-small;">Consultant Name : </label>
                        </td>
                        <td style="width: 250px;">
                            <input type="text" id="txtConsName" onchange="checkDates();" style="font-size: x-small; width: inherit;" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label style="font-size: x-small;">Consultant Employee Number : </label>
                        </td>
                        <td style="width: 250px;">
                            <input type="text" id="txtConsNumber" onchange="checkDates();" style="font-size: x-small; width: inherit;" /><br />
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div class="left-half">
            <div id="statusDiv" class="form-group">
                <div class="form-group">

                    <label><b>Status :</b></label>
                    <span id="stsSpan"></span>
                </div>
            </div>
            <div>
                <asp:HiddenField ID="hdnServerPrefix" runat="server" />

            </div>

            <div class="status">

                <div id="statusDiv2">
                    <span id="stsSpan2"></span>
                </div>
            </div>
        </div>
        <script type="text/javascript">

            //=============================== cookie handling =====================================

            function setCookie(cname, cvalue, exdays) {
                var d = new Date();
                d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
                var expires = "expires=" + d.toUTCString();
                document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
            }

            function getCookie(cname) {
                var name = cname + "=";
                var ca = document.cookie.split(';');
                for (var i = 0; i < ca.length; i++) {
                    var c = ca[i];
                    while (c.charAt(0) == ' ') {
                        c = c.substring(1);
                    }
                    if (c.indexOf(name) == 0) {
                        return c.substring(name.length, c.length);
                    }
                }
                return "";
            }


            function btnSaveclick() {
                var region = encodeURI(document.getElementById("txtRegion").value);
                var conName = encodeURI(document.getElementById("txtConsName").value);
                var conNumber = encodeURI(document.getElementById("txtConsNumber").value);
                document.cookie = "region=" + region + ";expires=Wed, 18 Dec 2023 12:00:00 GMT";
                document.cookie = "conName=" + conName + ";expires=Wed, 18 Dec 2023 12:00:00 GMT";
                document.cookie = "conNumber=" + conNumber + ";expires=Wed, 18 Dec 2023 12:00:00 GMT";
                alert("values saved.")
            }

            // ========================== cookie handling  ==================================== 


            function numberWithCommas(x) {
                x = x.toString();
                var pattern = /(-?\d+)(\d{3})/;
                while (pattern.test(x))
                    x = x.replace(pattern, "$1,$2");
                return x;
            }


            function checkDates() {
                var startDate = document.getElementById("dteFinStart").value;
                var endDate = document.getElementById("dteFinEnd").value;
                var dz = document.getElementById("dZUpload");
                var region = document.getElementById("txtRegion").value;
                var conName = document.getElementById("txtConsName").value;
                var conNumber = document.getElementById("txtConsNumber").value;

                if (startDate === "" || endDate === "" || region === "" || conName === "" || conNumber === "") {
                    dz.style.display = "none";
                } else {
                    dz.style.display = "block";
                }
            }
            // DateTime Picker
            $(function () {
                $('#datetimepicker6').datetimepicker({
                    format: 'YYYY-MM-DD'
                });
                $('#datetimepicker7').datetimepicker({
                    useCurrent: false,                     //Important! See issue #1075
                    format: 'YYYY-MM-DD'


                });
                $("#datetimepicker6").on("dp.change", function (e) {
                    $('#datetimepicker7').data("DateTimePicker").minDate(e.date);
                    checkDates();
                });
                $("#datetimepicker7").on("dp.change", function (e) {
                    $('#datetimepicker6').data("DateTimePicker").maxDate(e.date);
                    checkDates();
                });



            });

            $(document).ready(function () {

                Dropzone.autoDiscover = false;
                //Simple Dropzonejs 
                $("#dZUpload").dropzone({
                    url: "dummy.uel",
                    maxFiles: 10,
                    autoProcessQueue: true,
                    addRemoveLinks: false,


                    init: function () {
                        this.on("processing", function (file) {
                            var startDate = document.getElementById("dteFinStart").value;
                            // strip out the '-' from the date 
                            startDate = startDate.substring(0, 4) + startDate.substring(5, 7) + startDate.substring(8, 10);

                            var endDate = document.getElementById("dteFinEnd").value;
                            // strip out the '-' from the date 
                            endDate = endDate.substring(0, 4) + endDate.substring(5, 7) + endDate.substring(8, 10);

                            var region = encodeURI(document.getElementById("txtRegion").value);
                            var conName = encodeURI(document.getElementById("txtConsName").value);
                            var conNumber = encodeURI(document.getElementById("txtConsNumber").value);


                            var url = "hn_SimpeFileUploader.ashx?start=" + startDate + "&end=" + endDate + "&region=" + region + "&name=" + conName + "&num=" + conNumber;
                            this.options.url = url;
                            console.log("url = " + url);
                        });
                        this.on("complete", function (file) {
                            this.removeFile(file);
                        });
                    },

                    success: function (file, response) {
                        console.log("success response " + response);
                        var obj = JSON.parse(response);
                        console.log("obj => " + obj);
                        console.log(" filename => " + obj.hasOwnProperty("filename"));
                        var result = "";

                        //file.previewElement.classList.add("dz-success");
                        var sts = document.getElementById("stsSpan");

                        if (obj.hasOwnProperty('filename')) {

                            // NB change this to the path of the generated files on the proper server 
                            //                        var sp = document.getElementById("hdnServerPrefix").value;

                            var hdnId = "<%=hdnServerPrefix.ClientID%>";
                            var hdn = document.getElementById(hdnId);
                            var sp = hdn.value;
                            console.log(" href path + filename  = " + sp + obj.filename);

                            var ih = "<b>File Processed Successfully</b><br/><br/><a href='" + sp + obj.filename + "' target='_blank'> DOWNLOAD REPORT FOR STATEMENT ACCOUNT </a>";

                            if (obj.hasOwnProperty("accountTotals")) {
                                ih = ih + "<br/><br/><table border = \"1\" borderpadding=\"5px\" ><tr><th>Account number</th><th>Interest Amount</th></tr> ";

                                var arr = obj.accountTotals;  // = [ {"id":"10", "class": "child-of-9"}, {"id":"11", "classd": "child-of-10"}];

                                for (var i = 0; i < arr.length; i++) {
                                    var a_obj = arr[i];
                                    ih += "<tr>";
                                    // console.log("array elements : " + a_obj);
                                    for (var key in a_obj) {
                                        var attrName = key;
                                        var attrValue = a_obj[key];
                                        if (attrName == "Total") {
                                            ih += "<td align=right>" + numberWithCommas(attrValue) + "&nbsp;</td>";
                                        }
                                        else {
                                            ih += "<td >" + attrValue.substring(0,2) + " " + attrValue.substring(2,5) + " " + attrValue.substring(5,8) + " " + attrValue.substring(8,9) +  "</td>";
                                        }
                                    }
                                    ih += "</tr>";
                                }
                                ih += "</table>";
                            }
                            sts.innerHTML = ih;
                        }


                    },


                    error: function (file, response) {
                        file.previewElement.classList.add("dz-error");
                    }
                });

                // read cookies look for region, name & number 
                document.getElementById("txtRegion").value = "";
                document.getElementById("txtConsName").value = "";
                document.getElementById("txtConsNumber").value = "";

                var region = getCookie("region");
                if (region != "") document.getElementById("txtRegion").value = decodeURI(region);
                var conName = getCookie("conName");
                if (conName != "") document.getElementById("txtConsName").value = decodeURI(conName);
                var conNumber = getCookie("conNumber");
                if (conNumber != "") document.getElementById("txtConsNumber").value = decodeURI(conNumber);


            });
        </script>
    </form>
</body>
</html>

