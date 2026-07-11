# Lesson 02 — Databases — quiz grades

Teach-back quiz record. Grades: **Strong** (solid, minor polish) / **Partial** (right idea, real gap) / **Weak** (needs re-study).
Use the Gaps column to target reinforcement.

---

## Q1 — The two independent axes of a database
**Question:** Name both axes, what each governs, and why keeping them separate matters (give a claim that confuses the layers).

**Answer:** Data model = how you think about/query the data (relational/KV/document/wide-column). Storage engine = what's under the hood (B-tree read-opt vs LSM write-opt). Keeping separate matters: read/write optimization is an engine choice, distinct from the model. Confused claim: "SQL/relational → read-optimized, NoSQL → write-optimized" — false because SQL-on-LSM (MyRocks) and NoSQL-on-B-tree (MongoDB/WiredTiger) both exist.

**Grade: Strong.**
- Both axes correct; both-directions counterexample nailed.
- Polish: sharpen "governs" — model governs the *interface* (shape of questions you can ask); engine governs the *physical byte layout on disk* (which drives read vs write perf).

**Gaps:** none material.

---

## Q2 — Why LSM writes are faster than B-tree writes (mechanism)
**Question:** Walk the path a single write takes in each engine; explain why LSM writes are faster, invoking the Lesson-01 fact.

**Answer:** B-tree: walk index tree to leaf (3–5 reads) → WAL append → in-place write → occasional page split. LSM: write to in-RAM memtable (1000× faster than SSD) + WAL; when memtable full (~64KB), sequential flush to SSTable. Concluded LSM ~2 writes, one in RAM → faster.

**Grade: Strong.**
- Both paths traced correctly; RAM-speed fact invoked; "sequential append" mentioned.
- Corrections: (1) memtable is ~64 **MB** not KB. (2) SSTable flush is **amortized** (once per ~thousands of writes), not per-write — per-write path is just WAL append + memtable insert.
- Reframe: the win is **kind** of write (random seek vs sequential/RAM), not the **count**. Must say the **sequential-crushes-random-seek** half of the Lesson-01 fact out loud, not just the RAM half.

**Gaps:** emphasize B-tree in-place = **random seek**; state amortized flush; both halves of the L01 fact (sequential>random AND RAM>disk).

---

## Q3 — Why LSM reads can be slower; the two rescue mechanisms
**Question:** Why can LSM reads be slower than B-tree reads, and what two mechanisms fix it (be precise about one)?

**Answer:** B-tree read = 3–5 page walk to the row. LSM = a key can live across many SSTables; check newest→oldest, stop at first (most recent = authoritative) → touching many SSTables is slow. Fixes: (1) Bloom filters — tiny in-RAM probabilistic structure saying "DEFINITELY NOT in this SSTable"; false positives possible, false negatives never. (2) Compaction — merge SSTables keeping newest versions, dropping superseded/tombstones; costs disk I/O + CPU. Closed with: B-tree pays amplification at write time, LSM pays it in compaction.

**Grade: Strong (best answer so far).**
- Bloom-filter semantics stated perfectly; compaction correct; amplification-location insight is senior-level.
- Refinements: (1) read checks **memtable first**, then SSTables. (2) Within one SSTable it's a sorted binary-search, not a full 64MB scan — the cost is the **number** of SSTables consulted. (3) Read path = **read** amplification (don't label it write amp).

**Gaps:** minor — memtable-first ordering; "read amplification" term.

---

## Q4 — What an index is; why it speeds reads/slows writes; when not to add one
**Question:** What is an index fundamentally; why does each index speed reads but slow writes (mechanism with 5 indexes); when to deliberately NOT add one?

**Answer:** Index = separate redundant sorted structure. Sorting by a column lets reads binary-search / use the tree instead of looking randomly → fast reads. Slows writes: with 5 indexes a write = write table + find insert spot in each index + write each → ~6 writes + reads to locate spots. Don't add indexes in write-heavy scenarios.

**Grade: Strong.**
- Mechanism and 6-writes count correct; write-heavy "don't index" case correct.
- Refinements: (1) index entries hold value → **row location (pointer)**; the baseline it defeats is a **full table scan**. (2) Second "don't index" case: **columns you rarely filter/join/sort by** — even on read-heavy tables, else you pay the write tax for an unused read speedup. Rule is per-column AND per-workload.

**Gaps:** minor — name "full table scan" baseline; the rarely-queried-column case.

---

## Q5 — ACID-C vs CAP-C (the trap)
**Question:** Define each "C" precisely; explain why conflating them is a probed mistake.

**Answer:** ACID-C = database state is valid at any point (foreign keys, constraints). CAP-C = all replicas agree.

**Grade: Strong (definitions) / Partial (missed the "why").**
- Both definitions correct. Add to CAP-C: "…so every read sees the latest write (**linearizability**)."
- Did NOT address why conflation is a trap. Key points: they're **orthogonal axes** — ACID-C = single logical DB honoring invariants across a txn (single-node); CAP-C = agreement across distributed replicas. Confusions probed: "need CP for consistency" when you mean ACID; assuming CAP-C ⇒ transactional (it doesn't — no multi-key atomicity); assuming AP can't enforce constraints (it can, locally). Wrong axis → wrong tech choice / needless latency.

**Gaps:** the **"why the conflation matters"** half — practice stating the orthogonality + the wrong-tech-choice consequence.

---

## Q6 — Applied: 2M IoT readings/sec, range query by device+time
**Question:** Pick data model + storage engine; justify both from mechanism; reference partition/clustering key; why relational+B-tree is wrong.

**Answer:** Wide-column (partition key = deviceId, clustering key = time; query trivial). LSM engine (write-heavy, 2M/s). Cassandra (not DDB "since it's on B-tree"). Not relational+B-tree "because both read-optimized."

**Grade: Strong (conclusions) / Partial (mechanism depth + one axis-slip).**
- All conclusions correct (wide-column, LSM, Cassandra, reject default).
- Mechanism was too thin — must state: clustering-key-time ⇒ rows sorted by time ⇒ range query = one contiguous sequential scan; partition-key-device ⇒ single-partition locality (no scatter-gather). LSM absorbs 2M/s as sequential appends; B-tree would do 2M random seeks/s = death.
- **Axis-slip (regression from Q1):** called "relational read-optimized." Relational is a MODEL, not read/write-optimized; only the B-tree ENGINE is. Two real reasons vs default: (1) B-tree engine can't take the write volume, (2) relational shards painfully vs wide-column native partition-key sharding.
- Product flag: "DynamoDB is on B-tree" — not firmly verifiable; anchor on clearly-LSM stores (Cassandra/ScyllaDB/HBase/Bigtable).

**Gaps:** mechanism-level justification depth; **do not re-conflate model vs engine** under time pressure.

---

## Lesson 02 — summary
6/6 answered. **Q1–Q4 Strong, Q5 Strong-def/Partial-why, Q6 Strong-conclusions/Partial-depth.** Overall: solid command of the mechanism content (B-tree vs LSM, Bloom/compaction, indexes). **Reinforce:** (a) the "why it matters" halves (Q5 conflation consequence, Q6 mechanism depth) — you get conclusions fast but under-explain the *why*; (b) guard the **model-vs-engine axis** under pressure (slipped in Q6 after nailing it in Q1).
