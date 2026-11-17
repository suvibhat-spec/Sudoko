using System.Text;

namespace SudokuGame;

public sealed class ConsoleGame
{
    private readonly SudokuPuzzleGenerator _generator;
    private readonly SudokuCommandParser _parser = new();

    public ConsoleGame(SudokuPuzzleGenerator generator)
    {
        _generator = generator;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine("Welcome to Sudoku!");
            Console.WriteLine();

            if (!PlayRound(_generator.Generate()))
            {
                return;
            }

            Console.WriteLine("Press Enter to play again, or type quit to exit.");
            var response = Console.ReadLine();
            if (response is null || response.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }

    private bool PlayRound(SudokuBoard board)
    {
        Console.WriteLine("Here is your puzzle:");
        PrintBoard(board);

        while (true)
        {
            Console.WriteLine();
            Console.Write("Enter command (e.g., A3 4, C5 clear, hint, check, quit): ");
            var input = Console.ReadLine();
            if (input is null)
            {
                return false;
            }

            var command = _parser.Parse(input);
            var changed = false;

            switch (command.Type)
            {
                case SudokuCommandType.Set:
                    if (board.TrySetValue(command.Row, command.Column, command.Value, out var setError))
                    {
                        Console.WriteLine("Move accepted.");
                        changed = true;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid move. {setError}");
                    }
                    break;

                case SudokuCommandType.Clear:
                    if (board.TryClear(command.Row, command.Column, out var clearError))
                    {
                        Console.WriteLine("Cell cleared.");
                        changed = true;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid move. {clearError}");
                    }
                    break;

                case SudokuCommandType.Hint:
                    var hint = board.RevealHint();
                    if (hint.HasValue)
                    {
                        Console.WriteLine(
                            $"Hint: Cell {SudokuBoard.FormatCell(hint.Value.Row, hint.Value.Column)} = {hint.Value.Value}");
                        changed = true;
                    }
                    else
                    {
                        Console.WriteLine("No hints are available.");
                    }
                    break;

                case SudokuCommandType.Check:
                    var violations = board.FindViolations();
                    if (violations.Count == 0)
                    {
                        Console.WriteLine("No rule violations detected.");
                    }
                    else
                    {
                        foreach (var violation in violations)
                        {
                            Console.WriteLine(violation);
                        }
                    }
                    break;

                case SudokuCommandType.Quit:
                    return false;

                default:
                    Console.WriteLine(command.Error);
                    break;
            }

            if (changed)
            {
                Console.WriteLine();
                Console.WriteLine("Current grid:");
                PrintBoard(board);

                if (board.IsCompleteAndCorrect())
                {
                    Console.WriteLine();
                    Console.WriteLine("You have successfully completed the Sudoku puzzle!");
                    return true;
                }
            }
        }
    }

    private static void PrintBoard(SudokuBoard board)
    {
        var output = new StringBuilder("    1 2 3 4 5 6 7 8 9");

        for (var row = 0; row < SudokuBoard.Size; row++)
        {
            output.AppendLine();
            output.Append("  ").Append((char)('A' + row));

            for (var column = 0; column < SudokuBoard.Size; column++)
            {
                output.Append(' ').Append(board.GetValue(row, column)?.ToString() ?? "_");
            }
        }

        Console.WriteLine(output);
    }
}