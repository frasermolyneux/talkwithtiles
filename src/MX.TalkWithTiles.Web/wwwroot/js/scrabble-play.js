let userTiles = [];
let tileWidth;
let gameId;
let playerId;
let currentPlayerId;
let gameEtag;
let selectedTile;

function InitGameGlobals(theGameId, thePlayerId, theCurrentPlayerId, theGameEtag) {
    gameId = theGameId;
    playerId = thePlayerId;
    currentPlayerId = theCurrentPlayerId;
    gameEtag = theGameEtag;

    setInterval(checkForMovePlayed, 5000);

    $("#recallTiles").click(recallTiles);
    $("#shuffleTiles").click(shuffleTiles);

    $(window).on("resize",
        function() {
            InitSizes();
        });
}

function InitTileRack() {
    var i;
    for (i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];

        addTileToRack(tile, `rack_${tile.rackPosition}`);
    }

    $(".scrabbleRackCell").droppable({
        drop: function(event, ui) {
            const rackId = $(this).attr("id");
            const tileId = $(ui.draggable).attr("id");
            const tile = userTiles.find((o) => { return o["tileId"] === tileId });

            console.log(`Tile ${tileId} has been dropped onto ${rackId}`);

            removeDraggableTile(tile);
            addTileToRack(tile, rackId);
            clearSelectedTile();
        }
    });

    $(".scrabbleRackCell").on("click touch",
        function(event, ui) {
            if ($(this).children().length > 0) {
                return;
            }

            if (selectedTile != null) {

                const rackId = $(this).attr("id");
                const tileId = selectedTile.attr("id");
                const tile = userTiles.find((o) => { return o["tileId"] === tileId });

                console.log(`Tile ${tileId} has been dropped onto ${rackId}`);

                removeDraggableTile(tile);
                addTileToRack(tile, rackId);
                clearSelectedTile();
            }
        });
}

function InitBoard() {
    $(".availableBoardCell").droppable({
        drop: function(event, ui) {
            const cellId = $(this).attr("id");
            const tileId = $(ui.draggable).attr("id");
            const tile = userTiles.find((o) => { return o["tileId"] === tileId });

            console.log(`Tile ${tileId} has been dropped onto ${cellId}`);

            removeDraggableTile(tile);
            addTileToBoard(tile, cellId);
            clearSelectedTile();
        }
    });

    $(".availableBoardCell").on("click touch",
        function(event, ui) {
            if ($(this).children().length > 0) {
                return;
            }

            if (selectedTile != null) {

                const cellId = $(this).attr("id");
                const tileId = selectedTile.attr("id");
                const tile = userTiles.find((o) => { return o["tileId"] === tileId });

                console.log(`Tile ${tileId} has been dropped onto ${cellId}`);

                removeDraggableTile(tile);
                addTileToBoard(tile, cellId);
                clearSelectedTile();
            }
        });
}

function InitSizes() {
    if ($(window).width() < 768) {
        var mainContainerWidth = $(window).width();

        var board = $("#scrabbleBoard");
        var boardWidth = (mainContainerWidth * 95) * 0.01;

        board.width(boardWidth);
        board.height($("#scrabbleBoard").width());
    } else if ($(window).width() > 1200) {
        var mainContainerWidth = $("#mainContainer").width();

        var board = $("#scrabbleBoard");
        var boardWidth = (mainContainerWidth * 50) * 0.01;

        board.width(boardWidth);
        board.height($("#scrabbleBoard").width());
    } else {
        var mainContainerWidth = $("#mainContainer").width();

        var board = $("#scrabbleBoard");
        var boardWidth = (mainContainerWidth * 65) * 0.01;

        board.width(boardWidth);
        board.height($("#scrabbleBoard").width());
    }

    tileWidth = $("#cell_0-0").width();
    console.log(`The global tile width is: ${tileWidth}`);

    $(".scrabbleRackCell").width(tileWidth);
    $(".scrabbleRackCell").height(tileWidth);

    $(".rackScrabbleTile").width(tileWidth);
    $(".rackScrabbleTile").height(tileWidth);

    $(".scrabbleTile").width(tileWidth - 6);
    $(".scrabbleTile").height(tileWidth - 6);
}

function clearSelectedTile() {
    if (selectedTile != null) {
        selectedTile.removeClass("selectedTile");
        selectedTile = null;
        $(".selectedTileMessage").hide();
    }
}

function setSelectedTile(element) {
    selectedTile = element;
    selectedTile.addClass("selectedTile");
    $(".selectedTileMessage").show();
}

function removeDraggableTile(tile) {
    const rackTile = $(`#${tile.tileId}`);
    rackTile.draggable("destroy");
    rackTile.remove();
}

function addTileToBoard(tile, position) {
    const cell = $(`#${position}`);
    console.log(`Adding '${tile.letter}' to board in postion ${position}`);
    cell.append(
        `<img id="${tile.tileId}" src="/img/tiles/${tile.letter}.jpg" class="scrabbleTile" alt="${tile.letter
        }" style="width:${tileWidth}px; height:${tileWidth}px; padding:3px;">`);

    const cellRegex = new RegExp("^cell_([0-9]{1,2})-([0-9]{1,2})$");
    const match = cellRegex.exec(position);

    tile.posX = parseInt(match[1]);
    tile.posY = parseInt(match[2]);

    tile.rackPosition = -1;

    updateMoveScore();

    $(`#${tile.tileId}`).draggable({
        opacity: 0.8,
        revert: "invalid",
        start: function(event, ui) {
            $(ui.helper).css("width", "50%");
            $(ui.helper).css("height", "50%");
        },
        stop: function(event, ui) {
            $(ui.helper).css("width", "100%");
            $(ui.helper).css("height", "100%");
        }
    });

    $(`#${tile.tileId}`).droppable({
        greedy: true,
        tolerance: "touch",
        drop: function(event, ui) {
            ui.draggable.draggable("option", "revert", true);
        }
    });

    $(`#${tile.tileId}`).on("click touch",
        function() {
            clearSelectedTile();
            setSelectedTile($(this));
        });
}

function addTileToRack(tile, position) {
    const rackCell = $(`#${position}`);
    console.log(`Adding '${tile.letter}' to rack in postion ${tile.rackPosition}`);
    rackCell.append(
        `<img id="${tile.tileId}" src="/img/tiles/${tile.letter}.jpg" class="rackScrabbleTile" alt="${tile.letter
        }" style="width:${tileWidth}px; height:${tileWidth}px">`);

    const rackRegex = new RegExp("^rack_([0-9]{1,2})$");
    const match = rackRegex.exec(position);

    tile.posX = 0;
    tile.posY = 0;

    tile.rackPosition = parseInt(match[1]);

    updateMoveScore();

    $(`#${tile.tileId}`).draggable({
        opacity: 0.8,
        revert: "invalid",
        start: function(event, ui) {
            $(ui.helper).css("width", "50%");
            $(ui.helper).css("height", "50%");
        },
        stop: function(event, ui) {
            $(ui.helper).css("width", "100%");
            $(ui.helper).css("height", "100%");
        }
    });

    $(`#${tile.tileId}`).droppable({
        greedy: true,
        tolerance: "touch",
        drop: function(event, ui) {
            ui.draggable.draggable("option", "revert", true);
        }
    });

    $(`#${tile.tileId}`).on("click touch",
        function() {
            clearSelectedTile();
            setSelectedTile($(this));
        });
}

function updateMoveScore() {
    var tilesOnBoard = userTiles.find((o) => { return o["rackPosition"] === -1 });

    if (!tilesOnBoard) {
        $("#turnScore").hide();
        return;
    }

    $.ajax({
        url: `/Scrabble/GetPlayerMoveResult/${gameId}`,
        headers: {
            'RequestVerificationToken': antiforgeryToken
        },
        type: "POST",
        data: JSON.stringify({
            "PlayerId": playerId,
            "Tiles": userTiles
        }),
        dataType: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function(response) {
            console.log(response);

            const wordsAndPoints = response.wordsAndPoints;

            var i;
            var turnScoreEl = document.getElementById("turnScore");
            turnScoreEl.textContent = "";

            var strong = document.createElement("strong");
            strong.textContent = "Score: ";
            turnScoreEl.appendChild(strong);

            for (i = 0; i < wordsAndPoints.length; i++) {
                turnScoreEl.appendChild(document.createTextNode(wordsAndPoints[i].word + " (" + wordsAndPoints[i].score + ") "));
            }
            turnScoreEl.appendChild(document.createTextNode("= " + response.points));

            tilesOnBoard = userTiles.find((o) => { return o["rackPosition"] === -1 });
            if (!tilesOnBoard) {
                $("#turnScore").hide();
            } else {
                $("#turnScore").show();
            }
        }
    });
}

function submitPlayerMove() {
    const tilesOnBoard = userTiles.find((o) => { return o["rackPosition"] === -1 });

    if (!tilesOnBoard)
        return;

    $.ajax({
        url: `/Scrabble/SubmitPlayerMove/${gameId}`,
        headers: {
            'RequestVerificationToken': antiforgeryToken
        },
        type: "POST",
        data: JSON.stringify({
            "PlayerId": playerId,
            "Tiles": userTiles
        }),
        dataType: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function(response) {
            console.log(response);
            location.reload();
        }
    });
}

function checkForMovePlayed() {
    console.log("Checking to see if the other player has made their move");

    $.ajax({
        url: `/Scrabble/GetGameEtag/${gameId}`,
        headers: {
            'RequestVerificationToken': antiforgeryToken
        },
        type: "POST",
        dataType: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function(response) {
            console.log(response);

            if (parseInt(response.gameEtag) !== gameEtag) {
                location.reload();
            }
        }
    });
}

function recallTiles() {
    var i;
    for (i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];

        if (tile.rackPosition === -1) {
            $(`#${tile.tileId}`).remove();

            var j;
            for (j = 0; j < 8; j++) {
                if (!userTiles.find((o) => { return o["rackPosition"] === j })) {
                    addTileToRack(tile, `rack_${j}`);
                    break;
                }
            }
        }
    }

    updateMoveScore();
}

function shuffle(array) {
    var currentIndex = array.length, temporaryValue, randomIndex;
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

    var i;
    for (i = 0; i < userTiles.length; i++) {
        const tile = userTiles[i];
        $(`#${tile.tileId}`).remove();
    }

    shuffle(userTiles);

    var j;
    for (j = 0; j < userTiles.length; j++) {
        const tile = userTiles[j];
        tile.rackPosition = j;
    }

    InitTileRack();
}