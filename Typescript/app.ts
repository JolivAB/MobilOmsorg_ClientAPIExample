namespace App {
    const API = "http://localhost:52798/api";
    // const API = "https://apibeta.mobilomsorg.se/api";
    const TOKEN_ENDPOINT = "/oauth2/token";

    let form: JQuery,
        companyCode: JQuery,
        userName: JQuery,
        password: JQuery,
        otp: JQuery,
        result: JQuery;

    interface TokenEndpointResult {
        access_token: string;
        mfa_token: string;
        // ...
    }

    function login_first_factor(companyCode: string, userName: string, password: string) {
        console.log("logging in user with password");
        return $.ajax({
            type: "POST",
            url: API + TOKEN_ENDPOINT,
            data: {
                grant_type: "password",
                username: userName + "@" + companyCode,
                password: password
            }
        });
    }

    function login_second_factor(access_token: string, companyCode: string, userName: string, otp: string) {
        console.log("sending otp code");
        return $.ajax({
            type: "POST",
            url: API + TOKEN_ENDPOINT,
            data: {
                grant_type: "mfa_totp",
                username: userName + "@" + companyCode,
                code: otp
            },
            beforeSend: (request: JQuery.jqXHR) => {
                request.setRequestHeader("Authorization", "Bearer " + access_token);
            }
        });
    }

    function get_user_information(access_token: string) {
        console.log("getting user information");
        return $.ajax({
            type: "GET",
            url: API + "/authentication/get",
            beforeSend: (request: JQuery.jqXHR) => {
                request.setRequestHeader("Authorization", "Bearer " + access_token);
            }
        });
    }

    export function setup() {
        form = $("#authentication-form");
        companyCode = $("#companycode-field");
        userName = $("#username-field");
        password = $("#password-field");
        otp = $("#otp-field");
        result = $("#authentication-result");

        form.on("submit", (evt: JQuery.Event) => {
            evt.preventDefault();
            result.text("");

            login_first_factor(companyCode.val() as string, userName.val() as string, password.val() as string)
                .done((authResult: TokenEndpointResult) => {
                    console.log("Password was accepted");
                    result.text("Password ok");

                    login_second_factor(authResult.access_token, companyCode.val() as string, userName.val() as string, otp.val() as string)
                        .done((authResult: TokenEndpointResult) => {
                            console.log("OTP was accepted");
                            result.text("OTP ok");

                            get_user_information(authResult.access_token)
                                .done((user: any) => {
                                    console.log("Got user information", user);
                                    result.text("Done!");
                                })
                                .fail((res: JQuery.jqXHR) => {
                                    console.error("Couldn't get user information", res.responseJSON);
                                    result.text("Authentication failed");
                                });
                        })
                        .fail((res: JQuery.jqXHR) => {
                            console.error("OTP authentication failed", res.responseJSON);
                            result.text("Authentication failed");
                        });
                })
                .fail((res: JQuery.jqXHR) => {
                    console.error("Password authentication failed", res.responseJSON);
                    result.text("Authentication failed");
                });
        });
    }
}

$(function () {
    App.setup();
});
