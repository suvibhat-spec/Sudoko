using System.Globalization;

namespace SudokuGame;

public enum SudokuCommandType
{
    Invalid,
    Set,
    Clear,
    Hint,
    Check,
    Quit
}

public readonly record struct SudokuCommand(
    SudokuCommandType Type,
    int Row = 0,
    int Column = 0,
    int Value = 0,
    string Error = "");

public sealed class SudokuCommandParser
{
    public SudokuCommand Parse(string input)
    {
        var parts = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            return parts[0].ToLowerInvariant() switch
            {
                "hint" => new SudokuCommand(SudokuCommandType.Hint),
                "check" => new SudokuCommand(SudokuCommandType.Check),
                "quit" => new SudokuCommand(SudokuCommandType.Quit),
                _ => InvalidCommand()
            };
        }

        if (parts.Length != 2 || !TryParseCell(parts[0], out var row, out var column))
        {
            return InvalidCommand();
        }

        if (parts[1].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            return new SudokuCommand(SudokuCommandType.Clear, row, column);
        }

        if (int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            return new SudokuCommand(SudokuCommandType.Set, row, column, value);
        }

        return InvalidCommand();
    }

    private static bool TryParseCell(string input, out int row, out int column)
    {
        row = 0;
        column = 0;

        if (input.Length != 2 || char.ToUpperInvariant(input[0]) is < 'A' or > 'I' || input[1] is < '1' or > '9')
        {
            return false;
        }

        row = char.ToUpperInvariant(input[0]) - 'A';
        column = input[1] - '1';
        return true;
    }

    private static SudokuCommand InvalidCommand() =>
        new(SudokuCommandType.Invalid, Error: "Use A3 4, C5 clear, hint, check, or quit.");
}