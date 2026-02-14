let playerCount = 0;

function addAdditionalPlayer() {
    console.log("Adding additional player");

    playerCount++;

    if (playerCount >= 2) {
        $("#addAdditionalPlayers").hide();
    }

    $("#additionalPlayers").before(`<div class="form-group">
                            <div class="input-group-append">
                                <input class="form-control" data-val="true" data-val-required="This field is required." id="PlayerModels_${
        playerCount}__Identifier" name="PlayerModels[${playerCount
        }].Identifier" type="text" value="" placeholder="Enter your opponents username or email here">
                                <button id="removeAdditionalPlayer-${playerCount}" 
                                    class="btn-danger" type="button"> <i class="fas fa-trash"></i></button>
                            </div>
                            <span class="field-validation-valid text-danger" data-valmsg-for="PlayerModels[${
        playerCount}].Identifier" data-valmsg-replace="true"></span>
                        </div>`);

    $(`#removeAdditionalPlayer-${playerCount}`).on("click",
        function() {

            this.parentNode.parentNode.remove();
            playerCount--;

            if (playerCount < 2) {
                $("#addAdditionalPlayers").show();
            }
        });

    $(`#PlayerModels_${playerCount}__PlayerName`).prop("required", true);

    $(`#PlayerModels_${playerCount}__Identifier`).autocomplete({
        source: contacts
    });
}