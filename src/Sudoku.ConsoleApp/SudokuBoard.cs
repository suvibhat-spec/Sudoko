namespace SudokuGame;

public sealed class SudokuBoard
{
    public const int Size = 9;
    public const int ClueCount = 30;

    private readonly int?[,] _values;
    private readonly bool[,] _fixedCells;
    private readonly int[,] _solution;

    internal SudokuBoard(int?[,] values, bool[,] fixedCells, int[,] solution)
    {
        _values = (int?[,])values.Clone();
        _fixedCells = (bool[,])fixedCells.Clone();
        _solution = (int[,])solution.Clone();
    }

    public int? GetValue(int row, int column)
    {
        ValidateCoordinates(row, column);
        return _values[row, column];
    }

    public bool IsFixed(int row, int column)
    {
        ValidateCoordinates(row, column);
        return _fixedCells[row, column];
    }

    public bool TrySetValue(int row, int column, int value, out string error)
    {
        if (!IsValidCoordinate(row, column))
        {
            error = "The cell must be between A1 and I9.";
            return false;
        }

        if (_fixedCells[row, column])
        {
            error = $"{FormatCell(row, column)} is pre-filled.";
            return false;
        }

        if (value is < 1 or > Size)
        {
            error = "The number must be between 1 and 9.";
            return false;
        }

        _values[row, column] = value;
        error = string.Empty;
        return true;
    }

    public bool TryClear(int row, int column, out string error)
    {
        if (!IsValidCoordinate(row, column))
        {
            error = "The cell must be between A1 and I9.";
            return false;
        }

        if (_fixedCells[row, column])
        {
            error = $"{FormatCell(row, column)} is pre-filled.";
            return false;
        }

        _values[row, column] = null;
        error = string.Empty;
        return true;
    }

    public Hint? RevealHint(Random? random = null)
    {
        var candidates = new List<(int Row, int Column)>();

        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (!_fixedCells[row, column] && _values[row, column] != _solution[row, column])
                {
                    candidates.Add((row, column));
                }
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        var cell = candidates[(random ?? Random.Shared).Next(candidates.Count)];
        var value = _solution[cell.Row, cell.Column];
        _values[cell.Row, cell.Column] = value;
        _fixedCells[cell.Row, cell.Column] = true;
        return new Hint(cell.Row, cell.Column, value);
    }

    public IReadOnlyList<string> FindViolations()
    {
        var violations = new List<string>();

        for (var row = 0; row < Size; row++)
        {
            var rowIndex = row;
            AddDuplicateViolations(
                Enumerable.Range(0, Size).Select(column => _values[rowIndex, column]),
                $"Row {(char)('A' + row)}",
                violations);
        }

        for (var column = 0; column < Size; column++)
        {
            var columnIndex = column;
            AddDuplicateViolations(
                Enumerable.Range(0, Size).Select(row => _values[row, columnIndex]),
                $"Column {column + 1}",
                violations);
        }

        for (var boxRow = 0; boxRow < 3; boxRow++)
        {
            for (var boxColumn = 0; boxColumn < 3; boxColumn++)
            {
                var cells = from rowOffset in Enumerable.Range(0, 3)
                            from columnOffset in Enumerable.Range(0, 3)
                            select _values[boxRow * 3 + rowOffset, boxColumn * 3 + columnOffset];
                AddDuplicateViolations(cells, "same 3x3 subgrid", violations);
            }
        }

        return violations;
    }

    public bool IsCompleteAndCorrect()
    {
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (!_values[row, column].HasValue)
                {
                    return false;
                }
            }
        }

        return FindViolations().Count == 0;
    }

    public static string FormatCell(int row, int column) => $"{(char)('A' + row)}{column + 1}";

    private static void AddDuplicateViolations(
        IEnumerable<int?> values,
        string group,
        ICollection<string> violations)
    {
        var seen = new HashSet<int>();
        var duplicates = new HashSet<int>();

        foreach (var value in values)
        {
            if (value.HasValue && !seen.Add(value.Value))
            {
                duplicates.Add(value.Value);
            }
        }

        foreach (var duplicate in duplicates.Order())
        {
            violations.Add($"Number {duplicate} already exists in {group}.");
        }
    }

    private static bool IsValidCoordinate(int row, int column) =>
        row is >= 0 and < Size && column is >= 0 and < Size;

    private static void ValidateCoordinates(int row, int column)
    {
        if (!IsValidCoordinate(row, column))
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Cell coordinates must be within the board.");
        }
    }
}

public readonly record struct Hint(int Row, int Column, int Value);