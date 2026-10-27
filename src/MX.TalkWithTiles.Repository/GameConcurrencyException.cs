using System;

namespace MX.TalkWithTiles.Repository;

public sealed class GameConcurrencyException() : Exception("The game changed. Reload it before trying again.");
