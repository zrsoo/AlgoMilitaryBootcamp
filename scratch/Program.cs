using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal static class Program
{
    private static void Main(string[] args)
    {
        var cases = new List<TestCase>();
        cases.AddRange(HandCrafted());
        cases.AddRange(RandomCases(seed: 12345, count: 2000));

        // Optional: limit to the first N tests, e.g. `dotnet run -- 4`.
        if (args.Length > 0 && int.TryParse(args[0], out int limit) && limit > 0)
            cases = cases.Take(limit).ToList();

        var failures = new List<(TestCase tc, string detail)>();
        int passed = 0;

        for (int i = 0; i < cases.Count; i++)
        {
            var tc = cases[i];
            string original = Canonical(tc.Root);

            // The invariant: deserialize(serialize(tree)) must structurally
            // equal the original tree. The serialization FORMAT is the solver's
            // choice, so we never compare the encoded string — only the round trip.
            string detail = null;
            string roundTripped;
            try
            {
                var codec = new Codec();
                string encoded = codec.serialize(tc.Root);
                Node rebuilt = codec.deserialize(encoded);
                roundTripped = Canonical(rebuilt);
            }
            catch (Exception ex)
            {
                roundTripped = $"<exception: {ex.GetType().Name}: {ex.Message}>";
            }

            bool ok = roundTripped == original;
            if (!ok) detail = $"original={original}  roundtrip={roundTripped}";

            // Explicit output for the first 5 tests, LeetCode-style.
            if (i < 5)
            {
                Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] Test {i + 1}: {tc.Name}");
                Console.WriteLine($"        Original  = {original}");
                Console.WriteLine($"        RoundTrip = {roundTripped}");
            }

            if (ok) passed++;
            else failures.Add((tc, detail));
        }

        Console.WriteLine();
        Console.WriteLine(new string('-', 48));
        Console.WriteLine($"Total: {cases.Count}   Passed: {passed}   Failed: {failures.Count}");

        if (failures.Count > 0)
        {
            var (tc, detail) = failures[0];
            Console.WriteLine();
            Console.WriteLine("First failing test:");
            Console.WriteLine($"  Name   : {tc.Name}");
            Console.WriteLine($"  {detail}");
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
        public Node Root;
    }

    private static IEnumerable<TestCase> HandCrafted()
    {
        yield return new TestCase { Name = "Null tree", Root = null };
        yield return new TestCase { Name = "Single node", Root = new Node(1) };
        yield return new TestCase { Name = "Root + 3 leaves", Root = N(1, N(2), N(3), N(4)) };

        // LeetCode canonical example.
        yield return new TestCase
        {
            Name = "LC example",
            Root = N(1,
                N(3, N(5), N(6)),
                N(2),
                N(4)),
        };

        // Deeper, uneven.
        yield return new TestCase
        {
            Name = "Deep uneven",
            Root = N(1,
                N(2, N(5, N(9))),
                N(3),
                N(4, N(6), N(7), N(8))),
        };

        yield return new TestCase { Name = "Left spine", Root = N(1, N(2, N(3, N(4, N(5))))) };
        yield return new TestCase { Name = "Wide root", Root = N(1, Enumerable.Range(2, 20).Select(v => new Node(v)).ToArray()) };
        yield return new TestCase { Name = "Negative & zero vals", Root = N(0, N(-1), N(-2, N(-3))) };
    }

    private static IEnumerable<TestCase> RandomCases(int seed, int count)
    {
        var rng = new Random(seed);
        for (int t = 0; t < count; t++)
        {
            int budget = 1 + rng.Next(30);   // up to ~30 nodes
            int next = 0;
            Node root = BuildRandom(rng, ref budget, ref next, depth: 0, maxDepth: 5);
            yield return new TestCase { Name = $"random#{t} (seed {seed})", Root = root };
        }
    }

    private static Node BuildRandom(Random rng, ref int budget, ref int next, int depth, int maxDepth)
    {
        if (budget <= 0) return null;
        budget--;
        var node = new Node(rng.Next(-50, 51));
        int maxChildren = depth >= maxDepth ? 0 : rng.Next(0, 4);
        for (int c = 0; c < maxChildren && budget > 0; c++)
        {
            var child = BuildRandom(rng, ref budget, ref next, depth + 1, maxDepth);
            if (child != null) node.children.Add(child);
        }
        return node;
    }

    // ---------- Independent structural canonicalization ----------
    // Pre-order with explicit child-count + bracketing so two trees produce the
    // same string IFF they are structurally identical (val + ordered children).
    private static string Canonical(Node node)
    {
        if (node == null) return "#";
        var sb = new StringBuilder();
        Walk(node, sb);
        return sb.ToString();
    }

    private static void Walk(Node node, StringBuilder sb)
    {
        sb.Append(node.val);
        var kids = node.children ?? new List<Node>();
        sb.Append('[');
        for (int i = 0; i < kids.Count; i++)
        {
            if (i > 0) sb.Append(',');
            Walk(kids[i], sb);
        }
        sb.Append(']');
    }

    // ---------- Helpers ----------

    private static Node N(int val, params Node[] children) =>
        new Node(val, children.ToList());
}
