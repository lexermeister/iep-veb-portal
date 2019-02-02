window.onload = function () {
    
    events();
}

function events() {

    //change first name
    $('#changeFirstNameIcon').on("click", function () {
        var form = $('#hidden-form-firstname');

        if (form.css('display') == 'none')
            form.css('display', 'inline');
        else
            form.css('display', 'none');
    });

    //change last name
    $('#changeLastNameIcon').on("click", function () {
        var form = $('#hidden-form-lastname');

        if (form.css('display') == 'none')
            form.css('display', 'inline');
        else
            form.css('display', 'none');
    });

    //change e-mail
    $('#changeEmail').on("click", function () {
        var form = $('#hidden-form-email');

        if (form.css('display') == 'none')
            form.css('display', 'inline');
        else
            form.css('display', 'none');
    });

    //change password
    $('#changePassword').on("click", function () {        
        var form = $('#hidden-form-password');

        if (form.css('display') == 'none')
            form.css('display', 'inline');
        else
            form.css('display', 'none');
    });
}