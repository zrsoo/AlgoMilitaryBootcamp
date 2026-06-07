using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        var cases = new List<TestCase>();
        cases.AddRange(HandCrafted());
        cases.AddRange(RandomCases(seed: 12345, count: 2000));

        var failures = new List<(TestCase tc, string got)>();
        int passed = 0;

        for (int i = 0; i < cases.Count; i++)
        {
            var tc = cases[i];

            // Enforce the problem's guarantee: each employee's intervals are
            // sorted by start and non-overlapping (touching is allowed). If a
            // test ever violates this, the harness is invalid — fail loudly.
            string violation = ValidateInput(tc.Schedule);
            if (violation != null)
            {
                Console.WriteLine($"[INVALID INPUT] Test {i + 1} ({tc.Name}): {violation}");
                Console.WriteLine($"        Input = {FormatSchedule(tc.Schedule)}");
                Environment.ExitCode = 2;
                return;
            }

            string got = Format(new Solution().EmployeeFreeTime(Build(tc.Schedule)));
            bool ok = got == tc.Expected;

            // Explicit output for the first 5 tests, LeetCode-style.
            if (i < 5)
            {
                Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] Test {i + 1}: {tc.Name}");
                Console.WriteLine($"        Input    = {FormatSchedule(tc.Schedule)}");
                Console.WriteLine($"        Expected = {tc.Expected}");
                Console.WriteLine($"        Got      = {got}");
            }

            if (ok) passed++;
            else failures.Add((tc, got));
        }

        Console.WriteLine();
        Console.WriteLine(new string('-', 48));
        Console.WriteLine($"Total: {cases.Count}   Passed: {passed}   Failed: {failures.Count}");

        if (failures.Count > 0)
        {
            var (tc, got) = failures[0];
            Console.WriteLine();
            Console.WriteLine("First failing test:");
            Console.WriteLine($"  Name     : {tc.Name}");
            Console.WriteLine($"  Input    : {FormatSchedule(tc.Schedule)}");
            Console.WriteLine($"  Expected : {tc.Expected}");
            Console.WriteLine($"  Got      : {got}");
            Environment.ExitCode = 1;
        }
        else
        {
            Console.WriteLine("All tests passed.");
        }
    }

    // ---------- Test definitions ----------

    private sealed class TestCase
    {
        public string Name;
        public (int, int)[][] Schedule;
        public string Expected;
    }

    private static IEnumerable<TestCase> HandCrafted()
    {
        var raw = new (string, (int, int)[][])[]
        {
            ("Example 1", new[] { new[] { (1, 2), (5, 6) }, new[] { (1, 3) }, new[] { (4, 10) } }),
            ("Example 2", new[] { new[] { (1, 3), (6, 7) }, new[] { (2, 4) }, new[] { (2, 5), (9, 12) } }),
            ("Single employee gap", new[] { new[] { (1, 2), (5, 6) } }),
            ("Full coverage", new[] { new[] { (1, 5) }, new[] { (2, 6) } }),
            ("Touching (half-open)", new[] { new[] { (1, 3) }, new[] { (3, 5) } }),
            ("Single interval only", new[] { new[] { (5, 8) } }),
            ("Identical schedules", new[] { new[] { (1, 2), (4, 5) }, new[] { (1, 2), (4, 5) } }),
            ("Nested intervals", new[] { new[] { (1, 10) }, new[] { (3, 4), (6, 7) } }),
            ("Many small gaps", new[] { new[] { (1, 2), (3, 4), (5, 6) } }),
            ("Chained no gaps", new[] { new[] { (1, 2), (2, 3), (3, 4) } }),
            ("Large coordinates", new[] { new[] { (0, 50_000_000), (90_000_000, 100_000_000) } }),
            ("One employee inside another's gap", new[] { new[] { (1, 4), (8, 11) }, new[] { (5, 6) } }),
            ("Overlap then gap then overlap", new[] { new[] { (1, 5), (10, 12) }, new[] { (3, 7) } }),
        };

        foreach (var (name, sched) in raw)
            yield return new TestCase { Name = name, Schedule = sched, Expected = Format(Reference(sched)) };
    }

    private static IEnumerable<TestCase> RandomCases(int seed, int count)
    {
        var rng = new Random(seed);
        for (int t = 0; t < count; t++)
        {
            int employees = 1 + rng.Next(5);          // 1..5 employees
            int coordMax = 40;
            var sched = new (int, int)[employees][];

            for (int e = 0; e < employees; e++)
            {
                int blocks = 1 + rng.Next(5);          // 1..5 intervals
                var intervals = new List<(int, int)>();
                int cursor = rng.Next(4);              // random starting offset
                for (int b = 0; b < blocks; b++)
                {
                    int gap = rng.Next(4);             // 0..3 spacing (0 => touching)
                    int len = 1 + rng.Next(5);         // length 1..5, strictly positive
                    int start = cursor + gap;
                    int end = start + len;
                    if (end > coordMax) break;
                    intervals.Add((start, end));
                    cursor = end;
                }
                if (intervals.Count == 0) intervals.Add((rng.Next(coordMax), rng.Next(coordMax) + coordMax + 1));
                sched[e] = intervals.ToArray();
            }

            yield return new TestCase
            {
                Name = $"random#{t} (seed {seed})",
                Schedule = sched,
                Expected = Format(Reference(sched)),
            };
        }
    }

    // ---------- Independent reference (coverage array; small coords only) ----------
    // Marks each half-open unit cell [i, i+1) as covered, then reports maximal
    // uncovered runs strictly between the first and last covered cell.
    private static List<Interval> Reference((int, int)[][] schedule)
    {
        int maxEnd = 0;
        foreach (var emp in schedule)
            foreach (var (s, e) in emp)
                maxEnd = Math.Max(maxEnd, e);

        var covered = new bool[maxEnd + 1];
        int firstCovered = int.MaxValue, lastCovered = -1;
        foreach (var emp in schedule)
            foreach (var (s, e) in emp)
                for (int i = s; i < e; i++)
                {
                    covered[i] = true;
                    firstCovered = Math.Min(firstCovered, i);
                    lastCovered = Math.Max(lastCovered, i);
                }

        var free = new List<Interval>();
        if (lastCovered < 0) return free;

        int runStart = -1;
        for (int i = firstCovered; i <= lastCovered; i++)
        {
            if (!covered[i])
            {
                if (runStart < 0) runStart = i;
            }
            else if (runStart >= 0)
            {
                free.Add(new Interval(runStart, i));
                runStart = -1;
            }
        }
        return free;
    }

    // ---------- Input invariant check ----------
    // Per-employee: each interval is non-empty, sorted by start, and the
    // previous interval ends no later than the next one starts (non-overlapping;
    // touching is fine). No constraint across employees.
    private static string ValidateInput((int, int)[][] schedule)
    {
        for (int e = 0; e < schedule.Length; e++)
        {
            var emp = schedule[e];
            for (int i = 0; i < emp.Length; i++)
            {
                if (emp[i].Item1 >= emp[i].Item2)
                    return $"employee {e} interval [{emp[i].Item1},{emp[i].Item2}] is empty/inverted";
                if (i > 0 && emp[i - 1].Item2 > emp[i].Item1)
                    return $"employee {e} intervals [{emp[i - 1].Item1},{emp[i - 1].Item2}] and " +
                           $"[{emp[i].Item1},{emp[i].Item2}] overlap or are out of order";
            }
        }
        return null;
    }

    // ---------- Helpers ----------

    private static IList<IList<Interval>> Build((int, int)[][] schedule) =>
        schedule.Select(emp => (IList<Interval>)emp.Select(p => new Interval(p.Item1, p.Item2)).ToList()).ToList();

    private static string Format(IList<Interval> intervals)
    {
        if (intervals == null || intervals.Count == 0) return "[]";
        return "[" + string.Join(",", intervals.Select(i => $"[{i.start},{i.end}]")) + "]";
    }

    private static string FormatSchedule((int, int)[][] schedule)
    {
        var sb = new StringBuilder("[");
        for (int e = 0; e < schedule.Length; e++)
        {
            if (e > 0) sb.Append(',');
            sb.Append('[');
            for (int i = 0; i < schedule[e].Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append($"[{schedule[e][i].Item1},{schedule[e][i].Item2}]");
            }
            sb.Append(']');
        }
        sb.Append(']');
        return sb.ToString();
    }
}
