using System.Reflection;
using SystemsProgramming.Common;

// Entry point for the concurrency lab.
// Auto-discovers every IDemo in the assembly so new per-lesson demos are picked up
// without editing this file (add a class implementing IDemo and it appears in the list).

List<IDemo> demos = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => typeof(IDemo).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
    .Select(t => (IDemo)Activator.CreateInstance(t)!)
    .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (args.Length == 0)
{
    Console.WriteLine("Systems Programming — concurrency lab");
    Console.WriteLine("Usage: dotnet run -- <demo-name> [demo args]");
    Console.WriteLine();
    Console.WriteLine("Available demos:");
    foreach (IDemo d in demos)
        Console.WriteLine($"  {d.Name,-14} {d.Description}");
    return;
}

string name = args[0];
IDemo? demo = demos.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase))
           ?? demos.FirstOrDefault(d => d.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));

if (demo is null)
{
    Console.Error.WriteLine($"No demo matches '{name}'. Run with no args to list demos.");
    Environment.Exit(1);
    return;
}

Console.WriteLine($"=== {demo.Name}: {demo.Description} ===");
demo.Run(args.Skip(1).ToArray());
