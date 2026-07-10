# 02 — Databases

This lesson assumes you understand Lesson 01 (latency hierarchy, back-of-envelope sizing, CAP/PACELC, consistency models) and **nothing** about databases beyond that. By the end you should be able to answer the question every storage decision comes down to: *given this access pattern, this consistency need, and this scale, which kind of database — and why, at the mechanism level.*

We build it in five parts:

1. **What a database actually is** — and the one distinction that unlocks everything: *data model* vs *storage engine*.
2. **The data-model families** — relational, key-value, document, wide-column, and the specialist stores — what each is *for* and what it costs.
3. **Storage engines: B-tree vs LSM-tree** — the mechanism deep-dive. This is the senior-level heart of the lesson.
4. **Indexing** — what an index really is, the kinds, and why every index you add speeds reads but slows writes.
5. **SQL vs NoSQL, and transactions/ACID** — putting it together into a decision.

Take your time. Part 3 is the part interviewers push on; don't skim it.

---

## Part 0 — What a database is, and the one distinction that matters

A **database** is a program whose job is to **store data durably and let you get it back efficiently.** "Durably" (from Lesson 01) means the data *survives a crash or power loss* — it's written to disk (SSD/HDD), not held only in volatile RAM. "Efficiently" means you can retrieve the specific data you want without reading everything.

That's it. A database is a specialized program that sits on one or more machines, owns some files on disk, and answers two kinds of requests you already met in Lesson 01: **reads** ("give me the data matching this") and **writes** ("store/change this data").

### The distinction that unlocks the whole lesson: data model vs storage engine

Every database is really **two separate design choices stacked on top of each other**, and beginners conflate them constantly. Keep them apart:

- **Data model** = *how you, the user, think about and query the data.* Is it tables with rows and columns? Is it a big dictionary of key → value? Is it nested JSON documents? This is the **shape of the questions you can ask** and how you phrase them. It's the *interface*.
- **Storage engine** = *how the database physically arranges bytes on disk and finds them.* When you say "store this row," what actually gets written to which file, in what order, and how does a later read locate it? This is the *implementation underneath*.

```
   ┌─────────────────────────────────────────────┐
   │  DATA MODEL   (what you see / how you query) │   ← relational? key-value?
   │   "SELECT * FROM users WHERE id = 42"        │      document? wide-column?
   ├─────────────────────────────────────────────┤
   │  STORAGE ENGINE (how bytes live on disk)     │   ← B-tree? LSM-tree?
   │   pages, trees, logs, files, indexes         │
   └─────────────────────────────────────────────┘
                         │
                         ▼
                   disk (SSD/HDD)
```

Why does separating them matter? Because **the two choices are largely independent**, and mixing them up leads to wrong conclusions. For example:
- People say "SQL is slow at writes and NoSQL is fast at writes." That's confusing the layers. Write speed is mostly a **storage-engine** property (B-tree vs LSM), *not* a data-model property. You can have a relational (SQL) database running on an LSM engine (MyRocks) that's write-fast, and a key-value (NoSQL) database on a B-tree engine. The data model (SQL vs NoSQL) and the write performance (B-tree vs LSM) are **different axes**.

So this lesson deliberately treats them as two axes:
- **Part 2** is the *data-model* axis (relational, KV, document, …).
- **Part 3** is the *storage-engine* axis (B-tree vs LSM).

When you can say *"I want the relational model but on a write-optimized LSM engine"* as a coherent sentence, you've got the core idea of this lesson.

---

## Part 1 — Two more pieces of vocabulary first

Two terms will show up constantly; define them now.

**Schema.** The **schema** is the *agreed-upon shape of your data* — what fields exist and what type each is. "A user has an integer `id`, a string `name`, and a timestamp `created_at`" is a schema. Two flavors:
- **Schema-on-write (rigid / enforced):** the database *refuses* to store data that doesn't match the declared shape. You must declare the columns up front; a row missing a required column, or with a string where a number belongs, is rejected *at write time*. Relational databases work this way. The benefit is **guarantees** (every row is well-formed; the database can catch bugs). The cost is **rigidity** (changing the shape later — a "schema migration" — is real work).
- **Schema-on-read (flexible):** the database stores whatever you give it (e.g. arbitrary JSON), and the *meaning* is imposed later, by the code that reads it. Document databases lean this way. The benefit is **flexibility** (add a field on Tuesday with no migration). The cost is **no guarantees** (different records can have different shapes — "schema drift" — and the burden of correctness moves into your application code).

**OLTP vs OLAP.** Two fundamentally different *workloads*, and databases are optimized for one or the other:
- **OLTP — Online Transaction Processing.** Lots of small operations, each touching **a few rows**, mixed reads and writes, and you want each to be **fast** (single-digit milliseconds). "Fetch user 42's profile," "insert this order," "decrement inventory by 1." This is the *operational* database behind a live app. Everything in this lesson is primarily about OLTP stores.
- **OLAP — Online Analytical Processing.** A *few big* queries that each scan **millions or billions of rows** to compute an aggregate. "What was total revenue by region, by month, for the last 3 years?" These are *analytics/reporting* queries, run by data teams, and they're happy to take seconds or minutes. OLAP uses very different storage (columnar) and gets its own treatment in Lesson 08. For now just hold the contrast: **OLTP = many tiny row-level ops, fast; OLAP = few huge scans, throughput-oriented.**

---

## Part 2 — The data-model families (the interface axis)

This is the "which kind of database" menu you branch on first. For each family: what it is, the mechanism that makes it good at its job, what it's for, and what it costs. The senior move is to **default to relational and only move off it when a specific decision point pushes you** — so we start there.

### 2a. Relational (SQL) — tables, rows, joins, transactions

The **relational model** organizes data into **tables**. A table is a grid: each **row** is one record (one user, one order), each **column** is one field (name, price). Every row in a table has the *same* columns (schema-on-write / rigid schema). You query it with **SQL** (Structured Query Language), a declarative language where you say *what* you want and the database figures out *how* to get it: `SELECT name FROM users WHERE id = 42`.

Two features define the relational model and are the reason it's the right default:

**1. Joins.** A **join** combines rows from two tables by matching a shared value. Suppose you keep `users` in one table and `orders` in another, and each order row stores the `user_id` of who placed it. "Show me every order placed by Maria" is a join: match `orders.user_id` to `users.id` where `users.name = 'Maria'`.

```
   users                         orders
   ┌────┬────────┐               ┌─────┬─────────┬────────┐
   │ id │ name   │               │ id  │ user_id │ total  │
   ├────┼────────┤               ├─────┼─────────┼────────┤
   │ 42 │ Maria  │◄──────────────│ 900 │   42    │ $30    │   join on
   │ 43 │ Omar   │      match     │ 901 │   43    │ $12    │   orders.user_id
   └────┴────────┘   id=user_id   │ 902 │   42    │ $99    │   = users.id
                                  └─────┴─────────┴────────┘
```

Why joins matter: they let you store each fact **exactly once** and combine facts on demand. Maria's name lives in *one* place (the `users` table). If she changes it, you update one row and every query about her orders reflects it. Storing each fact once is called **normalization**, and it's the relational model's superpower: no duplicated, drift-prone copies of the same fact.

**2. Transactions (ACID).** A **transaction** is a group of operations that the database treats as **one indivisible unit** — either *all* of them happen or *none* of them do. The canonical example is a money transfer: "subtract $40 from account A" and "add $40 to account B" must both happen or neither, or money is created or destroyed. Relational databases guarantee this with **ACID** (defined fully in Part 5). For now: ACID is the reason you can trust a relational database with money.

- **Use when:** you need transactions, joins, strong consistency, and a structured schema, at *moderate* scale (one beefy machine, or up to a handful with effort). This covers a huge fraction of real applications — "boring Postgres" is the correct answer far more often than candidates think.
- **Cost:** the classic relational databases (Postgres, MySQL) **scale up** (vertical) well but **shard hard** — splitting one logical database across many machines (Lesson 01, Part 1c) is manual, painful, and breaks joins and transactions that span shards. There's a ceiling on a single machine, and getting past it is where the pain starts. (Newer "NewSQL" systems like **Google Spanner** and **CockroachDB** keep the relational model *and* shard automatically across the globe using consensus — at the cost of higher write latency. More in Lesson 03.)

### 2b. Key-value — the dictionary at massive scale

A **key-value (KV) store** is conceptually a giant **dictionary / hash map** that lives on many machines: you store a **value** under a **key**, and later you fetch the value **by its exact key**. `put("session:abc", "{user:42,...}")` then `get("session:abc")`. That's the whole interface: **point lookups by key.**

```
   key                     value
   ─────────────────────   ────────────────────────────
   "user:42"           →   {name:"Maria", email:"..."}
   "session:abc123"    →   {userId:42, expires:...}
   "cart:42"           →   [item1, item2, item3]
        ▲
        │  get("user:42")  → returns the value in ~1 hop
```

Why it scales so beautifully: because you *always* access data by its key, the store can **shard on the key** trivially (Lesson 01, Part 1c) — hash the key to pick the shard, and any lookup goes straight to the one shard holding it, in a single hop. No coordination, no scanning. This is why KV stores (DynamoDB, Cassandra's KV usage, Redis) handle **millions of QPS at low, predictable latency**.

- **Use when:** access is "given this key, get this value" — user sessions, user profiles by id, shopping carts, feature flags, caches, the URL-shortener from Lesson 01. Massive scale, simple access, lowest latency.
- **Cost:** the interface is *deliberately* limited. **No joins.** **No "query by a field inside the value"** — you can't ask "all users in Texas" unless Texas is part of the key, because the store only knows how to find things by key. You must **model your data around exactly one access pattern** and pick the key to match it. And a single wildly-popular key (one celebrity's profile) can overload the one shard that holds it — a **hot key** (Lesson 04 covers mitigations).

### 2c. Document — nested JSON, flexible schema

A **document store** holds **documents**, which are self-contained nested structures — think **JSON objects**. Instead of splitting a user across several tables (user table, addresses table, orders table) and joining them, you store the whole thing as *one* document with nested fields and arrays inside it.

```json
{
  "id": 42,
  "name": "Maria",
  "addresses": [ {"type":"home", "city":"Austin"},
                 {"type":"work", "city":"Dallas"} ],
  "recentOrders": [ {"id":900,"total":30}, {"id":902,"total":99} ]
}
```

The mechanism win: everything about one entity is in **one place**, so fetching "the whole user" is a *single read* of one document — no joins needed. And it's typically **schema-on-read** (flexible): different user documents can have different fields; you add a field with no migration.

- **Use when:** your data is naturally document-shaped and mostly accessed **one whole document at a time**, the schema evolves rapidly, or nesting fits the domain (product catalogs, user profiles, content/CMS). MongoDB is the archetype.
- **Cost:** the flip side of "one document at a time." **Cross-document joins and transactions are weak or awkward** — the model doesn't want you relating many documents. Because each fact can be **duplicated** across documents (Maria's name copied into every order document that embeds it), updating a fact means finding and rewriting every copy, and copies can **drift** out of sync. Flexible schema also means the *discipline* of a consistent shape moves into your application code — "schema drift" is a real maintenance tax.

### 2d. Wide-column — write-heavy, huge, time-ordered

A **wide-column store** (Cassandra, HBase, Bigtable) looks table-ish but is really a **massive, sparse, partitioned map**. You define a **partition key** (which decides *which machine* the data lives on) and a **clustering key** (which decides the *sort order within* that partition). Data is stored **physically sorted by the clustering key inside each partition**, which makes "give me a *range* of rows for this partition" extremely fast.

```
   partition key = deviceId      clustering key = timestamp (sorted)
   ┌───────────────────────────────────────────────────────────┐
   │ device "sensor-7"  →  [ t=1000: 21.5°, t=1001: 21.6°,       │
   │                         t=1002: 21.4°, t=1003: 21.7° … ]    │  ← sorted run
   └───────────────────────────────────────────────────────────┘
   query: "sensor-7 readings between t=1000 and t=1002"  → one contiguous scan
```

The reason these exist: they're built on **LSM-tree storage engines** (Part 3), which make **writes extremely cheap and fast**, and they **shard on the partition key by design**. So they eat enormous, relentless write streams — sensor data, event logs, message histories, time-series — at scale that would crush a classic relational DB.

- **Use when:** **write-heavy**, append-mostly, huge scale, and queries are known in advance and shaped like "range scan within a partition" (all events for a user, all readings for a device in a time window). Tunable consistency (you can dial per-query how many replicas must agree — Lesson 03).
- **Cost:** you must **design your query patterns up front** and shape the table around them — the partition/clustering keys *are* your query plan. **No ad-hoc queries**, no joins. Ask a question the keys weren't designed for and you're doing a full, slow scan of everything.

### 2e. The specialist stores (know when each is the right tool)

You don't design around these, but you must recognize the *one thing* each is for and reach for it when that need appears:

- **Object store** (Amazon S3, Azure Blob/ADLS, Google GCS): for **large blobs** — images, video, backups, data-lake files. Cheap, effectively infinite, extremely durable. High *per-operation* latency and no row-level transactions, so the pattern is: **store the big file in the object store, keep only a small pointer/URL + metadata row in your real database.** Never stuff a 4 MB image into a relational row.
- **Search index** (Elasticsearch, OpenSearch): for **full-text search and relevance ranking** — "find articles containing these words, best matches first," faceted filtering. It is **not a source of truth** (it's eventually-consistent and rebuildable); you keep the authoritative data in your main DB and **feed a copy into the search index** to serve search queries. (Lesson 08.)
- **Time-series** (InfluxDB, TimescaleDB, Prometheus): purpose-built for **metrics/telemetry** — append-heavy, timestamped, with built-in time-window queries, downsampling, and retention ("keep 1-second data for a week, then roll up to 1-minute"). A narrow but very deep specialty.
- **Graph** (Neo4j, Neptune): for **deep relationship traversal** — "friends of friends of friends," fraud rings, recommendation paths. When the *relationships* are the point and you'd need many self-joins in SQL, a graph DB traverses them natively. Niche; hard to scale.
- **Vector** (pgvector, Pinecone, Milvus): for **similarity search over embeddings** — "find the 10 items whose meaning is closest to this one," the retrieval half of RAG/semantic search. Tunes recall vs speed via approximate-nearest-neighbor indexes.
- **In-memory** (Redis, Memcached): data lives in **RAM** for **sub-millisecond** access — caches, sessions, leaderboards, rate-limit counters. Blazing fast but **volatile** (lose power, lose data, unless you enable persistence) and **RAM is expensive**, so you hold only hot/ephemeral data here. (Redis is the star of Lesson 04.)

---

## Part 3 — Storage engines: B-tree vs LSM-tree (the mechanism deep-dive)

This is the part senior interviews probe. We now drop *below* the data model to the **storage engine** — how bytes physically get onto disk and found again. There are two dominant designs, and they make **opposite trade-offs**. Understanding *why* is the goal.

First, recall two facts from Lesson 01 that drive everything here:
1. **Disk is ~1000× slower than RAM**, and a network hop is in between. Touching the disk is the expensive thing.
2. **Sequential access crushes random access.** Reading/writing data laid out *in order* is dramatically faster than jumping around to scattered locations — this is true even on SSDs, and enormously true on spinning HDDs where a physical arm must move. *Remember this; it is the entire reason LSM-trees exist.*

Also one new term: a **write to a random location** ("update the row for user 42, wherever it happens to live on disk") forces the disk to seek to that spot. A **sequential write** ("append these bytes to the end of a file") never seeks — it just keeps writing forward. Sequential is far cheaper. Hold that.

### 3a. B-tree — the "update data in place" design

A **B-tree** is the classic storage engine, used by essentially every relational database (Postgres, MySQL/InnoDB, SQL Server, Oracle) and many others. It keeps the data **sorted by key** in fixed-size chunks called **pages** (typically 4–16 KB each), arranged as a **balanced tree** so any key can be found in a few hops.

Picture the tree. The top page (**root**) doesn't hold data — it holds a small set of keys that say "keys less than 100 are down this branch; 100–200 down that branch; …". Each branch leads to another such **index page**, and eventually to **leaf pages** that hold the actual rows, in sorted order.

```
                       ┌─────────────────────┐
        root  ───────► │  <100 | 100–200 | … │      (index page: pointers)
                       └───┬───────┬─────────┘
              ┌────────────┘       └───────────┐
              ▼                                ▼
      ┌───────────────┐               ┌───────────────┐
      │ 10 | 40 | 70  │  (index)      │ 120 | 160 | …  │
      └──┬────┬────┬──┘               └──────┬────────┘
         ▼    ▼    ▼                         ▼
      ┌─────┐ ...                       ┌──────────┐
      │leaf │  actual rows here,        │  leaf    │   ← real data,
      │10..│  sorted by key             │ 120..    │      sorted
      └─────┘                           └──────────┘
```

**How a read works.** To find key 160: start at the root ("160 is in the 100–200 branch"), follow one pointer to the next index page ("160 ≥ 120, go right"), follow to the leaf, read the row. Because the tree is **balanced** (all leaves at the same depth) and each page has *many* keys, the tree is very *shallow* — even a database with billions of rows is only ~3–5 levels deep. So **any single-row read is ~3–5 page reads** — a handful of disk touches. **Reads are excellent and predictable.**

**How a write works — and where the cost hides.** To update user 42, the B-tree finds the *exact page* where user 42 already lives and **modifies it in place** — it overwrites the existing bytes on disk at that location. This is the defining trait: **B-trees update data in place.** Two consequences:
- That page is at some effectively random spot on disk, so a write is a **random write** (a seek). Under heavy write load, you're seeking all over the disk.
- To be crash-safe, before modifying the page the database first appends the change to a **write-ahead log (WAL)** — a sequential log written *before* the real page is touched, so that if the machine crashes mid-write, it can replay the log and recover. (WAL is a recurring pattern; it reappears in Lesson 03 for replication.) So a single logical write actually causes **more than one physical write** (log + the page, plus sometimes splitting a full page into two). This "one logical write → several physical writes" multiplier is called **write amplification**, and B-trees have a meaningful amount of it.

**B-tree summary:** *reads are fast and predictable; writes are in-place random writes with real write-amplification.* B-trees are **read-optimized.**

### 3b. LSM-tree — the "never update in place, just append" design

The **LSM-tree** (Log-Structured Merge-tree) makes the **opposite** bet. Its designers took Lesson 01's rule — *sequential writes crush random writes* — and built the whole engine around **never doing a random write.** It powers Cassandra, HBase, RocksDB, LevelDB, ScyllaDB, and (via RocksDB/MyRocks) even relational databases. Here's the mechanism, step by step.

**Step 1 — writes go to RAM first (the memtable).** A write doesn't touch the data files on disk at all. It goes into an in-memory sorted structure called the **memtable** (plus a quick sequential append to a WAL on disk, purely for crash recovery). Writing to RAM is ~1000× faster than a random disk write (Lesson 01), so **writes are blazingly fast** and never seek.

```
   write("user:42", ...)  ─┐
   write("user:17", ...)  ─┤──►  MEMTABLE (in RAM, kept sorted)   + append to WAL (disk, sequential)
   write("user:99", ...)  ─┘        [17, 42, 99, …]
```

**Step 2 — flush to disk as an immutable sorted file (SSTable).** When the memtable fills up (say 64 MB), the engine writes it out to disk **all at once, sequentially**, as a **sorted, immutable file** called an **SSTable** (Sorted String Table). "Immutable" = once written, this file is **never modified.** Because the memtable was already sorted, this flush is one long **sequential write** — the cheap kind. The memtable is then emptied and starts fresh.

```
   memtable full ──► flush (one big sequential write) ──►  SSTable-1 on disk
                                                           [ sorted, immutable ]
```

Over time you accumulate **many** SSTables on disk, each sorted internally, each a frozen snapshot of some past memtable.

```
   disk:  [SSTable-1]  [SSTable-2]  [SSTable-3]  …   (newest → oldest)
```

**Step 3 — updates and deletes don't modify anything; they just append a newer record.** Here's the mind-bender. To "update" user 42, the LSM-tree does **not** find and change the old record. It just writes a **new** record for user 42 into the current memtable, which later flushes to a *newer* SSTable. Now user 42 exists in *two* SSTables with two values. **The newer one wins** — and the engine knows which is newer because newer SSTables are checked first. A **delete** works the same way: it writes a special "this key is deleted" marker called a **tombstone**. Nothing is ever overwritten in place; the truth is *"the most recent record for a key, across all SSTables."*

**How a read works — and where the cost hides.** To read user 42, the engine must find the *newest* record for that key. It checks the memtable first (newest data), then SSTable-by-SSTable from newest to oldest, stopping at the first hit. In the worst case a key that isn't in RAM forces it to look in **many** SSTables — so **reads can be slower than a B-tree**, because one logical read may touch several files. This is the price of the write-cheap design. Two mechanisms rescue read performance:
- **Bloom filters.** A **Bloom filter** is a tiny in-RAM probabilistic structure that can answer *"is key 42 definitely NOT in this SSTable?"* very fast. If the filter says "definitely not," the engine **skips that SSTable entirely** without touching the disk. (It can have false positives — occasionally says "maybe" when the key is absent — but *never* false negatives, so it's safe to skip on a "no.") This lets a read skip most SSTables and touch only the one or two that might actually hold the key.
- **Compaction.** A background process called **compaction** continuously **merges** several SSTables into fewer, larger ones, and while merging it **throws away superseded records** — old versions of updated keys and tombstoned deletes. This keeps the number of SSTables (and thus read cost) bounded, and reclaims the space held by stale versions. Because SSTables are all sorted, merging them is a cheap **sequential** merge (like merging sorted lists). The cost: compaction consumes background CPU and disk I/O, and it *is* LSM's form of **write amplification** (data gets rewritten each time it's compacted).

```
   compaction:  [SST-1][SST-2][SST-3]  ──merge, drop stale/tombstones──►  [ one bigger sorted SSTable ]
```

**LSM-tree summary:** *writes are fast sequential appends to RAM then flushed in bulk; reads may touch several files but Bloom filters + compaction keep them acceptable; space and background I/O are spent on compaction.* LSM-trees are **write-optimized.**

### 3c. The head-to-head (the table to internalize)

| | **B-tree** | **LSM-tree** |
|---|---|---|
| **Core idea** | Update data **in place**, kept sorted in pages | **Never** update in place; **append** new records, merge later |
| **Writes** | Random writes (seeks) + WAL → moderate write-amp; slower under heavy write load | Sequential appends to RAM then bulk flush → **very fast writes** |
| **Reads** | **Fast, predictable** — ~3–5 page reads down a shallow tree | Can touch **several SSTables**; rescued by Bloom filters + compaction → good but less predictable |
| **Space** | Can leave partially-empty pages (fragmentation) | Compaction reclaims space; but keeps old versions until compacted |
| **Optimized for** | **Read-heavy** workloads | **Write-heavy** workloads |
| **Typical homes** | Postgres, MySQL/InnoDB, most relational DBs | Cassandra, HBase, RocksDB, LevelDB, ScyllaDB |

**The one-sentence version to say in an interview:** *"B-trees update in place — great predictable reads, costlier random writes; LSM-trees only ever append and compact in the background — great write throughput, at the cost of read amplification that Bloom filters and compaction contain. So read-heavy → lean B-tree; write-heavy/append-heavy → lean LSM."*

And crucially — this is a **storage-engine** choice, independent of the **data model**. That's why a write-heavy relational workload can run relational-model-on-LSM (MyRocks), and why wide-column stores (a NoSQL data model) are LSM underneath. Two axes.

---

## Part 4 — Indexing

We keep saying reads are "fast" because the database "finds" the row. **Indexes** are *how* it finds the row without reading the whole table. This section explains what an index is, the kinds, and the universal trade-off.

### 4a. What an index actually is

Imagine a table of 100 million users and the query `WHERE email = 'maria@x.com'`. Without help, the database has no idea where that row is, so it does a **full table scan** — read *every one* of the 100 million rows and check each email. That's catastrophically slow.

An **index** is a **separate, sorted data structure that maps a column's values to the locations of the matching rows** — exactly like the index at the back of a textbook maps "topic → page number" so you don't read the whole book. Build an index on `email`, and now the database looks up `maria@x.com` in the sorted index (a few hops, like the B-tree read in Part 3a), gets a pointer straight to the row, and fetches just that one row.

```
   index on `email` (sorted by email)          the table (rows by location)
   ┌───────────────────────┬──────────┐
   │ email                 │ row loc  │
   ├───────────────────────┼──────────┤
   │ "ann@x.com"           │  →  #883 │
   │ "maria@x.com"         │  →  #204 │──────────►  row #204: {id:42, name:Maria,…}
   │ "omar@x.com"          │  →  #911 │
   └───────────────────────┴──────────┘
   lookup "maria@x.com" → binary-search the sorted index → jump to row #204
```

An index is usually itself a **B-tree** (sorted → supports both exact lookups *and* range scans like `WHERE age BETWEEN 20 AND 30`). In an LSM store, indexes are LSM-structured. Either way, the point stands: **an index is a redundant, sorted, auxiliary copy of one (or a few) columns that turns a full scan into a quick lookup.**

### 4b. The kinds of index you should name

- **Primary / clustered index.** The index on the table's **primary key** (the unique id of each row). In many databases this is **clustered**, meaning **the table's actual rows are physically stored in primary-key order** — the leaf pages of this index *are* the data. Consequence: a lookup by primary key lands directly on the row (no second hop), and **range scans by primary key are sequential and fast**. A table has *at most one* clustered index, because the rows can only be physically sorted one way.
- **Secondary index.** Any *additional* index on a **non-key** column (like `email` or `age`). It's a separate structure whose entries point back to the rows. You can have **many** secondary indexes on a table — one per column (or set of columns) you frequently filter or sort by.
- **Composite (multi-column) index.** An index on **several columns in a specific order**, e.g. `(last_name, first_name)`. It's sorted by the first column, then the second within ties — exactly like a phone book sorted by last name then first name. Key rule (the **left-prefix rule**): a composite index on `(A, B)` helps queries filtering on `A`, or on `A` *and* `B`, but **not** a query filtering on `B` alone — just as a phone book sorted by (last, first) is useless for "find everyone whose *first* name is Maria." Order matters; put the column you always filter on first.
- **Covering index.** An index that happens to contain **every column a particular query needs**, so the database can answer the query **from the index alone without touching the table at all** ("the index *covers* the query"). Fastest possible read for that query, at the cost of a wider index.

### 4c. The universal trade-off: indexes speed reads, slow writes

Here is the single most important thing to say about indexes, and it ties straight back to Lesson 01's "read-optimized vs write-optimized" tension:

> **Every index you add makes matching reads faster but makes *every write* to that table slower and larger.**

Why writes get slower: an index is a **redundant sorted copy** of some columns. When you **insert, update, or delete** a row, the database must update **not just the row, but every index that includes an affected column** — keeping each of those sorted structures correct. Five indexes on a table means one insert becomes **six writes** (the row + five index updates). That's more write amplification (Part 3), more disk I/O, more latency on every write, and more storage.

So indexing is a **deliberate, per-query decision**, not "index everything":
- **Index the columns you frequently filter, join, or sort by** — you're spending write cost to buy read speed where reads are hot.
- **Don't index columns you rarely query** — you'd pay the write tax on every insert for a read speedup you almost never use.
- On a **write-heavy** table, **minimize indexes** (the framework rule "write-heavy → avoid global indexes"): each index is a tax on the very operation you do most.
- On a **read-heavy** table, **more indexes (and covering indexes) are worth it** — reads dominate, so paying on the rare write to make the common read fast is a good trade.

This is Lesson 01's read-optimized-vs-write-optimized tension made concrete: indexes, caches, replicas, and denormalized copies **all** speed reads by adding redundant data that every write must then maintain.

---

## Part 5 — SQL vs NoSQL, and transactions (ACID vs BASE)

Now we assemble the axes into the decision you actually voice in an interview.

### 5a. "SQL vs NoSQL" — the crisp, honest version

"NoSQL" isn't one thing — it's the umbrella for *everything that isn't the relational model* (key-value, document, wide-column, graph, …). But the useful contrast is:

- **SQL (relational):** strong consistency, **joins**, **transactions**, and flexible *ad-hoc queries* (you can ask questions you didn't plan for) — but a **rigid schema** and it **scales up more easily than out** (sharding is painful, and cross-shard joins/transactions are the pain).
- **NoSQL:** built for **scale, high write throughput, and flexible/evolving schema**, and it **shards naturally** — but you **give up joins and rich transactions**, and you must **design around one access pattern** decided up front.

The senior positioning, said out loud: **"Default to relational — 'boring Postgres' — unless a specific decision point pushes me off it."** Those decision points are:
- **Scale/throughput beyond one machine** (millions of QPS, or writes a single relational box can't take) → key-value / wide-column.
- **Access is pure point-lookup-by-key** → key-value.
- **Data is document-shaped and read whole; schema evolves fast** → document.
- **Relentless append/time-series at huge scale** → wide-column / time-series (LSM under the hood).
- **Need joins + transactions but also global horizontal scale** → NewSQL (Spanner, CockroachDB), accepting higher write latency.

Notice these map cleanly onto the two axes: the *data-model* choice (Part 2) picks the family; the *workload* (read- vs write-heavy) hints at the *storage-engine* leaning (Part 3); and the *access pattern* decides your keys and indexes (Part 4).

### 5b. Transactions: ACID vs BASE

A **transaction** (Part 2a) is a group of operations treated as one unit. The guarantee level has two named ends:

**ACID** — the strong guarantees relational databases give:
- **A — Atomicity:** all operations in the transaction happen, or **none** do. No half-applied transfer. (If anything fails midway, the whole thing is rolled back as if it never started.)
- **C — Consistency:** the transaction moves the database from one **valid state to another**, never violating declared rules (constraints, foreign keys, "balance ≥ 0"). *(Note: this "C" is a different, unrelated concept from the "C" in CAP/PACELC — same letter, different meaning. ACID-C = "database rules are never broken"; CAP-C = "all replicas agree / reads see the latest." Interviewers love that people conflate them; keep them separate.)*
- **I — Isolation:** concurrent transactions don't step on each other — each behaves as if it ran **alone**, even when many run at once. (No reading another transaction's half-finished changes.)
- **D — Durability:** once the database says a transaction **committed**, it **survives crashes** — it's on durable storage (this is what the WAL from Part 3a guarantees).

ACID is why you trust a relational database with money, orders, and inventory.

**BASE** — the weaker posture many NoSQL/AP systems take, and a direct application of Lesson 01's CAP/PACELC:
- **BA — Basically Available:** the system stays up and answering (favoring the **A** of CAP).
- **S — Soft state:** the data can be **in flux** — replicas may temporarily disagree (no promise every read reflects the latest write).
- **E — Eventual consistency:** if writes stop, all replicas **eventually converge** (the "eventual" model from Lesson 01, Part 4).

BASE is just what you *get* when you chose **AP / eventual consistency** to buy availability and low latency at scale. It's not sloppiness — it's the deliberate trade you make for the like-counts-and-feeds kind of data, exactly as CAP/PACELC prescribed. Money and inventory get ACID; feeds and counters get BASE. **You mix them per data type** — the same senior lesson as consistency models in Lesson 01.

---

## Part 6 — Putting the whole decision together

The clean way to reason about "which database," using everything above, as an ordered set of questions:

1. **What's the access pattern?** Point-lookup by key → KV. Whole-document reads → document. Range-scans within a partition, append-heavy → wide-column. Rich ad-hoc queries + joins → relational. Big blobs → object store + a metadata row. Full-text → search index alongside the source of truth.
2. **What consistency does *this data* need?** Money/inventory/uniqueness → strong + ACID (lean relational). Feeds/counts/telemetry → eventual + BASE (lean NoSQL). (And you may need *both*, for different data, in the same system.)
3. **Read-heavy or write-heavy?** Read-heavy → B-tree-ish engine + replicas + cache + more indexes. Write-heavy/append → LSM-ish engine + partition/shard + fewer indexes.
4. **Does it fit on one machine?** (Back-of-envelope from Lesson 01.) If yes → boring single relational box wins; don't over-engineer. If no → the family must shard naturally (KV/wide-column) or you adopt NewSQL to keep relational semantics while sharding.
5. **Then, and only then, name a product** — Postgres, DynamoDB, Cassandra, Mongo, S3+metadata, etc. — as the *implementation* of the model+engine+consistency choice you already justified.

The mistake to avoid: **naming a trendy database first and reverse-justifying it.** Derive the *properties* you need from access pattern + consistency + scale (the numbers), then the product falls out — the same discipline as deriving the URL-shortener's architecture from arithmetic in Lesson 01.

---

## Self-check

Say these out loud, in your own words, as if teaching them. Answers follow each — but answer first, *then* check.

**1. What are the two independent axes every database is composed of, and why does keeping them separate matter?**
> *Data model* (the interface — relational/KV/document/wide-column — how you query) and *storage engine* (how bytes live on disk — B-tree vs LSM). They're independent: e.g. a relational model can run on an LSM engine (MyRocks). Conflating them leads to false claims like "SQL is write-slow" — write speed is a storage-engine property, not a data-model one.

**2. Explain, at the mechanism level, why an LSM-tree has faster writes than a B-tree.**
> A B-tree updates data *in place* — a random write (seek) to the page where the row lives, plus a WAL append, plus possible page splits (write amplification). An LSM-tree *never* updates in place: writes go to an in-RAM memtable (plus a sequential WAL append), and when full it's flushed to disk as one big *sequential* write (an immutable SSTable). Updates/deletes just append newer records/tombstones. Sequential writes to RAM/disk crush the B-tree's random writes (Lesson 01), so LSM writes are far faster.

**3. If LSM writes are so cheap, why can its reads be slower, and what two mechanisms fix that?**
> A key can exist in many SSTables (old + new versions), so a read may have to check the memtable then several SSTables newest-to-oldest — read amplification. Fixes: **Bloom filters** (tiny in-RAM structure that says "key definitely not in this SSTable," letting the engine skip most files) and **compaction** (background merge of SSTables that drops superseded/tombstoned records, bounding the file count and reclaiming space).

**4. Why does every index speed reads but slow writes? When should you *not* add one?**
> An index is a redundant sorted copy of some column(s). Reads use it to jump to rows instead of full-scanning. But every insert/update/delete must also update *every* affected index to keep it sorted — so N indexes turn one write into ~N+1 writes (write amplification, latency, storage). Don't index columns you rarely query, and minimize indexes on write-heavy tables.

**5. Distinguish the "C" in ACID from the "C" in CAP.**
> ACID-C = a transaction never violates the database's declared rules/constraints (valid state → valid state). CAP-C = all replicas agree / every read sees the latest write (linearizability). Same letter, unrelated concepts — a classic trap.

**6. A service ingests 2 million IoT sensor readings per second, queried later as "all readings for device X in a time window." Which data model and storage engine, and why?**
> Write-heavy, append-mostly, time-ordered, range-scan-within-partition access → **wide-column** data model (partition key = deviceId, clustering key = timestamp) on an **LSM** storage engine. LSM absorbs the relentless write stream cheaply (sequential appends); wide-column shards on deviceId and stores rows sorted by time so the range query is one contiguous scan. Relational + B-tree would choke on the write volume and shard painfully.

**7. Why is "default to boring Postgres" good advice, and what specifically pushes you off it?**
> The relational model gives joins, ACID transactions, strong consistency, and ad-hoc queries — covering most applications — and a single machine handles a lot. You move off it only when a concrete decision point demands: scale/write-throughput beyond one machine (→ KV/wide-column), pure point-lookup access (→ KV), fast-evolving document-shaped data (→ document), huge append/time-series (→ wide-column/time-series), or global scale *with* relational semantics (→ NewSQL, accepting higher write latency).

---

*Next: Lesson 03 — Replication & Partitioning: how we keep copies in sync (replication modes, leader/follower, quorums, consensus/Raft) and how we split data across machines (sharding strategies, rebalancing). This lesson set up the "one machine isn't enough" cliff repeatedly; Lesson 03 is how you actually cross it.*
