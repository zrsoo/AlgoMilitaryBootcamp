using System;
using System.Collections.Generic;
using System.Collections.Immutable;

// LC 759 — Employee Free Time
// Definition the judge provides:
public class Interval {
    public int start;
    public int end;
    public Interval() {}
    public Interval(int _start, int _end) { start = _start; end = _end; }
}

public class Solution {
    public IList<Interval> EmployeeFreeTime(IList<IList<Interval>> schedule) {
        var allSchedules = new List<Interval>();

        foreach(var emp in schedule)
        {
            allSchedules.AddRange(emp);
        }

        allSchedules.Sort((a, b) => a.start.CompareTo(b.start));

        // Merge intervals
        var current = allSchedules[0];
        var merged = new List<Interval>();

        foreach(var interval in allSchedules)
        {
            // If overlapping with current, merge
            if(current.end >= interval.start)
            {
                current.end = Math.Max(current.end, interval.end);
            }
            // If non-overlapping, add current to the list and reset
            else
            {
                merged.Add(current);
                current = interval;
            }
        }

        // Add last remaining interval
        merged.Add(current);
        var result = new List<Interval>();

        // The answer will now be all intervals at the cut-points of merged
        for(int i = 0; i < merged.Count - 1; ++i)
        {
            result.Add(new Interval(merged[i].end, merged[i + 1].start));
        }

        return result;
    }
}
