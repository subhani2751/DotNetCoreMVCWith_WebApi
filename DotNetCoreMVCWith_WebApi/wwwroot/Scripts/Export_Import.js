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
