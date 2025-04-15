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

