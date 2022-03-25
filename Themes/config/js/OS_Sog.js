$(document).ready(function () {

    $('#OS_Sog_cmdSave').unbind("click");
    $('#OS_Sog_cmdSave').click(function () {
        $('.processing').show();
        $('.actionbuttonwrapper').hide();
        // lower case cmd must match ajax provider ref.
        nbxget('OS_Sog_savesettings', '.OS_Sogdata', '.OS_Sogreturnmsg');
    });

    $('.selectlang').unbind("click");
    $(".selectlang").click(function () {
        $('.editlanguage').hide();
        $('.actionbuttonwrapper').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        // lower case cmd must match ajax provider ref.
        nbxget('OS_Sog_selectlang', '.OS_Sogdata', '.OS_Sogdata');
    });

    $(document).on("nbxgetcompleted", OS_Sog_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function OS_Sog_nbxgetCompleted(e) {

        $('.processing').hide();
        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        if (e.cmd == 'OS_Sog_selectlang') {
                        
        }
        else if (e.cmd == 'OS_Sog_tokenize') {
            // Set form token
            KR.setFormToken($(".OS_Sogdata").text());

            // Add listener for submit event
            KR.onSubmit(onPaid);
        }

    };

});

