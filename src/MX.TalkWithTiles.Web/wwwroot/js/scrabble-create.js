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
    const formGroup = document.createElement("div");
    formGroup.className = "form-group";
    const inputGroup = document.createElement("div");
    inputGroup.className = "input-group";
    const input = document.createElement("input");
    input.className = "form-control";
    input.dataset.val = "true";
    input.dataset.valRequired = "This field is required.";
    input.id = `PlayerModels_${playerCount}__Identifier`;
    input.name = `PlayerModels[${playerCount}].Identifier`;
    input.type = "text";
    input.placeholder = "Enter your opponents username or email here";
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
    validationMessage.dataset.valmsgFor = `PlayerModels[${playerCount}].Identifier`;
    validationMessage.dataset.valmsgReplace = "true";
    formGroup.append(inputGroup, validationMessage);
    additionalPlayers.before(formGroup);

    const removeButtonId = playerCount;
    removeButton.addEventListener("click", function () {
        this.parentNode.parentNode.remove();

        if (document.querySelectorAll('[id^="removeAdditionalPlayer-"]').length < 2) {
            document.getElementById("addAdditionalPlayers").style.display = "";
        }
    });

    const newInput = input;
    newInput.required = true;
    setupAutocomplete(newInput);
}