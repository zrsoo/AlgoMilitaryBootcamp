namespace SystemsProgramming.Common;

/// <summary>
/// A runnable, self-contained concurrency demo for one lesson/topic.
/// Implementations are auto-discovered by <c>Program.cs</c> via reflection,
/// so a new demo just needs to implement this interface.
/// </summary>
public interface IDemo
{
    /// <summary>Stable id used to select the demo from the CLI, e.g. "00-smoke".</summary>
    string Name { get; }

    /// <summary>One-line description shown in the demo list.</summary>
    string Description { get; }

    /// <summary>Run the demo. Any CLI args after the demo name are passed through.</summary>
    void Run(string[] args);
}
