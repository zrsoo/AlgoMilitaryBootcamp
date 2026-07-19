// =============================================================================
//  Coding Round — Mock 2  (CoderPad-style single-pad harness)
//
//  Run:  cd coding_round/gameoflife && dotnet run
//
//  Workflow (coding-round mode):
//    1. Design the interface + implement in the SOLUTION region.
//    2. Reach the TEST phase — add real edge-case tests in Program.Main.
//    3. Do the follow-up variant when the interviewer asks.
// =============================================================================

// ─────────────────────────────  SOLUTION  ───────────────────────────────────
// Write your class(es) here.
class GameOfLife
{
    private readonly List<(int i, int j)> dirs = 
        new List<(int i, int j)>{(-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1), (-1, -1)};
    private int[,] cells;

    public GameOfLife(int[,] c)
    {
        cells = c;
    }

    public List<(int row, int col, int state)> AdvanceGeneration()
    {
        List<(int row, int col, int state)> changes = new();

        for(int i = 0; i < cells.GetLength(0); ++i)
        {
            for(int j = 0; j < cells.GetLength(1); ++j)
            {
                int liveNeighbourCount = LiveNeighbourCount(i, j);

                // Console.WriteLine($"i: {i}; j: {j}; liveNeighborCount: {liveNeighbourCount}");

                // Live cell
                if(cells[i, j] == 1)
                {
                    if(liveNeighbourCount < 2 || liveNeighbourCount > 3) 
                    {
                        // Console.WriteLine($"{i}; {j} -> 0");
                        changes.Add((i, j, 0));
                    }
                } // Dead cell
                else if(liveNeighbourCount == 3) changes.Add((i, j, 1));
            }
        }

        foreach(var (row, column, state) in changes)
        {
            // Console.WriteLine($"{row}; {column} -> {state}");
            cells[row, column] = state;
        }

        return changes;
    }

    public void PrintCells()
    {
        for(int i = 0; i < cells.GetLength(0); ++i)
        {
            for(int j = 0; j < cells.GetLength(1); ++j)
                Console.Write($"{cells[i, j]} ");
            Console.WriteLine();
        }
    }

    private int LiveNeighbourCount(int i, int j)
    {
        int ni, nj;
        int cnt = 0;

        foreach(var d in dirs)
        {
            ni = i + d.i;
            nj = j + d.j;

            if(ni < 0 || ni >= cells.GetLength(0) || nj < 0 || nj >= cells.GetLength(1)) continue;

            cnt += cells[ni, nj];
        }

        return cnt;
    }
}


// ─────────────────────────────  HARNESS  ────────────────────────────────────
// Tiny assert helpers. Don't touch unless you want more assertion kinds.

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
        Console.WriteLine("=== Coding Round — Mock 2 ===\n");

        // ───────────────────────  TESTS  ────────────────────────
        // Add tests here during the testing phase.

        // Fewer than two live neighbors dies
        int[,] cells = { {0, 0, 0}, {0, 1, 0}, {0, 0, 1} };
        var expected = new List<(int row, int col, int state)>{ (1, 1, 0), (2, 2, 0) };
        var game = new GameOfLife(cells);
        var actual = game.AdvanceGeneration();
        T.True("same count", expected.Count == actual.Count);
        foreach(var ch in expected) T.True("all contained", actual.Contains(ch));
        game.PrintCells();


        T.Summary();
    }
}
