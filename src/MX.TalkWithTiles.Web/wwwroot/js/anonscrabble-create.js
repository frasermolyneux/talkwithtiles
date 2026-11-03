let playerCount = 1;

function addAdditionalPlayer() {
    console.log("Adding additional player");

    playerCount++;

    if (playerCount >= 3) {
        document.getElementById("addAdditionalPlayers").style.display = "none";
    }

    const additionalPlayers = document.getElementById("additionalPlayers");
    const formGroup = document.createElement("div");
    formGroup.className = "form-group";
    const inputGroup = document.createElement("div");
    inputGroup.className = "input-group";
    const input = document.createElement("input");
    input.className = "form-control";
    input.dataset.val = "true";
    input.dataset.valEmail = "The Email field is not a valid e-mail address.";
    input.dataset.valRequired = "This field is required.";
    input.id = `PlayerModels_${playerCount}__PlayerName`;
    input.name = `PlayerModels[${playerCount}].PlayerName`;
    input.type = "text";
    const removeButton = document.createElement("button");
    removeButton.id = `removeAdditionalPlayer-${playerCount}`;
    removeButton.className = "btn btn-danger";
    removeButton.type = "button";
    const removeIcon = document.createElement("i");
    removeIcon.className = "fas fa-trash";
    removeButton.appendChild(removeIcon);
    inputGroup.append(input, removeButton);
    const validationMessage = document.createElement("span");
    validationMessage.className = "field-validation-valid text-danger";
    validationMessage.dataset.valmsgFor = `PlayerModels[${playerCount}].PlayerName`;
    validationMessage.dataset.valmsgReplace = "true";
    formGroup.append(inputGroup, validationMessage);
    additionalPlayers.before(formGroup);

    removeButton.addEventListener("click", function () {
        this.parentNode.parentNode.remove();
        playerCount--;

        if (playerCount < 3) {
            document.getElementById("addAdditionalPlayers").style.display = "";
        }
    });

    input.required = true;
}