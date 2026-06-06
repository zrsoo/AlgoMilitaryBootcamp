# LC <253> — <Meeting Rooms II>
- **Pattern:** <Intervals / sweep line — split into +1/-1 events, sort, track max concurrency>
- **Brute force:** <->
- **Optimized:** <Hold an array of events, like this: for each interval, add the start time with delta +1 (event has started), and end time with delta -1 (event has ended). Sort the array increasing, first criteria `time`, second criteria delta, so that events ending and starting at the same time contribute concurrently (reverse second criteria if they don't). Then, parse events array and hold maximum of rolling sum on deltas. This effectively answers the question: "What's the minimum number of concurrently executing intervals?", or, in our case, what's the minumum number of meetings happening at once = number of required rooms.> — O(n log n) time, O(n) space
- **Key insight:** <Events array with +1/-1 deltas>
- **Edge cases I had to handle:** <Careful on second criteria of sorting, you need to know if, for e.g. [1,4] and [4,7] are considered to be concurrently executing or not.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Intervals / sweep line + events>
- **Would I solve this in 25 min cold next week? Y/N> Y
