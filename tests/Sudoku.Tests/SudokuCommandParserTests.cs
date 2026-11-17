using SudokuGame;
using Xunit;

namespace Sudoku.Tests;

public sealed class SudokuCommandParserTests
{
    private readonly SudokuCommandParser _parser = new();

    [Theory]
    [InlineData("A3 4", 0, 2, 4)]
    [InlineData("i9 7", 8, 8, 7)]
    public void ParsesSetCommands(string input, int row, int column, int value)
    {
        var command = _parser.Parse(input);

        Assert.Equal(SudokuCommandType.Set, command.Type);
        Assert.Equal(row, command.Row);
        Assert.Equal(column, command.Column);
        Assert.Equal(value, command.Value);
    }

    [Theory]
    [InlineData("C5 clear", SudokuCommandType.Clear)]
    [InlineData("hint", SudokuCommandType.Hint)]
    [InlineData("CHECK", SudokuCommandType.Check)]
    [InlineData("quit", SudokuCommandType.Quit)]
    public void ParsesCommands(string input, SudokuCommandType expectedType)
    {
        Assert.Equal(expectedType, _parser.Parse(input).Type);
    }

    [Theory]
    [InlineData("J1 3")]
    [InlineData("A0 3")]
    [InlineData("A3 nope")]
    [InlineData("A3 4 extra")]
    public void RejectsMalformedCommands(string input)
    {
        Assert.Equal(SudokuCommandType.Invalid, _parser.Parse(input).Type);
    }
}