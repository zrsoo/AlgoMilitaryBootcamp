using System.Text;
using SystemsProgramming.Common;

namespace SystemsProgramming.Lessons;

/// <summary>
/// Lesson 01 — Threads &amp; the execution model.
/// Makes the core mental model VISIBLE:
///   1) real OS threads exist and have distinct identities (ManagedThreadId),
///   2) their execution INTERLEAVES non-deterministically (concurrency you can see),
///   3) each thread's OWN steps stay in order (sequential within a thread),
///   4) Join() is how the main thread waits for workers to finish.
/// Pairs with study/01-threads-execution-model.md, Part 6.
/// </summary>
public sealed class Lesson01Threads : IDemo
{
    public string Name => "01-threads";
    public string Description => "Spin up threads; observe non-deterministic interleaving + Join.";

    public void Run(string[] args)
    {
        const int workers = 4;
        const int stepsPerWorker = 5;

        Console.WriteLine($"Main thread is managed thread {Environment.CurrentManagedThreadId}.");
        Console.WriteLine($"Starting {workers} workers, each printing {stepsPerWorker} steps.");
        Console.WriteLine("Watch: each worker's steps stay in order (0,1,2,...), but ACROSS");
        Console.WriteLine("workers the lines interleave differently almost every run.\n");

        // A shared, thread-safe sink so the interleaving we see reflects SCHEDULING,
        // not garbled console writes. (We'll build such coordination by hand in later lessons;
        // here we just borrow a lock so the demo shows thread interleaving cleanly.)
        var sink = new object();

        Concurrency.RunThreads(workers, workerId =>
        {
            for (int step = 0; step < stepsPerWorker; step++)
            {
                // Tiny variable pause so the scheduler is tempted to switch between workers,
                // making the interleaving obvious. Thread.Yield() offers the core to another thread.
                Thread.Yield();

                var line = new StringBuilder()
                    .Append("  W").Append(workerId)
                    .Append("-step").Append(step)
                    .Append("  (managed thread ").Append(Environment.CurrentManagedThreadId).Append(')')
                    .ToString();

                lock (sink) Console.WriteLine(line);
            }
        });

        // RunThreads Join()s all workers before returning, so this ALWAYS prints last:
        Console.WriteLine("\nall workers done (Main resumed after Join of every worker).");
        Console.WriteLine("Note: these are FOREGROUND threads (created via the Thread ctor), so the");
        Console.WriteLine("process would wait for them even without Join. Task/pool threads are");
        Console.WriteLine("background — Main returning could abandon them mid-flight. (Lesson 01, Part 4a.)");
    }
}
