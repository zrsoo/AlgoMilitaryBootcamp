# H. Language guidance for the Databricks coding interview

## TL;DR

**Use Python.** Switch to Java only if you already write Java daily and have <5 days of Python experience. Do not switch languages now. C++ is fine if it's your daily driver; Scala is acceptable for Spark-team rounds but adds friction.

## Comparison

| Dimension | Python | Java | C++ | Scala |
|---|---|---|---|---|
| Code volume | Lowest (50–70% less than Java) | High | High | Medium |
| Standard-library coverage | Excellent (`heapq`, `bisect`, `collections`, `functools`) | Excellent (`PriorityQueue`, `TreeMap`, `Deque`, `HashMap`) | Excellent but verbose (`priority_queue`, `unordered_map`, `lower_bound`) | Decent; awkward for algo-style code |
| Speed | Slowest; rarely matters in an interview | Fast enough | Fastest | Fast enough |
| Recursion depth | ~1000 default (raise with `sys.setrecursionlimit`) | ~10000+ depending on JVM stack | Very high | Same as JVM |
| Type errors caught | Runtime | Compile-time | Compile-time | Compile-time |
| Whiteboard / shared-pad friendliness | Best | Worst (boilerplate) | Mid | Mid |
| Interviewer familiarity at Databricks | High | High | High | High (Spark-team) |
| Tradeoff cost during interview | Cleanest narrative | Time lost to ceremony | Time lost to verbosity + STL minutiae | Niche; less help available |

**Recommendation if no preference:** Python. You'll write fewer lines, talk more, and finish on time.

## Python — traps and must-know idioms

- **`heapq` is min-heap only.** For max-heap, push `-x` and negate on pop. For tuples with custom order: `heapq.heappush(h, (-priority, tie, item))`.
- **`deque` from `collections`** for O(1) popleft. **Never use `list.pop(0)`** — O(n).
- **`bisect.bisect_left` / `bisect_right`** for binary search on sorted lists.
- **`defaultdict(int)` / `defaultdict(list)`** to avoid `if k not in d` checks. Don't pass arbitrary callables you'll mutate.
- **`Counter`** for frequency maps; supports `+`, `-`, `most_common(k)`.
- **`functools.lru_cache(None)`** for top-down DP. **Args must be hashable.** Don't pass lists; convert to tuples.
- **Sort with key:** `arr.sort(key=lambda x: (x[1], -x[0]))`. For custom comparator, use `functools.cmp_to_key`.
- **Recursion depth:** `import sys; sys.setrecursionlimit(10**6)` on the first line of tree/graph problems with deep nesting.
- **Integer division:** `//` rounds toward `-inf`, not zero. For calculator-style problems where you need truncation toward zero: `int(a / b)`. (Watch float precision on huge inputs — for that use `math.trunc(a / b)` or do it via sign-and-divmod manually.)
- **Strings are immutable.** Build a list and `''.join(...)`. Do not do `s += c` in a loop.
- **Mutable default args** (`def f(a, b=[]):`) — never do this.
- **`set` membership** is O(1) average. **Use frozenset** if you need a set as a key (e.g. memoized graph state).
- **`==` on lists/dicts** is deep equality — fine but be aware of cost.
- **Tuple unpacking** inside heap items: tuples are compared lexicographically; ensure all components are comparable (no `None`, no incomparable types).

## Java — traps if you must

- **`PriorityQueue.remove(x)`** is O(n). For median-stream / interval-style problems, prefer `TreeMap<Integer, Integer>` for O(log n) deletion.
- **`HashMap` vs `TreeMap`:** `HashMap` O(1) average, `TreeMap` O(log n) ordered. `LinkedHashMap` for LRU.
- **`Deque`:** use `ArrayDeque`, **never** `Stack` (legacy synchronized).
- **Custom comparator:** `(a, b) -> Integer.compare(a, b)`. **Never `a - b`** — overflows.
- **Integer overflow:** `int` is 32-bit signed. Use `long` for sum-of-two-ints in binary search mid (`int mid = left + (right - left) / 2;`).
- **`Arrays.sort` on primitives** uses dual-pivot quicksort (no comparator allowed). `Arrays.sort(Integer[])` uses mergesort and accepts comparator.
- **Autoboxing surprises:** `Map<Integer, Integer>.get(k)` returns `null` if missing — use `getOrDefault(k, 0)`.
- **String concatenation in loops:** use `StringBuilder`.
- **Recursion:** JVM default stack is ~512 KB; ~10k–30k recursion depth is usually fine. If risk, convert to iterative.
- **`List<int[]>`** is more efficient than `List<Integer[]>` for graph adjacencies; in interviews use either.

## C++ — traps if you must

- **`unordered_map<int,int>`** is fast in average but has TLE risks on adversarial inputs (LC sometimes); use `gp_hash_table` from `__gnu_pbds` or fall back to `map`.
- **`priority_queue<int>` is max-heap.** For min-heap: `priority_queue<int, vector<int>, greater<int>>`.
- **`lower_bound` / `upper_bound`** on sorted `vector`. On `set`/`map`, use the member function `s.lower_bound(x)`, not `std::lower_bound(s.begin(), s.end(), x)` (latter is O(n) for these containers).
- **Integer overflow:** `int` × `int` can overflow even if the operands fit. Cast: `(long long) a * b`.
- **`vector<bool>`** is special — bit-packed, not a real `bool[]`. Use `vector<char>` if you need pointers / refs.
- **Comparator must be strict weak ordering.** `a < b` works; `a <= b` is **undefined behavior** as a `std::sort` comparator.
- **References to vector elements** become invalid after `push_back` if reallocation happens. Reserve first if you hold refs.
- **`std::stack`, `std::queue`** are container adapters — no iteration. Use `std::deque` if you need both.

## Scala — only for Spark-team rounds

- Tuple syntax `(a, b)` is fine; pattern matching is great for tree/graph problems.
- `mutable.PriorityQueue` is max-heap; reverse for min: `PriorityQueue.empty[Int](Ordering[Int].reverse)`.
- Avoid `var` mutability inside higher-order combinators; interviewers will judge it.
- `Map` is immutable by default; for performance use `mutable.HashMap`.
- **Don't go fully functional** in an interview — recursion + immutability + tail-call-not-guaranteed can blow stacks. Use `@tailrec` annotation or convert to iterative.

## Universal traps

- **Off-by-one in binary search:** decide upfront whether you're searching on a closed or half-open range and stick to it.
- **Integer overflow in `mid = (lo + hi) / 2`:** use `lo + (hi - lo) / 2` in Java/C++.
- **Recursion depth in tree problems** with skewed input; in Python set the limit explicitly.
- **Hash collisions** with adversarial inputs; mention it once if relevant ("worst case O(n) per op").
- **Comparator must be transitive.** "Sort tasks so equal-priority comes first" is a classic bug source.

## Final advice

Pick your language **today**. Write a 1-page personal cheat sheet of the idioms you forget the most (in your language). Re-read it daily during block 1.
