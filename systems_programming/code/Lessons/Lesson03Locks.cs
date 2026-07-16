using System.Diagnostics;
using SystemsProgramming.Common;

namespace SystemsProgramming.Lessons;

/// <summary>
/// Lesson 03 — Mutual exclusion &amp; locks (+ deadlock).
/// Two acts, mapped to study/03-mutual-exclusion-locks.md, Part 7:
///   1) FIX L02's race with a lock — the shared-counter workload, but the increment
///      runs inside lock(m){...}. Exact every run (mutual exclusion, Part 1).
///   2) FORCE a deadlock, then FIX it — two threads take two locks in OPPOSITE orders
///      (circular wait, Part 5a); we detect the hang with a timeout and print
///      DEADLOCK detected, then rerun with CONSISTENT lock ORDERING (Part 6a), which
///      completes cleanly. Same two locks; only the acquisition order changed.
/// </summary>
public sealed class Lesson03Locks : IDemo
{
    public string Name => "03-locks";
    public string Description => "Lock-fix L02's race, then force AND fix a deadlock.";

    public void Run(string[] args)
    {
        Act1_LockFixesTheRace();
        Console.WriteLine();
        Act2_ForceThenFixDeadlock();
    }

    // ---- Act 1: mutual exclusion makes the read-modify-write indivisible ----
    private static void Act1_LockFixesTheRace()
    {
        const int workers = 4;
        const int incrementsPerWorker = 100_000;
        const int trials = 20;
        int expected = workers * incrementsPerWorker;

        Console.WriteLine("(1) Lock-fixed counter: increment runs inside lock(m){...}.");
        Console.WriteLine($"    {workers} threads x {incrementsPerWorker:N0} increments; correct total = {expected:N0}.");

        Concurrency.SurfaceRaces("locked", trials, () =>
        {
            int count = 0;
            object m = new(); // the lock object that GUARDS `count`
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++)
                    lock (m) { count = count + 1; } // only ONE thread between acquire/release
            });
            return count == expected;
        });
        Console.WriteLine("    -> exact every run: the 3-step read-modify-write now runs ALONE.");
    }

    // ---- Act 2: opposite-order locking deadlocks; consistent order does not ----
    private static void Act2_ForceThenFixDeadlock()
    {
        Console.WriteLine("(2) Deadlock: two locks, two threads.");

        Console.WriteLine("    (a) OPPOSITE order (A-then-B vs B-then-A) — expect a hang:");
        bool finished = RunTwoLockWorkers(consistentOrder: false, timeout: TimeSpan.FromSeconds(2));
        Console.WriteLine(finished
            ? "        completed (no deadlock this run — rare; rerun to see it)."
            : "        DEADLOCK detected: threads stuck in a circular wait (watchdog fired).");

        Console.WriteLine("    (b) CONSISTENT order (both take A then B) — expect success:");
        finished = RunTwoLockWorkers(consistentOrder: true, timeout: TimeSpan.FromSeconds(2));
        Console.WriteLine(finished
            ? "        completed cleanly: a cycle cannot form, so no deadlock."
            : "        (unexpected) did not finish.");
    }

    /// <summary>
    /// Two workers each acquire lockA and lockB around a tiny critical section.
    /// If <paramref name="consistentOrder"/> is false, worker 2 takes them in the
    /// opposite order, creating the classic circular-wait deadlock. Runs the workers
    /// on BACKGROUND threads and joins with a timeout so a hang can't freeze the process.
    /// Returns true if both finished before the timeout, false if it (dead)locked.
    /// </summary>
    private static bool RunTwoLockWorkers(bool consistentOrder, TimeSpan timeout)
    {
        object lockA = new();
        object lockB = new();
        var done = new CountdownEvent(2);

        void Worker1()
        {
            lock (lockA)
            {
                Thread.Sleep(50);        // widen the window so the interleaving is reliable
                lock (lockB) { /* critical section */ }
            }
            done.Signal();
        }

        void Worker2()
        {
            // consistent: A then B (same as Worker1). inconsistent: B then A (opposite).
            object first = consistentOrder ? lockA : lockB;
            object second = consistentOrder ? lockB : lockA;
            lock (first)
            {
                Thread.Sleep(50);
                lock (second) { /* critical section */ }
            }
            done.Signal();
        }

        var t1 = new Thread(Worker1) { IsBackground = true, Name = "dl-worker-1" };
        var t2 = new Thread(Worker2) { IsBackground = true, Name = "dl-worker-2" };
        t1.Start();
        t2.Start();

        return done.Wait(timeout); // true = both signalled; false = stuck (deadlock)
    }
}
