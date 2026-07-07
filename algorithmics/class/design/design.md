# Class: Design (O(1) data-structure design)

> Canonical class for the `design` topic — the **tool library**. Living document; fold new templates/pitfalls back in as problems reveal them.
> Design problems are special: **the data structure *is* the answer**, so the "tool" can only be the reusable *primitives* (here: a hashmap + an intrusive doubly-linked list, and bucketing). Assembling them into a specific cache/policy is the cold solve — that wiring is deliberately **not** spelled out here.

---

## 1. Context & when to reach for it

These are "**design a class with these operations, all in O(1)**" problems: `get`/`put` with an eviction policy, an iterator with O(1) ops, a structure that must stay ordered by recency or frequency. The interview tests whether you can **compose standard primitives** so that *every* required operation is O(1) — not whether you know a clever algorithm.

**Signals:**
- "Implement a cache / data structure supporting `get` and `put` in **O(1)**."
- An **eviction / ordering policy**: least-recently-used, least-frequently-used, most-recent, etc.
- "All operations average O(1)" on a structure that naively would need a scan or a sort.

> Rule of thumb: O(1) lookup ⇒ a **hashmap**. O(1) "move/remove/insert at a known position" and maintained **order** ⇒ a **doubly-linked list** with sentinels. Almost every O(1) cache is *hashmap (key → node) + one or more doubly-linked lists*. The policy decides how many lists and how nodes move between them.

---

## 2. Mental model

Two primitives, each covering the other's weakness:
- A **hashmap** gives O(1) *find by key* but has no notion of order.
- A **doubly-linked list** gives O(1) *insert / remove / move* **at a node you already hold**, and maintains an explicit order — but finding a node by key would be O(n).

Combine them: the hashmap maps `key → the list node`, so you locate a node in O(1), then splice it in the list in O(1). That pairing is the backbone of every O(1) ordered/eviction structure.

The **policy** is just: *which* list a node lives in, and *when* it moves. Recency policy → one list, move-to-front on touch, evict the back. Frequency policy → one list **per frequency** (buckets), move a node to the next bucket when its count rises, evict from the lowest non-empty bucket.

---

## 3. Core primitives

### A. Intrusive doubly-linked list with sentinels
Nodes carry their own `prev`/`next`. Two **sentinel** nodes (`head`, `tail`) remove all the null-checking around the ends: the real list lives strictly *between* them, so every insert/remove sees real neighbours on both sides.

### B. Hashmap → node
`Dictionary<TKey, Node>` so any key's node is reachable in O(1) for a subsequent splice.

### C. Bucketing by a secondary key
When the policy orders by a *count* (frequency), keep a `Dictionary<int, DLL>` of `count → list of nodes with that count`, plus a `minCount` cursor so eviction finds the lowest non-empty bucket in O(1). Within a bucket, recency order breaks ties.

---

## 4. Reusable templates (C#)

### A. Sentinel doubly-linked list — the O(1) splice operations
```csharp
class Node {
    public int key, val;
    public Node prev, next;
    public Node(int k = 0, int v = 0) { key = k; val = v; }
}

// Sentinels: head <-> ... <-> tail. Real nodes live strictly between them.
Node head = new Node(), tail = new Node();
void Init() { head.next = tail; tail.prev = head; }

void Remove(Node x) {                 // unlink a node you already hold — O(1)
    x.prev.next = x.next;
    x.next.prev = x.prev;
}
void AddFront(Node x) {               // insert right after head (most-recent end) — O(1)
    x.next = head.next;  x.prev = head;
    head.next.prev = x;  head.next = x;
}
void MoveToFront(Node x) { Remove(x); AddFront(x); }   // "just touched" — O(1)
Node Back() => tail.prev;             // the eviction candidate at the LRU end
```

**Picture it.** The two sentinels are bumpers; the live nodes sit between them. Because every real node always has a non-null `prev` and `next` (worst case a sentinel), remove/insert never special-cases the ends:

```
 head <-> A <-> B <-> C <-> tail        (front = most recent, back = oldest)

MoveToFront(C):  Remove(C)            head <-> A <-> B <-> tail
                 AddFront(C)          head <-> C <-> A <-> B <-> tail

evict:  x = Back() (= B here)  ->  Remove(B), drop map[B.key]
```

**What it does:** gives O(1) **remove**, **add-to-front**, and **move-to-front** at a node you already have a handle to. The sentinels are the trick that makes the code branch-free — there's no "is this the first/last node?" check because the head/tail always sit beyond the real ends. Paired with a `Dictionary<int, Node>` (key → node), you get O(1) *find* **and** O(1) *reorder*: look the node up in the map, then splice it here. "Front" vs "back" is just a convention you pick for which end means "freshest."

### B. Hashmap → node (the lookup half)
```csharp
Dictionary<int, Node> map = new();    // key -> its list node

// get(key):  if (!map.TryGetValue(key, out var n)) return -1;
//            <touch n per the policy>;  return n.val;
// put(key):  existing -> update n.val + touch;  new -> make node, AddFront, map[key]=n,
//            and if over capacity: evict = Back(), Remove(evict), map.Remove(evict.key).
```

**What it does:** the map is the O(1) *find-by-key* half; the list (template A) is the O(1) *order/evict* half. Note the node stores its own `key` (not just `val`) — on eviction you have the node but need its key to also remove it from the map. That back-reference is easy to forget and is what makes eviction fully O(1).

### C. Frequency buckets (the count-ordered primitive)
```csharp
// One doubly-linked list PER frequency; nodes move up a bucket when touched.
Dictionary<int, LinkedList<Node>> buckets = new();   // freq -> nodes at that freq
Dictionary<int, (Node node, int freq)> map = new();  // key -> node + its current freq
int minFreq = 0;

// Touch(key): f = current freq; remove node from buckets[f];
//   if buckets[f] now empty AND f == minFreq -> minFreq++;
//   add node to buckets[f+1]; record freq f+1.
// Evict: drop from buckets[minFreq]'s LRU end (its oldest node), remove from map.
// On INSERT of a brand-new key: its freq is 1, so set minFreq = 1.
```

**Picture it.** Imagine shelves labelled by use-count. A new item goes on shelf 1. Each time you touch an item it hops up one shelf. To evict, you take the oldest item from the **lowest occupied shelf** — and `minFreq` remembers which shelf that is:

```
 freq 1:  X <-> Y         (Y oldest at freq 1)
 freq 2:  Z
 freq 3:  W               minFreq = 1

touch X  ->  X hops to freq 2:
 freq 1:  Y
 freq 2:  X <-> Z
 freq 3:  W               minFreq still 1 (freq-1 not empty)

evict now -> take oldest of freq 1 (= Y).   When a touch empties the
minFreq bucket, minFreq increases by 1 (a freshly-touched node sits one above).
```

**What it does:** keeps items ordered by **frequency**, with recency breaking ties inside a bucket, all O(1). The `minFreq` cursor is what makes eviction O(1) — without it you'd scan for the lowest non-empty bucket. The two `minFreq` rules to reason out (left as the cold solve to wire up): it resets to `1` whenever a brand-new key is inserted, and it increments only when a touch empties the current-minimum bucket. Each bucket is itself a recency list (template A's idea), so "evict the least-frequent, breaking ties by least-recent" falls out.

---

## 5. Common pitfalls

- **Node doesn't store its key** — on eviction you hold the node but must also delete the map entry; without `node.key` you can't, and eviction degrades. Always store the key in the node.
- **Forgetting sentinels** — hand-rolling head/tail as `null` spawns a swarm of edge cases (empty list, single node, removing the last). Two sentinel nodes kill all of them.
- **Updating the map but not the list (or vice versa)** — every operation must keep `map` and the list(s) in lockstep. A node removed from a list but left in the map (or the reverse) is the classic dangling-state bug.
- **`minFreq` mismanaged (frequency policy)** — reset to 1 on every brand-new insert; bump it only when the current min bucket empties after a touch. Getting this wrong evicts the wrong element.
- **Capacity 0 / first insert** — guard zero-capacity caches, and remember the very first `put` initialises ordering state.
- **Updating an existing key's value via the new-key path** — an existing key's `put` should update value + reorder, **not** insert a duplicate node.

---

## 6. Complexity cheat sheet

| Primitive | Find | Insert / Remove / Reorder | Evict |
|---|---|---|---|
| Hashmap → node | O(1) | — | — |
| Sentinel doubly-linked list | O(n) by value (don't) | O(1) at a held node | O(1) at the end |
| Hashmap + DLL (recency) | O(1) | O(1) | O(1) |
| Hashmap + freq buckets | O(1) | O(1) | O(1) via `minFreq` |

---

## 7. Map to the problem set

> **Recognition guard:** this maps primitives → problems. Reading the row for a problem you haven't attempted hands you the design — the thing you're meant to derive cold. Use it only during review / spaced reps.

| Primitive from above | Problem-set # (LC) |
|---|---|
| Hashmap + recency DLL | #118 (LC 146 LRU Cache) |
| Hashmap + frequency buckets | #119 (LC 460 LFU Cache) |
| Hashmap + ordering / snapshot variants | #120 (Snapshot), #124 (LC 981 Time-Based KV), #121 (LC 380 Insert-Delete-GetRandom) |
