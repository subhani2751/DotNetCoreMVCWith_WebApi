$(document).ready(function () {
 let pagesize = 10;
    let pagenumber = 1;
    let totalRecords = 0;
    let totalPages = 1;
    $("#pageSize").change(function () {
        pagenumber = 1;
        pagesize = $(this).val();
        LoadTable();
    });

    $(".prevPage").click(function () {
        pagenumber--;
        LoadTable();
    });

    $(".nextPage").click(function () {
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
    LoadTable();
});
