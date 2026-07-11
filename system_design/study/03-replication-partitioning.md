# 03 — Replication & Partitioning

This lesson assumes you understand Lesson 01 (latency, back-of-envelope sizing, CAP/PACELC, consistency models, replica-vs-shard) and Lesson 02 (data models, storage engines, ACID/BASE). Lessons 01 and 02 kept ending at the same cliff — *"one machine isn't enough; you must replicate and shard."* This lesson is how you actually cross that cliff: the concrete mechanisms for **keeping copies in sync (replication)** and **splitting data across machines (partitioning)**, plus the machinery that makes strong guarantees possible (**consensus / quorums**).

> **Sourcing note.** Every product/mechanism fact below is backed by a primary source checked on 2026-07-11; see the **Sources / Appendix** at the end. Inline markers like `[S1]` point there. Absolute numbers (failover seconds, default token counts, versions) are as-of that date and will drift — the *ratios and mechanisms* are what to remember.

We build it in nine parts:

1. Why replicate at all (the three reasons) — and the one hard problem it creates.
2. The three replication topologies: single-leader, multi-leader, leaderless.
3. Synchronous vs asynchronous replication — the durability/latency/consistency knob.
4. Consensus and Raft — how a group of machines agrees, and why that's the price of strong consistency.
5. Quorums — the `W + R > N` rule, worked as a proof.
6. Partitioning strategies: range, hash, consistent hashing (+ virtual nodes).
7. Rebalancing — moving data when the cluster changes size.
8. Request routing — how a request finds the shard that owns its data.
9. Putting it together — a decision guide, and how real systems combine all of this.

---

## Part 0 — Vocabulary refresh (so nothing is ambiguous)

From Lesson 01, held precisely (we resolved this confusion last session):

- **Replica.** A **full copy of the same data** on another machine. Multiple replicas can momentarily disagree — that disagreement is where *staleness* lives.
- **Shard / partition.** A **slice of *different* data** on another machine. A given key lives on one shard. Splitting data this way scales **writes and total data size**.
- **Network partition.** A *failure event*: the network splits so some nodes can't talk. (Unrelated to a data "partition/shard" despite the shared word.)

New in this lesson:

- **Leader (a.k.a. primary / master).** The one replica designated to accept **writes** for a piece of data. `[S2][S3]`
- **Follower (a.k.a. secondary / replica / standby).** A replica that receives a copy of the leader's writes and can serve **reads**. `[S2][S3]`
- **Replication log.** The ordered stream of changes the leader ships to followers so they can reproduce its state. In Postgres this is the **write-ahead log (WAL)** — the *same* WAL from Lesson 02's B-tree crash-safety, reused for replication `[S2]`; in MongoDB it's the **oplog** `[S3]`; in Kafka it's literally the partition **log** `[S5]`.
- **Replication factor (RF / N).** How many copies of each piece of data exist. `RF = 3` means three replicas per key/partition. `[S4][S5]`
- **Replication lag.** The delay between a write landing on the leader and it showing up on a follower. `[S3]`

---

## Part 1 — Why replicate, and the one hard problem it creates

**Replication = keeping multiple copies of the same data on different machines.** From Lesson 01, it buys three distinct things — keep them separate in your head, because different designs prioritize different ones:

1. **Durability / fault tolerance.** If one machine's disk dies, another copy still has the data. With replication factor `N`, you can generally lose `N − 1` copies without losing data `[S5]`. (Kafka states this directly: "With a replication factor of N, in general, we can tolerate N−1 failures, without data loss." `[S5]`)
2. **Availability.** If the machine serving you dies, another replica answers — the system stays up. (This is the **A** of CAP from Lesson 01.)
3. **Read scaling.** Reads don't change data, so you can spread them across *many* replicas and serve far more read QPS than one machine could. (Lesson 01: replication scales reads, not writes.)

```
         write "x = 5"
              │  (leader accepts the write)
        ┌─────▼─────┐        replication log         ┌───────────┐   ┌───────────┐
        │  LEADER   │ ─────────────────────────────► │ FOLLOWER  │   │ FOLLOWER  │
        │  (x = 5)  │ ─────────────────────────────► │ (x = 5)   │   │ (x = 5)   │
        └───────────┘                                └───────────┘   └───────────┘
              ▲                                            ▲               ▲
           writes go here                              reads can be served from any copy
```

**The hard problem replication creates (Lesson 01, made concrete):** the moment there is more than one copy, they can **disagree**. A write reaches the leader; until it propagates, followers still hold the old value, so a read from a follower is **stale**. Everything in Parts 2–5 is a different answer to the question: *how hard do we work to keep the copies in agreement, and what do we pay for it?* That's the CAP/PACELC trade-off from Lesson 01, now at the mechanism level.

---

## Part 2 — The three replication topologies

There are exactly three structural ways to arrange who-accepts-writes. Know all three, when each fits, and the failure story of each.

### 2a. Single-leader replication (the default)

**One replica is the leader; it alone accepts writes. All other replicas are followers that copy the leader's replication log and serve reads.** `[S2][S3]` This is what PostgreSQL streaming replication `[S2]` and MongoDB replica sets `[S3]` do by default.

```
                    ┌──────────── writes ────────────┐
   clients ─writes─►│           LEADER                │
        │           └───────┬─────────────┬───────────┘
        │            replication log   replication log
        │                   ▼             ▼
        │            ┌───────────┐  ┌───────────┐
        └──reads────►│ FOLLOWER  │  │ FOLLOWER  │◄── reads
                     └───────────┘  └───────────┘
```

- **Why it's the default:** writes have a single ordering authority (the leader), so there is **one** definitive sequence of changes — no write-write conflicts are possible, because only one machine ever decides "what happens next." This makes strong-ish consistency achievable and reasoning simple.
- **Reads scale; writes don't.** You can add followers to serve more reads, but every write still funnels through the one leader — the leader is the write ceiling (exactly Lesson 01's "replication doesn't scale writes"). To scale writes you must *shard* (Part 6), giving each shard its own leader.
- **The consistency catch — replication lag.** If followers are updated **asynchronously** (Part 3), a client that writes to the leader and then reads from a follower may **not see its own write** — the follower hasn't caught up. This is precisely Lesson 01's **read-your-own-writes** problem, and the reason MongoDB defaults reads to the primary and lets you opt into reading secondaries knowing they "may return data that does not reflect the state of the data on the primary." `[S3]`

**Failover — what happens when the leader dies.** Some follower must be promoted to leader. This needs an **election**: the followers detect the leader is gone (it stopped heartbeating) and agree on a new one. `[S3]` Concrete numbers to make it real (MongoDB defaults, as-of 2026-07-11 `[S3]`):
- A follower calls an election if it can't reach the primary for **`electionTimeoutMillis` = 10 seconds** (default).
- Median time to elect a new primary should **not exceed ~12 seconds**.
- Lowering the timeout detects failure faster but causes **spurious elections** on transient network blips.

**Failover's danger — lost writes and split brain:**
- If the old leader accepted writes that hadn't yet replicated when it died, those writes can be **lost** (rolled back) when a new leader takes over. MongoDB documents exactly this: for `w:1` writes, a failover can trigger a **rollback** of un-replicated writes. `[S3]`
- **Split brain:** two nodes both believe they're leader (e.g. during a network partition) and both accept writes → divergence. Systems prevent this by requiring a **majority** to elect/retain leadership (Part 4): a node that can't reach a majority must **step down**. MongoDB notes at most one node can complete `{w:"majority"}` writes at a time — the other, a former primary that hasn't yet realized it was demoted, will have its writes rolled back. `[S3]`

### 2b. Multi-leader replication

**More than one replica accepts writes; each leader also acts as a follower of the others, exchanging their write logs.** `[S2]` (Postgres calls the family "multimaster"; it isn't built-in for the synchronous case, but async multimaster tools exist, e.g. Bucardo. `[S2]`)

```
   writes ─►┌──────────┐  ⇄ replicate ⇄  ┌──────────┐◄─ writes
            │ LEADER A │                 │ LEADER B │
            │ (Europe) │  ⇄ replicate ⇄  │  (US)    │
            └──────────┘                 └──────────┘
```

- **Why anyone does this:** two big cases. (1) **Multi-datacenter**: put a leader in each region so writes are handled *locally* (fast — no cross-continent round trip on every write, per Lesson 01's latency hierarchy), and replicate between regions in the background. (2) **Offline-capable clients** (your phone's calendar): each device is effectively a leader that syncs when reconnected.
- **The price — write conflicts.** Because two leaders can accept a write to the *same* key concurrently, you get exactly Lesson 01's **conflicting writes**, and you must apply **conflict resolution**: last-write-wins (lossy), version vectors (detect true conflicts), CRDTs (auto-merge), or application merge. This is the whole reason multi-leader is avoided unless you truly need it — you're buying local-write latency at the cost of conflict complexity.

### 2c. Leaderless replication (Dynamo-style quorums)

**No designated leader. The client (or a coordinator node acting for it) sends every write to *several* replicas and every read to *several* replicas, and uses overlap between those sets to stay consistent.** This is the Amazon Dynamo design, implemented by Apache Cassandra `[S4]` and DynamoDB.

```
   write x=5 ──► sent to N replicas, wait for W acks
            ┌──────┐  ┌──────┐  ┌──────┐
            │ R1 5 │  │ R2 5 │  │ R3 ? │      (W=2 acked; R3 lagging)
            └──────┘  └──────┘  └──────┘
   read x  ──► ask R replicas, take the newest version returned
```

- **How it stays consistent — quorums (full treatment in Part 5).** If every write reaches `W` replicas and every read consults `R` replicas out of `N`, and you choose `W + R > N`, then the read set and write set are **guaranteed to overlap** in at least one replica — so a read always sees the latest write. `[S4]` Cassandra exposes this as tunable **consistency levels** (`ONE`, `QUORUM`, `ALL`, `LOCAL_QUORUM`, …), a per-operation dial between consistency and latency/availability `[S4]` — PACELC from Lesson 01 turned into a config knob.
- **Convergence machinery (how replicas that fell behind catch up):** `[S4]`
  - **Read repair:** when a read notices some replicas returned a stale value, it writes the newest value back to the laggards, on the read path.
  - **Hinted handoff:** if a replica is down during a write, another node stores a "hint" and replays the write to it when it recovers, on the write path.
  - **Anti-entropy repair:** a background process where replicas build **Merkle trees** (hierarchical hashes of their data) and compare them to find and fix mismatches. `[S4]`
- **Conflict resolution:** with no leader ordering writes, concurrent writes to a key must be reconciled. Cassandra uses **last-write-wins by mutation timestamp** (formally an LWW-Element-Set CRDT per CQL row) `[S4]` — simple, but as Lesson 01 warned, LWW can silently drop a write, and it depends on synchronized clocks (Cassandra requires NTP `[S4]`).

**One-line contrast to say out loud:**
> *Single-leader: one writer, easy consistency, write ceiling at the leader. Multi-leader: many writers for local/offline latency, at the cost of conflicts. Leaderless: quorum reads/writes for high availability and tunable consistency, at the cost of clock-based conflict resolution and repair machinery.*

---

## Part 3 — Synchronous vs asynchronous replication (the durability knob)

Independent of topology, there's a second choice: **when the leader accepts a write, does it wait for followers to confirm before telling the client "done"?** This is Lesson 01's sync-vs-async and latency-vs-consistency tensions, made concrete.

**Synchronous replication:** the leader waits until (some) followers have durably stored the write, *then* acknowledges the client. `[S2]`
- **Pro:** the write survives the leader dying immediately after (the follower already has it) → **no data loss on failover**, and a synchronous follower can serve **non-stale** reads. Postgres's feature matrix marks WAL streaming "with sync on" as "primary failure will never lose data." `[S2]`
- **Con:** every write pays the round-trip latency to the follower (Lesson 01: a cross-region hop is ~150 ms) and — critically — if the synchronous follower is **down or unreachable, writes block**. Postgres warns synchronous multimaster's "heavy write activity can cause excessive locking and commit delays." `[S2]`

**Asynchronous replication:** the leader acknowledges the client **immediately** and ships to followers in the background. `[S2][S3]` (MongoDB: "Secondaries replicate the primary's oplog … asynchronously." `[S3]`)
- **Pro:** low write latency, and the leader keeps serving even if followers are slow/down.
- **Con:** a window exists where an acknowledged write lives *only* on the leader — if the leader dies in that window, the write is **lost** (the rollback scenario from Part 2a `[S3]`), and followers can serve **stale** reads.

**The middle ground — semi-synchronous:** wait for **one** follower synchronously (so at least one other copy always has the write) and let the rest be async. This is the common production compromise: bounded data-loss risk without paying to wait for *every* replica. (Kafka's ISR mechanism in Part 4 is a principled version of this idea.)

> **The trade in one sentence:** synchronous buys durability + fresh reads at the cost of write latency and availability-during-follower-failure; asynchronous buys low latency + availability at the cost of a data-loss window and stale reads. There is no free lunch — this *is* PACELC's "consistency vs latency" (Lesson 01), chosen per system.

---

## Part 4 — Consensus and Raft (how a group agrees)

Single-leader replication needs a way to **elect** a leader and agree on **the order of writes** without ever ending up with two leaders or two conflicting histories — *even when messages are lost and machines crash*. That problem is called **consensus**, and it is the theoretical heart of strong consistency.

**Consensus, defined `[S1]`:** multiple servers agreeing on a value such that, once decided, the decision is **final** and every non-faulty server agrees on it. The canonical framing is a **replicated state machine**: each server holds a **log** of commands and applies them in the same order, so all servers end up in the same state — appearing to clients as a **single reliable machine** even if a minority fail. `[S1]`

**The majority rule (the single most important fact):** consensus algorithms make progress as long as a **majority** of servers is available. `[S1]` Concretely, a **cluster of 5 can tolerate 2 failures**; if more fail it stops making progress but **never returns a wrong result**. `[S1]` Why a *majority* (`⌊N/2⌋ + 1`)? Because any two majorities of the same set must **share at least one member** — so a decision approved by one majority can never be contradicted by another majority, preventing split brain. (We prove the overlap formally in Part 5; it's the same math as quorums.)

**Raft `[S1]`** is the consensus algorithm designed to be understandable (equivalent to Paxos in fault-tolerance and performance, but decomposed into clear sub-problems `[S1]`). Its two core pieces:

1. **Leader election.** Time is divided into **terms** (a monotonically increasing number). Servers are followers by default; if a follower hears nothing from a leader for a timeout, it becomes a **candidate**, increments the term, and requests votes. A candidate that collects votes from a **majority** becomes leader for that term. Because each server votes for at most one candidate per term and a majority is required, **at most one leader can be elected per term** — no split brain. `[S1]`
2. **Log replication.** The leader appends each client command to its log and replicates it to followers. Once a **majority** has stored an entry, the leader marks it **committed** and applies it; followers apply committed entries in the same order. Raft's core safety property: *if any server has applied a command as the n-th entry, no other server will ever apply a different command as the n-th entry.* `[S1]`

**Where you meet Raft/consensus in the wild (as-of 2026-07-11):**
- **etcd** (the config store behind Kubernetes) and **Consul** use Raft. `[S1]`
- **Kafka's KRaft** mode runs the cluster's metadata as a Raft log; and Kafka's *data* replication uses a closely related, majority-free variant — see below `[S5]`.
- **Google Spanner** uses **Paxos** (Raft's older cousin) to replicate each shard synchronously, and adds **TrueTime** — a globally-synchronized clock — to give **external consistency** (a guarantee even stronger than linearizability), meaning it behaves "as if all transactions run sequentially" across the whole planet. `[S6]` This is the NewSQL callback from Lesson 02: relational model + global horizontal scale, paid for with consensus round trips (higher write latency). `[S6]`

### Kafka's ISR — a different, elegant take on "agreement" `[S5]`

Kafka replicates each partition with a **leader** and **followers**, but instead of committing on a *majority* it commits on the **in-sync replica set (ISR)** — the set of followers currently caught up to the leader `[S5]`:
- Producers write to the **leader**; followers **fetch** from the leader to stay current. `[S5]`
- A record is **committed** (and made visible to consumers, marked by the **high-water mark**) only once **all replicas in the ISR** have it. `[S5]`
- If a follower falls behind by a configured time, it's **dropped from the ISR** so the leader can keep advancing — availability over waiting for a straggler. `[S5]`
- On leader failure, a new leader is chosen **only from the ISR**, guaranteeing it already has every committed record → **no committed data lost**. `[S5]`
- Each leader carries a monotonic **leader epoch** (like Raft's term) used to **reconcile/truncate** followers' logs that diverged with uncommitted records after a failover. `[S5]`

The lesson to extract: "agreement" doesn't always mean "majority vote." Kafka trades the majority rule for an **operator-tunable set of in-sync copies**, getting durability guarantees with a different availability/latency profile. Same goal (never lose a committed write, never elect a leader missing data), different mechanism.

---

## Part 5 — Quorums, worked as a proof

Leaderless systems (and, in spirit, majority consensus) rest on one inequality. Let's derive it so it's yours, not memorized.

**Setup.** Each piece of data has `N` replicas (the replication factor). Every **write** must be acknowledged by at least `W` replicas before it's considered done. Every **read** must gather responses from at least `R` replicas, and the reader takes the **newest** version among them. `[S4]`

**Claim.** If `W + R > N`, then every read is guaranteed to observe the latest completed write.

**Proof (pigeonhole).** A completed write has updated **some set of `W` replicas**. A read contacts **some set of `R` replicas**. Both sets are drawn from the same `N` replicas. If the two sets were **disjoint** (no overlap), they'd together contain `W + R` distinct replicas — but there are only `N` replicas, so that would require `W + R ≤ N`. We assumed `W + R > N`, contradicting disjointness. Therefore the write-set and read-set **must share at least one replica** — and that shared replica holds the latest write, so the read sees it. ∎

```
   N = 3 replicas.  Pick W = 2, R = 2  →  W + R = 4 > 3  ✓
        write went to  { R1, R2 }
        read  asks      { R2, R3 }
                         ▲
                 overlap at R2 → read sees the write
```

**The special case you'll quote — QUORUM.** Choosing `W = R = ⌊N/2⌋ + 1` (a **majority**) always satisfies `W + R > N`. `[S4]` For `N = 3`, that's `2` `[S4]`; Cassandra defines `QUORUM` as exactly `⌊N/2⌋ + 1` replicas `[S4]`. Using `QUORUM` for both reads and writes guarantees overlap — this is the same majority-overlap argument that makes Raft's majority safe (Part 4). `[S4]`

**Tuning the knobs (this is PACELC as a dial `[S4]`):**
- **Read-heavy, want fast reads?** Lower `R` (e.g. `R=1`) and raise `W` — reads hit fewer replicas (faster/more available) while writes do more work. `W=N, R=1` still satisfies `W+R>N`.
- **Write-heavy, want fast writes?** Lower `W`, raise `R`.
- **Want max availability, accept staleness?** Pick `W + R ≤ N` (e.g. Cassandra `ONE`) — no overlap guarantee, so reads may be stale, but any single replica can answer, so you stay up even when most replicas are unreachable. `[S4]` (This is choosing **AP** from Lesson 01.)

**Sloppy quorums** (a real-world wrinkle): during a partition, some systems let a write be accepted by *any* `W` reachable nodes — even ones that don't normally own the key — to stay available, then hand the data off to the proper replicas later (hinted handoff, Part 2c). This raises availability but weakens the overlap guarantee during the failure. `[S4]`

---

## Part 6 — Partitioning (sharding) strategies

Replication makes copies of the *same* data; **partitioning splits *different* data across machines** to scale writes and total size (Lesson 01). The core question: **given a key, which shard owns it?** Three schemes, with sharply different trade-offs.

### 6a. Range partitioning

**Assign contiguous key ranges to shards:** keys `A–H` on shard 1, `I–P` on shard 2, `Q–Z` on shard 3. (This is what a wide-column store does *within* a partition by clustering key, and what HBase/Bigtable do across shards.)

- **Pro:** **range scans are efficient** — "all keys between `T1` and `T2`" hits a small number of adjacent shards, because related keys are stored together (Lesson 02's IoT time-range query loves this).
- **Con — hotspots.** If the key is something monotonically increasing like a **timestamp**, all *new* writes land on the *same* (latest) shard → one shard is red-hot while the others idle. This is the classic range-partition failure mode; you mitigate it by prefixing the key with something higher-entropy (e.g. `deviceId + timestamp` so writes spread across devices).

### 6b. Hash partitioning

**Hash the key and use the hash to pick the shard** (e.g. `shard = hash(key) mod N`). Because a good hash scatters keys uniformly, writes and data spread **evenly** — killing the hotspot problem.

- **Pro:** even load distribution; no monotonic-key hotspot.
- **Con:** **range scans are destroyed** — adjacent keys hash to different shards, so "keys between T1 and T2" now has to ask *every* shard (a scatter-gather). You trade range-query efficiency for even distribution. (Lesson 02's key-value point-lookup access pattern is perfect for this; a range/analytics pattern is not.)

### 6c. Consistent hashing (why `mod N` isn't enough)

Naive `hash(key) mod N` has a **fatal operational flaw**: change `N` (add or remove a machine) and **almost every key remaps to a different shard**, forcing a near-total data reshuffle. `[S4]` Cassandra's docs state it plainly: "adding a single node might invalidate almost all of the mappings." `[S4]`

**Consistent hashing** fixes this. `[S4]` Picture a **ring** of hash values (say `0` to `2^{64}-1`, wrapping around). Each **node** is placed at one or more positions (**tokens**) on the ring. To find a key's owner, **hash the key to a point on the ring and walk clockwise to the next node** — that node owns it. `[S4]`

```
                 0 / 2^64
                    │
          Node C ●──┴──● Node A
                /        \
   key ─hash─► ·  ↻ walk clockwise to next node = owner
               \        /
          Node B ●──────●  ...
```

**Why this is the whole point:** when a node is **added or removed**, only the keys in the arc **between it and its neighbor** move — roughly **`1/N` of the data** — instead of nearly all of it. `[S4]` "Consistent hashing only has to move a small fraction of the keys." `[S4]` That is what makes elastic scaling (Part 7) practical.

**Virtual nodes (vnodes) — the essential refinement.** `[S4]` With one token per physical node and few nodes, the ring is **lopsided** (some arcs much bigger → uneven load), and adding a single node can only relieve *one* neighbor. `[S4]` The fix: give each physical node **many tokens** scattered around the ring (**virtual nodes**). `[S4]` Then:
- Load is **evenly distributed**, because each physical node owns many small arcs sprinkled everywhere. `[S4]`
- Adding a node pulls a little data from **many** existing nodes at once (balanced), and removing one spreads its data across many. `[S4]`
- Trade-off documented by Cassandra: more tokens per node = more even balance but a **higher probability that some node-failure combination loses availability** for part of the ring, and slower cluster-wide maintenance. `[S4]` (Cassandra 2.x used 256 random tokens/node; 3.x+ added a smarter allocator so far fewer tokens suffice. `[S4]`)

**Replication rides on top of the ring:** to place `RF` copies, walk the ring clockwise from the key and pick the next `RF` **distinct physical nodes** (skipping extra vnodes of a node you already chose), often deliberately spanning racks/datacenters for fault isolation. `[S4]` So partitioning (where a key lives) and replication (its `RF` copies) compose on the same structure.

---

## Part 7 — Rebalancing (moving data when the cluster changes)

**Rebalancing** = redistributing data when you **add** capacity (scale out) or a node **dies**. The goals: (1) end with **even** load, (2) move the **minimum** data necessary, and (3) keep serving during the move. `[S4]`

- **Why naive `mod N` fails rebalancing:** as shown, changing `N` remaps almost everything → a data tsunami. Don't hash-mod for a system that must grow. `[S4]`
- **Consistent hashing + vnodes is the standard answer:** adding a node grabs many small vnode arcs from many existing nodes → roughly `1/N` of data moves, spread out, so no single node is hammered feeding the newcomer. `[S4]` Decommissioning does the reverse, evenly. `[S4]`
- **Fixed-partition strategy (another common approach):** create *many more* partitions than nodes up front (e.g. 1000 partitions on 10 nodes = 100 each) and, to rebalance, **move whole partitions** between nodes without changing the key→partition mapping. Adding an 11th node just reassigns some existing partitions to it. (This is how systems like Elasticsearch and Riak-style setups keep the mapping stable.)
- **Operational caution (from the field `[S4]`):** Cassandra deliberately **won't auto-remove a node from the ring** on a temporary failure, to avoid needless rebalancing storms; and it guards against **multiple replicas of one range moving simultaneously**, which "can violate monotonic consistency and can even cause data loss." `[S4]` Rebalancing is powerful but dangerous — real systems throttle it and avoid concurrent range moves.

---

## Part 8 — Request routing (finding the shard)

Once data is spread across shards, a request for key `k` has to reach the shard that owns `k`. **Who knows the key→node mapping?** Three architectures (Kleppmann's taxonomy; all appear in real systems):

1. **Routing tier / coordinator in front.** Requests hit a routing layer that knows the mapping and forwards to the right node. (MongoDB's `mongos` router; many proxies.)
2. **Any node can receive any request** and forwards it to the owner. In Cassandra, the node a client contacts becomes the **coordinator**: it hashes the partition key to find the token range and routes to the replicas itself. `[S4]`
3. **Client is mapping-aware.** The client library knows the topology and talks directly to the owning node (token-aware drivers) — one fewer hop.

**How does everyone learn the mapping, and keep it current as nodes join/leave?** Two families:
- **A separate coordination service** holds the authoritative membership/mapping — often built on **consensus** (Part 4): e.g. **etcd/ZooKeeper-style** stores. (Full circle: routing metadata is itself a small strongly-consistent replicated state machine.)
- **Gossip** — nodes chatter peer-to-peer to spread membership. Cassandra uses a **gossip protocol**: every second each node exchanges cluster state (who's up, token map, schema version) with a random peer, and a **Phi-accrual failure detector** decides if peers are up/down from missed heartbeats. `[S4]` No central authority; the map converges by epidemic spread.

The point to hold: **routing is a lookup problem, and the mapping is itself replicated state** — so it's either kept in a consensus store or gossiped, the same primitives from Parts 4–5 applied one level up.

---

## Part 9 — Putting it together (decision guide)

Replication and partitioning are **orthogonal and composed**: real systems **shard for write+size, then replicate each shard for read+durability** (Lesson 01). Cassandra literally does both on one ring — partition by consistent hashing, replicate each partition `RF` times across racks/DCs. `[S4]`

Questions to voice, in order:

1. **How many copies (RF)?** Usually **3** — tolerates one failure while keeping a majority. More for higher durability/read-scaling, at storage + write cost. `[S4][S5]`
2. **Which replication topology?**
   - Default **single-leader** (simple, consistent, write ceiling at the leader) `[S2][S3]`.
   - **Multi-leader** only for multi-region local writes or offline clients (accept conflict resolution).
   - **Leaderless/quorum** for high availability + tunable per-op consistency at scale (accept LWW/clock issues + repair) `[S4]`.
3. **Sync or async replication?** Sync (or semi-sync) when losing an acknowledged write is unacceptable (money) — pay write latency; async when low latency/availability matters more and a small loss window is tolerable — accept stale reads `[S2][S3]`. This is the PACELC dial.
4. **Need strong consistency / a single agreed order?** Use **consensus** (Raft/Paxos) or **majority quorums** (`W+R>N`) — pay coordination round trips `[S1][S4][S6]`.
5. **Partitioning scheme?** **Range** if you need range scans (watch hotspots on monotonic keys); **hash** for even load (lose range scans); **consistent hashing + vnodes** whenever the cluster must **grow/shrink elastically** `[S4]`.
6. **How will it rebalance and route?** Consistent hashing/fixed-partitions to bound data movement; consensus-store or gossip to distribute the mapping `[S4]`.

The through-line from Lessons 01–02: you **derive** these from the numbers (RF and shard count from storage/QPS estimates) and the **consistency needs per data type** (strong→consensus/sync/quorum; eventual→leaderless/async) — you don't pick a topology first and justify it after.

---

## Self-check

Answer out loud, teaching-style, then check.

**1. What three distinct benefits does replication provide, and which one does it *not* help with?**
> Durability (survive disk/machine loss), availability (stay up when a node dies), and read scaling (spread reads across copies). It does **not** scale writes — every write still goes through the leader / to all replicas. `[S5]`

**2. Contrast single-leader, multi-leader, and leaderless replication in one sentence each, with the main cost of each.**
> Single-leader: one writer → easy consistency, but the leader is the write ceiling. Multi-leader: many writers for local/offline low-latency writes, at the cost of write-write conflict resolution. Leaderless: quorum reads/writes for high availability + tunable consistency, at the cost of clock-based conflict resolution (LWW) and repair machinery. `[S2][S3][S4]`

**3. Explain replication lag and how it breaks read-your-own-writes. How is it mitigated?**
> With async replication, a follower may not yet have a write the leader accepted, so a client reading a follower right after writing may not see its own write. Mitigate by reading from the leader (or a synchronous/caught-up replica), or session-sticking the client to a replica that has its writes. `[S3]`

**4. Sync vs async replication: state the trade for each, tying it to PACELC.**
> Sync: leader waits for follower(s) before acking → no data loss on failover + fresh reads, but higher write latency and blocked writes if a sync follower is down. Async: ack immediately → low latency + availability, but a data-loss window on leader failure and stale reads. This is PACELC's latency-vs-consistency choice. `[S2][S3]`

**5. Why do consensus algorithms require a *majority*, and how many failures does a 5-node cluster tolerate?**
> Any two majorities of the same set share ≥1 member, so decisions can't contradict each other (no split brain). A majority is ⌊N/2⌋+1; a 5-node cluster tolerates 2 failures and stops (never errs) beyond that. `[S1]`

**6. State and prove the quorum rule W + R > N.**
> If a write hits W replicas and a read consults R replicas out of N, and W+R>N, the two sets can't be disjoint (that would need W+R≤N), so they overlap in ≥1 replica holding the latest write → the read sees it. QUORUM = ⌊N/2⌋+1 for both always satisfies it. `[S4]`

**7. Why does naive `hash(key) mod N` fail when you add a node, and how does consistent hashing fix it? What do virtual nodes add?**
> Changing N remaps almost all keys → near-total reshuffle. Consistent hashing puts nodes on a ring and walks clockwise to the owner, so adding/removing a node moves only ~1/N of keys (the neighboring arc). Virtual nodes give each physical node many small arcs → even load and balanced data movement when nodes join/leave. `[S4]`

**8. Range vs hash partitioning: one advantage and one drawback of each, with a workload that fits.**
> Range: efficient range scans (fits time-series "device X between T1–T2"), but hotspots on monotonic keys. Hash: even load / no hotspots (fits key-value point lookups), but destroys range scans (scatter-gather). `[S4]`

**9. A system shards data and replicates each shard RF=3. Sketch how Cassandra places the 3 replicas and how a read finds them.**
> Hash the partition key onto the token ring; walk clockwise to the first 3 distinct physical nodes (skipping extra vnodes / spanning racks) = the replicas. A contacted node becomes coordinator, hashes the key, and routes to those replicas, gathering a quorum per the consistency level. `[S4]`

**10. Kafka commits a record when all replicas in the ISR have it, not on a majority. What does this buy, and how is a new leader kept safe?**
> Committing on the in-sync set (with slow followers dropped from the ISR) lets the leader keep advancing without waiting on a majority-that-includes-stragglers, tuning availability/latency. A new leader is elected only from the ISR, so it already holds every committed record → no committed data lost; the leader epoch is used to truncate followers' divergent uncommitted tails. `[S5]`

---

## Sources / Appendix

All fetched and verified **2026-07-11** (as-of dates noted where the page states one).

- **[S1] Raft / consensus** — *The Raft Consensus Algorithm*, raft.github.io (Ongaro & Ousterhout, "In Search of an Understandable Consensus Algorithm"). Consensus definition, replicated state machines, majority progress (5 servers tolerate 2 failures), leader election + log replication safety. https://raft.github.io/
- **[S2] PostgreSQL replication** — PostgreSQL 18 docs, "Comparison of Different Solutions" (WAL streaming sync/async, logical replication, multimaster, sync-on = no data loss). https://www.postgresql.org/docs/current/different-replication-solutions.html
- **[S3] MongoDB replication** — MongoDB Manual, "Replication" (replica set primary/secondaries, oplog, asynchronous replication, automatic failover, `electionTimeoutMillis`=10s default / ~12s median, `w:"majority"`, secondary-read staleness, `w:1` rollbacks). https://www.mongodb.com/docs/manual/replication/
- **[S4] Cassandra / Dynamo** — Apache Cassandra 5.0 docs, "Dynamo" (consistent hashing token ring, virtual nodes, replication factor, tunable consistency `W+R>RF`, QUORUM=⌊N/2⌋+1, LWW conflict resolution, read repair, hinted handoff, anti-entropy/Merkle trees, gossip, Phi-accrual failure detector). https://cassandra.apache.org/doc/latest/cassandra/architecture/dynamo.html
- **[S5] Kafka replication** — Confluent Developer, "Data Plane: Replication Protocol" (Jun Rao): leader/follower/ISR, RF=N tolerates N−1 failures, high-water mark, commit = all ISR, leader elected from ISR, leader epoch + log reconciliation/truncation. https://developer.confluent.io/courses/architecture/data-replication/
- **[S6] Google Spanner** — Google Cloud docs, "Spanner: TrueTime and external consistency" (last updated 2026-07-07): TrueTime global clock, Paxos-replicated shards, MVCC, external consistency (stronger than linearizability), strong reads across regions. https://docs.cloud.google.com/spanner/docs/true-time-external-consistency

---

## Changelog

- **v1.0 — 2026-07-11:** First full-depth draft of Replication & Partitioning. Written under the new sourcing directive — all product/mechanism claims cited inline `[S1]–[S6]` to primary sources verified 2026-07-11, with a Sources appendix. Covers: why replicate; single-/multi-/leaderless topologies; sync vs async; consensus + Raft + Kafka ISR; quorum proof (W+R>N); range/hash/consistent-hashing partitioning + vnodes; rebalancing; request routing; decision guide; 10-question self-check.
