// =============================================================================
//  Coding Round — Mock 1  (CoderPad-style single-pad harness)
//
//  Run:  cd coding_round/tictactoe && dotnet run
//
//  Workflow (coding-round mode):
//    1. Design the interface + implement in the SOLUTION region.
//    2. Reach the TEST phase — add real edge-case tests in Program.Main.
//    3. Do the follow-up variant when the interviewer asks.
// =============================================================================

// ─────────────────────────────  SOLUTION  ───────────────────────────────────
// Write your class(es) here.

class TicTacToe
{
    // Size
    private readonly int size;
    // Board: -1 - X; 1 - O
    private int moveCount;
    private bool gameOver;

    private int[] sumRow;
    private int[] sumCol;
    private int sumMainDiag;
    private int sumSecDiag;

    private HashSet<(int, int)> moves;

    public TicTacToe(int s)
    {
        // If size negative, throw
        if(s <= 0) 
                throw new ArgumentOutOfRangeException(nameof(s), s, "Board size must be a positive integer.");

        moves = new();
        size = s;
        moveCount = 0;
        gameOver = false;

        sumRow = new int[size];
        sumCol = new int[size];

        // Default to 0 but we can do this if we don't rely on that
        Array.Fill(sumRow, 0);
        Array.Fill(sumCol, 0);
        sumMainDiag = 0;
        sumSecDiag = 0;
    }

    // Returns 0 - nobody won yet; 1 - X won; 2 - O won
    public int Move(int row, int col)
    {
        // Out of bounds
        if(row < 0 || row >= size || col < 0 || col >= size)
            throw new ArgumentOutOfRangeException($"Move {row}, {col} out of bounds for board of size {size}.");

        // Game already over, throw
        if(gameOver)
            throw new InvalidOperationException("Game already over.");

        // Position already written, throw
        if(moves.Contains((row, col)))
            throw new InvalidOperationException("Move has already been played");

        moves.Add((row, col));
        moveCount++;
        int mark = moveCount % 2 == 1 ? -1 : 1;

        sumRow[row] += mark;
        sumCol[col] += mark;

        // If on main diag
        if(row == col) sumMainDiag += mark;
        // If on sec diag
        if(row + col == size - 1) sumSecDiag += mark;

        // Win-check
        // X won - 1
        if(sumRow[row] == -size || sumCol[col] == -size || sumMainDiag == -size || sumSecDiag == -size) 
        {
            gameOver = true;
            return 1;
        }

        // O won - 2
        if(sumRow[row] == size || sumCol[col] == size || sumMainDiag == size || sumSecDiag == size) 
        {
            gameOver = true;
            return 2;
        }

        return 0;
    }
}



// ──────────────────────────────  HARNESS  ───────────────────────────────────
static class T
{
    static int passed, failed;

    public static void Eq<TV>(string name, TV actual, TV expected)
    {
        if (Equals(actual, expected)) { passed++; Console.WriteLine($"  PASS  {name}"); }
        else { failed++; Console.WriteLine($"  FAIL  {name}  expected={expected}  actual={actual}"); }
    }

    public static void True(string name, bool cond)
    {
        if (cond) { passed++; Console.WriteLine($"  PASS  {name}"); }
        else { failed++; Console.WriteLine($"  FAIL  {name}"); }
    }

    public static void Throws<TEx>(string name, Action act) where TEx : Exception
    {
        try { act(); failed++; Console.WriteLine($"  FAIL  {name}  (no exception thrown)"); }
        catch (TEx) { passed++; Console.WriteLine($"  PASS  {name}"); }
        catch (Exception e) { failed++; Console.WriteLine($"  FAIL  {name}  (wrong exception {e.GetType().Name})"); }
    }

    public static void Summary()
        => Console.WriteLine($"\n{passed} passed, {failed} failed");
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Coding Round — Mock 1 ===\n");

        // ───────────────────────  TESTS  ────────────────────────
        // Add tests here during the testing phase, e.g.:
        //   var game = new TicTacToe(3);
        //   T.Eq("row win", game.Move(0, 2, 1), 1);

        var game1 = new TicTacToe(3);
        // Consecutive non-winning moves yield 0, X wins by row
        // Mixed row, no win
        T.Eq("move", game1.Move(0, 0), 0); // X
        T.Eq("move", game1.Move(0, 1), 0); // O
        T.Eq("move", game1.Move(0, 2), 0); // X
        // O fills row 1
        T.Eq("move", game1.Move(1, 0), 0); // O
        T.Eq("move", game1.Move(2, 0), 0); // X
        T.Eq("move", game1.Move(1, 1), 0); // O
        T.Eq("move", game1.Move(2, 1), 0); // X
        T.Eq("move", game1.Move(1, 2), 2); // O

        var game2 = new TicTacToe(3);
        // X wins by row
        T.Eq("move", game2.Move(0, 0), 0); // X
        T.Eq("move", game2.Move(1, 0), 0); // O
        T.Eq("move", game2.Move(0, 2), 0); // X
        T.Eq("move", game2.Move(1, 1), 0); // O
        T.Eq("move", game2.Move(0, 1), 1); // X

        var game3 = new TicTacToe(3);
        // X wins by col
        T.Eq("move", game3.Move(0, 2), 0); // X
        T.Eq("move", game3.Move(1, 0), 0); // O
        T.Eq("move", game3.Move(2, 2), 0); // X
        T.Eq("move", game3.Move(1, 1), 0); // O
        T.Eq("move", game3.Move(1, 2), 1); // X

        // X wins by main diag
        var game4 = new TicTacToe(3);
        T.Eq("move", game4.Move(0, 0), 0); // X
        T.Eq("move", game4.Move(1, 0), 0); // O
        T.Eq("move", game4.Move(1, 1), 0); // X
        T.Eq("move", game4.Move(1, 2), 0); // O
        T.Eq("move", game4.Move(2, 2), 1); // X

        // O wins by sec diag
        var game5 = new TicTacToe(3);
        T.Eq("move", game5.Move(1, 0), 0); // X
        T.Eq("move", game5.Move(0, 2), 0); // O
        T.Eq("move", game5.Move(2, 2), 0); // X
        T.Eq("move", game5.Move(1, 1), 0); // O
        T.Eq("move", game5.Move(0, 1), 0); // X
        T.Eq("move", game5.Move(2, 0), 2); // O

        // 1x1 board - X wins
        var game6 = new TicTacToe(1);
        T.Eq("move", game6.Move(0, 0), 1); // X

        // Full board - draw - all moves return 0
        var game7 = new TicTacToe(3);
        T.Eq("move", game7.Move(0, 0), 0); // X
        T.Eq("move", game7.Move(0, 1), 0); // O
        T.Eq("move", game7.Move(0, 2), 0); // X
        T.Eq("move", game7.Move(2, 0), 0); // O
        T.Eq("move", game7.Move(2, 1), 0); // X
        T.Eq("move", game7.Move(2, 2), 0); // O
        T.Eq("move", game7.Move(1, 0), 0); // X
        T.Eq("move", game7.Move(1, 1), 0); // O
        T.Eq("move", game7.Move(1, 2), 0); // X

        // Invalid board size
        T.Throws<ArgumentOutOfRangeException>("invalid board size", () => { var g = new TicTacToe(0); });

        // out of bounds
        T.Throws<ArgumentOutOfRangeException>("out of bounds", () => 
        { 
            var g = new TicTacToe(3);
            g.Move(-1, 2); 
        });
        // out of bounds
        T.Throws<ArgumentOutOfRangeException>("out of bounds", () => 
        { 
            var g = new TicTacToe(3);
            g.Move(2, 4); 
        });

        // move already made
        T.Throws<InvalidOperationException>("move already made", () => 
        { 
            var g = new TicTacToe(3);
            g.Move(2, 2);
            g.Move(2, 2);
        });

        // game already over
        T.Throws<InvalidOperationException>("game already over", () => 
        { 
            var g = new TicTacToe(3);
            // X wins by col
            g.Move(0, 2);
            g.Move(1, 0);            
            g.Move(2, 2);
            g.Move(1, 1);
            g.Move(1, 2); // X won
            g.Move(0, 1);
        });

        T.Summary();
    }
}
