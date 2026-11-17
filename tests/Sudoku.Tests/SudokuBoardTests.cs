using SudokuGame;
using Xunit;

namespace Sudoku.Tests;

public sealed class SudokuBoardTests
{
    [Fact]
    public void RunDisplaysThirtyCluesAndEmptyCells()
    {
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        using var input = new StringReader("quit");
        using var output = new StringWriter();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);
            new ConsoleGame(new SudokuPuzzleGenerator(new Random(1234))).Run();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }

        var display = output.ToString();
        Assert.StartsWith("Welcome to Sudoku!", display);
        Assert.Contains("Here is your puzzle:", display);

        var gridRows = display
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line =>
            {
                var trimmed = line.TrimStart();
                return trimmed.Length > 1 && trimmed[0] is >= 'A' and <= 'I' && char.IsWhiteSpace(trimmed[1]);
            })
            .ToArray();
        var cells = gridRows
            .SelectMany(line => line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))
            .ToArray();

        Assert.Equal(SudokuBoard.Size, gridRows.Length);
        Assert.Equal(SudokuBoard.ClueCount, cells.Count(cell => int.TryParse(cell, out _)));
        Assert.Equal(SudokuBoard.Size * SudokuBoard.Size - SudokuBoard.ClueCount, cells.Count(cell => cell == "_"));
    }

    [Fact]
    public void GeneratedPuzzleContainsExactlyThirtyFixedClues()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var fixedCells = 0;

        for (var row = 0; row < SudokuBoard.Size; row++)
        {
            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                if (board.IsFixed(row, column))
                {
                    fixedCells++;
                    Assert.InRange(board.GetValue(row, column)!.Value, 1, 9);
                }
                else
                {
                    Assert.Null(board.GetValue(row, column));
                }
            }
        }

        Assert.Equal(SudokuBoard.ClueCount, fixedCells);
        Assert.Empty(board.FindViolations());
    }

    [Fact]
    public void FixedClueCannotBeChangedOrCleared()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column) = FindFixedCell(board);

        Assert.False(board.TrySetValue(row, column, 1, out var setError));
        Assert.Contains("pre-filled", setError);
        Assert.False(board.TryClear(row, column, out var clearError));
        Assert.Contains("pre-filled", clearError);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ValuesOutsideOneToNineAreRejected(int value)
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column) = FindEmptyCell(board);

        Assert.False(board.TrySetValue(row, column, value, out var error));
        Assert.Contains("between 1 and 9", error);
    }

    [Fact]
    public void EditableCellCanBeSetAndCleared()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column) = FindEmptyCell(board);

        Assert.True(board.TrySetValue(row, column, 1, out _));
        Assert.Equal(1, board.GetValue(row, column));
        Assert.True(board.TryClear(row, column, out _));
        Assert.Null(board.GetValue(row, column));
    }

    [Fact]
    public void CheckFindsDuplicateInRow()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column, value) = FindConflictTarget(
            board,
            _ => true,
            (first, second) => first.Row == second.Row);

        Assert.True(board.TrySetValue(row, column, value, out _));
        Assert.Contains(board.FindViolations(), violation => violation.Contains($"Row {(char)('A' + row)}"));
    }

    [Fact]
    public void CheckFindsDuplicateInColumn()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column, value) = FindConflictTarget(
            board,
            _ => true,
            (first, second) => first.Column == second.Column);

        Assert.True(board.TrySetValue(row, column, value, out _));
        Assert.Contains(board.FindViolations(), violation => violation.Contains($"Column {column + 1}"));
    }

    [Fact]
    public void CheckFindsDuplicateInSubgrid()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();
        var (row, column, value) = FindConflictTarget(
            board,
            _ => true,
            (first, second) => first.Row / 3 == second.Row / 3 && first.Column / 3 == second.Column / 3);

        Assert.True(board.TrySetValue(row, column, value, out _));
        Assert.Contains(board.FindViolations(), violation => violation.Contains("same 3x3 subgrid"));
    }

    [Fact]
    public void HintsRevealCorrectValuesAndEventuallyCompletePuzzle()
    {
        var board = new SudokuPuzzleGenerator(new Random(1234)).Generate();

        while (board.RevealHint(new Random(1234)) is not null)
        {
        }

        Assert.True(board.IsCompleteAndCorrect());
        Assert.Null(board.RevealHint());
    }

    private static (int Row, int Column) FindFixedCell(SudokuBoard board)
    {
        for (var row = 0; row < SudokuBoard.Size; row++)
        {
            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                if (board.IsFixed(row, column))
                {
                    return (row, column);
                }
            }
        }

        throw new InvalidOperationException("Generated puzzle has no fixed clues.");
    }

    private static (int Row, int Column) FindEmptyCell(SudokuBoard board)
    {
        for (var row = 0; row < SudokuBoard.Size; row++)
        {
            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                if (!board.IsFixed(row, column))
                {
                    return (row, column);
                }
            }
        }

        throw new InvalidOperationException("Generated puzzle has no empty cells.");
    }

    private static (int Row, int Column, int Value) FindConflictTarget(
        SudokuBoard board,
        Func<(int Row, int Column), bool> inScope,
        Func<(int Row, int Column), (int Row, int Column), bool> inSameGroup)
    {
        for (var row = 0; row < SudokuBoard.Size; row++)
        {
            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                if (board.IsFixed(row, column) || !inScope((row, column)))
                {
                    continue;
                }

                for (var other = 0; other < SudokuBoard.Size; other++)
                {
                    for (var otherColumn = 0; otherColumn < SudokuBoard.Size; otherColumn++)
                    {
                        if ((other == row && otherColumn == column) ||
                            !inSameGroup((row, column), (other, otherColumn)))
                        {
                            continue;
                        }

                        var existing = board.GetValue(other, otherColumn);
                        if (existing.HasValue)
                        {
                            return (row, column, existing.Value);
                        }
                    }
                }
            }
        }

        throw new InvalidOperationException("Could not find a conflict target in the generated puzzle.");
    }
}