/**
 * Definition of Interval:
 * public class Interval {
 *     public int start, end;
 *     public Interval(int start, int end) {
 *         this.start = start;
 *         this.end = end;
 *     }
 * }
 */

public class Solution {
    public int MinMeetingRooms(List<Interval> intervals) {
        var events = new List<(int t, int delta)>();

        foreach(var interval in intervals)
        {
            events.Add((interval.start, +1));
            events.Add((interval.end, -1));
        }

        events.Sort((a, b) => a.t != b.t ? a.t.CompareTo(b.t) : a.delta.CompareTo(b.delta));

        int meetings = 0;
        int minRooms = 0;

        foreach(var e in events)
        {
            meetings += e.delta;
            minRooms = Math.Max(minRooms, meetings);
        }

        return minRooms;
    }
}
