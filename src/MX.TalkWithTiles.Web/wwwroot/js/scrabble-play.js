let userTiles = [];
let gameId;
let playerId;
let currentPlayerId;
let gameEtag;
let selectedTile;
let draggedTileId = null;

function InitGameGlobals(theGameId, thePlayerId, theCurrentPlayerId, theGameEtag) {
    gameId = theGameId;
    playerId = thePlayerId;
    currentPlayerId = theCurrentPlayerId;
    gameEtag = theGameEtag;

    setInterval(checkForMovePlayed, 5000);

    document.getElementById("recallTiles").addEventListener("click", recallTiles);
    document.getElementById("shuffleTiles").addEventListener("click", shuffleTiles);
}

function InitTileRack() {
    for (let i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];
        addTileToRack(tile, `rack_${tile.rackPosition}`);
    }

    document.querySelectorAll(".scrabbleRackCell").forEach(function (cell) {
        cell.addEventListener("dragover", function (e) {
            e.preventDefault();
        });

        cell.addEventListener("drop", function (e) {
            e.preventDefault();
            const tileId = e.dataTransfer.getData("text/plain");
            if (!tileId) return;

            const rackId = this.id;
            const tile = userTiles.find(function (o) { return o.tileId === tileId; });
            if (!tile) return;

            console.log(`Tile ${tileId} has been dropped onto ${rackId}`);

            removeDraggableTile(tile);
            addTileToRack(tile, rackId);
            clearSelectedTile();
        });

        cell.addEventListener("click", function () {
            if (this.children.length > 0) {
                return;
            }

            if (selectedTile != null) {
                const rackId = this.id;
                const tileId = selectedTile.id;
                const tile = userTiles.find(function (o) { return o.tileId === tileId; });
                if (!tile) return;

                console.log(`Tile ${tileId} has been dropped onto ${rackId}`);

                removeDraggableTile(tile);
                addTileToRack(tile, rackId);
                clearSelectedTile();
            }
        });
    });
}

function InitBoard() {
    document.querySelectorAll(".availableBoardCell").forEach(function (cell) {
        cell.addEventListener("dragover", function (e) {
            e.preventDefault();
        });

        cell.addEventListener("drop", function (e) {
            e.preventDefault();
            const tileId = e.dataTransfer.getData("text/plain");
            if (!tileId) return;

            const cellId = this.id;
            const tile = userTiles.find(function (o) { return o.tileId === tileId; });
            if (!tile) return;

            console.log(`Tile ${tileId} has been dropped onto ${cellId}`);

            removeDraggableTile(tile);
            addTileToBoard(tile, cellId);
            clearSelectedTile();
        });

        cell.addEventListener("click", function () {
            if (this.children.length > 0) {
                return;
            }

            if (selectedTile != null) {
                const cellId = this.id;
                const tileId = selectedTile.id;
                const tile = userTiles.find(function (o) { return o.tileId === tileId; });
                if (!tile) return;

                console.log(`Tile ${tileId} has been dropped onto ${cellId}`);

                removeDraggableTile(tile);
                addTileToBoard(tile, cellId);
                clearSelectedTile();
            }
        });
    });
}

function InitSizes() {
    // Board sizing is now handled by CSS (aspect-ratio + max-width).
    // This function is kept as a no-op for backward compatibility.
}

function clearSelectedTile() {
    if (selectedTile != null) {
        selectedTile.classList.remove("selectedTile");
        selectedTile = null;
        document.querySelectorAll(".selectedTileMessage").forEach(function (el) {
            el.classList.add("d-none");
        });
    }
}

function setSelectedTile(element) {
    selectedTile = element;
    selectedTile.classList.add("selectedTile");
    document.querySelectorAll(".selectedTileMessage").forEach(function (el) {
        el.classList.remove("d-none");
    });
}

function removeDraggableTile(tile) {
    const rackTile = document.getElementById(tile.tileId);
    if (rackTile) {
        rackTile.remove();
    }
}

function makeTileDraggable(tileEl) {
    tileEl.setAttribute("draggable", "true");

    tileEl.addEventListener("dragstart", function (e) {
        draggedTileId = this.id;
        e.dataTransfer.setData("text/plain", this.id);
        e.dataTransfer.effectAllowed = "move";
        this.style.opacity = "0.5";
    });

    tileEl.addEventListener("dragend", function () {
        this.style.opacity = "1";
        draggedTileId = null;
    });
}

function addTileToBoard(tile, position) {
    const cell = document.getElementById(position);
    console.log(`Adding '${tile.letter}' to board in postion ${position}`);
    cell.insertAdjacentHTML("beforeend",
        `<img id="${tile.tileId}" src="/img/tiles/${tile.letter.toLowerCase()}.jpg" class="scrabbleTile scrabbleTile--placed" alt="${tile.letter}" draggable="true">`);

    const cellRegex = new RegExp("^cell_([0-9]{1,2})-([0-9]{1,2})$");
    const match = cellRegex.exec(position);

    tile.posX = parseInt(match[1]);
    tile.posY = parseInt(match[2]);

    tile.rackPosition = -1;

    updateMoveScore();

    const tileEl = document.getElementById(tile.tileId);
    makeTileDraggable(tileEl);

    tileEl.addEventListener("click", function () {
        clearSelectedTile();
        setSelectedTile(this);
    });
}

function addTileToRack(tile, position) {
    const rackCell = document.getElementById(position);
    console.log(`Adding '${tile.letter}' to rack in postion ${tile.rackPosition}`);
    rackCell.insertAdjacentHTML("beforeend",
        `<img id="${tile.tileId}" src="/img/tiles/${tile.letter.toLowerCase()}.jpg" class="rackScrabbleTile" alt="${tile.letter}" draggable="true">`);

    const rackRegex = new RegExp("^rack_([0-9]{1,2})$");
    const match = rackRegex.exec(position);

    tile.posX = 0;
    tile.posY = 0;

    tile.rackPosition = parseInt(match[1]);

    updateMoveScore();

    const tileEl = document.getElementById(tile.tileId);
    makeTileDraggable(tileEl);

    tileEl.addEventListener("click", function () {
        clearSelectedTile();
        setSelectedTile(this);
    });
}

async function updateMoveScore() {
    const tilesOnBoard = userTiles.find(function (o) { return o.rackPosition === -1; });
    const turnScoreEl = document.getElementById("turnScore");

    if (!tilesOnBoard) {
        turnScoreEl.style.display = "none";
        return;
    }

    try {
        const response = await fetch(`/Scrabble/GetPlayerMoveResult/${gameId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
                "RequestVerificationToken": antiforgeryToken
            },
            body: JSON.stringify({
                "PlayerId": playerId,
                "Tiles": userTiles
            })
        });

        if (!response.ok) {
            console.error("Failed to get move score:", response.status);
            return;
        }

        const result = await response.json();
        console.log(result);

        const wordsAndPoints = result.wordsAndPoints;

        turnScoreEl.textContent = "";

        const strong = document.createElement("strong");
        strong.textContent = "Score: ";
        turnScoreEl.appendChild(strong);

        for (let i = 0; i < wordsAndPoints.length; i++) {
            turnScoreEl.appendChild(document.createTextNode(wordsAndPoints[i].word + " (" + wordsAndPoints[i].score + ") "));
        }
        turnScoreEl.appendChild(document.createTextNode("= " + result.points));

        const stillOnBoard = userTiles.find(function (o) { return o.rackPosition === -1; });
        if (!stillOnBoard) {
            turnScoreEl.style.display = "none";
        } else {
            turnScoreEl.style.display = "";
        }
    } catch (err) {
        console.error("Failed to update move score:", err);
    }
}

async function submitPlayerMove() {
    const tilesOnBoard = userTiles.find(function (o) { return o.rackPosition === -1; });

    if (!tilesOnBoard)
        return;

    try {
        const response = await fetch(`/Scrabble/SubmitPlayerMove/${gameId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
                "RequestVerificationToken": antiforgeryToken
            },
            body: JSON.stringify({
                "PlayerId": playerId,
                "Tiles": userTiles
            })
        });

        if (!response.ok) {
            console.error("Failed to submit move:", response.status);
            return;
        }

        const result = await response.json();
        console.log(result);
        location.reload();
    } catch (err) {
        console.error("Failed to submit player move:", err);
    }
}

async function checkForMovePlayed() {
    console.log("Checking to see if the other player has made their move");

    try {
        const response = await fetch(`/Scrabble/GetGameEtag/${gameId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
                "RequestVerificationToken": antiforgeryToken
            }
        });

        if (!response.ok) {
            console.error("Failed to check for move:", response.status);
            return;
        }

        const result = await response.json();
        console.log(result);

        if (parseInt(result.gameEtag) !== gameEtag) {
            location.reload();
        }
    } catch (err) {
        console.error("Failed to check for move played:", err);
    }
}

function recallTiles() {
    for (let i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];

        if (tile.rackPosition === -1) {
            const tileEl = document.getElementById(tile.tileId);
            if (tileEl) tileEl.remove();

            for (let j = 0; j < 8; j++) {
                if (!userTiles.find(function (o) { return o.rackPosition === j; })) {
                    addTileToRack(tile, `rack_${j}`);
                    break;
                }
            }
        }
    }

    updateMoveScore();
}

function shuffle(array) {
    let currentIndex = array.length, temporaryValue, randomIndex;
    while (0 !== currentIndex) {
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex -= 1;

        temporaryValue = array[currentIndex];
        array[currentIndex] = array[randomIndex];
        array[randomIndex] = temporaryValue;
    }
    return array;
}

function shuffleTiles() {
    recallTiles();

    for (let i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];
        const tileEl = document.getElementById(tile.tileId);
        if (tileEl) tileEl.remove();
    }

    shuffle(userTiles);

    for (let j = 0; j < userTiles.length; j++) {
        const tile = userTiles[j];
        tile.rackPosition = j;
    }

    InitTileRack();
}