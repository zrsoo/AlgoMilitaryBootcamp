# 04 — Caching

This lesson assumes Lesson 01 (the **latency hierarchy** — RAM is ~100,000× faster to reach than a spinning disk, and a same-datacenter network hop is ~0.5 ms while a cross-region hop is ~150 ms — plus back-of-envelope sizing and the consistency ladder), Lesson 02 (storage engines, read-vs-write optimization, why a disk-backed database is *slow* relative to memory), and Lesson 03 (replication, sharding, consistent hashing, staleness between copies). Caching is, in one sentence, **the discipline of keeping a second, faster copy of data close to where it's used so you don't pay the full cost of computing or fetching it again** — and it inherits *every* consistency problem from Lesson 03, because a cache is just another replica that can go stale.

> **Sourcing note.** Every product/mechanism fact below is backed by a primary source checked on 2026-07-12; see the **Sources / Appendix** at the end. Inline markers like `[S1]` point there. Absolute numbers (default TTLs, counter saturation points, versions) are as-of that date and will drift — the *ratios and mechanisms* are what to remember.

We build it in ten parts:

1. Why cache at all — the latency + load argument, worked as a proof.
2. The one hard truth: a cache is a replica, so it can be stale (and "there are only two hard problems").
3. Where to cache — the layers, from browser to database.
4. Read patterns: cache-aside (lazy loading) vs read-through.
5. Write patterns: write-through, write-back, write-around.
6. Invalidation: TTL, explicit invalidation, and the write-ordering trap.
7. Eviction: LRU, LFU, TTL, random, noeviction — and how they actually work.
8. The four failure modes: stampede, penetration, avalanche, hot keys.
9. Consistency: how stale can you get, and tying it back to L01/L03.
10. Putting it together — a decision guide.

---

## Part 0 — Vocabulary refresh (so nothing is ambiguous)

- **Cache.** A **faster, smaller** store holding copies of data whose *authoritative* home (the **origin** or **source of truth** — usually your database from Lesson 02) is slower to reach. "Faster/smaller" is the whole trade: memory is far quicker than disk but costs more per byte, so a cache holds only a *subset* of the data.
- **Origin / source of truth / backing store.** The authoritative copy — the database, an object store, or an upstream service. If the cache and origin disagree, **the origin is correct by definition.**
- **Cache hit.** A read that finds its data in the cache (fast path). `[S3]`
- **Cache miss.** A read that does *not* find its data in the cache and must fall through to the origin (slow path). `[S3]`
- **Hit ratio (hit rate).** `hits / (hits + misses)`. The single most important cache health metric — Redis computes it from `keyspace_hits` and `keyspace_misses`. `[S1]` A cache with a low hit ratio can be *worse than no cache* (you pay the cache lookup **and** the origin fetch).
- **TTL (time to live).** An expiry timer on a cache entry: after `TTL` seconds the entry is treated as absent, forcing a refresh from the origin. `[S3]`
- **Eviction.** Removing entries because the cache is **full** (out of memory), as opposed to **expiry** (removing because a TTL elapsed). Different triggers, often confused — keep them separate. `[S1]`
- **Staleness.** The cache holds an *older* value than the origin now has. This is Lesson 03's replication lag, reappearing: a cache is a copy, and copies drift.
- **Warm vs cold cache.** *Warm* = populated with useful data (high hit ratio). *Cold* = empty or just-restarted (every request misses → all load hits the origin). A "cold start" is dangerous precisely because the origin gets hit by *everything* at once.

---

## Part 1 — Why cache at all (worked as a proof)

Caching buys two distinct things. Keep them separate, because a design might want one and not the other.

### 1a. Latency — serve reads faster

From Lesson 01's latency hierarchy (orders of magnitude, not exact):

| Where the data is | Rough time to read it |
|---|---|
| Local process memory (in-app cache) | ~100 ns |
| A remote in-memory cache (Redis/Memcached) over a same-DC network hop | ~0.5 ms (500,000 ns) |
| A database query hitting disk (B-tree page reads, Lesson 02) | ~5–50 ms |
| A cross-region round trip (Lesson 01) | ~150 ms |

**Worked comparison.** Suppose an uncached read is a database query that takes **20 ms**. Put a Redis cache in front of it; a cache hit is one same-DC network round trip plus an in-memory lookup ≈ **0.5 ms**. The speedup on a hit:

$$\frac{20\text{ ms}}{0.5\text{ ms}} = 40\times \text{ faster per hit.}$$

But you only get that on *hits*. The **effective average latency** depends on the hit ratio `h`:

$$T_\text{avg} = h \cdot T_\text{hit} + (1-h)\cdot T_\text{miss}.$$

A miss is actually *slightly worse* than no cache, because you check the cache **and then** query the origin **and then** write it back — AWS spells this out: a cache miss is **three trips** (ask cache → query DB → write cache). `[S3]` Let's say a miss costs `T_miss ≈ 20.5 ms` (the 20 ms DB query plus the wasted 0.5 ms cache round trip). Plug in a realistic `h = 0.95`:

$$T_\text{avg} = 0.95 \times 0.5 + 0.05 \times 20.5 = 0.475 + 1.025 = 1.5\text{ ms}.$$

So a 95%-hit cache turns a **20 ms** average into **1.5 ms** — a ~13× improvement *on average*, even though a single miss is marginally slower than not caching at all. **This is why hit ratio dominates everything:** at `h = 0.5` the same formula gives `0.5·0.5 + 0.5·20.5 = 10.5 ms` — barely half the benefit, for all the added complexity. A cache is only worth it when the access pattern is **skewed** (a small hot set accounts for most reads — the Pareto/80-20 principle Redis explicitly calls out `[S1]`).

### 1b. Load — protect the origin

The second, often *more important*, benefit: every hit is a request the **database never sees**. If your DB can serve 10,000 queries/sec and you're receiving 200,000 reads/sec, a 95%-hit cache means the DB only sees:

$$200{,}000 \times (1 - 0.95) = 10{,}000 \text{ queries/sec} \le \text{DB capacity.}$$

Without the cache, 200,000 QPS would **crush** a 10,000-QPS database. So the cache isn't just a speedup — it's the thing that makes the workload *survivable at all*. This reframes caching as a **capacity/scaling** tool, not only a latency tool, and it's why "what happens when the cache is cold or fails?" is a life-or-death design question (Part 8): lose the cache and the full 200,000 QPS lands on a database rated for 10,000.

---

## Part 2 — The one hard truth: a cache is a replica

The famous joke (Phil Karlton): *"There are only two hard things in computer science: cache invalidation and naming things."* The reason cache **invalidation** is hard is exactly Lesson 03: **a cache is a second copy of the data, and the moment there are two copies they can disagree.** Everything painful about caching descends from this.

```
        write "x = 5"  ─────────────►  ┌───────────┐
                                        │  ORIGIN   │  x = 5   ← source of truth
                                        │  (DB)     │
                                        └───────────┘
   read x ──►  ┌───────────┐
               │  CACHE    │  x = 4   ← still holds the OLD value = STALE
               └───────────┘
```

A cache entry is only *known* correct at the instant it's populated. After that, any write to the origin makes it potentially stale, and the cache has **no automatic way to know** — it's a passive copy. So the entire art is: **how do we bound staleness, and at what cost?** That's the same CAP/PACELC trade from Lesson 01 (consistency vs latency/availability), now at the caching layer. The three levers are: **short TTLs** (accept staleness up to `TTL`), **explicit invalidation** (delete/update the entry on write — precise but requires the writer to know every cached key), and **write-through** (update cache and origin together — fresh but slower writes). Parts 4–6 are these three levers in detail.

---

## Part 3 — Where to cache (the layers)

Caching isn't one thing in one place; it's a *stack* of copies at increasing distance from the user. Each layer catches some requests so deeper (slower, more expensive) layers see fewer. Think of it as a series of filters.

```
   ┌─────────┐   ┌───────────┐   ┌────────────┐   ┌───────────┐   ┌────────────┐   ┌──────────┐
   │ Browser │──►│  CDN /    │──►│  Reverse   │──►│ App-local │──►│ Distributed│──►│ Database │
   │  cache  │   │  edge     │   │  proxy     │   │ in-proc   │   │ cache      │   │ (origin) │
   │(client) │   │  cache    │   │  cache     │   │  cache    │   │(Redis/MC)  │   │          │
   └─────────┘   └───────────┘   └────────────┘   └───────────┘   └────────────┘   └──────────┘
     closest/fastest ───────────────────────────────────────────────► farthest/slowest/truth
```

1. **Client / browser cache.** The response is stored *on the user's device*, governed by HTTP headers like `Cache-Control: max-age=...` and `Expires`. `[S4]` A request served here never even leaves the user's machine — the fastest possible cache, but you control it only indirectly (via headers) and can't forcibly invalidate a copy already on someone's laptop.
2. **CDN / edge cache (Lesson 06 preview).** A globally distributed network of **Points of Presence (PoPs)** near users that cache static (and increasingly dynamic) content. This kills the cross-region latency from Lesson 01: a user in Tokyo hits a Tokyo PoP instead of your `us-east` origin. Cloudflare, by default, caches by **file extension** (images, CSS, JS, video, fonts — not HTML/JSON unless you opt in) `[S4]`, and applies **default Edge TTLs** by status code when the origin doesn't specify one — e.g. `200/206/301 → 120 min`, `302/303 → 20 min`, `404/410 → 3 min`. `[S4]` It respects origin `Cache-Control` (won't cache `private`/`no-store`/`no-cache`/`max-age=0`, or responses with `Set-Cookie`, or non-`GET` methods). `[S4]`
3. **Reverse-proxy cache.** A cache in front of your app servers (nginx, Varnish) that stores full HTTP responses so the app never runs for a repeated request. Same idea as a CDN but *you* run it, usually in your own datacenter.
4. **Application-local (in-process) cache.** A cache *inside* the app process's own memory (a hash map, Guava/Caffeine, an LRU dict). It has the **lowest *access latency* of the caches the app itself consults (~100 ns, a memory lookup with no network hop)** — note this is a different "fast" from the reverse proxy: a *reverse-proxy hit* is faster **end-to-end** because it short-circuits *before* the app runs, whereas app-local is only reached *after* the request has entered the app, but once there it's the quickest to read. The catch: it is **private to one instance** — Azure warns that with many app instances each holds its *own* copy, so they "can quickly become inconsistent" and you may need shorter TTLs or a shared cache instead. `[S2]` This is Lesson 03's multi-copy divergence in miniature: N app instances = N independent caches that drift.
5. **Distributed / remote cache (Redis, Memcached).** A dedicated in-memory store *shared* by all app instances over the network (~0.5 ms). One coherent copy for the whole fleet (no per-instance divergence), scalable independently, and the workhorse of most system-design answers. Costs a network hop and is itself a component that can fail (Part 8). Sharded across nodes via **consistent hashing** (Lesson 03) so a key lands on a predictable node.
6. **Database's own cache.** The DB keeps hot pages in its **buffer pool** (Lesson 02's B-tree pages cached in RAM). Even "hitting the database" often doesn't hit disk — but you're still paying the query round trip and the DB's CPU, which the outer caches avoid.

**The senior instinct:** cache **as close to the user as the data's volatility allows.** Static, rarely-changing data (images, JS bundles) → push all the way to the CDN/browser with long TTLs. Highly dynamic, per-user, must-be-fresh data → keep it in the distributed cache with short TTLs or don't cache it at all. The more you can push outward, the fewer requests reach your origin — but the *harder it is to invalidate* (you can't delete a file from a user's browser).

---

## Part 4 — Read patterns (how data gets *into* the cache on reads)

Two dominant patterns. The difference is **who is responsible** for loading the cache on a miss: your application, or the cache itself.

### 4a. Cache-aside (a.k.a. lazy loading) — the default

**The application manages the cache. On a read: check the cache; on a miss, the *app* fetches from the origin, writes it into the cache, and returns it.** `[S2][S3]` "Lazy" because data is loaded **only when actually requested** — nothing is cached until someone asks for it. `[S3]`

```
   read(key):
     v = cache.get(key)
     if v is not None:          # HIT (fast path)
         return v
     v = db.query(key)          # MISS → app goes to origin
     cache.set(key, v, ttl)     # app populates the cache
     return v
```

(This is AWS's lazy-loading pseudocode almost verbatim. `[S3]`)

- **Pros:** (1) **Only requested data is cached** — the cache never fills with data nobody reads. `[S3]` (2) **Resilient to cache failure**: if the cache dies and comes back empty, the app still works — every read just misses and falls through to the DB (slower, but correct). `[S3]` The cache is an *optimization*, not a dependency for correctness.
- **Cons:** (1) **The cache-miss penalty is three trips** (check cache → query DB → write cache), so the *first* request for any key is slower than no cache. `[S3]` (2) **Stale data:** since the cache is only written on a miss, if the DB changes underneath, the cache keeps serving the old value until the entry expires or is invalidated. `[S3]` You *must* pair cache-aside with a TTL and/or explicit invalidation (Part 6).
- **Cold-start weakness:** a freshly emptied cache-aside cache means *every* key misses at once → a flood of DB queries (Part 8's cold-cache stampede).

### 4b. Read-through — the cache loads itself

**The application only ever talks to the cache; the *cache library/service* fetches from the origin on a miss, transparently.** `[S2]` The app can't tell a hit from a miss — it just calls `cache.get(key)` and always gets a value.

- **Difference from cache-aside:** purely *who* contains the load logic. Cache-aside puts it in **your application code**; read-through puts it in the **cache layer** (a provided loader function). Behaviorally on a miss they're identical (fetch from origin, populate, return). Read-through centralizes the logic (less duplicated code across services) but requires a cache that supports it or a client library that wraps it. Azure notes many commercial caches provide read-through/write-through natively; cache-aside is how you *emulate* read-through when they don't. `[S2]`

**One-liner:** *cache-aside = app-managed lazy load (most common, most resilient); read-through = the cache manages its own loading (cleaner code, needs cache support). Same miss behavior.*

---

## Part 5 — Write patterns (how writes interact with the cache)

Reads populate the cache; **writes** are where consistency is won or lost. Three strategies, differing in *when* the cache and origin get updated relative to acknowledging the client.

### 5a. Write-through — write both, synchronously

**Every write updates the cache *and* the origin as one operation, before acknowledging.** `[S3]`

```
   write(key, v):
     db.write(key, v)          # write origin
     cache.set(key, v)         # AND update cache, together
     return ok
```

- **Pro:** **the cache is never stale** — it's updated on every write, so a subsequent read is guaranteed fresh. `[S3]` Great for read-after-write consistency.
- **Cons:** (1) **Every write pays two writes** (cache + DB) → higher write latency. AWS notes users tolerate this better on writes than on reads (updates *feel* like they should take work). `[S3]` (2) **Cache churn / wasted memory:** you cache *everything written*, including data that's never read again — filling the cache with cold data. `[S3]` The standard fix is to combine write-through with a **TTL** so unused written entries eventually expire. `[S3]` (3) **Empty-node problem:** a fresh/replaced cache node is missing all data until it's next *written*; reads for not-yet-rewritten keys miss. Mitigate by combining with lazy loading. `[S3]`

### 5b. Write-back (write-behind) — write cache now, origin later

**Write to the cache and acknowledge immediately; flush to the origin *asynchronously* (batched) later.** `[S2]` (Azure lists "write-behind" as a commercial-cache feature. `[S2]`)

- **Pro:** **fastest writes** (client waits only for the in-memory cache write), and writes can be **batched/coalesced** — if a key is written 100 times in a second, you flush once. Excellent for write-heavy, bursty workloads (counters, metrics, view counts).
- **Con — durability risk:** an acknowledged write lives *only in memory* until flushed. If the cache node dies in that window, the write is **lost** (unless the cache is itself replicated/persisted). This is exactly Lesson 03's **asynchronous replication data-loss window**, now between cache and DB. Do **not** use write-back for data you can't afford to lose (money) without a durable/replicated cache.

### 5c. Write-around — write origin only, skip the cache

**Writes go straight to the origin and do *not* touch the cache; the cache is populated later, lazily, on the next read (cache-aside on reads).**

- **Pro:** avoids **cache churn** from write-heavy data that's rarely read back — you don't waste cache space on writes nobody will read. Good when writes and reads are for *different* data (e.g. you write logs constantly but read them rarely).
- **Con:** a read *immediately after* a write is a guaranteed **miss** (the write bypassed the cache) → the just-written data pays the full origin latency on first read, and if an *old* value was still cached you must invalidate it (else you serve stale). Usually paired with **invalidate-on-write**.

**The composite most systems actually run:** **cache-aside reads + write-around (or write-through) + invalidate-on-write + TTL as a safety net.** The TTL is the backstop that bounds staleness even if an explicit invalidation is missed.

| Pattern | When cache updated | Write latency | Staleness risk | Durability risk |
|---|---|---|---|---|
| Write-through | on every write, sync | higher (2 writes) | none (fresh) | none |
| Write-back | in cache now, DB later | lowest | none in cache; DB lags | **yes** (memory-only window) |
| Write-around | never on write (lazy on read) | low (1 write) | until invalidated/TTL | none |

---

## Part 6 — Invalidation (the hard problem, concretely)

Invalidation = **making sure the cache doesn't keep serving a value the origin has changed.** Three mechanisms, usually combined.

### 6a. TTL expiry — bound staleness by time

Attach an expiry to every entry; after `TTL` the entry is treated as absent and the next read refreshes it. `[S3]` This is the **simplest and most robust** invalidation: even if you forget to explicitly invalidate somewhere, staleness is *bounded by the TTL*. The trade is a **freshness ⇄ load** dial:

- **Short TTL** → fresher data, but more misses → more origin load (and Azure warns a *too-short* TTL causes the app to "continually retrieve data from the data store" — you lose the cache's benefit `[S2]`).
- **Long TTL** → fewer misses / less load, but staler data (and Azure warns a *too-long* TTL lets data "become stale" `[S2]`).

You pick the TTL from the data's **tolerance for staleness**: a stock price might get 1 s, a user's display name 5 min, a list of countries 24 h. **Match the TTL to the access/change pattern** `[S2]`, and per-item TTLs are legitimate — Azure notes a single global policy "might not suit all items." `[S2]`

### 6b. Explicit invalidation — delete/update on write

On a write, actively **evict (delete) or update** the affected cache entry so the next read refreshes it. `[S2]` More precise than TTL (no staleness window at all, in principle), but it requires the writer to **know every cache key affected by the write** — which is genuinely hard when one DB row feeds many cached views (a user's profile might appear in a cached profile page, a cached friends list, a cached search result…). Missing one leaves stale data.

### 6c. The write-ordering trap (a classic bug)

**Order matters: update the database *first*, then invalidate the cache — never the reverse.** Azure spells out exactly why. `[S2]`

If you **invalidate the cache first, then update the DB**, a dangerous interleaving exists:
```
   1. Writer:  cache.delete(k)                 # cache now empty for k
   2. Reader:  cache.get(k) → miss
   3. Reader:  db.query(k) → reads the OLD value (writer hasn't updated DB yet)
   4. Reader:  cache.set(k, OLD)               # cache re-populated with STALE value
   5. Writer:  db.update(k, NEW)               # DB now NEW, cache now OLD → stale forever
```
The cache is now stale until the TTL saves you. Doing it the correct way (**DB first, then delete cache** `[S2]`) shrinks the bad window to nearly nothing: a reader can only catch a brief miss or momentary staleness *before* the delete, not a re-poisoned cache after it. (Even DB-first isn't *perfectly* race-free under pathological timing, which is why a TTL backstop and, in strict systems, techniques like versioning or write-through are used — but DB-first is the mandatory baseline.)

**The rule to memorize:** *write the source of truth first; invalidate the copy second.* Same principle as Lesson 03 — the authoritative store leads, replicas follow.

---

## Part 7 — Eviction (what to drop when the cache is full)

Eviction is triggered by **memory pressure**, not time. When the cache hits its memory limit (Redis's `maxmemory` `[S1]`), it must remove something to admit new data. The **eviction policy** decides *what*. Getting this right is what keeps the *hot set* resident and the hit ratio high.

Redis exposes these policies via `maxmemory-policy` `[S1]`:

- **`noeviction`** — evict nothing; **reject writes** with an error when full (reads still work). `[S1]` Use when losing cached data is unacceptable and you'd rather fail the write — e.g. when Redis is used as a primary store, not a pure cache.
- **`allkeys-lru`** — evict the **Least Recently Used** key. `[S1]` The default choice when a **small hot subset** dominates accesses (the Pareto case) — Redis literally recommends it as "a good default option if you have no reason to prefer any others." `[S1]`
- **`allkeys-lfu`** — evict the **Least Frequently Used** key (Redis 4.0+). `[S1]` Better than LRU when frequency matters more than recency — a key accessed a lot historically but not in the last few seconds should *survive*, which LRU would wrongly evict.
- **`allkeys-random`** — evict a random key. `[S1]` Only sensible when **all keys are equally likely** (e.g. a repeating cyclic scan where recency/frequency carry no signal). `[S1]`
- **`volatile-lru` / `volatile-lfu` / `volatile-random` / `volatile-ttl`** — the same, but **only over keys that have a TTL set**; `volatile-ttl` specifically evicts the key with the **shortest remaining TTL** first. `[S1]` These fall back to `noeviction` behavior if no key has a TTL. `[S1]` Useful when you mix cache entries (evictable) with persistent entries (no TTL, never evicted) in one instance — though Redis suggests just running two instances instead. `[S1]`

### How LRU and LFU actually work (the mechanism, since a senior probe asks)

- **Approximated LRU.** Redis does **not** track exact recency (a true LRU needs a linked list touched on every access — too much memory/CPU). Instead it **samples a few random keys** and evicts the least-recently-used *among the sample*. `[S1]` The sample size is tunable (`maxmemory-samples`, default 5); raising it to 10 gets very close to true LRU at some CPU cost. `[S1]` Under a power-law (skewed) access pattern, the approximation is nearly indistinguishable from true LRU. `[S1]` **This is a recurring systems theme: trade exactness for cheapness when the approximation is statistically good enough** (same spirit as Lesson 03's bloom filters and gossip).
- **Approximated LFU.** Redis tracks frequency with a **Morris counter** — a *probabilistic* counter using just a few bits per key that estimates a count on a logarithmic scale, plus a **decay** so old popularity fades. `[S1]` By default the counter saturates around **1,000,000 accesses** and **decays every minute**, both tunable (`lfu-log-factor`, `lfu-decay-time`). `[S1]` The decay is essential: without it, a key that was hot *last week* would never be evicted; the decay lets the policy **adapt to shifting access patterns.** `[S1]`

**LRU vs LFU in one line:** *LRU asks "who did we touch least recently?" (recency); LFU asks "who do we touch least often?" (frequency, with decay). LFU protects a consistently-popular-but-not-just-now key that LRU would evict.*

---

## Part 8 — The four failure modes (this is where senior interviews live)

A cache changes your failure surface. Four classic problems, each with a name and a mitigation.

### 8a. Cache stampede / thundering herd (a.k.a. dogpile)

**A hot key expires (or the cache is cold), and *many concurrent requests* all miss at the same instant → they *all* hit the origin simultaneously for the *same* data → the origin is hammered by a herd of identical queries.** The very key that most needed caching causes the worst spike when it expires.

```
   t=0  key "trending" TTL expires
   t=0+ 10,000 concurrent readers all cache.get("trending") → all MISS
   t=0+ all 10,000 → db.query("trending")  ← origin gets 10,000 identical queries at once 💥
```

**Mitigations:**
1. **Request collapsing / cache lock (single-flight).** When many requests miss the same key at once, let **only the first** go to the origin; the rest **wait** and receive the result the first one fetches. This is exactly what Cloudflare does — a **cache lock** so "only the first request is forwarded to the origin… the remaining requests wait… after which the response is streamed to all waiting requests," preventing the origin from "receiving excessive traffic." `[S4]` In app code this is "single-flight."
2. **TTL jitter (randomized expiry).** Don't give many keys the *same* TTL, or they all expire together (a synchronized stampede — see avalanche). Add a random spread, e.g. `TTL = 300 s ± rand(0..60)`, so expirations desynchronize.
3. **Early / probabilistic recomputation.** Refresh a hot key *slightly before* it expires (a background refresh, or probabilistically as expiry approaches), so it never actually goes cold under load.
4. **Serve-stale-while-revalidate.** On expiry, serve the *old* value to readers while a single background task refreshes it — trading a little staleness for zero stampede.

### 8b. Cache penetration — missing on data that doesn't exist

**Requests for keys that don't exist in the origin either.** Every such request misses the cache (nothing to cache) **and** misses the DB (nothing there), so the cache provides *zero* protection — an attacker (or a bug) can hammer your DB with queries for random non-existent IDs, bypassing the cache entirely.

**Mitigations:**
1. **Negative caching (cache the "not found").** Cache a **null/empty sentinel** for a missing key with a short TTL, so repeat requests for the same missing key are absorbed by the cache. (Note: cache-aside code must be careful *not* to cache a null as if it were real data — Azure's example explicitly "avoids caching a null value" for the *found* path, but negative caching deliberately caches a *marked* sentinel.) `[S2]`
2. **Bloom filter (Lesson 03 callback).** Keep a bloom filter of all keys that *do* exist; check it before hitting cache/DB. A bloom filter can say "definitely not present" cheaply, short-circuiting the query for genuinely non-existent keys.

### 8c. Cache avalanche — mass simultaneous expiry (or cache down)

**A large fraction of the cache expires (or the whole cache node dies) at once**, so a huge share of traffic suddenly falls through to the origin together — the cold-cache catastrophe from Part 1b, where a DB rated for 10,000 QPS suddenly faces 200,000.

**Mitigations:**
1. **TTL jitter** (as above) so keys don't share an expiry instant.
2. **Cache high availability** — replicate the cache (Lesson 03: leader + replicas), so a single node failure doesn't empty the whole cache. Redis notes it uses extra RAM for replication buffers; size `maxmemory` accordingly. `[S1]`
3. **Origin protection** — rate-limit / circuit-break (Lesson 09 preview) the path to the DB so a cold cache **degrades** rather than **melts** the database. Better to serve some errors than to take the DB down for everyone.
4. **Gradual warm-up** — on restart, pre-load (prime) the hot set before taking full traffic. Azure calls this "priming the cache." `[S2]`

### 8d. Hot key — one key too popular for one node

**A single key is so hot that the *one shard/node* holding it (Lesson 03's consistent hashing places each key on one node) becomes a bottleneck** — a celebrity's profile, a viral post. Sharding doesn't help because it's *one* key on *one* node.

**Mitigations:**
1. **Replicate the hot key** across multiple nodes and read from a random replica (spread the read load).
2. **Local (in-process) cache in front of the distributed cache** — cache the handful of hottest keys in each app instance's own memory (Part 3, layer 4), so most reads never even reach the remote node. Accept the per-instance staleness (short TTL) as the price.
3. **Key splitting** — store the hot value under several suffixed keys (`post:123#0..9`) and have readers pick one at random, spreading a single logical key across nodes.

---

## Part 9 — Consistency: how stale can you get?

Tie it back to Lessons 01 and 03. A cache is a replica, so caching sits on the **PACELC "else, latency vs consistency"** axis: you're trading freshness for speed/load.

- **Cache-aside + TTL** gives you **eventual consistency** bounded by the TTL: readers may see a value up to `TTL` old. This is the overwhelming default and is *fine* for the vast majority of read paths (a slightly stale like-count harms no one).
- **Write-through** gives **read-after-write freshness** for the cached key (Lesson 03's read-your-own-writes), at the cost of slower writes. `[S3]`
- **Strong consistency with a cache is expensive and rare** — you'd need to invalidate synchronously on every write *before* acknowledging, and handle every race, which usually defeats the point of caching. For data that must be strongly consistent (a bank balance), the honest answer is often **don't cache it**, or cache it only with write-through and accept the write cost, or read it from the origin under a consistent read.

**The senior framing:** decide the **staleness budget per data type** (exactly Lesson 01's "mix consistency per data type"), then pick the pattern: *tolerant of staleness → cache-aside + TTL (cheap, fast); needs freshness → write-through or don't cache; must never be wrong → origin only.* You do **not** cache everything the same way.

---

## Part 10 — Putting it together (decision guide)

Questions to voice, in order, for any "add a cache" decision:

1. **Is the workload read-heavy and access-skewed?** Caching only pays when a hot subset dominates (high achievable hit ratio, Part 1). If reads are uniform over a huge keyspace, a cache may not help — measure the expected hit ratio first. `[S1]`
2. **What's the staleness budget for this data?** (Part 9.) This sets the TTL and whether you need write-through. Static/rare-change → long TTL, push to CDN. Volatile/must-be-fresh → short TTL or write-through or no cache.
3. **Where should it live?** (Part 3.) As close to the user as volatility allows — CDN/browser for static, distributed cache for dynamic-but-shared, in-process for the few hottest keys.
4. **Read pattern?** Cache-aside (default, resilient) or read-through (cleaner, needs support). `[S2][S3]`
5. **Write pattern?** Write-around + invalidate (default), write-through (freshness-critical), or write-back (write-heavy + durable cache only). `[S2][S3]`
6. **Invalidation?** TTL as the backstop *always*; explicit invalidation for precision; **DB-first, then invalidate** ordering, no exceptions. `[S2]`
7. **Eviction policy?** `allkeys-lru` by default; `lfu` if frequency beats recency; `noeviction` if it's really a store, not a cache. `[S1]`
8. **What happens when it's cold or dies?** (Part 8 — the question that separates senior answers.) Size the origin (or its protection) for the miss storm; add request-collapsing, TTL jitter, HA replication, and negative caching so a cache problem *degrades* rather than *melts* the system.

The through-line from Lessons 01–03: you **derive** the cache design from the numbers (hit ratio from access skew, origin QPS survivability from Part 1b) and the **staleness needs per data type** — you don't bolt on "add Redis" reflexively. A cache is a replica; treat its consistency and failure modes with the same seriousness as any replica in Lesson 03.

---

## Self-check

Answer out loud, teaching-style, then check.

**1. Write the effective-average-latency formula for a cache and explain why hit ratio dominates.**
> `T_avg = h·T_hit + (1−h)·T_miss`. Because a hit is often ~40× faster than a miss but a miss is *slightly worse* than no cache (three trips: check cache → query DB → write cache `[S3]`), the benefit is almost entirely driven by how often you hit. At h=0.95 a 20 ms read averages ~1.5 ms; at h=0.5 it's ~10.5 ms. Caches only pay off on **skewed** access patterns. `[S1][S3]`

**2. "A cache is a replica." What single fact does all cache pain descend from, and what are the three levers to control it?**
> The moment there are two copies (cache + origin) they can disagree → staleness (Lesson 03). Three levers: short **TTL** (bound staleness by time), **explicit invalidation** (delete on write), **write-through** (update both together). Each trades freshness against latency/load/complexity. `[S2]`

**3. Contrast cache-aside and read-through. What's actually different?**
> Only *who holds the load-on-miss logic*: cache-aside = the **application** checks cache, fetches from DB on miss, populates cache; read-through = the **cache layer** does that transparently. Miss behavior is identical. Cache-aside is most common and resilient (works even if cache is empty/down); read-through centralizes the code but needs cache support. `[S2][S3]`

**4. Give the three write patterns and the key risk of each.**
> Write-through (update cache+DB every write): fresh but 2 writes → slower, plus cache churn (fix with TTL) `[S3]`. Write-back (cache now, DB async later): fastest but **data-loss window** if cache dies before flush (async replication risk, L03). Write-around (DB only, lazy on read): avoids churn but first read after write misses; needs invalidate-on-write.

**5. Why must you update the DB *before* invalidating the cache, not the reverse?**
> If you delete the cache first then update the DB, a reader can miss, read the **old** DB value, and re-populate the cache with stale data *before* the DB write lands — leaving the cache stale until TTL. DB-first, delete-second shrinks the bad window to a brief miss/staleness that can't re-poison the cache. Source-of-truth leads, copy follows. `[S2]`

**6. Explain how Redis approximates LRU and LFU, and why it approximates rather than computing exactly.**
> LRU: it **samples a few random keys** (default 5, tunable via `maxmemory-samples`) and evicts the least-recently-used of the sample — true LRU would need a per-access linked list (too costly). Under skewed access it's near-identical to true LRU. LFU: a **Morris counter** (few bits, log-scale, probabilistic) estimates access frequency with a **decay** so old popularity fades; saturates ~1M accesses, decays ~1/min by default. Approximates to save memory/CPU where the estimate is statistically good enough. `[S1]`

**7. A hot key's TTL expires under 10,000 concurrent readers. Name the failure and two mitigations.**
> **Cache stampede / thundering herd** — all 10,000 miss simultaneously and hit the origin for the same key. Mitigations: **request collapsing / cache lock** (only the first request goes to origin, the rest wait and share the result `[S4]`); **TTL jitter** (desynchronize expiries); early/background refresh; serve-stale-while-revalidate.

**8. What is cache penetration and how does it differ from a stampede? Mitigation?**
> Penetration = requests for keys that **don't exist anywhere** (cache miss *and* DB miss), so the cache gives zero protection and the DB is hit repeatedly (attack vector). Differs from stampede (which is about *existing* hot keys expiring). Mitigate with **negative caching** (cache a null sentinel with short TTL) and/or a **bloom filter** of existing keys to short-circuit non-existent lookups. `[S2]`

**9. Your distributed cache node holding one celebrity's profile is saturated though the cluster is fine. Name it and two fixes.**
> **Hot key** — one key on one shard/node (consistent hashing, L03) is too popular; sharding doesn't help because it's a single key. Fixes: **replicate the hot key** across nodes and read a random replica; put a small **in-process cache** in each app instance for the few hottest keys; **key-splitting** (`post:123#0..9`) to spread one logical key across nodes.

**10. When would you deliberately NOT cache something, and what do you use instead?**
> When the data must be **strongly consistent / never stale** (a bank balance) and the staleness budget is zero — caching it correctly would require synchronous invalidation on every write and race handling that defeats the benefit. Instead read from the origin under a consistent read, or use write-through and accept the write cost. Decide the **staleness budget per data type** (L01) and only cache what tolerates it. `[S3]`

---

## Sources / Appendix

All fetched and verified **2026-07-12** (page "last modified"/date noted where available).

- **[S1] Redis key eviction** — Redis OSS docs, "Key eviction" (redis.io/docs/latest/develop/reference/eviction/). `maxmemory` + `maxmemory-policy`; policies `noeviction`, `allkeys-lru`, `allkeys-lfu`, `allkeys-lrm`, `allkeys-random`, `volatile-lru/lfu/random/ttl`; approximated LRU via random sampling (`maxmemory-samples`, default 5); LFU via Morris counter (saturates ~1M, decays ~1/min; `lfu-log-factor`, `lfu-decay-time`); hit-ratio from `keyspace_hits`/`keyspace_misses`; Pareto/`allkeys-lru`-as-default guidance. https://redis.io/docs/latest/develop/reference/eviction/
- **[S2] Cache-Aside pattern** — Microsoft Azure Architecture Center, "Cache-Aside pattern" (ms.date 2025-09-11; updated 2025-12-09). Cache-aside/lazy-load mechanism; read-through & write-through/write-behind as native features cache-aside emulates; **update DB then invalidate cache** ordering with the exact race explanation; staleness-after-write vs write-through; per-item TTL/eviction guidance; local (in-process) cache divergence across instances; priming the cache. https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside
- **[S3] ElastiCache caching strategies** — AWS ElastiCache docs, "Caching strategies" (Lazy loading, Write-through, Adding TTL). Lazy-load = load-on-miss, cache-miss = **three trips**, resilient to node failure, allows stale; write-through = fresh but 2 writes, write penalty, cache churn, empty-node problem, combine with lazy loading + TTL; TTL definition and pseudocode. https://docs.aws.amazon.com/AmazonElastiCache/latest/red-ug/Strategies.html
- **[S4] Cloudflare CDN default cache behavior** — Cloudflare Cache docs, "Default cache behavior" (dateModified 2026-05-06). **Request collapsing / cache lock** (only first request to origin, rest wait — stampede mitigation); default Edge TTLs by status code (200/206/301→120m, 302/303→20m, 404/410→3m); caches by file extension (not HTML/JSON by default); respects `Cache-Control` (`private`/`no-store`/`no-cache`/`max-age=0` → no cache), `Set-Cookie` and non-GET → no cache. https://developers.cloudflare.com/cache/concepts/default-cache-behavior/

*Latency-hierarchy figures in Part 1 are orders-of-magnitude from Lesson 01 (established there), used here as ratios, not re-sourced absolute measurements.*

---

## Changelog

- **v1.0 — 2026-07-12:** First full-depth draft of Caching. Written under the sourcing directive — product/mechanism claims cited inline `[S1]–[S4]` to primary sources verified 2026-07-12, Sources appendix. Covers: why cache (latency + load, worked with the T_avg formula and origin-survivability math); cache-as-replica/staleness; the caching layers (browser→CDN→proxy→in-proc→distributed→DB); read patterns (cache-aside/lazy-load vs read-through); write patterns (write-through/back/around); invalidation (TTL, explicit, DB-first ordering trap); eviction (LRU/LFU/TTL/random/noeviction + approximated-LRU sampling & Morris-counter LFU mechanisms); four failure modes (stampede, penetration, avalanche, hot key) with mitigations; consistency/PACELC framing; decision guide; 10-question self-check.
