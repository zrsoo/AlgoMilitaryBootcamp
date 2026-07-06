# LC <21> — <Merge Two Sorted Lists>
- **Pattern:** <Linked list — merge with dummy head>
- **Brute force:** <Dump all node values into a list, sort it, then build a fresh list. O((m+n)log(m+n)) time, O(m+n) space.>
- **Optimized:** <Dummy head + tail pointer; walk both lists, splice the smaller node each step (`<=` keeps it stable), then attach the leftover tail with `current.next = list1 ?? list2`.> — O(m+n) time, O(1) space
- **Key insight:** <Dummy head removes the special-case for choosing the first node; since each input is already sorted, the surviving list needs no more comparisons — one `??` splices the whole remainder.>
- **Edge cases I had to handle:** <Either/both lists null (while-loop never runs, `list1 ?? list2` returns the non-null one or null); unequal lengths (leftover tail spliced in one shot).>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Dummy head + tail-pointer splice.>
- **Would I solve this in 25 min cold next week? Y/N> 
