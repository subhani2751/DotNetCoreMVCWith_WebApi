$.validator.addMethod("salaryRange", function (value, element) {
    return this.optional(element) || new RegExp("^\\d+(\\.\\d{1,2})?$").test(value);
}, "Enter a valid salary (numbers and up to 2 decimal places only)");

$.validator.addMethod("EmailFormate", function (value, element) {
    return this.optional(element) || new RegExp("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$").test(value);
}, "Enter a valid Email only");

$.validator.addMethod("AlphabelsFormate", function (value, element) {
    return this.optional(element) || new RegExp("^[a-zA-Z\\s]+$").test(value);
}, "Accepts only alphabets and spaces");



    function logout() {
        localStorage.removeItem("jwt"); // Or sessionStorage.removeItem("jwt")
        $.ajax({
            url: '/Employee/Login?partialviewneed=1',
            type: 'GET',
            success: function (data) {
                $(".containermain").html(data);
            },
            error: function (err) {
                alert('Access denied or not authorized');
            }
        });
       // window.location.href = "/Employee/Login";
        //alert("Session expired due to inactivity.");
}

    async function parseJwtandGetTokenTime() {
        const token = await localStorage.getItem('jwt');
        let _Rvalue = 0;
        if (token) {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(c =>
                '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
            //JSON.parse(jsonPayload)
            const payload = JSON.parse(jsonPayload);
            _Rvalue = payload.exp
        }
        return _Rvalue;
    }

    let logoutTimer;
    async function startInactivityTimer() {
    const token = localStorage.getItem('jwt');
    if (token) {
        clearTimeout(logoutTimer);
        let TokenTime = await parseJwtandGetTokenTime();
        TokenTime = (TokenTime * 1000) - new Date().getTime();
        logoutTimer = setTimeout(() => {
            logout();
        }, TokenTime);
      }
    }

    async function resetInactivityTimerAndRefreshToken() {
        const token = localStorage.getItem('jwt');
        if (token) {
            let TokenTime = await parseJwtandGetTokenTime();
            TokenTime = (TokenTime * 1000) - new Date().getTime();
            if (TokenTime <= ((1000 * 60) * 60)) {
                // Call refresh endpoint
                $.ajax({
                    url: '/api/ApiEmployee/RefreshToken',
                    type: 'POST',
                    contentType: 'application/json',
                    headers: {
                        'Authorization': 'Bearer ' + token
                    },
                    success: function (res) {
                        if (res.token) {
                            localStorage.setItem('jwt', res.token);
                            startInactivityTimer();
                        }
                    },
                    error: function () {
                        console.log("Failed to refresh token.");
                    }
                });
            }

        }
    }

    document.addEventListener('click', function (e) {
        if (e.target.id !== "Logoutbtn") {
            resetInactivityTimerAndRefreshToken();
        }
    });
    $(document).on('click', '#Logoutbtn', logout);
    //window.addEventListener('beforeunload', removeJWT);
    //window.addEventListener('unload', removeJWT);

    //window.addEventListener('unload', () => {
    //    debugger;
    //    localStorage.removeItem('jwt');
    //});
    function removeJWT() {
        localStorage.removeItem('jwt');
    }

    // Attach events to detect user interaction
    window.onbeforeunload = removeJWT;
    //window.onunload = removeJWT;
    window.onload = startInactivityTimer;
    //document.onclick = resetInactivityTimerAndRefreshToken;
    //document.onmousemove = resetInactivityTimerAndRefreshToken;
    //document.onkeydown = resetInactivityTimerAndRefreshToken;
    //document.onscroll = resetInactivityTimerAndRefreshToken;
    //document.onmousedown = resetInactivityTimerAndRefreshToken;
$(document).on('click','#importExcel',function () {
    // $("#excelFile").click();
    $('#excelFile').trigger('click');
});
$(document).on('change','#excelFile',function () {
    var file = this.files[0];
    if (file) {
        var formData = new FormData();
        formData.append("excelFile", file);

        $.ajax({
            url: "/api/ApiEmployee/ImportExcel",
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    $("#employeeTableBody").empty();
                    listEmployees(); // Refresh the table
                    alert("Excel file imported successfully!");
                } else {
                    alert("Error importing Excel: " + response.message);
                }
            },
            error: function (err) {
                alert("Error importing Excel file: " + err.responseText);
            }
        });
    }
});

$(document).on("click", ".export-option", function () {
    let format = $(this).data("format");
    let selectedIds = [];

    $(".rowCheckbox:checked").each(function () {
        selectedIds.push($(this).data("id"));
    });
    var empIdsCSV = selectedIds.join(',');

    if (selectedIds.length === 0) {
        alert("Please select at least one employee to export.");
        return;
    }

    $.ajax({
        url: "/api/ApiEmployee/Export?format=" + format + "&ids=" + empIdsCSV,
        type: "POST",
        //data: JSON.stringify({ format: format, ids: empIdsCSV }),
        xhrFields: { responseType: 'blob' }, // Handle binary data
        success: function (data, status, xhr) {
            let contentDisposition = xhr.getResponseHeader("Content-Disposition");
            let filename = contentDisposition ? contentDisposition.split("filename=")[1] : "SelectedEmployees.xlsx";
            let fileType = "";
            if (format === "Excel") fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            else if (format === "CSV") fileType = "text/csv";
            else if (format === "PDF") fileType = "application/pdf";
            let blob = new Blob([data], { type: fileType });
            let link = document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (e) {
            alert("Error exporting file. Please try again.");
        }
    });
});


 let pagesize = 10;
    let pagenumber = 1;
    let totalRecords = 0;
    let totalPages = 1;
    $(document).on('change', '#pageSize', function () {
        pagenumber = 1;
        pagesize = $(this).val();
        LoadTable();
    });

    $(document).on('click','.prevPage',function () {
        pagenumber--;
        LoadTable();
    });

    $(document).on('click','.nextPage',function () {
        pagenumber++;
        LoadTable();
    });

    function updatePaginationUI() {
        totalPages = Math.ceil(totalRecords / pagesize);
        $("#currentPageText").text(pagenumber);
        $("#totalPagesText").text(totalPages);

        // Enable/Disable buttons based on current page
        $(".prevPage").prop("disabled", pagenumber === 1);
        $(".nextPage").prop("disabled", pagenumber === totalPages || totalPages === 0);
}
//async function LoadTable() {
//    const url = `/api/ApiEmployee/getTablePageData?pagesize=${pagesize}&pagenumber=${pagenumber}`;
//    console.log(url);

//    try {
//        const Result = await $.ajax({
//            url: url,
//            type: 'GET',
//            contentType: 'application/json'
//        });

//        $("#employeeTableBody").empty();
//        const tableBody = $("#employeeTableBody");
//        const lstemployees = Result.lstEmployees;
//        totalRecords = Result.totalRecords;

//        $.each(lstemployees, function (index, emp) {
//            const row = `<tr id="row_${emp.employeeId}">
//                <td><input type="checkbox" class="rowCheckbox" data-id="${emp.employeeId}"></td>
//                <td>${emp.employeeId}</td>
//                <td>${emp.firstName}</td>
//                <td>${emp.lastName}</td>
//                <td>${emp.email}</td>
//                <td>${emp.department}</td>
//                <td>${emp.hireDate ? emp.hireDate.split("T")[0] : ""}</td>
//                <td>${emp.salary}</td>
//                <td>
//                    <button class="btn btn-success btn-sm Edit" data-id="${emp.employeeId}">Edit</button>
//                    <button class="btn btn-danger btn-sm deleteRow" data-id="${emp.employeeId}">Delete</button>
//                </td>
//            </tr>`;
//            tableBody.append(row);
//        });

//        updatePaginationUI();
//    } catch (Er) {
//        console.log("Error:", Er);
//    }
//}


    function LoadTable() {
        const url = '/api/ApiEmployee/getTablePageData?pagesize=' + pagesize + '&pagenumber=' + pagenumber;
        console.log(url);
        $.ajax({
            url: url,
            type: 'GET',
            contentType: 'application/json',
            // data : {pagesize:pagesize,pagenumber:pagenumber},
            success: function (Result) {
                $("#employeeTableBody").empty();
                var tableBody = $("#employeeTableBody");
                var lstemployees = Result.lstEmployees;
                totalRecords = Result.totalRecords;
                $.each(lstemployees, function (index, emp) {
                    var row = `<tr id="row_${emp.employeeId}">
                                        <td><input type="checkbox" class="rowCheckbox" data-id="${emp.employeeId}"></td>
                                        <td>${emp.employeeId}</td>
                                        <td>${emp.firstName}</td>
                                        <td>${emp.lastName}</td>
                                        <td>${emp.email}</td>
                                        <td>${emp.department}</td>
                                        <td>${emp.hireDate ? emp.hireDate.split("T")[0] : ""}</td>
                                        <td>${emp.salary}</td>
                                        <td>
                                            <button class="btn btn-success btn-sm Edit" data-id="${emp.employeeId}">Edit</button>
                                            <button class="btn btn-danger btn-sm deleteRow" data-id="${emp.employeeId}">Delete</button>
                                        </td>
                                    </tr>`;
                    tableBody.append(row);
                });
                updatePaginationUI();
            },
            Error: function (Er) {
                console.log(Er); 
            }
        });
    }

async function Ajaxcallcommon(Url, type) {
    const token = await localStorage.getItem('jwt');
    $.ajax({
        url: Url,
        type: type,
        headers: {
            'Authorization': `Bearer ${token}`
        },
        success: function (data) {
            $(".containermain").html(data);
        },
        error: function (err) {
            alert('Access denied or not authorized');
        }
    });
}

