    let captcha;
    function generate() {
        document.getElementById("captchaSubmit").value = "";

        captcha = document.getElementById("image");
        let uniquechar = "";

        const randomchar =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        for (let i = 1; i < 5; i++) {
            uniquechar += randomchar.charAt(
                Math.random() * randomchar.length)
        }

        captcha.innerHTML = uniquechar;
    }

function printmsg() {
    const usr_input = document
        .getElementById("captchaSubmit").value;

        if (usr_input == captcha.innerHTML) {
            document.getElementById("captcha").value = "true";
        }
        else {
            document.getElementById("captcha").value = "false";
            generate();
        }
    }
