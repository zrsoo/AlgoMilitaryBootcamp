# Grades — Lesson 03: Replication & Partitioning

Quiz date: 2026-07-12. Format: 10 questions, integrative with L01–L02. Scale: Strong / Partial / Weak.

---

## Q1 — Size storage (RF=3) + pick topology under read skew
**Asked:** 500M profiles @2KB, R:W 50:1, 200K reads/sec peak, RF=3. (a) raw storage; (b) topology + what it can't scale.
**Answer:** (a) 500M×2KB = 1 TB primary; ×3 = 3 TB raw. (b) writes = 200K/50 = 4K/sec; single-leader to scale reads via followers; can't scale writes but 4K/sec fine on one leader.
**Grade:** Strong
**Gaps:** Minor — (1) 3 TB is data-only; didn't mention index/headroom overhead. (2) Didn't note RF=3 caps read fan-out at 3 nodes; scaling reads may need read-only followers *beyond* RF (durability RF vs read-replica count can decouple).

## Q2 — Read-your-own-writes failure on single-leader + mitigations
**Asked:** User edits profile, re-reads from follower, sees stale (old) value. (a) name failure + mechanism (tie to sync/async); (b) two mitigations that KEEP reads on followers + trade-off each.
**Answer:** (a) Correctly explained mechanism (async, leader ACKs, follower lag), named RYOW in the generalization. (b) Gave full-sync replication and leaderless read-repair; RYOW generalization = read from leader or caught-up replica.
**Grade:** Partial
**Gaps:** (a) Strong. (b) The two mitigations didn't match the "stay on followers" constraint — sync just makes followers fresh, leaderless is a topology change. Missed the targeted ones: (1) session-sticky read-your-writes routing (recent writers → leader, others → followers); (2) LSN/timestamp-gated follower reads. Trade-offs under-stated across the board (recurring "why it matters" gap): sync = write latency + blocks if sync follower down; read-repair = clock conflicts + repair cost.

## Q3 — L02 refresher: storage engine per table (LSM vs B-tree)
**Asked:** (a) write-heavy append-only audit log; (b) read-heavy point-lookup profile table. Engine + mechanical why. Watch model-vs-engine axis.
**Answer:** (a) LSM — memtable buffers writes, sequential append to SSTable vs B-tree random writes; recent reads hit memtable. (b) B-tree — single index traversal (3-5 reads) to one location; LSM read-amplifies across SSTables.
**Grade:** Strong
**Gaps:** None material. Kept model-vs-engine axis clean (no regression from L02-Q6 slip).

## Q4 — Write ceiling → shard; replication+partitioning compose
**Asked:** 80K writes/sec exceeds one leader. (a) why sharding fixes it and more followers don't (what each scales); (b) how replication+partitioning compose at RF=3.
**Answer:** (a) followers scale reads only; sharding splits data across multiple leaders (each single-leader for its slice) → parallel writes. Explicitly distinguished from multi-leader replication (same data, multiple writers). (b) each shard = 1 leader + 2 followers; write→shard's leader by key→replicate to that shard's followers; reads→right shard then follower.
**Grade:** Strong
**Gaps:** None conceptual — actively defended replica-vs-shard distinction (recurring flag, no regression). Minor: didn't derive shard count from the number (80K/3≈27K per leader; 9 nodes total); a range typo (K-Z / overlap).

## Q5 — Partitioning scheme vs a date-range query
**Asked:** (a) hash(user_id) shard → what happens to date-range query; (b) range-by-timestamp → new write failure mode; (c) key design keeping writes spread + range query reasonable.
**Answer:** (a) Strong — hash scatters → date range = all-shard scatter-gather. (b) Strong — monotonic timestamp → recent writes hit one shard = hotspot. (c) Prefixed user_id+timestamp to spread writes; honestly flagged it reverts range query to scatter-gather; asked why wide-column has partition key + sort key when range partitioning seemed to have one.
**Grade:** Partial
**Gaps:** (c) Missed bounded bucketing/salting (partition key = hash%K + coarse time; sort key = timestamp) → spreads writes across K AND bounds range fan-out to K shards. Taught the wide-column two-key model: partition key = distribution (which node, hashed), clustering/sort key = order within partition (range scans). Insightful question — the confusion is the right one.

## Q6 — 5-node Raft consensus fault tolerance + 5→6 trap
**Asked:** (a) how many failures a 5-node Raft cluster tolerates for writes + why majority is safe threshold; (b) does 5→6 improve write fault tolerance (numbers)?
**Answer:** (a) 2 failures; majority ⌊5/2⌋+1=3, 2 live can't form majority. Reasoned safety via W+R>N. Half-remembered a partition case as "2 live nodes keep running." (b) Correct — 6 nodes majority=4, tolerates 2, same as 5.
**Grade:** Strong
**Gaps:** Corrected the inverted memory: in a 3–2 partition the MAJORITY (3) side continues, the MINORITY (2) steps down — never "2 nodes run alone." Cleaner "why majority" = two majorities overlap in ≥1 node (not W+R phrasing). "Reads still served" only for stale/non-linearizable reads. (b) bonus: even clusters risk 3–3 split → odd node counts preferred.

## Q7 — Quorum tuning at N=5
**Asked:** (a) smallest-R (W,R) guaranteeing latest-write read + prove overlap; (b) why it fits read-heavy/read-latency-sensitive (what small R buys / large W costs); (c) team copies W=2,R=2 from N=3 to N=5 — what's lost + arithmetic.
**Answer:** (a) R=1,W=5, 6>5 ✓, correct reasoning. (b) deferred to prior answer. (c) Lose linearizability; 2+2=4<5, worked disjoint example {R1,R2} write vs {R3,R4} read = stale.
**Grade:** Strong
**Gaps:** (b) deferred AGAIN (recurring "articulate the consequence" flag) — had to supply: small R=1 = lowest read latency + max read availability; W=5 = slowest-of-5 tail + zero write fault tolerance (one replica down blocks writes); safer point W=4,R=2. (c) add: quorum settings don't transplant across N — re-derive W+R>N per new N (need W=3,R=3 at N=5).

## Q8 — Consistent hashing 10→11 nodes + vnodes
**Asked:** (a) fraction moved under mod N + why; (b) fraction under ring + which keys; (c) single-token load-balance problem on add + how vnodes fix.
**Answer:** (a) Strong — ~all move, mod N changes for nearly every key. (b) ~1/N; keys = "first half of the cut arc(s)". (c) Named general uneven-arc problem, said vnodes make arcs more balanced ("larger arcs get more random vnodes").
**Grade:** Partial
**Gaps:** (b) Missed that only ONE existing node donates to the newcomer (single token splits one arc); "first half" imprecise. (c) Missed the specific subtlety: single token → newcomer relieves only ONE neighbor (uneven + that donor hammered); vnodes → newcomer steals a little from MANY donors (balanced relief + spread transfer). Mechanism correction: vnodes balance by law-of-large-numbers (many small arcs sum to ~equal), not "big arcs attract more vnodes".

## Q9 — Multi-region topology + conflict resolution (L01 latency integ.)
**Asked:** (a) cross-continent RTT + why it rules out single global leader; (b) which topology + the new problem it forces; (c) two conflict-resolution strategies + which silently loses data + why.
**Answer:** (a) Strong — ~150ms RTT, single leader forces far continent to pay it. (b) Strong — multi-leader per continent, fast local writes, but W-W conflicts arise; single-leader immune via one ordering authority. (c) LWW (loses data, discards other value, good example a→0 vs a→1), version vectors (detect vs resolve), CRDTs (auto-merge counter). Correctly flagged Merkle trees as anti-entropy not conflict-res.
**Grade:** Strong
**Gaps:** None material. Reinforced: LWW danger also = clock-skew across regions (NTP dependence) + invisible loss. Version vectors detect only, need app merge for true conflicts.

## Q10 — Capstone: payments ledger full-stack decisions (L01→L02→L03)
**Asked:** strong consistency, zero acked-write loss (survive any single node failure), moderate writes, single region. (a) topology + why other two wrong; (b) sync vs async + durability reason; (c) mechanism for single order/no split brain + failure count @3,5 nodes; (d) 1 sentence why LWW categorically unacceptable.
**Answer:** (a) Single-leader; caught single-leader loses acked write only under async → needs sync; floated leaderless W=N+sync, rejected as brittle. (b) Sync (said "covered"). (c) Described quorum overlap + majority-on-N for no split brain; failure counts 3→1, 5→2 correct; wrote "R+W<N" (typo, meant >N). (d) Focused on clock-skew → wrong write kept/stale value.
**Grade:** Strong (d Partial)
**Gaps:** (a) add: leaderless also wrong because ledger needs multi-key ACID transactions / single serialized order, not just per-key consistency. (b) precise form = SEMI-sync (≥1 follower before ack → write on ≥2 nodes → survives 1 failure) not full-sync-to-all. (c) conflated consensus (Raft majority-commit, the single-leader mechanism) with quorum vocabulary; typo R+W<N should be >N. (d) MISSED categorical reason: LWW discards writes by design, but every ledger write is a distinct real money movement that must ALL persist — dropping any acked write violates zero-loss; clock skew is only an aggravator. Also: single-leader+consensus has NO w-w conflicts so LWW never arises.

---

## Lesson 03 summary — OVERALL: STRONG
- Score: 7 Strong, 3 Partial (Q1 S, Q2 P, Q3 S, Q4 S, Q5 P, Q6 S, Q7 S, Q8 P, Q9 S, Q10 S/d-Partial).
- **Strengths:** mechanism recall excellent; replica-vs-shard distinction actively defended (no regression); reasoned rather than pattern-matched on capstone; asked several genuinely senior questions (geo-sharding vs multi-leader, Instagram, unbounded partition growth, composite-key hashing).
- **Recurring gaps to drill:** (1) UNDER-ARTICULATES the "why it matters"/consequence half — deferred (b) parts twice (Q2, Q7); push to state trade-offs out loud. (2) Consistent-hashing donation subtlety (single token relieves ONE neighbor; vnodes spread donation across many) — Q8 Partial. (3) Bounded bucketing/salting for range+spread — Q5 Partial. (4) LWW's categorical (not clock) reason — Q10d.
- **Next reinforcement:** when quizzing L04+, occasionally re-ask a trade-off "so what's the consequence?" follow-up to force articulation.
