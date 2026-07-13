using SystemsProgramming.Common;

namespace SystemsProgramming.Lessons;

/// <summary>
/// Day-0 smoke test. Proves the harness can (a) run real OS threads and (b) reliably surface a
/// data race — exactly the two capabilities the whole concurrency track is built on.
/// Not a lesson; just validation that the lab works.
/// </summary>
public sealed class Lesson00Smoke : IDemo
{
    public string Name => "00-smoke";
    public string Description => "Harness self-test: run threads + surface a lost-update race.";

    public void Run(string[] args)
    {
        Console.WriteLine("1) Hello from real OS threads:");
        Concurrency.RunThreads(4, id =>
            Console.WriteLine($"   worker {id} on managed thread {Environment.CurrentManagedThreadId}"));

        Console.WriteLine();
        Console.WriteLine("2) Surfacing a data race (unsynchronized ++ from 4 threads, 50k each):");
        const int threads = 4, perThread = 50_000, expected = threads * perThread;

        Concurrency.SurfaceRaces("unsync-counter", trials: 20, trial: () =>
        {
            int counter = 0; // shared, deliberately UNsynchronized
            Concurrency.RunThreads(threads, _ =>
            {
                for (int i = 0; i < perThread; i++)
                    counter++; // read-modify-write is NOT atomic → concurrent increments get lost
            });
            return counter == expected; // false whenever the race ate increments
        });

        Console.WriteLine($"   (each correct run should equal {expected})");
    }
}
