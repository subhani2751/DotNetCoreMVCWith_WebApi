    let pagesize = 10;
    let pagenumber = 1;
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

    function LoadTable() {
        $.ajax({
            url: 'https://localhost:44389/api/ApiEmployee/getTablePageData?pagesize=' + pagesize + '&pagenumber=' + pagenumber,
            type: 'GET',
            contentType: 'application/json',
            // data : {pagesize:pagesize,pagenumber:pagenumber},
            success: function (result) {
                console.log(result); 
            },
            Error: function (Er) {
                console.log(Er); 
            }
        });
    }
