function logout() {
    debugger;
        localStorage.removeItem("jwt"); // Or sessionStorage.removeItem("jwt")
        $.ajax({
            url: '/Employee/Login',
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
    $('#Logoutbtn').on('click', logout);
    //window.addEventListener('beforeunload', removeJWT);
    //window.addEventListener('unload', removeJWT);

    //window.addEventListener('unload', () => {
    //    debugger;
    //    localStorage.removeItem('jwt');
    //});
    function removeJWT() {
        debugger;
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