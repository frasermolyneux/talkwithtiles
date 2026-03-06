let playerCount = 0;

function setupAutocomplete(inputEl) {
    const listId = inputEl.id + "_suggestions";
    let datalist = document.getElementById(listId);
    if (!datalist) {
        datalist = document.createElement("datalist");
        datalist.id = listId;
        if (typeof contacts !== "undefined") {
            contacts.forEach(function (c) {
                const option = document.createElement("option");
                option.value = c;
                datalist.appendChild(option);
            });
        }
        inputEl.parentNode.appendChild(datalist);
    }
    inputEl.setAttribute("list", listId);
}

function addAdditionalPlayer() {
    console.log("Adding additional player");

    playerCount++;

    if (playerCount >= 2) {
        document.getElementById("addAdditionalPlayers").style.display = "none";
    }

    const additionalPlayers = document.getElementById("additionalPlayers");
    additionalPlayers.insertAdjacentHTML("beforebegin", `<div class="form-group">
                            <div class="input-group">
                                <input class="form-control" data-val="true" data-val-required="This field is required." id="PlayerModels_${playerCount}__Identifier" name="PlayerModels[${playerCount}].Identifier" type="text" value="" placeholder="Enter your opponents username or email here">
                                <button id="removeAdditionalPlayer-${playerCount}" 
                                    class="btn btn-danger" type="button"> <i class="fas fa-trash"></i></button>
                            </div>
                            <span class="field-validation-valid text-danger" data-valmsg-for="PlayerModels[${playerCount}].Identifier" data-valmsg-replace="true"></span>
                        </div>`);

    const removeButtonId = playerCount;
    document.getElementById(`removeAdditionalPlayer-${removeButtonId}`).addEventListener("click", function () {
        this.parentNode.parentNode.remove();

        if (document.querySelectorAll('[id^="removeAdditionalPlayer-"]').length < 2) {
            document.getElementById("addAdditionalPlayers").style.display = "";
        }
    });

    const newInput = document.getElementById(`PlayerModels_${playerCount}__Identifier`);
    newInput.required = true;
    setupAutocomplete(newInput);
}