---
description: "Repository-specific xUnit and Moq conventions for .NET test source files."
applyTo: "src/**/*.Tests/**/*.cs"
---

# .NET unit tests

- Use xUnit and Moq. Mock injected interfaces rather than concrete manager or
  repository implementations.
- Name test classes `{ClassUnderTest}Tests` and test methods
  `{Method}_{Scenario}`.
- Keep tests arranged as Arrange, Act, Assert and cover the behavior changed,
  including relevant invalid or boundary cases.

Project mapping:

| Production project | Test project |
| --- | --- |
| `MX.TalkWithTiles.CoreEngine` | `MX.TalkWithTiles.CoreEngine.Tests` |
| `MX.TalkWithTiles.Scrabble` | `MX.TalkWithTiles.Scrabble.Tests` |
| `MX.TalkWithTiles.Web` | `MX.TalkWithTiles.Web.Tests` |

For manager tests, reuse the existing dependency boundaries:

- `GameEngine`: mock `IManagerFactory`.
- `PlayerMoveManager`: mock its board, player, bag, and end-game managers.
- `ScrabbleBoardManager`: mock `ITileFactory`.
- Web controllers: mock `IGameEngineFactory`, repository interfaces, and
  related services.

Run the affected test project or a focused `FullyQualifiedName` filter first.
Escalate to the solution test suite when the change crosses project boundaries
or targeted results indicate a broader regression risk.
