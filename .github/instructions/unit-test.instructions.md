---
applyTo: "src/**/*.Tests/**,src/**/*.cs"
---

# C# Unit Testing Instructions

## Mandatory Rule

When modifying logic in any `src/` project (game engine, managers, controllers, helpers), always create or update corresponding unit tests. Do not treat unit tests as optional or deferred work.

## Test Stack

- **Framework**: xUnit 2.9.3 — use `[Fact]` for single cases, `[Theory]`/`[InlineData]` for parameterised cases.
- **Mocking**: Moq 4.20.72 — mock interfaces (`IBoardManager`, `IPlayerManager`, `ITileFactory`, etc.), not concrete classes.
- **SDK**: Microsoft.NET.Test.Sdk 17.12.0.
- **Target**: net9.0, C# 13, file-scoped namespaces, nullable reference types enabled.

## Project Mapping

| Source Project | Test Project | What to Test |
|---|---|---|
| `MX.TalkWithTiles.Scrabble` | `MX.TalkWithTiles.Scrabble.Tests` | Board manager logic, scoring, tile placement, validation |
| `MX.TalkWithTiles.CoreEngine` | `MX.TalkWithTiles.CoreEngine.Tests` | GameEngine orchestration, PlayerMoveManager, ChallengeManager, EndGameManager |
| `MX.TalkWithTiles.Web` | `MX.TalkWithTiles.Web.Tests` | Controller actions, view model logic, extensions |

## Naming and Organisation

- Test class name: `{ClassUnderTest}Tests` (e.g. `ScrabbleBoardManagerTests`).
- Group related tests into separate classes when a class has distinct responsibilities (e.g. `ScrabbleBoardManagerValidationTests` for placement validation, `ScrabbleBoardManagerTests` for scoring).
- Test method name: `{Method}_{Scenario}` (e.g. `MakeMove_RejectsOutOfBoundsTiles`, `MakeMove_AcceptsHorizontalLine`).
- One test file per test class.

## Test Structure

Follow Arrange/Act/Assert:

```csharp
[Fact]
public void MakeMove_RejectsPlacementOnOccupiedCell()
{
    // Arrange
    InitBoardWithExistingTiles();
    var move = CreateMove((7, 7, "Z"));

    // Act
    var result = _boardManager.MakeMove(move);

    // Assert
    Assert.False(result.IsValid);
    Assert.Matches("occupied|already", result.InvalidMessage!.ToLowerInvariant());
}
```

## Board Test Helpers

When testing `ScrabbleBoardManager`, use these patterns:

- **Empty board**: Create a `Tile[width, height]` array with `TileType.CentreTile` at the center coordinate. Use `InitFromStateModel()` to load it.
- **Board with existing tiles**: Same as empty board but set `Letter` on specific tiles to simulate prior moves.
- **Move helper**: Use a `CreateMove(params (int x, int y, string letter)[] placements)` factory that creates `PlayerMove` with `RackPosition = -1` for placed tiles.
- **Center coordinates**: StandardBoard(7,7), MiniBoard(4,4), SuperSizeBoard(9,9).

## Mocking Patterns

- `ScrabbleBoardManager` tests: Mock only `ITileFactory` (board manager has no other dependencies).
- `PlayerMoveManager` tests: Mock `IBoardManager`, `IPlayerManager`, `IBagManager`, `IEndGameManager` — these are all injected managers.
- `GameEngine` tests: Mock `IManagerFactory` which returns mocked managers.
- Controller tests: Mock `IGameEngineFactory`, `IGameStateRepository`, and related services.

## What to Test

For every logic change, cover:

1. **Positive cases** — valid inputs produce expected results.
2. **Negative cases** — invalid inputs are rejected with appropriate error messages.
3. **Boundary cases** — edge values (zero, max, empty collections, null).
4. **Regression guards** — existing behaviour is preserved (scoring, state changes, turn order).

For validation rules specifically, assert on:
- `result.IsValid` (true/false)
- `result.InvalidMessage` content (use `Assert.Contains` or `Assert.Matches` with key terms)
- `result.Points` (zero for invalid, correct value for valid)

## Running Tests

```bash
# All tests
dotnet test src/MX.TalkWithTiles.sln

# Single project
dotnet test src/MX.TalkWithTiles.Scrabble.Tests

# With verbosity
dotnet test src/MX.TalkWithTiles.sln --verbosity normal
```

Always run the full solution test suite after making changes to verify no regressions.
