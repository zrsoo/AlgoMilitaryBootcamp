# System Design — Decision Framework

> A catalog of **every part of a system**, and for each part: the **decision points** (the factors/requirements that force a choice) and the **options** (what to pick from), each with a **"use when"** rule and its **trade-offs**.
>
> How to use this: in a mock, walk the layers top-to-bottom. For each layer ask "does the prompt touch this? which decision point fires? which option does that point me to, and what's the cost?" Say the *why* out loud — that's what's graded.
>
> Companion to [method.md](method.md) (the *how to run the interview*) and [study/](study/) (the *why each option works*). Target level: **senior (L5)** — go one level deeper than the option name; know the mechanism.

Status: v0.1 (2026-07-07). This is a **living document** — correct/expand as reps expose gaps.

---

## How to read an entry

- **Decision point** = a requirement or condition that forces a choice (e.g. "read:write is 100:1", "need sub-10ms p99", "payload > 1 MB").
- **Option** = a concrete technology or pattern.
- **Use when** = the signal that selects the option.
- **Cost** = what you give up by picking it.

The skill is mapping *decision point → option → stated trade-off*, not memorizing tools.

---

## Layer 0 — Requirements & scale (drives everything below)

Before any component, these decision points set the whole shape:

| Decision point | Why it matters / where it sends you |
|---|---|
| **Read-heavy vs write-heavy** (ratio) | Read-heavy → replicas + cache + denormalize. Write-heavy → partition/shard, LSM stores, async ingest, avoid global indexes. |
| **QPS (avg and peak)** | Sets single-box vs fleet vs sharded. Peak = avg × 2–5. |
| **Data volume + growth** | GB → one DB. TB–PB → partitioning, object store, tiering. |
| **Latency SLA (p50/p99)** | Sub-10ms → in-memory/cache/colocated. 100ms–1s → normal web. Seconds+ → batch/async OK. |
| **Consistency need** | Money/inventory → strong. Feeds/counts/analytics → eventual is fine. |
| **Availability target** | 99.9% vs 99.99% → replication, multi-AZ/region, failover cost. |
| **Durability need** | Can we lose data? → replication factor, sync vs async, WAL/checkpoint. |
| **Access pattern** | Point lookups → KV. Ranges/joins → relational. Text → search. Aggregations → OLAP. Relationships → graph. |
| **Payload size / type** | Small structured → DB row. Large blobs/media → object store + CDN. |
| **Freshness** | Real-time → streaming. Hourly/daily OK → batch (cheaper, simpler). |
| **Cost ceiling** | Forces tiering, scale-to-zero, batch over stream, cache over compute. |
| **Multi-tenancy** (senior) | Isolation, quotas, noisy-neighbor control, per-tenant keys/RBAC. |

> Senior tell: pick **one primary metric** to optimize and justify sacrificing the others. Don't gold-plate.

---

## Layer 1 — Client & edge (CDN)

**Decision points:** static vs dynamic content · global user base · large media · read-heavy public assets · latency sensitivity of first byte.

| Option | Use when | Cost |
|---|---|---|
| **CDN** (CloudFront/Fastly/Akamai) | Static assets, media, cacheable GETs, geographically spread users | Cache invalidation complexity; not for personalized/dynamic data |
| **Edge compute** (Lambda@Edge/Workers) | Lightweight personalization, auth, A/B at the edge | Limited runtime; harder debugging |
| **Client-side cache** (browser, mobile) | Data that changes rarely; offline tolerance | Staleness; invalidation |
| **No edge layer** | Internal/low-traffic service | — (don't add complexity you don't need) |

---

## Layer 2 — Load balancing & traffic routing

**Decision points:** need TLS termination / path routing (L7) vs raw throughput (L4) · global vs regional · session affinity · health-based failover · sudden spikes.

| Option | Use when | Cost |
|---|---|---|
| **L4 LB** (NLB, TCP) | High throughput, non-HTTP, lowest latency | No content-aware routing |
| **L7 LB** (ALB, Envoy, NGINX) | HTTP routing by path/host/header, TLS termination, retries | Slightly higher latency; more config |
| **DNS / GeoDNS / Anycast** | Global traffic steering, region failover | Slow to propagate (TTL); coarse-grained |
| **API gateway** | Auth, rate-limit, routing, aggregation at one entry | Single choke point; must scale it |
| **Service mesh** (Istio/Linkerd) | Many microservices needing mTLS, retries, observability | Operational heaviness; overkill for few services |

Routing tactics: **round-robin** (uniform nodes) · **least-connections** (uneven request cost) · **consistent hashing** (sticky/cache locality, minimizes reshuffle on scale changes) · **weighted** (canary/heterogeneous nodes).

---

## Layer 3 — API / communication style

**Decision points:** request/response vs streaming · internal vs public · latency/throughput · payload shape · client diversity · real-time push needed · one-to-one vs one-to-many.

| Option | Use when | Cost |
|---|---|---|
| **REST/JSON** | Public APIs, CRUD, broad client support, simplicity | Verbose; over/under-fetching; no streaming |
| **gRPC/protobuf** | Internal service-to-service, low latency, high throughput, streaming, typed contracts | Binary (harder to debug); limited browser support |
| **GraphQL** | Many clients with varied data needs; avoid over-fetching; aggregation | Server complexity; caching harder; N+1 risk |
| **WebSocket** | Bidirectional real-time (chat, presence, collaborative) | Stateful connections; scaling fan-out is work |
| **SSE (server-sent events)** | Server→client one-way stream (feeds, notifications) | One-directional; HTTP/1 connection limits |
| **Long polling** | Near-real-time without WS infra; simple | Wasteful; higher latency than WS |
| **Message-based / async (via queue)** | Decoupled producers/consumers, spiky load, long jobs | Eventual; needs a broker; harder request tracing |
| **Webhooks** | Server→server event notification to third parties | Delivery/retry/security burden on receiver |

Cross-cutting API decisions: **pagination** (cursor > offset at scale) · **idempotency keys** (safe retries on writes) · **versioning** (URL vs header) · **batch vs chatty** (fewer round trips) · **backward-compatible schema evolution** (senior: APIs that survive years).

---

## Layer 4 — Compute / application tier

**Decision points:** team size · deploy independence · scaling granularity · state · request duration · traffic burstiness.

| Option | Use when | Cost |
|---|---|---|
| **Monolith** | Early stage, small team, MVP, low operational budget | Scales as one unit; deploy coupling |
| **Microservices** | Independent scaling/teams, clear bounded contexts, big org | Distributed-systems tax (network, consistency, ops) |
| **Serverless / FaaS** | Spiky/unpredictable load, event-driven, want scale-to-zero | Cold starts; time limits; vendor lock; state is external |
| **Stateless services** (default) | Horizontal scale, easy failover | Push state to DB/cache (that becomes the bottleneck) |
| **Stateful services** | Colocated state for latency (in-mem index, sticky sessions) | Hard to scale/rebalance; failover complexity |
| **Batch workers** | Throughput over latency; scheduled jobs | Not for interactive paths |
| **Stream processors** | Continuous low-latency transforms on events | State/checkpoint management; exactly-once is work |

Sync vs async: **sync** when the caller needs the result now and it's fast; **async (queue + worker)** when work is slow, spiky, or can fail/retry — return a job id, notify on completion.

---

## Layer 5 — Data storage (the big one)

First branch on **access pattern + consistency + scale**. Then pick a family, then a product.

### 5a. Store families

| Option | Use when | Cost |
|---|---|---|
| **Relational / SQL** (Postgres, MySQL, Aurora, Spanner) | Transactions, joins, strong consistency, structured schema, moderate scale (or Spanner/Cockroach for global) | Vertical scale limits; sharding is manual pain (classic RDBMS) |
| **Key-value** (Redis, DynamoDB, Cassandra) | Point lookups by key, massive scale, simple access, low latency | No joins; you model around one access pattern; hot-key risk |
| **Document** (MongoDB, DocumentDB) | Flexible/nested schema, per-document access, rapid iteration | Weak cross-document transactions/joins; schema drift |
| **Wide-column** (Cassandra, HBase, Bigtable) | Write-heavy, time-series-ish, huge scale, tunable consistency | Query patterns must be designed up front; no ad-hoc queries |
| **Object store** (S3, ADLS, GCS) | Large blobs, media, data-lake files, cheap durable bulk | High per-op latency; not for transactional row access |
| **Search index** (Elasticsearch, OpenSearch) | Full-text, faceted, relevance-ranked queries | Not a source of truth; eventual; costly to keep in sync |
| **Time-series** (InfluxDB, TimescaleDB, Prometheus) | Metrics, telemetry, append-heavy with time queries + retention | Narrow purpose |
| **Graph** (Neo4j, Neptune) | Deep relationship traversal (social, fraud, recommendations) | Niche; scaling traversals is hard |
| **Vector** (pgvector, Pinecone, Milvus) | Similarity search / embeddings / RAG | New op surface; index tuning (ANN recall vs speed) |
| **In-memory** (Redis, Memcached) | Cache, sessions, leaderboards, rate counters, sub-ms | Volatile (unless persistence); RAM cost |

### 5b. Decision points → what they select

- **Need ACID transactions across rows** → relational (or Spanner/Cockroach at global scale).
- **Point lookups at millions QPS** → KV / wide-column with a well-chosen partition key.
- **Write-heavy, append-mostly, time-ordered** → wide-column / LSM-based / time-series.
- **Ad-hoc analytics / aggregations over huge data** → OLAP / columnar / lakehouse (Layer 12), *not* the OLTP store.
- **Full-text or relevance** → search index alongside the source-of-truth DB.
- **Big blobs** → object store; keep only metadata/pointer in the DB.
- **Flexible evolving schema** → document (but beware losing constraints).
- **Global strong consistency** → Spanner/CockroachDB (Paxos/Raft) — accept higher write latency.

### 5c. SQL vs NoSQL — the crisp version

- **SQL** = strong consistency, joins, transactions, flexible *queries*, rigid *schema*; scales up, shards hard.
- **NoSQL** = scale + flexible schema + high write throughput; you trade joins/transactions and must design around *one* access pattern.
- Default to **relational** unless a decision point (scale, write throughput, schema flexibility, access shape) pushes you off it. "Boring Postgres" is the right answer more often than candidates think.

### 5d. Storage/compute separation (senior default)

Put durable data in cheap object storage; run elastic compute on top; scale each independently; scale compute to zero when idle. Cloud-native baseline for data-heavy systems.

---

## Layer 6 — Caching

**Decision points:** read:write ratio · tolerance for staleness · hot keys · expensive-to-compute results · latency SLA.

| Option / pattern | Use when | Cost |
|---|---|---|
| **Cache-aside (lazy)** | General read-heavy; app controls it | First-miss latency; stale until TTL/evict |
| **Write-through** | Reads must be fresh right after write | Slower writes; caches data that may never be read |
| **Write-back** | Write-heavy, can tolerate loss window | Risk of data loss on cache failure |
| **CDN cache** | Static/public content | See Layer 1 |
| **Local/in-process cache** | Tiny hot set, per-node, lowest latency | Inconsistent across nodes; memory per node |
| **Distributed cache** (Redis) | Shared hot data across fleet | Network hop; another system to run |
| **Materialized view / precompute** | Expensive aggregations read often | Refresh lag; storage |

Where to cache: client → CDN → API gateway → app-local → distributed cache → DB buffer. **Invalidation** is the hard part: TTL (simple, stale window), explicit invalidation on write (fresh, more coupling), versioned keys. Guard against **cache stampede** (locking/request coalescing) and **hot keys** (replicate/shard the key).

---

## Layer 7 — Messaging & streaming

**Decision points:** decoupling · spiky load absorption · one-to-one vs pub/sub · ordering · replay · delivery guarantee · throughput.

| Option | Use when | Cost |
|---|---|---|
| **Task queue** (SQS, RabbitMQ) | Decouple producers/consumers, work distribution, retries | Ordering weak; not for replay/analytics |
| **Log / stream** (Kafka, Kinesis, Pulsar) | High-throughput, ordered-per-partition, replayable, multi-consumer, event sourcing | Operational weight; partitions cap parallelism |
| **Pub/Sub** (SNS, Google Pub/Sub) | Fan-out one event to many subscribers | At-least-once dupes; ordering |
| **In-memory bus** | Same-process/simple | No durability |

Delivery semantics: **at-most-once** (fire-forget, lossy) · **at-least-once** (default; needs **idempotent** consumers to dedupe) · **exactly-once** (effectively: idempotency keys + transactional/atomic commit + checkpointing). Ordering: global ordering is expensive → order **per partition/key**. Back-pressure + a **dead-letter queue** for poison messages.

---

## Layer 8 — Consistency, replication, partitioning

**Decision points:** consistency vs availability under partition (CAP) · read scaling · write scaling · durability · geo-distribution · hot partitions.

### Replication

| Option | Use when | Cost |
|---|---|---|
| **Leader-follower (async)** | Scale reads, durability; can tolerate small replication lag | Stale reads on followers; possible data loss on failover |
| **Leader-follower (sync)** | No data loss on failover | Higher write latency; availability drops if follower slow |
| **Multi-leader** | Multi-region writes, offline edits | Conflict resolution (LWW/CRDT/app logic) |
| **Quorum (Dynamo-style, W+R>N)** | Tunable consistency/availability, no single leader | Read-repair, sloppy quorum complexity |
| **Consensus (Raft/Paxos)** | Strong consistency with HA (metadata, config, Spanner) | Latency of agreement; min 3 nodes |

### Partitioning / sharding

| Strategy | Use when | Cost |
|---|---|---|
| **Hash** | Even spread, point lookups | No range queries; resharding pain |
| **Range** | Range scans, time queries | Hot spots on sequential keys |
| **Consistent hashing** | Elastic clusters, minimize movement on add/remove | Virtual-node bookkeeping |
| **Directory/lookup** | Full flexibility | Lookup service is a dependency + SPOF |

Consistency models to name: **strong / linearizable** (money) · **read-your-writes** · **monotonic reads** · **eventual** (feeds, counts). Match model to *data type*, not the whole system. CAP: under a partition you choose **C or A**; PACELC reminds you it's **latency vs consistency** even when healthy.

---

## Layer 9 — Search & analytics (OLTP vs OLAP)

**Decision points:** transactional row access vs large-scan aggregation · full-text · real-time vs batch analytics.

| Option | Use when | Cost |
|---|---|---|
| **OLTP store** (Postgres, etc.) | Per-row transactional reads/writes | Bad at big scans/aggregations |
| **OLAP / columnar** (Snowflake, BigQuery, Redshift, ClickHouse) | Aggregations/scans over huge datasets, BI | Not for point writes; batch-loaded |
| **Search engine** (Elasticsearch) | Full-text, relevance, facets | Sync from source; eventual |
| **Lakehouse** (Delta/Iceberg on object store) | Unified batch+stream analytics at PB scale (Layer 12) | Table-format + pipeline complexity |
| **CDC → analytics** | Keep OLAP/search in sync with OLTP | Pipeline + lag |

Rule: **don't run analytics on your OLTP DB.** Replicate/stream into a system built for scans.

---

## Layer 10 — Concurrency & coordination

**Decision points:** contended writes · exactly-once effects · leader election · distributed locks · unique-id generation.

| Option | Use when | Cost |
|---|---|---|
| **Optimistic concurrency** (version/CAS) | Low contention | Retries under contention |
| **Pessimistic locking** | High contention, must not conflict | Throughput hit; deadlock risk |
| **Idempotency keys** | Safe retries / at-least-once delivery | Dedup store to maintain |
| **Distributed lock/lease** (Redis, ZK, etcd) | Single-writer/leader election | Fencing tokens needed; lock service SPOF |
| **ID generation** (UUID, Snowflake, ULID) | Unique keys without coordination | UUID = no ordering; Snowflake = clock/coordination |

---

## Layer 11 — Cross-cutting: reliability, observability, security, cost

- **Reliability:** timeouts + retries with backoff+jitter · circuit breakers · bulkheads · graceful degradation · idempotency · checkpointing · multi-AZ then multi-region *only if SLA needs it*.
- **Observability:** metrics (RED/USE), structured logs, distributed tracing, health checks, alerting on SLOs. Senior: design for debuggability from the start.
- **Security:** authn (OAuth/OIDC/JWT) · authz (RBAC/ABAC) · encryption in transit (TLS) + at rest · secrets management · rate limiting (token bucket / sliding window) · input validation at boundaries · least privilege.
- **Cost:** right-size compute/storage tiers · autoscale + scale-to-zero · batch over stream when freshness allows · cache over recompute · don't run hot/GPU idle. Databricks grades cost awareness explicitly.

---

## Layer 12 — Data-platform / lakehouse layer (Databricks flavor)

Pull in when the prompt is ingestion/analytics/ML-flavored. (Mirrors [method.md](method.md) §7; details in [study/](study/).)

**Decision points:** batch vs streaming freshness · exactly-once · late/out-of-order data · schema evolution · petabyte scale · governance/lineage · train/serve skew · query latency.

| Part | Options / decisions |
|---|---|
| **Ingestion** | Batch ETL (cheap, simple, stale) vs streaming (Kafka/Kinesis → structured streaming; checkpointing for exactly-once; watermarking for late data). **CDC** to sync operational DB → lake. |
| **Storage/table format** | Raw object store vs **ACID table format** (Delta/Iceberg/Hudi): transaction log over Parquet → ACID, schema enforcement/evolution, time travel, fixes small-file/consistency problems. |
| **Architecture** | **Medallion**: Bronze (raw) → Silver (clean) → Gold (aggregated). **Lambda** (dual batch+speed) vs **Kappa** (one unified stream) — default unified. |
| **Query performance** | Partition by common filter keys; cluster/Z-order for data-skipping; cache hot data; materialized views. Watch **over-partitioning** → small files. |
| **Compute** | Separate storage/compute; elastic autoscaling clusters; scale-to-zero. |
| **Governance** | Catalog for schema/lineage/RBAC; encryption; multi-tenant isolation. (Forgetting this is a graded miss.) |
| **ML platform** | Feature store (offline point-in-time for training + online KV for low-latency serving → no train/serve skew); experiment tracking + model registry; batch scoring vs real-time serving endpoints. |

---

## Quick fault-line summary (the reflexes)

- High read:write → **replicas + cache + denormalize**.
- High write throughput → **partition/shard + LSM/wide-column + async**.
- Sub-10ms → **in-memory / colocate**.
- Strong consistency on money → **relational / consensus**, accept latency.
- Huge blobs → **object store + CDN**, pointer in DB.
- Full-text → **search index** beside the DB.
- Big aggregations → **OLAP/lakehouse**, never the OLTP DB.
- Spiky/slow work → **queue + async workers**.
- Real-time push → **WebSocket/SSE**.
- Internal low-latency RPC → **gRPC**.
- Global writes → **Spanner/Cockroach** or **multi-leader + conflict resolution**.
- Decouple + replay + fan-out → **Kafka**.

---

## Changelog

- **v0.1 — 2026-07-07:** Initial catalog. Layers 0–12 with decision points + options + use-when + trade-offs, plus fault-line reflex summary. Not yet stress-tested against reps.
