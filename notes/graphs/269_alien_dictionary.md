# LC <269> — <Alien Dictionary>
- **Pattern:** <Graphs — build a letter graph from adjacent word pairs (first differing char), then topological sort (Kahn's BFS) with cycle + prefix-trap detection>
- **Brute force:** <Enumerate permutations of the distinct letters and keep one consistent with every adjacent-word ordering — factorial>
- **Optimized:** <The words are already sorted lexicographically in the input, by first diffing letter. That tells us exactly what we need to know for getting the letters in lexi order: how do letters precede each other? We can parse the words in adjacent consecutive pairs, and define "dependencies". For two adjacent consecutive words, the first different letter of word1 precedes (points to) the first different letter of word2. Build an adjacency list of this form, and then use topological sorting to get the lexi order of the letters.>
- **Key insight:** <Topological sorting allows us to sort any entity set where we can define dependencies (or order of precedence) between the entities.>
- **Edge cases I had to handle:** <The input can contain invalid words / letters / orderings, such as: word2 is a prefix of word1, invalid because if there is no diffing letter, the words are compared by length, so there can't be a shorter word after a longer word if all their letters are the same; in the end, there can also be cycles in the dependency graph, which means that an order between the letters can't be derived, since they preceed each other in in a cycle. In both cases, return "".>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Kahn's Topological Sort>
- **Hint level needed (L0 class / L1 category / L2 structural / L3 editorial):** <None>
- **Would I solve this in 25 min cold next week? Y/N> Dunno, I was kinda spoon fed the solution, read the full editorial before solving.
