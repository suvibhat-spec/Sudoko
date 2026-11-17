namespace SudokuGame;

public sealed class SudokuPuzzleGenerator
{
    private const int Size = 9;
    private readonly Random _random;

    public SudokuPuzzleGenerator(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    public SudokuBoard Generate()
    {
        var solution = CreateSolution();
        var values = new int?[Size, Size];
        var fixedCells = new bool[Size, Size];
        var positions = Enumerable.Range(0, Size * Size)
            .OrderBy(_ => _random.Next())
            .ToArray();
        var blankCount = Size * Size - SudokuBoard.ClueCount;

        for (var index = blankCount; index < positions.Length; index++)
        {
            var row = positions[index] / Size;
            var column = positions[index] % Size;

            values[row, column] = solution[row, column];
            fixedCells[row, column] = true;
        }

        return new SudokuBoard(values, fixedCells, solution);
    }

    private int[,] CreateSolution()
    {
        var values = new int[Size, Size];
        var rows = new bool[Size, Size + 1];
        var columns = new bool[Size, Size + 1];
        var boxes = new bool[Size, Size + 1];

        if (!Fill(0))
        {
            throw new InvalidOperationException("Unable to generate a Sudoku solution.");
        }

        return values;

        bool Fill(int position)
        {
            if (position == Size * Size)
            {
                return true;
            }
            var row = position / Size;
            var column = position % Size;
            var box = GetBoxIndex(row, column);
            var candidates = Enumerable.Range(1, Size).ToArray();
            Shuffle(candidates);

            foreach (var number in candidates)
            {
                if (rows[row, number] || columns[column, number] || boxes[box, number])
                {
                    continue;
                }
                values[row, column] = number;
                rows[row, number] = true;
                columns[column, number] = true;
                boxes[box, number] = true;

                if (Fill(position + 1))
                {
                    return true;
                }

                values[row, column] = 0;
                rows[row, number] = false;
                columns[column, number] = false;
                boxes[box, number] = false;
            }

            return false;
        }
    }
    private void Shuffle<T>(IList<T> items)
    {
        for (var index = items.Count - 1; index > 0; index--)
        {
            var swapIndex = _random.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }
    private static int GetBoxIndex(int row, int column) => row / 3 * 3 + column / 3;
}