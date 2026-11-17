# Sudoku Console App

A command-line Sudoku game written in C#/.NET. Each new game generates a valid completed grid, then hides 51 cells to leave exactly 30 pre-filled clues. The solution is retained privately by the game for hints and completion validation.

## Requirements

- .NET 8 SDK or later
- Windows, Linux, or macOS with a terminal

## Design

`ConsoleGame` handles the command-line interaction, `SudokuCommandParser` converts user input into commands, and `SudokuBoard` owns the grid state and validates moves. `SudokuPuzzleGenerator` uses randomized backtracking to create a complete solution, then selects 30 cells as clues. The solution stays with the board for hints and completion checks. Puzzles have a known completion, but solution uniqueness is not guaranteed.

## Run

From the project directory:

```powershell
dotnet run --project src/Sudoku.ConsoleApp/Sudoku.ConsoleApp.csproj
```

Enter moves as `A3 4`, clear a cell with `C5 clear`, or enter `hint`, `check`, or `quit`. Rows are labeled A-I and columns 1-9. Conflicting entries are allowed so `check` can report duplicates in rows, columns, and 3x3 subgrids. Pre-filled clues and hint-revealed cells cannot be changed.

The puzzle always has exactly 30 pre-filled values.

## Test

```powershell
dotnet test tests/Sudoku.Tests/Sudoku.Tests.csproj
```

Tests cover clue count, move and clear behavior, hints and completion, duplicate detection, and command parsing.