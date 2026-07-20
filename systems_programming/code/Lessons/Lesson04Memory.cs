using SystemsProgramming.Common;

namespace SystemsProgramming.Lessons;

/// <summary>
/// Lesson 04 — Memory model &amp; visibility.
/// Two acts, mapped to study/04-memory-model-visibility.md, Part 7:
///   1) ATOMIC COUNTER — the shared-counter workload three ways:
///        (a) plain count++            -> loses updates (the L02 race),
///        (b) Interlocked.Increment    -> exact every run (atomic, no lock),
///        (c) volatile int, still ++   -> STILL loses updates, proving volatile
///           fixes VISIBILITY, not the read-modify-write RACE (Part 3).
///   2) VISIBLE STOP-FLAG — a worker spins on a stop flag while main sets it.
///        volatile flag  -> worker sees the write and stops promptly.
///        plain flag      -> run under a watchdog timeout; whether it actually
///           hangs is JIT/hardware/build-dependent (Release-mode hoisting), so we
///           REPORT what happened this run rather than assert a guaranteed freeze.
/// </summary>
public sealed class Lesson04Memory : IDemo
{
    public string Name => "04-memory";
    public string Description => "Atomic counter (Interlocked/CAS) + a visible stop-flag (volatile).";

    public void Run(string[] args)
    {
        Act1_AtomicCounter();
        Console.WriteLine();
        Act2_VisibleStopFlag();
    }

    // ---- Act 1: atomicity (Interlocked) vs mere visibility (volatile) ----
    private static void Act1_AtomicCounter()
    {
        const int workers = 4;
        const int incrementsPerWorker = 100_000;
        const int trials = 20;
        int expected = workers * incrementsPerWorker;

        Console.WriteLine("(1) Atomic counter: same workload three ways.");
        Console.WriteLine($"    {workers} threads x {incrementsPerWorker:N0} increments; correct total = {expected:N0}.");

        // (a) plain count++ — the L02 read-modify-write race
        Concurrency.SurfaceRaces("plain ++", trials, () =>
        {
            int count = 0;
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++) count = count + 1;
            });
            return count == expected;
        });

        // (b) Interlocked.Increment — whole read-modify-write is ONE atomic step
        Concurrency.SurfaceRaces("interlocked", trials, () =>
        {
            int count = 0;
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++)
                    Interlocked.Increment(ref count);
            });
            return count == expected;
        });

        // (c) volatile field but STILL ++ — visibility fixed, race NOT fixed
        Concurrency.SurfaceRaces("volatile ++", trials, () =>
        {
            var box = new VolatileBox();
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++)
                    box.Count = box.Count + 1; // read-modify-write on a volatile is STILL 3 ops
            });
            return box.Count == expected;
        });

        Console.WriteLine("    -> Interlocked is exact; BOTH plain and volatile-`++` lose updates.");
        Console.WriteLine("       volatile fixes VISIBILITY, not the read-modify-write RACE.");
    }

    // A field wrapper so we can use the `volatile` modifier (locals can't be volatile).
    private sealed class VolatileBox
    {
        public volatile int Count;
    }

    // ---- Act 2: a finished write must be PUBLISHED to be seen ----
    private static void Act2_VisibleStopFlag()
    {
        Console.WriteLine("(2) Visible stop-flag: worker spins until main sets stop.");

        Console.WriteLine("    (a) VOLATILE flag — worker should see the write and stop promptly:");
        bool stopped = RunStopFlagWorker(useVolatile: true, timeout: TimeSpan.FromSeconds(2));
        Console.WriteLine(stopped
            ? "        worker stopped: the volatile write was published and observed."
            : "        (unexpected) worker did not stop within the timeout.");

        Console.WriteLine("    (b) NON-VOLATILE flag — may stop, or may read a stale `false` forever:");
        stopped = RunStopFlagWorker(useVolatile: false, timeout: TimeSpan.FromSeconds(2));
        Console.WriteLine(stopped
            ? "        worker stopped this run (this JIT/build made the write visible)."
            : "        watchdog fired: worker never saw the write (stale-read hang reproduced).");
        Console.WriteLine("        (Whether it hangs is JIT/hardware/build-dependent — Release-mode hoisting triggers it.)");
    }

    private sealed class StopFlags
    {
        public volatile bool VolatileStop;
        public bool PlainStop;
    }

    /// <summary>
    /// Start a background worker that loops until its stop flag is set, then set the
    /// flag from this thread. Returns true if the worker observed the flag and exited
    /// before <paramref name="timeout"/>, false if it had to be abandoned (stale read).
    /// The worker is a background thread so a hang can't keep the process alive.
    /// </summary>
    private static bool RunStopFlagWorker(bool useVolatile, TimeSpan timeout)
    {
        var flags = new StopFlags();
        var started = new ManualResetEventSlim(false);
        var exited = new ManualResetEventSlim(false);

        void Worker()
        {
            started.Set();
            if (useVolatile)
                while (!flags.VolatileStop) { /* spin */ }
            else
                while (!flags.PlainStop) { /* spin — may be hoisted */ }
            exited.Set();
        }

        var t = new Thread(Worker) { IsBackground = true, Name = "stop-flag-worker" };
        t.Start();
        started.Wait();
        Thread.Sleep(100); // let the worker settle into its spin loop

        if (useVolatile) flags.VolatileStop = true;
        else flags.PlainStop = true;

        return exited.Wait(timeout);
    }
}
