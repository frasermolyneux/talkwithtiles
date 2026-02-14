let playerCount = 1;

function addAdditionalPlayer() {
    console.log("Adding additional player");

    playerCount++;

    if (playerCount >= 3) {
        $("#addAdditionalPlayers").hide();
    }

    $("#additionalPlayers").before(`<div class="form-group">
                            <div class="input-group-append">
                                <input class="form-control" data-val="true" data-val-email="The Email field is not a valid e-mail address." data-val-required="This field is required." id="PlayerModels_${
        playerCount}__PlayerName" name="PlayerModels[${playerCount}].PlayerName" type="text" value="">
                                <button id="removeAdditionalPlayer-${playerCount}" 
                                    class="btn-danger" type="button"> <i class="fas fa-trash"></i></button>
                            </div>
                            <span class="field-validation-valid text-danger" data-valmsg-for="PlayerModels[${
        playerCount}].PlayerName" data-valmsg-replace="true"></span>
                        </div>`);

    $(`#removeAdditionalPlayer-${playerCount}`).on("click",
        function() {

            this.parentNode.parentNode.remove();
            playerCount--;

            if (playerCount < 3) {
                $("#addAdditionalPlayers").show();
            }
        });

    $(`#PlayerModels_${playerCount}__PlayerName`).prop("required", true);
}